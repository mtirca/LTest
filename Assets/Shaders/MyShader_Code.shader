Shader "Custom/MyShader_Code"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        float4 _ColorArray[32];

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 color: COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float4 CombineLabels(float4 vColor)
        {
            float4 final_color = float4(1, 1, 1, 1);

            for (int channel = 0; channel < 4; channel++)
            {
                for (int bit = 0; bit < 8; bit++)
                {
                    const int label_index = channel * 8 + bit;
                    const int mask = 1 << bit;
                    // const int color32 = round(vColor[channel] * 255);
                    const float f = vColor[channel];
                    const int color32 = f >= 1.0 ? 255 : f <= 0.0 ? 0 : (int)floor(f * 256.0);
                    const int result = mask & color32;
                    const bool is_label_set = result != 0;

                    if (is_label_set)
                    {
                        const float4 label_color = _ColorArray[label_index];
                        if (label_color.a >= 0.5) // Check if the label is visible
                        {
                        final_color *= label_color;
                        }
                    }
                }
            }

            final_color.a = 0.5;
            return final_color;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            float4 mainTexColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Combined labels color
            float4 labelColor = CombineLabels(IN.color);

            float4 c = mainTexColor * labelColor;

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}