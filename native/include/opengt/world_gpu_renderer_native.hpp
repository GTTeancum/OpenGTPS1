#pragma once
#include "opengt/world_gpu_renderer.hpp"
namespace opengt::render {
// Platform-neutral entry points used by the live C ABI. Linux uses its own
// EGL/OpenGL context; Windows retains D3D11. This is not a D3D compatibility shim.
WorldGpuRenderResult render_world_native(const WorldDrawList&,const std::uint16_t*,std::size_t,
    std::uint8_t*,std::size_t,WorldGpuRenderOptions,WorldGpuRenderStats*) noexcept;
WorldGpuReadbackResult try_read_world_native_image(bool,std::uint8_t*,std::size_t,bool=false) noexcept;
WorldGpuReadbackResult try_read_world_native_pair(bool,std::uint8_t*,std::uint8_t*,std::size_t,bool=false) noexcept;
void reset_world_native_readback(bool) noexcept;
// Live handles are isolated within their owning render thread on Linux. The
// standalone C++ path uses the null token. No token or GL pointer is exposed
// to the shaders or used to identify a texture/material.
void select_world_native_context(const void*) noexcept;
void release_world_native_context(const void*) noexcept;
void drain_world_native_direct_output() noexcept;
bool set_world_native_presentation_device(void*) noexcept;
bool set_world_native_texture_uploads(bool,const WorldTextureUpload*,std::size_t) noexcept;
// Linux diagnostics; no GL handles cross the public C ABI. Counters accumulate on the current rendering thread.
struct NativeLightingStats {
    std::uint64_t frames{},materials_lit{},shadow_vertices{},reflection_probes{},reflection_faces{},
        reflected_scene_vertices{},readback_bytes{};
};
NativeLightingStats world_native_lighting_stats() noexcept;
const char* world_native_renderer_name() noexcept;
}
