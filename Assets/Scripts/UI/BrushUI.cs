using System.Collections.Specialized;
using System.Linq;
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
            artefact.Labels.CollectionChanged += UpdateUILabels;
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