using UnityEngine;

namespace ArtefactSystem
{
    public class Artefact : MonoBehaviour
    {
        private Renderer _renderer;

        private Renderer Renderer
        {
            get
            {
                if (_renderer == null)
                {
                    _renderer = GetComponent<Renderer>();
                }

                return _renderer;
            }
        }

        private MeshFilter MeshFilter { get; set; }
        public Mesh Mesh => MeshFilter.sharedMesh;
        public MeshCollider MeshCollider { get; private set; }

        private static readonly int RedBandID = Shader.PropertyToID("_RedBand");
        private static readonly int GreenBandID = Shader.PropertyToID("_GreenBand");
        private static readonly int BlueBandID = Shader.PropertyToID("_BlueBand");

        [Tooltip("The specific wavelengths (in nm) for each slice of the texture")]
        public int[] Wavelengths;

        private static readonly int MSTexID = Shader.PropertyToID("_MSTex");

        public Texture2DArray MSTex => Renderer.material.GetTexture(MSTexID) as Texture2DArray;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
        }
        
        /// <summary>
        /// Updates the material to display specific slices of the MSTex on the RGB channels.
        /// </summary>
        public void SetRGBBands(int rIndex, int gIndex, int bIndex)
        {
            Renderer.material.SetFloat(RedBandID, rIndex);
            Renderer.material.SetFloat(GreenBandID, gIndex);
            Renderer.material.SetFloat(BlueBandID, bIndex);
        }
    }
}