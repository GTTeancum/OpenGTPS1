// L07: exercise production resident decoding/commit, not a parallel decoder.
#include "../src/live_renderer_bridge.cpp"
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <unistd.h>
namespace {
unsigned checks=0;void check(bool value,const char* why){++checks;if(!value)throw std::runtime_error(why);}
opengt::render::WorldCaptureHeader header(){opengt::render::WorldCaptureHeader h{};h.display_width=320;h.display_height=240;h.input_poll=61;h.frame_index=62;h.camera_rotation[0]=h.camera_rotation[4]=h.camera_rotation[8]=4096;h.projection_plane=256;h.projection_offset_x=(160<<16)+32768;h.projection_offset_y=(120<<16)+32768;h.camera_transform_id=1;return h;}
void setup(LiveContext& c,bool primary,bool quad,bool reverse){
 ResidentMesh mesh;mesh.key=1;mesh.vertices={{-100,-100,0,1},{100,-100,0,2},{100,100,0,3},{-100,100,0,4}};
 ResidentPrimitive p;p.source_address=0x80001000;p.flags=resident_primitive_textured|resident_primitive_one_sided|(quad?resident_primitive_quad:0)|(primary?resident_primitive_primary_path:0);p.lod_threshold=2000;
 for(int k=0;k<4;++k){p.indices[k]=static_cast<std::uint16_t>(reverse?3-k:k);p.near_material.uv[k]=std::uint16_t((10+k)|((20+k)<<8));p.distant_material.uv[k]=std::uint16_t((100+k)|((110+k)<<8));p.near_material.color[k]=0x8090a0;p.distant_material.color[k]=0xa09080;}
 p.near_material.texture_page=3;p.near_material.clut=11;p.distant_material.texture_page=4;p.distant_material.clut=12;mesh.primitives={p};c.resident_meshes.emplace(1,std::move(mesh));ResidentInstance i;i.mesh_key=1;i.object_id=7;i.model_pointer=0x80002000;i.transform_id=1;i.rotation[0]=i.rotation[4]=i.rotation[8]=4096;i.translation[2]=500;i.projection_offset_x=(160<<16)+32768;i.projection_offset_y=(120<<16)+32768;i.projection_plane=256;i.clip_x1=319;i.clip_y1=239;i.depth_scale_valid=true;c.resident_instances={i};
}
void build_visible(LiveContext& c,const opengt::render::WorldCaptureHeader& h){using namespace opengt::render;
 auto result=build_world_draw_list(h,c.triangles.data(),c.triangles.size(),WorldDrawListOptions{true,true,true,true},&c.draw_list);check(result==WorldDrawListResult::success,"visible resident list conversion failed");c.draw_list.lighting_extra_casters.clear();
 if(!c.lighting_rejected_casters.empty()){WorldDrawList rejected;result=build_world_draw_list(h,c.lighting_rejected_casters.data(),c.lighting_rejected_casters.size(),WorldDrawListOptions{false,false,true,true},&rejected);check(result==WorldDrawListResult::success,"eye-rejected reflection list failed");for(auto q:rejected.commands)c.draw_list.lighting_extra_casters.push_back({q,rejected.materials[q.material_index]});}
}
void cases(){using namespace opengt::render;
 for(bool primary:{false,true})for(bool quad:{false,true})for(bool reverse:{false,true}){
  LiveContext context;setup(context,primary,quad,reverse);auto h=header();std::vector<std::array<int,8>> sourceNear;
  // Force opposite authored eye-LOD choices by projected coverage, without
  // changing the shape/pose. The camera-facing test remains the real decoder.
  for(bool distant:{false,true}){context.triangles.clear();context.resident_meshes.at(1).primitives[0].lod_threshold=100;context.resident_instances[0].projection_plane=distant?1:256;h.projection_plane=distant?1:256;
   check(append_resident_course(&context,h)==0,"resident decode failed");const auto count=quad?2U:1U;
   check(context.triangles.size()+context.lighting_rejected_casters.size()==count,"eye culling lost a quad half");
   for(auto& t:context.triangles){check(t.texture_page==(distant?4:3),"visible eye LOD was forced to high detail");check(t.clut==(distant?12:11),"visible palette changed");}
   for(auto& t:context.lighting_rejected_casters)check(t.texture_page==(distant?4:3),"reflection eye LOD was changed");
   check(context.lighting_track_source_casters.size()==count,"complete source sun faces missing");std::vector<std::array<int,8>> source;
   for(auto& t:context.lighting_track_source_casters){check(t.texture_page==3&&t.clut==11,"sun caster selected eye LOD palette");for(auto v:t.vertices)source.push_back({v.model_x,v.model_y,v.model_z,v.u,v.v,int(t.texture_page),int(t.clut),int(v.source_vertex_identity)});}
   if(!distant)sourceNear=source;else check(source==sourceNear,"sun mask/UV changed at eye-LOD transition");
   build_visible(context,h);auto before=context.draw_list.commands.size();commit_resident_track_shadow_sources(&context,h);check(context.draw_list.commands.size()==before,"source caster commit changed visible geometry");
   unsigned replaced=0,extra=0;for(const auto&x:context.draw_list.lighting_extra_casters){if(x.material.primitive_flags&world_primitive_shadow_only_flag){++extra;check(!(x.material.primitive_flags&world_primitive_sun_caster_replaced_flag),"new sun face suppressed itself");}else check(x.material.primitive_flags&world_primitive_sun_caster_replaced_flag,"old eye-rejected face still casts duplicate sun shadow");}
   for(auto m:context.draw_list.materials)replaced+=(m.primitive_flags&world_primitive_sun_caster_replaced_flag)!=0;
   check(extra==count,"sun source commit omitted faces");check(before==0||replaced>0,"visible eye-LOD caster not suppressed");
   // A rejected normalization must not remove the old caster or leak stale
   // source faces into a subsequent frame. Conversion is all-or-nothing.
   build_visible(context,h);for(auto&t:context.lighting_track_source_casters)t.depth_scale_valid=false;
   commit_resident_track_shadow_sources(&context,h);for(auto m:context.draw_list.materials)check(!(m.primitive_flags&world_primitive_sun_caster_replaced_flag),"failed source conversion left visible shadow missing");
   for(auto x:context.draw_list.lighting_extra_casters)check(!(x.material.primitive_flags&(world_primitive_sun_caster_replaced_flag|world_primitive_shadow_only_flag)),"failed source conversion retained partial/new shadow faces");
  }
  // Subtractive effects/transparent source surfaces cannot become opaque sun casters.
  context.triangles.clear();context.resident_meshes.at(1).primitives[0].flags|=resident_primitive_semi_transparent;check(append_resident_course(&context,h)==0,"semi-transparent source decode failed");check(context.lighting_track_source_casters.empty(),"transparent effects became solid sun casters");
 }
}
}
int main(){auto path=std::filesystem::temp_directory_path()/("opengt-track-shadow-"+std::to_string(getpid())+".shader");try{std::ofstream f(path);f<<"settings { enabled 1 shadows 1 forceTrack test } track test { sunDirection .4 -.8 .3 sunIntensity 1 provisionalSun 1 }";f.close();setenv("OPENGT_LIGHTING_SCRIPT",path.c_str(),1);cases();std::filesystem::remove(path);std::cout<<"L07 track source shadows: "<<checks<<" checks passed\n";return 0;}catch(const std::exception&e){std::cerr<<"L07 track source shadows failed after "<<checks<<": "<<e.what()<<"\n";std::filesystem::remove(path);return 1;}}
