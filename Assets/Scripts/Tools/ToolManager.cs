using UI;
using UnityEngine;

namespace Tools
{
    public class ToolManager : MonoBehaviour
    {
        [Header("Tool Scripts")]
        [SerializeField] private Sampler sampler;
        [SerializeField] private Brush brush;
        [SerializeField] private CursorManager cursorManager;

        private Tool _currentTool = Tool.None;

        private void Start()
        {
            SetCursorTool();
        }

        public void SetCursorTool()
        {
            SetTool(Tool.None);
        }

        public void SetSamplerTool()
        {
            SetTool(Tool.Sampler);
        }

        public void SetBrushTool()
        {
            SetTool(Tool.Brush);
        }
        
        /// <summary>
        /// Called from UI buttons' OnClick events.
        /// </summary>
        private void SetTool(Tool toolToActivate)
        {
            _currentTool = toolToActivate;

            DisableAllTools();

            switch (_currentTool)
            {
                case Tool.Sampler:
                    sampler.enabled = true;
                    break;
                case Tool.Brush:
                    brush.enabled = true;
                    break;
                case Tool.None:
                default:
                    break;
            }

            cursorManager.SetCursorTexture(_currentTool);
        }
        
        private void DisableAllTools()
        {
            if (sampler != null)
            {
                sampler.enabled = false;
            }

            if (brush != null)
            {
                brush.enabled = false;
            }
        }
    }
}