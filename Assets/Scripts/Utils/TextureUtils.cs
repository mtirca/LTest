using UnityEngine;

namespace Utils
{
    public static class TextureUtils
    {
        private static Texture2D _cachedBlankTexture;

        /// <summary>
        /// Returns a new unoptimized Texture2D (able to hold raw scientific data).
        /// </summary>
        public static Texture2D CreateRaw(int width, int height, TextureFormat format)
        {
            return new Texture2D(width, height, format, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        /// <summary>
        /// Returns a cached, pure black, raw R8 texture of the requested size.
        /// </summary>
        public static Texture2D GetBlankR8(int width, int height)
        {
            // If we already built it and the size matches, return the cached copy
            if (_cachedBlankTexture != null &&
                _cachedBlankTexture.width == width &&
                _cachedBlankTexture.height == height)
            {
                return _cachedBlankTexture;
            }

            // If the requested size changed (or it's the first time), destroy the old one
            if (_cachedBlankTexture != null)
            {
                Object.Destroy(_cachedBlankTexture);
            }

            // Build the new linear texture
            _cachedBlankTexture = CreateRaw(width, height, TextureFormat.R8);

            // Fill it with zeros
            Color32[] clearColors = new Color32[width * height];
            _cachedBlankTexture.SetPixels32(clearColors);
            _cachedBlankTexture.Apply();

            return _cachedBlankTexture;
        }

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