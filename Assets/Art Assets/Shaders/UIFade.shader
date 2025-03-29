// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/UIFade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GradientStrength ("Gradient Strength", Range(0, 1)) = 1
        _GradientStart ("Gradient Start Position", Range(0, 1)) = 0.2
        _ScrollSpeed ("Scroll Speed", Range(0, 2)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _GradientStrength;
            float _GradientStart;
            float _ScrollSpeed;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Calculate scrolling offset
                float scrollOffset = _Time.y * _ScrollSpeed;
                o.uv = v.uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float scrollOffset = _Time.y * _ScrollSpeed;

                // Two-layer scrolling technique
                float2 uv1 = i.uv;
                uv1.y = frac(i.uv.y + scrollOffset);

                float2 uv2 = i.uv;
                uv2.y = frac(i.uv.y + scrollOffset - 1.0); // Second layer shifted up

                // Blend factor (creates a smooth transition between layers)
                float blendFactor = frac(scrollOffset);

                fixed4 col1 = tex2D(_MainTex, uv1);
                fixed4 col2 = tex2D(_MainTex, uv2);

                // Smoothly blend the two textures together
                fixed4 col = lerp(col1, col2, blendFactor);

                // Apply gradient fade
                float gradient = saturate((i.uv.y - _GradientStart) / (1.0 - _GradientStart));
                gradient = (gradient * _GradientStrength) + (1.0 - _GradientStrength);
                gradient = saturate(gradient);

                col.a *= gradient; // Apply alpha fade
                return col;
            }
            ENDCG
        }
    }
}