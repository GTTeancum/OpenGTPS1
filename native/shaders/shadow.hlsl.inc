R"OGTL(
// Uses the original VRAM page/CLUT/window path for cutout shadow silhouettes.
// Non-transparent source geometry only; road overlays never become casters.
struct ShadowInput {
    float3 position : POSITION;
    float2 uv : TEXCOORD0;
    uint3 material : TEXCOORD1;
    int4 window : TEXCOORD2;
};
struct ShadowOutput {
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD0;
    nointerpolation uint3 material : TEXCOORD1;
    nointerpolation int4 window : TEXCOORD2;
};
ShadowOutput ShadowVertex(ShadowInput input,uint cascade) {
    ShadowOutput output;
    output.position=SunPosition(input.position,cascade);
    output.uv=input.uv; output.material=input.material; output.window=input.window;
    return output;
}
ShadowOutput VSShadowNear(ShadowInput input) { return ShadowVertex(input,0); }
ShadowOutput VSShadowFar(ShadowInput input) { return ShadowVertex(input,1); }
void PSShadow(ShadowOutput input) {
    if((input.material.z&1)!=0) {
        MaterialData material=(MaterialData)0;
        material.texturePage=input.material.x; material.clut=input.material.y;
        material.textureMaskX=input.window.x;material.textureMaskY=input.window.y;
        material.textureOffsetX=input.window.z;material.textureOffsetY=input.window.w;
        uint word=TextureWord((int)floor(input.uv.x+0.0001),
            (int)floor(input.uv.y+0.0001),material);
        if(word==0) discard;
    }
}
)OGTL"
