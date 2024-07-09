using System;
using System.Collections.Generic;
using System.Linq;
using ArtefactSystem;
using LabelSystem;
using Player.Movement;
using UnityEngine;
using UI;

namespace Pick.Mode
{
    public class Brush : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private BrushUI ui;
        [SerializeField] private MovementManager movementManager;

        [SerializeField] private Artefact artefact;

        // in screen space
        [SerializeField] private double brushRadius = 15;

        private Vector3 _cursorPos;
        private Label _activeLabel;
        private bool _quitting;
        private BrushMode _brushMode;

        private void Awake()
        {
            _activeLabel = null;
            _brushMode = BrushMode.Paint;
        }

        private void OnDisable()
        {
            if (_quitting) return;
            artefact.ShaderUpdater.HideLabels();
        }

        private void OnEnable()
        {
            artefact.ShaderUpdater.ShowLabels();
        }

        private void OnApplicationQuit()
        {
            _quitting = true;
        }

        public void ActivatePaint()
        {
            _brushMode = BrushMode.Paint;
        }

        public void ActivateEraser()
        {
            _brushMode = BrushMode.Eraser;
        }

        public void NewLabel()
        {
            if (artefact.Labels.Count > 31) return;
            artefact.NewLabel();
        }

        public void ActivateLabel(int labelIndex)
        {
            artefact.ShowLabel(labelIndex);
            var label = artefact.FindLabel(labelIndex);
            _activeLabel = label;
        }

        public void RemoveLabel(int labelIndex)
        {
            var label = artefact.FindLabel(labelIndex);
            if (_activeLabel == label)
            {
                _activeLabel = null;
            }

            artefact.RemoveLabel(label);
        }

        public void ClearActiveLabel()
        {
            _activeLabel = null;
        }

        public void UpdateLabel(int labelIndex, string newName, string newDescription, Color newColor)
        {
            artefact.UpdateLabel(labelIndex, newName, newDescription, newColor);
        }

        private bool IsOutsideBrush(Vector3 worldVertex)
        {
            Vector3 screenPointVertex = mainCamera.WorldToScreenPoint(worldVertex);

            // Check vertex is inside brush
            return Vector3.Distance(screenPointVertex, _cursorPos) > brushRadius;
        }

        private bool IsPointingAway(Vector3 worldVertex, Vector3 vertexNormal)
        {
            Vector3 directionToCamera = (mainCamera.transform.position - worldVertex).normalized;
            Vector3 normal = artefact.transform.TransformDirection(vertexNormal);

            // Calculate the angle between the vertex's normal vector and the direction to the camera
            float angle = Vector3.Angle(normal, directionToCamera);

            // Check vertex is pointing towards the camera
            return angle > 90f;
        }

        private bool IsOccluded(Vector3 worldVertex, int vIndex)
        {
            var vertices = artefact.Mesh.vertices;
            var triangles = artefact.Mesh.triangles;
            var direction = worldVertex - mainCamera.transform.position;
            var distance = direction.magnitude;
            var ray = new Ray(mainCamera.transform.position, direction.normalized);

            if (!Physics.Raycast(ray, out var hit, distance)) return false;
            
            var p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
            var p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
            var p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
            
            return !vertices[vIndex].Equals(p0) && !vertices[vIndex].Equals(p1) && !vertices[vIndex].Equals(p2);
        }

        private void Update()
        {
            if (_activeLabel == null || !Input.GetMouseButton(0) || movementManager.Movement != Movement.None) return;

            _cursorPos = Input.mousePosition;
            var vertices = artefact.Mesh.vertices;
            var brushedThisFrame = false;
            var brushedVertices = new List<int>();

            for (var i = 0; i < vertices.Length; i++)
            {
                // check it hasn't been painted or erased already
                if (_brushMode == BrushMode.Paint && _activeLabel.IsVertexPresent(i) ||
                    _brushMode == BrushMode.Eraser && !_activeLabel.IsVertexPresent(i)) continue;

                Vector3 worldVertex = artefact.transform.TransformPoint(vertices[i]);

                if (IsOutsideBrush(worldVertex)) continue;

                if (IsPointingAway(worldVertex, artefact.Mesh.normals[i])) continue;

                if (IsOccluded(worldVertex, i)) continue;

                brushedVertices.Add(i);
                brushedThisFrame = true;
            }

            if (!brushedThisFrame) return;

            switch (_brushMode)
            {
                case BrushMode.Paint:
                    _activeLabel.vertices.AddRange(brushedVertices);
                    artefact.ShaderUpdater.AddVertices(brushedVertices, _activeLabel.index);
                    break;
                case BrushMode.Eraser:
                    _activeLabel.vertices.RemoveAll(v => brushedVertices.Contains(v));
                    artefact.ShaderUpdater.RemoveVertices(brushedVertices, _activeLabel.index);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal enum BrushMode
    {
        Paint,
        Eraser
    }
}