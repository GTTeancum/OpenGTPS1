#include "opengt/lighting.hpp"
#include <cmath>
#include <iostream>
#include <limits>
#include <stdexcept>
using namespace opengt::render;
using namespace opengt::render::lighting;
namespace {
unsigned checks=0;
void check(bool yes,const char* what){++checks;if(!yes)throw std::runtime_error(what);}
const char* sample=R"(
settings { enabled 1 shadows 1 shadowResolution 1024 minimumSunConfidence 0.75 }
material car/paint { surface car roughness .32 specular .22 reflection .12 clearcoat .24 clearcoatRoughness .15 diffuseMix .12 }
material road/dry { surface road roughness .84 specular .07 reflection .025 diffuseMix .16 }
material world { surface track roughness .9 specular .02 reflection 0 diffuseMix .12 }
track sunny { upload 0x123 upload 0x456 sunDirection .3 -.8 .4 sunIntensity .6 sunConfidence 1 }
track night { upload 0xabc upload 0xdef night 1 }
)";
WorldDrawList scene(){WorldDrawList l{};l.display_width=320;l.display_height=240;l.continuous_projection=true;l.track_commands=1;l.lighting_camera_rotation={4096,0,0,0,4096,0,0,0,4096};
    WorldMaterial m{};m.primitive_flags=world_primitive_track_overlay_support_flag;l.materials.push_back(m);
    WorldDrawCommand c{};c.object_kind=1;c.channel=WorldViewChannel::main_view;c.lighting_depth_scale=1024;c.material_index=0;
    Vec3 p[3]={{-100,100,100},{100,100,100},{0,100,500}};
    for(int i=0;i<3;++i){auto& v=c.vertices[i];v.view_x=p[i].x;v.view_y=p[i].y;v.view_z=p[i].z;v.clip_w=p[i].z*1024;v.model_x=static_cast<short>(p[i].x);v.model_y=static_cast<short>(p[i].y);v.model_z=static_cast<short>(p[i].z);v.source_vertex_identity=i+1;}
    l.commands.push_back(c);return l;
}
void parser(){Database d;std::string e;check(parse_script(sample,&d,&e),"sample shader parse");check(d.tracks.size()==2&&d.materials.size()==3,"block count");
    const auto count=d.tracks.size();
    for(auto text:{"settings { enabled 2 }","settings { shadows NaN }","settings { shadowResolution 1000 }","settings { enabled 1 enabled 0 }","settings { nonsense 1 }","track a { sunDirection 0 0 0 }","track a { upload -1 }","track a { upload 0x123junk }","track a { sunIntensity 1 }","material a { surface car bakedGain 2 2 2 }","material a { surface track track sunny primitiveKey 1 bakedGain 2 2 2 }","material a { surface track track sunny primitiveKey 1 sourceMeshKey 2 bakedGain 2 2 2 }","material a { surface track track sunny sourceMeshKey 2 sourcePrimitiveAddress 0x100000000 }","material a { surface track sourceMeshKey 2 sourcePrimitiveAddress 3 }","material a { surface track track sunny residentContentKey 4 }","material a { surface track track sunny primitiveKey 1 sourceMeshKey 2 sourcePrimitiveAddress 3 bakedGain 2 2 2 }","material a { surface car roughness inf }","/* unterminated","track a { upload 1 upload 1 }","settings { forceTrack missing }","track a {} track a {}","material a { surface invalid }"}){
        check(!parse_script(text,&d,&e),"bad script rejected");check(d.tracks.size()==count,"parse failure atomic");check(!e.empty(),"error explained");}
    Database exact;check(parse_script("track sunny {} material exact { surface track track sunny primitiveKey 1 sourceMeshKey 2 sourcePrimitiveAddress 3 residentContentKey 4 bakedGain 2 2 2 }",&exact,&e),"content-bound destructive rule parses");
    check(exact.materials.size()==1&&exact.materials[0].resident_content_key==4,"resident content key retained in material rule");
    check(!parse_script(std::string("settings { enabled 1 }\0x",25),&d,&e),"NUL rejected");
    check(select_track(d,{0x123})==nullptr,"one shared texture insufficient");check(select_track(d,{0x123,0x456})->name=="sunny","bank selection");
    d.tracks.push_back(d.tracks[0]);d.tracks.back().name="ambiguous";check(select_track(d,{0x123,0x456})==nullptr,"bank tie no guessing");d.tracks.pop_back();
    WorldMaterial source{};check(select_material(d,Surface::road,"sunny",1,source)->reflection<.03F,"dry road not wet");
}
void sun(){Vec3 direction=normalized({.4F,-.8F,.2F});std::vector<ShadowEvidence> samples;
    for(unsigned i=0;i<8;++i){Vec3 shadow{float(i*100),0,float(i*17)};samples.push_back({shadow+direction*50,shadow,1,i});}
    auto fit=fit_sun(samples);check(fit.accepted&&fit.independent_objects==8,"independent shadows fit");check(dot(fit.direction,direction)>.9999F,"correct sun sign");
    samples.push_back({{0,-50,100},{0,0,0},1,999});fit=fit_sun(samples);check(fit.accepted&&dot(fit.direction,direction)>.999,"outlier rejected");
    for(auto& p:samples)p.object=1;fit=fit_sun(samples);check(!fit.accepted,"one object cannot prove sun");
    samples={{{0,0,0},{0,0,0},1,1},{{0,50,0},{0,0,0},1,2}};check(!fit_sun(samples).accepted,"degenerate/below-ground rejected");
    samples.push_back({{std::numeric_limits<float>::quiet_NaN(),0,0},{},1,5});check(!fit_sun(samples).accepted,"NaN evidence rejected");
}
void frame(){Database db;std::string e;check(parse_script(sample,&db,&e),"parse");auto l=scene();auto f=prepare(l,db,&db.tracks[0]);
    check(f.enabled&&f.solar&&f.shadows,"prepared solar scene");check(f.casters.size()==3,"opaque geometry casts");
    check(std::abs(f.vertices[0].position.x+102400)<.01,"common depth scale applied");check(f.materials[0].surface[2]==.025F,"road role uses dry shader");
    check(f.constants.sun[1]<0,"source +Y down handled");
    WorldShadowCaster hidden{l.commands[0],l.materials[0]};hidden.command.vertices[0].view_z=-100;
    l.lighting_extra_casters.push_back(hidden);check(prepare(l,db,&db.tracks[0]).casters.size()==6,"eye-rejected geometry retained for sun");
    auto billboard=l;
    billboard.lighting_extra_casters.clear();
    billboard.materials[0].primitive_flags|=world_primitive_track_billboard_flag;
    auto billboard_frame=prepare(billboard,db,&db.tracks[0]);
    check(billboard_frame.casters.empty(),"camera-facing billboard quad is not directional-sun geometry");
    check(billboard_frame.enabled&&billboard_frame.solar&&billboard_frame.materials[0].surface[3]==0,"camera-facing billboard uses authored-color passthrough instead of a view-dependent lighting normal");
    check(billboard_frame.materials[0].gain[0]==1&&billboard_frame.materials[0].gain[1]==1&&billboard_frame.materials[0].gain[2]==1&&billboard_frame.materials[0].gain[3]==0,"billboard passthrough cannot acquire unbake/removal state");
    WorldShadowCaster hidden_billboard{l.commands[0],l.materials[0]};
    hidden_billboard.material.primitive_flags|=world_primitive_track_billboard_flag;
    auto mixed=scene();mixed.lighting_extra_casters.push_back(hidden_billboard);
    const auto mixed_frame=prepare(mixed,db,&db.tracks[0]);
    check(mixed_frame.casters.size()==3,"auxiliary billboard caster is suppressed without affecting stable track caster");
    auto legacy_billboard=scene();
    legacy_billboard.materials[0].primitive_flags|=world_primitive_track_billboard_depth_flag;
    const auto legacy_billboard_frame=prepare(legacy_billboard,db,&db.tracks[0]);
    check(legacy_billboard_frame.casters.empty(),"legacy authored-depth billboard subset remains excluded from directional-sun geometry");
    check(legacy_billboard_frame.materials[0].surface[3]==0,"legacy billboard subset also preserves authored color instead of using camera-facing lighting normal");
    auto rotated=mixed;
    auto& rotated_billboard=rotated.lighting_extra_casters.back().command;
    for(auto& v:rotated_billboard.vertices){
        const float x=v.view_x;
        v.view_x=v.view_z;
        v.view_z=-x;
    }
    const auto rotated_frame=prepare(rotated,db,&db.tracks[0]);
    check(rotated_frame.casters.size()==mixed_frame.casters.size(),"camera-facing billboard rotation changed stable caster count");
    bool same_casters=rotated_frame.casters.size()==mixed_frame.casters.size();
    for(std::size_t i=0;same_casters&&i<mixed_frame.casters.size();++i){
        const auto& a=mixed_frame.casters[i];const auto& b=rotated_frame.casters[i];
        for(int j=0;j<3;++j)same_casters=same_casters&&a.position[j]==b.position[j];
        for(int j=0;j<2;++j)same_casters=same_casters&&a.uv[j]==b.uv[j];
        same_casters=same_casters&&a.texture_page==b.texture_page&&a.clut==b.clut&&a.flags==b.flags&&
            a.mask_x==b.mask_x&&a.mask_y==b.mask_y&&a.offset_x==b.offset_x&&a.offset_y==b.offset_y;
    }
    check(same_casters,"camera-facing billboard rotation perturbed source-stable sun caster stream");
    // Same metric geometry with a different GTE normalization has identical light coordinates.
    auto other=l;other.commands[0].lighting_depth_scale=512;for(auto& v:other.commands[0].vertices){v.view_x*=2;v.view_y*=2;v.view_z*=2;}
    auto g=prepare(other,db,&db.tracks[0]);for(int i=0;i<3;++i)check(std::sqrt(dot(g.vertices[i].position-f.vertices[i].position,g.vertices[i].position-f.vertices[i].position))<.01F,"per-object normalization invariance");
    db.tracks[0].confidence=.2F;g=prepare(l,db,&db.tracks[0]);check(g.enabled&&!g.solar&&!g.shadows&&g.constants.sun[3]==0,"uncertain profile environment only");
    check(!prepare(l,db,&db.tracks[1]).enabled,"night retains original");check(!prepare(l,db,nullptr).enabled,"unknown course retains original");
    l.commands[0].channel=WorldViewChannel::secondary_view;g=prepare(l,db,&db.tracks[0]);check(g.materials[0].surface[3]==0,"mirror excluded from main camera light");
    l=scene();l.lighting_camera_rotation={};check(!prepare(l,db,&db.tracks[0]).enabled,"invalid camera stays original");
    l=scene();auto h=primitive_key(l.commands[0],l.materials[0]);l.commands[0].vertices[0].r=0;l.commands[0].object_id=123;l.commands[0].model_pointer=999;l.commands[0].source_mesh_key=77;l.commands[0].source_primitive_address=88;
    check(h==primitive_key(l.commands[0],l.materials[0]),"material primitive key remains independent of baked color and source pointers; destructive rules bind source separately");
}
void unbake_audit(){
    auto l=scene();auto& c=l.commands[0];auto& m=l.materials[0];
    m.primitive_flags=world_primitive_resident_course_flag|1U|world_primitive_track_overlay_support_flag;
    m.texture_page=12;m.clut=0x7e21;c.source_mesh_key=0x8010200000000042ULL;c.source_primitive_address=0x80123456U;c.resident_content_key=0x9a7b6c5d4e3f2101ULL;
    for(int i=0;i<3;++i){auto& v=c.vertices[i];v.r=96;v.g=88;v.b=72;v.u=32.0F+i*16.0F;v.v=64.0F+i*8.0F;
        v.world_x=float(v.model_x)*.01F;v.world_y=float(v.model_y)*.01F;v.world_z=float(v.model_z)*.01F;}
    const auto expected=primitive_key(c,m);auto audit=inspect_unbake_candidate(c,m,Surface::road,.99F);
    check(audit.eligible,"exact resident road enters non-destructive unbake audit");
    check(audit.primitive_key==expected&&audit.source_mesh_key==c.source_mesh_key&&audit.source_primitive_address==c.source_primitive_address&&audit.resident_content_key==c.resident_content_key,"audit carries exact destructive-rule/content provenance");
    check(audit.texture_page==12&&audit.clut==0x7e21&&audit.color[0]==96&&audit.color[1]==88&&audit.color[2]==72,"audit carries authored material/color evidence");
    check(audit.uv[0]==32&&audit.uv[1]==64&&audit.uv[4]==64&&audit.uv[5]==80,"audit carries all source UV corners");
    check(audit.world_scale>1&&std::isfinite(audit.world_centroid.x)&&std::isfinite(audit.world_normal.y),"audit carries finite reconstructed world-plane evidence");
    auto bad=c;bad.source_mesh_key=0;check(!inspect_unbake_candidate(bad,m,Surface::road,.99F).eligible,"capture without exact resident source identity is never an unbake candidate");
    bad=c;bad.resident_content_key=0;check(!inspect_unbake_candidate(bad,m,Surface::road,.99F).eligible,"capture without immutable resident content identity is never an unbake candidate");
    bad=c;bad.vertices[1].r++;check(!inspect_unbake_candidate(bad,m,Surface::road,.99F).eligible,"gouraud/tinted triangle is not confused with uniform baked shadow evidence");
    bad=c;bad.vertices[1].world_x=std::numeric_limits<float>::quiet_NaN();check(!inspect_unbake_candidate(bad,m,Surface::road,.99F).eligible,"invalid reconstructed world geometry is excluded from unbake audit");
    bad=c;for(auto& v:bad.vertices)v.world_x=bad.vertices[0].world_x, v.world_y=bad.vertices[0].world_y, v.world_z=bad.vertices[0].world_z;check(!inspect_unbake_candidate(bad,m,Surface::road,.99F).eligible,"degenerate reconstructed world geometry is excluded from unbake audit");
    check(!inspect_unbake_candidate(c,m,Surface::road,.95F).eligible,"non-horizontal surface is excluded from conservative audit");
    auto flags=m;flags.primitive_flags|=2U;check(!inspect_unbake_candidate(c,flags,Surface::road,.99F).eligible,"semi-transparent source is excluded from audit");
    flags=m;flags.primitive_flags|=world_primitive_track_billboard_flag;check(!inspect_unbake_candidate(c,flags,Surface::track,.99F).eligible,"camera-facing billboard is excluded from unbake audit");
    flags=m;flags.primitive_flags&=~world_primitive_resident_course_flag;check(!inspect_unbake_candidate(c,flags,Surface::road,.99F).eligible,"non-resident capture cannot author an exact unbake rule");
    check(!inspect_unbake_candidate(c,m,Surface::car,.99F).eligible,"vehicle material never enters course unbake audit");
    Database db;std::string error;check(parse_script(sample,&db,&error),"audit runtime fixture parse");
    const auto prepared=prepare(l,db,&db.tracks[0]);check(prepared.enabled,"audit runtime fixture traverses normal lighting prepare path");
    // Same exact destructive identity, different authored color: runtime audit
    // must preserve both observations so the analyzer can reject the identity.
    for(auto& v:l.commands[0].vertices){v.r=120;v.g=110;v.b=90;}
    check(prepare(l,db,&db.tracks[0]).enabled,"audit runtime fixture preserves alternate authored-color observation");
    // A separate exact identity is observed across a bounded frame span. With
    // OPENGT_LIGHTING_UNBAKE_AUDIT enabled this emits stage 0 at first sight,
    // stage 1 after three distinct frames spanning >=30, and stage 2 after
    // eight distinct frames spanning >=180. Rendering is unchanged either way.
    l.commands[0].source_primitive_address=0x80123556U;
    for(auto& v:l.commands[0].vertices){v.r=84;v.g=77;v.b=63;}
    const std::uint64_t audit_frames[]={0,15,30,60,90,120,150,180};
    for(auto frame:audit_frames){
        l.frame_index=frame;
        if(frame>=60)l.lighting_camera_rotation={0,0,4096,0,4096,0,-4096,0,0};
        check(prepare(l,db,&db.tracks[0]).enabled,"temporal/cross-view audit observation preserves normal prepare path");
    }
}

void overlay_rules(){
    Database db;std::string error;check(parse_script(sample,&db,&error),"overlay fixture parse");
    auto l=scene();WorldMaterial overlay{};
    overlay.primitive_flags=1U<<world_primitive_track_overlay_layer_shift;
    l.materials.push_back(overlay);auto c=l.commands[0];c.material_index=1;
    const Vec3 p[3]={{-25,100,200},{25,100,200},{0,100,300}};
    for(int j=0;j<3;++j){auto& v=c.vertices[j];v.view_x=p[j].x;v.view_y=p[j].y;v.view_z=p[j].z;
        v.model_x=static_cast<short>(p[j].x);v.model_y=static_cast<short>(p[j].y);v.model_z=static_cast<short>(p[j].z);}
    l.commands.push_back(c);l.track_commands=2;
    c.source_mesh_key=0x1122334455667788ULL;c.source_primitive_address=0x80123456U;c.resident_content_key=0x8877665544332211ULL;
    l.commands.back()=c;
    Material rule;rule.name="verified/test/overlay";rule.surface=Surface::track;rule.track="sunny";
    rule.primitive_key=primitive_key(c,overlay);rule.source_mesh_key=c.source_mesh_key;rule.source_primitive_address=c.source_primitive_address;rule.resident_content_key=c.resident_content_key;
    rule.baked_gain={1.5F,1.5F,1.5F};rule.remove_shadow_overlay=true;
    db.materials.push_back(rule);
    auto f=prepare(l,db,&db.tracks[0]);check(f.materials[1].gain[3]==1,"source-identified overlay with complete opaque support marked for removal");
    auto alias=l;alias.commands[1].source_primitive_address++;f=prepare(alias,db,&db.tracks[0]);
    check(f.materials[1].gain[0]==1&&f.materials[1].gain[3]==0,"duplicate geometry cannot inherit destructive unbake rule from another source primitive");
    alias=l;alias.commands[1].source_mesh_key++;f=prepare(alias,db,&db.tracks[0]);
    check(f.materials[1].gain[0]==1&&f.materials[1].gain[3]==0,"same primitive address in another resident mesh cannot inherit unbake rule");
    alias=l;alias.commands[1].resident_content_key++;f=prepare(alias,db,&db.tracks[0]);
    check(f.materials[1].gain[0]==1&&f.materials[1].gain[3]==0,"same source addresses with different immutable resident content cannot inherit unbake rule");
    check(f.casters.size()==3,"overlay cannot cast a second shadow");
    l.materials[0].primitive_flags=0;
    check(prepare(l,db,&db.tracks[0]).materials[1].gain[3]==0,"road cannot be deleted without complete supporting surface");
    l.materials[0].primitive_flags=world_primitive_track_overlay_support_flag;
    db.tracks[0].provisional_sun=true;db.tracks[0].confidence=0;
    f=prepare(l,db,&db.tracks[0]);
    check(f.solar&&f.shadows&&f.provisional_sun,"provisional light renders without measured confidence");
    check(f.materials[1].gain[0]==1&&f.materials[1].gain[3]==0,"provisional sunlight cannot authorize unbaking/removal");
    db.tracks[0].provisional_sun=false;
    db.tracks[0].confidence=0;f=prepare(l,db,&db.tracks[0]);
    check(f.materials[1].gain[0]==1&&f.materials[1].gain[3]==0,"uncalibrated sun leaves baked shading untouched");
    c.vertices[0].u=std::numeric_limits<float>::quiet_NaN();check(primitive_key(c,overlay)==0,"non-finite UV never matches exact unbake rule");
}
void brdf(){Constants c{};c.sky={.4F,.5F,.6F,1};c.ground={.15F,.15F,.15F,1};c.sun={0,-1,0,1};c.up={0,-1,0,.85F};c.color={1,.9F,.8F,.35F};
    DrawMaterial m;m.surface={.84F,.07F,.025F,1};m.coat={0,.16F,.16F,1};
    Vec3 base{.4F,.4F,.4F};auto lit=shade_reference(base,{0,-1,0},{0,-1,1},m,c,1),shade=shade_reference(base,{0,-1,0},{0,-1,1},m,c,0);
    check(lit.x>shade.x&&shade.x>.2F,"shadow blocks sunlight not ambient");
    m.surface[3]=0;auto legacy=shade_reference(base,{0,-1,0},{0,-1,1},m,c,0);check(legacy.x==base.x,"disabled is exact pass-through");
    m.surface[3]=1;for(int r=0;r<1000;++r){float rough=.06F+.94F*float(r)/999;m.surface[0]=rough;auto rgb=shade_reference(base,normalized({float(r%7),-1,1}),normalized({1,-1,float(r%11)}),m,c,float(r%100)/99);
        check(std::isfinite(rgb.x)&&std::isfinite(rgb.y)&&std::isfinite(rgb.z)&&rgb.x>=0&&rgb.x<=1,"BRDF finite bounded output");}
}
void extended_material_contracts(){
    Database db;std::string error;
    const std::string valid=R"(
settings { localReflections 1 reflectionResolution 128 maximumReflectionProbes 4 nearShadowCarSpans 6 }
material default { surface track roughness .9 reflection 0 clearcoat 0 }
material asphalt { surface track track seattle texturePage 12 clut 32289 uvRect 128 96 254 127 minimumUpNormal .9 roughness .84 reflection .025 clearcoat 0 }
track seattle { upload 1 upload 2 }
)";
    check(parse_script(valid,&db,&error),"L03 settings and source-bound asphalt parse");
    check(db.settings.local_reflections&&db.settings.reflection_resolution==128&&db.settings.maximum_reflection_probes==4&&db.settings.near_shadow_car_spans==6,"new settings retained");
    WorldMaterial source{};source.texture_page=12;source.clut=32289;
    auto command=scene().commands[0];for(auto& v:command.vertices){v.u=128;v.v=96;}
    auto select=[&](float n=1){return select_material(db,Surface::track,"seattle",0,source,&command,n);};
    check(select()->name=="asphalt","original page/CLUT/UV selects asphalt");
    check(select()->reflection<.03F&&select()->roughness>.8F&&select()->clearcoat==0,"asphalt stays dry");
    check(select(.8F)->name=="default","same UV on a wall is not asphalt");
    command.vertices[1].u=255;check(select()->name=="default","one corner outside UV rectangle prevents binding");
    command.vertices[1].u=254;command.vertices[2].v=127;check(select()->name=="asphalt","inclusive UV edges accepted");
    command.vertices[2].v=std::numeric_limits<float>::quiet_NaN();check(select()->name=="default","invalid UV cannot bind source material");command.vertices[2].v=96;
    source.clut++;check(select()->name=="default","different palette is not the verified asphalt");source.clut--;
    source.texture_page++;check(select()->name=="default","different page is not the verified asphalt");source.texture_page--;
    check(select_material(db,Surface::track,"other",0,source,&command,1)->name=="default","another track cannot inherit asphalt binding");
    check(select_material(db,Surface::car,"seattle",0,source,&command,1)==nullptr,"car cannot inherit track asphalt");
    check(select_material(db,Surface::track,"seattle",0,source,nullptr,1)->name=="default","no source UV means no source binding");
    for(auto bad:{"settings { reflectionResolution 0 }","settings { reflectionResolution 63 }","settings { reflectionResolution 1024 }","settings { maximumReflectionProbes 0 }","settings { maximumReflectionProbes 9 }","settings { nearShadowCarSpans 1 }","settings { nearShadowCarSpans nan }","settings { localReflections 2 }","material x { surface track uvRect 1 2 3 4 }","material x { surface track uvRect 9 8 7 6 }","material x { surface track uvRect 0 0 256 255 }","material x { surface track minimumUpNormal 1.1 }"}){
        check(!parse_script(bad,&db,&error),"invalid L03 directive rejected");check(db.materials.size()==2&&db.materials[1].name=="asphalt","L03 failure preserves prior database");
    }
}

void provisional_parser(){
    Database db;std::string error;
    check(parse_script("track a { provisionalSun 1 sunDirection .3 -.8 .4 sunIntensity .6 }",&db,&error),"explicit provisional sun parses without confidence claim");
    check(db.tracks[0].provisional_sun&&db.tracks[0].confidence==0,"provisional identity retained");
    check(!parse_script("track a { provisionalSun 1 sunConfidence 1 }",&db,&error),"provisional cannot masquerade as accepted fit");
    check(!parse_script("track a { provisionalSun 1 sunIntensity 1 }",&db,&error),"provisional still requires explicit direction");
}
void live_camera_axes(){
    Database db;std::string error;check(parse_script(sample,&db,&error),"live camera fixture parses");
    auto l=scene();
    // Actual Seattle grid camera captured by the full Linux game host.
    l.lighting_camera_rotation={0,-16384,0,-160,0,-14892,-16384,0,176};
    l.lighting_camera_primary_axes=true;
    auto f=prepare(l,db,&db.tracks[0]);
    check(f.enabled&&f.solar,"reflected authored primary camera enables lighting");
    check(f.constants.up[1]<-.99F&&std::abs(f.constants.up[0])<.01F,"source Z-up maps to screen up");
    // This is the equivalent canonical (inspection) matrix, not a guessed sun.
    auto canonical=l;canonical.lighting_camera_primary_axes=false;
    for(int i=0;i<3;++i){auto& m=canonical.lighting_camera_rotation;auto z=m[i*3+2];m[i*3+2]=m[i*3+1];m[i*3+1]=short(-z);}
    auto g=prepare(canonical,db,&db.tracks[0]);
    for(int i=0;i<3;++i){check(std::abs(f.constants.up[i]-g.constants.up[i])<1e-5F,"primary/canonical up agree");check(std::abs(f.constants.sun[i]-g.constants.sun[i])<1e-5F,"primary/canonical sun agree");}
    l.lighting_camera_rotation={4096,0,0,4096,0,0,0,0,4096};
    check(!prepare(l,db,&db.tracks[0]).enabled,"collinear reflected camera remains rejected");
}

}
int main(){try{parser();sun();frame();unbake_audit();overlay_rules();brdf();extended_material_contracts();live_camera_axes();provisional_parser();std::cout<<"Lighting L04: "<<checks<<" checks passed\n";return 0;}catch(const std::exception& e){std::cerr<<"FAIL: "<<e.what()<<" (after "<<checks<<" checks)\n";return 1;}}
