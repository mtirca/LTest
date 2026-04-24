using System;
using UnityEngine;

namespace Tools
{
    [Serializable]
    public class Label
    {
        public string id;
        public int sliceIndex;
        public string name;
        public string description;
        public Color color;
        public bool visible;
        [NonSerialized] public Color32[] Pixels;

        public Label(string name, Color color, int sliceIndex, int maskWidth, int maskHeight)
        {
            id = Guid.NewGuid().ToString();
            visible = true;
            description = "";
            Pixels = new Color32[maskWidth * maskHeight];
            for (int i = 0; i < Pixels.Length; i++)
            {
                Pixels[i] = new Color32(0, 0, 0, 0);
            }
            
            this.name = name;
            this.color = color;
            this.sliceIndex = sliceIndex;
        }
    }
}