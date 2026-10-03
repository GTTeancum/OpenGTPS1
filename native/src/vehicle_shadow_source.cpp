#include "opengt/vehicle_shadow_source.hpp"
#include <algorithm>
#include <array>
#include <cmath>
#include <cstring>
#include <map>
#include <set>
#include <tuple>

namespace opengt::render {
namespace {
std::uint16_t u16(const std::uint8_t* p){return std::uint16_t(p[0])|std::uint16_t(p[1])<<8;}
std::uint32_t u32(const std::uint8_t* p){return u16(p)|std::uint32_t(u16(p+2))<<16;}
std::uint64_t u64(const std::uint8_t* p){return u32(p)|std::uint64_t(u32(p+4))<<32;}
using Point=std::array<std::int16_t,3>;
using FaceKey=std::array<Point,3>;
FaceKey key(const VehicleShadowFace& f){FaceKey r;for(int j=0;j<3;++j)r[j]={f.vertices[j].x,f.vertices[j].y,f.vertices[j].z};std::sort(r.begin(),r.end());return r;}
FaceKey key(const WorldDrawCommand& c){FaceKey r;for(int j=0;j<3;++j)r[j]={c.vertices[j].model_x,c.vertices[j].model_y,c.vertices[j].model_z};std::sort(r.begin(),r.end());return r;}
bool equal_material(const WorldMaterial& a,const WorldMaterial& b){return (a.primitive_flags&3)==(b.primitive_flags&3)&&(!(a.primitive_flags&1)||(a.texture_page==b.texture_page&&a.clut==b.clut));}
bool equal_uvs(const WorldDrawCommand& c,const VehicleShadowFace& f){
    if(!(f.material.primitive_flags&1U))return true;
    for(const auto& v:c.vertices){bool found=false;for(const auto& p:f.vertices)
        if(v.model_x==p.x&&v.model_y==p.y&&v.model_z==p.z&&v.u==p.u&&v.v==p.v){found=true;break;}
        if(!found)return false;
    }return true;
}
bool equal_pose(const WorldDrawCommand& a,const WorldDrawCommand& b){return a.object_id==b.object_id&&a.model_pointer==b.model_pointer&&a.lighting_depth_scale==b.lighting_depth_scale&&a.exact_transform_valid&&b.exact_transform_valid&&std::equal(a.transform_rotation,a.transform_rotation+9,b.transform_rotation)&&std::equal(a.transform_translation,a.transform_translation+3,b.transform_translation);}
}
bool parse_vehicle_shadow_source(const std::uint8_t* p,std::size_t n,VehicleShadowSource* out) noexcept{
    if(!p||!out||n<48||u64(p)!=vehicle_shadow_magic||u32(p+8)!=1||u32(p+12)!=48||u32(p+40)||u32(p+44))return false;
    const auto count=u32(p+36);if(!count||count>8192||n!=48+std::size_t(count)*32)return false;
    VehicleShadowSource s;s.model=u32(p+16);s.lod=u32(p+20);s.object=u32(p+24);s.poll=static_cast<std::int32_t>(u32(p+28));s.generation=u32(p+32);
    auto ram=[](std::uint32_t a){return (a&0xffe00000U)==0x80000000U||(a&0xffe00000U)==0xa0000000U;};
    if(!ram(s.model)||!ram(s.lod)||!s.generation||s.poll<0||s.object>255)return false;
    try{s.faces.resize(count);for(std::size_t i=0;i<count;++i){auto q=p+48+i*32;auto& f=s.faces[i];f.material.primitive_flags=u32(q);if(f.material.primitive_flags&~3U)return false;f.material.texture_page=u16(q+4);f.material.clut=u16(q+6);
        for(int j=0;j<3;++j){auto v=q+8+j*8;f.vertices[j]={static_cast<std::int16_t>(u16(v)),static_cast<std::int16_t>(u16(v+2)),static_cast<std::int16_t>(u16(v+4)),v[6],v[7]};}}
        *out=std::move(s);return true;}catch(...){return false;}
}
VehicleShadowStats append_vehicle_shadow_sources(WorldDrawList& list,const std::vector<VehicleShadowSource>& sources){
    VehicleShadowStats stats;
    // Source poll selection is bounded and cannot use geometry from a later frame.
    std::map<std::pair<std::uint32_t,std::uint32_t>,const VehicleShadowSource*> selected;
    for(const auto& s:sources){if(s.poll>list.input_poll||std::int64_t(list.input_poll)-s.poll>3)continue;auto k=std::make_pair(s.object,s.model);auto it=selected.find(k);if(it==selected.end()||it->second->poll<s.poll)selected[k]=&s;}
    for(const auto& item:selected){const auto& source=*item.second;++stats.definitions;
        std::map<FaceKey,std::vector<const VehicleShadowFace*>> faces;
        for(const auto& f:source.faces)if(!(f.material.primitive_flags&2))faces[key(f)].push_back(&f);
        std::vector<const WorldDrawCommand*> matches;
        for(const auto& c:list.commands){if(c.object_kind!=2||c.object_id!=source.object||c.model_pointer!=source.model||c.channel!=WorldViewChannel::main_view||!c.exact_transform_valid||!std::isfinite(c.lighting_depth_scale)||c.lighting_depth_scale<=0||c.material_index>=list.materials.size())continue;
            const auto& m=list.materials[c.material_index];if(m.primitive_flags&(world_primitive_screen_space_flag|2U))continue;auto it=faces.find(key(c));if(it==faces.end())continue;
            if(std::any_of(it->second.begin(),it->second.end(),[&](auto f){return equal_material(f->material,m)&&equal_uvs(c,*f);}))matches.push_back(&c);}
        const WorldDrawCommand* anchor=nullptr;std::size_t best=0;
        for(auto c:matches){std::size_t count=0;for(auto d:matches)if(equal_pose(*c,*d))++count;if(count>best){best=count;anchor=c;}}
        if(best<2||!anchor){++stats.missing;continue;}++stats.matched;
        std::set<FaceKey> visible;for(auto c:matches)if(equal_pose(*anchor,*c))visible.insert(key(*c));
        for(const auto& f:source.faces){if(f.material.primitive_flags&2U||visible.count(key(f)))continue;
            WorldShadowCaster caster;caster.command=*anchor;caster.material=f.material;caster.material.primitive_flags|=world_primitive_shadow_only_flag;
            // Source car packets use the same active texture window as their
            // verified visible anchor; hidden faces retain their own page/CLUT/UV.
            const auto& a=list.materials[anchor->material_index];caster.material.texture_mask_x=a.texture_mask_x;caster.material.texture_mask_y=a.texture_mask_y;caster.material.texture_offset_x=a.texture_offset_x;caster.material.texture_offset_y=a.texture_offset_y;
            for(int j=0;j<3;++j){auto& v=caster.command.vertices[j];const auto& src=f.vertices[j];v.model_x=src.x;v.model_y=src.y;v.model_z=src.z;v.u=src.u;v.v=src.v;v.r=v.g=v.b=128;v.source_vertex_identity=0;
                double model[3]={double(src.x),double(src.y),double(src.z)},view[3];for(int axis=0;axis<3;++axis){view[axis]=anchor->transform_translation[axis];for(int k=0;k<3;++k)view[axis]+=double(anchor->transform_rotation[axis*3+k])*model[k]/4096.;}
                v.view_x=float(view[0]);v.view_y=float(view[1]);v.view_z=float(view[2]);}
            list.lighting_extra_casters.push_back(caster);++stats.added;
        }
    }return stats;
}
}
