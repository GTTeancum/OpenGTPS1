#include "opengt/vehicle_paint.hpp"
#include <algorithm>
#include <cmath>
#include <cstring>
#include <fstream>
#include <iterator>
#include <set>
#include <stdexcept>

namespace opengt::render {
namespace {
std::uint32_t u32(const std::vector<std::uint8_t>& b, std::size_t p) {
    if (p + 4 > b.size()) throw std::runtime_error("truncated paint pack");
    return b[p] | (std::uint32_t(b[p+1])<<8) | (std::uint32_t(b[p+2])<<16) |
        (std::uint32_t(b[p+3])<<24);
}
std::int16_t i16(const std::vector<std::uint8_t>& b, std::size_t p) {
    if (p + 2 > b.size()) throw std::runtime_error("truncated paint corner");
    return static_cast<std::int16_t>(b[p] | (std::uint16_t(b[p+1])<<8));
}
PaintFaceKey command_key(const WorldDrawCommand& command) {
    PaintFace face{};
    for (int i=0;i<3;++i) {
        face.corners[i].position[0]=command.vertices[i].model_x;
        face.corners[i].position[1]=command.vertices[i].model_y;
        face.corners[i].position[2]=command.vertices[i].model_z;
    }
    return paint_face_key(face);
}
bool transform_normal(const std::int16_t* matrix, const float* source, float* result) {
    // Guest transforms may include non-uniform scale. Normals use the inverse
    // transpose, not the position matrix; reject singular transforms safely.
    float m[9];
    for (int i=0;i<9;++i) m[i]=matrix[i]/4096.0F;
    const float c[9]{m[4]*m[8]-m[5]*m[7], m[5]*m[6]-m[3]*m[8], m[3]*m[7]-m[4]*m[6],
        m[2]*m[7]-m[1]*m[8], m[0]*m[8]-m[2]*m[6], m[1]*m[6]-m[0]*m[7],
        m[1]*m[5]-m[2]*m[4], m[2]*m[3]-m[0]*m[5], m[0]*m[4]-m[1]*m[3]};
    const float determinant=m[0]*c[0]+m[1]*c[1]+m[2]*c[2];
    if(std::fabs(determinant)<1e-8F) return false;
    float length=0;
    for(int axis=0;axis<3;++axis) {
        result[axis]=(c[axis*3]*source[0]+c[axis*3+1]*source[1]+c[axis*3+2]*source[2])/determinant;
        length+=result[axis]*result[axis];
    }
    if(length<1e-12F || !std::isfinite(length)) return false;
    for(int axis=0;axis<3;++axis) result[axis]/=std::sqrt(length);
    return true;
}
}
std::size_t PaintFaceHash::operator()(const PaintFaceKey& key) const noexcept {
    std::uint64_t hash=14695981039346656037ULL;
    for (const auto value:key) {
        const auto word=static_cast<std::uint16_t>(value);
        hash=(hash^(word&255U))*1099511628211ULL;
        hash=(hash^(word>>8))*1099511628211ULL;
    }
    return static_cast<std::size_t>(hash);
}
PaintFaceKey paint_face_key(const PaintFace& face) noexcept {
    std::array<std::array<std::int16_t,3>,3> points{};
    for (int i=0;i<3;++i) std::copy_n(face.corners[i].position,3,points[i].begin());
    std::sort(points.begin(),points.end());
    PaintFaceKey result{};
    for (int i=0;i<3;++i) std::copy(points[i].begin(),points[i].end(),result.begin()+i*3);
    return result;
}
bool load_vehicle_paint_pack(const char* path, VehiclePaintPack* pack,
    std::string* error) noexcept {
    if (!path || !pack) return false;
    try {
        std::ifstream stream(path,std::ios::binary|std::ios::ate);
        if (!stream) throw std::runtime_error("paint pack absent");
        const auto size=stream.tellg();
        if (size<32 || size>4*1024*1024) throw std::runtime_error("invalid paint pack size");
        stream.seekg(0);
        std::vector<std::uint8_t> bytes(static_cast<std::size_t>(size));
        if (!stream.read(reinterpret_cast<char*>(bytes.data()),size))
            throw std::runtime_error("paint pack read failed");
        const auto version=u32(bytes,8);
        if (std::memcmp(bytes.data(),"OGTPAINT",8)!=0 || (version!=1 && version!=2))
            throw std::runtime_error("unsupported paint pack");
        const std::size_t header_size=version==2?40:32;
        VehiclePaintPack result{};
        result.width=u32(bytes,12); result.height=u32(bytes,16);
        const auto count=u32(bytes,20);
        result.bitmap_key=std::uint64_t(u32(bytes,24))|(std::uint64_t(u32(bytes,28))<<32);
        if(version==2) result.alternate_bitmap_key=std::uint64_t(u32(bytes,32))|(std::uint64_t(u32(bytes,36))<<32);
        if (result.width!=256 || result.height!=224 || count==0 || count>4096 ||
            result.bitmap_key==0 || bytes.size()!=header_size+count*48+256*224*4)
            throw std::runtime_error("invalid paint pack layout");
        for (std::uint32_t index=0;index<count;++index) {
            PaintFace face{};
            for (int corner=0;corner<3;++corner) {
                auto& c=face.corners[corner];
                const auto p=header_size+index*48+corner*16;
                for(int axis=0;axis<3;++axis) {
                    c.position[axis]=i16(bytes,p+axis*2);
                    c.normal[axis]=i16(bytes,p+6+axis*2)/4096.0F;
                }
                const float length=c.normal[0]*c.normal[0]+c.normal[1]*c.normal[1]+c.normal[2]*c.normal[2];
                if(length<0.8F || length>1.2F) throw std::runtime_error("invalid authored paint normal");
                c.uv[0]=static_cast<float>(i16(bytes,p+12));
                c.uv[1]=static_cast<float>(i16(bytes,p+14));
                if(c.uv[0]<0 || c.uv[0]>=256 || c.uv[1]<0 || c.uv[1]>=224)
                    throw std::runtime_error("paint UV out of bounds");
            }
            if (!result.faces.emplace(paint_face_key(face),face).second)
                throw std::runtime_error("ambiguous paint face");
        }
        result.mask.assign(bytes.begin()+header_size+count*48,bytes.end());
        *pack=std::move(result);
        return true;
    } catch(const std::exception& e) { if(error)*error=e.what(); return false; }
}
std::uint64_t vehicle_paint_bitmap_key(const std::uint16_t* vram,
    std::uint16_t page,std::uint32_t width,std::uint32_t height) noexcept {
    if (!vram || ((page>>7)&3)!=0 || width!=256 || height!=224) return 0;
    const int x=(page&15)*64, y=((page>>4)&1)*256;
    std::uint64_t hash=14695981039346656037ULL;
    for(std::uint32_t row=0;row<height;++row) for(std::uint32_t col=0;col<width/4;++col) {
        const auto word=vram[(y+row)*1024+x+col];
        hash=(hash^(word&255U))*1099511628211ULL;
        hash=(hash^(word>>8))*1099511628211ULL;
    }
    return hash;
}
VehiclePaintFrame build_vehicle_paint_frame(const WorldDrawList& list,
    const std::uint16_t* vram,const VehiclePaintPack& pack) {
    VehiclePaintFrame result;
    result.vertex_indices.resize(list.commands.size()*3);
    result.vertices.emplace_back(); // index zero is the unchanged legacy path.
    std::unordered_map<std::uint16_t,bool> pages;
    std::set<std::pair<std::uint32_t,std::uint32_t>> vehicles;
    for (const auto& command:list.commands) {
        if(command.object_kind!=2 || command.material_index>=list.materials.size()) continue;
        const auto& material=list.materials[command.material_index];
        if((material.primitive_flags&7)!=5 ||
            (material.primitive_flags&world_primitive_screen_space_flag)!=0 ||
            pack.faces.find(command_key(command))==pack.faces.end()) continue;
        auto [page,inserted]=pages.emplace(material.texture_page,false);
        if(inserted) {
            const auto key=vehicle_paint_bitmap_key(vram,material.texture_page,pack.width,pack.height);
            page->second=key!=0 && (key==pack.bitmap_key || key==pack.alternate_bitmap_key);
        }
        if(page->second) vehicles.emplace(command.object_id,command.model_pointer);
    }
    result.matched_vehicles=static_cast<std::uint32_t>(vehicles.size());
    // Deliberately small GT2000-style material experiment: a broad, viewer-
    // relative softbox, not a new course-lighting or reflection-probe system.
    // Authored per-corner intensity still modulates the response.
    const float light[3]{-0.10F,-0.75F,0.65F};
    for(std::size_t index=0;index<list.commands.size();++index) {
        const auto& command=list.commands[index];
        if(command.object_kind!=2 || !vehicles.count({command.object_id,command.model_pointer}) ||
            command.material_index>=list.materials.size()) continue;
        const auto& material=list.materials[command.material_index];
        // Only replace the authored additive environment pass, never the base
        // diffuse layer, transparent glass ownership, shadows, wheels or HUD.
        if((material.primitive_flags&7)!=3 || ((material.texture_page>>5)&3)!=1 ||
            (material.primitive_flags&world_primitive_screen_space_flag)!=0) continue;
        const auto found=pack.faces.find(command_key(command));
        if(found==pack.faces.end()) continue;
        bool valid=true;
        for(const auto& v:command.vertices) valid=valid && v.exact_transform_valid;
        if(!valid) continue;
        PaintGpuVertex triangle[3]{};
        for(int corner=0;corner<3;++corner) {
            const auto& source=command.vertices[corner];
            const PaintCorner* authored=nullptr;
            for(const auto& candidate:found->second.corners)
                if(candidate.position[0]==source.model_x && candidate.position[1]==source.model_y &&
                    candidate.position[2]==source.model_z) authored=&candidate;
            if(!authored) { valid=false; break; }
            auto& vertex=triangle[corner];
            if(!transform_normal(source.transform_rotation,authored->normal,vertex.normal)) {
                valid=false; break;
            }
            vertex.mode=1.0F;
            vertex.intensity=(source.r+source.g+source.b)/(3.0F*128.0F);
            vertex.view_position[0]=source.view_x;
            vertex.view_position[1]=source.view_y;
            vertex.view_position[2]=source.view_z;
            std::copy_n(authored->uv,2,vertex.uv);
            std::copy_n(light,3,vertex.light_direction);
        }
        // A partial triangle would interpolate with an uninitialized material.
        if(!valid) continue;
        ++result.reflection_triangles;
        for(int corner=0;corner<3;++corner) {
            result.vertex_indices[index*3+corner]=static_cast<std::uint32_t>(result.vertices.size());
            result.vertices.push_back(triangle[corner]);
        }
    }
    return result;
}
} // namespace opengt::render
