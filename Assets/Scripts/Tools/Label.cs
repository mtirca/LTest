using System;
using UnityEngine;

namespace Tools
{
    [Serializable]
    public class Label
    {
        public string id;
        public string name;
        public string description;
        public Color color;

        public Label(string name, Color color)
        {
            id = Guid.NewGuid().ToString();
            description = "";
            this.name = name;
            this.color = color;
        }
    }
}