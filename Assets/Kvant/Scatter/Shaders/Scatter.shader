Shader "Kvant/Scatter"
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

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _Transition("Transition", Float) = 0
    }
    SubShader
    {
        Cull off

        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #include "ClassicNoise3D.cginc"

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma target 3.0

        #pragma shader_feature _METALLICGLOSSMAP
        #pragma shader_feature _NORMALMAP
        #pragma shader_feature _EMISSION

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

        float _Transition;

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

        void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);

            float3 cp = v.texcoord1.xyz;

            float param2 = (10 - _Transition) * 0.6 * 4 + cp.y * 8 - 4;
            float param = max(0, param2);

            float3 nv = float3(
                cnoise(cp * 5),
                cnoise(cp * 5 + float3(2.3, 3.3, 4.3)),
                cnoise(cp * 5 + float3(8.3, 8.3, 3.3))
            );

            float r = param;
            float3 ra = random_axis(cp.xy + cp.z * 10);
            float4 rq = float4(ra * sin(r * 0.5), cos(r * 0.5));

            float sc = 1;//2.0 / (2.0 + param);

            float3 vp = v.vertex.xyz - cp;
            vp = rotate_vector(vp, rq) * sc;
            vp += nv * param * 0.03;
            v.vertex.xyz = vp + cp;

            data.emission = param2;
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

            o.Emission += saturate(1 - abs(IN.emission)) * 8;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
