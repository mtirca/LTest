using System;
using ArtefactSystem;
using Exceptions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FreeCursorUI : MonoBehaviour
    {
        [SerializeField] private Button resetButton;
        [SerializeField] private Button applyButton;
        [SerializeField] private Image redPanel;
        [SerializeField] private Image greenPanel;
        [SerializeField] private Image bluePanel;
        [SerializeField] private TMP_InputField redField;
        [SerializeField] private TMP_InputField greenField;
        [SerializeField] private TMP_InputField blueField;
        [SerializeField] private ArtefactTexture artefactTexture;

        private static Color32 ValidChannelColor => new(255, 255, 255, 50);
        private static Color32 InvalidChannelColor => new(255, 0, 0, 50);
        
        private void Awake()
        {
            applyButton.onClick.AddListener(OnApplyButtonClick);
            resetButton.onClick.AddListener(OnResetButtonClick);
        }

        private void OnResetButtonClick()
        {
            redField.text = "R";
            greenField.text = "G";
            blueField.text = "B";
        }

        private void OnApplyButtonClick()
        {
            ResetValidationColors();
            try
            {
                artefactTexture.SetTexture(redField.text, greenField.text, blueField.text);
            }
            catch (InvalidChannelExpressionException e)
            {
                foreach (var channel in e.Channels)
                {
                    GetChannelPanel(channel).color = InvalidChannelColor;
                }
            }
        }

        private Image GetChannelPanel(string channel)
        {
            return channel switch
            {
                "R" => redPanel,
                "G" => greenPanel,
                "B" => bluePanel,
                _ => throw new Exception($"Invalid channel name \"{channel}\"")
            };
        }

        private void ResetValidationColors()
        {
            redPanel.color = ValidChannelColor;
            greenPanel.color = ValidChannelColor;
            bluePanel.color = ValidChannelColor;
        }
    }
}