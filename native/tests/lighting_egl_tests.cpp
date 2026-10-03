// L03: executes the same platform-neutral backend used by the live bridge.
#include "opengt/world_gpu_renderer_native.hpp"
#include "opengt/lighting.hpp"
#include <array>
#include <chrono>
#include <cmath>
#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <thread>
#include <vector>
#include <limits>
#if defined(_WIN32)
#include <process.h>
#else
#include <unistd.h>
#endif
using namespace opengt::render;
namespace {
constexpr int size=96;
void require(bool value,const char* message) {if(!value)throw std::runtime_error(message);}
WorldDrawVertex point(float x,float y,float z) {
    WorldDrawVertex v{};v.view_x=x;v.view_y=y;v.view_z=z;
    v.clip_x=x;v.clip_y=-y;v.clip_z=16;v.clip_w=z;
    v.screen_x=size*.5F+(x/z)*size*.5F;v.screen_y=size*.5F+(y/z)*size*.5F;
    v.model_x=static_cast<std::int16_t>(x);v.model_y=static_cast<std::int16_t>(y);v.model_z=static_cast<std::int16_t>(z);
    v.r=v.g=v.b=140;v.projection_plane=size*.5F;
    v.projection_offset_x=v.projection_offset_y=size*.5F;
    return v;
}
WorldDrawCommand triangle(WorldDrawVertex a,WorldDrawVertex b,WorldDrawVertex c) {
    WorldDrawCommand command{};command.vertices[0]=a;command.vertices[1]=b;command.vertices[2]=c;
    command.clip_x1=command.clip_y1=size-1;
    command.object_kind=1;command.object_id=1;command.model_pointer=0x80001000;
    command.channel=WorldViewChannel::main_view;command.lighting_depth_scale=1;
    return command;
}
WorldDrawList scene(bool cutout) {
    WorldDrawList l{};l.display_width=l.display_height=size;l.continuous_projection=true;
    l.lighting_camera_rotation={4096,0,0,0,4096,0,0,0,4096};
    l.materials.push_back(WorldMaterial{});
    auto a=point(-450,100,120),b=point(450,100,120),c=point(450,100,1100),d=point(-450,100,1100);
    l.commands.push_back(triangle(a,c,b));l.commands.push_back(triangle(a,d,c));l.track_commands=2;
    // A caster in the independent sun stream, not the color stream. This
    // proves shadows are not limited to camera-visible submitted faces.
    auto e=point(-70,-30,360),f=point(70,-30,360),g=point(70,-30,510),h=point(-70,-30,510);
    WorldMaterial caster{};if(cutout){caster.primitive_flags=1U;caster.texture_page=2U<<7U;}
    l.lighting_extra_casters.push_back({triangle(e,f,g),caster});
    l.lighting_extra_casters.push_back({triangle(e,g,h),caster});
    return l;
}
std::vector<std::uint8_t> image(const WorldDrawList& l) {
    std::vector<std::uint16_t> vram(1024U*512U); // all-zero source texel is a real cutout
    std::vector<std::uint8_t> pixels(size*size*4);
    WorldGpuRenderStats stats{};reset_world_native_readback(true);
    auto result=render_world_native(l,vram.data(),vram.size(),pixels.data(),pixels.size(),
        WorldGpuRenderOptions{true,true,false,true,false,false,1,0xFF050505U},&stats);
    if(result!=WorldGpuRenderResult::success)throw std::runtime_error(world_gpu_render_result_name(result));
    require(stats.output_valid,"EGL readback did not return an image");return pixels;
}
std::size_t different(const std::vector<std::uint8_t>& a,const std::vector<std::uint8_t>& b,bool only_darker=false) {
    std::size_t n=0;for(std::size_t i=0;i<a.size();i+=4) {
        int change=int(b[i])+b[i+1]+b[i+2]-int(a[i])-a[i+1]-a[i+2];
        if(only_darker?change<-6:std::abs(change)>6)++n;
    }return n;
}
void config(const std::filesystem::path& path,bool enabled,bool solar,bool shadows) {
    std::ofstream o(path,std::ios::binary|std::ios::trunc);require(bool(o),"cannot write isolated test shader");
    o<<"settings { enabled "<<enabled<<" shadows "<<shadows<<" shadowResolution 512 forceTrack test }\n"
        "material world { surface track roughness .8 specular .12 reflection .03 diffuseMix .35 }\n"
        "track test { sunDirection .4 -.8 .3 sunColor 1 .93 .83 skyColor .4 .5 .6 "
        "groundColor .15 .15 .15 ambient .72 shadowStrength .6 sunConfidence 1 sunIntensity "<<(solar?.8:0)<<" }\n";
    o.close();require(bool(o),"test shader flush failed");
    // The production hot-reloader deliberately polls only once per second.
    std::this_thread::sleep_for(std::chrono::milliseconds(1100));
}void write_shader(const std::filesystem::path& p,const std::string& s){
    std::ofstream o(p,std::ios::binary|std::ios::trunc);o<<s;o.close();require(bool(o),"shader write failed");
    std::this_thread::sleep_for(std::chrono::milliseconds(1050));
}
std::string mirror_shader(bool enabled=true,bool night=false,unsigned maximum=4){
    return std::string("settings { enabled ")+(enabled?"1":"0")+" shadows 0 forceTrack mirror localReflections 1 reflectionResolution 64 maximumReflectionProbes "+std::to_string(maximum)+" }\n"
      "material body { surface car roughness .06 specular 0 reflection 1 clearcoat 0 diffuseMix 0 }\n"
      "material ground { surface track roughness .9 specular 0 reflection 0 diffuseMix 0 }\n"
      "track mirror { skyColor .2 .2 .2 groundColor .2 .2 .2 ambient 1 night "+(night?"1":"0")+" }\n";
}
void quad(WorldDrawList& l,float x,float y,float z,float half,unsigned kind,unsigned owner,std::array<unsigned char,3> rgb,bool extra=false){
    auto a=point(x-half,y-half,z),b=point(x+half,y-half,z),c=point(x+half,y+half,z),d=point(x-half,y+half,z);
    for(auto* v:{&a,&b,&c,&d}){v->r=rgb[0];v->g=rgb[1];v->b=rgb[2];}
    auto first=triangle(a,c,b),second=triangle(a,d,c);
    for(auto* t:{&first,&second}){t->object_kind=kind;t->object_id=owner;t->model_pointer=owner*100;t->material_index=0;}
    if(extra){l.lighting_extra_casters.push_back({first,l.materials[0]});l.lighting_extra_casters.push_back({second,l.materials[0]});}
    else{l.commands.push_back(first);l.commands.push_back(second);if(kind==1)l.track_commands+=2;else if(kind==2)l.vehicle_commands+=2;}
}
WorldDrawList mirror_scene(bool blue=false,int owner_occluder=0){
    WorldDrawList l{};l.display_width=l.display_height=size;l.continuous_projection=true;
    l.lighting_camera_rotation={4096,0,0,0,4096,0,0,0,4096};l.materials.push_back(WorldMaterial{});
    // Black mirror facing the eye. Source wall is behind the camera, so it
    // cannot contribute screen pixels without the real cube capture.
    quad(l,0,0,500,190,2,7,{12,12,12});
    quad(l,0,0,-100,3000,1,8,blue?std::array<unsigned char,3>{0,0,255}:std::array<unsigned char,3>{255,0,0});
    if(owner_occluder)quad(l,0,0,480,700,2,unsigned(owner_occluder),{0,255,0},true);
    return l;
}
std::size_t channel_dominant(const std::vector<std::uint8_t>& p,unsigned channel){
    std::size_t count=0;for(int y=32;y<64;y++)for(int x=32;x<64;x++){
        auto i=std::size_t(y*size+x)*4;if(p[i+channel]>p[i+(channel+1)%3]+8&&p[i+channel]>p[i+(channel+2)%3]+8)++count;}
    return count;
}
void additional_graphics_tests(const std::filesystem::path& path){
    write_shader(path,mirror_shader());auto red=image(mirror_scene()),blue=image(mirror_scene(true));
    require(channel_dominant(red,0)>300,"off-camera red wall is absent from local reflection");
    require(channel_dominant(blue,2)>300,"off-camera blue wall is absent from local reflection");
    auto owner=image(mirror_scene(false,7));require(owner==red,"owner car was captured in its own reflection");
    auto other=image(mirror_scene(false,9));require(channel_dominant(other,1)>300,"other car was wrongly excluded from reflection");
    auto before=world_native_lighting_stats();auto l=mirror_scene();
    quad(l,400,0,500,90,2,10,{12,12,12});image(l);auto after=world_native_lighting_stats();
    require(after.reflection_probes-before.reflection_probes==2,"two cars did not receive independent probes");
    require(after.reflection_faces-before.reflection_faces==12,"two cars did not render 12 cube faces");
    write_shader(path,mirror_shader(true,false,1));before=world_native_lighting_stats();image(l);after=world_native_lighting_stats();
    require(after.reflection_probes-before.reflection_probes==1,"configured probe limit not enforced");
    write_shader(path,mirror_shader(false));auto disabled=image(mirror_scene());
    write_shader(path,mirror_shader(true,true));require(image(mirror_scene())==disabled,"night profile did not preserve exact unlit pixels");
    write_shader(path,mirror_shader());auto live=image(mirror_scene());
    write_shader(path,"settings { reflectionResolution 65 }\n");require(image(mirror_scene())==live,"invalid reload did not preserve last good database");
    std::filesystem::remove(path);std::this_thread::sleep_for(std::chrono::milliseconds(1050));require(image(mirror_scene())==disabled,"removing shader did not restore exact disabled pixels");
    write_shader(path,mirror_shader());auto main_thread=image(mirror_scene());std::vector<std::uint8_t> worker;
    std::thread thread([&]{worker=image(mirror_scene());});thread.join();
    require(worker==main_thread,"independent EGL worker produced different pixels");require(image(mirror_scene())==main_thread,"worker destruction invalidated surviving EGL display");
    // Invalid input must neither submit an image nor poison the next frame.
    auto invalid=mirror_scene();invalid.commands[0].vertices[0].u=std::numeric_limits<float>::quiet_NaN();
    std::vector<std::uint16_t> vram(1024U*512U);std::vector<std::uint8_t> p(size*size*4);WorldGpuRenderStats stats{};
    require(render_world_native(invalid,vram.data(),vram.size(),p.data(),p.size(),WorldGpuRenderOptions{true,true,false,true,false,false,1,0},&stats)==WorldGpuRenderResult::invalid_argument,"nonfinite source UV accepted");
    require(image(mirror_scene())==main_thread,"invalid input poisoned subsequent image");
    std::cout<<"Local reflections: red/blue off-camera walls, own/other car distinction, independent probes, bounded probe count, night/off/reload/recovery, worker lifecycle passed\n";
}
void texture_and_queue_tests(const std::filesystem::path& path){
    write_shader(path,"settings { enabled 0 }\n");
    WorldDrawList l{};l.display_width=l.display_height=size;l.continuous_projection=true;l.lighting_camera_rotation={4096,0,0,0,4096,0,0,0,4096};l.materials.push_back(WorldMaterial{});
    quad(l,0,0,500,200,1,1,{128,128,128});l.materials[0].primitive_flags=5;
    std::vector<std::uint16_t> vram(1024U*512U);std::vector<std::uint8_t> baseline;
    WorldGpuRenderOptions opt{true,true,false,true,false,false,1,0xFF202020};
    for(unsigned mode=0;mode<3;mode++){
        vram.assign(vram.size(),0);l.materials[0].texture_page=std::uint16_t(mode<<7);l.materials[0].clut=32;
        vram[0]=mode==0?0x1111:mode==1?0x0101:0x001f;vram[513]=0x001f;
        std::vector<std::uint8_t> out(size*size*4);WorldGpuRenderStats s{};
        require(render_world_native(l,vram.data(),vram.size(),out.data(),out.size(),opt,&s)==WorldGpuRenderResult::success&&s.output_valid,"indexed texture render failed");
        require(channel_dominant(out,0)>300,"original indexed texture palette was decoded incorrectly");
        if(mode==0)baseline=out;else require(out==baseline,"4/8/16-bit texture decoding disagrees");
    }
    // Immutable queued images and frame consumers cannot reuse live texels.
    reset_world_native_readback(true);opt.asynchronous_readback=true;
    std::vector<std::uint8_t> out(size*size*4),first(out.size()),second(out.size());WorldGpuRenderStats s{};
    vram[0]=0x001f;require(render_world_native(l,vram.data(),vram.size(),out.data(),out.size(),opt,&s)==WorldGpuRenderResult::success&&!s.output_valid,"queued submission should have no immediate output");
    vram[0]=0x7c00;opt.reuse_uploaded_vram=true;require(render_world_native(l,vram.data(),vram.size(),out.data(),out.size(),opt,&s)==WorldGpuRenderResult::success,"second queued submission failed");
    require(try_read_world_native_image(true,first.data(),first.size())==WorldGpuReadbackResult::success,"first queue read failed");
    require(try_read_world_native_image(true,second.data(),second.size())==WorldGpuReadbackResult::success,"second queue read failed");
    require(channel_dominant(first,0)>300&&channel_dominant(second,2)>300,"queued image or upload reuse crossed frames");
    require(try_read_world_native_image(true,second.data(),second.size())==WorldGpuReadbackResult::not_ready,"empty queue returned stale pixels");
    for(unsigned i=0;i<world_gpu_async_readback_image_capacity;i++)require(render_world_native(l,vram.data(),vram.size(),out.data(),out.size(),opt,&s)==WorldGpuRenderResult::success,"bounded image queue filled prematurely");
    require(render_world_native(l,vram.data(),vram.size(),out.data(),out.size(),opt,&s)==WorldGpuRenderResult::resource_failed,"full queue silently overwrote an authored image");
    reset_world_native_readback(true);require(try_read_world_native_image(true,out.data(),out.size())==WorldGpuReadbackResult::not_ready,"reset retained stale queued image");
    opt.asynchronous_readback=false;opt.output_scale=0;require(render_world_native(l,vram.data(),vram.size(),out.data(),out.size(),opt,&s)==WorldGpuRenderResult::invalid_argument,"zero resolution accepted");
    opt.output_scale=1;opt.direct_gpu_output=true;require(render_world_native(l,vram.data(),vram.size(),out.data(),out.size(),opt,&s)==WorldGpuRenderResult::unsupported_platform,"Linux accepted a D3D direct texture request");
    opt.direct_gpu_output=false;opt.use_software_adapter=false;require(render_world_native(l,vram.data(),vram.size(),out.data(),out.size(),opt,&s)==WorldGpuRenderResult::success,"second context selector failed");
    // Switch back before thread-local destruction: catches wrong-current-context cleanup.
    opt.use_software_adapter=true;require(render_world_native(l,vram.data(),vram.size(),out.data(),out.size(),opt,&s)==WorldGpuRenderResult::success,"switching contexts corrupted renderer");
    std::cout<<"Texture and queue: original 4/8/16-bit exact match, immutable FIFO, upload invalidation, capacity/backpressure/reset, invalid resolution, D3D rejection, two contexts passed\n";
}


void widescreen_scissor_tests(const std::filesystem::path& path){
    write_shader(path,"settings { enabled 0 }\n");
    WorldGpuRenderOptions opt{true,true,false,true,false,false,1,0xFF050505U};
    opt.target_aspect_width=2;opt.target_aspect_height=1;
    std::vector<std::uint16_t> vram(1024U*512U);
    // Twice the authored display width. Geometry already exists outside the
    // source view; the source draw-area scissor must not crop Hor+ world edges.
    unsigned cases=0;
    for(unsigned kind:{1U,2U,3U})for(unsigned scope=0;scope<3;scope++){
        WorldDrawList l{};l.display_width=l.display_height=size;l.continuous_projection=true;
        l.materials.push_back(WorldMaterial{});quad(l,0,0,500,1500,kind,1,{200,0,0});
        for(auto& c:l.commands){c.clip_y0=24;c.clip_y1=71;
            if(scope==1)c.channel=WorldViewChannel::secondary_view;}
        if(scope==2)l.materials[0].primitive_flags|=world_primitive_screen_space_flag;
        const auto w=world_gpu_target_display_width(l,opt);
        require(w==192,"wide fixture did not select its requested aspect");
        std::vector<std::uint8_t> p(w*size*4);WorldGpuRenderStats stats{};
        require(render_world_native(l,vram.data(),vram.size(),p.data(),p.size(),opt,&stats)==WorldGpuRenderResult::success&&stats.output_valid,"wide fixture did not render");
        auto red=[&](unsigned x,unsigned y){auto i=(y*w+x)*4;return p[i]>150&&p[i+1]==0&&p[i+2]==0;};
        require(red(96,48),"main/subview/screen fixture lost its center");
        require(red(24,48)==(scope==0)&&red(168,48)==(scope==0),"main world/backdrop or subview/UI horizontal scissor regressed");
        require(!red(96,12)&&!red(96,84),"Hor+ improperly widened vertical source scissor");
        ++cases;
    }
    std::cout<<"Hor+ scissor: "<<cases<<" actual wide-image cases passed (track/car/backdrop, main/subview/UI, retained vertical clips)\n";
}

void material_and_legacy_contract_tests(const std::filesystem::path& path){
    std::vector<std::uint16_t> vram(1024U*512U);
    auto render=[&](const WorldDrawList& l,bool dither=false){
        WorldGpuRenderOptions opt{true,true,dither,true,false,false,1,0xFF050505U};
        WorldGpuRenderStats stats{};std::vector<std::uint8_t> p(size*size*4);
        require(render_world_native(l,vram.data(),vram.size(),p.data(),p.size(),opt,&stats)==WorldGpuRenderResult::success&&stats.output_valid,"material contract render failed");
        return p;
    };
    WorldDrawList l{};l.display_width=l.display_height=size;l.continuous_projection=true;
    l.lighting_camera_rotation={4096,0,0,0,4096,0,0,0,4096};l.materials.push_back(WorldMaterial{});
    quad(l,0,0,500,300,2,1,{103,103,103});
    // Original source environment dither is optional, not driver-default noise.
    write_shader(path,"settings { enabled 0 }\n");l.materials[0].environment_flags=4;
    auto plain=render(l),dithered=render(l,true);
    constexpr int pattern[]={-4,0,-3,1,2,-2,3,-1,-3,1,-4,0,3,-1,2,-2};
    for(int y=40;y<56;y++)for(int x=40;x<56;x++){
        auto pixel=std::size_t(y*size+x)*4;int five=(103+pattern[(y&3)*4+(x&3)])>>3;
        int expected=(five<<3)|(five>>2);
        for(int ch=0;ch<3;ch++){
            require(plain[pixel+ch]==103,"dither-disabled source color changed");
            require(dithered[pixel+ch]==expected,"PS1 4x4 dither or top-left parity mismatch");
        }
    }
    l.materials[0].environment_flags=0;require(render(l,true)==plain,"unflagged material was dithered");
    // Course billboard planes are rebuilt from camera yaw and therefore have
    // no source-stable physical normal. The native lighting stage must leave
    // their authored color exactly unchanged while ordinary track geometry
    // still receives the active material response.
    WorldDrawList billboard{};billboard.display_width=billboard.display_height=size;billboard.continuous_projection=true;
    billboard.lighting_camera_rotation={4096,0,0,0,4096,0,0,0,4096};billboard.materials.push_back(WorldMaterial{});
    quad(billboard,0,0,500,300,1,3,{103,103,103});
    write_shader(path,"settings { enabled 0 }\n");const auto billboard_stock=render(billboard);
    write_shader(path,"settings { enabled 1 shadows 0 localReflections 0 forceTrack bill }\nmaterial world { surface track roughness .4 specular .8 reflection .6 clearcoat .5 diffuseMix .8 }\ntrack bill { sunDirection .7 -.2 -.4 sunColor 1 .8 .6 sunIntensity 1 sunConfidence 1 ambient .2 skyColor .5 .6 .9 groundColor .1 .1 .1 }\n");
    require(different(billboard_stock,render(billboard))>100,"ordinary track geometry stopped receiving material lighting");
    billboard.materials[0].primitive_flags=world_primitive_track_billboard_flag;
    require(render(billboard)==billboard_stock,"camera-facing billboard color changed under directional/material lighting");
    billboard.materials[0].primitive_flags=world_primitive_track_billboard_depth_flag;
    require(render(billboard)==billboard_stock,"legacy billboard subset color changed under directional/material lighting");
    write_shader(path,"settings { enabled 0 }\n");
    // Keep the existing subtle vehicle footprint rather than turning the
    // original reverse-subtract packet into a new opaque black polygon.
    WorldDrawList contact=l;contact.commands.clear();contact.vehicle_commands=0;
    quad(contact,0,0,500,300,1,1,{140,140,140});
    quad(contact,0,0,480,200,2,2,{255,255,255});
    WorldMaterial footprint{};footprint.primitive_flags=2;footprint.texture_page=2U<<5U;contact.materials.push_back(footprint);
    contact.commands[2].material_index=contact.commands[3].material_index=1;
    auto contact_image=render(contact);
    for(int y=40;y<56;y++)for(int x=40;x<56;x++)for(int ch=0;ch<3;ch++)
        require(std::abs(int(contact_image[(y*size+x)*4+ch])-132)<=1,"legacy car contact footprint is no longer subtle");
    // GLSL numerical agreement with the shipping shared CPU BRDF contract.
    // No shadow/probe ambiguity: a known flat face, exact source RGB, and
    // an analytically known perspective position for every sampled pixel.
    const std::array<std::string,4> controls={
        "roughness .26 specular .32 reflection .45 clearcoat .6 clearcoatRoughness .1 diffuseMix .2",
        "roughness .84 specular .07 reflection .025 clearcoat 0 diffuseMix .16",
        "roughness .95 specular .01 reflection 0 clearcoat 0 diffuseMix .18",
        "roughness .5 specular .7 reflection .8 clearcoat .4 clearcoatRoughness .3 diffuseMix .75"
    };
    // Lighting is deliberately disabled for car-only garage scenes. Include a
    // real (off-camera) track quad so this fixture exercises a race-like list.
    quad(l,0,0,-100,100,1,8,{80,80,80});
    unsigned samples=0;int largest=0;
    for(const auto& control:controls){
        std::string script="settings { enabled 1 shadows 0 localReflections 0 forceTrack parity }\nmaterial face { surface car "+control+" }\ntrack parity { sunDirection .2 -.1 -1 sunColor 1 .93 .83 sunIntensity .6 sunConfidence 1 ambient .84 skyColor .3 .39 .52 groundColor .15 .14 .13 }\n";
        write_shader(path,script);lighting::Database db;std::string error;
        require(lighting::parse_script(script,&db,&error),"CPU parity shader rejected");
        const auto prepared=lighting::prepare(l,db,lighting::select_track(db,{}));
        require(prepared.enabled&&prepared.solar,"parity fixture did not enable solar BRDF");
        auto gpu=render(l);
        for(int y=32;y<64;y+=3)for(int x=32;x<64;x+=3){
            const float vx=(float(x)+.5F-size*.5F)*500/(size*.5F);
            const float vy=(float(y)+.5F-size*.5F)*500/(size*.5F);
            const auto expected=lighting::shade_reference({103/255.F,103/255.F,103/255.F},{0,0,-1},{-vx,-vy,-500},prepared.materials[0],prepared.constants);
            const float rgb[]={expected.x,expected.y,expected.z};
            for(int ch=0;ch<3;ch++){
                int delta=std::abs(int(gpu[(y*size+x)*4+ch])-int(std::lround(rgb[ch]*255)));
                largest=std::max(largest,delta);++samples;require(delta<=2,"GLSL/shared CPU BRDF differs by more than two output levels");
            }
        }
    }
    std::cout<<"Material/legacy: exact 4x4 dither and opt-out, subtle .06-alpha vehicle contact, GLSL/CPU BRDF samples="<<samples<<" largest8BitError="<<largest<<" passed\n";
}

}
int main() {
    std::filesystem::path path=std::filesystem::temp_directory_path()/
        ("opengt-lighting-L01-gpu-test-"+std::to_string(getpid())+".shader");
    const char* prior_env=std::getenv("OPENGT_LIGHTING_SCRIPT");
    const std::string prior=prior_env?prior_env:"";
    try {
        require(setenv("OPENGT_LIGHTING_SCRIPT",path.u8string().c_str(),1)==0,"set test shader path");
        auto l=scene(false);
        config(path,false,false,false);auto stock=image(l);
        config(path,true,false,false);auto environment=image(l);
        require(different(stock,environment)>10,"environment material lighting did not change pixels");
        config(path,true,true,false);auto direct=image(l);
        require(different(environment,direct)>10,"directional light did not change pixels");
        config(path,true,true,true);auto shadow=image(l);
        const auto dark=different(direct,shadow,true);
        require(dark>4,"off-camera caster did not cast a sun shadow: shader/resource fallback or projection error");
        auto cutout=image(scene(true));
        require(different(direct,cutout)<different(direct,shadow),"zero-alpha VRAM caster did not remove shadow silhouette");
        config(path,false,false,false);require(image(l)==stock,"disabling lighting did not restore exact stock pixels");
        additional_graphics_tests(path);texture_and_queue_tests(path);material_and_legacy_contract_tests(path);widescreen_scissor_tests(path);
        std::error_code ec;std::filesystem::remove(path,ec);setenv("OPENGT_LIGHTING_SCRIPT",prior.c_str(),1);
        std::cout<<"Lighting GPU: EGL material, solar, off-camera shadow, keyed cutout and exact disable checks passed; darkerPixels="<<dark<<"\n";
        return 0;
    } catch(const std::exception& e) {
        std::error_code ec;std::filesystem::remove(path,ec);setenv("OPENGT_LIGHTING_SCRIPT",prior.c_str(),1);
        std::cerr<<"Lighting GPU FAIL: "<<e.what()<<"\n";return 1;
    }
}
