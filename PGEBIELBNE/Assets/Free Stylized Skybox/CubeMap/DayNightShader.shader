Shader "Skybox/DayNightSkybox" {
    Properties{
        _Tint("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
        _Rotation("Rotation", Range(0, 360)) = 0
        _Blend("Blend", Range(0.001, 1.0)) = 0.001
        [NoScaleOffset] _Tex("Cubemap   (HDR)", Cube) = "grey" {}
        _Tex1("Cubemap1", CUBE) = ""
        _Tex2("Cubemap2", CUBE) = ""
    }

        SubShader{
            Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
            Cull Off ZWrite Off

            Pass {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"

                samplerCUBE _Tex1;
                samplerCUBE _Tex2;
                half4 _Tex_HDR;
                half4 _Tint;
                half _Exposure;
                float _Rotation;
                float _Blend;

                float3 RotateAroundYInDegrees(float3 vertex, float degrees)
                {
                    float alpha = degrees * UNITY_PI / 180.0;
                    float sina, cosa;
                    sincos(alpha, sina, cosa);
                    float2x2 m = float2x2(cosa, -sina, sina, cosa);
                    return float3(mul(m, vertex.xz), vertex.y).xzy;
                }

                struct appdata_t {
                    float4 vertex : POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    float3 texcoord : TEXCOORD0;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                v2f vert(appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
                    o.vertex = UnityObjectToClipPos(rotated);
                    o.texcoord = v.vertex.xyz;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    half4 tex1 = texCUBE(_Tex1, i.texcoord);
                    half4 tex2 = texCUBE(_Tex2, i.texcoord);
                    half4 tex = lerp(tex1, tex2, _Blend);
                    half3 c = DecodeHDR(tex, _Tex_HDR);
                    c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                    c *= _Exposure;
                    return half4(c, 1);
                }
                    ENDCG
        }
        }
            Fallback Off

}