using System.IO;
using System.Linq;
using ArtefactSystem;
using Player.Movement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Utils;

namespace Tools
{
    public class Brush : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Artefact artefact;
        [SerializeField] private LabelManager labelManager;
        [SerializeField] private MovementManager movementManager;

        [Header("GPU Resources")]
        public ComputeShader brushCompute;
        public Shader bakerShader;

        [Header("Brush Settings")] 
        public float brushRadius = 0.5f;
        public bool isErasing;

        [Header("Advanced Rendering")] 
        public int maxLabels = 64;

        // GPU Buffers
        // Tex2DArray, where each slice contains that label's paint, as R8. So if on slice L, pixel P is colored with 1.0f,
        // then pixel P is labeled with label L
        public RenderTexture MaskTexArray { get; private set; }
        private RenderTexture _positionMap;
        private RenderTexture _normalMap;
        
        private Texture2D _paletteTexture;
        private Renderer _artefactRenderer;
        private Material _bakerMaterial;
        private Mesh _targetMesh;
        private int _computeKernel;

        // Shader Properties
        private static readonly int MaskArrayID = Shader.PropertyToID("_MaskArray");
        private static readonly int PaletteID = Shader.PropertyToID("_Palette");
        private static readonly int ActiveCountID = Shader.PropertyToID("_LabelCount");
        private static readonly int PositionMap = Shader.PropertyToID("_PositionMap");
        private static readonly int NormalMap = Shader.PropertyToID("_NormalMap");
        private static readonly int HitPosition = Shader.PropertyToID("_HitPosition");
        private static readonly int HitNormal = Shader.PropertyToID("_HitNormal");
        private static readonly int BrushRadius = Shader.PropertyToID("_BrushRadius");
        private static readonly int PaintValue = Shader.PropertyToID("_PaintValue");
        private static readonly int ActiveSlice = Shader.PropertyToID("_ActiveSlice");
        private static readonly int MaskResolution = Shader.PropertyToID("_MaskResolution");

        private void Awake()
        {
            _targetMesh = artefact.GetComponent<MeshFilter>().sharedMesh;
            _artefactRenderer = artefact.GetComponent<Renderer>();
            _bakerMaterial = new Material(bakerShader);
            _computeKernel = brushCompute.FindKernel("PaintMask");

            InitializeGPUArrays();
            BakeSpatialData();
        }

        private void OnEnable()
        {
            UpdateShaderVariables();
        }

        private void OnDisable()
        {
            if (_artefactRenderer != null)
            {
                UpdateShaderVariables();
            }
        }
        
        private void InitializeGPUArrays()
        {
            RenderTextureDescriptor desc = new RenderTextureDescriptor(artefact.MSTex.width, artefact.MSTex.height, RenderTextureFormat.R8)
                {
                    dimension = TextureDimension.Tex2DArray,
                    volumeDepth = maxLabels,
                    enableRandomWrite = true,
                    sRGB = false,
                    useMipMap = false,
                    autoGenerateMips = false
                };
            MaskTexArray = new RenderTexture(desc)
            {
                filterMode = FilterMode.Point
            };
            MaskTexArray.Create();

            // Setup Spatial Maps for the Baker
            _positionMap = new RenderTexture(artefact.MSTex.width, artefact.MSTex.height, 0, RenderTextureFormat.ARGBFloat)
                {
                    enableRandomWrite = true
                };
            _positionMap.Create();

            _normalMap = new RenderTexture(artefact.MSTex.width, artefact.MSTex.height, 0, RenderTextureFormat.ARGBFloat)
                {
                    enableRandomWrite = true
                };
            _normalMap.Create();

            // Initialize Palette
            _paletteTexture = TextureUtils.CreateRaw(maxLabels, 1, TextureFormat.RGBA32);

            // Bind to the Artifact's material's shader
            _artefactRenderer.material.SetTexture(MaskArrayID, MaskTexArray);
            _artefactRenderer.material.SetTexture(PaletteID, _paletteTexture);

            // Bind to the compute shader
            brushCompute.SetTexture(_computeKernel, PositionMap, _positionMap);
            brushCompute.SetTexture(_computeKernel, NormalMap, _normalMap);
            brushCompute.SetTexture(_computeKernel, MaskArrayID, MaskTexArray);
            
            UpdateShaderVariables();
        }

        private void BakeSpatialData()
        {
            RenderBuffer[] mrt = { _positionMap.colorBuffer, _normalMap.colorBuffer };
            Graphics.SetRenderTarget(mrt, _positionMap.depthBuffer);
            GL.Clear(false, true, Color.clear);
            
            _bakerMaterial.SetPass(0);
            Graphics.DrawMeshNow(_targetMesh, artefact.transform.localToWorldMatrix);
            
            Graphics.SetRenderTarget(null);
        }

        private void Update()
        {
            if (!Input.GetMouseButton(0)
                || movementManager.Movement != Movement.None
                || EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            PaintSurface();
        }

        private void PaintSurface()
        {
            if (labelManager.activeLabel == null)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit) || hit.collider.gameObject != artefact.gameObject)
            {
                return;
            }

            ExecuteGPUPaint(hit.point, hit.normal);
        }

        private void ExecuteGPUPaint(Vector3 hitPos, Vector3 hitNorm)
        {
            // Upload Variables
            brushCompute.SetVector(HitPosition, hitPos);
            brushCompute.SetVector(HitNormal, hitNorm);
            brushCompute.SetFloat(BrushRadius, brushRadius);
            
            // Set paint value based on toggle (1.0 for painting, 0.0 for erasing)
            brushCompute.SetFloat(PaintValue, isErasing ? 0.0f : 1.0f);
            
            // Sync with your Label Manager
            brushCompute.SetInt(ActiveSlice, labelManager.activeLabel.sliceIndex);
            brushCompute.SetInts(MaskResolution, artefact.MSTex.width, artefact.MSTex.height);
            
            // Dispatch
            int threadGroupsX = Mathf.CeilToInt(artefact.MSTex.width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(artefact.MSTex.height / 8.0f);
            brushCompute.Dispatch(_computeKernel, threadGroupsX, threadGroupsY, 1);
        }
        
        /// <summary>
        /// To be called whenever a label is created, deleted, or changes color.
        /// </summary>
        public void UpdateShaderVariables()
        {
            if (enabled)
            {
                // Tell the shader the highest index we are currently using, so it doesn't loop unnecessarily (performance reasons only)
                int highestIndex = labelManager.allLabels.Select(label => label.sliceIndex).Prepend(0).Max();
                _artefactRenderer.material.SetFloat(ActiveCountID, highestIndex + 1);
            }
            else
            {
                _artefactRenderer.material.SetFloat(ActiveCountID, 0);
            }

            // Update the Nx1 color palette
            for (int i = 0; i < maxLabels; i++)
            {
                _paletteTexture.SetPixel(i, 0, Color.clear);
            }

            if (labelManager.activeLabel != null)
            {
                _paletteTexture.SetPixel(labelManager.activeLabel.sliceIndex, 0, labelManager.activeLabel.color);
            }

            _paletteTexture.Apply();
        }

        /// <summary>
        /// Debug-only: dumps the baked position and normal maps as PNGs, for illustration purposes
        /// (e.g. thesis figures). Right-click this component in the Inspector while in Play mode to run it.
        /// Values are remapped into [0, 1] purely for visualization; they are not used by the app itself.
        /// </summary>
        [ContextMenu("Debug: Save Position/Normal Maps as PNG")]
        private void SaveDebugMaps()
        {
            Bounds bounds = _artefactRenderer.bounds;
            SaveRenderTextureAsPNG(_positionMap, "PositionMap_debug.png", bounds);
            SaveRenderTextureAsPNG(_normalMap, "NormalMap_debug.png", null);
        }

        private static void SaveRenderTextureAsPNG(RenderTexture rt, string fileName, Bounds? worldBounds)
        {
            var readTex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            readTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readTex.Apply();
            RenderTexture.active = prevActive;

            Color[] pixels = readTex.GetPixels();
            Destroy(readTex);

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
                    // Position map: remap world-space XYZ into [0, 1] using the artefact's bounds, purely for visualization
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

            string folderPath = Path.Combine(Application.dataPath, "..", "Screenshots");
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);
            File.WriteAllBytes(filePath, outTex.EncodeToPNG());

            Debug.Log($"Saved debug map to {filePath}");
            Destroy(outTex);
        }

        private void OnDestroy()
        {
            if (MaskTexArray != null) MaskTexArray.Release();
            if (_positionMap != null) _positionMap.Release();
            if (_normalMap != null) _normalMap.Release();
            if (_bakerMaterial != null) Destroy(_bakerMaterial);
        }
    }
}