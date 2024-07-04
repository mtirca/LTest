// combine_labels.hlsl
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

void CombineLabels_float(
    const float4 _VertexColor,
    UnityTexture2D _Labels,
    out float4 _FinalColor)
{
    float4 finalAux = float4(1, 1, 1, 1);
    // _FinalColor = float4(1, 0, 0, 1);

    for (int channel = 0; channel < 4; channel++)
    {
        for (int bit = 0; bit < 8; bit++)
        {
            const int label_index = channel * 8 + bit;
            const int mask = pow(2, bit);
            const int color32 = _VertexColor[channel] * 255;
            const int result = mask & color32;
            const bool is_label_set = result != 0;
    
            if (is_label_set)
            {
                const float2 uv = float2((label_index + 0.5) / 32.0, 0.5);
                const float4 label_color = SAMPLE_TEXTURE2D(_Labels, _Labels.samplerstate, uv);
                finalAux *= label_color;
                if (label_color.a >= 0.5) // Check if the label is visible
                {
                _FinalColor *= label_color;
                }
            }
        }
    }
    
    finalAux.a = 0.5;
    _FinalColor = finalAux;
}
