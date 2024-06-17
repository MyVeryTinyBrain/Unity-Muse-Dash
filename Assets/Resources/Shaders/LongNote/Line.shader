// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "LongNote/Line" 
{
    Properties
    {
        _Texture0("Texture0 (RGB) Trans (A)", 2D) = "clear" {}
        _Texture1("Texture1 (RGB) Trans (A)", 2D) = "clear" {}
        _Texture2("Texture2 (RGB) Trans (A)", 2D) = "clear" {}
        _Texture1WaveAmount("Texture1 Wave Amount", Range(-1,1)) = -0.07
        _Texture2WaveAmount("Texture2 Wave Amount", Range(-1,1)) = 0.07
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            { 
                float4 vertex : SV_POSITION;
                half2 texcoord0 : TEXCOORD0;
                half2 texcoord1 : TEXCOORD1;
                half2 texcoord2 : TEXCOORD2;
                float4 color : COLOR;
            };

            sampler2D _Texture0;
            sampler2D _Texture1;
            sampler2D _Texture2;
            float4 _Texture0_ST;
            float4 _Texture1_ST;
            float4 _Texture2_ST;
            float _Texture1WaveAmount;
            float _Texture2WaveAmount;

            v2f vert(appdata_t v)
            {
                v2f o; 
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;

                o.texcoord0 = TRANSFORM_TEX(v.texcoord, _Texture0);

                float sinTime = _SinTime.w;
                float sqrSinTime = pow(sinTime, 2);
                // 1번, 2번 텍스쳐가 위 아래로 움직이도록 uv좌표를 조작합니다.
                o.texcoord1 = TRANSFORM_TEX(v.texcoord, _Texture1) + float2(0, sqrSinTime * _Texture1WaveAmount);
                o.texcoord2 = TRANSFORM_TEX(v.texcoord, _Texture2) + float2(0, sqrSinTime * _Texture2WaveAmount);
                return o;
            }

            fixed4 SampleBlend(v2f i)
            {
                fixed4 tex0 = tex2D(_Texture0, i.texcoord0);
                fixed4 tex1 = tex2D(_Texture1, i.texcoord1);
                fixed4 tex2 = tex2D(_Texture2, i.texcoord2);

                // 세 개의 텍스쳐를 혼합합니다.
                fixed4 blend01 = lerp(tex0, tex1, tex1.a);
                fixed4 blend012 = lerp(blend01, tex2, tex2.a);
                return blend012;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = SampleBlend(i);
                return color * i.color;
            }
            ENDCG
        }
    }
}