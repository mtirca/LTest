using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace ArtefactSystem
{
    [Serializable]
    public class Label
    {
        public Color color;
        public string text;
        public List<Vector3> vertices;
        
        public Label(Color color, string text, List<Vector3> vertices)
        {
            this.color = color;
            this.text = text;
            this.vertices = vertices;
        }

        public bool IsVisible()
        {
            return color.a != 0;
        }

        public void MakeVisible()
        {
            color.a = 0.5f;
        }

        public void MakeInvisible()
        {
            color.a = 0.0f;
        }
        
        public static Label DefaultLabel()
        {
            var color = ColorUtil.RandomColor();
            var text = color.ToString();
            var vertices = new List<Vector3>();
            return new Label(color, text, vertices);
        }
    }
}