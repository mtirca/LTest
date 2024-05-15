using System.Collections.Generic;
using UnityEngine;
using Other;

namespace Pick.Mode
{
    [RequireComponent(typeof(Camera))]
    public class Brush : MonoBehaviour
    {
        private Picker _picker;
        private Camera _mainCamera;
        private Vector3 _center;
        private List<Vector3> _tempVertices = new();
        private GameObject _highlight;
        
        // in screen space
        private readonly int _brushRadius = 15;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _picker = _mainCamera.GetComponent<Picker>();
            _center = _mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));

            GameObject obj = new("highlight", typeof(MeshFilter), typeof(MeshRenderer));
            obj.GetComponent<MeshFilter>().mesh = new Mesh();
            obj.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Highlight") as Material;
            
            // maybe create prefab in editor instead of programatically like above?
            // set as parent the artifact to have the same local coordinates
            // _highlight = Instantiate();
        }

        private void Update()
        {
            if (_picker.Value != PickMode.Brush)
            {
                _tempVertices.Clear();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                //todo specify label
                Artefact.Instance.Labels.Add(Artefact.Instance.Labels.Count, _tempVertices);
                _tempVertices.Clear();
                Artefact.Instance.RefreshTexture();
            }

            if (!Input.GetMouseButtonDown(0)) return;

            var aTransform = Artefact.Instance.transform;
            var aRenderer = Artefact.Instance.GetComponent<Renderer>();
            var aMeshCollider = Artefact.Instance.GetComponent<MeshCollider>();
            var aMesh = aMeshCollider.sharedMesh;
            var vertices = aMesh.vertices;
            var aTexture = aRenderer.material.mainTexture as Texture2D;
            // var colors = aMesh.colors;
            // var triangles = aMesh.triangles;
            Vector2[] uvs = aMesh.uv;

            if (!_highlight)
            {
                _highlight = new();
            }

            List<Vector3> hVertices = new();
            for (var i = 0; i < vertices.Length; i++)
            {
                //todo check it hasnt been painted already
                Vector3 vertex = aTransform.TransformPoint(vertices[i]);

                #region Eliminate if outside brush

                Vector3 screenPointVertex = _mainCamera.WorldToScreenPoint(vertex);

                // Check vertex is inside brush
                if (Vector3.Distance(screenPointVertex, _center) > _brushRadius) continue;

                #endregion

                #region Eliminate if pointing away

                Vector3 directionToCamera = (_mainCamera.transform.position - vertex).normalized;
                Vector3 normal = aTransform.TransformDirection(aMesh.normals[i]);

                // Calculate the angle between the vertex's normal vector and the direction to the camera
                float angle = Vector3.Angle(normal, directionToCamera);

                // Check vertex is pointing towards the camera
                if (angle > 90f) continue;

                #endregion

                //todo eliminate if invisible (not pointing away but behind another vertex)

                // bool alreadyPainted = colors[triangles[3 * i]] == Color.magenta;
                // colors[triangles[3 * i]] = Color.magenta;
                // colors[triangles[3 * i + 1]] = Color.magenta;
                // colors[triangles[3 * i + 2]] = Color.magenta;
                // aMesh.Clear();
                // aMesh.vertices = vertices;
                // aMesh.triangles = triangles;
                // aMesh.colors = colors;

                hVertices.Add(vertex);
                
                Vector2 uv = uvs[i];
                uv.x *= aTexture.width;
                uv.y *= aTexture.height;
                aTexture.SetPixel((int)uv.x, (int)uv.y, Color.magenta);
                
                _tempVertices.Add(vertices[i]);
            }

            // var prevVertices = _highlight.Value.GetComponent<MeshFilter>().mesh.vertices;
            // hVertices.AddRange(prevVertices);
            // _highlight.Value.GetComponent<MeshFilter>().mesh.vertices = hVertices.ToArray();
            aTexture.Apply();
        }
    }
}