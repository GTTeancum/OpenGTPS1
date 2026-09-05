#include "opengt/vehicle_paint.hpp"
#include <algorithm>
#include <cmath>
#include <cstdio>
#include <filesystem>
#include <fstream>

using namespace opengt::render;
namespace {
bool expect(bool value,const char* message) {
    if(!value) std::fprintf(stderr,"FAILED: %s\n",message);
    return value;
}
}
int main() {
    bool okay=true;
    PaintFace face{};
    for(int i=0;i<3;++i) {
        face.corners[i].position[i]=100;
        face.corners[i].normal[0]=face.corners[i].normal[1]=std::sqrt(0.5F);
        face.corners[i].uv[0]=float(i*20);
        face.corners[i].uv[1]=140;
    }
    auto reordered=face;
    std::swap(reordered.corners[0],reordered.corners[2]);
    okay &= expect(paint_face_key(face)==paint_face_key(reordered),"source face is winding independent");
    std::vector<std::uint16_t> vram(1024*512,0x1234);
    VehiclePaintPack pack{};
    pack.width=256;pack.height=224;
    pack.bitmap_key=vehicle_paint_bitmap_key(vram.data(),0,256,224);
    pack.faces.emplace(paint_face_key(face),face);
    okay &= expect(vehicle_paint_bitmap_key(vram.data(),128,256,224)==0,"reject non-4-bit texture");
    okay &= expect(vehicle_paint_bitmap_key(nullptr,0,256,224)==0,"null VRAM fails closed");
    WorldDrawList list{};
    WorldMaterial base{},env{};base.primitive_flags=5;env.primitive_flags=3;env.texture_page=32;
    list.materials={base,env};
    WorldDrawCommand command{};
    command.object_kind=2;command.object_id=0;command.model_pointer=0x80000100;
    for(int i=0;i<3;++i) {
        auto& v=command.vertices[i];
        v.model_x=reordered.corners[i].position[0];v.model_y=reordered.corners[i].position[1];v.model_z=reordered.corners[i].position[2];
        v.exact_transform_valid=true;
        v.transform_rotation[0]=8192;v.transform_rotation[4]=v.transform_rotation[8]=4096;
        v.view_z=1000;v.r=v.g=v.b=96;
    }
    list.commands.push_back(command);command.material_index=1;list.commands.push_back(command);
    command.object_id=1;list.commands.push_back(command); // no matching base ownership
    command.object_id=0;command.object_kind=1;list.commands.push_back(command); // track/flare
    const auto frame=build_vehicle_paint_frame(list,vram.data(),pack);
    okay &= expect(frame.matched_vehicles==1 && frame.reflection_triangles==1,"only identified car's environment pass changes");
    okay &= expect(frame.vertices.size()==4 && frame.vertex_indices[0]==0 && frame.vertex_indices[6]==0 && frame.vertex_indices[9]==0,"base, other car and course untouched");
    const auto& vertex=frame.vertices[frame.vertex_indices[3]];
    okay &= expect(vertex.uv[0]==40 && vertex.uv[1]==140,"UV follows source corner after reordering");
    okay &= expect(std::fabs(vertex.normal[1]/vertex.normal[0]-2.0F)<0.001F,"inverse transpose handles non-uniform scaling");
    okay &= expect(vertex.view_position[2]==1000 && vertex.intensity==0.75F,"view and authored intensity retained");
    okay &= expect(list.commands[1].vertices[0].u==0 && list.commands[1].vertices[0].r==96,"source draw list not mutated");
    list.commands[1].vertices[1].exact_transform_valid=false;
    okay &= expect(build_vehicle_paint_frame(list,vram.data(),pack).reflection_triangles==0,"incomplete transform falls back whole triangle");
    list.commands[1].vertices[1].exact_transform_valid=true;
    list.commands[1].vertices[1].transform_rotation[0]=0;
    okay &= expect(build_vehicle_paint_frame(list,vram.data(),pack).vertices.size()==1,"singular transform falls back whole triangle");
    vram[0]^=1;
    okay &= expect(build_vehicle_paint_frame(list,vram.data(),pack).matched_vehicles==0,"different bitmap cannot inherit paint by memory address");
    pack.alternate_bitmap_key=vehicle_paint_bitmap_key(vram.data(),0,256,224);
    okay &= expect(build_vehicle_paint_frame(list,vram.data(),pack).matched_vehicles==1,"explicitly verified night bitmap is accepted");
    okay &= expect(build_vehicle_paint_frame(list,nullptr,pack).matched_vehicles==0,"unavailable bitmap cannot match a variant");
    std::string error;
    okay &= expect(!load_vehicle_paint_pack("no-such-test-pack",&pack,&error) && !error.empty(),"missing pack safely rejected");
    // A bounded malformed file must not replace a previously valid pack.
    const auto path=std::filesystem::temp_directory_path()/"opengt-invalid-paint-test.bin";
    { std::ofstream output(path,std::ios::binary); std::string invalid(64,'x'); output.write(invalid.data(),invalid.size()); }
    okay &= expect(!load_vehicle_paint_pack(path.string().c_str(),&pack,&error) && pack.faces.size()==1,"bad magic rejected without partial state");
    std::filesystem::remove(path);
    std::printf("vehicle paint tests: %s\n",okay?"PASS":"FAIL");
    return okay?0:1;
}
