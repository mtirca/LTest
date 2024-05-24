using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Model;
using UnityEngine;

namespace Other
{
    public class Artefact : MonoBehaviour
    {
        public static Artefact Instance;

        private MeshCollider _meshCollider;
        private Mesh _mesh;

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

            #endregion

            #region Init

            _meshCollider = GetComponent<MeshCollider>();
            _mesh = _meshCollider.sharedMesh;
            ResetMeshColor();

            #endregion
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