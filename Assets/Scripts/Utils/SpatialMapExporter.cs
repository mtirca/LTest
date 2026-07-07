using System.IO;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Debug-only: bakes and exports the position/normal maps of any mesh, using the same
    /// ArtefactBaker approach as Brush.cs, without depending on the Artefact/multispectral pipeline.
    /// Attach to any GameObject with a MeshFilter and Renderer, assign bakerShader, and run
    /// "Export Position/Normal Maps" from the component's context menu, in Edit or Play mode.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(Renderer))]
    public class SpatialMapExporter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Shader bakerShader;

        [Header("Export Settings")]
        [SerializeField] private int resolution = 2048;
        [SerializeField] private string outputFolder = "Screenshots";

        [ContextMenu("Export Position/Normal Maps")]
        private void ExportMaps()
        {
            if (!bakerShader)
            {
                Debug.LogError("SpatialMapExporter: bakerShader nu este setat.");
                return;
            }

            var meshFilter = GetComponent<MeshFilter>();
            var meshRenderer = GetComponent<Renderer>();
            if (!meshFilter || !meshFilter.sharedMesh || !meshRenderer)
            {
                Debug.LogError("SpatialMapExporter: obiectul nu are un MeshFilter/Renderer valid.");
                return;
            }

            var bakerMaterial = new Material(bakerShader);

            var positionMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat);
            var normalMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat);
            positionMap.Create();
            normalMap.Create();

            RenderBuffer[] mrt = { positionMap.colorBuffer, normalMap.colorBuffer };
            Graphics.SetRenderTarget(mrt, positionMap.depthBuffer);
            GL.Clear(false, true, Color.clear);

            bakerMaterial.SetPass(0);
            Graphics.DrawMeshNow(meshFilter.sharedMesh, transform.localToWorldMatrix);

            Graphics.SetRenderTarget(null);

            Bounds bounds = meshRenderer.bounds;
            SaveRenderTextureAsPNG(positionMap, $"{gameObject.name}_PositionMap.png", bounds);
            SaveRenderTextureAsPNG(normalMap, $"{gameObject.name}_NormalMap.png", null);

            positionMap.Release();
            normalMap.Release();
            DestroyImmediate(bakerMaterial);
        }

        private void SaveRenderTextureAsPNG(RenderTexture rt, string fileName, Bounds? worldBounds)
        {
            var readTex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            readTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readTex.Apply();
            RenderTexture.active = prevActive;

            Color[] pixels = readTex.GetPixels();
            DestroyImmediate(readTex);

            for (int i = 0; i < pixels.Length; i++)
            {
                // Empty UV space (alpha channel from the baker shader): show as black
                if (pixels[i].a < 0.1f)
                {
                    pixels[i] = Color.black;
                    continue;
                }

                if (worldBounds.HasValue)
                {
                    // Position map: remap world-space XYZ into [0, 1] using the object's bounds, purely for visualization
                    Bounds b = worldBounds.Value;
                    float r = Mathf.InverseLerp(b.min.x, b.max.x, pixels[i].r);
                    float g = Mathf.InverseLerp(b.min.y, b.max.y, pixels[i].g);
                    float bl = Mathf.InverseLerp(b.min.z, b.max.z, pixels[i].b);
                    pixels[i] = new Color(r, g, bl, 1f);
                }
                else
                {
                    // Normal map: components are in [-1, 1], remap to [0, 1] for visualization
                    pixels[i] = new Color(pixels[i].r * 0.5f + 0.5f, pixels[i].g * 0.5f + 0.5f, pixels[i].b * 0.5f + 0.5f, 1f);
                }
            }

            var outTex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            outTex.SetPixels(pixels);
            outTex.Apply();

            string folderPath = Path.Combine(Application.dataPath, "..", outputFolder);
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);
            File.WriteAllBytes(filePath, outTex.EncodeToPNG());

            Debug.Log($"Saved debug map to {filePath}");
            DestroyImmediate(outTex);
        }
    }
}
