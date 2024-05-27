using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PixelUI : MonoBehaviour
    {
        [SerializeField] private Image colorSquare;
        public Image ColorSquare => colorSquare;
    }
}