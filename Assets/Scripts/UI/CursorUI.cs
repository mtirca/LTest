using Global;
using Pick.Mode;
using UnityEngine;

namespace UI
{
    public class CursorUI : MonoBehaviour
    {
        [SerializeField] private StateManager stateManager;
        [SerializeField] private Picker picker;
        [SerializeField] private Texture2D pixelCursorTex;
        [SerializeField] private Texture2D brushCursorTex;

        private void Awake()
        {
            stateManager.OnGlobalStateChanged += ChangeCursorVisibility;
            picker.OnPickModeChanged += ChangeCursorTexture;
            Cursor.lockState = CursorLockMode.Confined;
        }

        private void ChangeCursorTexture(object sender, Picker.OnPickModeChangedEventArgs e)
        {
            SetCursorTexture(e.NewValue);
        }

        private void SetCursorTexture(PickMode pickMode)
        {
            Texture2D tex;
            Vector2 hotspot;

            switch (pickMode)
            {
                case PickMode.Pixel:
                    tex = pixelCursorTex;
                    hotspot = new Vector2(tex.width / 2.0f, tex.height / 2.0f);
                    break;
                case PickMode.Brush:
                    tex = brushCursorTex;
                    hotspot = new Vector2(tex.width / 2.0f, tex.height / 2.0f);
                    break;
                case PickMode.None:
                default:
                    tex = null;
                    hotspot = Vector2.zero;
                    break;
            }

            Cursor.SetCursor(tex, hotspot, CursorMode.Auto);
        }

        private static void ChangeCursorVisibility(object sender, StateManager.OnGlobalStateChangedEventArgs e)
        {
            Cursor.visible = e.NewValue == State.Cursor;
        }
    }
}