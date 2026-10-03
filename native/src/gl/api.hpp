#pragma once
// Private OpenGL loader. This module uses only the system EGL/OpenGL runtime;
// it does not assume development headers or link a vendor graphics library.
#include <cstddef>
#include <cstdint>
namespace opengt::render::gl {
using GLenum=unsigned; using GLuint=unsigned; using GLint=int; using GLsizei=int;
using GLbitfield=unsigned; using GLboolean=unsigned char; using GLfloat=float;
using GLchar=char; using GLubyte=unsigned char; using GLsizeiptr=std::ptrdiff_t;
using GLintptr=std::ptrdiff_t; using GLdouble=double;
#define OPENGT_GL_FUNCTIONS(X) \
 X(const GLubyte*,GetString,(GLenum)) \
 X(GLenum,GetError,()) \
 X(void,GetIntegerv,(GLenum,GLint*)) \
 X(void,GenVertexArrays,(GLsizei,GLuint*)) \
 X(void,DeleteVertexArrays,(GLsizei,const GLuint*)) \
 X(void,BindVertexArray,(GLuint)) \
 X(void,GenBuffers,(GLsizei,GLuint*)) \
 X(void,DeleteBuffers,(GLsizei,const GLuint*)) \
 X(void,BindBuffer,(GLenum,GLuint)) \
 X(void,BufferData,(GLenum,GLsizeiptr,const void*,GLenum)) \
 X(void,BindBufferBase,(GLenum,GLuint,GLuint)) \
 X(void,EnableVertexAttribArray,(GLuint)) \
 X(void,VertexAttribPointer,(GLuint,GLint,GLenum,GLboolean,GLsizei,const void*)) \
 X(void,VertexAttribIPointer,(GLuint,GLint,GLenum,GLsizei,const void*)) \
 X(GLuint,CreateShader,(GLenum)) \
 X(void,ShaderSource,(GLuint,GLsizei,const GLchar* const*,const GLint*)) \
 X(void,CompileShader,(GLuint)) \
 X(void,GetShaderiv,(GLuint,GLenum,GLint*)) \
 X(void,GetShaderInfoLog,(GLuint,GLsizei,GLsizei*,GLchar*)) \
 X(void,DeleteShader,(GLuint)) \
 X(GLuint,CreateProgram,()) \
 X(void,AttachShader,(GLuint,GLuint)) \
 X(void,LinkProgram,(GLuint)) \
 X(void,GetProgramiv,(GLuint,GLenum,GLint*)) \
 X(void,GetProgramInfoLog,(GLuint,GLsizei,GLsizei*,GLchar*)) \
 X(void,DeleteProgram,(GLuint)) \
 X(void,UseProgram,(GLuint)) \
 X(GLint,GetUniformLocation,(GLuint,const GLchar*)) \
 X(void,Uniform1i,(GLint,GLint)) \
 X(void,Uniform1ui,(GLint,GLuint)) \
 X(void,Uniform1f,(GLint,GLfloat)) \
 X(void,Uniform2f,(GLint,GLfloat,GLfloat)) \
 X(void,Uniform3f,(GLint,GLfloat,GLfloat,GLfloat)) \
 X(void,Uniform4f,(GLint,GLfloat,GLfloat,GLfloat,GLfloat)) \
 X(void,Uniform4fv,(GLint,GLsizei,const GLfloat*)) \
 X(void,Uniform3fv,(GLint,GLsizei,const GLfloat*)) \
 X(void,GenTextures,(GLsizei,GLuint*)) \
 X(void,DeleteTextures,(GLsizei,const GLuint*)) \
 X(void,ActiveTexture,(GLenum)) \
 X(void,BindTexture,(GLenum,GLuint)) \
 X(void,TexImage2D,(GLenum,GLint,GLint,GLsizei,GLsizei,GLint,GLenum,GLenum,const void*)) \
 X(void,TexImage3D,(GLenum,GLint,GLint,GLsizei,GLsizei,GLsizei,GLint,GLenum,GLenum,const void*)) \
 X(void,TexParameteri,(GLenum,GLenum,GLint)) \
 X(void,TexParameterfv,(GLenum,GLenum,const GLfloat*)) \
 X(void,GenerateMipmap,(GLenum)) \
 X(void,GenFramebuffers,(GLsizei,GLuint*)) \
 X(void,DeleteFramebuffers,(GLsizei,const GLuint*)) \
 X(void,BindFramebuffer,(GLenum,GLuint)) \
 X(void,FramebufferTexture2D,(GLenum,GLenum,GLenum,GLuint,GLint)) \
 X(void,FramebufferTextureLayer,(GLenum,GLenum,GLuint,GLint,GLint)) \
 X(GLenum,CheckFramebufferStatus,(GLenum)) \
 X(void,DrawBuffer,(GLenum)) \
 X(void,ReadBuffer,(GLenum)) \
 X(void,Viewport,(GLint,GLint,GLsizei,GLsizei)) \
 X(void,Scissor,(GLint,GLint,GLsizei,GLsizei)) \
 X(void,Enable,(GLenum)) \
 X(void,Disable,(GLenum)) \
 X(void,StencilFunc,(GLenum,GLint,GLuint)) \
 X(void,StencilOp,(GLenum,GLenum,GLenum)) \
 X(void,StencilMask,(GLuint)) \
 X(void,ClearStencil,(GLint)) \
 X(void,DepthFunc,(GLenum)) \
 X(void,DepthMask,(GLboolean)) \
 X(void,ClearColor,(GLfloat,GLfloat,GLfloat,GLfloat)) \
 X(void,ClearDepth,(GLdouble)) \
 X(void,Clear,(GLbitfield)) \
 X(void,BlendEquation,(GLenum)) \
 X(void,BlendFunc,(GLenum,GLenum)) \
 X(void,BlendFuncSeparate,(GLenum,GLenum,GLenum,GLenum)) \
 X(void,BlendColor,(GLfloat,GLfloat,GLfloat,GLfloat)) \
 X(void,DrawArrays,(GLenum,GLint,GLsizei)) \
 X(void,PixelStorei,(GLenum,GLint)) \
 X(void,ReadPixels,(GLint,GLint,GLsizei,GLsizei,GLenum,GLenum,void*)) \
 X(void,Finish,())
#define DECL(R,N,A) using N##Fn=R(*)A; N##Fn N{};
struct Api { OPENGT_GL_FUNCTIONS(DECL)
 using ClipControlFn=void(*)(GLenum,GLenum); ClipControlFn ClipControl{};
 bool zero_to_one{};
};
#undef DECL
struct Context {
    Context(); ~Context();
    Context(const Context&)=delete; Context& operator=(const Context&)=delete;
    void make_current();
    Api api;
    const char* renderer() const;
private:
    void* display_{}; void* context_{}; void* surface_{};
};
// Core constants used here. Names deliberately private to avoid macro leakage.
constexpr GLenum FALSE_=0, TRUE_=1, NONE=0, FLOAT=0x1406, UNSIGNED_INT=0x1405,
 UNSIGNED_BYTE=0x1401, UNSIGNED_SHORT=0x1403, ARRAY_BUFFER=0x8892,
 SHADER_STORAGE_BUFFER=0x90D2, DYNAMIC_DRAW=0x88E8, TRIANGLES=4,
 VERTEX_SHADER=0x8B31, FRAGMENT_SHADER=0x8B30, COMPILE_STATUS=0x8B81,
 LINK_STATUS=0x8B82, INFO_LOG_LENGTH=0x8B84, TEXTURE0=0x84C0,
 TEXTURE_2D=0x0DE1, TEXTURE_2D_ARRAY=0x8C1A, TEXTURE_CUBE_MAP_ARRAY=0x9009,
 TEXTURE_MIN_FILTER=0x2801,TEXTURE_MAG_FILTER=0x2800,TEXTURE_WRAP_S=0x2802,
 TEXTURE_WRAP_T=0x2803,TEXTURE_WRAP_R=0x8072,CLAMP_TO_EDGE=0x812F,
 NEAREST=0x2600,LINEAR=0x2601,LINEAR_MIPMAP_LINEAR=0x2703,
 TEXTURE_COMPARE_MODE=0x884C,TEXTURE_COMPARE_FUNC=0x884D,COMPARE_REF_TO_TEXTURE=0x884E,
 R32F=0x822E,RED=0x1903,R16UI=0x8234, RED_INTEGER=0x8D94, RGBA8=0x8058,RGBA16F=0x881A,RGBA=0x1908,
 DEPTH_COMPONENT32F=0x8CAC,DEPTH_COMPONENT=0x1902,FRAMEBUFFER=0x8D40,
 DEPTH32F_STENCIL8=0x8CAD,DEPTH_STENCIL=0x84F9,FLOAT_32_UNSIGNED_INT_24_8_REV=0x8DAD,DEPTH_STENCIL_ATTACHMENT=0x821A,
 STENCIL_TEST=0x0B90,STENCIL_BUFFER_BIT=0x400,EQUAL=0x0202,KEEP=0x1E00,INVERT=0x150A,
 COLOR_ATTACHMENT0=0x8CE0,DEPTH_ATTACHMENT=0x8D00,FRAMEBUFFER_COMPLETE=0x8CD5,
 DEPTH_TEST=0x0B71,SCISSOR_TEST=0x0C11,BLEND=0x0BE2,DITHER=0x0BD0,CULL_FACE=0x0B44,
 FRAMEBUFFER_SRGB=0x8DB9,TEXTURE_CUBE_MAP_SEAMLESS=0x884F,
 LESS=0x0201,LEQUAL=0x0203,GEQUAL=0x0206,GREATER=0x0204,ALWAYS=0x0207,
 COLOR_BUFFER_BIT=0x4000,DEPTH_BUFFER_BIT=0x0100,
 FUNC_ADD=0x8006,FUNC_REVERSE_SUBTRACT=0x800B,ONE=1,ZERO=0,
 SRC_ALPHA=0x0302,ONE_MINUS_SRC_ALPHA=0x0303,CONSTANT_COLOR=0x8001,
 ONE_MINUS_CONSTANT_COLOR=0x8002,PACK_ALIGNMENT=0x0D05,UNPACK_ALIGNMENT=0x0CF5,
 RENDERER=0x1F01,VERSION=0x1F02;
} // namespace opengt::render::gl
