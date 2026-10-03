#version 430 core
// OpenGT L03 native Linux backend. This is rasterized source geometry, not an
// image-space restyle. Compile the same source with VERTEX_STAGE or FRAGMENT_STAGE.
struct Material {
    uvec4 source; // flags, page, CLUT, owner
    ivec4 window;
    vec4 surface;
    vec4 coat;
    vec4 gain;
    vec4 extra; // object kind, selected probe (-1 is hemisphere), environment flags, spare
    vec4 plane; // unsmoothed geometric normal: shadow receiver plane, not paint normal
};
layout(std430,binding=0) readonly buffer Materials { Material materials[]; };
uniform vec4 LightSun,LightColor,LightSky,LightGround,LightUp,LightShadowConfig;
uniform vec4 LightShadowRows[8];
uniform int Mode,Cascade,Face,BlendPass,Perspective,TextureSmoothing,Dithering,OutputHeight;
uniform float HorizontalScale,ProbeNear,ProbeFar,ContactShadowOpacity;
uniform int ShadowDiagnostic;
uniform vec3 ProbeCenter;
uniform uint ProbeOwner;
uniform vec4 ProbePositions[8]; // XYZ center, W influence radius
uniform int ProbeCount;
uniform float ProbeMaxLod;
const uint ScreenFlag=0x80000000u;
vec4 sunPosition(vec3 p,int c) {
    vec4 q=vec4(p,1);
    return vec4(dot(q,LightShadowRows[c*4]),dot(q,LightShadowRows[c*4+1]),dot(q,LightShadowRows[c*4+2]),1);
}
#ifdef VERTEX_STAGE
layout(location=0) in vec4 inClip;
layout(location=1) in vec3 inPosition;
layout(location=2) in vec3 inNormal;
layout(location=3) in vec3 inColor;
layout(location=4) in vec2 inUv;
layout(location=5) in uint inMaterial;
out vec3 position,normal,color;
noperspective out vec3 affineColor;
out vec2 uv;
noperspective out vec2 affineUv;
flat out uint materialIndex;
#ifdef PRECISE_DEPTH_FALLBACK
out float preciseDepthClip;
#endif
void main() {
    position=inPosition;normal=inNormal;color=inColor;affineColor=inColor;
    uv=inUv;affineUv=inUv;materialIndex=inMaterial;
    Material m=materials[inMaterial];
    if(Mode==1) {
        vec4 p=sunPosition(inPosition,Cascade);gl_Position=p;
    } else if(Mode==2) {
        vec3 d=inPosition-ProbeCenter,q;
        if(Face==0)q=vec3(-d.z,-d.y,d.x);
        else if(Face==1)q=vec3(d.z,-d.y,-d.x);
        else if(Face==2)q=vec3(d.x,d.z,d.y);
        else if(Face==3)q=vec3(d.x,-d.z,-d.y);
        else if(Face==4)q=vec3(d.x,-d.y,d.z);
        else q=vec3(-d.x,-d.y,-d.z);
        gl_Position=vec4(q.xy,ProbeFar/(ProbeFar-ProbeNear)*q.z-
            ProbeFar*ProbeNear/(ProbeFar-ProbeNear),q.z);
        if((m.source.x&ScreenFlag)!=0u || (m.extra.x!=1 && m.extra.x!=2) ||
           (m.source.x&2u)!=0u || (m.extra.x==2 && m.source.w==ProbeOwner))
            gl_Position=vec4(0,0,2,1);
    } else {
        gl_Position=vec4(inClip.x*HorizontalScale,inClip.y,inClip.z,inClip.w);
    }
#ifdef PRECISE_DEPTH_FALLBACK
    // Preserve the small reversed-depth numerator through perspective
    // interpolation, rather than losing its bits in 2*near-W.
    preciseDepthClip=gl_Position.z;
    gl_Position.z=2*gl_Position.z-gl_Position.w;
#endif
}
#endif
#ifdef FRAGMENT_STAGE
layout(binding=0) uniform usampler2D Vram;
layout(binding=1) uniform sampler2DArray SunDepth;
layout(binding=2) uniform samplerCubeArray LocalEnvironment;
in vec3 position,normal,color;
noperspective in vec3 affineColor;
in vec2 uv;
noperspective in vec2 affineUv;
flat in uint materialIndex;
#ifdef PRECISE_DEPTH_FALLBACK
in float preciseDepthClip;
#endif
layout(location=0) out vec4 fragment;
uint vramAt(int x,int y){return texelFetch(Vram,ivec2(x&1023,y&511),0).r;}
uint textureWord(ivec2 raw,Material m){
    ivec2 p=(raw&~(m.window.xy*8))|((m.window.zw&m.window.xy)*8);p&=255;
    int page=int(m.source.y),clut=int(m.source.z);
    int px=(page&15)*64,py=((page>>4)&1)*256,mode=(page>>7)&3;
    int cx=(clut&63)*16,cy=(clut>>6)&511;
    if(mode==0){uint b=vramAt(px+(p.x>>2),py+p.y);return vramAt(cx+int((b>>((p.x&3)*4))&15u),cy);}
    if(mode==1){uint b=vramAt(px+(p.x>>1),py+p.y);return vramAt(cx+int((b>>((p.x&1)*8))&255u),cy);}
    return vramAt(px+p.x,py+p.y);
}
float expand5(uint v){return float((v<<3)|(v>>2))/255;}
vec3 textureColor(uint w){return vec3(expand5(w&31u),expand5((w>>5)&31u),expand5((w>>10)&31u));}
vec3 filteredColor(vec2 p,uint center,Material m){
    ivec2 b=ivec2(floor(p));vec2 f=fract(p);vec3 sum=vec3(0);float weight=0;
    for(int y=0;y<2;y++)for(int x=0;x<2;x++){
        uint w=textureWord(b+ivec2(x,y),m);
        float a=(x==0?1-f.x:f.x)*(y==0?1-f.y:f.y);
        // Filtering never promotes a transparent pixel or crosses STP classes.
        if(w!=0u && (w&32768u)==(center&32768u)){sum+=textureColor(w)*a;weight+=a;}
    }
    return weight>1e-6?sum/weight:textureColor(center);
}
float sampleCascade(vec3 p,vec3 n,vec4 geometric,int c){
    vec4 q=sunPosition(p,c);vec2 st=q.xy*.5+.5;
    // Compare each PCF tap against the depth of THIS receiver plane at that
    // tap, not the center pixel's depth. Otherwise an ordinary sloped road
    // incorrectly shadows itself over most of the filtering footprint.
    // Analytic plane derivatives are independent of the screen's 2x2 pixel
    // quad, triangle size and normal smoothing. Finite screen derivatives lose
    // precision at grazing incidence and a fixed slope clamp is not valid.
    vec3 ru=LightShadowRows[c*4].xyz,rv=LightShadowRows[c*4+1].xyz,rw=LightShadowRows[c*4+2].xyz;
    // Dual-basis plane transform: the original primary camera includes
    // nonuniform scale/shear. Dot-products alone assume orthogonal atlas axes
    // and create false self-occlusion when that assumption is violated.
    float lu=length(ru),lv=length(rv),lw=length(rw);
    vec3 u=ru/max(lu,1e-30),v=rv/max(lv,1e-30),w=rw/max(lw,1e-30);
    float denominator=dot(geometric.xyz,cross(u,v));
    vec2 gradient=vec2(0);
    if(abs(denominator)>1e-8)
        gradient=-2*vec2((lw/lu)*dot(geometric.xyz,cross(v,w)),
                         (lw/lv)*dot(geometric.xyz,cross(w,u)))/denominator;
    float edge=2*LightShadowConfig.z;
    if(any(lessThan(st,vec2(edge)))||any(greaterThan(st,vec2(1-edge)))||q.z<=0||q.z>=1)return 1;
    // Preserve the configured scene-space bias when depth fitting extends a
    // cascade. A fixed normalized bias would grow with unrelated tall casters.
    float depthScale=4*length(rw)/max(length(ru),1e-20);
    float bias=LightShadowConfig.w*depthScale*(1+2*(1-clamp(dot(n,LightSun.xyz),0,1)));
    // Evaluate the old 3x3 bilinear-PCF footprint as its equivalent 4x4
    // weighted texels. Each comparison uses this receiver's depth at that
    // actual texel center. A hardware bilinear comparison cannot do this: it
    // uses one depth for four different receiver positions, causing low-sun
    // acne or requiring a large, detached-shadow bias.
    vec2 texel=st/LightShadowConfig.z-.5;
    ivec2 base=ivec2(floor(texel));vec2 fraction=fract(texel);
    float total=0;
    for(int y=-1;y<=2;y++)for(int x=-1;x<=2;x++){
        ivec2 pixel=base+ivec2(x,y);
        vec2 offset=(vec2(pixel)+.5)*LightShadowConfig.z-st;
        float wx=x==-1?1-fraction.x:(x==2?fraction.x:1);
        float wy=y==-1?1-fraction.y:(y==2?fraction.y:1);
        float depth=texelFetch(SunDepth,ivec3(pixel,c),0).r;
        total+=(q.z+dot(gradient,offset)-bias<=depth?1:0)*wx*wy;
    }
    return total/9;
}
float cascadeCoverage(vec3 p,int c){
    vec4 q=sunPosition(p,c);vec2 st=q.xy*.5+.5;
    if(q.z<=0||q.z>=1)return 0;
    float edge=min(min(st.x,st.y),min(1-st.x,1-st.y));
    // A radial split alone cannot describe a square projected atlas. Blend
    // onto a covering neighbor BEFORE a PCF footprint exits the near map.
    return smoothstep(2*LightShadowConfig.z,10*LightShadowConfig.z,edge);
}
float sunVisibility(vec3 p,vec3 n,vec4 geometric){
    if(LightGround.w==0)return 1;
    float distanceToEye=length(p);
    float farValue=mix(1,sampleCascade(p,n,geometric,1),cascadeCoverage(p,1));
    farValue=mix(farValue,1,smoothstep(LightShadowConfig.y*.8,LightShadowConfig.y,distanceToEye));
    float blend=max(1-cascadeCoverage(p,0),smoothstep(LightShadowConfig.x*.75,LightShadowConfig.x,distanceToEye));
    return mix(sampleCascade(p,n,geometric,0),farValue,blend);
}
vec3 toLinear(vec3 c){c=clamp(c,0,1);return mix(pow((c+.055)/1.055,vec3(2.4)),c/12.92,lessThanEqual(c,vec3(.04045)));}
vec3 toSrgb(vec3 c){c=max(c,0);return clamp(mix(1.055*pow(c,vec3(1/2.4))-.055,12.92*c,lessThanEqual(c,vec3(.0031308))),0,1);}
float specular(float roughness,float strength,float nl,float nv,float nh,float vh){
    float a=roughness*roughness,a2=a*a,d=nh*nh*(a2-1)+1;
    float distribution=a2/(3.14159265359*d*d),k=(roughness+1)*(roughness+1)/8;
    float geometry=nv/(nv*(1-k)+k)*nl/(nl*(1-k)+k),fresnel=.04+.96*pow(1-vh,5);
    return strength*distribution*geometry*fresnel/(4*nv*max(.001,nl))*nl;
}
vec3 environment(vec3 direction,float roughness,Material m){
    float skyWeight=mix(clamp(dot(direction,LightUp.xyz)*.5+.5,0,1),.5,roughness);
    vec3 hemi=mix(LightGround.rgb,LightSky.rgb,skyWeight);
    int probe=int(m.extra.y);
    if(Mode==2 || probe<0 || probe>=ProbeCount)return hemi;
    vec4 info=ProbePositions[probe];
    float fade=1-smoothstep(info.w*.55,info.w,length(position-info.xyz));
    vec4 sampleColor=textureLod(LocalEnvironment,vec4(direction,float(probe)),roughness*ProbeMaxLod);
    // Cleared texels are transparent; their environment is the original profile
    // hemisphere, not black. Mip levels carry premultiplied coverage.
    return sampleColor.rgb*fade+hemi*(1-sampleColor.a*fade);
}
vec3 light(vec3 baseColor,Material m){
    if(LightSky.w==0 || m.surface.w==0)return Mode==2?toLinear(baseColor):baseColor;
    vec3 n=normal*inversesqrt(max(dot(normal,normal),1e-20));
    vec3 v=(Mode==2?ProbeCenter-position:-position);v*=inversesqrt(max(dot(v,v),1e-20));
    if(dot(n,v)<0)n=-n;
    vec3 h=LightSun.xyz+v;h*=inversesqrt(max(dot(h,h),1e-20));
    float nl=clamp(dot(n,LightSun.xyz),0,1),nv=max(.001,clamp(dot(n,v),0,1));
    float nh=clamp(dot(n,h),0,1),vh=clamp(dot(v,h),0,1);
    float raw=sunVisibility(position,n,m.plane),solar=mix(1,raw,m.coat.w),legacy=1-LightColor.w*m.coat.w*(1-raw);
    vec3 base=toLinear(baseColor*m.gain.xyz);
    vec3 illumination=LightUp.w+LightColor.rgb*(LightSun.w*nl*solar);
    vec3 result=base*((1-m.coat.z)*legacy+m.coat.z*illumination);
    float shine=specular(m.surface.x,m.surface.y,nl,nv,nh,vh)+specular(m.coat.y,m.coat.x,nl,nv,nh,vh);
    result+=LightColor.rgb*(LightSun.w*shine*solar);
    if(Mode!=2){
        float fresnel=.04+.96*pow(1-nv,5);
        result+=environment(reflect(-v,n),m.surface.x,m)*(m.surface.z*fresnel*(1-.75*m.surface.x));
    }
    return Mode==2?result:toSrgb(result);
}
void main(){
#ifdef PRECISE_DEPTH_FALLBACK
    gl_FragDepth=preciseDepthClip*gl_FragCoord.w;
#endif
    Material m=materials[materialIndex];uint flags=m.source.x;
    bool textured=(flags&1u)!=0u,semi=(flags&2u)!=0u,screen=(flags&ScreenFlag)!=0u;
    vec2 p=(Perspective!=0&&!screen)||Mode!=0?uv:affineUv;
    // Primary sample selects alpha/STP; interpolation only affects color.
    uint word=textured?textureWord(ivec2(floor(p)),m):32768u;
    if(textured&&word==0u)discard;
    if(Mode==1){fragment=vec4(0);return;}
    if(Mode==0){
        bool blend=semi&&(!textured||(word&32768u)!=0u);
        if((BlendPass==0&&blend)||(BlendPass==1&&!blend))discard;
    }
    if(m.gain.w>.5&&LightGround.w!=0)discard;
    vec3 base=((Perspective!=0&&!screen)||Mode!=0)?color:affineColor;
    if(textured){vec3 texel=(!screen&&TextureSmoothing!=0)?filteredColor(p,word,m):textureColor(word);
        base=(flags&4u)!=0u?texel:clamp(texel*base*2,0,1);}
    // Diagnostic grayscale is the real shadow-map occlusion, not a painted
    // footprint. It is used only in separately labelled development runs.
    if(Mode==0 && ShadowDiagnostic!=0){
        if(m.extra.w>.5)discard;
        vec3 n=normal*inversesqrt(max(dot(normal,normal),1e-20));
        if(dot(n,-position)<0)n=-n;
        float amount=m.surface.w>0?m.coat.w*(1-sunVisibility(position,n,m.plane)):0;
        fragment=vec4(vec3(amount),1);return;
    }
    vec3 result=light(base,m);
    if(Mode==0 && Dithering!=0 && (uint(m.extra.z)&4u)!=0u && (!textured||(flags&4u)==0u)){
        const int pattern[16]=int[16](-4,0,-3,1,2,-2,3,-1,-3,1,-4,0,3,-1,2,-2);
        ivec2 pixel=ivec2(gl_FragCoord.x,float(OutputHeight)-gl_FragCoord.y);
        ivec3 value=clamp(ivec3(result*255+.5)+pattern[(pixel.y&3)*4+(pixel.x&3)],0,255);
        ivec3 five=min(value>>3,31);
        result=vec3((five<<3)|(five>>2))/255;
    }
    float contactOpacity=m.extra.w<1.5?ContactShadowOpacity:.06;
    fragment=m.extra.w>.5?vec4(0,0,0,contactOpacity):vec4(result,1);
}
#endif
