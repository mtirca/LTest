using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;

namespace ArtefactSystem
{
    public class Artefact : MonoBehaviour
    {
        private MeshCollider _meshCollider;
        private Mesh _mesh;

        public ObservableCollection<Label> Labels { get; private set; }

        private void ListChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            ResetMeshColor();
        }

        private void Awake()
        {
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = _meshCollider.sharedMesh;
            ResetMeshColor();
            
            Labels = new ObservableCollection<Label>();
            
            //todo load existing labels
            
            Labels.CollectionChanged += ListChanged;
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