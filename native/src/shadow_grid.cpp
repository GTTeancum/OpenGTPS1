#include "opengt/shadow_grid.hpp"
#include <cmath>
#include <algorithm>
namespace opengt::render::lighting {
Vec3 ShadowCameraTransform::vector_to_world(Vec3 p) const noexcept {
    return {float(inverse[0][0]*p.x+inverse[0][1]*p.y+inverse[0][2]*p.z),
            float(inverse[1][0]*p.x+inverse[1][1]*p.y+inverse[1][2]*p.z),
            float(inverse[2][0]*p.x+inverse[2][1]*p.y+inverse[2][2]*p.z)};
}
Vec3 ShadowCameraTransform::world_axis_in_view(Vec3 p) const noexcept {
    return {float(p.x*inverse[0][0]+p.y*inverse[1][0]+p.z*inverse[2][0]),
            float(p.x*inverse[0][1]+p.y*inverse[1][1]+p.z*inverse[2][1]),
            float(p.x*inverse[0][2]+p.y*inverse[1][2]+p.z*inverse[2][2])};
}
double ShadowCameraTransform::origin_on_axis(Vec3 p) const noexcept {
    return p.x*origin[0]+p.y*origin[1]+p.z*origin[2];
}
ShadowCameraTransform shadow_camera_transform(const WorldDrawList& list) noexcept {
    ShadowCameraTransform out{};
    if(!list.lighting_world_anchor_valid||!list.lighting_camera_primary_axes||
       list.lighting_camera_depth_exponent<8||list.lighting_camera_depth_exponent>10)return out;
    for(auto v:list.lighting_camera_world_offset)if((v&1023)!=0)return out;
    const double scale=std::ldexp(1.0,list.lighting_camera_depth_exponent)/(4096.0*1024.0);
    double a[3][3];
    for(unsigned j=0;j<3;++j){
        a[j][0]=list.lighting_camera_rotation[j*3]*scale;
        a[j][1]=-list.lighting_camera_rotation[j*3+2]*scale;
        a[j][2]=list.lighting_camera_rotation[j*3+1]*scale;
    }
    // Exact inverse (canonical columns X,-Z,Y). Reject singular or badly
    // conditioned metadata instead of borrowing another camera's position.
    double co[3][3];
    for(int i=0;i<3;++i)for(int j=0;j<3;++j)
        co[i][j]=a[(i+1)%3][(j+1)%3]*a[(i+2)%3][(j+2)%3]
                -a[(i+1)%3][(j+2)%3]*a[(i+2)%3][(j+1)%3];
    const double det=a[0][0]*co[0][0]+a[0][1]*co[0][1]+a[0][2]*co[0][2];
    if(!std::isfinite(det)||std::abs(det)<1e-5)return out;
    double norm=0,invnorm=0;
    for(int i=0;i<3;++i)for(int j=0;j<3;++j){
        norm+=a[i][j]*a[i][j];out.inverse[i][j]=co[j][i]/det;
        invnorm+=out.inverse[i][j]*out.inverse[i][j];
    }
    if(norm<.1||norm>100||std::sqrt(norm*invnorm)>20)return {};
    const auto& o=list.lighting_camera_world_offset;
    out.origin[0]=-double(o[0]);out.origin[1]=double(o[2]);out.origin[2]=-double(o[1]);
    out.valid=true;return out;
}
std::array<float,4> snapped_shadow_row(const ShadowCameraTransform& camera,Vec3 axis,
    Vec3 center,float radius,unsigned resolution) noexcept {
    if(!camera.valid||!std::isfinite(radius)||radius<=0||resolution<4)return {};
    const auto dual=camera.world_axis_in_view(axis);
    const double origin=camera.origin_on_axis(axis);
    const double relative=double(dual.x)*center.x+double(dual.y)*center.y+double(dual.z)*center.z;
    const double step=2.0*radius/resolution;
    const double snapped=std::floor((origin+relative)/step+.5)*step;
    return {dual.x/radius,dual.y/radius,dual.z/radius,float((origin-snapped)/radius)};
}
}
