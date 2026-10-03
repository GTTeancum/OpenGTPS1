#include "opengt/lighting.hpp"
#include "opengt/shadow_grid.hpp"
#include <algorithm>
#include <cerrno>
#include <chrono>
#include <cmath>
#include <cstdio>
#include <cstdlib>
#include <fstream>
#include <limits>
#include <map>
#include <set>
#include <sstream>
#include <stdexcept>
#include <tuple>
#include <unordered_map>
#if defined(_WIN32)
#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <windows.h>
#endif

namespace opengt::render::lighting {
Vec3 operator+(Vec3 a,Vec3 b) noexcept{return {a.x+b.x,a.y+b.y,a.z+b.z};}
Vec3 operator-(Vec3 a,Vec3 b) noexcept{return {a.x-b.x,a.y-b.y,a.z-b.z};}
Vec3 operator*(Vec3 a,float b) noexcept{return {a.x*b,a.y*b,a.z*b};}
float dot(Vec3 a,Vec3 b) noexcept{return a.x*b.x+a.y*b.y+a.z*b.z;}
Vec3 cross(Vec3 a,Vec3 b) noexcept{return {a.y*b.z-a.z*b.y,a.z*b.x-a.x*b.z,a.x*b.y-a.y*b.x};}
Vec3 normalized(Vec3 a) noexcept{float l=std::sqrt(dot(a,a));return std::isfinite(l)&&l>1e-12F?a*(1/l):Vec3{};}
namespace {
constexpr float pi=3.14159265358979323846F;
float sat(float x){return std::clamp(x,0.0F,1.0F);}
bool finite(Vec3 a){return std::isfinite(a.x)&&std::isfinite(a.y)&&std::isfinite(a.z);}
float length(Vec3 a){return std::sqrt(dot(a,a));}
struct Token {std::string text; unsigned line;};
class Parser {
    std::vector<Token> tokens; std::size_t at{};
public:
    explicit Parser(const std::string& s){
        if(s.size()>4*1024*1024)throw std::runtime_error("script exceeds 4 MiB");
        unsigned line=1;std::size_t i=0;
        while(i<s.size()){
            unsigned char c=static_cast<unsigned char>(s[i]);
            if(c==0)throw std::runtime_error("NUL in shader script");
            if(c<=32){line+=c=='\n';++i;continue;}
            if(s.compare(i,2,"//")==0){while(i<s.size()&&s[i]!='\n')++i;continue;}
            if(s.compare(i,2,"/*")==0){i+=2;bool done=false;while(i<s.size()){
                if(s.compare(i,2,"*/")==0){i+=2;done=true;break;}line+=s[i++]=='\n';}
                if(!done)throw std::runtime_error("unterminated comment");
                continue;}
            unsigned start_line=line;
            if(c=='{'||c=='}'){tokens.push_back({s.substr(i++,1),start_line});continue;}
            std::string text;
            if(c=='"'){
                ++i;bool done=false;while(i<s.size()){
                    char x=s[i++];if(x=='"'){done=true;break;}
                    if(x=='\n'||x=='\r'||x=='\0')throw std::runtime_error("newline/NUL in quoted token");
                    if(x=='\\'){if(i==s.size()||(s[i]!='"'&&s[i]!='\\'))throw std::runtime_error("invalid escape");x=s[i++];}
                    text+=x;
                }if(!done)throw std::runtime_error("unterminated string");
            }else{
                while(i<s.size()&&static_cast<unsigned char>(s[i])>32&&s[i]!='{'&&s[i]!='}'&&s.compare(i,2,"//")&&s.compare(i,2,"/*"))text+=s[i++];
            }
            if(text.size()>256)throw std::runtime_error("token exceeds 256 bytes");
            tokens.push_back({std::move(text),start_line});
            if(tokens.size()>300000)throw std::runtime_error("too many shader tokens");
        }
    }
    bool end()const{return at==tokens.size();}
    std::string next(){if(end())fail("unexpected end of file");return tokens[at++].text;}
    [[noreturn]] void fail(const std::string& s)const{throw std::runtime_error("line "+std::to_string(at?tokens[at-1].line:1)+": "+s);}
    void expect(const char* s){if(next()!=s)fail(std::string("expected '")+s+"'");}
    float number(float lo,float hi){auto t=next();char* e=nullptr;errno=0;float f=std::strtof(t.c_str(),&e);
        if(t.empty()||e!=t.c_str()+t.size()||errno==ERANGE||!std::isfinite(f)||f<lo||f>hi)fail("invalid/out-of-range number '"+t+"'");
        return f;}
    unsigned integer(unsigned lo,unsigned hi){float f=number(static_cast<float>(lo),static_cast<float>(hi));if(std::floor(f)!=f)fail("expected integer");return static_cast<unsigned>(f);}
    std::uint64_t key(){auto t=next();if(t.empty()||t[0]=='-'||t[0]=='+')fail("invalid unsigned key");char* e=nullptr;errno=0;
        auto k=std::strtoull(t.c_str(),&e,t.size()>2&&t[0]=='0'&&(t[1]=='x'||t[1]=='X')?16:10);
        if(e!=t.c_str()+t.size()||errno||!k)fail("invalid/zero source key");
        return k;}
    bool flag(){return integer(0,1)!=0;}
    Vec3 vector(float lo,float hi){float x=number(lo,hi),y=number(lo,hi),z=number(lo,hi);return {x,y,z};}
    std::string name(){auto n=next();if(n.empty()||n=="{"||n=="}"||n.size()>128)fail("invalid name");return n;}
    static Surface surface(const std::string& s){if(s=="car")return Surface::car;if(s=="rubber")return Surface::rubber;if(s=="road")return Surface::road;if(s=="track")return Surface::track;if(s=="legacy")return Surface::legacy;throw std::runtime_error("unknown surface '"+s+"'");}
};
void unique(Parser& p,std::set<std::string>& seen,const std::string& key){if(!seen.insert(key).second)p.fail("duplicate directive '"+key+"'");}
Vec3 rgb(std::array<float,4> a){return {a[0],a[1],a[2]};}
float linear(float x){x=sat(x);return x<=0.04045F?x/12.92F:std::pow((x+0.055F)/1.055F,2.4F);}
float srgb(float x){x=std::max(0.0F,x);return sat(x<=0.0031308F?12.92F*x:1.055F*std::pow(x,1/2.4F)-0.055F);}
Vec3 multiply(Vec3 a,Vec3 b){return {a.x*b.x,a.y*b.y,a.z*b.z};}
struct Basis{Vec3 r[3];bool valid{};Vec3 apply(Vec3 a)const{return {dot(r[0],a),dot(r[1],a),dot(r[2],a)};}};
Basis basis(const WorldDrawList& list){
    Basis b;auto& m=list.lighting_camera_rotation;
    Vec3 x{float(m[0]),float(m[1]),float(m[2])},y{float(m[3]),float(m[4]),float(m[5])},z{float(m[6]),float(m[7]),float(m[8])};
    x=normalized(x);y=normalized(y-x*dot(x,y));Vec3 zz=normalized(cross(x,y));
    const float handedness=dot(zz,normalized(z));
    if(length(x)<0.99F||length(y)<0.99F||std::abs(handedness)<0.98F)return b;
    // Authored primary GTE cameras have a reflected source basis. Preserve its
    // sign; treating every valid transform as a proper rotation disabled live
    // lighting while the right-handed inspection camera appeared to work.
    if(handedness<0)zz=zz*-1;
    b.r[0]=x;b.r[1]=y;b.r[2]=zz;
    if(list.lighting_camera_primary_axes) {
        // Script convention is canonical X, -source-Z, source-Y. Convert the
        // camera columns, not any source vertices or guest projection state.
        for(auto& row:b.r)row={row.x,-row.z,row.y};
    }
    b.valid=true;return b;
}
bool physical(const WorldDrawCommand& c,const WorldMaterial& m){
    return c.channel==WorldViewChannel::main_view&&(c.object_kind==1||c.object_kind==2)&&
        !(m.primitive_flags&world_primitive_screen_space_flag)&&c.lighting_depth_scale>0&&std::isfinite(c.lighting_depth_scale);
}
Vec3 position(const WorldDrawVertex& v,float scale){return {v.view_x*scale,v.view_y*scale,v.view_z*scale};}
bool vehicle_wheel_tread(
    const WorldDrawCommand& command,
    const WorldMaterial& material
) noexcept {
    if (
        command.object_kind != 2U ||
        (material.primitive_flags &
            (1U | 2U)) != 0
    )
        return false;

    // GT2 wheel meshes are centred cylinders in model space. Their tread
    // triangles bridge the two narrow axle planes while every vertex stays
    // on the same much-larger radial ring. Detect that shape rather than a
    // particular car, model pointer, or capture-local material index.
    std::int32_t minimum_z = command.vertices[0].model_z;
    std::int32_t maximum_z = minimum_z;
    std::int64_t minimum_radius_squared =
        (std::numeric_limits<std::int64_t>::max)();
    std::int64_t maximum_radius_squared = 0;
    std::int32_t maximum_abs_z = 0;
    for (const auto& vertex : command.vertices) {
        minimum_z = (std::min)(
            minimum_z, static_cast<std::int32_t>(vertex.model_z));
        maximum_z = (std::max)(
            maximum_z, static_cast<std::int32_t>(vertex.model_z));
        maximum_abs_z = (std::max)(
            maximum_abs_z,
            std::abs(static_cast<std::int32_t>(vertex.model_z)));
        const std::int64_t x = vertex.model_x;
        const std::int64_t y = vertex.model_y;
        const std::int64_t radius_squared = x * x + y * y;
        minimum_radius_squared = (std::min)(
            minimum_radius_squared, radius_squared);
        maximum_radius_squared = (std::max)(
            maximum_radius_squared, radius_squared);
    }
    if (
        minimum_z >= 0 || maximum_z <= 0 ||
        maximum_abs_z == 0 || minimum_radius_squared < 256 * 256
    )
        return false;
    const std::int64_t maximum_abs_z_squared =
        static_cast<std::int64_t>(maximum_abs_z) * maximum_abs_z;
    return
        minimum_radius_squared > maximum_abs_z_squared * 4 &&
        maximum_radius_squared * 4 <= minimum_radius_squared * 5;
}

Surface infer_surface(const WorldDrawCommand& c,const WorldMaterial& m,Vec3 n,Vec3 up){
    if(m.primitive_flags&2U)return Surface::legacy;
    if(c.object_kind==2)return vehicle_wheel_tread(c,m)?Surface::rubber:Surface::car;
    if(c.object_kind==1){
        if((m.primitive_flags&world_primitive_track_overlay_support_flag)&&std::abs(dot(n,up))>0.8F)return Surface::road;
        return Surface::track;
    }return Surface::legacy;
}
void push_caster(std::vector<ShadowVertex>& out,const WorldDrawCommand& c,const WorldMaterial& m){
    // GT2's course billboards are camera-facing render impostors: their quad
    // plane is rebuilt from the current camera yaw before projection. Feeding
    // that view-dependent plane to a directional shadow map makes a stationary
    // tree/sign shadow rotate when only the camera changes. Until the capture
    // supplies source-stable physical billboard geometry, preserve the visible
    // billboard but do not treat its display-facing quad as sun-caster geometry.
    if(!physical(c,m)||(m.primitive_flags&(2U|world_primitive_sun_caster_replaced_flag|
        world_primitive_track_billboard_depth_flag|world_primitive_track_billboard_flag))||
        (m.primitive_flags&world_primitive_track_overlay_layer_mask))return;
    for(const auto& v:c.vertices){Vec3 p=position(v,c.lighting_depth_scale);if(!finite(p))return;}
    for(const auto& v:c.vertices){auto p=position(v,c.lighting_depth_scale);ShadowVertex s;
        s.position[0]=p.x;s.position[1]=p.y;s.position[2]=p.z;s.uv[0]=v.u;s.uv[1]=v.v;
        s.texture_page=m.texture_page;s.clut=m.clut;s.flags=m.primitive_flags;
        s.mask_x=m.texture_mask_x;s.mask_y=m.texture_mask_y;s.offset_x=m.texture_offset_x;s.offset_y=m.texture_offset_y;out.push_back(s);}
}
bool supported_overlay(const WorldDrawList& list,std::size_t index){
    const auto& c=list.commands[index];const auto& m=list.materials[c.material_index];
    if(!(m.primitive_flags&world_primitive_track_overlay_layer_mask)||(m.primitive_flags&world_primitive_track_replacement_flag))return false;
    Vec3 p[3];for(int j=0;j<3;++j)p[j]=position(c.vertices[j],c.lighting_depth_scale);
    for(std::size_t i=0;i<list.commands.size();++i){if(i==index)continue;const auto& base=list.commands[i];
        if(base.object_kind!=1||base.channel!=c.channel||base.lighting_depth_scale<=0)continue;
        const auto& bm=list.materials[base.material_index];
        if((bm.primitive_flags&(2U|world_primitive_track_overlay_layer_mask))||!(bm.primitive_flags&world_primitive_track_overlay_support_flag))continue;
        Vec3 a=position(base.vertices[0],base.lighting_depth_scale),b=position(base.vertices[1],base.lighting_depth_scale)-a,d=position(base.vertices[2],base.lighting_depth_scale)-a;
        Vec3 n=normalized(cross(b,d));float bb=dot(b,b),bd=dot(b,d),dd=dot(d,d),den=bb*dd-bd*bd;if(den<=1e-12F)continue;
        bool inside=true;for(auto x:p){Vec3 q=x-a;float v=(dot(q,b)*dd-dot(q,d)*bd)/den,w=(dot(q,d)*bb-dot(q,b)*bd)/den;
            if(v<-.002F||w<-.002F||v+w>1.002F||std::abs(dot(q,n))>std::sqrt(std::max(bb,dd))*.002F){inside=false;break;}}
        if(inside)return true;
    }return false;
}
} // namespace

bool parse_script(const std::string& text,Database* out,std::string* error) noexcept{
    if(!out){if(error)*error="null database";return false;}
    try{Parser p(text);Database db;std::set<std::string> names;bool settings=false;
        while(!p.end()){auto type=p.next();std::set<std::string> seen;
            if(type=="settings"){
                if(settings)p.fail("duplicate settings block");
                settings=true;p.expect("{");
                for(auto k=p.next();k!="}";k=p.next()){unique(p,seen,k);
                    if(k=="enabled")db.settings.enabled=p.flag();else if(k=="shadows")db.settings.shadows=p.flag();
                    else if(k=="shadowResolution"){unsigned n=p.integer(256,4096);if(n&(n-1))p.fail("shadowResolution must be power of two");db.settings.shadow_resolution=n;}
                    else if(k=="minimumSunConfidence")db.settings.minimum_confidence=p.number(0.5F,1);
                    else if(k=="normalCrease")db.settings.normal_crease_degrees=p.number(0,85);
                    else if(k=="shadowBias")db.settings.shadow_bias=p.number(0.00001F,0.02F);
                    else if(k=="nearShadowCarSpans")db.settings.near_shadow_car_spans=p.number(2,72);
                    else if(k=="localReflections")db.settings.local_reflections=p.flag();
                    else if(k=="reflectionResolution"){unsigned n=p.integer(32,512);if(n&(n-1))p.fail("reflectionResolution must be power of two");db.settings.reflection_resolution=n;}
                    else if(k=="maximumReflectionProbes")db.settings.maximum_reflection_probes=p.integer(1,8);
                    else if(k=="forceTrack")db.settings.force_track=p.name();else p.fail("unknown settings directive '"+k+"'");}
            }else if(type=="material"){
                Material m;m.name=p.name();if(!names.insert("m/"+m.name).second)p.fail("duplicate material");p.expect("{");
                for(auto k=p.next();k!="}";k=p.next()){unique(p,seen,k);
                    if(k=="surface")m.surface=Parser::surface(p.next());else if(k=="track")m.track=p.name();else if(k=="primitiveKey")m.primitive_key=p.key();
                    else if(k=="sourceMeshKey")m.source_mesh_key=p.key();
                    else if(k=="sourcePrimitiveAddress"){auto address=p.key();if(address>0xffffffffULL)p.fail("sourcePrimitiveAddress exceeds 32 bits");m.source_primitive_address=static_cast<std::uint32_t>(address);}
                    else if(k=="residentContentKey")m.resident_content_key=p.key();
                    else if(k=="texturePage")m.texture_page=static_cast<int>(p.integer(0,65535));else if(k=="clut")m.clut=static_cast<int>(p.integer(0,65535));
                    else if(k=="uvRect"){for(auto& x:m.uv_rect)x=static_cast<int>(p.integer(0,255));if(m.uv_rect[0]>m.uv_rect[2]||m.uv_rect[1]>m.uv_rect[3])p.fail("inverted uvRect");}
                    else if(k=="minimumUpNormal")m.minimum_up_normal=p.number(0,1);
                    else if(k=="roughness")m.roughness=p.number(0.06F,1);else if(k=="specular")m.specular=p.number(0,1);
                    else if(k=="reflection")m.reflection=p.number(0,1);else if(k=="clearcoat")m.clearcoat=p.number(0,1);
                    else if(k=="clearcoatRoughness")m.clearcoat_roughness=p.number(0.06F,1);else if(k=="diffuseMix")m.diffuse_mix=p.number(0,1);
                    else if(k=="receiveShadows")m.receive_shadows=p.flag()?1.0F:0.0F;else if(k=="castShadows")m.cast_shadows=p.flag()?1.0F:0.0F;
                    else if(k=="bakedGain")m.baked_gain=p.vector(0.25F,4);else if(k=="removeShadowOverlay")m.remove_shadow_overlay=p.flag();else p.fail("unknown material directive '"+k+"'");}
                if(!seen.count("surface"))p.fail("material needs surface");
                if(m.uv_rect[0]>=0&&(m.track.empty()||m.texture_page<0||m.clut<0))p.fail("uvRect requires track, texturePage and clut");
                if((m.source_mesh_key!=0)!=(m.source_primitive_address!=0))p.fail("sourceMeshKey and sourcePrimitiveAddress must be paired");
                if((m.source_mesh_key||m.source_primitive_address)&&m.track.empty())p.fail("source primitive binding requires exact track");
                if(m.resident_content_key&&(!m.source_mesh_key||!m.source_primitive_address||m.track.empty()))
                    p.fail("residentContentKey requires exact track and source primitive binding");
                if((m.remove_shadow_overlay||m.baked_gain.x!=1||m.baked_gain.y!=1||m.baked_gain.z!=1)&&
                    (m.track.empty()||!m.primitive_key||!m.source_mesh_key||!m.source_primitive_address||!m.resident_content_key))
                    p.fail("unbaking requires exact track, primitiveKey, sourceMeshKey, sourcePrimitiveAddress and residentContentKey");
                db.materials.push_back(std::move(m));
            }else if(type=="track"){
                Track t;t.name=p.name();if(!names.insert("t/"+t.name).second)p.fail("duplicate track");p.expect("{");
                for(auto k=p.next();k!="}";k=p.next()){
                    if(k=="upload"){auto key=p.key();if(std::find(t.uploads.begin(),t.uploads.end(),key)!=t.uploads.end())p.fail("duplicate upload");t.uploads.push_back(key);continue;}
                    unique(p,seen,k);
                    if(k=="minimumMatches")t.minimum_matches=p.integer(2,128);else if(k=="night")t.night=p.flag();
                    else if(k=="provisionalSun")t.provisional_sun=p.flag();
                    else if(k=="sunDirection"){t.sun_direction=normalized(p.vector(-1,1));if(length(t.sun_direction)<.99F||t.sun_direction.y>-.05F)p.fail("sunDirection must be nonzero and above course ground (+Y down)");}
                    else if(k=="sunColor")t.sun_color=p.vector(0,4);else if(k=="skyColor")t.sky_color=p.vector(0,4);else if(k=="groundColor")t.ground_color=p.vector(0,4);
                    else if(k=="sunIntensity")t.sun_intensity=p.number(0,4);else if(k=="sunConfidence")t.confidence=p.number(0,1);
                    else if(k=="ambient")t.ambient=p.number(0,2);else if(k=="shadowStrength")t.shadow_strength=p.number(0,0.8F);
                    else if(k=="contactShadowOpacity")t.contact_shadow_opacity=p.number(0,0.8F);
                    else p.fail("unknown track directive '"+k+"'");}
                if(t.sun_intensity>0&&(!seen.count("sunDirection")||(!t.provisional_sun&&!seen.count("sunConfidence"))))p.fail("solar light needs direction and confidence, or explicit provisionalSun");
                if(t.provisional_sun&&t.confidence!=0)p.fail("provisionalSun cannot claim measured confidence");
                if(!t.uploads.empty()&&t.uploads.size()<t.minimum_matches)p.fail("minimumMatches exceeds upload keys");
                db.tracks.push_back(std::move(t));
            }else p.fail("unknown block '"+type+"'");
        }
        if(db.materials.size()>4096||db.tracks.size()>1024)throw std::runtime_error("too many materials/tracks");
        for(const auto& m:db.materials)if(!m.track.empty()&&!names.count("t/"+m.track))throw std::runtime_error("unknown material track '"+m.track+"'");
        if(!db.settings.force_track.empty()&&!names.count("t/"+db.settings.force_track))throw std::runtime_error("unknown forceTrack");
        *out=std::move(db);if(error)error->clear();return true;
    }catch(const std::exception& e){if(error)*error=e.what();return false;}catch(...){if(error)*error="shader parser allocation failure";return false;}
}

const Database* runtime_database() noexcept{
    // Per rendering thread, no callbacks/locks across D3D deferred contexts.
    struct Cache{std::chrono::steady_clock::time_point next{};std::filesystem::file_time_type stamp{};std::filesystem::path path;Database db;bool valid{},observed{};};
    static thread_local Cache c;
    try{
        auto now=std::chrono::steady_clock::now();if(now<c.next)return c.valid?&c.db:nullptr;c.next=now+std::chrono::seconds(1);
        std::filesystem::path p;
        if(const char* env=std::getenv("OPENGT_LIGHTING_SCRIPT"))p=std::filesystem::u8path(env);
        else{
#if defined(_WIN32)
            wchar_t exe[32768];DWORD n=GetModuleFileNameW(nullptr,exe,32768);if(!n||n==32768)return nullptr;p=std::filesystem::path(std::wstring(exe,n)).parent_path()/L"lighting"/L"lighting.shader";
#else
            p=std::filesystem::current_path()/"lighting"/"lighting.shader";
#endif
        }
        std::error_code ec;auto stamp=std::filesystem::last_write_time(p,ec);
        if(ec){c.valid=false;c.observed=false;return nullptr;} // removing file is the emergency OFF switch
        if(c.observed&&p==c.path&&stamp==c.stamp)return c.valid?&c.db:nullptr;
        c.observed=true;c.stamp=stamp;c.path=p;
        auto size=std::filesystem::file_size(p);if(size>4*1024*1024)throw std::runtime_error("script exceeds 4 MiB");
        std::ifstream in(p,std::ios::binary);if(!in)throw std::runtime_error("cannot open lighting shader");
        std::string text(static_cast<std::size_t>(size),'\0');if(!in.read(text.data(),static_cast<std::streamsize>(size)))throw std::runtime_error("incomplete shader read");
        Database replacement;std::string error;if(!parse_script(text,&replacement,&error))throw std::runtime_error(error);
        c.db=std::move(replacement);c.valid=true;
        std::fprintf(stderr,"[Lighting-L01] loaded %zu materials, %zu tracks; enabled=%u shadows=%u\n",c.db.materials.size(),c.db.tracks.size(),c.db.settings.enabled?1U:0U,c.db.settings.shadows?1U:0U);
    }catch(const std::exception& e){std::fprintf(stderr,"[Lighting-L01] keeping last valid shader: %s\n",e.what());}catch(...){return c.valid?&c.db:nullptr;}
    return c.valid?&c.db:nullptr;
}
bool requested() noexcept {
    const auto* d=runtime_database();
    if(!d||!d->settings.enabled)return false;
    for(const auto& t:d->tracks) {
        const bool selected=d->settings.force_track.empty()?!t.uploads.empty():t.name==d->settings.force_track;
        if(selected&&!t.night&&(d->settings.local_reflections||
            (d->settings.shadows&&t.sun_intensity>0&&(t.provisional_sun||t.confidence>=d->settings.minimum_confidence))))return true;
    }
    // Local environment capture also needs the retained off-camera geometry.
    return false;
}
const Track* select_track(const Database& d,const std::vector<std::uint64_t>& uploads) noexcept {
    if(!d.settings.force_track.empty()) {
        for(const auto& t:d.tracks)if(t.name==d.settings.force_track)return &t;
        return nullptr;
    }
    try {
        // Uploads are small but each course can own hundreds of source keys.
        // Binary search avoids a profile x bank x live-upload scan every frame.
        auto keys=uploads;std::sort(keys.begin(),keys.end());
        keys.erase(std::unique(keys.begin(),keys.end()),keys.end());
        const Track* best=nullptr;unsigned score=0;bool tie=false;
        for(const auto& t:d.tracks) {
            if(t.uploads.empty())continue;
            unsigned matches=0;
            for(auto k:t.uploads)if(std::binary_search(keys.begin(),keys.end(),k))++matches;
            if(matches<t.minimum_matches)continue;
            if(matches>score){best=&t;score=matches;tie=false;}else if(matches==score)tie=true;
        }
        return tie?nullptr:best; // never guess between two bank-identical courses
    } catch(...) {return nullptr;}
}
const Material* select_material(const Database& d,Surface role,const std::string& track,std::uint64_t key,const WorldMaterial& source,const WorldDrawCommand* command,float normal_up) noexcept{
    const Material* best=nullptr;int score=-1;
    for(const auto& m:d.materials){if(m.surface!=role||(!m.track.empty()&&m.track!=track)||(m.primitive_key&&m.primitive_key!=key)||(m.texture_page>=0&&m.texture_page!=source.texture_page)||(m.clut>=0&&m.clut!=source.clut))continue;
        if(m.source_mesh_key||m.source_primitive_address){if(!command||command->source_mesh_key!=m.source_mesh_key||command->source_primitive_address!=m.source_primitive_address)continue;}
        if(m.resident_content_key&&(!command||command->resident_content_key!=m.resident_content_key))continue;
        if(m.minimum_up_normal>normal_up)continue;
        if(m.uv_rect[0]>=0){if(!command)continue;bool inside=true;
            for(const auto& v:command->vertices)if(!std::isfinite(v.u)||!std::isfinite(v.v)||v.u<m.uv_rect[0]||v.v<m.uv_rect[1]||v.u>m.uv_rect[2]||v.v>m.uv_rect[3])inside=false;
            if(!inside)continue;}
        int s=(m.uv_rect[0]>=0?32:0)+(!m.track.empty()?8:0)+(m.primitive_key?16:0)+(m.source_mesh_key?64:0)+(m.resident_content_key?128:0)+(m.texture_page>=0?2:0)+(m.clut>=0?4:0);
        if(s>=score){best=&m;score=s;}}
    return best;
}
std::uint64_t primitive_key(const WorldDrawCommand& c,const WorldMaterial& m) noexcept{
    // Source geometry/UV/material, independent of guest pointers, camera and
    // authored baked RGB (which is the quantity a rule can remove).
    std::uint64_t h=14695981039346656037ULL;auto add=[&](std::uint32_t x){for(int i=0;i<4;++i){h^=(x>>(8*i))&255U;h*=1099511628211ULL;}};
    add(c.object_kind);add(m.primitive_flags&7U);add(m.texture_page);add(m.clut);
    std::array<std::array<std::int32_t,5>,3> v{};
    for(int i=0;i<3;++i) {
        const auto& source=c.vertices[i];
        // Invalid UVs must not trigger undefined float-to-integer conversion
        // or accidentally match an exact source-primitive unbaking rule.
        if(!std::isfinite(source.u)||!std::isfinite(source.v)||
           std::abs(source.u)>1048576.0F||std::abs(source.v)>1048576.0F)return 0;
        v[i]={source.model_x,source.model_y,source.model_z,
              static_cast<std::int32_t>(source.u),static_cast<std::int32_t>(source.v)};
    }
    std::sort(v.begin(),v.end());for(auto a:v)for(auto x:a)add(static_cast<std::uint32_t>(x));return h;
}

UnbakeAuditRecord inspect_unbake_candidate(const WorldDrawCommand& c,
    const WorldMaterial& m,Surface role,float normal_up) noexcept{
    UnbakeAuditRecord out;out.surface=role;out.source_mesh_key=c.source_mesh_key;
    out.source_primitive_address=c.source_primitive_address;out.resident_content_key=c.resident_content_key;
    out.primitive_flags=m.primitive_flags;
    out.object_id=c.object_id;out.model_pointer=c.model_pointer;out.texture_page=m.texture_page;out.clut=m.clut;
    out.normal_up=normal_up;
    if((role!=Surface::road&&role!=Surface::track)||c.object_kind!=1||c.channel!=WorldViewChannel::main_view||
       !c.source_mesh_key||!c.source_primitive_address||!c.resident_content_key||
       !(m.primitive_flags&world_primitive_resident_course_flag)||
       !(m.primitive_flags&1U)||(m.primitive_flags&2U)||
       (m.primitive_flags&(world_primitive_track_billboard_depth_flag|world_primitive_track_billboard_flag))||
       !std::isfinite(normal_up)||normal_up<.96F)return out;
    const auto& first=c.vertices[0];
    for(int i=0;i<3;++i){const auto& v=c.vertices[i];
        if(v.r!=first.r||v.g!=first.g||v.b!=first.b||!std::isfinite(v.u)||!std::isfinite(v.v)||
           std::abs(v.u)>1048576.0F||std::abs(v.v)>1048576.0F)return out;
        out.uv[i*2]=v.u;out.uv[i*2+1]=v.v;
    }
    out.color={first.r,first.g,first.b};out.primitive_key=primitive_key(c,m);
    Vec3 world[3];
    for(int i=0;i<3;++i){
        world[i]={c.vertices[i].world_x,c.vertices[i].world_y,c.vertices[i].world_z};
        if(!finite(world[i]))return out;
    }
    const Vec3 ab=world[1]-world[0],ac=world[2]-world[0],bc=world[2]-world[1];
    const Vec3 worldNormal=normalized(cross(ab,ac));
    out.world_scale=std::max({length(ab),length(ac),length(bc)});
    if(length(worldNormal)<.99F||!std::isfinite(out.world_scale)||out.world_scale<=1e-6F)return out;
    out.world_centroid=(world[0]+world[1]+world[2])*(1.0F/3.0F);
    out.world_normal=worldNormal;
    out.eligible=out.primitive_key!=0;return out;
}

namespace {
const char* audit_surface_name(Surface surface) noexcept{
    switch(surface){case Surface::road:return "road";case Surface::track:return "track";
        case Surface::car:return "car";case Surface::rubber:return "rubber";default:return "legacy";}
}
std::uint16_t audit_view_bucket(Vec3 forward) noexcept {
    forward=normalized(forward);
    if(length(forward)<.99F)return 0xffffU;
    const float yaw=std::atan2(forward.x,forward.z);
    const float pitch=std::asin(std::clamp(forward.y,-1.0F,1.0F));
    int yaw_bin=static_cast<int>(std::floor((yaw+pi)/(2*pi)*24.0F));
    int pitch_bin=static_cast<int>(std::floor((pitch+pi*.5F)/pi*12.0F));
    yaw_bin=(yaw_bin%24+24)%24;pitch_bin=std::clamp(pitch_bin,0,11);
    return static_cast<std::uint16_t>(pitch_bin*24+yaw_bin);
}
void emit_unbake_audit(const std::string& track,const WorldDrawList& list,const UnbakeAuditRecord& record,Vec3 camera_forward){
    // Diagnostic only. Keep the observation state bounded and emit at most
    // three records for one exact authored-color/content variant: first sighting,
    // short-span confirmation, and long-span confirmation. Each state also
    // accumulates coarse source-camera orientation diversity. The offline
    // analyzer can therefore reject a dark primitive seen only in one narrow
    // view without turning every visible triangle on every frame into log traffic.
    using AuditKey=std::tuple<std::string,std::uint64_t,std::uint32_t,std::uint64_t,std::uint32_t,std::uint64_t>;
    struct AuditState {
        std::uint64_t first_frame{};
        std::uint64_t last_distinct_frame{};
        std::uint32_t distinct_frames{};
        std::uint8_t emitted_stage{};
        Vec3 first_forward{};
        float maximum_view_angle{};
        std::array<std::uint16_t,16> view_buckets{};
        std::uint8_t view_bucket_count{};
        Vec3 first_world_centroid{};
        Vec3 first_world_normal{};
        float first_world_scale{};
        float maximum_world_centroid_drift{};
        float maximum_world_plane_offset{};
        float maximum_world_plane_angle{};
    };
    static thread_local std::map<AuditKey,AuditState> observations;
    static thread_local std::size_t emitted_records{};
    static thread_local bool truncated=false;
    static thread_local bool have_last_frame=false;
    static thread_local std::uint64_t last_frame{};
    constexpr std::size_t maximum_records=250000;
    constexpr std::uint32_t short_frames=3;
    constexpr std::uint64_t short_span=30;
    constexpr std::uint32_t long_frames=8;
    constexpr std::uint64_t long_span=180;
    const bool enabled=std::getenv("OPENGT_LIGHTING_UNBAKE_AUDIT")!=nullptr;
    if(!enabled){
        observations.clear();emitted_records=0;truncated=false;have_last_frame=false;last_frame=0;
        return;
    }
    if(!record.eligible)return;
    // A backwards frame stream marks a new diagnostic session on this thread.
    // Never let persistence evidence leak across an explicit restart/reset.
    if(have_last_frame&&list.frame_index<last_frame){
        observations.clear();emitted_records=0;truncated=false;
    }
    have_last_frame=true;last_frame=list.frame_index;
    const std::uint32_t authoredColor=static_cast<std::uint32_t>(record.color[0])|
        (static_cast<std::uint32_t>(record.color[1])<<8)|(static_cast<std::uint32_t>(record.color[2])<<16);
    const AuditKey key{track,record.source_mesh_key,record.source_primitive_address,record.primitive_key,authoredColor,record.resident_content_key};
    auto it=observations.find(key);
    bool first=false;
    if(it==observations.end()){
        if(emitted_records>=maximum_records){if(!truncated){truncated=true;std::fprintf(stderr,
            "[Lighting-Unbake-Audit] truncated=1 maximumRecords=%zu\n",maximum_records);}return;}
        AuditState state;state.first_frame=list.frame_index;state.last_distinct_frame=list.frame_index;
        state.distinct_frames=1;state.emitted_stage=0;state.first_forward=normalized(camera_forward);
        state.first_world_centroid=record.world_centroid;state.first_world_normal=normalized(record.world_normal);
        state.first_world_scale=record.world_scale;
        it=observations.emplace(key,state).first;first=true;
    }else if(list.frame_index!=it->second.last_distinct_frame){
        it->second.last_distinct_frame=list.frame_index;
        if(it->second.distinct_frames<std::numeric_limits<std::uint32_t>::max())++it->second.distinct_frames;
    }
    auto& state=it->second;
    if(first||list.frame_index==state.last_distinct_frame){
        const float scale=std::max({state.first_world_scale,record.world_scale,1e-6F});
        const Vec3 centroid_delta=record.world_centroid-state.first_world_centroid;
        state.maximum_world_centroid_drift=std::max(state.maximum_world_centroid_drift,length(centroid_delta)/scale);
        state.maximum_world_plane_offset=std::max(state.maximum_world_plane_offset,
            std::abs(dot(state.first_world_normal,centroid_delta))/scale);
        const Vec3 world_normal=normalized(record.world_normal);
        if(length(world_normal)>.99F&&length(state.first_world_normal)>.99F){
            const float world_cosine=std::clamp(std::abs(dot(state.first_world_normal,world_normal)),0.0F,1.0F);
            state.maximum_world_plane_angle=std::max(state.maximum_world_plane_angle,std::acos(world_cosine)*180.0F/pi);
        }
        const Vec3 current=normalized(camera_forward);
        if(length(current)>.99F){
            if(length(state.first_forward)<.99F)state.first_forward=current;
            const float cosine=std::clamp(dot(state.first_forward,current),-1.0F,1.0F);
            state.maximum_view_angle=std::max(state.maximum_view_angle,std::acos(cosine)*180.0F/pi);
            const std::uint16_t bucket=audit_view_bucket(current);
            bool known=false;for(std::uint8_t j=0;j<state.view_bucket_count;++j)known=known||state.view_buckets[j]==bucket;
            if(!known&&bucket!=0xffffU&&state.view_bucket_count<state.view_buckets.size())
                state.view_buckets[state.view_bucket_count++]=bucket;
        }
    }
    const std::uint64_t span=list.frame_index>=state.first_frame?list.frame_index-state.first_frame:0;
    int stage=-1;
    if(first)stage=0;
    else if(state.emitted_stage<1&&state.distinct_frames>=short_frames&&span>=short_span)stage=1;
    else if(state.emitted_stage<2&&state.distinct_frames>=long_frames&&span>=long_span)stage=2;
    if(stage<0)return;
    if(emitted_records>=maximum_records){if(!truncated){truncated=true;std::fprintf(stderr,
        "[Lighting-Unbake-Audit] truncated=1 maximumRecords=%zu\n",maximum_records);}return;}
    state.emitted_stage=static_cast<std::uint8_t>(stage);++emitted_records;
    std::fprintf(stderr,
        "[Lighting-Unbake-Audit] track=%s frame=%llu poll=%d surface=%s primitiveKey=0x%016llx "
        "sourceMeshKey=0x%016llx sourcePrimitiveAddress=0x%08x residentContentKey=0x%016llx "
        "object=%u model=0x%08x page=%u clut=%u flags=0x%08x normalUp=%.6f "
        "uv=%.3f,%.3f;%.3f,%.3f;%.3f,%.3f rgb=%u,%u,%u auditStage=%d seenFrames=%u "
        "firstFrame=%llu spanFrames=%llu viewBuckets=%u maxViewAngle=%.3f "
        "maxWorldCentroidDrift=%.6f maxWorldPlaneOffset=%.6f maxWorldPlaneAngle=%.3f cameraTransform=0x%016llx\n",
        track.c_str(),static_cast<unsigned long long>(list.frame_index),list.input_poll,audit_surface_name(record.surface),
        static_cast<unsigned long long>(record.primitive_key),static_cast<unsigned long long>(record.source_mesh_key),
        record.source_primitive_address,static_cast<unsigned long long>(record.resident_content_key),record.object_id,record.model_pointer,
        record.texture_page,record.clut,record.primitive_flags,record.normal_up,record.uv[0],record.uv[1],record.uv[2],record.uv[3],record.uv[4],record.uv[5],
        static_cast<unsigned>(record.color[0]),static_cast<unsigned>(record.color[1]),static_cast<unsigned>(record.color[2]),
        stage,state.distinct_frames,static_cast<unsigned long long>(state.first_frame),static_cast<unsigned long long>(span),
        static_cast<unsigned>(state.view_bucket_count),state.maximum_view_angle,state.maximum_world_centroid_drift,
        state.maximum_world_plane_offset,state.maximum_world_plane_angle,static_cast<unsigned long long>(list.camera_transform_id));
}
} // namespace
SunFit fit_sun(const std::vector<ShadowEvidence>& input,float tolerance) noexcept{
    SunFit out;if(input.size()>100000||!std::isfinite(tolerance)||tolerance<=0||tolerance>30)return out;
    try{struct Ray{Vec3 n;float w;std::uint32_t id;};std::vector<Ray> rays;std::map<std::uint32_t,float> totals;
        for(const auto& e:input){Vec3 n=normalized(e.caster-e.shadow);if(!finite(e.caster)||!finite(e.shadow)||!std::isfinite(e.weight)||e.weight<=0||n.y>-.05F||length(n)<.99F)continue;rays.push_back({n,e.weight,e.object});totals[e.object]+=e.weight;}
        if(rays.size()<3)return out;
        for(auto& r:rays)r.w/=totals[r.id]; // each independent caster has equal influence
        const float threshold=std::cos(tolerance*pi/180);float best=0;Vec3 seed{};
        // Deterministic bounded hypothesis set prevents quadratic abuse.
        const std::size_t step=std::max<std::size_t>(1,rays.size()/256);
        for(std::size_t i=0;i<rays.size();i+=step){float score=0;for(auto r:rays)if(dot(r.n,rays[i].n)>=threshold)score+=r.w;if(score>best){best=score;seed=rays[i].n;}}
        Vec3 sum{};for(auto r:rays)if(dot(r.n,seed)>=threshold)sum=sum+r.n*r.w;out.direction=normalized(sum);
        std::set<std::uint32_t> ids;float square=0,weight=0;
        for(auto r:rays){float cosine=std::clamp(dot(r.n,out.direction),-1.0F,1.0F);if(cosine<threshold)continue;
            float a=std::acos(cosine)*180/pi;square+=a*a*r.w;weight+=r.w;++out.inliers;ids.insert(r.id);}
        out.independent_objects=static_cast<unsigned>(ids.size());out.rms_degrees=weight>0?std::sqrt(square/weight):180;
        out.confidence=std::min(1.0F,float(ids.size())/3.0F)*(weight/float(totals.size()))*std::exp(-out.rms_degrees/tolerance);
        out.accepted=ids.size()>=3&&out.confidence>=.75F;return out;
    }catch(...){return {};}
}

Frame prepare(const WorldDrawList& list,const Database& db,const Track* track,const std::vector<Surface>& roles,ShadowGridState* grid){
    const auto sourceCamera=shadow_camera_transform(list);
    if(grid && (!sourceCamera.valid||!list.continuous_projection||!list.track_commands||!db.settings.enabled||!db.settings.shadows||!track||track->night||
        grid->track!=track->name||list.frame_index<grid->frame_index)) *grid={};
    if(grid&&sourceCamera.valid&&track){grid->track=track->name;grid->frame_index=list.frame_index;}
    Frame f;
    if(!db.settings.enabled||!track||track->night||!list.continuous_projection||list.track_commands==0)return f;
    Basis camera=basis(list);if(!camera.valid)return f;
    f.vertices.resize(list.commands.size()*3);f.materials.resize(list.commands.size());
    f.enabled=true;f.track=track->name;f.solar=(track->provisional_sun||track->confidence>=db.settings.minimum_confidence)&&track->sun_intensity>0;
    f.provisional_sun=track->provisional_sun;
    const bool accepted_sun=f.solar&&!track->provisional_sun;
    Vec3 light=normalized(camera.apply(track->sun_direction)),up=normalized(camera.apply({0,-1,0}));
    f.constants.sun={light.x,light.y,light.z,f.solar?track->sun_intensity:0};
    f.constants.color={track->sun_color.x,track->sun_color.y,track->sun_color.z,track->shadow_strength};
    f.constants.sky={track->sky_color.x,track->sky_color.y,track->sky_color.z,1};
    f.constants.ground={track->ground_color.x,track->ground_color.y,track->ground_color.z,0};
    f.constants.up={up.x,up.y,up.z,track->ambient};
    std::vector<Vec3> normals(list.commands.size());std::vector<float> areas(list.commands.size());
    using NormalKey=std::tuple<std::uint32_t,std::uint32_t,std::uint32_t>;
    std::map<NormalKey,std::vector<std::size_t>> neighbors;
    struct Bounds{Vec3 lo{1e30F,1e30F,1e30F},hi{-1e30F,-1e30F,-1e30F};};std::map<std::uint32_t,Bounds> cars;
    for(std::size_t i=0;i<list.commands.size();++i){const auto& c=list.commands[i];if(c.material_index>=list.materials.size())continue;const auto& m=list.materials[c.material_index];if(!physical(c,m))continue;
        Vec3 p[3];bool good=true;for(int j=0;j<3;++j){p[j]=position(c.vertices[j],c.lighting_depth_scale);good=good&&finite(p[j]);}if(!good)continue;
        Vec3 n=cross(p[1]-p[0],p[2]-p[0]);areas[i]=length(n);n=normalized(n);if(dot(n,p[0])>0)n=n*-1;normals[i]=n;
        for(int j=0;j<3;++j){f.vertices[i*3+j]={p[j],n};if(c.object_kind==2&&!(m.primitive_flags&2U)&&c.vertices[j].source_vertex_identity)
            neighbors[{c.object_id,c.model_pointer,c.vertices[j].source_vertex_identity}].push_back(i);}
        Surface role=i<roles.size()?roles[i]:infer_surface(c,m,n,up);
        const float normalUp=std::abs(dot(n,up));
        const Material* mat=select_material(db,role,track->name,primitive_key(c,m),m,&c,normalUp);
        emit_unbake_audit(track->name,list,inspect_unbake_candidate(c,m,role,normalUp),camera.r[2]);
        const bool camera_facing_billboard=(m.primitive_flags&
            (world_primitive_track_billboard_depth_flag|world_primitive_track_billboard_flag))!=0;
        // GT2's course billboard normal is a render-facing normal rebuilt from
        // the current camera yaw, not an authored physical surface normal.
        // Lighting that plane would make a stationary tree/sign change diffuse
        // and specular response when only the camera rotates. Preserve the
        // authored billboard color until source-stable physical orientation is
        // available; the visible card still renders through the normal world
        // pass, while push_caster() independently suppresses its fake sun plane.
        if(mat&&role!=Surface::legacy&&!(m.primitive_flags&2U)&&!camera_facing_billboard&&length(n)>.9F){auto& dest=f.materials[i];
            dest.surface={mat->roughness,mat->specular,mat->reflection,1};dest.coat={mat->clearcoat,mat->clearcoat_roughness,mat->diffuse_mix,mat->receive_shadows};
            if(accepted_sun)dest.gain={mat->baked_gain.x,mat->baked_gain.y,mat->baked_gain.z,0};
            // Setting removal is deferred until a usable shadow atlas exists.
            if(accepted_sun&&db.settings.shadows&&mat->remove_shadow_overlay&&supported_overlay(list,i))dest.gain[3]=1;
        }
        if(f.solar&&db.settings.shadows&&(!mat||mat->cast_shadows>0))push_caster(f.casters,c,m);
        if(c.object_kind==2&&!(m.primitive_flags&2U)){auto& b=cars[c.object_id];for(auto x:p){b.lo={std::min(b.lo.x,x.x),std::min(b.lo.y,x.y),std::min(b.lo.z,x.z)};b.hi={std::max(b.hi.x,x.x),std::max(b.hi.y,x.y),std::max(b.hi.z,x.z)};}}
    }
    float crease=std::cos(db.settings.normal_crease_degrees*pi/180);
    for(std::size_t i=0;i<list.commands.size();++i){const auto& c=list.commands[i];if(c.object_kind!=2||f.materials[i].surface[3]==0)continue;
        for(int j=0;j<3;++j){auto id=c.vertices[j].source_vertex_identity;if(!id)continue;auto it=neighbors.find({c.object_id,c.model_pointer,id});if(it==neighbors.end())continue;
            Vec3 sum{};for(auto k:it->second)if(f.materials[k].surface==f.materials[i].surface&&f.materials[k].coat==f.materials[i].coat&&dot(normals[i],normals[k])>=crease)sum=sum+normals[k]*areas[k];
            if(length(sum)>1e-12F)f.vertices[i*3+j].normal=normalized(sum);}
    }
    if(f.solar&&db.settings.shadows){for(const auto& x:list.lighting_extra_casters){
        Vec3 n=normalized(cross(position(x.command.vertices[1],x.command.lighting_depth_scale)-position(x.command.vertices[0],x.command.lighting_depth_scale),position(x.command.vertices[2],x.command.lighting_depth_scale)-position(x.command.vertices[0],x.command.lighting_depth_scale)));
        auto role=infer_surface(x.command,x.material,n,up);auto* mat=select_material(db,role,track->name,primitive_key(x.command,x.material),x.material,&x.command,std::abs(dot(n,up)));
        if(!mat||mat->cast_shadows>0)push_caster(f.casters,x.command,x.material);
    }}
    f.shadows=f.solar&&db.settings.shadows&&!f.casters.empty();
    if(!f.shadows){for(auto& m:f.materials)m.gain[3]=0;return f;}
    // Prefer the complete, visibly anchored body's SOURCE-space extent. A
    // camera-space AABB changes width when the camera/car turns; sizing both
    // cascades from that box makes every stationary track shadow "breathe".
    // Include L06's source-only faces, keep separate rigid transforms (wheels
    // must not expand the body in a different local frame), and retain the old
    // fallback for captures without exact source transforms.
    struct SourceBody { Bounds local; float scale{}; float nearest{1e30F};
        bool visible{}, hidden{}; unsigned vertices{}; };
    using BodyKey=std::tuple<std::uint32_t,std::uint32_t,std::uint64_t>;
    std::map<BodyKey,SourceBody> sourceBodies;
    auto sourceBody=[&](const WorldDrawCommand& c,const WorldMaterial& m,bool visible){
        if(!physical(c,m)||c.object_kind!=2||(m.primitive_flags&2U)||
            !c.exact_transform_valid)return;
        float largestColumn=0;
        for(unsigned k=0;k<3;++k){
            Vec3 column{float(c.transform_rotation[k]),float(c.transform_rotation[3+k]),float(c.transform_rotation[6+k])};
            // Undo the actual primary camera scale/shear before measuring a
            // source model. An orthogonalized camera is not its inverse.
            if(sourceCamera.valid)column=sourceCamera.vector_to_world(column);
            largestColumn=std::max(largestColumn,length(column));}
        const float scale=largestColumn*(c.lighting_depth_scale/4096.0F);
        if(!std::isfinite(scale)||scale<=0)return;
        auto& b=sourceBodies[{c.object_id,c.model_pointer,c.transform_id}];
        b.visible|=visible;b.hidden|=(m.primitive_flags&world_primitive_shadow_only_flag)!=0;
        b.scale=std::max(b.scale,scale);
        for(const auto& v:c.vertices){Vec3 x{float(v.model_x),float(v.model_y),float(v.model_z)};
            b.local.lo={std::min(b.local.lo.x,x.x),std::min(b.local.lo.y,x.y),std::min(b.local.lo.z,x.z)};
            b.local.hi={std::max(b.local.hi.x,x.x),std::max(b.local.hi.y,x.y),std::max(b.local.hi.z,x.z)};
            b.nearest=std::min(b.nearest,v.view_z*c.lighting_depth_scale);++b.vertices;
        }
    };
    for(const auto& c:list.commands)if(c.material_index<list.materials.size())sourceBody(c,list.materials[c.material_index],true);
    for(const auto& x:list.lighting_extra_casters)sourceBody(x.command,x.material,false);
    struct BodySize {float span{},nearest{};};std::map<std::uint32_t,BodySize> bodySizes;
    for(const auto& pair:sourceBodies){const auto& b=pair.second;
        // Source-only faces are the evidence of a completed selected body,
        // not a cached guess at wholly off-camera or unanchored vehicles.
        if(!b.visible||!b.hidden||b.vertices<6)continue;
        const Vec3 e=b.local.hi-b.local.lo;const float size=std::max({e.x,e.y,e.z})*b.scale;
        auto& best=bodySizes[std::get<0>(pair.first)];
        if(size>best.span)best={size,b.nearest};
    }
    // The common OT-derived coordinate unit is arbitrary. Size the atlas from
    // the nearest complete vehicle, not its independently normalized GTE Z.
    float span=0,nearest=std::numeric_limits<float>::max();
    for(auto [id,b]:cars){(void)id;Vec3 extent=b.hi-b.lo;float s=std::max({extent.x,extent.y,extent.z});Vec3 center=(b.lo+b.hi)*.5F;
        if(s>1&&center.z>0&&center.z<nearest){nearest=center.z;span=s;}}
    float sourceNearest=std::numeric_limits<float>::max(),sourceSpan=0;
    for(const auto& pair:bodySizes){const auto& b=pair.second;
        if(b.span>1&&b.nearest>0&&b.nearest<sourceNearest){sourceNearest=b.nearest;sourceSpan=b.span;}}
    if(sourceSpan>0)span=sourceSpan;
    // Lock only a COMPLETE source body, not a depth heuristic or partial box.
    // Keep that world scale while its scene runs, even if another car becomes
    // the closest candidate. Live positions always come from this frame.
    if(sourceCamera.valid&&grid){
        if(grid->source_span<=0&&sourceSpan>0)grid->source_span=sourceSpan;
        if(grid->source_span>0)span=grid->source_span;
    }
    if(span<=0){std::vector<float> depths;for(const auto& v:f.vertices)if(v.position.z>16)depths.push_back(v.position.z);if(depths.empty()){f.shadows=false;return f;}
        auto mid=depths.begin()+depths.size()/2;std::nth_element(depths.begin(),mid,depths.end());span=std::max(1.0F,*mid/32);}
    const float near_range=span*db.settings.near_shadow_car_spans,far_range=near_range*4;
    f.constants.shadow_config={near_range,far_range,1.0F/db.settings.shadow_resolution,db.settings.shadow_bias};
    // Define the light-plane axes from the world/script sun, then transform
    // them with the camera. Rebuilding axes from view-up rotates the shadow
    // texel grid on camera tilt/roll and can abruptly flip its fallback axis.
    const Vec3 worldLight=normalized(track->sun_direction);
    const Vec3 worldU=normalized(cross(std::abs(worldLight.y)<.9F?Vec3{0,1,0}:Vec3{1,0,0},worldLight));
    const Vec3 worldV=normalized(cross(worldLight,worldU));
    Vec3 u=normalized(camera.apply(worldU)),v=normalized(camera.apply(worldV)),w=light*-1;
    f.world_grid=sourceCamera.valid&&sourceSpan>0;
    if(sourceCamera.valid&&grid&&grid->source_span>0)f.world_grid=true;
    if(f.world_grid){
        u=sourceCamera.world_axis_in_view(worldU);
        v=sourceCamera.world_axis_in_view(worldV);
        w=sourceCamera.world_axis_in_view(worldLight*-1);
    }
    for(unsigned c=0;c<2;++c){float range=c?far_range:near_range;Vec3 center{0,0,range*.45F};float radius=range,depth=range*2;
        f.constants.shadow_rows[c*4+0]={u.x/radius,u.y/radius,u.z/radius,-dot(u,center)/radius};
        f.constants.shadow_rows[c*4+1]={v.x/radius,v.y/radius,v.z/radius,-dot(v,center)/radius};
        if(f.world_grid){
            f.constants.shadow_rows[c*4]=snapped_shadow_row(sourceCamera,worldU,center,radius,db.settings.shadow_resolution);
            f.constants.shadow_rows[c*4+1]=snapped_shadow_row(sourceCamera,worldV,center,radius,db.settings.shadow_resolution);
        }
        const auto& rowU=f.constants.shadow_rows[c*4];
        const auto& rowV=f.constants.shadow_rows[c*4+1];
        // A receiver-centered fixed Z slab clips tall/upstream track geometry
        // even when its sun projection falls inside the receiver atlas. Retain
        // the existing receiver volume and extend only depth to include every
        // resident caster that can touch this cascade's XY/filter footprint.
        float zlo=dot(w,center)-depth,zhi=dot(w,center)+depth;
        const float xyPad=1.0F+6.0F/db.settings.shadow_resolution;
        for(std::size_t i=0;i+2<f.casters.size();i+=3){
            Vec3 lo{1e30F,1e30F,1e30F},hi{-1e30F,-1e30F,-1e30F};
            for(unsigned j=0;j<3;++j){const auto& a=f.casters[i+j].position;
                const Vec3 p{a[0],a[1],a[2]};
                const Vec3 q{rowU[0]*p.x+rowU[1]*p.y+rowU[2]*p.z+rowU[3],
                    rowV[0]*p.x+rowV[1]*p.y+rowV[2]*p.z+rowV[3],dot(w,p)};
                lo={std::min(lo.x,q.x),std::min(lo.y,q.y),std::min(lo.z,q.z)};
                hi={std::max(hi.x,q.x),std::max(hi.y,q.y),std::max(hi.z,q.z)};
            }
            if(hi.x < -xyPad || lo.x > xyPad || hi.y < -xyPad || lo.y > xyPad)continue;
            zlo=std::min(zlo,lo.z);zhi=std::max(zhi,hi.z);
        }
        // Floating-point rasterization must not put an extreme caster exactly
        // on a clip plane. Padding is in the common scene unit, not texel bias.
        const float pad=std::max(radius*.001F,(zhi-zlo)*.00001F);
        zlo-=pad;zhi+=pad;const float inverseDepth=1.0F/(zhi-zlo);
        f.constants.shadow_rows[c*4+2]={w.x*inverseDepth,w.y*inverseDepth,w.z*inverseDepth,-zlo*inverseDepth};
        f.constants.shadow_rows[c*4+3]={0,0,0,1};}
    f.constants.ground[3]=1;
    f.contact_shadow_opacity=track->contact_shadow_opacity;
    return f;
}

Vec3 shade_reference(Vec3 s,Vec3 n,Vec3 view,const DrawMaterial& m,const Constants& c,float visibility) noexcept{
    if(m.surface[3]==0||c.sky[3]==0)return s;
    n=normalized(n);view=normalized(view);if(dot(n,view)<0)n=n*-1;
    Vec3 l=normalized(rgb(c.sun)),h=normalized(l+view);float nl=sat(dot(n,l)),nv=std::max(.001F,sat(dot(n,view))),nh=sat(dot(n,h)),vh=sat(dot(view,h));
    const float raw_visibility=sat(visibility);
    visibility=1-m.coat[3]*(1-raw_visibility);
    const float legacy_visibility=1-c.color[3]*m.coat[3]*(1-raw_visibility);
    auto spec=[&](float rough,float strength){float a=rough*rough,a2=a*a,d=nh*nh*(a2-1)+1;float D=a2/(pi*d*d);float k=(rough+1)*(rough+1)/8;float G=nv/(nv*(1-k)+k)*nl/(nl*(1-k)+k);float F=.04F+.96F*std::pow(1-vh,5.0F);return strength*D*G*F/(4*nv*std::max(.001F,nl))*nl;};
    Vec3 base{linear(s.x*m.gain[0]),linear(s.y*m.gain[1]),linear(s.z*m.gain[2])};
    Vec3 direct=rgb(c.color)*(c.sun[3]*nl*visibility);Vec3 illumination=Vec3{c.up[3],c.up[3],c.up[3]}+direct;
    Vec3 out=multiply(base,Vec3{(1-m.coat[2])*legacy_visibility,(1-m.coat[2])*legacy_visibility,(1-m.coat[2])*legacy_visibility}+illumination*m.coat[2]);
    // Existing baked diffuse is retained by default, but new occluders can
    // darken the sunlit authored layer without blacking out ambient lighting.
    float sunSpec=(spec(m.surface[0],m.surface[1])+spec(m.coat[1],m.coat[0]))*c.sun[3]*visibility;
    out=out+rgb(c.color)*sunSpec;
    Vec3 reflect=n*(2*dot(n,view))-view;float skyWeight=sat(dot(reflect,rgb(c.up))*.5F+.5F);skyWeight=skyWeight*(1-m.surface[0])+.5F*m.surface[0];
    Vec3 environment=rgb(c.ground)*(1-skyWeight)+rgb(c.sky)*skyWeight;
    float fresnel=.04F+.96F*std::pow(1-nv,5.0F);out=out+environment*(m.surface[2]*fresnel*(1-.75F*m.surface[0]));
    return {srgb(out.x),srgb(out.y),srgb(out.z)};
}
} // namespace opengt::render::lighting
