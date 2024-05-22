using System;
using System.Collections.Specialized;
using System.Linq;
using Other;
using Pick.Mode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BrushUI : MonoBehaviour
    {
        [SerializeField] private GameObject labelPrefab;
        [SerializeField] private Transform contentHolder;
        
        private void Start()
        {
            Artefact.Instance.Labels.CollectionChanged += UpdateUILabels;
        }

        //todo update if remove labels
        private void UpdateUILabels(object sender, NotifyCollectionChangedEventArgs args)
        {
            var newLabels = args.NewItems.Cast<Label>();
            foreach (var newLabel in newLabels)
            {
                var uiLabel = Instantiate(labelPrefab, contentHolder);
                var text = uiLabel.GetComponentInChildren<TMP_Text>();
                var image = uiLabel.GetComponentInChildren<Image>();
                text.text = newLabel.Text;
                image.color = newLabel.Color;
            }
        }
    }
}