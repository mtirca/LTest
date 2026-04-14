#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#endif

/**
 * 
 * @param mask_array The 3D stack of 64 textures, one for each label.
 * @param palette A 1x64 2D texture, each representing the color of a label.
 * @param uv The 2D coordinate of the artifact's texture.
 * @param label_count How many labels exist on the artifact.
 * @param out_color The calculated RGB color.
 * @param out_alpha The calculated alpha channel of the output color.
 */
void stack_labels_float(UnityTexture2DArray mask_array, UnityTexture2D palette, float2 uv, float label_count,
                        out float3 out_color, out float out_alpha)
{
    float3 final_color = float3(0, 0, 0);
    float final_alpha = 0;
    int label_count_int = (int)label_count;

    for (int i = 0; i < label_count_int; i++)
    {
        // 1. Sample the array slice. We use the Red channel (R) for opacity.
        float paint_opacity = SAMPLE_TEXTURE2D_ARRAY(mask_array, mask_array.samplerstate, uv, i).r;

        if (paint_opacity > 0.01)
        {
            // 2. Look up the color for this specific slice from our 64x1 Palette
            // We add 0.5 to sample the exact center of the pixel to prevent color bleeding
            float2 palette_uv = float2((i + 0.5) / 64.0, 0.5);
            float4 label_color = SAMPLE_TEXTURE2D(palette, palette.samplerstate, palette_uv);

            // 3. Blend the colors together (Standard Alpha Blending)
            final_color = lerp(final_color, label_color.rgb, paint_opacity * label_color.a);
            final_alpha = saturate(final_alpha + paint_opacity * label_color.a);
        }
    }

    out_color = final_color;
    out_alpha = final_alpha;
}
