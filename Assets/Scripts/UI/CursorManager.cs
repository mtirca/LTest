using Pick.Mode;
using Player.Movement;
using UnityEngine;

namespace UI
{
    public class CursorManager : MonoBehaviour
    {
        [SerializeField] private MovementManager movementManager;
        [SerializeField] private Picker picker;
        [SerializeField] private Texture2D pixelCursorTex;
        [SerializeField] private Texture2D brushCursorTex;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Confined;
        }

        public void ChangeCursorTexture(PickMode _, PickMode newValue)
        {
            SetCursorTexture(newValue);
        }

        private void SetCursorTexture(PickMode pickMode)
        {
            Texture2D tex;
            Vector2 hotspot;

            switch (pickMode)
            {
                case PickMode.Sampler:
                    tex = pixelCursorTex;
                    hotspot = new Vector2(tex.width / 2.0f, tex.height / 2.0f);
                    break;
                case PickMode.Brush:
                    tex = brushCursorTex;
                    hotspot = new Vector2(tex.width / 2.0f, tex.height / 2.0f);
                    break;
                case PickMode.Cursor:
                default:
                    tex = null;
                    hotspot = Vector2.zero;
                    break;
            }

            Cursor.SetCursor(tex, hotspot, CursorMode.Auto);
        }
        
        public static void ChangeCursorVisibility(Movement newMovement)
        {
            Cursor.visible = newMovement == Movement.None;
        }
    }
}