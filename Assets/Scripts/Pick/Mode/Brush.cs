using ArtefactSystem;
using LabelSystem;
using UnityEngine;
using UI;
using UnityEngine.EventSystems;

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

        private Label ActiveLabel
        {
            get => _activeLabel;
            set
            {
                _activeLabel = value;
                if (value != null) ui.UpdateActiveLabel(value);
            }
        }

        private bool _quitting;

        private void Awake()
        {
            _activeLabel = null;
        }

        private void OnDisable()
        {
            if (_quitting) return;
            if (_activeLabel != null)
            {
                artefact.ShaderUpdater.RemoveShaderLabel(_activeLabel);
            }
        }

        private void OnApplicationQuit()
        {
            _quitting = true;
        }

        public void NewLabel()
        {
            //todo handle over 32
            var newLabel = artefact.NewLabel();
            Debug.Log($"Old value: {_activeLabel}; New value: {newLabel}");
            ActiveLabel = newLabel;
        }

        public void ActivateLabel(int labelIndex)
        {
            var label = artefact.FindLabel(labelIndex);
            Debug.Log($"Old value: {_activeLabel}; New value: {label}");
            ActiveLabel = label;
        }

        public void UpdateLabel(int labelIndex, string newName, string newDescription, Color newColor)
        {
            artefact.UpdateLabel(labelIndex, newName, newDescription, newColor);
        }

        private void Update()
        {
            _cursorPos = Input.mousePosition;

            if (ActiveLabel == null || !Input.GetMouseButton(0)) return;
                // EventSystem.current.IsPointerOverGameObject()) return;

            var aTransform = artefact.transform;
            var vertices = artefact.Mesh.vertices;
            var cameraPosition = mainCamera.transform.position;
            var triangles = artefact.Mesh.triangles;

            for (var i = 0; i < vertices.Length; i++)
            {
                // check it hasn't been painted already
                if (ActiveLabel.IsVertexPresent(i)) continue;

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

                ActiveLabel.vertices.Add(new LabelVertex(i, vertices[i]));
            }

            artefact.ShaderUpdater.UpdateShaderLabel(ActiveLabel);
        }
    }
}