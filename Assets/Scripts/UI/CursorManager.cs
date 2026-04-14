using Player.Movement;
using UnityEngine;
using Tools;

namespace UI
{
    public class CursorManager : MonoBehaviour
    {
        [SerializeField] private Texture2D pixelCursorTex;
        [SerializeField] private Texture2D brushCursorTex;

        [Header("Editor Preview")]
        [Tooltip("Change this dropdown to preview cursors in the Game view!")]
        [SerializeField] private Tool previewTool = Tool.None;
        
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private Texture2D GetCursorTexture(Tool activeTool)
        {
            switch (activeTool)
            {
                case Tool.Sampler:
                    return pixelCursorTex;
                case Tool.Brush:
                    return brushCursorTex;
                case Tool.None:
                default:
                    return null;
            }
        }

        private static Vector2 GetHotspot(Tool activeTool, Texture2D cursorTex)
        {
            switch (activeTool)
            {
                case Tool.Sampler:
                case Tool.Brush:
                    return new Vector2(cursorTex.width / 2.0f, cursorTex.height / 2.0f);
                case Tool.None:
                default:
                    return Vector2.zero;
            }
        }

        /// <summary>
        /// Sets the cursor texture and hotspot (where the cursor clicks), based on the active tool.
        ///
        /// The sampler and brush have custom textures, and the hotspot is in the middle of their textures. When no tool
        /// is selected (Tool.NONE), the cursor and hotspot are the default.
        /// </summary>
        /// <param name="activeTool">The currently active tool.</param>
        public void SetCursorTexture(Tool activeTool)
        {
            Texture2D tex = GetCursorTexture(activeTool);
            Vector2 hotspot = GetHotspot(activeTool, tex);
            Cursor.SetCursor(tex, hotspot, CursorMode.Auto);
        }

        //todo ??
        public static void ChangeCursorVisibility(Movement newMovement)
        {
            Cursor.visible = newMovement == Movement.None;
        }
        
        /// <summary>
        /// This method is called automatically by Unity strictly in the Editor 
        /// whenever a value is modified in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            SetCursorTexture(previewTool);
        }
    }
}