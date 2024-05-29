using ArtefactSystem;
using UnityEngine;
using UI;

namespace Pick.Mode
{
    public class Brush : MonoBehaviour
    {
        [SerializeField] private Picker picker;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private BrushUI ui;
        [SerializeField] private Artefact artefact;

        private Vector3 _cursorPos;
        private Label _activeLabel;

        // in screen space
        public double brushRadius = 15;

        private void Awake()
        {
            _activeLabel = Label.DefaultLabel();
        }

        private void Start()
        {
            picker.OnPickModeChanged += ResetBrush;
        }

        private void ResetBrush(object sender, Picker.OnPickModeChangedEventArgs args)
        {
            if (args.OldValue != PickMode.Brush) return;
            artefact.ResetMeshColor();
            _activeLabel = Label.DefaultLabel();
        }

        private void Update()
        {
            _cursorPos = Input.mousePosition;
            
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //todo should i return or just get out of if()?
                if (_activeLabel.vertices.Count == 0) return;
                artefact.AddLabel(_activeLabel);
                _activeLabel = Label.DefaultLabel();
                return;
            }

            if (!Input.GetMouseButton(0)) return;

            //todo add as readonly field to artefact instance
            var aTransform = artefact.transform;
            var aMeshCollider = artefact.GetComponent<MeshCollider>();
            var aMesh = aMeshCollider.sharedMesh;
            var vertices = aMesh.vertices;
            var colors = aMesh.colors;
            var cameraPosition = mainCamera.transform.position;
            var triangles = aMesh.triangles;

            //todo iterate only through unpainted vertices, not through all while asking if they're unpainted
            for (var i = 0; i < vertices.Length; i++)
            {
                // check it hasn't been painted already
                if (colors[i] == _activeLabel.color) continue;

                Vector3 worldVertex = aTransform.TransformPoint(vertices[i]);

                #region Eliminate if outside brush

                Vector3 screenPointVertex = mainCamera.WorldToScreenPoint(worldVertex);

                // Check vertex is inside brush
                if (Vector3.Distance(screenPointVertex, _cursorPos) > brushRadius) continue;

                #endregion

                #region Eliminate if pointing away

                Vector3 directionToCamera = (cameraPosition - worldVertex).normalized;
                Vector3 normal = aTransform.TransformDirection(aMesh.normals[i]);

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

                colors[i] = _activeLabel.color;

                _activeLabel.vertices.Add(vertices[i]);
            }

            aMesh.colors = colors;
        }
    }
}