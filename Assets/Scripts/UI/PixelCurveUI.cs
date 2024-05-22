using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PixelCurveUI : MonoBehaviour
    {
        [SerializeField] private Image colorSquare;
        public Image ColorSquare => colorSquare;


        [SerializeField] private GameObject hitPointPrefab;
        public GameObject HitPointPrefab => hitPointPrefab;
    }
}