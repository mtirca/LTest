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
        public string name;
        public string description;
        public Color color;
        public List<int> vertices;

        public Label(int index, string name, string description, Color color, List<int> vertices)
        {
            this.index = index;
            this.name = name;
            this.description = description;
            this.color = color;
            this.vertices = vertices;
        }

        public Label(int index) : this(index, "", "", ColorUtil.RandomColor(), new List<int>()) {}

        public bool IsVisible()
        {
            return color.a >= 0.5f;
        }

        public void Show()
        {
            color.a = 1;
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
            return vertices.Exists(vertex => vertex == vIndex);
        }
    }
}