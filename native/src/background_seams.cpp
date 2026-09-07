#include "background_seams.hpp"

#include <algorithm>
#include <array>
#include <cmath>
#include <map>
#include <tuple>
#include <vector>

namespace opengt::render {
namespace {
using Point = std::array<int, 3>;
using Edge = std::pair<Point, Point>;
struct Occurrence { std::size_t command; int edge; };
struct Group {
    std::map<Point, WorldDrawVertex> points;
    std::map<Edge, std::vector<Occurrence>> edges;
};
using GroupKey = std::tuple<std::uint32_t, std::uint32_t, std::uint64_t,
    float, float, float, float, float, int, int, int, int>;

Point point(const WorldDrawVertex& v) { return {v.model_x, v.model_y, v.model_z}; }
Edge edge(Point a, Point b) { return a < b ? Edge{a,b} : Edge{b,a}; }
bool color(const WorldDrawVertex& a, const WorldDrawVertex& b) {
    return a.r == b.r && a.g == b.g && a.b == b.b;
}
bool same_projection(const WorldDrawVertex& a, const WorldDrawVertex& b) {
    return a.transform_id == b.transform_id &&
        std::equal(a.transform_rotation, a.transform_rotation + 9, b.transform_rotation) &&
        std::equal(a.transform_translation, a.transform_translation + 3, b.transform_translation) &&
        a.projection_plane == b.projection_plane &&
        a.projection_offset_x == b.projection_offset_x &&
        a.projection_offset_y == b.projection_offset_y &&
        a.draw_offset_x == b.draw_offset_x && a.draw_offset_y == b.draw_offset_y;
}
bool eligible(const WorldDrawList& list, const WorldDrawCommand& c) {
    if (c.object_kind != 3 || c.channel != WorldViewChannel::main_view ||
        c.model_pointer == 0 || c.material_index >= list.materials.size()) return false;
    const auto flags = list.materials[c.material_index].primitive_flags;
    if ((flags & (7U | world_primitive_screen_space_flag)) != 0) return false;
    for (const auto& v : c.vertices)
        if (!v.exact_transform_valid || v.transform_id == 0 ||
            v.source_vertex_identity == 0 ||
            (v.provenance_flags & world_vertex_source_identity_flag) == 0 ||
            !same_projection(c.vertices[0], v)) return false;
    return true;
}
bool opposite_side(Point a, Point b, Point c, Point d) {
    const auto cross = [a,b](Point p) {
        std::array<double,3> u{}, v{};
        for (int k=0; k<3; ++k) { u[k]=b[k]-a[k]; v[k]=p[k]-a[k]; }
        return std::array<double,3>{u[1]*v[2]-u[2]*v[1],
            u[2]*v[0]-u[0]*v[2], u[0]*v[1]-u[1]*v[0]};
    };
    const auto n=cross(c), m=cross(d);
    return n[0]*m[0]+n[1]*m[1]+n[2]*m[2] < 0.0;
}
bool projected_opposite_side(const WorldDrawVertex& a, const WorldDrawVertex& b,
    const WorldDrawVertex& c, const WorldDrawVertex& d) {
    for (const auto* v : {&a,&b,&c,&d})
        if (!(v->clip_w>0) || !std::isfinite(v->clip_w) ||
            !std::isfinite(v->clip_x) || !std::isfinite(v->clip_y)) return false;
    const double ax=a.clip_x/a.clip_w, ay=a.clip_y/a.clip_w;
    const double bx=b.clip_x/b.clip_w, by=b.clip_y/b.clip_w;
    const auto side=[&](const WorldDrawVertex& v) {
        return (bx-ax)*(v.clip_y/v.clip_w-ay)-(by-ay)*(v.clip_x/v.clip_w-ax);
    };
    return side(c)*side(d)<0;
}
}

std::uint32_t repair_background_midpoint_seams(WorldDrawList& list) {
    // Original sky meshes mix coarse flat faces and finer Gouraud faces.
    // A rounded integer midpoint can lie half a source unit off the coarse
    // edge. Continuous projection exposes clear-color dots at that crack.
    // Conform only a proven boundary chain A-M-B to the existing vertex M:
    // no screen-space proximity, dilation, new colors, or texture edits.
    std::map<GroupKey, Group> groups;
    for (std::size_t i=0; i<list.commands.size(); ++i) {
        const auto& c=list.commands[i];
        if (!eligible(list,c)) continue;
        const auto& v=c.vertices[0];
        auto& group=groups[{c.model_pointer,c.object_id,v.transform_id,
            v.projection_plane,v.projection_offset_x,v.projection_offset_y,
            v.draw_offset_x,v.draw_offset_y,c.clip_x0,c.clip_y0,c.clip_x1,c.clip_y1}];
        for (int e=0; e<3; ++e) {
            group.points.emplace(point(c.vertices[e]),c.vertices[e]);
            group.edges[edge(point(c.vertices[e]),point(c.vertices[(e+1)%3]))].push_back({i,e});
        }
    }
    using Splits = std::map<Edge, WorldDrawVertex>;
    std::map<std::size_t, Splits> splits;
    std::map<std::size_t, std::vector<WorldDrawCommand>> gap_faces;
    std::uint32_t junctions=0;
    for (const auto& entry : groups) {
        const auto& group=entry.second;
        for (const auto& boundary : group.edges) {
            if (boundary.second.size()!=1) continue;
            const auto occurrence=boundary.second[0];
            const auto& c=list.commands[occurrence.command];
            const auto& va=c.vertices[occurrence.edge];
            const auto& vb=c.vertices[(occurrence.edge+1)%3];
            const auto& vc=c.vertices[(occurrence.edge+2)%3];
            // Non-flat parents remain untouched. Their gap may use original
            // endpoint/midpoint RGB only when source colors prove the same
            // rounded half-grid subdivision as the source positions.
            const bool flat_parent=color(va,vb) && color(va,vc);
            const auto& a=boundary.first.first;
            const auto& b=boundary.first.second;
            Point low{};
            for (int k=0; k<3; ++k) low[k]=static_cast<int>(std::floor((a[k]+b[k])*0.5));
            std::map<Point, WorldDrawVertex> candidates;
            for (int bits=0; bits<8; ++bits) {
                Point m=low;
                bool rounded=true;
                for (int k=0; k<3; ++k) {
                    m[k]+=(bits>>k)&1;
                    rounded &= std::abs(2*m[k]-a[k]-b[k])<=1;
                }
                if (!rounded || m==a || m==b) continue;
                const auto vertex=group.points.find(m);
                if (vertex==group.points.end() || !same_projection(c.vertices[0],vertex->second)) continue;
                const auto& vm=vertex->second;
                if (std::abs(2*int(vm.r)-int(va.r)-int(vb.r))>1 ||
                    std::abs(2*int(vm.g)-int(va.g)-int(vb.g))>1 ||
                    std::abs(2*int(vm.b)-int(va.b)-int(vb.b))>1) continue;
                const auto am=group.edges.find(edge(a,m));
                const auto mb=group.edges.find(edge(m,b));
                if (am==group.edges.end() || mb==group.edges.end() ||
                    am->second.size()!=1 || mb->second.size()!=1) continue;
                bool adjoining=true;
                for (const auto& ref : {am->second[0],mb->second[0]}) {
                    const auto& neighbor=list.commands[ref.command];
                    for (int endpoint : {ref.edge,(ref.edge+1)%3}) {
                        const auto& actual=neighbor.vertices[endpoint];
                        const auto& expected=point(actual)==m?vm:
                            point(actual)==point(va)?va:vb;
                        adjoining &= color(expected,actual);
                    }
                    adjoining &= opposite_side(a,b,point(c.vertices[(occurrence.edge+2)%3]),
                            point(neighbor.vertices[(ref.edge+2)%3]));
                    if (!flat_parent) {
                        const auto& na=neighbor.vertices[ref.edge];
                        const auto& nb=neighbor.vertices[(ref.edge+1)%3];
                        const auto& nd=neighbor.vertices[(ref.edge+2)%3];
                        const auto& other=point(na)==point(va) || point(nb)==point(va)?vb:va;
                        adjoining &= projected_opposite_side(na,nb,nd,other);
                    }
                }
                if (!flat_parent)
                    adjoining &= projected_opposite_side(va,vb,vc,vertex->second);
                if (adjoining) candidates.emplace(m,vertex->second);
            }
            // Ambiguous source adjacency is not permission to pick a weld.
            if (candidates.size()!=1) continue;
            auto midpoint=candidates.begin()->second;
            if (flat_parent) {
                splits[occurrence.command].emplace(boundary.first,midpoint);
            } else {
                // All three projected edge tests prove this tiny source face
                // lies outside the original parent and both neighbors. Fill
                // only A-M-B with original vertex RGB: do not re-triangulate
                // or recolor either original affine gradient.
                auto gap=c;
                gap.vertices[0]=vb; gap.vertices[1]=va; gap.vertices[2]=midpoint;
                gap_faces[occurrence.command].push_back(gap);
            }
            ++junctions;
        }
    }
    if (splits.empty() && gap_faces.empty()) return 0;
    std::vector<WorldDrawCommand> rebuilt;
    rebuilt.reserve(list.commands.size()+junctions);
    for (std::size_t i=0; i<list.commands.size(); ++i) {
        const auto gaps=gap_faces.find(i);
        if (gaps!=gap_faces.end())
            rebuilt.insert(rebuilt.end(),gaps->second.begin(),gaps->second.end());
        const auto found=splits.find(i);
        if (found==splits.end()) { rebuilt.push_back(list.commands[i]); continue; }
        std::vector<WorldDrawCommand> pieces{list.commands[i]};
        for (const auto& split : found->second) {
            for (std::size_t part=0; part<pieces.size(); ++part) {
                bool done=false;
                for (int e=0; e<3; ++e) {
                    const auto original=pieces[part];
                    if (edge(point(original.vertices[e]),point(original.vertices[(e+1)%3]))!=split.first) continue;
                    auto first=original, second=original;
                    first.vertices[(e+1)%3]=split.second;
                    second.vertices[e]=split.second;
                    pieces[part]=first; pieces.push_back(second); done=true; break;
                }
                if (done) break;
            }
        }
        rebuilt.insert(rebuilt.end(),pieces.begin(),pieces.end());
    }
    list.commands=std::move(rebuilt);
    return junctions;
}
}
