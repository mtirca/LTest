using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace ArtefactSystem
{
    public class Artefact : MonoBehaviour
    {
        private MeshCollider _meshCollider;
        private Mesh _mesh;

        public List<Label> Labels { get; set; }

        public event EventHandler<LabelsChangedEventArgs> LabelsChanged;

        public class LabelsChangedEventArgs
        {
            public LabelEvent Type;
            public Label Item;
        }

        private void Awake()
        {
            Labels = new List<Label>(LabelLoader.Load());
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = _meshCollider.sharedMesh;
        }

        //todo update
        public void ResetMeshColor()
        {
            var colors = new Color[_mesh.vertices.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }

            _mesh.colors = colors;
        }

        public void AddLabel(Label newLabel)
        {
            LabelsChanged?.Invoke(this, new LabelsChangedEventArgs { Type = LabelEvent.Add, Item = newLabel });
            Labels.Add(newLabel);
            LabelLoader.Save(Labels);
            ResetMeshColor();
        }
    }
}