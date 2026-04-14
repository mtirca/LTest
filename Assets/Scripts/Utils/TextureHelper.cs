using UnityEngine;

namespace Utils
{
    public static class TextureHelper
    {
        /// <summary>
        /// Computes the pixel index of a texture based on the given UV mapping. The result
        /// is clamped to the texture's dimensions.
        /// </summary>
        /// <param name="tex2D">The texture.</param>
        /// <param name="uv">The UV coordinates [0, 1].</param>
        /// <returns>The computed pixel index.</returns>
        public static int ComputePixelIndex(Texture tex2D, Vector2 uv)
        {
            int width = tex2D.width;
            int height = tex2D.height;

            int pixelX = Mathf.FloorToInt(uv.x * width);
            int pixelY = Mathf.FloorToInt(uv.y * height);

            pixelX = Mathf.Clamp(pixelX, 0, width - 1);
            pixelY = Mathf.Clamp(pixelY, 0, height - 1);

            return pixelY * width + pixelX;
        }
    }
}