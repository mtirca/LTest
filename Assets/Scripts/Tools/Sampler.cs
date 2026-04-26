using ArtefactSystem;
using Player.Movement;
using UI;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace Tools
{
    public class Sampler : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject hitPointPrefab;
        [SerializeField] private MovementManager movementManager;
        [SerializeField] private SamplerUI samplerUI;

        private GameObject _hitPoint;

        private void OnDisable()
        {
            if (_hitPoint) _hitPoint.SetActive(false);
        }

        private void OnEnable()
        {
            if (_hitPoint) _hitPoint.SetActive(true);
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)
                || movementManager.Movement != Movement.None
                || EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                return;
            }

            Artefact artefact = hit.collider.GetComponent<Artefact>();
            if (!artefact || !artefact.MeshCollider || !artefact.MSTex)
            {
                return;
            }

            ushort[] signature = ExtractSpectroscopicSignature(hit.textureCoord, artefact.MSTex);
            samplerUI.SetRawData(signature, artefact.Wavelengths);

            // Add new hit point
            if (_hitPoint)
            {
                Destroy(_hitPoint);
            }

            _hitPoint = Instantiate(hitPointPrefab, hit.point, Quaternion.identity);
        }

        /// <summary>
        /// Reads the exact 16-bit intensity values directly from the CPU memory block.
        /// Works on any 3D mesh with a valid UV map and any resolution Data Cube.
        /// </summary>
        private static ushort[] ExtractSpectroscopicSignature(Vector2 uv, Texture2DArray msTex)
        {
            int bands = msTex.depth;
            ushort[] spectralSignature = new ushort[bands];
            int flatIndex = TextureUtils.ComputePixelIndex(msTex, uv);

            for (int slice = 0; slice < bands; slice++)
            {
                NativeArray<ushort> rawSliceData = msTex.GetPixelData<ushort>(0, slice);
                spectralSignature[slice] = rawSliceData[flatIndex];
            }

            return spectralSignature;
        }
    }
}