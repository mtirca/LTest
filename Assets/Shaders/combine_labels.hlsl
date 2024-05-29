// combine_labels.hlsl

void combine_labels_float(
    const float4 vertex_color,
    const UnityTexture2D label_colors,
    const UnityTexture2D label_visibility,
    out float4 final_color)
{
    final_color = float4(1, 1, 1, 1);

    for (int channel = 0; channel < 4; channel++)
    {
        for (int bit = 0; bit < 8; bit++)
        {
            const int label_index = channel * 8 + bit;
            const float mask = pow(2.0, bit);
            const float is_label_set = step(0.5, fmod(vertex_color[channel] / mask, 2.0));
            
            if (is_label_set > 0.5)
            {
                const float2 uv = float2((label_index + 0.5) / 32.0, 0.5);
                const float4 label_color = tex2D(label_colors, uv);
                const float4 visibility = tex2D(label_visibility, uv);

                if (visibility.r > 0.5) // Check if the label is visible
                {
                    final_color *= label_color;
                }
            }
        }
    }
}
