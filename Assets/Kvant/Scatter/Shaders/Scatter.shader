Shader "Kvant/Scatter"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        _NormalMap      ("-", 2D) = "bump"{}
        _NormalScale    ("-", Range(0,2)) = 1
        _OcclusionMap   ("-", 2D) = "white"{}
        _OcclusionStr   ("-", Float) = 1.0
    }
    SubShader
    {
        Cull off
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #include "ClassicNoise3D.cginc"

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma target 3.0

        sampler2D _MainTex;

        sampler2D _NormalMap;
        half _NormalScale;

        sampler2D _OcclusionMap;
        half _OcclusionStr;

        sampler2D _NormalBuffer;

        struct Input
        {
            float2 uv_MainTex;
            float emission;
        };

        half _Glossiness;
        half _Metallic;
        half4 _Color;

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

            float param2 = (10 - _Time.y) * 0.6 * 4 + cp.y * 8 - 4;
            float param = max(0, param2);

            float3 nv = float3(
                cnoise(cp * 5),
                cnoise(cp * 5 + float3(2.3, 3.3, 4.3)),
                cnoise(cp * 5 + float3(8.3, 8.3, 3.3))
            );

            float r = param * 3;
            float3 ra = random_axis(cp.xy + cp.z * 10);
            float4 rq = float4(ra * sin(r * 0.5), cos(r * 0.5));

            float sc = 2.0 / (2.0 + param);

            float3 vp = v.vertex.xyz - cp;
            vp = rotate_vector(vp, rq) * sc;
            vp += nv * param * 0.7;
            v.vertex.xyz = vp + cp;

            data.emission = param2;
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            half4 n = tex2D(_NormalMap, IN.uv_MainTex);
            o.Normal = UnpackScaleNormal(n, _NormalScale);

            half occ = tex2D(_OcclusionMap, IN.uv_MainTex).g;
            o.Occlusion = lerp(1, occ, _OcclusionStr);

            o.Emission = saturate(1 - abs(IN.emission)) * 8;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
