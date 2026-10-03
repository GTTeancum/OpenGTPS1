// Offline inverse-shadow search. Input is prepared from actual revision-31
// course geometry by analyze_track_lighting.py. This does not run per frame.
#include <algorithm>
#include <array>
#include <cmath>
#include <cstdint>
#include <cstring>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <limits>
#include <numeric>
#include <set>
#include <stdexcept>
#include <vector>
namespace {
struct V {double x{},y{},z{};double axis(int i)const{return i==0?x:i==1?y:z;}};
V operator+(V a,V b){return {a.x+b.x,a.y+b.y,a.z+b.z};}V operator-(V a,V b){return {a.x-b.x,a.y-b.y,a.z-b.z};}V operator*(V a,double t){return {a.x*t,a.y*t,a.z*t};}
double dot(V a,V b){return a.x*b.x+a.y*b.y+a.z*b.z;}V cross(V a,V b){return {a.y*b.z-a.z*b.y,a.z*b.x-a.x*b.z,a.x*b.y-a.y*b.x};}
V normal(V v){double d=std::sqrt(dot(v,v));return d>1e-20?v*(1/d):V{};}
struct Triangle{V a,b,c;};struct Sample{V p,n;unsigned label,group;double weight;};
struct Box{V lo{1e30,1e30,1e30},hi{-1e30,-1e30,-1e30};void add(V p){lo={std::min(lo.x,p.x),std::min(lo.y,p.y),std::min(lo.z,p.z)};hi={std::max(hi.x,p.x),std::max(hi.y,p.y),std::max(hi.z,p.z)};}};
struct Node{Box box;unsigned begin{},end{},left{},right{};};
class BVH{
    std::vector<Triangle> tris;std::vector<unsigned> indices;std::vector<Node> nodes;
    unsigned build(unsigned begin,unsigned end){unsigned me=static_cast<unsigned>(nodes.size());nodes.push_back({});Box b,centers;
        for(unsigned i=begin;i<end;++i){auto t=tris[indices[i]];b.add(t.a);b.add(t.b);b.add(t.c);centers.add((t.a+t.b+t.c)*(1.0/3));}
        nodes[me].box=b;nodes[me].begin=begin;nodes[me].end=end;if(end-begin<=8)return me;
        V extent=centers.hi-centers.lo;int axis=extent.y>extent.x?1:0;if(extent.z>extent.axis(axis))axis=2;
        unsigned middle=begin+(end-begin)/2;
        std::nth_element(indices.begin()+begin,indices.begin()+middle,indices.begin()+end,[&](unsigned a,unsigned z){auto p=tris[a],q=tris[z];return (p.a+p.b+p.c).axis(axis)<(q.a+q.b+q.c).axis(axis);});
        unsigned left=build(begin,middle),right=build(middle,end);nodes[me].left=left;nodes[me].right=right;return me;
    }
    bool box_hit(Box b,V o,V d)const{double low=0,high=1e20;for(int j=0;j<3;++j){double v=d.axis(j);if(std::abs(v)<1e-12){if(o.axis(j)<b.lo.axis(j)||o.axis(j)>b.hi.axis(j))return false;continue;}
        double a=(b.lo.axis(j)-o.axis(j))/v,z=(b.hi.axis(j)-o.axis(j))/v;if(a>z)std::swap(a,z);low=std::max(low,a);high=std::min(high,z);if(high<low)return false;}return true;}
    bool triangle_hit(Triangle t,V o,V d)const{V e1=t.b-t.a,e2=t.c-t.a,p=cross(d,e2);double det=dot(e1,p);if(std::abs(det)<1e-10)return false;double inv=1/det;V q=o-t.a;double u=dot(q,p)*inv;if(u<0||u>1)return false;V r=cross(q,e1);double v=dot(d,r)*inv;if(v<0||u+v>1)return false;return dot(e2,r)*inv>1.0;}
public:
    explicit BVH(std::vector<Triangle> t):tris(std::move(t)){indices.resize(tris.size());std::iota(indices.begin(),indices.end(),0U);nodes.reserve(tris.size()*2);if(!tris.empty())build(0,static_cast<unsigned>(tris.size()));}
    bool hit(V origin,V direction)const{if(nodes.empty())return false;std::array<unsigned,128> stack{};unsigned n=1;while(n){const auto& node=nodes[stack[--n]];if(!box_hit(node.box,origin,direction))continue;
            if(node.left){if(n+2>stack.size())throw std::runtime_error("BVH stack overflow");stack[n++]=node.left;stack[n++]=node.right;}else for(unsigned i=node.begin;i<node.end;++i)if(triangle_hit(tris[indices[i]],origin,direction))return true;}
        return false;}
};
unsigned u32(std::ifstream& in){unsigned char b[4];if(!in.read(reinterpret_cast<char*>(b),4))throw std::runtime_error("truncated evidence");return unsigned(b[0])|(unsigned(b[1])<<8)|(unsigned(b[2])<<16)|(unsigned(b[3])<<24);}
double f32(std::ifstream& in){std::uint32_t n=u32(in);float f;std::memcpy(&f,&n,4);if(!std::isfinite(f)||std::abs(f)>1e9F)throw std::runtime_error("invalid evidence coordinate");return f;}
V vector(std::ifstream& in){double x=f32(in),y=f32(in),z=f32(in);return {x,y,z};}
struct Candidate{V direction;double score{},even{},odd{},azimuth{},elevation{};};
}
int main(int argc,char**argv){try{
    if(argc!=2){std::cerr<<"usage: opengt_solar_fit evidence.bin\n";return 2;}
    std::ifstream in(argv[1],std::ios::binary);char magic[8];if(!in.read(magic,8)||std::memcmp(magic,"OGTSUN1\0",8))throw std::runtime_error("invalid evidence format");
    unsigned nt=u32(in),ns=u32(in);if(nt>2000000||ns>20000)throw std::runtime_error("evidence limits exceeded");
    std::vector<Triangle> triangles;triangles.reserve(nt);for(unsigned i=0;i<nt;++i){auto a=vector(in),b=vector(in),c=vector(in);triangles.push_back({a,b,c});}
    std::vector<Sample> samples;std::set<unsigned> groups;unsigned dark=0,bright=0;
    for(unsigned i=0;i<ns;++i){auto p=vector(in),n=normal(vector(in));unsigned label=u32(in),group=u32(in);double weight=f32(in);if(label>1||weight<=0||weight>1e6)throw std::runtime_error("invalid evidence sample");samples.push_back({p,n,label,group,weight});groups.insert(group);if(label)++dark;else++bright;}
    if(in.peek()!=std::ifstream::traits_type::eof())throw std::runtime_error("trailing evidence bytes");
    if(dark<10||bright<10||groups.size()<4){std::cout<<"{\"accepted\":false,\"reason\":\"insufficient independent lit/shadow samples\",\"samples\":"<<ns<<",\"dark\":"<<dark<<",\"bright\":"<<bright<<"}\n";return 0;}
    BVH bvh(std::move(triangles));std::vector<Candidate> candidates;
    constexpr double pi=3.14159265358979323846;
    auto evaluate=[&](double az,double el){Candidate c;c.azimuth=az;c.elevation=el;c.direction={std::cos(el*pi/180)*std::sin(az*pi/180),-std::sin(el*pi/180),std::cos(el*pi/180)*std::cos(az*pi/180)};
        double correct[2][2]{},total[2][2]{};
        for(auto s:samples){bool blocked=bvh.hit(s.p+s.n*2.0,c.direction);unsigned fold=s.group%2;correct[fold][s.label]+=(blocked==(s.label!=0))?s.weight:0;total[fold][s.label]+=s.weight;}
        auto rate=[&](int f,int l){return total[f][l]>0?correct[f][l]/total[f][l]:.5;};c.even=.5*(rate(0,0)+rate(0,1));c.odd=.5*(rate(1,0)+rate(1,1));c.score=.5*(c.even+c.odd);candidates.push_back(c);};
    for(double el=10;el<=80;el+=10)for(double az=0;az<360;az+=15)evaluate(az,el);
    Candidate coarse=*std::max_element(candidates.begin(),candidates.end(),[](auto a,auto b){return a.score<b.score;});
    for(double el=std::max(5.0,coarse.elevation-10);el<=std::min(85.0,coarse.elevation+10);el+=2)for(double az=coarse.azimuth-15;az<=coarse.azimuth+15;az+=3)evaluate(az,el);
    auto best=*std::max_element(candidates.begin(),candidates.end(),[](auto a,auto b){return a.score<b.score;});
    auto even=*std::max_element(candidates.begin(),candidates.end(),[](auto a,auto b){return a.even<b.even;});
    auto odd=*std::max_element(candidates.begin(),candidates.end(),[](auto a,auto b){return a.odd<b.odd;});
    double spread=0;for(auto c:candidates)if(c.score>=best.score-.02)spread=std::max(spread,std::acos(std::clamp(dot(c.direction,best.direction),-1.0,1.0))*180/pi);
    double fold_angle=std::acos(std::clamp(dot(even.direction,odd.direction),-1.0,1.0))*180/pi;
    bool accepted=best.even>=.78&&best.odd>=.78&&even.odd>=.74&&odd.even>=.74&&spread<=22&&fold_angle<=25&&groups.size()>=6;
    double confidence=accepted?std::clamp(.75+(std::min(best.even,best.odd)-.78)*.8,.75,.95):0;
    std::cout<<std::setprecision(9)<<"{\"accepted\":"<<(accepted?"true":"false")<<",\"reason\":\""<<(accepted?"geometry-and-baked-modulation agreement; estimated, not astronomical":"ambiguous or weak silhouette agreement; solar disabled")<<"\",\"direction\":["<<best.direction.x<<","<<best.direction.y<<","<<best.direction.z<<"],\"score\":"<<best.score<<",\"evenScore\":"<<best.even<<",\"oddScore\":"<<best.odd<<",\"evenFitOddScore\":"<<even.odd<<",\"oddFitEvenScore\":"<<odd.even<<",\"angularAmbiguityDegrees\":"<<spread<<",\"foldDisagreementDegrees\":"<<fold_angle<<",\"confidence\":"<<confidence<<",\"triangles\":"<<nt<<",\"samples\":"<<ns<<",\"groups\":"<<groups.size()<<",\"dark\":"<<dark<<",\"bright\":"<<bright<<",\"hypotheses\":"<<candidates.size()<<"}\n";
    return 0;
}catch(const std::exception&e){std::cerr<<"solar fit: "<<e.what()<<"\n";return 1;}}
