using System.Linq;
using ArtefactSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace Tools
{
    public class Brush : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Artefact artefact;
        [SerializeField] private LabelManager labelManager;

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
            _artefactRenderer.material.SetFloat(ActiveCountID, 0);
        }
        
        private void InitializeGPUArrays()
        {
            RenderTextureDescriptor desc = new RenderTextureDescriptor(artefact.MSTex.width, artefact.MSTex.height, RenderTextureFormat.R8)
                {
                    dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray,
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
            //todo check sampler.cs and make the same check at the beginning
            if (!Input.GetMouseButton(0) || EventSystem.current.IsPointerOverGameObject())
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
            // Tell the shader the highest index we are currently using, so it doesn't loop unnecessarily (performance reasons only)
            int highestIndex = labelManager.allLabels.Select(label => label.sliceIndex).Prepend(0).Max();
            _artefactRenderer.material.SetFloat(ActiveCountID, highestIndex + 1);

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

        //todo delete
        public int CountPaintedPixels(int sliceIndex)
        {
            // 1. Create a temporary CPU-side texture
            Texture2D temp = new Texture2D(MaskTexArray.width, MaskTexArray.height, TextureFormat.R8, false);
    
            // 2. Copy the GPU slice to the CPU
            RenderTexture previous = RenderTexture.active;
            Graphics.SetRenderTarget(MaskTexArray, 0, CubemapFace.Unknown, sliceIndex);
            temp.ReadPixels(new Rect(0, 0, MaskTexArray.width, MaskTexArray.height), 0, 0);
            temp.Apply();
            RenderTexture.active = previous;

            // 3. Count how many pixels aren't black (0)
            Color32[] pixels = temp.GetPixels32();
            int count = 0;
            foreach (var p in pixels) if (p.r > 0) count++;

            Destroy(temp);
            return count;
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