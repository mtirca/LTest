using UnityEngine;

namespace ArtefactSystem
{
    public class Artefact : MonoBehaviour
    {
        private Renderer _renderer;

        public Renderer Renderer
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
    }
}
