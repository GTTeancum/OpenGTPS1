#include "opengt/world_capture.hpp"
#include "opengt/world_draw_list.hpp"
#include "opengt/shadow_grid.hpp"
#include <array>
#include <cstdint>
#include <cstring>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <vector>
using namespace opengt::render;
namespace {
unsigned checks=0;void check(bool v,const char* why){++checks;if(!v)throw std::runtime_error(why);}
void put(std::vector<std::uint8_t>& b,unsigned o,std::uint64_t n,unsigned bytes=4){for(unsigned i=0;i<bytes;++i)b[o+i]=std::uint8_t(n>>(8*i));}
std::vector<std::uint8_t> fixture(){std::vector<std::uint8_t>b(160+1024*512*2);std::memcpy(b.data(),"OGTWCAP",7);put(b,8,6);put(b,12,160);put(b,16,77,8);put(b,24,999);put(b,36,320);put(b,40,240);put(b,48,384);put(b,52,1024);put(b,56,512);put(b,60,160,8);put(b,68,160,8);put(b,76,1024*512*2,8);put(b,84,28);put(b,88,27,8);
 std::int16_t r[]={4096,0,0,0,0,-4096,0,4096,0};for(unsigned j=0;j<9;++j)put(b,96+j*2,std::uint16_t(r[j]),2);
 put(b,114,10,2);put(b,116,123);put(b,120,456);put(b,124,789);put(b,128,160<<16);put(b,132,120<<16);put(b,136,256);put(b,148,std::uint32_t(-1234*1024));put(b,152,2345*1024);put(b,156,std::uint32_t(-6789*1024));return b;}
WorldDrawList decode(const std::vector<std::uint8_t>&b,WorldCaptureHeader&h){WorldCaptureTriangle triangle{};std::vector<std::uint16_t> vram(1024*512);check(load_world_capture_memory(b.data(),b.size(),&h,&triangle,0,vram.data(),vram.size())==WorldCaptureReadResult::success,"v6 capture parse failed");WorldDrawList list;WorldDrawListOptions options{};options.continuous_projection=true;check(build_world_draw_list(h,&triangle,0,options,&list)==WorldDrawListResult::success,"v6 header to draw-list failed");return list;}
}
int main(int argc,char**argv){try{auto b=fixture();WorldCaptureHeader h{};auto list=decode(b,h);check(h.camera_depth_scale_exponent==10,"wire depth exponent width/offset incorrect");check(h.camera_translation[0]==123&&h.camera_translation[2]==789,"world extension overwrote sector translation");check(list.lighting_world_anchor_valid&&list.lighting_camera_world_offset[0]==-1234*1024&&list.lighting_camera_world_offset[1]==2345*1024&&list.lighting_camera_world_offset[2]==-6789*1024,"world source offset not forwarded exactly");check(lighting::shadow_camera_transform(list).valid,"decoded valid primary anchor rejected");
 // Same v6 record without opt-in: arbitrary historical reserved bytes must
 // never be interpreted as a position, even after a previously valid frame.
 auto old=b;put(old,84,12);auto oldList=decode(old,h);check(!oldList.lighting_world_anchor_valid&&h.camera_depth_scale_exponent==0,"legacy reserved bytes enabled anchor");check(h.camera_world_offset[0]==0&&h.camera_world_offset[1]==0&&h.camera_world_offset[2]==0,"legacy read reused prior camera data");
 auto v5=b;put(v5,8,5);put(v5,48,256);auto legacy=decode(v5,h);check(!legacy.lighting_world_anchor_valid&&h.camera_depth_scale_exponent==0,"v5 reserved header treated as new source data");
 for(int i=0;i<4;++i){auto bad=b;if(i==0)put(bad,114,7,2);if(i==1)put(bad,114,11,2);if(i==2)put(bad,148,1);if(i==3)put(bad,84,20);auto l=decode(bad,h);check(!lighting::shadow_camera_transform(l).valid,"bad optional extension did not safely disable grid");}
 for(unsigned n:{0U,15U,127U,159U}){WorldCaptureTriangle t{};std::vector<std::uint16_t> vram(1024*512);check(load_world_capture_memory(b.data(),n,&h,&t,0,vram.data(),vram.size())!=WorldCaptureReadResult::success,"truncated extended header accepted");}
 if(argc>1){std::ifstream input(argv[1],std::ios::binary);check(bool(input),"managed header fixture missing");auto managed=fixture();input.read(reinterpret_cast<char*>(managed.data()),160);check(input.gcount()==160,"managed header fixture truncated");auto l=decode(managed,h);check(l.lighting_world_anchor_valid&&h.camera_depth_scale_exponent==10,"actual C# wire did not reach native world anchor");}
 std::cout<<"L08 source camera capture: "<<checks<<" checks passed\n";return 0;}catch(const std::exception&e){std::cerr<<"L08 camera capture failed after "<<checks<<": "<<e.what()<<"\n";return 1;}}
