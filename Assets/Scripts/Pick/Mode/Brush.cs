using UnityEngine;
using Other;

namespace Pick.Mode
{
    public class Brush : MonoBehaviour
    {
        private Picker _picker;
        private Camera _mainCamera;
        private Vector3 _center;
        private Label _activeLabel;
        
        // in screen space
        public double brushRadius = 15;
        
        private void Awake()
        {
            _mainCamera = Camera.main;
            _picker = GetComponentInParent<Picker>();
            _center = _mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
            _activeLabel = new Label();
        }

        private void Start()
        {
            _picker.OnPickModeChanged += ResetBrush;
        }

        private void ResetBrush(object sender, Picker.OnPickModeChangedEventArgs args)
        {
            if(args.OldValue != PickMode.Brush) return;
            Artefact.Instance.ResetMeshColor();
            _activeLabel = new Label();
        }
        
        private void Update()
        {
            if(_picker.Value != PickMode.Brush) return;
            
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //todo should i return or just get out of if()?
                if(_activeLabel.Vertices.Count == 0) return;
                Artefact.Instance.Labels.Add(_activeLabel);
                _activeLabel = new Label();
                return;
            }

            if (!Input.GetMouseButton(0)) return;

            //todo add as readonly field to artefact instance
            var aTransform = Artefact.Instance.transform;
            var aMeshCollider = Artefact.Instance.GetComponent<MeshCollider>();
            var aMesh = aMeshCollider.sharedMesh;
            var vertices = aMesh.vertices;
            var colors = aMesh.colors;
            var cameraPosition = _mainCamera.transform.position;
            var triangles = aMesh.triangles;
            
            //todo iterate only through unpainted vertices, not through all while asking if theyre unpainted
            for (var i = 0; i < vertices.Length; i++)
            {
                // check it hasn't been painted already
                if(colors[i] == _activeLabel.Color) continue;
                
                Vector3 worldVertex = aTransform.TransformPoint(vertices[i]);

                #region Eliminate if outside brush

                Vector3 screenPointVertex = _mainCamera.WorldToScreenPoint(worldVertex);

                // Check vertex is inside brush
                if (Vector3.Distance(screenPointVertex, _center) > brushRadius) continue;

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
                
                if(Physics.Raycast(ray, out var hit, distance)) {
                    var p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
                    var p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
                    var p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
                    if(!vertices[i].Equals(p0) && !vertices[i].Equals(p1) && !vertices[i].Equals(p2)) continue;
                }
                #endregion
                
                colors[i] = _activeLabel.Color;
                
                _activeLabel.Vertices.Add(vertices[i]);
            }
            aMesh.colors = colors;
        }
    }
}