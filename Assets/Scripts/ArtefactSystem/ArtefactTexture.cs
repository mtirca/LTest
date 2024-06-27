using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Exceptions;
using UnityEngine;

namespace ArtefactSystem
{
    public class ArtefactTexture : MonoBehaviour
    {
        [SerializeField] private Artefact artefact;
        [SerializeField] private Texture2D defaultTex;

        private static readonly Regex WhiteSpace = new(@"\s+");

        private void Awake()
        {
            artefact.Texture = CloneTexture2D(defaultTex);
        }

        private void ResetTexture()
        {
            artefact.Texture = CloneTexture2D(defaultTex);
        }

        private static Texture2D CloneTexture2D(Texture2D tex)
        {
            var texClone = new Texture2D(tex.width, tex.height);
            texClone.SetPixels(tex.GetPixels());
            texClone.Apply();

            return texClone;
        }

        private static void ValidateNullOrEmptyFields(string redFieldText, string greenFieldText, string blueFieldText)
        {
            var invalidChannels = new List<string>();

            var redNullOrEmpty = string.IsNullOrEmpty(redFieldText);
            var greenNullOrEmpty = string.IsNullOrEmpty(greenFieldText);
            var blueNullOrEmpty = string.IsNullOrEmpty(blueFieldText);

            if (redNullOrEmpty) invalidChannels.Add("R");
            if (greenNullOrEmpty) invalidChannels.Add("G");
            if (blueNullOrEmpty) invalidChannels.Add("B");

            if (invalidChannels.Count <= 0) return;

            string errMessage = "The following channels are null or empty: " + string.Join(", ", invalidChannels);
            Debug.LogError(errMessage);
            throw new InvalidChannelExpressionException(errMessage, invalidChannels);
        }

        private static bool AreDefaultValues(string redText, string greenText, string blueText)
        {
            return redText == "R" && greenText == "G" && blueText == "B";
        }

        public void SetTexture(string redFieldText, string greenFieldText, string blueFieldText)
        {
            ValidateNullOrEmptyFields(redFieldText, greenFieldText, blueFieldText);

            // Remove whitespace from expression
            var redText = WhiteSpace.Replace(redFieldText, "");
            var greenText = WhiteSpace.Replace(greenFieldText, "");
            var blueText = WhiteSpace.Replace(blueFieldText, "");

            if (AreDefaultValues(redText, greenText, blueText))
            {
                ResetTexture();
                return;
            }

            Texture2D newTexture = new Texture2D(defaultTex.width, defaultTex.height, defaultTex.format, false);

            var invalidChannels = new List<string>();
            for (int y = 0; y < defaultTex.height; y++)
            {
                for (int x = 0; x < defaultTex.width; x++)
                {
                    Color32 originalColor = defaultTex.GetPixel(x, y);

                    var rValid = EvaluateExpression(redText, originalColor, out var rResult);
                    var gValid = EvaluateExpression(greenText, originalColor, out var gResult);
                    var bValid = EvaluateExpression(blueText, originalColor, out var bResult);

                    if (!rValid) invalidChannels.Add("R");
                    if (!gValid) invalidChannels.Add("G");
                    if (!bValid) invalidChannels.Add("B");

                    if (invalidChannels.Count > 0)
                    {
                        string errMessage = "Invalid expressions for the following channels: " +
                                            string.Join(", ", invalidChannels);
                        Debug.LogError(errMessage);
                        throw new InvalidChannelExpressionException(errMessage, invalidChannels);
                    }

                    newTexture.SetPixel(x, y, new Color32(rResult, gResult, bResult, originalColor.a));
                }
            }

            newTexture.Apply();
            artefact.Texture = newTexture;
        }
        
        private static bool EvaluateExpression(string equation, Color32 originalColor, out byte result)
        {
            var expression = equation.Replace("R", originalColor.r.ToString())
                .Replace("G", originalColor.g.ToString())
                .Replace("B", originalColor.b.ToString());

            var valid = ExpressionEvaluator.Evaluate<int>(expression, out var res);
            result = (byte)Mathf.Clamp(res, 0, 255);
            return valid;
        }
    }
}