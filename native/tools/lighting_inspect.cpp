// Non-graphical diagnostic: runs the production lighting preparation on an
// actual capture. It does not claim to render or test Direct3D/HLSL.
#include "opengt/lighting.hpp"
#include "opengt/world_capture.hpp"
#include <algorithm>
#include <cmath>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <iterator>
#include <limits>
#include <map>
#include <stdexcept>
using namespace opengt::render;
int main(int argc,char** argv) {
    try {
        if(argc!=2 && argc!=4) {
            std::cerr<<"usage: opengt_lighting_inspect lighting.shader [capture.bin track-name]\n";
            return 2;
        }
        std::ifstream in(argv[1],std::ios::binary);
        if(!in)throw std::runtime_error("cannot read shader script");
        std::string text((std::istreambuf_iterator<char>(in)),{}),error;
        lighting::Database db;
        if(!lighting::parse_script(text,&db,&error))throw std::runtime_error(error);
        std::size_t solar_profiles=0,provisional_profiles=0,night_profiles=0,ambiguous_profiles=0;
        for(const auto& t:db.tracks) {
            if(t.night)++night_profiles;
            if(!t.uploads.empty()&&t.sun_intensity>0&&t.provisional_sun)++provisional_profiles;
            if(!t.uploads.empty()&&t.sun_intensity>0&&t.confidence>=db.settings.minimum_confidence)++solar_profiles;
            if(!t.uploads.empty()&&lighting::select_track(db,t.uploads)!=&t)++ambiguous_profiles;
        }
        std::cout<<"{\n  \"materials\":"<<db.materials.size()<<",\n  \"profiles\":"<<db.tracks.size()
            <<",\n  \"solarProfilesWithBankIdentity\":"<<solar_profiles
            <<",\n  \"provisionalProfilesWithBankIdentity\":"<<provisional_profiles
            <<",\n  \"nightProfiles\":"<<night_profiles
            <<",\n  \"profilesNotUniquelySelfSelected\":"<<ambiguous_profiles;
        if(argc==4) {
            const lighting::Track* track=nullptr;
            for(const auto& t:db.tracks)if(t.name==argv[3])track=&t;
            if(!track)throw std::runtime_error("unknown explicit diagnostic track name");
            WorldCaptureHeader h{};
            auto r=read_world_capture_header(argv[2],&h);
            if(r!=WorldCaptureReadResult::success)throw std::runtime_error(world_capture_read_result_name(r));
            if(h.triangle_count>2000000)throw std::runtime_error("capture exceeds diagnostic triangle limit");
            std::vector<WorldCaptureTriangle> triangles(h.triangle_count);
            std::vector<std::uint16_t> vram(1024*512);
            r=load_world_capture(argv[2],&h,triangles.data(),triangles.size(),vram.data(),vram.size());
            if(r!=WorldCaptureReadResult::success)throw std::runtime_error(world_capture_read_result_name(r));
            WorldDrawList list{};
            auto result=build_world_draw_list(h,triangles.data(),triangles.size(),WorldDrawListOptions{true,true,true},&list);
            if(result!=WorldDrawListResult::success)throw std::runtime_error(world_draw_list_result_name(result));
            auto f=lighting::prepare(list,db,track);
            std::size_t lit=0,car=0,road=0,missing_depth=0;
            std::map<unsigned,std::size_t> channels;
            for(std::size_t i=0;i<list.commands.size();++i) {
                const auto& c=list.commands[i];++channels[static_cast<unsigned>(c.channel)];
                if(c.lighting_depth_scale<=0)++missing_depth;
                if(f.enabled&&f.materials[i].surface[3]>0) {
                    ++lit;if(c.object_kind==2)++car;
                    if(list.materials[c.material_index].primitive_flags&world_primitive_track_overlay_support_flag)++road;
                }
            }
            bool finite=true;float maximum=0;
            for(const auto& v:f.vertices)for(float p:{v.position.x,v.position.y,v.position.z,v.normal.x,v.normal.y,v.normal.z}) {
                finite=finite&&std::isfinite(p);maximum=std::max(maximum,std::abs(p));
            }
            std::cout<<",\n  \"captureCommands\":"<<list.commands.size()
                <<",\n  \"continuousProjection\":"<<(list.continuous_projection?"true":"false")
                <<",\n  \"explicitDiagnosticSelection\":true,\n  \"track\":\""<<track->name<<"\""
                <<",\n  \"enabled\":"<<(f.enabled?"true":"false")<<",\n  \"solar\":"<<(f.solar?"true":"false")
                <<",\n  \"shadows\":"<<(f.shadows?"true":"false")
                <<",\n  \"litCommands\":"<<lit<<",\n  \"litCarCommands\":"<<car<<",\n  \"roadSupportCommands\":"<<road
                <<",\n  \"commandsWithoutDepthScale\":"<<missing_depth
                <<",\n  \"shadowTriangles\":"<<f.casters.size()/3
                <<",\n  \"allLightingValuesFinite\":"<<(finite?"true":"false")
                <<",\n  \"maximumAbsoluteLightingValue\":"<<std::setprecision(9)<<maximum
                <<",\n  \"nearShadowRange\":"<<f.constants.shadow_config[0]
                <<",\n  \"farShadowRange\":"<<f.constants.shadow_config[1]
                <<",\n  \"gpuRenderingTested\":false";
            if(!finite)throw std::runtime_error("non-finite prepared frame");
        }
        std::cout<<"\n}\n";return 0;
    } catch(const std::exception& e) {std::cerr<<"lighting inspect: "<<e.what()<<"\n";return 1;}
}
