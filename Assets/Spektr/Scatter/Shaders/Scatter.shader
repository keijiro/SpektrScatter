//
// Scatter - polygon scattering effect
//
Shader "Spektr/Scatter/Standard"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo", 2D) = "white" {}

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _BumpScale("Scale", Range(0.0, 2.0)) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0, 0, 0)
        _EmissionMap("Emission", 2D) = "white" {}

        _BackColor("Back Color", Color) = (1, 1, 1, 1)
        _BackGlossiness("Back Smoothness", Range(0.0, 1.0)) = 0.5
        [Gamma] _BackMetallic("Back Metallic", Range(0.0, 1.0)) = 0.0

        _TransitionAxis("Transition Axis", Vector) = (0, 1, 0)
        _TransitionBase("Transition Base", float) = 0
        _TransitionSpeed("Transition Speed", float) = 1
        _TransitionTime("Transition Time", float) = 0
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

    float3 _TransitionAxis;
    float _TransitionBase;
    float _TransitionSpeed;
    float _TransitionTime;

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

    // Uniformaly distributed points
    // http://mathworld.wolfram.com/SpherePointPicking.html
    float3 random_axis(float2 uv)
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
        float3 p_c = v.texcoord1.xyz;

        // time parameter
        float t_c = _TransitionBase - dot(p_c, _TransitionAxis);
        t_c /= _TransitionSpeed;
        t_c += _TransitionTime;

        // emission
        data.emission = (1.0 - saturate(t_c)) * (t_c > 0.0);

        t_c = max(t_c, 0.0);
        t_c *= 0.99;

        float itc = (1.0 - saturate(t_c * 0.3)) * (t_c > 0.0);

        // translation
        float3 move = random_axis(p_c.xy) * 0.04 * cnoise(p_c * 5 + float3(33, t_c * 2, 21.4)) * (t_c > 0.0);
        move *= itc;

        // rotation
        float r_a = cnoise(p_c * 2 + float3(0, t_c * 1.2, 0)) * 4;
        r_a *= itc;
        //float4 rotation = rotation_angle_axis(r_a, random_axis(p_c.xy));
        float4 rotation = rotation_angle_axis(r_a, float3(1, 0, 0));

        // scaling
        //float scale = 1.0 + sin(min(t_c * 2.4, UNITY_PI * 1.5)) * max(2.0 - 2.0 * t_c * 2.4 / (UNITY_PI * 1.5), 1.0);
        float scale = 1.0 + lerp(-1.0, 2.5, itc) * (t_c > 0.0);

        // apply transform in triangle-local space
        float3 p_v = v.vertex.xyz - p_c;
        p_v = rotate_vector(p_v, rotation);
        p_v += move;
        p_v *= scale;
        v.vertex.xyz = p_v + p_c;

        // rotate normal
        v.normal = rotate_vector(v.normal, rotation) * flipNormal;

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

            //o.Emission += saturate(1 - abs(IN.emission)) * 8;
            o.Emission += abs(IN.emission);
        }

        ENDCG

        Cull Front

        CGPROGRAM

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma target 3.0

        #pragma shader_feature _METALLICGLOSSMAP
        #pragma shader_feature _NORMALMAP
        #pragma shader_feature _EMISSION

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
            o.Emission = abs(IN.emission);
        }

        ENDCG
    }
    FallBack "Diffuse"
    CustomEditor "Spektr.ScatterStandardMaterialEditor"
}
