 Shader "Hidden/PreDepthOfField" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_onePixelTex ("Pixel (RGB)", 2D) = "" {}
	}

	#LINE 54

	
Subshader {
 Pass {
	  ZTest Always Cull Off ZWrite Off
	  ColorMask A
	  Fog { Mode off }      

      Program "vp" {
// Vertex combos: 1
//   opengl - ALU: 5 to 5
//   d3d9 - ALU: 5 to 5
//   d3d11 - ALU: 5 to 5, TEX: 0 to 0, FLOW: 1 to 1
//   d3d11_9x - ALU: 5 to 5, TEX: 0 to 0, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Vector 5 [_CameraDepthTexture_ST]
"!!ARBvp1.0
# 5 ALU
PARAM c[6] = { program.local[0],
		state.matrix.mvp,
		program.local[5] };
MAD result.texcoord[0].xy, vertex.texcoord[0], c[5], c[5].zwzw;
DP4 result.position.w, vertex.position, c[4];
DP4 result.position.z, vertex.position, c[3];
DP4 result.position.y, vertex.position, c[2];
DP4 result.position.x, vertex.position, c[1];
END
# 5 instructions, 0 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 4 [_CameraDepthTexture_ST]
"vs_2_0
; 5 ALU
dcl_position0 v0
dcl_texcoord0 v1
mad oT0.xy, v1, c4, c4.zwzw
dp4 oPos.w, v0, c3
dp4 oPos.z, v0, c2
dp4 oPos.y, v0, c1
dp4 oPos.x, v0, c0
"
}

SubProgram "d3d11 " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
ConstBuffer "$Globals" 80 // 80 used size, 8 vars
Vector 64 [_CameraDepthTexture_ST] 4
ConstBuffer "UnityPerDraw" 336 // 64 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
BindCB "$Globals" 0
BindCB "UnityPerDraw" 1
// 6 instructions, 1 temp regs, 0 temp arrays:
// ALU 5 float, 0 int, 0 uint
// TEX 0 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"vs_4_0
eefiecedjeggehgnckhcgoejnkkifolikjjmdalfabaaaaaaamacaaaaadaaaaaa
cmaaaaaaiaaaaaaaniaaaaaaejfdeheoemaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaaebaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafaepfdejfeejepeoaafeeffiedepepfceeaaklkl
epfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
fdfgfpfagphdgjhegjgpgoaafeeffiedepepfceeaaklklklfdeieefccmabaaaa
eaaaabaaelaaaaaafjaaaaaeegiocaaaaaaaaaaaafaaaaaafjaaaaaeegiocaaa
abaaaaaaaeaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaabaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaaddccabaaaabaaaaaagiaaaaac
abaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaabaaaaaa
abaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaaaaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaa
acaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaa
egiocaaaabaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaal
dccabaaaabaaaaaaegbabaaaabaaaaaaegiacaaaaaaaaaaaaeaaaaaaogikcaaa
aaaaaaaaaeaaaaaadoaaaaab"
}

SubProgram "gles " {
Keywords { }
"!!GLES


#ifdef VERTEX

varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 _CameraDepthTexture_ST;
uniform highp mat4 glstate_matrix_mvp;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesVertex;
void main ()
{
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _CameraDepthTexture_ST.xy) + _CameraDepthTexture_ST.zw);
}



#endif
#ifdef FRAGMENT

varying highp vec2 xlv_TEXCOORD0;
uniform highp float focalEnd01;
uniform highp float focalStart01;
uniform highp float focalFalloff;
uniform highp float focalDistance01;
uniform sampler2D _CameraDepthTexture;
uniform highp vec4 _ZBufferParams;
void main ()
{
  mediump float preDof_1;
  highp float d_2;
  lowp float tmpvar_3;
  tmpvar_3 = texture2D (_CameraDepthTexture, xlv_TEXCOORD0).x;
  d_2 = tmpvar_3;
  highp float tmpvar_4;
  tmpvar_4 = (1.0/(((_ZBufferParams.x * d_2) + _ZBufferParams.y)));
  d_2 = tmpvar_4;
  if ((tmpvar_4 > focalDistance01)) {
    highp float tmpvar_5;
    tmpvar_5 = ((tmpvar_4 - focalDistance01) / (focalEnd01 - focalDistance01));
    preDof_1 = tmpvar_5;
  } else {
    highp float tmpvar_6;
    tmpvar_6 = ((tmpvar_4 - focalDistance01) / (focalStart01 - focalDistance01));
    preDof_1 = tmpvar_6;
  };
  highp float tmpvar_7;
  tmpvar_7 = (preDof_1 * focalFalloff);
  preDof_1 = tmpvar_7;
  gl_FragData[0] = vec4(clamp (preDof_1, 0.0, 1.0));
}



#endif"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES


#ifdef VERTEX

varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 _CameraDepthTexture_ST;
uniform highp mat4 glstate_matrix_mvp;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesVertex;
void main ()
{
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _CameraDepthTexture_ST.xy) + _CameraDepthTexture_ST.zw);
}



#endif
#ifdef FRAGMENT

varying highp vec2 xlv_TEXCOORD0;
uniform highp float focalEnd01;
uniform highp float focalStart01;
uniform highp float focalFalloff;
uniform highp float focalDistance01;
uniform sampler2D _CameraDepthTexture;
uniform highp vec4 _ZBufferParams;
void main ()
{
  mediump float preDof_1;
  highp float d_2;
  lowp float tmpvar_3;
  tmpvar_3 = texture2D (_CameraDepthTexture, xlv_TEXCOORD0).x;
  d_2 = tmpvar_3;
  highp float tmpvar_4;
  tmpvar_4 = (1.0/(((_ZBufferParams.x * d_2) + _ZBufferParams.y)));
  d_2 = tmpvar_4;
  if ((tmpvar_4 > focalDistance01)) {
    highp float tmpvar_5;
    tmpvar_5 = ((tmpvar_4 - focalDistance01) / (focalEnd01 - focalDistance01));
    preDof_1 = tmpvar_5;
  } else {
    highp float tmpvar_6;
    tmpvar_6 = ((tmpvar_4 - focalDistance01) / (focalStart01 - focalDistance01));
    preDof_1 = tmpvar_6;
  };
  highp float tmpvar_7;
  tmpvar_7 = (preDof_1 * focalFalloff);
  preDof_1 = tmpvar_7;
  gl_FragData[0] = vec4(clamp (preDof_1, 0.0, 1.0));
}



#endif"
}

SubProgram "flash " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 4 [_CameraDepthTexture_ST]
"agal_vs
[bc]
adaaaaaaaaaaadacadaaaaoeaaaaaaaaaeaaaaoeabaaaaaa mul r0.xy, a3, c4
abaaaaaaaaaaadaeaaaaaafeacaaaaaaaeaaaaooabaaaaaa add v0.xy, r0.xyyy, c4.zwzw
bdaaaaaaaaaaaiadaaaaaaoeaaaaaaaaadaaaaoeabaaaaaa dp4 o0.w, a0, c3
bdaaaaaaaaaaaeadaaaaaaoeaaaaaaaaacaaaaoeabaaaaaa dp4 o0.z, a0, c2
bdaaaaaaaaaaacadaaaaaaoeaaaaaaaaabaaaaoeabaaaaaa dp4 o0.y, a0, c1
bdaaaaaaaaaaabadaaaaaaoeaaaaaaaaaaaaaaoeabaaaaaa dp4 o0.x, a0, c0
aaaaaaaaaaaaamaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v0.zw, c0
"
}

SubProgram "d3d11_9x " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
ConstBuffer "$Globals" 80 // 80 used size, 8 vars
Vector 64 [_CameraDepthTexture_ST] 4
ConstBuffer "UnityPerDraw" 336 // 64 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
BindCB "$Globals" 0
BindCB "UnityPerDraw" 1
// 6 instructions, 1 temp regs, 0 temp arrays:
// ALU 5 float, 0 int, 0 uint
// TEX 0 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"vs_4_0_level_9_1
eefiecedcbngialmobbpbpljihhbdgcefbooilfkabaaaaaapiacaaaaaeaaaaaa
daaaaaaabiabaaaaemacaaaakaacaaaaebgpgodjoaaaaaaaoaaaaaaaaaacpopp
kaaaaaaaeaaaaaaaacaaceaaaaaadmaaaaaadmaaaaaaceaaabaadmaaaaaaaeaa
abaaabaaaaaaaaaaabaaaaaaaeaaacaaaaaaaaaaaaaaaaaaaaacpoppbpaaaaac
afaaaaiaaaaaapjabpaaaaacafaaabiaabaaapjaaeaaaaaeaaaaadoaabaaoeja
abaaoekaabaaookaafaaaaadaaaaapiaaaaaffjaadaaoekaaeaaaaaeaaaaapia
acaaoekaaaaaaajaaaaaoeiaaeaaaaaeaaaaapiaaeaaoekaaaaakkjaaaaaoeia
aeaaaaaeaaaaapiaafaaoekaaaaappjaaaaaoeiaaeaaaaaeaaaaadmaaaaappia
aaaaoekaaaaaoeiaabaaaaacaaaaammaaaaaoeiappppaaaafdeieefccmabaaaa
eaaaabaaelaaaaaafjaaaaaeegiocaaaaaaaaaaaafaaaaaafjaaaaaeegiocaaa
abaaaaaaaeaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaabaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaaddccabaaaabaaaaaagiaaaaac
abaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaabaaaaaa
abaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaaaaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaa
acaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaa
egiocaaaabaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaal
dccabaaaabaaaaaaegbabaaaabaaaaaaegiacaaaaaaaaaaaaeaaaaaaogikcaaa
aaaaaaaaaeaaaaaadoaaaaabejfdeheoemaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaaebaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafaepfdejfeejepeoaafeeffiedepepfceeaaklkl
epfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
fdfgfpfagphdgjhegjgpgoaafeeffiedepepfceeaaklklkl"
}

SubProgram "gles3 " {
Keywords { }
"!!GLES3#version 300 es


#ifdef VERTEX

#define gl_Vertex _glesVertex
in vec4 _glesVertex;
#define gl_MultiTexCoord0 _glesMultiTexCoord0
in vec4 _glesMultiTexCoord0;

#line 151
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 187
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 181
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 315
struct v2f {
    highp vec4 pos;
    highp vec2 uv1;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[8];
uniform highp vec4 unity_LightPosition[8];
uniform highp vec4 unity_LightAtten[8];
#line 19
uniform highp vec4 unity_SpotDirection[8];
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
#line 23
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
#line 27
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
#line 31
uniform highp vec4 _LightSplitsNear;
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
#line 35
uniform highp vec4 unity_ShadowFadeCenterAndType;
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
#line 39
uniform highp mat4 _Object2World;
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
#line 43
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
#line 47
uniform highp mat4 glstate_matrix_projection;
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
#line 51
uniform lowp vec4 unity_ColorSpaceGrey;
#line 77
#line 82
#line 87
#line 91
#line 96
#line 120
#line 137
#line 158
#line 166
#line 193
#line 206
#line 215
#line 220
#line 229
#line 234
#line 243
#line 260
#line 265
#line 291
#line 299
#line 307
#line 311
#line 321
uniform sampler2D _MainTex;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _onePixelTex;
uniform highp float focalDistance01;
#line 325
uniform highp float focalFalloff;
uniform highp float focalStart01;
uniform highp float focalEnd01;
uniform highp float focalSize;
#line 329
uniform highp vec4 _MainTex_TexelSize;
uniform highp vec4 _CameraDepthTexture_ST;
#line 338
#line 331
v2f vert( in appdata_img v ) {
    #line 333
    v2f o;
    o.pos = (glstate_matrix_mvp * v.vertex);
    o.uv1.xy = ((v.texcoord.xy * _CameraDepthTexture_ST.xy) + _CameraDepthTexture_ST.zw);
    return o;
}
out highp vec2 xlv_TEXCOORD0;
void main() {
    v2f xl_retval;
    appdata_img xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xlt_v.texcoord = vec2(gl_MultiTexCoord0);
    xl_retval = vert( xlt_v);
    gl_Position = vec4(xl_retval.pos);
    xlv_TEXCOORD0 = vec2(xl_retval.uv1);
}


#endif
#ifdef FRAGMENT

#define gl_FragData _glesFragData
layout(location = 0) out mediump vec4 _glesFragData[4];
float xll_saturate_f( float x) {
  return clamp( x, 0.0, 1.0);
}
vec2 xll_saturate_vf2( vec2 x) {
  return clamp( x, 0.0, 1.0);
}
vec3 xll_saturate_vf3( vec3 x) {
  return clamp( x, 0.0, 1.0);
}
vec4 xll_saturate_vf4( vec4 x) {
  return clamp( x, 0.0, 1.0);
}
mat2 xll_saturate_mf2x2(mat2 m) {
  return mat2( clamp(m[0], 0.0, 1.0), clamp(m[1], 0.0, 1.0));
}
mat3 xll_saturate_mf3x3(mat3 m) {
  return mat3( clamp(m[0], 0.0, 1.0), clamp(m[1], 0.0, 1.0), clamp(m[2], 0.0, 1.0));
}
mat4 xll_saturate_mf4x4(mat4 m) {
  return mat4( clamp(m[0], 0.0, 1.0), clamp(m[1], 0.0, 1.0), clamp(m[2], 0.0, 1.0), clamp(m[3], 0.0, 1.0));
}
#line 151
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 187
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 181
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 315
struct v2f {
    highp vec4 pos;
    highp vec2 uv1;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[8];
uniform highp vec4 unity_LightPosition[8];
uniform highp vec4 unity_LightAtten[8];
#line 19
uniform highp vec4 unity_SpotDirection[8];
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
#line 23
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
#line 27
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
#line 31
uniform highp vec4 _LightSplitsNear;
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
#line 35
uniform highp vec4 unity_ShadowFadeCenterAndType;
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
#line 39
uniform highp mat4 _Object2World;
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
#line 43
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
#line 47
uniform highp mat4 glstate_matrix_projection;
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
#line 51
uniform lowp vec4 unity_ColorSpaceGrey;
#line 77
#line 82
#line 87
#line 91
#line 96
#line 120
#line 137
#line 158
#line 166
#line 193
#line 206
#line 215
#line 220
#line 229
#line 234
#line 243
#line 260
#line 265
#line 291
#line 299
#line 307
#line 311
#line 321
uniform sampler2D _MainTex;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _onePixelTex;
uniform highp float focalDistance01;
#line 325
uniform highp float focalFalloff;
uniform highp float focalStart01;
uniform highp float focalEnd01;
uniform highp float focalSize;
#line 329
uniform highp vec4 _MainTex_TexelSize;
uniform highp vec4 _CameraDepthTexture_ST;
#line 338
#line 276
highp float Linear01Depth( in highp float z ) {
    #line 278
    return (1.0 / ((_ZBufferParams.x * z) + _ZBufferParams.y));
}
#line 338
mediump vec4 frag( in v2f i ) {
    highp float d = float( texture( _CameraDepthTexture, i.uv1.xy));
    d = Linear01Depth( d);
    #line 342
    mediump float preDof;
    if ((d > focalDistance01)){
        preDof = ((d - focalDistance01) / (focalEnd01 - focalDistance01));
    }
    else{
        preDof = ((d - focalDistance01) / (focalStart01 - focalDistance01));
    }
    preDof *= focalFalloff;
    #line 346
    return vec4( xll_saturate_f(preDof));
}
in highp vec2 xlv_TEXCOORD0;
void main() {
    mediump vec4 xl_retval;
    v2f xlt_i;
    xlt_i.pos = vec4(0.0);
    xlt_i.uv1 = vec2(xlv_TEXCOORD0);
    xl_retval = frag( xlt_i);
    gl_FragData[0] = vec4(xl_retval);
}


#endif"
}

}
Program "fp" {
// Fragment combos: 1
//   opengl - ALU: 16 to 16, TEX: 1 to 1
//   d3d9 - ALU: 18 to 18, TEX: 1 to 1
//   d3d11 - ALU: 7 to 7, TEX: 1 to 1, FLOW: 1 to 1
//   d3d11_9x - ALU: 7 to 7, TEX: 1 to 1, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
Vector 0 [_ZBufferParams]
Float 1 [focalDistance01]
Float 2 [focalFalloff]
Float 3 [focalStart01]
Float 4 [focalEnd01]
SetTexture 0 [_CameraDepthTexture] 2D
"!!ARBfp1.0
OPTION ARB_precision_hint_fastest;
# 16 ALU, 1 TEX
PARAM c[6] = { program.local[0..4],
		{ 1, 0 } };
TEMP R0;
TEX R0.x, fragment.texcoord[0], texture[0], 2D;
MOV R0.y, c[1].x;
ADD R0.z, -R0.y, c[3].x;
MAD R0.x, R0, c[0], c[0].y;
RCP R0.w, R0.x;
RCP R0.x, R0.z;
ADD R0.z, R0.w, -c[1].x;
ADD R0.y, -R0, c[4].x;
SLT R0.w, c[1].x, R0;
RCP R0.y, R0.y;
MUL R0.x, R0.z, R0;
MUL R0.y, R0.z, R0;
ABS R0.w, R0;
CMP R0.z, -R0.w, c[5].y, c[5].x;
CMP R0.x, -R0.z, R0, R0.y;
MUL_SAT result.color, R0.x, c[2].x;
END
# 16 instructions, 1 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Vector 0 [_ZBufferParams]
Float 1 [focalDistance01]
Float 2 [focalFalloff]
Float 3 [focalStart01]
Float 4 [focalEnd01]
SetTexture 0 [_CameraDepthTexture] 2D
"ps_2_0
; 18 ALU, 1 TEX
dcl_2d s0
def c5, 0.00000000, 1.00000000, 0, 0
dcl t0.xy
texld r0, t0, s0
mad r0.x, r0, c0, c0.y
rcp r1.x, r0.x
add r0.x, -r1, c1
cmp r0.x, r0, c5, c5.y
mov r2.x, c4
mov r3.x, c3
add r2.x, -c1, r2
add r3.x, -c1, r3
add r1.x, r1, -c1
rcp r2.x, r2.x
mul r2.x, r1, r2
rcp r3.x, r3.x
abs_pp r0.x, r0
mul r1.x, r3, r1
cmp_pp r0.x, -r0, r1, r2
mul_pp_sat r0.x, r0, c2
mov_pp r0, r0.x
mov_pp oC0, r0
"
}

SubProgram "d3d11 " {
Keywords { }
ConstBuffer "$Globals" 80 // 32 used size, 8 vars
Float 16 [focalDistance01]
Float 20 [focalFalloff]
Float 24 [focalStart01]
Float 28 [focalEnd01]
ConstBuffer "UnityPerCamera" 128 // 128 used size, 8 vars
Vector 112 [_ZBufferParams] 4
BindCB "$Globals" 0
BindCB "UnityPerCamera" 1
SetTexture 0 [_CameraDepthTexture] 2D 0
// 11 instructions, 1 temp regs, 0 temp arrays:
// ALU 7 float, 0 int, 0 uint
// TEX 1 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"ps_4_0
eefiecedccceojhmnpafjgilnmijpldloaldilchabaaaaaaiaacaaaaadaaaaaa
cmaaaaaaieaaaaaaliaaaaaaejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafdfgfpfagphdgjhegjgpgoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcmaabaaaa
eaaaaaaahaaaaaaafjaaaaaeegiocaaaaaaaaaaaacaaaaaafjaaaaaeegiocaaa
abaaaaaaaiaaaaaafkaaaaadaagabaaaaaaaaaaafibiaaaeaahabaaaaaaaaaaa
ffffaaaagcbaaaaddcbabaaaabaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaac
abaaaaaaefaaaaajpcaabaaaaaaaaaaaegbabaaaabaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadcaaaaalbcaabaaaaaaaaaaaakiacaaaabaaaaaaahaaaaaa
akaabaaaaaaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaakbcaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaaaaaaaaaaaaaaaaj
ccaabaaaaaaaaaaaakaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaaabaaaaaa
dbaaaaaibcaabaaaaaaaaaaaakiacaaaaaaaaaaaabaaaaaaakaabaaaaaaaaaaa
aaaaaaakmcaabaaaaaaaaaaaagiacaiaebaaaaaaaaaaaaaaabaaaaaapgilcaaa
aaaaaaaaabaaaaaaaoaaaaahgcaabaaaaaaaaaaafgafbaaaaaaaaaaakgalbaaa
aaaaaaaadhaaaaajbcaabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadiaaaaaibcaabaaaaaaaaaaaakaabaaaaaaaaaaabkiacaaa
aaaaaaaaabaaaaaadgcaaaafpccabaaaaaaaaaaaagaabaaaaaaaaaaadoaaaaab
"
}

SubProgram "gles " {
Keywords { }
"!!GLES"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES"
}

SubProgram "flash " {
Keywords { }
Vector 0 [_ZBufferParams]
Float 1 [focalDistance01]
Float 2 [focalFalloff]
Float 3 [focalStart01]
Float 4 [focalEnd01]
SetTexture 0 [_CameraDepthTexture] 2D
"agal_ps
c5 0.0 1.0 0.0 0.0
[bc]
ciaaaaaaaaaaapacaaaaaaoeaeaaaaaaaaaaaaaaafaababb tex r0, v0, s0 <2d wrap linear point>
adaaaaaaaaaaabacaaaaaaaaacaaaaaaaaaaaaoeabaaaaaa mul r0.x, r0.x, c0
abaaaaaaaaaaabacaaaaaaaaacaaaaaaaaaaaaffabaaaaaa add r0.x, r0.x, c0.y
afaaaaaaabaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rcp r1.x, r0.x
bfaaaaaaaaaaabacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r0.x, r1.x
abaaaaaaaaaaabacaaaaaaaaacaaaaaaabaaaaoeabaaaaaa add r0.x, r0.x, c1
ckaaaaaaaaaaabacaaaaaaaaacaaaaaaafaaaaaaabaaaaaa slt r0.x, r0.x, c5.x
aaaaaaaaacaaabacaeaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r2.x, c4
aaaaaaaaadaaabacadaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r3.x, c3
aaaaaaaaadaaacacabaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r3.y, c1
bfaaaaaaabaaacacadaaaaffacaaaaaaaaaaaaaaaaaaaaaa neg r1.y, r3.y
abaaaaaaacaaabacabaaaaffacaaaaaaacaaaaaaacaaaaaa add r2.x, r1.y, r2.x
aaaaaaaaaeaaapacabaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r4, c1
bfaaaaaaacaaacacaeaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r2.y, r4.x
abaaaaaaadaaabacacaaaaffacaaaaaaadaaaaaaacaaaaaa add r3.x, r2.y, r3.x
acaaaaaaabaaabacabaaaaaaacaaaaaaabaaaaoeabaaaaaa sub r1.x, r1.x, c1
afaaaaaaacaaabacacaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rcp r2.x, r2.x
adaaaaaaacaaabacabaaaaaaacaaaaaaacaaaaaaacaaaaaa mul r2.x, r1.x, r2.x
afaaaaaaadaaabacadaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rcp r3.x, r3.x
beaaaaaaaaaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa abs r0.x, r0.x
adaaaaaaabaaabacadaaaaaaacaaaaaaabaaaaaaacaaaaaa mul r1.x, r3.x, r1.x
bfaaaaaaadaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r3.x, r0.x
ckaaaaaaadaaabacadaaaaaaacaaaaaaafaaaaaaabaaaaaa slt r3.x, r3.x, c5.x
acaaaaaaacaaabacacaaaaaaacaaaaaaabaaaaaaacaaaaaa sub r2.x, r2.x, r1.x
adaaaaaaaaaaabacacaaaaaaacaaaaaaadaaaaaaacaaaaaa mul r0.x, r2.x, r3.x
abaaaaaaaaaaabacaaaaaaaaacaaaaaaabaaaaaaacaaaaaa add r0.x, r0.x, r1.x
adaaaaaaaaaaabacaaaaaaaaacaaaaaaacaaaaoeabaaaaaa mul r0.x, r0.x, c2
bgaaaaaaaaaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa sat r0.x, r0.x
aaaaaaaaaaaaapacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa mov r0, r0.x
aaaaaaaaaaaaapadaaaaaaoeacaaaaaaaaaaaaaaaaaaaaaa mov o0, r0
"
}

SubProgram "d3d11_9x " {
Keywords { }
ConstBuffer "$Globals" 80 // 32 used size, 8 vars
Float 16 [focalDistance01]
Float 20 [focalFalloff]
Float 24 [focalStart01]
Float 28 [focalEnd01]
ConstBuffer "UnityPerCamera" 128 // 128 used size, 8 vars
Vector 112 [_ZBufferParams] 4
BindCB "$Globals" 0
BindCB "UnityPerCamera" 1
SetTexture 0 [_CameraDepthTexture] 2D 0
// 11 instructions, 1 temp regs, 0 temp arrays:
// ALU 7 float, 0 int, 0 uint
// TEX 1 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"ps_4_0_level_9_1
eefiecedomchnihimjcoelnncphkcdljpihajlbmabaaaaaanaadaaaaaeaaaaaa
daaaaaaahmabaaaaeeadaaaajmadaaaaebgpgodjeeabaaaaeeabaaaaaaacpppp
aeabaaaaeaaaaaaaacaaciaaaaaaeaaaaaaaeaaaabaaceaaaaaaeaaaaaaaaaaa
aaaaabaaabaaaaaaaaaaaaaaabaaahaaabaaabaaaaaaaaaaaaacppppbpaaaaac
aaaaaaiaaaaaadlabpaaaaacaaaaaajaaaaiapkaecaaaaadaaaaapiaaaaaoela
aaaioekaacaaaaadaaaaaciaaaaaaakbaaaappkaagaaaaacaaaaaciaaaaaffia
aeaaaaaeaaaaabiaabaaaakaaaaaaaiaabaaffkaagaaaaacaaaaabiaaaaaaaia
acaaaaadaaaaaeiaaaaaaaiaaaaaaakbacaaaaadaaaaabiaaaaaaaibaaaaaaka
afaaaaadaaaacciaaaaaffiaaaaakkiaacaaaaadaaaaaiiaaaaaaakbaaaakkka
agaaaaacaaaaaiiaaaaappiaafaaaaadaaaaceiaaaaappiaaaaakkiafiaaaaae
aaaaabiaaaaaaaiaaaaakkiaaaaaffiaafaaaaadaaaacbiaaaaaaaiaaaaaffka
abaaaaacaaaadpiaaaaaaaiaabaaaaacaaaicpiaaaaaoeiappppaaaafdeieefc
maabaaaaeaaaaaaahaaaaaaafjaaaaaeegiocaaaaaaaaaaaacaaaaaafjaaaaae
egiocaaaabaaaaaaaiaaaaaafkaaaaadaagabaaaaaaaaaaafibiaaaeaahabaaa
aaaaaaaaffffaaaagcbaaaaddcbabaaaabaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacabaaaaaaefaaaaajpcaabaaaaaaaaaaaegbabaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadcaaaaalbcaabaaaaaaaaaaaakiacaaaabaaaaaa
ahaaaaaaakaabaaaaaaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaakbcaabaaa
aaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaaaaaaaaa
aaaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaa
abaaaaaadbaaaaaibcaabaaaaaaaaaaaakiacaaaaaaaaaaaabaaaaaaakaabaaa
aaaaaaaaaaaaaaakmcaabaaaaaaaaaaaagiacaiaebaaaaaaaaaaaaaaabaaaaaa
pgilcaaaaaaaaaaaabaaaaaaaoaaaaahgcaabaaaaaaaaaaafgafbaaaaaaaaaaa
kgalbaaaaaaaaaaadhaaaaajbcaabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaaa
aaaaaaaackaabaaaaaaaaaaadiaaaaaibcaabaaaaaaaaaaaakaabaaaaaaaaaaa
bkiacaaaaaaaaaaaabaaaaaadgcaaaafpccabaaaaaaaaaaaagaabaaaaaaaaaaa
doaaaaabejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaa
adaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaa
adadaaaafdfgfpfagphdgjhegjgpgoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklkl"
}

SubProgram "gles3 " {
Keywords { }
"!!GLES3"
}

}

#LINE 67

  	}
  }
  
Fallback off

}