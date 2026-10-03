// Actual shared-library C ABI tests. Synthetic source commands are explicit;
// this is a renderer integration fixture, not a booted game.
#include "opengt/live_renderer_bridge.h"
#include <algorithm>
#include <array>
#include <cstdint>
#include <cstring>
#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>
#include <unistd.h>
namespace {
unsigned checks{};
void check(bool ok,const char* message){++checks;if(!ok)throw std::runtime_error(message);}
using Bytes=std::vector<std::uint8_t>;
void integer(Bytes& b,std::size_t offset,std::uint64_t value,unsigned width=4){
    if(offset+width>b.size())throw std::runtime_error("fixture overflow");
    for(unsigned i=0;i<width;i++)b[offset+i]=std::uint8_t(value>>(i*8));
}
void real(Bytes& b,std::size_t offset,float value){std::uint32_t bits{};std::memcpy(&bits,&value,4);integer(b,offset,bits);}
void identity(Bytes& b,std::size_t offset){for(unsigned i=0;i<9;i++)integer(b,offset+i*2,i%4==0?4096:0,2);}
Bytes capture(std::uint64_t frame,int poll,std::array<unsigned char,3> color,unsigned size=96){
    constexpr unsigned stride=384,count=2,vram=160+count*stride;
    Bytes b(vram+1048576);std::memcpy(b.data(),"OGTWCAP",7);
    integer(b,8,6);integer(b,12,160);integer(b,16,frame,8);integer(b,24,poll);
    integer(b,36,size);integer(b,40,size);integer(b,44,count);integer(b,48,stride);
    integer(b,52,1024);integer(b,56,512);integer(b,60,160,8);integer(b,68,vram,8);integer(b,76,1048576,8);integer(b,84,4);
    integer(b,88,1,8);identity(b,96);integer(b,128,(size/2)<<16);integer(b,132,(size/2)<<16);integer(b,136,size/2);
    const int positions[4][3]={{-190,-190,500},{190,-190,500},{190,190,500},{-190,190,500}};
    const unsigned corners[2][3]={{0,2,1},{0,3,2}};
    for(unsigned tri=0;tri<2;tri++){
        auto t=160+tri*stride;integer(b,t+8,tri);integer(b,t+16,size-1,2);integer(b,t+18,size-1,2);
        integer(b,t+32,1);integer(b,t+36,7);integer(b,t+40,0x80001000);integer(b,t+48,99,8);identity(b,t+56);
        for(unsigned j=0;j<3;j++){
            auto p=positions[corners[tri][j]];auto v=t+88+j*96;
            real(b,v,float(size)*.5F+float(p[0])*float(size)*.5F/p[2]);real(b,v+4,float(size)*.5F+float(p[1])*float(size)*.5F/p[2]);real(b,v+8,float(p[2]));
            for(unsigned k=0;k<3;k++){b[v+16+k]=color[k];integer(b,v+20+2*k,p[k],2);integer(b,v+28+4*k,p[k]);}
            b[v+19]=1;integer(b,v+40,(size/2)<<16);integer(b,v+44,(size/2)<<16);integer(b,v+48,size/2);integer(b,v+52,corners[tri][j]+1);integer(b,v+56,99,8);identity(b,v+64);
        }
        integer(b,t+376,0);integer(b,t+380,1);
    }
    return b;
}
std::uint64_t hash(const Bytes& b){std::uint64_t h=14695981039346656037ULL;for(auto v:b){h^=v;h*=1099511628211ULL;}return h;}
struct Handle {
    void* p=opengt_live_create();
    Handle(){check(p!=nullptr,"C ABI create failed");}
    ~Handle(){opengt_live_destroy(p);}
    Handle(const Handle&)=delete;
};
opengt_live_options options(bool realtime=false){return {sizeof(opengt_live_options),OPENGT_LIVE_DEPTH|OPENGT_LIVE_TOPOLOGY|OPENGT_LIVE_PERSPECTIVE|OPENGT_LIVE_WARP|(realtime?OPENGT_LIVE_REALTIME_READBACK:0U),1,0xff050505,0,0,0};}
Bytes render(Handle& h,const Bytes& cap,const opengt_live_options& opt,unsigned expected_width,unsigned expected_height,std::uint64_t frame,int poll){
    Bytes pixels(expected_width*expected_height*4);opengt_live_stats s{};s.struct_size=sizeof(s);
    auto result=opengt_live_render(h.p,cap.data(),cap.size(),pixels.data(),pixels.size(),&opt,&s);
    check(result==0&&s.result==0,"C ABI render failed");check(!(s.reserved&OPENGT_LIVE_STATS_NO_OUTPUT),"Linux completed frame unexpectedly missing");
    check(s.frame_index==frame&&s.input_poll==poll,"pixels/frame/poll identity drifted");
    check(s.capture_triangles==2&&s.output_commands>=2,"source geometry absent from bridge output");
    check(s.output_width==expected_width&&s.output_height==expected_height,"frame dimensions drifted");
    check(s.output_texture==0,"Linux returned a Windows texture handle");check(s.output_fingerprint==hash(pixels),"output hash does not match returned pixels");
    check(s.world_fingerprint!=0,"world command identity absent");return pixels;
}
}
int main(){
    const auto path=std::filesystem::temp_directory_path()/("opengt-L03-live-"+std::to_string(getpid())+".shader");
    try{
        static_assert(sizeof(opengt_live_options)==28);static_assert(sizeof(opengt_live_stats)==136);static_assert(sizeof(opengt_live_texture_upload)==24);
        std::ofstream(path)<<"settings { enabled 1 shadows 0 localReflections 0 }\nmaterial track { surface track roughness .8 specular 0 reflection 0 diffuseMix .7 }\ntrack bound { upload 1 upload 2 sunDirection .3 -.6 -.7 sunConfidence 1 sunIntensity .8 ambient .5 }\n";
        setenv("OPENGT_LIGHTING_SCRIPT",path.c_str(),1);setenv("RECOMPONE_AUDIT_NATIVE_WORLD_OUTPUT_HASH","1",1);setenv("RECOMPONE_AUDIT_NATIVE_WORLD_HASH","1",1);setenv("OPENGT_TEMPORAL_STREAM_DIAGNOSTICS","1",1);
        check(opengt_live_api_version()==11,"existing C ABI version changed");
        Handle a,b;opengt_live_texture_upload uploads[]={{1,0,0,64,1},{2,64,0,64,1}};
        check(opengt_live_set_texture_uploads(a.p,1,uploads,2)==0,"first handle uploads rejected");
        check(opengt_live_set_texture_uploads(b.p,1,nullptr,0)==0,"empty second handle uploads rejected");
        auto gray=capture(1,10,{140,140,140});auto lit=render(a,gray,options(),96,96,1,10);auto unlit=render(b,gray,options(),96,96,1,10);
        check(lit!=unlit,"live handles shared/overwrote course identity");check(render(a,gray,options(),96,96,1,10)==lit,"second handle changed first handle's renderer");
        // Every async-style source submission must retain the exact authored
        // frame/poll/pixels relationship. Linux completes the GPU copy synchronously.
        for(unsigned f=2;f<18;f++){
            auto cap=capture(f,9+int(f),f%2?std::array<unsigned char,3>{255,0,0}:std::array<unsigned char,3>{0,0,255});
            auto pixels=render(b,cap,options(true),96,96,f,9+int(f));auto center=(48*96+48)*4;
            check(pixels[center+(f%2?0:2)]>240,"real-time C ABI returned stale source color");
            check(pixels[center+(f%2?2:0)]<5,"real-time C ABI mixed source frames");
        }
        // Resize, aspect, backwards frame, poll discontinuity, and sync/queue transitions.
        auto opt=options(true);auto cap=capture(100,300,{0,255,0},80);
        render(b,cap,opt,80,80,100,300);opt.target_aspect_width=2;opt.target_aspect_height=1;render(b,capture(101,301,{0,255,0},80),opt,160,80,101,301);
        render(b,capture(1,1,{0,255,0}),options(true),96,96,1,1);render(b,capture(2,2,{0,255,0}),options(),96,96,2,2);
        opengt_live_stats stats{};stats.struct_size=sizeof(stats);Bytes output(96*96*4);auto invalid=options();invalid.struct_size=0;
        check(opengt_live_render(b.p,gray.data(),gray.size(),output.data(),output.size(),&invalid,&stats)!=0,"invalid C options accepted");
        invalid=options();stats.struct_size=sizeof(stats);check(opengt_live_render(b.p,gray.data(),128,output.data(),output.size(),&invalid,&stats)!=0,"truncated version-6 capture accepted");
        stats.struct_size=sizeof(stats);check(opengt_live_render(b.p,gray.data(),gray.size(),output.data(),8,&invalid,&stats)!=0,"undersized image buffer accepted");
        check(render(a,gray,options(),96,96,1,10)==lit,"invalid second-handle input poisoned first handle");
        {Handle temporary;render(temporary,gray,options(),96,96,1,10);}
        check(render(a,gray,options(),96,96,1,10)==lit,"destroying another live handle destroyed this renderer");
        std::filesystem::remove(path);std::cout<<"Live C ABI: "<<checks<<" checks passed; actual shared library, isolated handles, frame/poll/pixel hashes, FIFO, resize/aspect/reset, invalid input and destruction\n";return 0;
    }catch(const std::exception& e){std::filesystem::remove(path);std::cerr<<"FAIL: "<<e.what()<<" after "<<checks<<" checks\n";return 1;}
}
