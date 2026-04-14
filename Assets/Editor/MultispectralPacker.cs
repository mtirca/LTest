using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Editor
{
    public class MultispectralPacker
    {
        [MenuItem("Assets/Create/Texture2DArray (Multispectral Cube)")]
        private static void CreateTextureArray()
        {
            // 1. Get the textures you select in the Project window
            Texture2D[] selectedTextures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets)
                .OrderBy(t => t.name) // Sort alphabetically so 360nm is first
                .ToArray();

            // 2. Grab dimensions from the first texture (2048x2048)
            int width = selectedTextures[0].width;
            int height = selectedTextures[0].height;
            int depth = selectedTextures.Length; 

            // 3. Create the empty Array using R16 format to preserve your scientific data
            var textureArray = new Texture2DArray(width, height, depth, TextureFormat.R16, false, true)
                {
                    filterMode = FilterMode.Point, // Hard pixels, no blurring
                    wrapMode = TextureWrapMode.Clamp
                };

            // 4. Copy each image into the array one by one
            for (int i = 0; i < depth; i++)
            {
                Texture2D tex = selectedTextures[i];
                Graphics.CopyTexture(tex, 0, 0, textureArray, i, 0);
            }

            // 5. Save the final file
            string path = "Assets/House Painting/MultispectralDataCube.asset";
            AssetDatabase.CreateAsset(textureArray, path);
        
            Debug.Log("Success! Created the data cube at " + path);
        }
    }
}
