using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UILabel
    {
        public readonly GameObject Object;
        public TMP_InputField Name => Object.transform.Find("Name").GetComponent<TMP_InputField>();
        public TMP_InputField Description => Object.transform.Find("Description").GetComponent<TMP_InputField>();
        public Image Color => Object.transform.Find("Color").GetComponent<Image>();
        public TMP_InputField ColorField => Object.transform.Find("ColorField").GetComponent<TMP_InputField>();
        public Button DeleteButton => Object.transform.Find("DeleteButton").GetComponent<Button>();
        public Toggle VisibleToggle => Object.transform.Find("VisibleToggle").GetComponent<Toggle>();
        public Button ApplyButton => Object.transform.Find("ApplyButton").GetComponent<Button>();
        public Button ActivateButton => Object.transform.Find("ActivateButton").GetComponent<Button>();
        public Button GraphButton => Object.transform.Find("GraphButton").GetComponent<Button>();
        
        public UILabel(GameObject prefab, Transform contentHolder)
        {
            Object = UnityEngine.Object.Instantiate(prefab, contentHolder);
        }
        
        public static implicit operator bool(UILabel me)
        {
            return me != null;
        }
    }
}