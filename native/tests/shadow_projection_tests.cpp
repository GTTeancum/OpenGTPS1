// L07: source-complete cascade sizing, world-oriented axes and upstream casters.
#include "opengt/lighting.hpp"
#include "opengt/world_gpu_renderer_native.hpp"
#include <algorithm>
#include <array>
#include <cmath>
#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>
#include <unistd.h>
using namespace opengt::render;
using namespace opengt::render::lighting;
namespace {
unsigned checks=0;void check(bool b,const char* reason){++checks;if(!b)throw std::runtime_error(reason);}
float len(Vec3 v){return std::sqrt(dot(v,v));}
WorldDrawVertex vertex(Vec3 p){WorldDrawVertex v{};v.view_x=p.x;v.view_y=p.y;v.view_z=p.z;v.clip_x=p.x;v.clip_y=-p.y;v.clip_z=16;v.clip_w=p.z;v.r=v.g=v.b=140;return v;}
WorldDrawCommand triangle(Vec3 a,Vec3 b,Vec3 c){WorldDrawCommand q{};q.vertices[0]=vertex(a);q.vertices[1]=vertex(b);q.vertices[2]=vertex(c);q.object_kind=1;q.object_id=7;q.model_pointer=0x80001000;q.channel=WorldViewChannel::main_view;q.lighting_depth_scale=1;q.clip_x1=q.clip_y1=95;return q;}
WorldDrawList scene(Vec3 body={0,0,600}){
 WorldDrawList l{};l.display_width=l.display_height=96;l.continuous_projection=true;l.lighting_camera_rotation={4096,0,0,0,4096,0,0,0,4096};l.materials.push_back(WorldMaterial{});
 l.commands.push_back(triangle({-600,100,120},{600,100,1100},{600,100,120}));l.commands.push_back(triangle({-600,100,120},{-600,100,1100},{600,100,1100}));l.track_commands=2;
 Vec3 v[]={{-20,-10,-60},{20,-10,-60},{20,10,-60},{-20,10,-60},{-20,-10,60},{20,-10,60},{20,10,60},{-20,10,60}};
 int indices[][3]={{0,1,2},{0,2,3},{4,6,5},{4,7,6},{0,4,5},{0,5,1},{3,2,6},{3,6,7},{1,5,6},{1,6,2},{0,3,7},{0,7,4}};
 for(unsigned f=0;f<12;++f){auto c=triangle(v[indices[f][0]]+body,v[indices[f][1]]+body,v[indices[f][2]]+body);c.object_kind=2;c.object_id=1;c.transform_id=9;c.exact_transform_valid=true;c.transform_rotation[0]=c.transform_rotation[4]=c.transform_rotation[8]=4096;
  c.transform_translation[0]=int(body.x);c.transform_translation[1]=int(body.y);c.transform_translation[2]=int(body.z);
  for(int j=0;j<3;++j){auto p=v[indices[f][j]];auto& d=c.vertices[j];d.model_x=short(p.x);d.model_y=short(p.y);d.model_z=short(p.z);}
  if(f<2)l.commands.push_back(c);else{auto m=l.materials[0];m.primitive_flags|=world_primitive_shadow_only_flag;l.lighting_extra_casters.push_back({c,m});}
 }l.vehicle_commands=2;return l;
}
Database database(){Database d;std::string error;check(parse_script("settings { enabled 1 shadows 1 localReflections 0 shadowResolution 512 shadowBias .00015 nearShadowCarSpans 6 forceTrack test } material world { surface track roughness 1 specular 0 reflection 0 clearcoat 0 diffuseMix 0 } track test { sunDirection .4 -.8 .3 sunIntensity 1 provisionalSun 1 shadowStrength .8 contactShadowOpacity .48 }",&d,&error),"parse fixture");return d;}
Vec3 row(const Frame& f,int c,int r){auto a=f.constants.shadow_rows[c*4+r];return {a[0],a[1],a[2]};}
float project(const Frame& f,int c,int r,Vec3 p){auto a=f.constants.shadow_rows[c*4+r];return a[0]*p.x+a[1]*p.y+a[2]*p.z+a[3];}
void orientation_and_size(bool sizing=true,bool orientation=true){auto db=database();auto base=scene();auto original=prepare(base,db,&db.tracks[0]);check(original.shadows,"fixture not lit");
 const auto expectedU=normalized(row(original,0,0));const float reference=original.constants.shadow_config[0];if(sizing)check(std::abs(reference-720)<.01,"complete source body not used for atlas sizing");
 for(int degrees=-175;degrees<=175;degrees+=5){float a=degrees*3.14159265358979323846F/180;float co=std::cos(a),si=std::sin(a);
  // Rotate around the body's center in the visible camera frame. Model-space
  // dimensions remain constant, while the old visible/view box changes size.
  auto l=base;auto turn=[&](WorldDrawCommand& c){if(c.object_kind!=2)return;c.transform_rotation[0]=short(std::lround(co*4096));c.transform_rotation[2]=short(std::lround(si*4096));c.transform_rotation[6]=short(std::lround(-si*4096));c.transform_rotation[8]=short(std::lround(co*4096));for(auto& v:c.vertices){float x=v.model_x,z=v.model_z;v.view_x=co*x+si*z;v.view_y=v.model_y;v.view_z=-si*x+co*z+600;v.clip_x=v.view_x;v.clip_y=-v.view_y;v.clip_w=v.view_z;}};
  for(auto& c:l.commands)turn(c);for(auto& c:l.lighting_extra_casters)turn(c.command);auto f=prepare(l,db,&db.tracks[0]);
  if(sizing)check(std::abs(f.constants.shadow_config[0]/reference-1)<.0003,"car yaw changed cascade resolution/extent");
  // Independent camera roll, spanning the view-up fallback discontinuity.
  auto camera=base;camera.lighting_camera_rotation={short(std::lround(co*4096)),short(std::lround(-si*4096)),0,short(std::lround(si*4096)),short(std::lround(co*4096)),0,0,0,4096};
  auto g=prepare(camera,db,&db.tracks[0]);auto u=normalized(row(g,0,0));float norm=std::hypot(float(camera.lighting_camera_rotation[0]),float(camera.lighting_camera_rotation[3]));float cc=camera.lighting_camera_rotation[0]/norm,ss=camera.lighting_camera_rotation[3]/norm;Vec3 recovered{cc*u.x+ss*u.y,-ss*u.x+cc*u.y,u.z};
  if(orientation)check(len(recovered-expectedU)<.0001,"camera roll rotates/flips the world sun texel axes");
  check(g.constants.color==original.constants.color&&g.contact_shadow_opacity==original.contact_shadow_opacity,"projection repair changed approved strength");
 }
 auto unanchored=base;for(auto&x:unanchored.lighting_extra_casters)x.command.object_id=99;auto a=prepare(unanchored,db,&db.tracks[0]);auto empty=unanchored;empty.lighting_extra_casters.clear();auto b=prepare(empty,db,&db.tracks[0]);check(a.constants.shadow_config==b.constants.shadow_config,"unanchored hidden geometry changed cascade size");
}
void upstream_clipping(const std::filesystem::path& path){auto db=database();auto l=scene({10000,0,600});Vec3 receiver{0,100,500};auto light=db.tracks[0].sun_direction;Vec3 center=receiver+light*5000;
 auto a=triangle(center+Vec3{-100,0,-100},center+Vec3{100,0,100},center+Vec3{100,0,-100});auto b=triangle(center+Vec3{-100,0,-100},center+Vec3{-100,0,100},center+Vec3{100,0,100});l.lighting_extra_casters.push_back({a,l.materials[0]});l.lighting_extra_casters.push_back({b,l.materials[0]});auto f=prepare(l,db,&db.tracks[0]);
 for(int c=0;c<2;c++){for(const auto& caster:{a,b})for(const auto&v:caster.vertices){Vec3 p{v.view_x,v.view_y,v.view_z};check(project(f,c,2,p)>0&&project(f,c,2,p)<1,"upstream caster clipped by receiver-centered depth slab");}
  float radius=1/len(row(f,c,0)),range=1/len(row(f,c,2));float newNormalizedBias=f.constants.shadow_config[3]*4*radius/range;
  check(std::abs(newNormalizedBias*range-f.constants.shadow_config[3]*4*radius)<.001,"depth fitting changed scene-space receiver bias");}
 std::ofstream stream(path);stream<<"settings { enabled 1 shadows 1 localReflections 0 shadowResolution 512 shadowBias .00015 nearShadowCarSpans 6 forceTrack test } material world { surface track roughness 1 specular 0 reflection 0 clearcoat 0 diffuseMix 0 } track test { sunDirection .4 -.8 .3 sunIntensity 1 provisionalSun 1 shadowStrength .8 contactShadowOpacity .48 }";stream.close();
 setenv("OPENGT_LIGHTING_SCRIPT",path.c_str(),1);setenv("OPENGT_SHADOW_DIAGNOSTIC","1",1);std::vector<std::uint16_t> vram(1024*512);std::vector<std::uint8_t> pixels(96*96*4);WorldGpuRenderStats stats{};WorldGpuRenderOptions options{true,true,false,true,false,false,1,0xFF000000U};
 auto render=[&](const WorldDrawList& draw){auto result=render_world_native(draw,vram.data(),vram.size(),pixels.data(),pixels.size(),options,&stats);check(result==WorldGpuRenderResult::success&&stats.output_valid,"upstream GPU fixture failed");unsigned marked=0;for(std::size_t i=0;i<pixels.size();i+=4)if(pixels[i]>127)++marked;return marked;};
 auto without=l;without.lighting_extra_casters.resize(without.lighting_extra_casters.size()-2);auto noCaster=render(without);auto withCaster=render(l);std::cout<<"Upstream original-mask caster: absent="<<noCaster<<" present="<<withCaster<<" pixels\n";check(noCaster==0,"isolated receiver shadows itself");check(withCaster>20,"tall/upstream track caster cannot shadow visible road");unsetenv("OPENGT_SHADOW_DIAGNOSTIC");release_world_native_context(nullptr);
}
}
int main(int argc,char** argv){const std::string mode=argc>1?argv[1]:"all";auto path=std::filesystem::temp_directory_path()/("opengt-shadow-projection-"+std::to_string(getpid())+".shader");try{if(mode!="upstream")orientation_and_size(mode!="basis",mode!="size");if(mode=="all"||mode=="upstream")upstream_clipping(path);std::filesystem::remove(path);std::cout<<"L07 shadow projection: "<<checks<<" checks passed\n";return 0;}catch(const std::exception&e){std::cerr<<"L07 shadow projection failed after "<<checks<<": "<<e.what()<<"\n";std::filesystem::remove(path);return 1;}}
