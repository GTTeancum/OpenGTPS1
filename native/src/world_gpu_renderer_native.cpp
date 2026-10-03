#include "opengt/world_gpu_renderer_native.hpp"
namespace opengt::render {
#if defined(__linux__)
WorldGpuRenderResult render_world_egl(const WorldDrawList&,const std::uint16_t*,std::size_t,std::uint8_t*,std::size_t,WorldGpuRenderOptions,WorldGpuRenderStats*) noexcept;
WorldGpuReadbackResult read_world_egl_image(bool,std::uint8_t*,std::size_t,bool) noexcept;
WorldGpuReadbackResult read_world_egl_pair(bool,std::uint8_t*,std::uint8_t*,std::size_t,bool) noexcept;
void reset_world_egl(bool) noexcept;
void select_world_egl_context(const void*) noexcept;
void release_world_egl_context(const void*) noexcept;
bool uploads_world_egl(bool,const WorldTextureUpload*,std::size_t) noexcept;
#define NATIVE_RENDER render_world_egl
#define NATIVE_IMAGE read_world_egl_image
#define NATIVE_PAIR read_world_egl_pair
#define NATIVE_RESET reset_world_egl
#define NATIVE_UPLOADS uploads_world_egl
#else
#define NATIVE_RENDER render_world_d3d11
#define NATIVE_IMAGE try_read_world_d3d11_image
#define NATIVE_PAIR try_read_world_d3d11_pair
#define NATIVE_RESET reset_world_d3d11_readback
#define NATIVE_UPLOADS set_world_d3d11_texture_uploads
NativeLightingStats world_native_lighting_stats() noexcept{return {};}
const char* world_native_renderer_name() noexcept{return "D3D11";}
#endif
WorldGpuRenderResult render_world_native(const WorldDrawList& l,const std::uint16_t* v,std::size_t n,std::uint8_t* o,std::size_t size,WorldGpuRenderOptions options,WorldGpuRenderStats* s) noexcept{return NATIVE_RENDER(l,v,n,o,size,options,s);}
WorldGpuReadbackResult try_read_world_native_image(bool b,std::uint8_t* p,std::size_t n,bool wait) noexcept{return NATIVE_IMAGE(b,p,n,wait);}
WorldGpuReadbackResult try_read_world_native_pair(bool b,std::uint8_t* p,std::uint8_t* q,std::size_t n,bool wait) noexcept{return NATIVE_PAIR(b,p,q,n,wait);}
void reset_world_native_readback(bool b) noexcept{NATIVE_RESET(b);}
void select_world_native_context(const void* token) noexcept {
#if defined(__linux__)
    select_world_egl_context(token);
#else
    (void)token;
#endif
}
void release_world_native_context(const void* token) noexcept {
#if defined(__linux__)
    release_world_egl_context(token);
#else
    (void)token;
#endif
}
bool set_world_native_texture_uploads(bool b,const WorldTextureUpload* p,std::size_t n) noexcept{return NATIVE_UPLOADS(b,p,n);}
void drain_world_native_direct_output() noexcept{
#if !defined(__linux__)
 drain_world_d3d11_direct_output();
#endif
}
bool set_world_native_presentation_device(void* p) noexcept{
#if defined(__linux__)
 return p==nullptr; // a Windows COM pointer is never accepted as a GL device
#else
 return set_world_d3d11_presentation_device(p);
#endif
}
}
