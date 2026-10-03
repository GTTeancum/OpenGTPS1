#include "opengt/vehicle_shadow_source.hpp"
#include <iostream>
#include <stdexcept>
#include <cmath>
using namespace opengt::render;
unsigned checks=0;void check(bool x,const char* why){++checks;if(!x)throw std::runtime_error(why);}
void u32(std::vector<std::uint8_t>& p,std::size_t o,std::uint32_t v){for(int i=0;i<4;i++)p[o+i]=std::uint8_t(v>>(i*8));}
VehicleShadowSource source(){VehicleShadowSource s;s.model=0x80001234;s.lod=0x80002345;s.object=2;s.poll=10;s.generation=1;
 for(int i=0;i<3;++i){VehicleShadowFace f;f.vertices[0]={std::int16_t(i*100),0,0,0,0};f.vertices[1]={std::int16_t(i*100+50),0,0,0,0};f.vertices[2]={std::int16_t(i*100),50,0,0,0};s.faces.push_back(f);}return s;}
WorldDrawList frame(const VehicleShadowSource& s){WorldDrawList l;l.input_poll=10;l.materials.push_back({});for(int i=0;i<2;++i){WorldDrawCommand c{};c.object_id=s.object;c.object_kind=2;c.model_pointer=s.model;c.lighting_depth_scale=2;c.exact_transform_valid=true;c.transform_rotation[0]=c.transform_rotation[4]=c.transform_rotation[8]=4096;c.transform_translation[2]=1000;
for(int j=0;j<3;++j){c.vertices[j].model_x=s.faces[i].vertices[j].x;c.vertices[j].model_y=s.faces[i].vertices[j].y;c.vertices[j].model_z=0;}l.commands.push_back(c);}return l;}
int main(){try{auto s=source();auto l=frame(s);auto stats=append_vehicle_shadow_sources(l,{s});check(stats.matched==1&&stats.added==1,"complete selected LOD did not restore hidden face");auto c=l.lighting_extra_casters[0];check(c.material.primitive_flags&world_primitive_shadow_only_flag,"hidden body face leaked into ordinary scene/probe stream");check(c.command.vertices[0].view_x==200&&c.command.vertices[0].view_z==1000&&c.command.lighting_depth_scale==2,"hidden face lost actual captured pose or normalization");
for(int test=0;test<7;++test){l=frame(s);auto d=s;if(test==0)l.input_poll=9;if(test==1)l.input_poll=14;if(test==2)l.commands[1].object_id=99;if(test==3)l.commands[1].exact_transform_valid=false;if(test==4)l.commands[1].channel=WorldViewChannel::secondary_view;if(test==5)l.commands[1].transform_translation[0]=10;if(test==6)d.faces[0].vertices[0].x=77;stats=append_vehicle_shadow_sources(l,{d});check(stats.added==0,"stale/future/unverified/secondary/wrong-pose source accepted");}
 std::vector<std::uint8_t> b(48+3*32);u32(b,0,std::uint32_t(vehicle_shadow_magic));u32(b,4,std::uint32_t(vehicle_shadow_magic>>32));u32(b,8,1);u32(b,12,48);u32(b,16,s.model);u32(b,20,s.lod);u32(b,24,2);u32(b,28,10);u32(b,32,1);u32(b,36,3);VehicleShadowSource parsed;check(parse_vehicle_shadow_source(b.data(),b.size(),&parsed),"valid typed source rejected");
for(std::size_t n=0;n<b.size();++n)check(!parse_vehicle_shadow_source(b.data(),n,&parsed),"truncated source accepted");
for(auto off:{0U,8U,12U,16U,20U,28U,32U,36U,40U,44U,48U}){auto bad=b;u32(bad,off,off==32U?0U:0xffffffffU);check(!parse_vehicle_shadow_source(bad.data(),bad.size(),&parsed),"invalid source field accepted");}
check(parsed.model==s.model&&parsed.faces.size()==3,"failed parse damaged previously valid source");std::cout<<"Vehicle shadow source: "<<checks<<" checks passed\n";return 0;}catch(const std::exception&e){std::cerr<<"FAIL: "<<e.what()<<"\n";return 1;}}
