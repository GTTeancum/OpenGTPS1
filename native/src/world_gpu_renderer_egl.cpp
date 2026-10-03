#include "opengt/world_gpu_renderer_native.hpp"
#include "opengt/lighting.hpp"
#include "gl/api.hpp"
#include <algorithm>
#include <array>
#include <cmath>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <deque>
#include <limits>
#include <map>
#include <memory>
#include <stdexcept>
#include <string>
#include <tuple>
#include <vector>

namespace opengt::render {
namespace {
using namespace gl;
using lighting::Vec3;
using lighting::dot;
using lighting::normalized;
constexpr const char* source=
#include "opengt_world_glsl.inc"
;
struct ShaderFailure:std::runtime_error {using std::runtime_error::runtime_error;};
struct GpuVertex {float clip[4],position[3],normal[3],color[3],uv[2];std::uint32_t material;};
static_assert(sizeof(GpuVertex)==64);
struct alignas(16) GpuMaterial {
    std::array<std::uint32_t,4> source{};
    std::array<std::int32_t,4> window{};
    std::array<float,4> surface{},coat{},gain{1,1,1,0},extra{0,-1,0,0},plane{};
};
static_assert(sizeof(GpuMaterial)==112);
struct Batch {std::size_t first{},count{};int x{},y{},w{},h{},layer{},blend{};bool semi{},textured{},contact{},resolve_contact{};
    bool same(const Batch& b) const {return std::tie(x,y,w,h,layer,blend,semi,textured,contact,resolve_contact)==std::tie(b.x,b.y,b.w,b.h,b.layer,b.blend,b.semi,b.textured,b.contact,b.resolve_contact);}};
struct Probe {Vec3 center{};float radius{};std::uint32_t owner{};};
struct State {
    Context context; Api& g=context.api;
    GLuint program{},vao{},vbo{},ssbo{},vram{},color{},depth{},sun{},cube{},cube_depth{};
    GLuint main_fb{},shadow_fb{},cube_fb{};
    unsigned width{},height{},shadow_size{},cube_size{},cube_count{};
    std::vector<WorldTextureUpload> uploads;
    std::deque<std::vector<std::uint8_t>> completed;
    NativeLightingStats audit{};
    lighting::ShadowGridState shadow_grid;
    State(){
        std::string src(source);const auto nl=src.find('\n');
        auto compile=[&](GLenum stage,const char* macro){
            auto text=src.substr(0,nl+1)+"#define "+macro+"\n"+(g.zero_to_one?"#define ZERO_TO_ONE_DEPTH\n":"#define PRECISE_DEPTH_FALLBACK\n")+src.substr(nl+1);const char* c=text.c_str();
            GLuint shader=g.CreateShader(stage);g.ShaderSource(shader,1,&c,nullptr);g.CompileShader(shader);
            GLint ok{};g.GetShaderiv(shader,COMPILE_STATUS,&ok);
            if(!ok){std::array<char,8192> log{};g.GetShaderInfoLog(shader,static_cast<int>(log.size()),nullptr,log.data());g.DeleteShader(shader);throw ShaderFailure(log.data());}
            return shader;
        };
        // Shader objects are cleaned on every partial failure; no fallback
        // pretends a shader-compilation failure was a successful lit frame.
        GLuint vs=0,fs=0;
        try{vs=compile(VERTEX_SHADER,"VERTEX_STAGE");fs=compile(FRAGMENT_SHADER,"FRAGMENT_STAGE");
            program=g.CreateProgram();g.AttachShader(program,vs);g.AttachShader(program,fs);g.LinkProgram(program);
            GLint ok{};g.GetProgramiv(program,LINK_STATUS,&ok);if(!ok){std::array<char,8192> log{};g.GetProgramInfoLog(program,int(log.size()),nullptr,log.data());throw ShaderFailure(log.data());}
        }catch(...){if(vs)g.DeleteShader(vs);if(fs)g.DeleteShader(fs);if(program)g.DeleteProgram(program);throw;}
        g.DeleteShader(vs);g.DeleteShader(fs);
        g.GenVertexArrays(1,&vao);g.GenBuffers(1,&vbo);g.GenBuffers(1,&ssbo);
        g.BindVertexArray(vao);g.BindBuffer(ARRAY_BUFFER,vbo);
        const int components[]={4,3,3,3,2};const std::size_t offsets[]={0,16,28,40,52};
        for(unsigned i=0;i<5;i++){g.EnableVertexAttribArray(i);g.VertexAttribPointer(i,components[i],FLOAT,FALSE_,sizeof(GpuVertex),reinterpret_cast<const void*>(offsets[i]));}
        g.EnableVertexAttribArray(5);g.VertexAttribIPointer(5,1,UNSIGNED_INT,sizeof(GpuVertex),reinterpret_cast<const void*>(60));
        GLuint ids[6];g.GenTextures(6,ids);vram=ids[0];color=ids[1];depth=ids[2];sun=ids[3];cube=ids[4];cube_depth=ids[5];
        GLuint fb[3];g.GenFramebuffers(3,fb);main_fb=fb[0];shadow_fb=fb[1];cube_fb=fb[2];
        g.ActiveTexture(TEXTURE0);g.BindTexture(TEXTURE_2D,vram);params(TEXTURE_2D,NEAREST);
        g.PixelStorei(UNPACK_ALIGNMENT,1);g.PixelStorei(PACK_ALIGNMENT,1);
        g.Disable(DITHER);g.Disable(CULL_FACE);g.Disable(FRAMEBUFFER_SRGB);g.Enable(TEXTURE_CUBE_MAP_SEAMLESS);
        std::fprintf(stderr,"[OpenGT-L03] Linux native renderer: %s\n",context.renderer());
    }
    ~State(){
        try { context.make_current(); } catch(...) { return; }
        GLuint textures[]={vram,color,depth,sun,cube,cube_depth};g.DeleteTextures(6,textures);
        GLuint fb[]={main_fb,shadow_fb,cube_fb};g.DeleteFramebuffers(3,fb);
        g.DeleteBuffers(1,&vbo);g.DeleteBuffers(1,&ssbo);g.DeleteVertexArrays(1,&vao);g.DeleteProgram(program);
    }
    void params(GLenum target,int filter){g.TexParameteri(target,TEXTURE_MIN_FILTER,filter);g.TexParameteri(target,TEXTURE_MAG_FILTER,filter==int(LINEAR_MIPMAP_LINEAR)?LINEAR:filter);g.TexParameteri(target,TEXTURE_WRAP_S,CLAMP_TO_EDGE);g.TexParameteri(target,TEXTURE_WRAP_T,CLAMP_TO_EDGE);g.TexParameteri(target,TEXTURE_WRAP_R,CLAMP_TO_EDGE);}
    GLint loc(const char* n){return g.GetUniformLocation(program,n);}
    void i(const char* n,int x){g.Uniform1i(loc(n),x);} void f(const char* n,float x){g.Uniform1f(loc(n),x);}
    void constants(const lighting::Constants& c){
        g.Uniform4fv(loc("LightSun"),1,c.sun.data());g.Uniform4fv(loc("LightColor"),1,c.color.data());
        g.Uniform4fv(loc("LightSky"),1,c.sky.data());g.Uniform4fv(loc("LightGround"),1,c.ground.data());
        g.Uniform4fv(loc("LightUp"),1,c.up.data());g.Uniform4fv(loc("LightShadowRows"),8,c.shadow_rows[0].data());
        g.Uniform4fv(loc("LightShadowConfig"),1,c.shadow_config.data());
    }
    void checked_fb(){if(g.CheckFramebufferStatus(FRAMEBUFFER)!=FRAMEBUFFER_COMPLETE)throw std::runtime_error("incomplete OpenGT framebuffer");}
    void targets(unsigned w,unsigned h){
        if(w==width&&h==height)return;
        // A new-sized temporal stream cannot return old-sized pixels.
        completed.clear();width=w;height=h;
        g.ActiveTexture(TEXTURE0+3);g.BindTexture(TEXTURE_2D,color);params(TEXTURE_2D,NEAREST);
        g.TexImage2D(TEXTURE_2D,0,RGBA8,int(w),int(h),0,RGBA,UNSIGNED_BYTE,nullptr);
        g.BindTexture(TEXTURE_2D,depth);params(TEXTURE_2D,NEAREST);
        g.TexImage2D(TEXTURE_2D,0,DEPTH32F_STENCIL8,int(w),int(h),0,DEPTH_STENCIL,FLOAT_32_UNSIGNED_INT_24_8_REV,nullptr);
        g.BindFramebuffer(FRAMEBUFFER,main_fb);g.FramebufferTexture2D(FRAMEBUFFER,COLOR_ATTACHMENT0,TEXTURE_2D,color,0);
        g.FramebufferTexture2D(FRAMEBUFFER,DEPTH_STENCIL_ATTACHMENT,TEXTURE_2D,depth,0);g.DrawBuffer(COLOR_ATTACHMENT0);g.ReadBuffer(COLOR_ATTACHMENT0);checked_fb();
    }
    void shadow_target(unsigned n){
        g.ActiveTexture(TEXTURE0+1);g.BindTexture(TEXTURE_2D_ARRAY,sun);
        if(n!=shadow_size){shadow_size=n;params(TEXTURE_2D_ARRAY,LINEAR);
            g.TexImage3D(TEXTURE_2D_ARRAY,0,DEPTH_COMPONENT32F,int(n),int(n),2,0,DEPTH_COMPONENT,FLOAT,nullptr);
            g.TexParameteri(TEXTURE_2D_ARRAY,TEXTURE_COMPARE_MODE,0);g.TexParameteri(TEXTURE_2D_ARRAY,TEXTURE_COMPARE_FUNC,LEQUAL);
        }
    }
    void reflection_target(unsigned n,unsigned count){
        g.ActiveTexture(TEXTURE0+2);g.BindTexture(TEXTURE_CUBE_MAP_ARRAY,cube);
        if(n!=cube_size||count!=cube_count){cube_size=n;cube_count=count;params(TEXTURE_CUBE_MAP_ARRAY,LINEAR_MIPMAP_LINEAR);
            g.TexImage3D(TEXTURE_CUBE_MAP_ARRAY,0,RGBA16F,int(n),int(n),int(count*6),0,RGBA,FLOAT,nullptr);
            g.ActiveTexture(TEXTURE0+3);g.BindTexture(TEXTURE_2D,cube_depth);params(TEXTURE_2D,NEAREST);
            g.TexImage2D(TEXTURE_2D,0,DEPTH_COMPONENT32F,int(n),int(n),0,DEPTH_COMPONENT,FLOAT,nullptr);}
    }
};
struct ContextStates {std::unique_ptr<State> states[2];unsigned last_state{};};
thread_local std::map<const void*,ContextStates> live_contexts;
thread_local const void* selected_context=nullptr;
State* existing(bool software) noexcept {
    auto it=live_contexts.find(selected_context);
    return it==live_contexts.end()?nullptr:it->second.states[software?1:0].get();
}
State& state(bool software){
    auto& group=live_contexts[selected_context];auto& s=group.states[software?1:0];
    if(!s)s=std::make_unique<State>();
    s->context.make_current();group.last_state=software?1:0;return *s;
}
int layer(const WorldDrawCommand& c,const WorldMaterial& m){
    if(m.primitive_flags&world_primitive_screen_space_flag)return 4;
    if(c.object_kind==3)return 0;
    if(c.object_kind==1)return 1;
    if(c.object_kind==2)return 2;
    return 3;
}
GpuMaterial material(const WorldMaterial& m,const WorldDrawCommand& c,const lighting::DrawMaterial* l){
    GpuMaterial out;
    double e[2][3];for(int i=0;i<2;++i){const auto& a=c.vertices[0];const auto& b=c.vertices[i+1];e[i][0]=double(b.view_x)-a.view_x;e[i][1]=double(b.view_y)-a.view_y;e[i][2]=double(b.view_z)-a.view_z;}
    const double nx=e[0][1]*e[1][2]-e[0][2]*e[1][1],ny=e[0][2]*e[1][0]-e[0][0]*e[1][2],nz=e[0][0]*e[1][1]-e[0][1]*e[1][0];
    const double nl=std::sqrt(nx*nx+ny*ny+nz*nz);
    if(std::isfinite(nl)&&nl>1e-15)out.plane={float(nx/nl),float(ny/nl),float(nz/nl),0};
    out.source={m.primitive_flags,m.texture_page,m.clut,c.object_id};
    out.window={m.texture_mask_x,m.texture_mask_y,m.texture_offset_x,m.texture_offset_y};
    const bool contact=c.object_kind==2&&(m.primitive_flags&1U)==0&&(m.primitive_flags&2U)!=0&&((m.texture_page>>5)&3U)==2;
    // The stronger opt-in footprint belongs to the main physical view only.
    // Secondary views and screen-space packets retain the prior .06 treatment.
    const bool main_contact=contact&&c.channel==WorldViewChannel::main_view&&
        !(m.primitive_flags&world_primitive_screen_space_flag);
    out.extra={float(c.object_kind),-1,float(m.environment_flags),contact?(main_contact?1.0F:2.0F):0.0F};
    if(l){out.surface=l->surface;out.coat=l->coat;out.gain=l->gain;}return out;
}
Vec3 view_position(const WorldDrawCommand& c,const WorldDrawVertex& v){return {v.view_x*c.lighting_depth_scale,v.view_y*c.lighting_depth_scale,v.view_z*c.lighting_depth_scale};}
void append(std::vector<GpuVertex>& out,const WorldDrawCommand& c,std::uint32_t m,const lighting::Frame* lit,std::size_t index){
    auto a=view_position(c,c.vertices[0]),b=view_position(c,c.vertices[1]),d=view_position(c,c.vertices[2]);
    auto n=normalized(lighting::cross(b-a,d-a));
    for(unsigned j=0;j<3;j++){
        const auto& v=c.vertices[j];auto p=view_position(c,v);auto normal=n;
        if(lit&&lit->enabled&&index*3+j<lit->vertices.size()){p=lit->vertices[index*3+j].position;normal=lit->vertices[index*3+j].normal;}
        out.push_back({{v.clip_x,v.clip_y,v.clip_z,v.clip_w},{p.x,p.y,p.z},{normal.x,normal.y,normal.z},
            {float(v.r)/255,float(v.g)/255,float(v.b)/255},{v.u,v.v},m});
    }
}
std::vector<Probe> probes(const WorldDrawList& list,const lighting::Database* db,const lighting::Frame& frame){
    std::vector<Probe> result;if(!frame.enabled||!db||!db->settings.local_reflections)return result;
    struct Box{Vec3 lo{1e30F,1e30F,1e30F},hi{-1e30F,-1e30F,-1e30F};bool reflective{};};std::map<std::uint32_t,Box> boxes;
    for(std::size_t i=0;i<list.commands.size();i++){auto& c=list.commands[i];if(c.object_kind!=2||c.channel!=WorldViewChannel::main_view||c.lighting_depth_scale<=0)continue;
        if(list.materials[c.material_index].primitive_flags&(world_primitive_screen_space_flag|2U))continue;
        auto& b=boxes[c.object_id];if(frame.materials[i].surface[2]>0&&frame.materials[i].surface[3]>0)b.reflective=true;
        for(auto& v:c.vertices){auto p=view_position(c,v);b.lo={std::min(b.lo.x,p.x),std::min(b.lo.y,p.y),std::min(b.lo.z,p.z)};b.hi={std::max(b.hi.x,p.x),std::max(b.hi.y,p.y),std::max(b.hi.z,p.z)};}}
    for(auto [owner,b]:boxes){auto extent=b.hi-b.lo;float span=std::max({extent.x,extent.y,extent.z});auto center=(b.lo+b.hi)*.5F;
        if(b.reflective&&span>.0001F&&center.z>0)result.push_back({center,span*12,owner});}
    std::sort(result.begin(),result.end(),[](const Probe& a,const Probe& b){auto da=dot(a.center,a.center),dbb=dot(b.center,b.center);return da==dbb?a.owner<b.owner:da<dbb;});
    if(result.size()>db->settings.maximum_reflection_probes)result.resize(db->settings.maximum_reflection_probes);
    return result;
}
int choose_probe(const WorldDrawCommand& c,const std::vector<Probe>& p){
    if(p.empty())return -1;
    if(c.object_kind==2){
        for(std::size_t i=0;i<p.size();++i)if(c.object_id==p[i].owner)return int(i);
        return -1; // probe budget exhausted: hemisphere, never a self-containing neighbor probe
    }
    auto center=(view_position(c,c.vertices[0])+view_position(c,c.vertices[1])+view_position(c,c.vertices[2]))*(1.0F/3);
    int best=-1;float distance=std::numeric_limits<float>::max();
    for(std::size_t i=0;i<p.size();i++){if(c.object_kind==2&&c.object_id==p[i].owner)return int(i);
        auto d=center-p[i].center;float q=dot(d,d);if(q<distance&&q<p[i].radius*p[i].radius){distance=q;best=int(i);}}
    return best;
}
bool validate(const WorldDrawList& l){
    if(l.commands.size()>1000000||l.lighting_extra_casters.size()>1000000||l.materials.size()>1000000)return false;
    auto command=[](const WorldDrawCommand& c){for(auto& v:c.vertices){
        float values[]={v.clip_x,v.clip_y,v.clip_z,v.clip_w,v.view_x,v.view_y,v.view_z,v.u,v.v,c.lighting_depth_scale};
        for(float f:values)if(!std::isfinite(f)||std::abs(f)>1e12F)return false;
        if(std::abs(v.u)>1048576||std::abs(v.v)>1048576)return false;}return true;};
    for(auto& c:l.commands)if(c.material_index>=l.materials.size()||!command(c))return false;
    for(auto& c:l.lighting_extra_casters)if(!command(c.command))return false;
    return true;
}
} // namespace

WorldGpuRenderResult render_world_egl(const WorldDrawList& list,const std::uint16_t* vram,std::size_t words,
    std::uint8_t* output,std::size_t capacity,WorldGpuRenderOptions options,WorldGpuRenderStats* stats) noexcept {
    if(stats)*stats={};
    if(!stats||!output||!vram||words!=1024U*512U||list.display_width<=0||list.display_height<=0||
       options.output_scale==0||options.output_scale>8||!validate(list))return WorldGpuRenderResult::invalid_argument;
    const std::uint64_t w=std::uint64_t(world_gpu_target_display_width(list,options))*options.output_scale;
    const std::uint64_t h=std::uint64_t(list.display_height)*options.output_scale;
    if(w==0||h==0||w>8192||h>8192||w*h*4>capacity)return WorldGpuRenderResult::invalid_argument;
    if(options.direct_gpu_output || options.high_resolution_textures){
        std::fprintf(stderr,"[OpenGT-L03] unsupported Linux request: %s\n",
            options.direct_gpu_output?"D3D shared texture output":"external high-resolution texture atlas");
        return WorldGpuRenderResult::unsupported_platform;
    }
    try {
        auto& s=state(options.use_software_adapter);auto& g=s.g;
        if(options.asynchronous_readback&&s.completed.size()>=world_gpu_async_readback_image_capacity)
            return WorldGpuRenderResult::resource_failed;
        s.targets(unsigned(w),unsigned(h));g.UseProgram(s.program);g.BindVertexArray(s.vao);
        g.ActiveTexture(TEXTURE0);g.BindTexture(TEXTURE_2D,s.vram);
        // The authoritative uploaded pixels are always refreshed. A caller's
        // reuse hint must not make stale texels survive an upload invalidation.
        g.TexImage2D(TEXTURE_2D,0,R16UI,1024,512,0,RED_INTEGER,UNSIGNED_SHORT,vram);
        const auto* db=lighting::runtime_database();lighting::Frame lit;
        if(!db)s.shadow_grid={};
        if(db){std::vector<std::uint64_t> keys;for(auto u:s.uploads)keys.push_back(u.key);auto* track=lighting::select_track(*db,keys);lit=lighting::prepare(list,*db,track,{},&s.shadow_grid);
            if(const char* flag=std::getenv("OPENGT_LIGHTING_AUDIT");flag&&std::strcmp(flag,"1")==0&&(!lit.enabled||list.frame_index%120==0))
                std::fprintf(stderr,"[OpenGT-L04-lighting] frame=%llu uploads=%zu track=%s continuous=%u trackCommands=%u camera=%d,%d,%d/%d,%d,%d/%d,%d,%d\n",static_cast<unsigned long long>(list.frame_index),keys.size(),track?track->name.c_str():"unresolved",list.continuous_projection?1U:0U,list.track_commands,list.lighting_camera_rotation[0],list.lighting_camera_rotation[1],list.lighting_camera_rotation[2],list.lighting_camera_rotation[3],list.lighting_camera_rotation[4],list.lighting_camera_rotation[5],list.lighting_camera_rotation[6],list.lighting_camera_rotation[7],list.lighting_camera_rotation[8]);}
        if(const char* flag=std::getenv("OPENGT_SHADOW_AUDIT");flag&&std::strcmp(flag,"1")==0&&list.frame_index%120==0){
            const auto& c=lit.constants;
            std::fprintf(stderr,"[OpenGT-L08-shadow] frame=%llu near=%g far=%g bias=%g contact=%g strength=%g sun=%g,%g,%g up=%g,%g,%g anchor=%u grid=%u exponent=%d world=%d,%d,%d\n",static_cast<unsigned long long>(list.frame_index),c.shadow_config[0],c.shadow_config[1],c.shadow_config[3],lit.contact_shadow_opacity,c.color[3],c.sun[0],c.sun[1],c.sun[2],c.up[0],c.up[1],c.up[2],unsigned(list.lighting_world_anchor_valid),unsigned(lit.world_grid),int(list.lighting_camera_depth_exponent),list.lighting_camera_world_offset[0],list.lighting_camera_world_offset[1],list.lighting_camera_world_offset[2]);
        }
        const auto ps=probes(list,db,lit);s.constants(lit.constants);
        s.f("ContactShadowOpacity",lit.contact_shadow_opacity);
        // Explicit diagnostic view only; never enabled by the normal preset.
        const char* shadow_diagnostic=std::getenv("OPENGT_SHADOW_DIAGNOSTIC");
        s.i("ShadowDiagnostic",shadow_diagnostic&&std::strcmp(shadow_diagnostic,"1")==0?1:0);
        s.i("Dithering",options.dithering?1:0);s.i("OutputHeight",int(h));s.i("Perspective",options.perspective_correct?1:0);s.i("TextureSmoothing",options.texture_smoothing?1:0);
        s.f("HorizontalScale",float(list.display_width)/float(world_gpu_target_display_width(list,options)));
        s.i("ProbeCount",int(ps.size()));std::array<float,32> info{};
        for(std::size_t i=0;i<ps.size();i++){info[i*4]=ps[i].center.x;info[i*4+1]=ps[i].center.y;info[i*4+2]=ps[i].center.z;info[i*4+3]=ps[i].radius;}
        g.Uniform4fv(s.loc("ProbePositions"),8,info.data());
        std::vector<GpuVertex> vertices;std::vector<GpuMaterial> materials;std::vector<Batch> batches;
        vertices.reserve((list.commands.size()+list.lighting_extra_casters.size())*3+lit.casters.size());materials.reserve(list.commands.size()+list.lighting_extra_casters.size()+lit.casters.size()/3);
        const float scale=float(options.output_scale);const float pad=(float(world_gpu_target_display_width(list,options))-list.display_width)*.5F;
        // Original ordering is preserved within each explicit authored layer.
        // World depth owns track/car only; backdrop never occludes a car.
        for(int phase=0;phase<5;phase++)for(std::size_t i=0;i<list.commands.size();i++){
            auto& c=list.commands[i];auto& m=list.materials[c.material_index];if(layer(c,m)!=phase)continue;
            // Preserve the uploaded D3D Hor+ contract: only the main 3D
            // track, car and backdrop use the full widened horizontal view.
            // A mirror/subview and all screen-space UI retain their authored
            // drawing area. Vertical clips are never expanded.
            const bool full_main_world_scissor =
                !(m.primitive_flags & world_primitive_screen_space_flag) &&
                c.channel == WorldViewChannel::main_view &&
                (c.object_kind == 1U || c.object_kind == 2U || c.object_kind == 3U);
            int x0=full_main_world_scissor ? 0 : std::max(0,int(std::floor((c.clip_x0-list.display_x+pad)*scale)));
            int x1=full_main_world_scissor ? int(w) : std::min(int(w),int(std::ceil((c.clip_x1-list.display_x+1+pad)*scale)));
            int y0=std::max(0,int((c.clip_y0-list.display_y)*scale));int y1=std::min(int(h),int((c.clip_y1-list.display_y+1)*scale));
            if(x1<=x0||y1<=y0){++stats->skipped_empty_scissor_commands;continue;}
            auto gm=material(m,c,lit.enabled?&lit.materials[i]:nullptr);gm.extra[1]=float(choose_probe(c,ps));
            if(gm.surface[3])++s.audit.materials_lit;
            Batch b{vertices.size(),3,x0,int(h)-y1,x1-x0,y1-y0,phase,int((m.texture_page>>5)&3U),bool(m.primitive_flags&2U),bool(m.primitive_flags&1U),gm.extra[3]>0,gm.extra[3]==1&&lit.shadows};
            const auto index=std::uint32_t(materials.size());materials.push_back(gm);append(vertices,c,index,&lit,i);
            if(!batches.empty()&&batches.back().same(b)&&batches.back().first+batches.back().count==b.first)batches.back().count+=3;else batches.push_back(b);
            ++stats->commands;if(c.channel==WorldViewChannel::secondary_view)++stats->secondary_commands;
        }
        // Retained geometry is used for probes as well as the sun pass. It is
        // not appended to the visible color list or deleted from guest memory.
        if(lit.enabled)for(auto& extra:list.lighting_extra_casters){
            if(extra.material.primitive_flags&world_primitive_shadow_only_flag)continue;
            auto gm=material(extra.material,extra.command,nullptr);
            const auto idx=std::uint32_t(materials.size());materials.push_back(gm);append(vertices,extra.command,idx,nullptr,0);
        }
        const std::size_t scene_count=vertices.size(),shadow_first=scene_count;
        for(std::size_t i=0;i<lit.casters.size();i+=3){
            const auto& v=lit.casters[i];GpuMaterial gm;gm.source={v.flags,v.texture_page,v.clut,0};gm.window={v.mask_x,v.mask_y,v.offset_x,v.offset_y};
            auto idx=std::uint32_t(materials.size());materials.push_back(gm);
            for(unsigned j=0;j<3;j++){const auto& p=lit.casters[i+j];GpuVertex out{};std::copy(p.position,p.position+3,out.position);std::copy(p.uv,p.uv+2,out.uv);out.material=idx;vertices.push_back(out);}}
        // glBufferData with zero-sized arrays is legal; shader draws never
        // dereference an empty SSBO because the associated vertex count is zero.
        g.BindBuffer(ARRAY_BUFFER,s.vbo);g.BufferData(ARRAY_BUFFER,std::ptrdiff_t(vertices.size()*sizeof(GpuVertex)),vertices.data(),DYNAMIC_DRAW);
        g.BindBuffer(SHADER_STORAGE_BUFFER,s.ssbo);g.BufferData(SHADER_STORAGE_BUFFER,std::ptrdiff_t(materials.size()*sizeof(GpuMaterial)),materials.data(),DYNAMIC_DRAW);g.BindBufferBase(SHADER_STORAGE_BUFFER,0,s.ssbo);
        g.Disable(SCISSOR_TEST);g.Disable(BLEND);g.Enable(DEPTH_TEST);g.DepthMask(TRUE_);g.DepthFunc(LESS);g.ClearDepth(1);
        if(lit.shadows){
            s.shadow_target(db->settings.shadow_resolution);
            g.ActiveTexture(TEXTURE0+1);g.BindTexture(TEXTURE_2D_ARRAY,0); // no depth feedback while writing
            g.BindFramebuffer(FRAMEBUFFER,s.shadow_fb);g.DrawBuffer(NONE);g.ReadBuffer(NONE);s.i("Mode",1);
            g.Viewport(0,0,int(s.shadow_size),int(s.shadow_size));
            for(int c=0;c<2;c++){g.FramebufferTextureLayer(FRAMEBUFFER,DEPTH_ATTACHMENT,s.sun,0,c);s.checked_fb();g.Clear(DEPTH_BUFFER_BIT);s.i("Cascade",c);g.DrawArrays(TRIANGLES,int(shadow_first),int(lit.casters.size()));}
            s.audit.shadow_vertices+=lit.casters.size();
            g.ActiveTexture(TEXTURE0+1);g.BindTexture(TEXTURE_2D_ARRAY,s.sun);
        } else {g.ActiveTexture(TEXTURE0+1);g.BindTexture(TEXTURE_2D_ARRAY,s.sun);}
        if(!ps.empty()){
            s.reflection_target(db->settings.reflection_resolution,unsigned(ps.size()));
            g.BindFramebuffer(FRAMEBUFFER,s.cube_fb);g.FramebufferTexture2D(FRAMEBUFFER,DEPTH_ATTACHMENT,TEXTURE_2D,s.cube_depth,0);g.DrawBuffer(COLOR_ATTACHMENT0);g.ReadBuffer(COLOR_ATTACHMENT0);
            s.i("Mode",2);g.Viewport(0,0,int(s.cube_size),int(s.cube_size));g.ClearColor(0,0,0,0);g.DepthFunc(LESS);g.ClearDepth(1);
            // Do not bind a texture for sampling while that same object is an
            // FBO color attachment, even though the capture shader skips reflection.
            g.ActiveTexture(TEXTURE0+2);g.BindTexture(TEXTURE_CUBE_MAP_ARRAY,0);
            for(std::size_t i=0;i<ps.size();i++){
                g.Uniform3f(s.loc("ProbeCenter"),ps[i].center.x,ps[i].center.y,ps[i].center.z);g.Uniform1ui(s.loc("ProbeOwner"),ps[i].owner);
                s.f("ProbeNear",std::max(.01F,ps[i].radius/1200));s.f("ProbeFar",ps[i].radius*8);
                for(int face=0;face<6;face++){s.i("Face",face);g.FramebufferTextureLayer(FRAMEBUFFER,COLOR_ATTACHMENT0,s.cube,0,int(i*6)+face);s.checked_fb();g.Clear(COLOR_BUFFER_BIT|DEPTH_BUFFER_BIT);g.DrawArrays(TRIANGLES,0,int(scene_count));}
            }
            g.BindFramebuffer(FRAMEBUFFER,s.main_fb);g.ActiveTexture(TEXTURE0+2);g.BindTexture(TEXTURE_CUBE_MAP_ARRAY,s.cube);g.GenerateMipmap(TEXTURE_CUBE_MAP_ARRAY);s.f("ProbeMaxLod",std::log2(float(s.cube_size)));
            s.audit.reflection_probes+=ps.size();s.audit.reflection_faces+=ps.size()*6;s.audit.reflected_scene_vertices+=scene_count;
        }
        g.BindFramebuffer(FRAMEBUFFER,s.main_fb);g.DrawBuffer(COLOR_ATTACHMENT0);g.ReadBuffer(COLOR_ATTACHMENT0);
        g.Viewport(0,0,int(w),int(h));g.ClearDepth(0);g.DepthMask(TRUE_);g.ClearColor(float(options.clear_color_rgba8&255)/255,float((options.clear_color_rgba8>>8)&255)/255,float((options.clear_color_rgba8>>16)&255)/255,float((options.clear_color_rgba8>>24)&255)/255);
        g.StencilMask(0xFF);g.ClearStencil(0);g.Disable(STENCIL_TEST);
        g.Clear(COLOR_BUFFER_BIT|DEPTH_BUFFER_BIT|STENCIL_BUFFER_BIT);s.i("Mode",0);g.Enable(SCISSOR_TEST);g.DepthFunc(GEQUAL);
        // Source shadow meshes are overlapping polygon strips, not opacity
        // layers. Resolve their union once, AFTER opaque world/car depth and
        // BEFORE effects/HUD. A covered pixel cannot be darkened repeatedly,
        // and a later road/body packet cannot cut holes in that coverage.
        auto contacts=[&]{
            g.Enable(STENCIL_TEST);g.StencilMask(1);g.StencilFunc(EQUAL,0,1);g.StencilOp(KEEP,KEEP,INVERT);
            g.DepthMask(FALSE_);if(options.depth_buffer)g.Enable(DEPTH_TEST);else g.Disable(DEPTH_TEST);
            g.Enable(BLEND);g.BlendEquation(FUNC_ADD);g.BlendFuncSeparate(SRC_ALPHA,ONE_MINUS_SRC_ALPHA,ONE,ONE_MINUS_SRC_ALPHA);s.i("BlendPass",1);
            for(const auto& b:batches)if(b.resolve_contact){g.Scissor(b.x,b.y,b.w,b.h);g.DrawArrays(TRIANGLES,int(b.first),int(b.count));++stats->draw_calls;++stats->transparent_draw_calls;}
            g.Disable(STENCIL_TEST);g.StencilMask(0xFF);
        };
        for(int phase=0;phase<5;++phase){
        if(phase==3)contacts();
        for(auto b:batches){
            if(b.layer!=phase||b.resolve_contact)continue;
            g.Scissor(b.x,b.y,b.w,b.h);bool physical=b.layer==1||b.layer==2;
            if(options.depth_buffer&&physical)g.Enable(DEPTH_TEST);else g.Disable(DEPTH_TEST);
            g.DepthMask(physical?TRUE_:FALSE_);g.Disable(BLEND);s.i("BlendPass",0);
            if(!b.semi||b.textured){g.DrawArrays(TRIANGLES,int(b.first),int(b.count));++stats->draw_calls;}
            if(b.semi){s.i("BlendPass",1);g.DepthMask(FALSE_);g.Enable(BLEND);
                g.BlendEquation(b.contact?FUNC_ADD:b.blend==2?FUNC_REVERSE_SUBTRACT:FUNC_ADD);
                if(b.contact)g.BlendFuncSeparate(SRC_ALPHA,ONE_MINUS_SRC_ALPHA,ONE,ONE_MINUS_SRC_ALPHA);
                else if(b.blend==0){g.BlendColor(.5F,.5F,.5F,.5F);g.BlendFunc(CONSTANT_COLOR,CONSTANT_COLOR);}
                else if(b.blend==3){g.BlendColor(.25F,.25F,.25F,.25F);g.BlendFunc(CONSTANT_COLOR,ONE);}
                else g.BlendFunc(ONE,ONE);
                g.DrawArrays(TRIANGLES,int(b.first),int(b.count));++stats->draw_calls;++stats->transparent_draw_calls;
            }
        }
        }
        g.Disable(BLEND);g.Disable(STENCIL_TEST);g.Disable(SCISSOR_TEST);g.DepthMask(TRUE_);g.BlendEquation(FUNC_ADD);
        // Linux's initial live backend completes readback synchronously, then
        // queues immutable CPU images for the existing authored-frame protocol.
        // This preserves chronology but is NOT claimed to be asynchronous GPU I/O.
        std::vector<std::uint8_t> pixels(std::size_t(w*h*4));g.ReadPixels(0,0,int(w),int(h),RGBA,UNSIGNED_BYTE,pixels.data());
        const auto error=g.GetError();if(error){std::fprintf(stderr,"[OpenGT-L03] GL error 0x%x\n",error);return WorldGpuRenderResult::render_failed;}
        const std::size_t stride=std::size_t(w*4);std::vector<std::uint8_t> row(stride);
        for(std::size_t y=0;y<h/2;y++){auto* a=pixels.data()+y*stride;auto* b=pixels.data()+(std::size_t(h)-1-y)*stride;std::memcpy(row.data(),a,stride);std::memcpy(a,b,stride);std::memcpy(b,row.data(),stride);}
        if(options.asynchronous_readback){s.completed.push_back(std::move(pixels));stats->output_valid=false;}
        else{std::memcpy(output,pixels.data(),pixels.size());stats->output_valid=true;}
        const std::string renderer=s.context.renderer();stats->software_adapter=renderer.find("llvmpipe")!=std::string::npos||renderer.find("softpipe")!=std::string::npos;
        ++s.audit.frames;s.audit.readback_bytes+=w*h*4;
        if(const char* flag=std::getenv("OPENGT_LIGHTING_AUDIT");flag&&std::strcmp(flag,"1")==0)
            std::fprintf(stderr,"[OpenGT-L03-frame] frame=%llu commands=%u lights=%u sun=%u provisional=%u shadowVertices=%zu probes=%zu faces=%zu rgba=%llux%llu\n",static_cast<unsigned long long>(list.frame_index),stats->commands,lit.enabled?1U:0U,lit.solar?1U:0U,lit.provisional_sun?1U:0U,lit.casters.size(),ps.size(),ps.size()*6,static_cast<unsigned long long>(w),static_cast<unsigned long long>(h));
        return WorldGpuRenderResult::success;
    }catch(const ShaderFailure& e){std::fprintf(stderr,"[OpenGT-L03] GLSL failed: %s\n",e.what());return WorldGpuRenderResult::shader_failed;}
    catch(const std::bad_alloc&){return WorldGpuRenderResult::resource_failed;}
    catch(const std::exception& e){std::fprintf(stderr,"[OpenGT-L03] EGL render failed: %s\n",e.what());return WorldGpuRenderResult::device_failed;}
    catch(...){return WorldGpuRenderResult::render_failed;}
}
WorldGpuReadbackResult read_world_egl_image(bool software,std::uint8_t* out,std::size_t n,bool) noexcept{
    if(!out)return WorldGpuReadbackResult::invalid_argument;
    auto* s=existing(software);if(!s||s->completed.empty())return WorldGpuReadbackResult::not_ready;
    if(n<s->completed.front().size())return WorldGpuReadbackResult::invalid_argument;
    std::memcpy(out,s->completed.front().data(),s->completed.front().size());s->completed.pop_front();return WorldGpuReadbackResult::success;
}
WorldGpuReadbackResult read_world_egl_pair(bool b,std::uint8_t* x,std::uint8_t* y,std::size_t n,bool wait) noexcept{
    if(!x||!y)return WorldGpuReadbackResult::invalid_argument;
    auto* s=existing(b);if(!s||s->completed.size()<2)return WorldGpuReadbackResult::not_ready;
    if(n<s->completed[0].size()||n<s->completed[1].size())return WorldGpuReadbackResult::invalid_argument;
    read_world_egl_image(b,x,n,wait);return read_world_egl_image(b,y,n,wait);
}
void reset_world_egl(bool b) noexcept{auto* s=existing(b);if(s){s->completed.clear();s->shadow_grid={};}}
bool uploads_world_egl(bool b,const WorldTextureUpload* p,std::size_t n) noexcept{
    if(n>65536||(n&&!p))return false;
    try{std::vector<WorldTextureUpload> candidate;if(n)candidate.assign(p,p+n);
        for(auto u:candidate)if(!u.key||u.x<0||u.y<0||u.word_width<=0||u.height<=0||std::int64_t(u.x)+u.word_width>1024||std::int64_t(u.y)+u.height>512)return false;
        auto& s=state(b);s.uploads=std::move(candidate);return true;
    }catch(...){return false;}
}
void select_world_egl_context(const void* token) noexcept {selected_context=token;}
void release_world_egl_context(const void* token) noexcept {
    live_contexts.erase(token);
    if(selected_context==token)selected_context=nullptr;
}
NativeLightingStats world_native_lighting_stats() noexcept{
    auto it=live_contexts.find(selected_context);if(it==live_contexts.end())return {};
    auto& s=it->second.states[it->second.last_state];return s?s->audit:NativeLightingStats{};
}
const char* world_native_renderer_name() noexcept{
    auto it=live_contexts.find(selected_context);if(it==live_contexts.end())return "not initialized";
    auto& s=it->second.states[it->second.last_state];return s?s->context.renderer():"not initialized";
}
} // namespace opengt::render
