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
        public int maskWidth = 2048;
        public int maskHeight = 1536;
        public float brushRadius = 0.5f;
        public bool isErasing;

        [Header("Advanced Rendering")] 
        public int maxLabels = 64;

        // GPU Buffers
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

        private void InitializeGPUArrays()
        {
            MaskTexArray = new RenderTexture(maskWidth, maskHeight, 0, RenderTextureFormat.R8)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray,
                volumeDepth = maxLabels,
                enableRandomWrite = true,
                filterMode = FilterMode.Point
            };
            MaskTexArray.Create();

            // Setup Spatial Maps for the Baker
            _positionMap = new RenderTexture(maskWidth, maskHeight, 0, RenderTextureFormat.ARGBFloat)
                {
                    enableRandomWrite = true
                };
            _positionMap.Create();

            _normalMap = new RenderTexture(maskWidth, maskHeight, 0, RenderTextureFormat.ARGBFloat)
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
            brushCompute.SetInts(MaskResolution, maskWidth, maskHeight);
            
            // Dispatch
            int threadGroupsX = Mathf.CeilToInt(maskWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(maskHeight / 8.0f);
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
            foreach (var label in labelManager.allLabels.Where(label => label.visible))
            {
                _paletteTexture.SetPixel(label.sliceIndex, 0, label.color);
            }
            _paletteTexture.Apply();
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