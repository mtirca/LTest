using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SamplerUI : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text rgbText;
        
        public void SetColor(Color color)
        {
            image.color = color;
            Color32 color32 = color;
            rgbText.text = $"({color32.r}, {color32.g}, {color32.b})";
        }
    }
}