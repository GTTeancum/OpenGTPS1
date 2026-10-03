#include "api.hpp"
#include <dlfcn.h>
#include <mutex>
#include <stdexcept>
#include <string>
#include <cstdlib>
#include <cstring>
namespace opengt::render::gl {
namespace {
struct Egl {
 void* library{};
 void* (*GetProcAddress)(const char*){};
 void* (*GetPlatformDisplay)(unsigned,void*,const std::intptr_t*){};
 unsigned (*Initialize)(void*,int*,int*){};
 unsigned (*Terminate)(void*){};
 unsigned (*BindAPI)(unsigned){};
 unsigned (*ChooseConfig)(void*,const int*,void**,int,int*){};
 void* (*CreateContext)(void*,void*,void*,const int*){};
 void* (*CreatePbufferSurface)(void*,void*,const int*){};
 unsigned (*MakeCurrent)(void*,void*,void*,void*){};
 unsigned (*DestroyContext)(void*,void*){};
 unsigned (*DestroySurface)(void*,void*){};
 unsigned (*GetError)(){};
 std::mutex mutex; void* display{}; unsigned users{};
 Egl() {
  library=dlopen("libEGL.so.1",RTLD_NOW|RTLD_LOCAL);
  if(!library)throw std::runtime_error("libEGL.so.1 unavailable");
#define LOAD(N) N=reinterpret_cast<decltype(N)>(dlsym(library,"egl" #N)); if(!N)throw std::runtime_error("Missing egl" #N);
  LOAD(GetProcAddress) LOAD(GetPlatformDisplay) LOAD(Initialize) LOAD(Terminate)
  LOAD(BindAPI) LOAD(ChooseConfig) LOAD(CreateContext) LOAD(CreatePbufferSurface)
  LOAD(MakeCurrent) LOAD(DestroyContext) LOAD(DestroySurface) LOAD(GetError)
#undef LOAD
 }
};
// Keep the loader alive through thread_local resource destruction. Displays
// themselves are reference leased and terminated after their final context.
Egl& egl(){static Egl* e=new Egl;return *e;}
void* lease(){auto& e=egl();std::lock_guard<std::mutex> lock(e.mutex);
 if(!e.users){e.display=e.GetPlatformDisplay(0x31DD,nullptr,nullptr);int a,b;
  if(!e.display||!e.Initialize(e.display,&a,&b))throw std::runtime_error("EGL surfaceless display initialization failed");}
 ++e.users;return e.display;
}
void release(){auto& e=egl();std::lock_guard<std::mutex> lock(e.mutex);if(e.users&&!--e.users){e.Terminate(e.display);e.display=nullptr;}}
}
Context::Context(){auto& e=egl();display_=lease();
 try {
  if(!e.BindAPI(0x30A2))throw std::runtime_error("EGL OpenGL API unavailable");
  const int attrs[]={0x3033,1,0x3040,8,0x3024,8,0x3023,8,0x3022,8,0x3021,8,0x3038};
  void* config{};int count{};
  if(!e.ChooseConfig(display_,attrs,&config,1,&count)||!count)throw std::runtime_error("No EGL OpenGL pbuffer config");
  const int ctx[]={0x3098,4,0x30FB,3,0x30FD,1,0x3038};
  context_=e.CreateContext(display_,config,nullptr,ctx);
  if(!context_)throw std::runtime_error("OpenGL 4.3 core context unavailable");
  const int pbuf[]={0x3057,1,0x3056,1,0x3038};surface_=e.CreatePbufferSurface(display_,config,pbuf);
  if(!surface_)throw std::runtime_error("EGL pbuffer creation failed");
  make_current();
#define LOAD(R,N,A) api.N=reinterpret_cast<Api::N##Fn>(e.GetProcAddress("gl" #N));if(!api.N)throw std::runtime_error("Missing gl" #N);
  OPENGT_GL_FUNCTIONS(LOAD)
#undef LOAD
  // Clip control is core in 4.5, optional in the original 4.3 requirement.
  // Function-pointer availability alone does not establish extension support.
  int major=0,minor=0;api.GetIntegerv(0x821B,&major);api.GetIntegerv(0x821C,&minor);
  bool supported=major>4||(major==4&&minor>=5);
  using Stringi=const GLubyte*(*)(GLenum,GLuint);
  auto stringi=reinterpret_cast<Stringi>(e.GetProcAddress("glGetStringi"));
  int n=0;api.GetIntegerv(0x821D,&n);
  for(int i=0;!supported&&stringi&&i<n;++i){auto* x=stringi(0x1F03,GLuint(i));supported=x&&std::strcmp(reinterpret_cast<const char*>(x),"GL_ARB_clip_control")==0;}
  api.ClipControl=reinterpret_cast<Api::ClipControlFn>(e.GetProcAddress("glClipControl"));
  api.zero_to_one=supported&&api.ClipControl&&!std::getenv("OPENGT_GL_DEPTH_FALLBACK");
  if(api.zero_to_one)api.ClipControl(0x8CA1,0x935F); // LOWER_LEFT, ZERO_TO_ONE
 } catch(...) {e.MakeCurrent(display_,nullptr,nullptr,nullptr);if(surface_)e.DestroySurface(display_,surface_);if(context_)e.DestroyContext(display_,context_);release();throw;}
}
Context::~Context(){auto& e=egl();e.MakeCurrent(display_,nullptr,nullptr,nullptr);if(surface_)e.DestroySurface(display_,surface_);if(context_)e.DestroyContext(display_,context_);release();}
void Context::make_current(){if(!egl().MakeCurrent(display_,surface_,surface_,context_))throw std::runtime_error("eglMakeCurrent failed");}
const char* Context::renderer()const{return reinterpret_cast<const char*>(api.GetString(RENDERER));}
}
