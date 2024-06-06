using System;
using System.Collections.Generic;
using ArtefactSystem;
using LabelSystem;
using UnityEngine;
using UI;
using Utils;

namespace Pick.Mode
{
    public class Brush : MonoBehaviour
    {
        [SerializeField] private Picker picker;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private BrushUI ui;

        [SerializeField] private Artefact artefact;

        // in screen space
        [SerializeField] private double brushRadius = 15;

        private Vector3 _cursorPos;
        private Label _activeLabel;

        private void Awake()
        {
            _activeLabel = NewActiveLabel();
        }

        private void Start()
        {
            picker.OnPickModeChanged += ResetBrush;
        }

        private void ResetBrush(object sender, Picker.OnPickModeChangedEventArgs args)
        {
            // Changed PickMode from brush
            if (args.OldValue == PickMode.Brush && args.NewValue != PickMode.Brush)
            {
                artefact.ShaderUpdater.RemoveShaderLabels(new List<Label> { _activeLabel });
                _activeLabel = null;
            }

            // Changed PickMode to brush
            if (args.OldValue != PickMode.Brush && args.NewValue == PickMode.Brush)
            {
                _activeLabel = NewActiveLabel();
            }
        }

        private Label NewActiveLabel()
        {
            int index = artefact.GetFirstAvailableLabelIndex();
            if (index == -1)
                throw new IndexOutOfRangeException($"Reached maximum label number per artefact ({Label.Max})");
            var color = ColorUtil.RandomColor();
            return new Label(index, color.ToString(), color.ToString(), color, new List<LabelVertex>());
        }

        private void Update()
        {
            _cursorPos = Input.mousePosition;

            if (Input.GetKeyDown(KeyCode.Return) && _activeLabel.vertices.Count != 0)
            {
                artefact.AddLabel(_activeLabel);
                artefact.HideLabel(_activeLabel.index);
                _activeLabel = NewActiveLabel();
                return;
            }

            if (!Input.GetMouseButton(0)) return;

            var aTransform = artefact.transform;
            var vertices = artefact.Mesh.vertices;
            var cameraPosition = mainCamera.transform.position;
            var triangles = artefact.Mesh.triangles;

            for (var i = 0; i < vertices.Length; i++)
            {
                // check it hasn't been painted already
                if (_activeLabel.IsVertexPresent(i)) continue;

                Vector3 worldVertex = aTransform.TransformPoint(vertices[i]);

                #region Eliminate if outside brush

                Vector3 screenPointVertex = mainCamera.WorldToScreenPoint(worldVertex);

                // Check vertex is inside brush
                if (Vector3.Distance(screenPointVertex, _cursorPos) > brushRadius) continue;

                #endregion

                #region Eliminate if pointing away

                Vector3 directionToCamera = (cameraPosition - worldVertex).normalized;
                Vector3 normal = aTransform.TransformDirection(artefact.Mesh.normals[i]);

                // Calculate the angle between the vertex's normal vector and the direction to the camera
                float angle = Vector3.Angle(normal, directionToCamera);

                // Check vertex is pointing towards the camera
                if (angle > 90f) continue;

                #endregion

                #region Eliminate if occluded

                var direction = worldVertex - cameraPosition;
                var distance = direction.magnitude;
                var ray = new Ray(cameraPosition, direction.normalized);

                if (Physics.Raycast(ray, out var hit, distance))
                {
                    var p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
                    var p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
                    var p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
                    if (!vertices[i].Equals(p0) && !vertices[i].Equals(p1) && !vertices[i].Equals(p2)) continue;
                }

                #endregion

                _activeLabel.vertices.Add(new LabelVertex(i, vertices[i]));
            }

            artefact.ShaderUpdater.UpdateShaderLabels(new List<Label> { _activeLabel });
        }
    }
}