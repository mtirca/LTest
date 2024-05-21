using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        //todo event on collection works?
        public ObservableCollection<Label> Labels { get; private set; }

        private void ListChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            ResetMeshColor();
        }

        private void Awake()
        {
            Labels = new ObservableCollection<Label>();
            Labels.CollectionChanged += ListChanged;

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

        //todo still need?
        public void RefreshTexture()
        {
            _renderer.material.mainTexture = Instantiate(_originalTexture);
        }

        public void ResetMeshColor()
        {
            var colors = new Color[_mesh.vertices.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }

            _mesh.colors = colors;
        }
    }
}