// Windows/WARP integration gate. Exercises the *production* D3D11 shaders.
// This is not a substitute renderer and is intentionally not built on Linux.
#include "opengt/world_gpu_renderer.hpp"
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
#if defined(_WIN32)
#include <process.h>
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
    WorldGpuRenderStats stats{};reset_world_d3d11_readback(true);
    auto result=render_world_d3d11(l,vram.data(),vram.size(),pixels.data(),pixels.size(),
        WorldGpuRenderOptions{true,true,false,true,false,false,1,0xFF050505U},&stats);
    if(result!=WorldGpuRenderResult::success)throw std::runtime_error(world_gpu_render_result_name(result));
    require(stats.output_valid,"WARP readback did not return an image");return pixels;
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
}
}
int main() {
    std::filesystem::path path=std::filesystem::temp_directory_path()/
        ("opengt-lighting-L01-gpu-test-"+std::to_string(_getpid())+".shader");
    const char* prior_env=std::getenv("OPENGT_LIGHTING_SCRIPT");
    const std::string prior=prior_env?prior_env:"";
    try {
        require(_putenv_s("OPENGT_LIGHTING_SCRIPT",path.u8string().c_str())==0,"set test shader path");
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
        std::error_code ec;std::filesystem::remove(path,ec);_putenv_s("OPENGT_LIGHTING_SCRIPT",prior.c_str());
        std::cout<<"Lighting GPU: WARP material, solar, off-camera shadow, keyed cutout and exact disable checks passed; darkerPixels="<<dark<<"\n";
        return 0;
    } catch(const std::exception& e) {
        std::error_code ec;std::filesystem::remove(path,ec);_putenv_s("OPENGT_LIGHTING_SCRIPT",prior.c_str());
        std::cerr<<"Lighting GPU FAIL: "<<e.what()<<"\n";return 1;
    }
}
