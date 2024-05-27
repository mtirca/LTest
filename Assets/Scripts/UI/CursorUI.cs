using Global;
using Pick.Mode;
using UnityEngine;

namespace UI
{
    public class CursorUI : MonoBehaviour
    {
        [SerializeField] private StateManager stateManager;
        [SerializeField] private Picker picker;

        private void Awake()
        {
            stateManager.OnGlobalStateChanged += ChangeCursorVisibility;
            picker.OnPickModeChanged += ChangeCursorTexture;
            Cursor.lockState = CursorLockMode.Confined;
        }

        private static void ChangeCursorTexture(object sender, Picker.OnPickModeChangedEventArgs e)
        {
            if (e.NewValue == PickMode.None)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }

            var tex = Utils.Resource.LoadCrosshairTexture(e.NewValue);
            var hotspot = new Vector2(tex.width / 2.0f, tex.height / 2.0f);
            Cursor.SetCursor(tex, hotspot, CursorMode.Auto);
        }

        private static void ChangeCursorVisibility(object sender, StateManager.OnGlobalStateChangedEventArgs e)
        {
            Cursor.visible = e.NewValue == State.Cursor;
        }
    }
}