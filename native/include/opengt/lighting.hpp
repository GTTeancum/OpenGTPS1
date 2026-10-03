#pragma once
#include "opengt/world_draw_list.hpp"
#include <array>
#include <cstdint>
#include <filesystem>
#include <string>
#include <vector>

namespace opengt::render::lighting {
// L01: independent of D3D. All lighting positions use the shared GT2 depth
// normalization, NEVER the independently normalized raw GTE coordinates.
struct Vec3 { float x{}, y{}, z{}; };
Vec3 operator+(Vec3, Vec3) noexcept;
Vec3 operator-(Vec3, Vec3) noexcept;
Vec3 operator*(Vec3, float) noexcept;
float dot(Vec3, Vec3) noexcept;
Vec3 cross(Vec3, Vec3) noexcept;
Vec3 normalized(Vec3) noexcept;

enum class Surface { legacy, car, rubber, road, track };
struct Material {
    std::string name;
    Surface surface{Surface::legacy};
    // An exact source-material override is optional. A course-qualified rule
    // never leaks onto another track or a garage render.
    std::string track;
    std::uint64_t primitive_key{};
    // Destructive baked-light edits need stricter provenance than primitiveKey:
    // identical local geometry/UVs can legitimately occur more than once in a
    // course. source_mesh_key is the path-neutral packed authored vertex-table
    // identity; source_primitive_address is the original primitive record.
    std::uint64_t source_mesh_key{};
    std::uint32_t source_primitive_address{};
    // Immutable resident-definition fingerprint. This is distinct from the
    // path-neutral source address identity above: destructive baked-light
    // edits must bind both, so same-address content reuse cannot inherit a rule.
    std::uint64_t resident_content_key{};
    int texture_page{-1}, clut{-1};
    // Inclusive bounds in the original texture page, not a sampled pixel color.
    // A UV restriction requires track + page + CLUT so it cannot leak to another bank.
    std::array<int,4> uv_rect{-1,-1,-1,-1};
    float minimum_up_normal{};
    float roughness{0.7F}, specular{0.1F}, reflection{0.0F};
    float clearcoat{}, clearcoat_roughness{0.16F}, diffuse_mix{0.10F};
    float receive_shadows{1.0F}, cast_shadows{1.0F};
    Vec3 baked_gain{1,1,1};
    bool remove_shadow_overlay{};
};
struct Track {
    std::string name;
    std::vector<std::uint64_t> uploads;
    unsigned minimum_matches{2};
    bool night{};
    // Explicit art-directed development light, NOT an accepted inverse fit.
    // It can project shadows but can NEVER enable baked-gain/removal rules.
    bool provisional_sun{};
    // Direction is TOWARDS sun in source course axes; +Y is down.
    Vec3 sun_direction{0,-1,0}, sun_color{1,1,1};
    Vec3 sky_color{0.38F,0.48F,0.62F}, ground_color{0.18F,0.17F,0.16F};
    float sun_intensity{}, confidence{}, ambient{0.85F};
    float shadow_strength{0.35F};
    // Author-controlled opacity for the existing source vehicle footprint.
    // Defaults preserve the earlier renderer; opt-in profiles can ground cars
    // more firmly without changing sunlit textures, exposure or speculars.
    float contact_shadow_opacity{0.06F};
    // NOT latitude/time-of-day claims. These are asset-derived/art-directed
    // scene parameters. Solar light is gated by the evidence confidence.
};
struct Settings {
    bool enabled{true}, shadows{true};
    unsigned shadow_resolution{1024};
    float minimum_confidence{0.75F}, normal_crease_degrees{55};
    float shadow_bias{0.001F};
    float near_shadow_car_spans{36.0F};
    bool local_reflections{true};
    unsigned reflection_resolution{128}, maximum_reflection_probes{4};
    std::string force_track;
};
struct Database {
    Settings settings;
    std::vector<Material> materials;
    std::vector<Track> tracks;
};
// Transactional: out remains unchanged on errors. Text is <= 4 MiB, numeric
// data must be finite, duplicate blocks/keys and unknown directives reject.
bool parse_script(const std::string&, Database* out, std::string* error) noexcept;
const Database* runtime_database() noexcept; // missing file => stock rendering
bool requested() noexcept; // extra sun-caster geometry required by active configuration
const Track* select_track(const Database&, const std::vector<std::uint64_t>& uploads) noexcept;
const Material* select_material(const Database&, Surface, const std::string& track,
    std::uint64_t primitive_key, const WorldMaterial&,
    const WorldDrawCommand* command=nullptr, float normal_up=0) noexcept;
std::uint64_t primitive_key(const WorldDrawCommand&, const WorldMaterial&) noexcept;

// Diagnostic-only evidence for authoring exact baked-light rules. This never
// changes rendering and deliberately carries the strict resident provenance
// required by destructive material overrides. Records are restricted to the
// same conservative class used by the original offline inverse-light audit:
// opaque textured resident-course triangles, nearly horizontal, uniform
// authored vertex color, and never camera-facing billboards.
struct UnbakeAuditRecord {
    bool eligible{};
    Surface surface{Surface::legacy};
    std::uint64_t primitive_key{};
    std::uint64_t source_mesh_key{};
    std::uint32_t source_primitive_address{};
    std::uint64_t resident_content_key{};
    std::uint32_t primitive_flags{};
    std::uint32_t object_id{};
    std::uint32_t model_pointer{};
    std::uint16_t texture_page{};
    std::uint16_t clut{};
    std::array<float,6> uv{};
    std::array<std::uint8_t,3> color{};
    // Camera-independent reconstructed world geometry used only by the
    // non-destructive audit. These values never participate in rendering or
    // destructive-rule matching. They let the analyzer reject a source
    // identity whose apparent receiver plane drifts as the camera changes.
    Vec3 world_centroid{};
    Vec3 world_normal{};
    float world_scale{};
    float normal_up{};
};
UnbakeAuditRecord inspect_unbake_candidate(const WorldDrawCommand&,
    const WorldMaterial&, Surface, float normal_up) noexcept;

struct ShadowEvidence { Vec3 caster, shadow; float weight{1}; std::uint32_t object{}; };
struct SunFit {
    Vec3 direction{};
    float confidence{}, rms_degrees{};
    unsigned inliers{}, independent_objects{};
    bool accepted{};
};
// Robust consensus of CORRESPONDING caster tips and shadow tips. Dark RGB
// alone is never a correspondence, and repeated samples from one object do
// not become independent evidence.
SunFit fit_sun(const std::vector<ShadowEvidence>&, float tolerance_degrees=10) noexcept;

struct Vertex { Vec3 position{}, normal{}; };
struct DrawMaterial {
    // Mirrors three float4s in the HLSL MaterialData, plus exact gain/control.
    std::array<float,4> surface{}; // roughness, specular, reflection, enabled
    std::array<float,4> coat{};    // coat, coat roughness, diffuse mix, receive
    std::array<float,4> gain{1,1,1,0}; // RGB unbake; W=remove verified overlay
};
struct ShadowVertex {
    float position[3]{}; float uv[2]{};
    std::uint32_t texture_page{}, clut{}, flags{};
    std::int32_t mask_x{},mask_y{},offset_x{},offset_y{};
};
static_assert(sizeof(ShadowVertex)==48);
struct alignas(16) Constants {
    std::array<float,4> sun{}; // camera-space to-sun, intensity
    std::array<float,4> color{}; // solar linear RGB, shadow strength
    std::array<float,4> sky{}; // linear RGB, enabled
    std::array<float,4> ground{}; // linear RGB, shadows active
    std::array<float,4> up{}; // camera-space world up, ambient
    std::array<std::array<float,4>,8> shadow_rows{};
    std::array<float,4> shadow_config{}; // near/far distance, 1/res, normalized bias
};
static_assert(sizeof(Constants)==224);
// Owned by one renderer context; never a static/global last-camera cache.
// The scale lock contains no positions/vertices and is cleared on invalid
// source metadata, scene changes, stream reset or disabled lighting.
struct ShadowGridState {
    std::string track;
    std::uint64_t frame_index{};
    float source_span{};
};
struct Frame {
    Constants constants{};
    std::vector<Vertex> vertices;
    std::vector<DrawMaterial> materials;
    std::vector<ShadowVertex> casters;
    std::string track;
    bool enabled{}, solar{}, shadows{}, provisional_sun{}, world_grid{};
    float contact_shadow_opacity{0.06F};
};
// Optional roles come from the renderer's existing authored wheel/road
// ownership classifiers, not from the pixel's apparent color.
Frame prepare(const WorldDrawList&, const Database&, const Track*,
    const std::vector<Surface>& roles = {}, ShadowGridState* grid = nullptr);
// A reference-only BRDF, used by CPU tests; no rasterized postprocess in game.
Vec3 shade_reference(Vec3 srgb, Vec3 normal, Vec3 view_direction,
    const DrawMaterial&, const Constants&, float shadow_visibility=1) noexcept;
} // namespace opengt::render::lighting
