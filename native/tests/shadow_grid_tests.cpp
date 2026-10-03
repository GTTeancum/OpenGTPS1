// L08: world-anchored texels from the actual non-orthogonal primary camera.
#include "opengt/shadow_grid.hpp"
#include "opengt/world_gpu_renderer_native.hpp"
#include <algorithm>
#include <array>
#include <cmath>
#include <chrono>
#include <thread>
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
unsigned checks=0;
void check(bool b,const char* reason){++checks;if(!b)throw std::runtime_error(reason);}
float norm(Vec3 x){return std::sqrt(dot(x,x));}
struct Camera {
    // Independently construct a canonical source-world -> view transform.
    // Quantize as the original primary R registers do, not in lighting code.
    std::array<std::int16_t,9> raw{};Vec3 origin{};int exponent=10;
    explicit Camera(float yaw=0,float shear=0,float scale=.91F,Vec3 position={}) :origin(position){
        float co=std::cos(yaw),si=std::sin(yaw);
        float m[3][3]={{co,0,si},{shear*co,scale,shear*si},{-si,0,co}};
        for(int row=0;row<3;++row){raw[row*3]=short(std::lround(m[row][0]*4096));raw[row*3+1]=short(std::lround(m[row][2]*4096));raw[row*3+2]=short(std::lround(-m[row][1]*4096));}
    }
    Vec3 vector(Vec3 p) const {auto row=[&](int r){return (float(raw[r*3])*p.x-float(raw[r*3+2])*p.y+float(raw[r*3+1])*p.z)*std::ldexp(1.0F,exponent)/(4096*1024);};return {row(0),row(1),row(2)};}
    Vec3 position(Vec3 p) const {return vector(p-origin);}
    void header(WorldDrawList& l) const {
        l.lighting_camera_primary_axes=true;l.lighting_world_anchor_valid=true;
        l.lighting_camera_rotation=raw;l.lighting_camera_depth_exponent=std::int16_t(exponent);
        l.lighting_camera_world_offset={-int(origin.x),-int(origin.z),int(origin.y)};
    }
};
WorldDrawCommand triangle(Vec3 a,Vec3 b,Vec3 c,const Camera& camera){
    WorldDrawCommand q{};q.object_kind=1;q.object_id=7;q.model_pointer=0x80001000;
    q.channel=WorldViewChannel::main_view;q.lighting_depth_scale=1024;q.clip_x1=q.clip_y1=127;
    Vec3 pts[]={a,b,c};for(int j=0;j<3;++j){Vec3 p=camera.position(pts[j]);auto& v=q.vertices[j];
        v.view_x=p.x/1024;v.view_y=p.y/1024;v.view_z=p.z/1024;
        v.clip_x=p.x;v.clip_y=-p.y;v.clip_z=16*1024;v.clip_w=p.z;v.r=v.g=v.b=140;}
    return q;
}
WorldDrawList scene(const Camera& camera,Vec3 offset={},bool caster=true){
    WorldDrawList l{};l.display_width=l.display_height=128;l.continuous_projection=true;camera.header(l);l.materials.push_back(WorldMaterial{});l.frame_index=1;
    auto p=[&](float x,float y,float z){return offset+Vec3{x*1024,y*1024,z*1024};};
    l.commands.push_back(triangle(p(-2000,100,100),p(2000,100,3000),p(2000,100,100),camera));
    l.commands.push_back(triangle(p(-2000,100,100),p(-2000,100,3000),p(2000,100,3000),camera));l.track_commands=2;
    Vec3 local[]={{-20,-10,-60},{20,-10,-60},{20,10,-60},{-20,10,-60},{-20,-10,60},{20,-10,60},{20,10,60},{-20,10,60}};
    int faces[][3]={{0,1,2},{0,2,3},{4,6,5},{4,7,6},{0,4,5},{0,5,1},{3,2,6},{3,6,7},{1,5,6},{1,6,2},{0,3,7},{0,7,4}};
    for(unsigned f=0;f<12;++f){Vec3 points[3];for(int j=0;j<3;++j)points[j]=local[faces[f][j]]*1024+p(0,75,600);
        auto c=triangle(points[0],points[1],points[2],camera);c.object_kind=2;c.object_id=1;c.transform_id=9;c.exact_transform_valid=true;
        for(int r=0;r<3;++r){c.transform_rotation[r*3]=camera.raw[r*3];c.transform_rotation[r*3+1]=std::int16_t(-camera.raw[r*3+2]);c.transform_rotation[r*3+2]=camera.raw[r*3+1];}
        for(int j=0;j<3;++j){Vec3 v=local[faces[f][j]];c.vertices[j].model_x=short(v.x);c.vertices[j].model_y=short(v.y);c.vertices[j].model_z=short(v.z);}
        if(f<2)l.commands.push_back(c);else{auto m=l.materials[0];m.primitive_flags|=world_primitive_shadow_only_flag;l.lighting_extra_casters.push_back({c,m});}
    }l.vehicle_commands=2;
    if(caster){l.lighting_extra_casters.push_back({triangle(p(60,-30,550),p(260,-30,850),p(260,-30,550),camera),l.materials[0]});l.lighting_extra_casters.push_back({triangle(p(60,-30,550),p(60,-30,850),p(260,-30,850),camera),l.materials[0]});}
    return l;
}
std::string script="settings { enabled 1 shadows 1 localReflections 0 shadowResolution 512 shadowBias .00015 nearShadowCarSpans 6 forceTrack test } material world { surface track roughness 1 specular 0 reflection 0 clearcoat 0 diffuseMix 0 } track test { sunDirection .4 -.8 .3 sunIntensity 1 provisionalSun 1 shadowStrength .8 contactShadowOpacity .48 }";
Database database(){Database db;std::string error;check(parse_script(script,&db,&error),"parse grid fixture");return db;}
double texel(const Frame& f,int cascade,int axis,Vec3 p){auto r=f.constants.shadow_rows[cascade*4+axis];return (double(r[0])*p.x+double(r[1])*p.y+double(r[2])*p.z+r[3]+1)*256;}
double phase_error(double a,double b){double d=a-b;return std::abs(d-std::round(d));}
void matrix_and_phase(){auto db=database();ShadowGridState state;double maximum=0,legacy=0;
    Vec3 landmark{170*1024,100*1024,850*1024};auto firstCamera=Camera(0,.08F,.91F);auto initial=scene(firstCamera);
    auto base=prepare(initial,db,&db.tracks[0],{},&state);check(base.world_grid,"valid primary source did not anchor atlas");
    auto noAnchor=initial;noAnchor.lighting_world_anchor_valid=false;auto oldBase=prepare(noAnchor,db,&db.tracks[0]);
    for(int sample=0;sample<180;++sample){
        Camera camera((sample-90)*.003F, .08F+float(sample%7)*.005F,.86F+float(sample%11)*.01F,
            {float((sample%19-9)*1024),float((sample%3-1)*1024),float((sample%29-14)*1024)});
        auto l=scene(camera);l.frame_index=sample+2;auto transform=shadow_camera_transform(l);check(transform.valid,"realistic source matrix rejected");
        for(Vec3 v: {Vec3{3,-7,5},Vec3{123400,456700,-832000}}){auto q=transform.vector_to_world(camera.vector(v));check(norm(q-v)<std::max(.0001F,norm(v)*1e-6F),"source matrix inverse loses camera shear/scale");}
        auto f=prepare(l,db,&db.tracks[0],{},&state);check(f.world_grid,"world anchor lost during source-camera motion");
        check(f.constants.shadow_config==base.constants.shadow_config,"source scale/camera changed locked atlas extent");
        for(int c=0;c<2;++c)for(int a=0;a<2;++a){double e=phase_error(texel(f,c,a,camera.position(landmark)),texel(base,c,a,firstCamera.position(landmark)));maximum=std::max(maximum,e);check(e<.0002,"stationary world point drifts across shadow texels");}
        auto old=l;old.lighting_world_anchor_valid=false;auto oldFrame=prepare(old,db,&db.tracks[0]);legacy=std::max(legacy,phase_error(texel(oldFrame,0,0,camera.position(landmark)),texel(oldBase,0,0,firstCamera.position(landmark))));
        check(f.constants.color==base.constants.color&&f.contact_shadow_opacity==base.contact_shadow_opacity,"grid changed accepted lighting strength");
    }
    check(legacy>.15,"fixture fails to reproduce old camera-relative texel drift");
    // Huge world translation, small camera-relative geometry: cancellation must
    // happen in double on CPU, not after uploading floats to the GPU.
    for(int sign:{-1,1}){
        Vec3 offset{float(sign*800000*1024),float(sign*100000*1024),float(sign*500000*1024)};
        Camera camera(0,.08F,.91F,offset);auto l=scene(camera,offset);auto f=prepare(l,db,&db.tracks[0]);
        check(f.world_grid,"large world source rejected");
        for(int a=0;a<2;++a){auto t=shadow_camera_transform(l);Vec3 sun=normalized(db.tracks[0].sun_direction);Vec3 u=normalized(cross(Vec3{0,1,0},sun));Vec3 axis=a?normalized(cross(sun,u)):u;
            double worldPoint=double(axis.x)*(landmark.x+double(offset.x))+double(axis.y)*(landmark.y+double(offset.y))+double(axis.z)*(landmark.z+double(offset.z));
            double phase=worldPoint/(2.0*f.constants.shadow_config[0]/512)+256;
            check(phase_error(texel(f,0,a,firstCamera.position(landmark)),phase)<.0002,"large absolute origin loses world grid phase");}
    }
    std::cout<<"Fixed world texel phase max="<<maximum<<" px; unanchored baseline drift="<<legacy<<" px\n";
}
void invalid_and_lifecycle(){auto db=database();auto l=scene(Camera());ShadowGridState a,b;auto f=prepare(l,db,&db.tracks[0],{},&a);check(a.source_span>0,"complete body did not initialize grid");
    float span=a.source_span;l.frame_index=2;for(auto& c:l.commands)if(c.object_kind==2)for(auto&v:c.vertices)v.model_z*=2;for(auto& c:l.lighting_extra_casters)if(c.command.object_kind==2)for(auto&v:c.command.vertices)v.model_z*=2;
    auto locked=prepare(l,db,&db.tracks[0],{},&a);auto independent=prepare(l,db,&db.tracks[0],{},&b);
    check(a.source_span==span&&b.source_span==span*2,"renderer grid state shared or follows changed nearest body");
    check(locked.constants.shadow_config[0]*2==independent.constants.shadow_config[0],"independent grid scale not used");
    l.frame_index=1;prepare(l,db,&db.tracks[0],{},&a);check(a.source_span==span*2,"rewound frame stream kept old grid extent");
    auto copy=db.tracks[0];copy.name="other";l.frame_index=3;prepare(l,db,&copy,{},&a);check(a.track=="other","different track retained prior grid identity");
    for(int test=0;test<6;++test){auto bad=l;if(test==0)bad.lighting_world_anchor_valid=false;if(test==1)bad.lighting_camera_primary_axes=false;if(test==2)bad.lighting_camera_depth_exponent=7;if(test==3)bad.lighting_camera_depth_exponent=11;if(test==4)bad.lighting_camera_world_offset[0]=1;if(test==5)bad.lighting_camera_rotation.fill(0);
        check(!shadow_camera_transform(bad).valid,"invalid anchor/matrix accepted");auto fallback=prepare(bad,db,&db.tracks[0],{},&a);check(!fallback.world_grid&&a.source_span==0,"invalid camera reused stale world anchor");}
    prepare(l,db,&db.tracks[0],{},&a);db.settings.enabled=false;prepare(l,db,&db.tracks[0],{},&a);check(a.source_span==0,"disabled lighting retained grid");
    check(!f.casters.empty(),"fixture lacks shadow geometry");
}
void gpu_shear(const std::filesystem::path& path){std::ofstream(path)<<script;setenv("OPENGT_LIGHTING_SCRIPT",path.c_str(),1);setenv("OPENGT_SHADOW_DIAGNOSTIC","1",1);
    std::vector<std::uint16_t> vram(1024*512);std::vector<std::uint8_t> pixels(128*128*4);WorldGpuRenderOptions opts{true,true,false,true,false,false,1,0xFF000000U};
    auto render=[&](const WorldDrawList& l){WorldGpuRenderStats stats{};check(render_world_native(l,vram.data(),vram.size(),pixels.data(),pixels.size(),opts,&stats)==WorldGpuRenderResult::success&&stats.output_valid,"GPU grid shader failed");};
    for(float shear:{-.35F,0.0F,.35F}){Camera cam(0,shear,.75F);auto l=scene(cam,{},false);
        // Move car out of the image while preserving its complete source size.
        for(auto& c:l.commands)if(c.object_kind==2)for(auto& v:c.vertices){v.view_x+=50000;v.clip_x+=50000*1024;}
        for(auto& c:l.lighting_extra_casters)if(c.command.object_kind==2)for(auto& v:c.command.vertices){v.view_x+=50000;v.clip_x+=50000*1024;}
        render(l);unsigned falselyShadowed=0;for(std::size_t i=0;i<pixels.size();i+=4)if(pixels[i]>5)++falselyShadowed;
        check(falselyShadowed==0,"sheared primary camera makes an isolated road shadow itself");
        auto with=scene(cam,{},true);with.commands=l.commands;for(std::size_t i=0;i<10;++i)with.lighting_extra_casters[i]=l.lighting_extra_casters[i];render(with);unsigned shaded=0;for(std::size_t i=0;i<pixels.size();i+=4)if(pixels[i]>16)++shaded;
        check(shaded>10,"sheared camera lost genuine projected scenery shadow");std::cout<<"GPU shear="<<shear<<" false="<<falselyShadowed<<" real="<<shaded<<" pixels\n";
    }unsetenv("OPENGT_SHADOW_DIAGNOSTIC");release_world_native_context(nullptr);
}

void gpu_cascade_edge(const std::filesystem::path& path){
    auto edgeScript=script;auto pos=edgeScript.find("sunDirection .4 -.8 .3");edgeScript.replace(pos,std::string("sunDirection .4 -.8 .3").size(),"sunDirection .447 -.3 .894");
    auto file=path.string()+".edge";std::ofstream(file)<<edgeScript;setenv("OPENGT_LIGHTING_SCRIPT",file.c_str(),1);setenv("OPENGT_SHADOW_DIAGNOSTIC","1",1);
    Database db;std::string error;check(parse_script(edgeScript,&db,&error),"cascade edge script");
    // Runtime config checks are deliberately throttled. Confirm the new
    // fixture profile actually became active before judging its atlas edges.
    bool loaded=false;
    for(int attempt=0;attempt<80;++attempt){auto live=runtime_database();
        if(live&&!live->tracks.empty()&&norm(live->tracks[0].sun_direction-db.tracks[0].sun_direction)<1e-6F){loaded=true;break;}
        std::this_thread::sleep_for(std::chrono::milliseconds(50));}
    check(loaded,"cascade fixture profile did not reload");
    Camera camera(0,0,1);auto l=scene(camera,{},false);
    const float range=120*1024*6;Vec3 a{range*.82F,range*.04F,range*.005F},b{range*1.01F,range*.04F,range*.005F},c{range*.82F,range*.04F,range*.08F},d{range*1.01F,range*.04F,range*.08F};
    l.commands[0]=triangle(a,d,b,camera);l.commands[1]=triangle(a,c,d,camera);
    // Orthographic inspection of a square-atlas corner, independently of the
    // perspective eye bounds. The lighting coordinates remain actual geometry.
    for(unsigned i=0;i<2;++i)for(auto&v:l.commands[i].vertices){v.clip_x=(v.view_x*1024-a.x)/(b.x-a.x)*2-1;v.clip_y=(v.view_z*1024-a.z)/(c.z-a.z)*2-1;v.clip_w=1;v.clip_z=.5;}
    for(auto&cmd:l.commands)if(cmd.object_kind==2)for(auto&v:cmd.vertices){v.view_x+=50000;v.clip_x+=50000*1024;}
    for(auto&cmd:l.lighting_extra_casters)if(cmd.command.object_kind==2)for(auto&v:cmd.command.vertices){v.view_x+=50000;v.clip_x+=50000*1024;}
    Vec3 shift=normalized(db.tracks[0].sun_direction)*(range*.3F);
    // Oversize the occluder to rule out filter/geometry penumbra in this check.
    Vec3 aa{a.x-range*.3F,a.y,a.z-range*.3F},dd{d.x+range*.3F,d.y,d.z+range*.3F};Vec3 bb{dd.x,aa.y,aa.z},cc{aa.x,dd.y,dd.z};
    l.lighting_extra_casters.push_back({triangle(aa+shift,dd+shift,bb+shift,camera),l.materials[0]});l.lighting_extra_casters.push_back({triangle(aa+shift,cc+shift,dd+shift,camera),l.materials[0]});
    auto f=prepare(l,db,&db.tracks[0]);unsigned nearMissing=0;
    for(int y=8;y<120;++y)for(int x=8;x<120;++x){Vec3 p{a.x+(b.x-a.x)*(x+.5F)/128,a.y,a.z+(c.z-a.z)*(y+.5F)/128};auto row=f.constants.shadow_rows[0];float q=row[0]*p.x+row[1]*p.y+row[2]*p.z+row[3];if(q>1)++nearMissing;}
    check(nearMissing>500,"fixture did not cross the near square coverage boundary");
    std::vector<std::uint16_t> vram(1024*512);std::vector<std::uint8_t> pixels(128*128*4);WorldGpuRenderStats stats{};WorldGpuRenderOptions opts{true,true,false,true,false,false,1,0xFF000000U};
    check(render_world_native(l,vram.data(),vram.size(),pixels.data(),pixels.size(),opts,&stats)==WorldGpuRenderResult::success&&stats.output_valid,"cascade edge rendering failed");
    unsigned minimum=255,below=0;for(int y=8;y<120;++y)for(int x=8;x<120;++x){auto value=pixels[(y*128+x)*4];minimum=std::min(minimum,unsigned(value));if(value<250)++below;}
    std::cout<<"Cascade edge: near-uncovered="<<nearMissing<<" tested="<<112*112<<" minimum shadow="<<minimum<<" false bright="<<below<<"\n";
    check(below==0,"near-map exit creates a bright hole despite complete far shadow coverage");
    unsetenv("OPENGT_SHADOW_DIAGNOSTIC");std::filesystem::remove(file);release_world_native_context(nullptr);
}
}
int main(){auto path=std::filesystem::temp_directory_path()/("opengt-shadow-grid-"+std::to_string(getpid())+".shader");try{matrix_and_phase();invalid_and_lifecycle();gpu_shear(path);gpu_cascade_edge(path);std::filesystem::remove(path);std::cout<<"L08 shadow grid: "<<checks<<" checks passed\n";return 0;}catch(const std::exception&e){std::filesystem::remove(path);std::cerr<<"L08 shadow grid failed after "<<checks<<": "<<e.what()<<"\n";return 1;}}
