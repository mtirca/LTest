using System;
using UnityEngine;
using ArtefactSystem;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class BrushUI : MonoBehaviour
    {
        [SerializeField] private GameObject labelPrefab;
        [SerializeField] private Transform contentHolder;
        [SerializeField] private Artefact artefact;

        private void Start()
        {
            artefact.LabelsChanged += UpdateUILabels;
        }

        private void UpdateUILabels(object sender, Artefact.LabelsChangedEventArgs args)
        {
            switch (args.Type)
            {
                case LabelEvent.Add:
                {
                    var newLabel = args.Item;
                    var uiLabel = Instantiate(labelPrefab, contentHolder);
                    var text = uiLabel.GetComponentInChildren<TMP_Text>();
                    var image = uiLabel.GetComponentInChildren<Image>();
                    text.text = newLabel.text;
                    image.color = newLabel.color;
                    break;
                }
                case LabelEvent.Remove:
                    //todo update if remove label
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}