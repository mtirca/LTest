using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace LabelSystem
{
    [Serializable]
    public class Label
    {
        public const int Max = 32;

        public int index;
        public Color color;
        public string text;
        public List<LabelVertex> vertices;
        
        public Label(int index, Color color, string text, List<LabelVertex> vertices)
        {
            this.index = index;
            this.color = color;
            this.text = text;
            this.vertices = vertices;
        }

        public bool IsVisible()
        {
            return color.a >= 0.5f;
        }

        public void Show()
        {
            color.a = 1;
            //todo also change tex
        }

        public void Hide()
        {
            color.a = 0;
        }

        /**
         * Check if vertex with index vIndex is present in label
         */
        public bool IsVertexPresent(int vIndex)
        {
            return vertices.Exists(vertex => vertex.index == vIndex);
        }
    }
}