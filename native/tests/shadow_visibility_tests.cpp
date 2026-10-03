// L05: actual EGL pixels for per-course shadow strength and legacy isolation.
#include "opengt/lighting.hpp"
#include "opengt/world_gpu_renderer_native.hpp"
#include <algorithm>
#include <cstdint>
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
#include <unistd.h>
using namespace opengt::render;
namespace {
constexpr int size=96;unsigned checks=0;
void check(bool b,const char* why){++checks;if(!b)throw std::runtime_error(why);}
WorldDrawVertex point(float x,float y,float z){WorldDrawVertex v{};v.view_x=x;v.view_y=y;v.view_z=z;v.clip_x=x;v.clip_y=-y;v.clip_z=16;v.clip_w=z;v.r=v.g=v.b=140;return v;}
WorldDrawCommand triangle(WorldDrawVertex a,WorldDrawVertex b,WorldDrawVertex c,unsigned material=0,unsigned kind=1){
    WorldDrawCommand t{};t.vertices[0]=a;t.vertices[1]=b;t.vertices[2]=c;t.material_index=material;t.object_kind=kind;t.object_id=kind;t.model_pointer=kind*256;t.channel=WorldViewChannel::main_view;t.lighting_depth_scale=1;t.clip_x1=t.clip_y1=size-1;return t;}
void quad(WorldDrawList& l,float half,float z,unsigned material,unsigned kind){auto a=point(-half,-half,z),b=point(half,-half,z),c=point(half,half,z),d=point(-half,half,z);l.commands.push_back(triangle(a,c,b,material,kind));l.commands.push_back(triangle(a,d,c,material,kind));}
WorldDrawList contact_scene(){WorldDrawList l{};l.display_width=l.display_height=size;l.continuous_projection=true;l.lighting_camera_rotation={4096,0,0,0,4096,0,0,0,4096};l.materials.push_back(WorldMaterial{});WorldMaterial m{};m.primitive_flags=2;m.texture_page=2U<<5U;l.materials.push_back(m);quad(l,300,500,0,1);quad(l,170,480,1,2);l.track_commands=l.vehicle_commands=2;return l;}
std::string shader(float alpha,bool enabled=true,bool shadows=true,bool night=false,bool selected=true,float strength=0){
 return "settings { enabled "+std::to_string(enabled)+" shadows "+std::to_string(shadows)+" localReflections 0 shadowResolution 512 shadowBias .00015 nearShadowCarSpans 6 "+(selected?"forceTrack test":"")+" }\nmaterial world { surface track roughness 1 specular 0 reflection 0 clearcoat 0 diffuseMix 0 }\ntrack test { sunDirection .4 -.8 .3 sunIntensity 1 provisionalSun 1 ambient .84 night "+std::to_string(night)+" shadowStrength "+std::to_string(strength)+" contactShadowOpacity "+std::to_string(alpha)+" }\n";}
void write(const std::filesystem::path& p,const std::string& s){std::ofstream f(p,std::ios::binary|std::ios::trunc);f<<s;f.close();check(bool(f),"cannot flush shadow test config");std::this_thread::sleep_for(std::chrono::milliseconds(1060));}
std::vector<std::uint8_t> render(const WorldDrawList& l){std::vector<std::uint16_t> vram(1024*512);std::vector<std::uint8_t> out(size*size*4);WorldGpuRenderStats stats{};auto result=render_world_native(l,vram.data(),vram.size(),out.data(),out.size(),WorldGpuRenderOptions{true,true,false,true,false,false,1,0xFF050505U},&stats);check(result==WorldGpuRenderResult::success&&stats.output_valid,"shadow pixel render failed");return out;}
// Coverage union must preserve the accepted opacity rather than accumulating
// source strip overlap. Independent depth occluders still win.
void contact_union_tests(const std::filesystem::path& path) {
    write(path,shader(.50F));auto scene=contact_scene();auto single=render(scene);
    auto original=scene.commands;
    for(int i=0;i<4;++i){auto duplicate=original[2+i%2];duplicate.object_id=20+i;scene.commands.push_back(duplicate);}
    check(render(scene)==single,"overlapping contact strips accumulated darkness");
    std::reverse(scene.commands.begin(),scene.commands.end());
    check(render(scene)==single,"contact coverage depends on source submission order");
    quad(scene,95,400,0,2);for(std::size_t i=scene.commands.size()-2;i<scene.commands.size();++i)for(auto&v:scene.commands[i].vertices){v.r=220;v.g=20;v.b=10;}
    auto front=render(scene);check(front[(48*size+48)*4]==220,"contact union drew through a nearer solid object");
    auto fresh=contact_scene();check(render(fresh)==single,"contact stencil leaked into next frame");
    release_world_native_context(nullptr);check(render(fresh)==single,"contact stencil leaked across context recreation");
}
void parser(){lighting::Database d;std::string error;
 for(float f:{0.0F,.06F,.48F,.5F,.8F}){check(lighting::parse_script(shader(f),&d,&error),"valid shadow opacity rejected");check(std::abs(d.tracks[0].contact_shadow_opacity-f)<1e-6F,"shadow opacity not retained");}
 for(auto bad:{"-.01",".801","nan","inf","-inf","2"}){check(!lighting::parse_script(std::string("track test { contactShadowOpacity ")+bad+" }",&d,&error),"invalid contact opacity accepted");check(d.tracks[0].contact_shadow_opacity==.8F,"invalid config changed last good profile");}
 check(!lighting::parse_script("track test { contactShadowOpacity .1 contactShadowOpacity .2 }",&d,&error),"duplicate opacity accepted");
 check(lighting::parse_script("track old { }",&d,&error)&&d.tracks[0].contact_shadow_opacity==.06F,"old shader's default changed");
 check(lighting::parse_script(shader(.48F),&d,&error),"prepare test shader parse");auto l=contact_scene();auto f=lighting::prepare(l,d,&d.tracks[0]);check(f.shadows&&f.contact_shadow_opacity==.48F,"active frame did not forward profile opacity");
 d.settings.shadows=false;check(lighting::prepare(l,d,&d.tracks[0]).contact_shadow_opacity==.06F,"disabled shadow pass changed source footprint");d.settings.shadows=true;
 d.tracks[0].night=true;check(lighting::prepare(l,d,&d.tracks[0]).contact_shadow_opacity==.06F,"night fallback changed source footprint");d.tracks[0].night=false;
 check(lighting::prepare(l,d,nullptr).contact_shadow_opacity==.06F,"unknown profile changed source footprint");
}
void contact_tests(const std::filesystem::path& p){auto l=contact_scene();write(p,shader(.06F));auto old=render(l);write(p,shader(.48F));auto strong=render(l);
 for(int y=40;y<56;y++)for(int x=40;x<56;x++)for(int c=0;c<3;c++){std::size_t i=(y*size+x)*4+c;check(std::abs(int(old[i])-132)<=1,"baseline .06 footprint changed");check(std::abs(int(strong[i])-73)<=1,".48 footprint does not darken the original geometry");}
 for(int y=0;y<size;y++)for(int x=0;x<size;x++)if(x<30||x>=66||y<30||y>=66)for(int c=0;c<4;c++){auto i=(y*size+x)*4+c;check(old[i]==strong[i],"opacity changed pixels outside the source footprint");}
 write(p,shader(.50F));auto dusk=render(l);check(dusk[(48*size+48)*4]==70,"dusk .50 opacity not applied");
 write(p,shader(0));auto zero=render(l);check(zero[(48*size+48)*4]==140,"explicit zero opacity still darkens ground");
 write(p,shader(.48F));auto good=render(l);write(p,"track test { contactShadowOpacity nan }");check(render(l)==good,"invalid reload lost last valid opacity");
 for(auto text:{shader(.48F,false),shader(.48F,true,false),shader(.48F,true,true,true),shader(.48F,true,true,false,false)}){write(p,text);check(render(l)==old,"inactive/disabled/night/unknown profile changed original contact treatment");}
 write(p,shader(.48F));auto sub=l;sub.commands[2].channel=sub.commands[3].channel=WorldViewChannel::secondary_view;check(render(sub)==old,"stronger main shadow leaked into a secondary view");auto screen=l;screen.materials[1].primitive_flags|=world_primitive_screen_space_flag;check(render(screen)==old,"stronger shadow leaked into screen-space art");
 // An unrelated subtractive primitive retains the original source blend.
 auto other=l;other.commands[2].object_kind=other.commands[3].object_kind=1;auto first=render(other);write(p,shader(.06F));check(render(other)==first,"car footprint setting changed non-car subtraction");
}
WorldDrawList projected_scene(){WorldDrawList l{};l.display_width=l.display_height=size;l.continuous_projection=true;l.lighting_camera_rotation={4096,0,0,0,4096,0,0,0,4096};l.materials.push_back(WorldMaterial{});auto a=point(-450,100,120),b=point(450,100,120),c=point(450,100,1100),d=point(-450,100,1100);l.commands.push_back(triangle(a,c,b));l.commands.push_back(triangle(a,d,c));l.track_commands=2;auto e=point(-70,-30,360),f=point(70,-30,360),g=point(70,-30,510),h=point(-70,-30,510);l.lighting_extra_casters.push_back({triangle(e,f,g),l.materials[0]});l.lighting_extra_casters.push_back({triangle(e,g,h),l.materials[0]});return l;}
void receiver_plane_tests(const std::filesystem::path& p){
 // An isolated receiver cannot cast a projected shadow onto itself. Check low
 // dusk elevations as well as daytime light, flat/sloping roads and both atlas
 // sizes. This test exposed the old center-depth-for-all-PCF-taps defect.
 const char* suns[]={".4 -.8 .3", ".383 -.25 -.889", "1 -.08 .2"};
 for(int resolution:{512,2048})for(int slope:{0,1})for(auto sun:suns){
   auto l=projected_scene();l.lighting_extra_casters.clear();
   if(slope)for(auto& t:l.commands)for(auto& v:t.vertices){
     v.view_y+=.10F*(v.view_z-120)+.08F*v.view_x;v.clip_y=-v.view_y;
   }
   auto config=shader(.06F,true,true,false,true,.8F);
   auto at=config.find("nearShadowCarSpans 6");config.replace(at,20,"nearShadowCarSpans 36");
   at=config.find("shadowResolution 512");config.replace(at,20,"shadowResolution "+std::to_string(resolution));
   at=config.find("sunDirection .4 -.8 .3");config.replace(at,21,std::string("sunDirection ")+sun);
   write(p,config);setenv("OPENGT_SHADOW_DIAGNOSTIC","1",1);
   auto mask=render(l);unsetenv("OPENGT_SHADOW_DIAGNOSTIC");
   unsigned maximum=0,spurious=0;for(std::size_t i=0;i<mask.size();i+=4){maximum=std::max(maximum,unsigned(mask[i]));if(mask[i]>8)++spurious;}
   std::cout<<"Isolated receiver: resolution="<<resolution<<" slope="<<slope<<" sun="<<sun<<" max="<<maximum<<" spurious="<<spurious<<"\n";
   check(spurious==0,"isolated road falsely shadows itself under PCF");
 }
}
void projected_tests(const std::filesystem::path& p){auto l=projected_scene();
 // This no-car fixture sizes its cascades from median receiver depth. Keep
 // the receiver well inside the near cascade, not in the deliberate far fade.
 auto config=[](float strength){auto s=shader(.06F,true,true,false,true,strength);auto at=s.find("nearShadowCarSpans 6");s.replace(at,20,"nearShadowCarSpans 36");return s;};
 write(p,config(.4F));auto before=render(l);write(p,config(.8F));auto after=render(l);unsigned darker=0,unchanged=0;double ratio=0;
 for(std::size_t i=0;i<before.size();i+=4){check(after[i]<=before[i],"stronger projected shadow made pixels brighter");if(int(before[i])-after[i]>12){++darker;ratio+=double(after[i])/before[i];}else if(after[i]==before[i])++unchanged;}
 std::cerr<<"PROJECTED_DIAGNOSTIC darker="<<darker<<" ratio="<<ratio/darker<<" unchanged="<<unchanged<<"\n";
 check(darker>10,"projected sun shadows did not visibly deepen");check(ratio/darker<.8,"projected shadow increase too weak");check(unchanged>size*size/2,"shadow control darkened unrelated image areas");
 std::cout<<"Projected shadow fixture: "<<darker<<" clearly darker pixels; mean new/old sRGB="<<ratio/darker<<"; unchanged="<<unchanged<<"\n";
 setenv("OPENGT_SHADOW_DIAGNOSTIC","1",1);auto mask=render(l);unsetenv("OPENGT_SHADOW_DIAGNOSTIC");unsigned marked=0;for(std::size_t i=0;i<mask.size();i+=4){check(mask[i]==mask[i+1]&&mask[i]==mask[i+2],"occlusion diagnostic is not grayscale");if(mask[i]>127)++marked;}check(marked>10,"occlusion diagnostic cannot see the off-camera caster");check(render(l)==after,"diagnostic mode changed subsequent normal output");
}
}
int main(){std::filesystem::path p=std::filesystem::temp_directory_path()/("opengt-shadow-L05-"+std::to_string(getpid())+".shader");try{setenv("OPENGT_LIGHTING_SCRIPT",p.c_str(),1);unsetenv("OPENGT_SHADOW_DIAGNOSTIC");parser();contact_tests(p);contact_union_tests(p);projected_tests(p);receiver_plane_tests(p);std::filesystem::remove(p);std::cout<<"L05 shadow visibility: "<<checks<<" checks passed\n";return 0;}catch(const std::exception&e){std::cerr<<"L05 shadow visibility failed after "<<checks<<" checks: "<<e.what()<<"\n";std::filesystem::remove(p);return 1;}}
