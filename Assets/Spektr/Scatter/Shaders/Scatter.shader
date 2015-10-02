//
// SpektrScatter - Polygon scatter effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
Shader "Spektr/Scatter/Standard"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
        _MainTex("", 2D) = "white" {}

        _Glossiness("", Range(0.0, 1.0)) = 0.5
        [Gamma] _Metallic("", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("", 2D) = "white" {}

        _BumpScale("", Range(0.0, 2.0)) = 1.0
        _BumpMap("", 2D) = "bump" {}

        _OcclusionStrength("", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("", 2D) = "white" {}

        _EmissionColor("", Color) = (0, 0, 0)
        _EmissionMap("", 2D) = "white" {}

        _BackColor("", Color) = (0.5, 0.5, 0.5, 1)
        _BackGlossiness("", Range(0.0, 1.0)) = 0.1
        [Gamma] _BackMetallic("", Range(0.0, 1.0)) = 0.0
    }

    CGINCLUDE

    #include "ClassicNoise3D.cginc"

    half4 _Color;
    sampler2D _MainTex;

    half _Glossiness;
    half _Metallic;
    sampler2D _MetallicGlossMap;

    half _BumpScale;
    sampler2D _BumpMap;

    half _OcclusionStrength;
    sampler2D _OcclusionMap;

    half3 _EmissionColor;
    sampler2D _EmissionMap;

    half4 _BackColor;
    half _BackGlossiness;
    half _BackMetallic;

    float4 _EffectorAxis;
    float3 _EffectorSize;

    float3 _PNoise;
    float3 _RNoise;
    float _Inflation;

    struct Input
    {
        float2 uv_MainTex;
        float emission;
    };

    // PRNG function
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, 0);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // Quaternion multiplication
    // http://mathworld.wolfram.com/Quaternion.html
    float4 qmul(float4 q1, float4 q2)
    {
        return float4(
            q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
            q1.w * q2.w - dot(q1.xyz, q2.xyz)
        );
    }

    // Uniformaly distributed points on a unit sphere
    // http://mathworld.wolfram.com/SpherePointPicking.html
    float3 random_point_on_sphere(float2 uv)
    {
        float u = nrand(uv, 0) * 2 - 1;
        float theta = nrand(uv, 1) * UNITY_PI * 2;
        float u2 = sqrt(1 - u * u);
        return float3(u2 * cos(theta), u2 * sin(theta), u);
    }

    // Vector rotation with a quaternion
    // http://mathworld.wolfram.com/Quaternion.html
    float3 rotate_vector(float3 v, float4 r)
    {
        float4 r_c = r * float4(-1, -1, -1, 1);
        return qmul(r, qmul(float4(v, 0), r_c)).xyz;
    }

    // A given angle of rotation about a given aixs
    float4 rotation_angle_axis(float angle, float3 axis)
    {
        float sn, cs;
        sincos(angle * 0.5, sn, cs);
        return float4(axis * sn, cs);
    }

    void vert_common(inout appdata_full v, out Input data, float flipNormal)
    {
        UNITY_INITIALIZE_OUTPUT(Input, data);

        // position of centroid
        float3 center = v.texcoord1.xyz;

        // transition parameter at centroid
        float trans = _EffectorAxis.w - dot(center, _EffectorAxis.xyz);
        trans = trans / _EffectorSize.y + 0.5;

        // unit step function
        float unit = trans > 0.0;

        // decay parameter
        float decay = saturate(1.0 - trans) * unit;

        // displacement
        float3 pnoise = center * _PNoise.x + float3(18.4, 28.1, 21.4);
        pnoise += _EffectorAxis.xyz * _PNoise.y * _Time.y;

        float3 displace = random_point_on_sphere(center.xy);
        displace *= cnoise(pnoise) * _PNoise.z * decay;

        // rotation
        float3 rnoise = center * _RNoise.x + float3(23.1, 38.4, 15.3);
        rnoise += _EffectorAxis.xyz * _RNoise.y * _Time.y;

        float3 raxis = normalize(cross(_EffectorAxis.xyz, center));
        float rangle = cnoise(rnoise) * _RNoise.z * decay;

        float4 rotation = rotation_angle_axis(rangle, raxis);

        // scaling
        float scale = 1.0 + lerp(-1, _Inflation - 1, decay) * unit;

        // apply transform in triangle-local space
        float3 p_v = v.vertex.xyz - center;

        p_v = rotate_vector(p_v, rotation);
        p_v += displace;
        p_v *= scale;

        v.vertex.xyz = p_v + center;

        // rotate normal vector
        v.normal = rotate_vector(v.normal, rotation) * flipNormal;

        // emission
        data.emission = pow(decay, 8);
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma target 3.0

        #pragma shader_feature _METALLICGLOSSMAP
        #pragma shader_feature _NORMALMAP
        #pragma shader_feature _EMISSION

        void vert(inout appdata_full v, out Input data)
        {
            vert_common(v, data, 1);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            half4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            #ifdef _METALLICGLOSSMAP
            half4 mg = tex2D(_MetallicGlossMap, IN.uv_MainTex);
            o.Metallic = mg.r;
            o.Smoothness = mg.a;
            #else
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            #endif

            #ifdef _NORMALMAP
            half4 n = tex2D(_BumpMap, IN.uv_MainTex);
            o.Normal = UnpackScaleNormal(n, _BumpScale);
            #endif

            half occ = tex2D(_OcclusionMap, IN.uv_MainTex).g;
            o.Occlusion = LerpOneTo(occ, _OcclusionStrength);

            #ifdef _EMISSION
            half3 e = tex2D(_EmissionMap, IN.uv_MainTex).rgb;
            o.Emission = e * _EmissionColor.rgb;
            #endif

            o.Emission += IN.emission;
        }

        ENDCG

        Cull Front

        CGPROGRAM

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma target 3.0

        void vert(inout appdata_full v, out Input data)
        {
            vert_common(v, data, -1);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _BackColor.rgb;
            o.Alpha = _BackColor.a;
            o.Metallic = _BackMetallic;
            o.Smoothness = _BackGlossiness;
            o.Emission = IN.emission;
        }

        ENDCG
    }
    FallBack "Diffuse"
    CustomEditor "Spektr.ScatterStandardMaterialEditor"
}
