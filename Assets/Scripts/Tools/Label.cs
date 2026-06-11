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
        public int rBandIndex;
        public int gBandIndex;
        public int bBandIndex;

        public Label(string name, Color color, int sliceIndex, int rBandIndex, int gBandIndex, int bBandIndex)
        {
            id = Guid.NewGuid().ToString();
            visible = true;
            description = "";
            this.name = name;
            this.color = color;
            this.sliceIndex = sliceIndex;
            this.rBandIndex = rBandIndex;
            this.gBandIndex = gBandIndex;
            this.bBandIndex = bBandIndex;
        }
    }
}