using System;
using System.IO;
using UnityEngine;

namespace Utils
{
    public class ScreenCaptor : MonoBehaviour
    {
        [SerializeField] private KeyCode captureKey = KeyCode.F12;
        [SerializeField] private string outputFolder = "Screenshots";

        private void Update()
        {
            if (Input.GetKeyDown(captureKey))
                CaptureScreenshot();
        }

        private void CaptureScreenshot()
        {
            var folderPath = Path.Combine(Application.dataPath, "..", outputFolder);
            Directory.CreateDirectory(folderPath);

            var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var filePath = Path.Combine(folderPath, fileName);

            ScreenCapture.CaptureScreenshot(filePath);
            Debug.Log($"Screenshot saved to {filePath}");
        }
    }
}
