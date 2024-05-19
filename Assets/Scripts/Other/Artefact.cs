using System.Collections.Generic;
using Pick.Mode;
using UnityEngine;

namespace Other
{
    public class Artefact : MonoBehaviour
    {
        public static Artefact Instance;

        private Renderer _renderer;
        private MeshCollider _meshCollider;
        private Mesh _mesh;
        private Texture2D _originalTexture;

        public List<Label> Labels = new();

        private void Awake()
        {
            #region Singleton

            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }

            // DontDestroyOnLoad(this);

            #endregion

            #region Init

            _meshCollider = GetComponent<MeshCollider>();
            _mesh = _meshCollider.sharedMesh;
            ResetMeshColor();
            _renderer = transform.GetComponent<Renderer>();
            _originalTexture = _renderer.material.mainTexture as Texture2D;

            #endregion

            #region TextureInit

            RefreshTexture();

            #endregion
        }

        public void RefreshTexture()
        {
            _renderer.material.mainTexture = Instantiate(_originalTexture);
        }

        public void ResetMeshColor()
        {
            Color[] colors = new Color[_mesh.vertices.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }
            _mesh.colors = colors;
        }
    }
}