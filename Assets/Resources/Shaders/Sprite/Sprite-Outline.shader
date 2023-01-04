Shader "Sprite/Sprite-Outline" 
{
    Properties
    {
        _MainTex("Texture (RGB) Trans (A)", 2D) = "clear" {}
        _OutlineWidth("Outline Width", float) = 1
        _OutlineCutoff("Outline Cutoff", Range(0, 1)) = 0.5
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
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
                half2 texcoord : TEXCOORD;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            half _OutlineWidth;
            half _OutlineCutoff;
            fixed4 _OutlineColor;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t v)
            {
                v2f o; 
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                const float DIV_SQRT_2 = 0.70710678118;
                float2 directions[8] = 
                { 
                    float2(1, 0), float2(0, 1), float2(-1, 0), float2(0, -1),
                    float2(DIV_SQRT_2, DIV_SQRT_2), float2(-DIV_SQRT_2, DIV_SQRT_2),
                    float2(-DIV_SQRT_2, -DIV_SQRT_2), float2(DIV_SQRT_2, -DIV_SQRT_2) 
                };

                float2 sampleDistance = _MainTex_TexelSize.xy * _OutlineWidth;
                float sumAlpha = 0;
                for (uint index = 0; index < 8; ++index)
                {
                    float2 sampleUV = i.texcoord + directions[index] * sampleDistance;
                    sumAlpha += tex2D(_MainTex, sampleUV).a;
                }
                float outline = sumAlpha > _OutlineCutoff;

                fixed4 color = tex2D(_MainTex, i.texcoord);
                return lerp(color * i.color, outline * _OutlineColor, color.a < _OutlineCutoff);
            }
            ENDCG
        }
    }
}