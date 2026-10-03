// OpenGT L01. Linear-space material lighting; named controls are in
// lighting/lighting.shader. No screen-space painting or generated geometry.
Texture2DArray<float> SunDepth : register(t4);
SamplerComparisonState SunCompare : register(s1);
cbuffer LightingConstants : register(b2) {
    float4 LightSun;
    float4 LightColor;
    float4 LightSky;
    float4 LightGround;
    float4 LightUp;
    float4 LightShadowRows[8];
    float4 LightShadowConfig;
};
float4 SunPosition(float3 position, uint cascade) {
    float4 p = float4(position,1);
    return float4(dot(p,LightShadowRows[cascade*4]),
        dot(p,LightShadowRows[cascade*4+1]),
        dot(p,LightShadowRows[cascade*4+2]),1);
}
float SampleSunCascade(float3 position, float3 normal, uint cascade) {
    float4 q = SunPosition(position,cascade);
    float2 uv = q.xy * float2(0.5,-0.5) + 0.5;
    // Uncovered or clipped points must never acquire a phantom dark border.
    float3 dx=ddx(float3(uv,q.z)),dy=ddy(float3(uv,q.z));
    float det=dx.x*dy.y-dx.y*dy.x;
    float jacobianScale=max(dot(dx.xy,dx.xy),dot(dy.xy,dy.xy));
    float2 gradient=0;
    if(abs(det)>max(1e-20,jacobianScale*1e-6))
        gradient=clamp(float2(dx.z*dy.y-dy.z*dx.y,dx.x*dy.z-dy.x*dx.z)/det,-32.0,32.0);
    float edge = LightShadowConfig.z * 2.0;
    if (any(uv < edge) || any(uv > 1.0-edge) || q.z<=0.0 || q.z>=1.0)
        return 1.0;
    float slope = 1.0-saturate(dot(normal,LightSun.xyz));
    float depthScale=4.0*length(LightShadowRows[cascade*4+2].xyz)/max(length(LightShadowRows[cascade*4].xyz),1e-20);
    float bias = LightShadowConfig.w*depthScale*(1.0+slope*2.0);
    // Equivalent 3x3 bilinear PCF, comparing the receiver plane at each
    // actual texel center rather than adding an excessive slope bias.
    float2 texel=uv/LightShadowConfig.z-.5;
    int2 base=(int2)floor(texel);float2 fraction=frac(texel);
    float result=0;
    [unroll] for(int y=-1;y<=2;++y) {
        [unroll] for(int x=-1;x<=2;++x) {
            int2 pixel=base+int2(x,y);
            float2 offset=(float2(pixel)+.5)*LightShadowConfig.z-uv;
            float wx=x==-1?1-fraction.x:(x==2?fraction.x:1);
            float wy=y==-1?1-fraction.y:(y==2?fraction.y:1);
            float depth=SunDepth.Load(int4(pixel,cascade,0));
            result+=(q.z+dot(gradient,offset)-bias<=depth?1:0)*wx*wy;
        }
    }
    return result/9.0;
}
float SunCascadeCoverage(float3 position,uint cascade) {
    float4 q=SunPosition(position,cascade);
    float2 uv=q.xy*float2(.5,-.5)+.5;
    if(q.z<=0||q.z>=1)return 0;
    float edge=min(min(uv.x,uv.y),min(1-uv.x,1-uv.y));
    return smoothstep(2*LightShadowConfig.z,10*LightShadowConfig.z,edge);
}
float SunVisibility(float3 position, float3 normal) {
    if (LightGround.w==0.0) return 1.0;
    float distanceToEye=length(position);
    float farVisibility=lerp(1.0,SampleSunCascade(position,normal,1),SunCascadeCoverage(position,1));
    float fade=smoothstep(LightShadowConfig.y*0.8,LightShadowConfig.y,distanceToEye);
    farVisibility=lerp(farVisibility,1.0,fade);
    float blend=max(1.0-SunCascadeCoverage(position,0),smoothstep(LightShadowConfig.x*0.75,LightShadowConfig.x,distanceToEye));
    return lerp(SampleSunCascade(position,normal,0),farVisibility,blend);
}
float3 LightToLinear(float3 c) {
    c=saturate(c);
    return float3(c.x<=0.04045?c.x/12.92:pow((c.x+0.055)/1.055,2.4),
        c.y<=0.04045?c.y/12.92:pow((c.y+0.055)/1.055,2.4),
        c.z<=0.04045?c.z/12.92:pow((c.z+0.055)/1.055,2.4));
}
float3 LightToSrgb(float3 c) {
    c=max(c,0.0);
    return saturate(float3(c.x<=0.0031308?12.92*c.x:1.055*pow(c.x,1.0/2.4)-0.055,
        c.y<=0.0031308?12.92*c.y:1.055*pow(c.y,1.0/2.4)-0.055,
        c.z<=0.0031308?12.92*c.z:1.055*pow(c.z,1.0/2.4)-0.055));
}
float LightSpecular(float roughness,float strength,float nl,float nv,float nh,float vh) {
    float a=roughness*roughness, a2=a*a;
    float d=nh*nh*(a2-1.0)+1.0;
    float distribution=a2/(3.14159265359*d*d);
    float k=(roughness+1.0)*(roughness+1.0)/8.0;
    float geometry=nv/(nv*(1.0-k)+k)*nl/(nl*(1.0-k)+k);
    float fresnel=0.04+0.96*pow(1.0-vh,5.0);
    return strength*distribution*geometry*fresnel/(4.0*nv*max(0.001,nl))*nl;
}
float3 ApplyMaterialLighting(float3 color,float3 position,float3 normal,
    float4 surface,float4 coat,float4 gain) {
    if(LightSky.w==0.0 || surface.w==0.0) return color;
    float3 n=normal*rsqrt(max(dot(normal,normal),1e-20));
    float3 view=-position*rsqrt(max(dot(position,position),1e-20));
    if(dot(n,view)<0.0) n=-n;
    float3 h=LightSun.xyz+view;h*=rsqrt(max(dot(h,h),1e-20));
    float nl=saturate(dot(n,LightSun.xyz));
    float nv=max(0.001,saturate(dot(n,view)));
    float nh=saturate(dot(n,h)), vh=saturate(dot(view,h));
    float rawVisibility=SunVisibility(position,n);
    float solarVisibility=lerp(1.0,rawVisibility,coat.w);
    float legacyVisibility=1.0-LightColor.w*coat.w*(1.0-rawVisibility);
    float3 base=LightToLinear(color*gain.xyz);
    // Authored vertex/palette shading is not assumed to be unlit albedo.
    // Exact proven regions may be unbaked with gain; defaults preserve it.
    float3 illumination=LightUp.w+LightColor.rgb*(LightSun.w*nl*solarVisibility);
    float3 result=base*((1.0-coat.z)*legacyVisibility+coat.z*illumination);
    float specular=LightSpecular(surface.x,surface.y,nl,nv,nh,vh)+
        LightSpecular(coat.y,coat.x,nl,nv,nh,vh);
    result+=LightColor.rgb*(LightSun.w*specular*solarVisibility);
    // Rough hemispherical environment, NOT a reflection of nearby scenery.
    // Dry roads deliberately have a tiny reflection strength and high roughness.
    float3 reflected=reflect(-view,n);
    float skyWeight=saturate(dot(reflected,LightUp.xyz)*0.5+0.5);
    skyWeight=lerp(skyWeight,0.5,surface.x);
    float3 environment=lerp(LightGround.rgb,LightSky.rgb,skyWeight);
    float fresnel=0.04+0.96*pow(1.0-nv,5.0);
    result+=environment*(surface.z*fresnel*(1.0-0.75*surface.x));
    return LightToSrgb(result);
}
