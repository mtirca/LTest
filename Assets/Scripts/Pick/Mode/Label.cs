using System.Collections.Generic;
using UnityEngine;

namespace Pick.Mode
{
    public class Label
    {
        public string Text;
        public Color Highlight;
        public List<Vector3> Vertices;

        public Label()
        {
            Highlight = RandomColor();
            Text = Highlight.ToString();
            Vertices = new List<Vector3>();
        }

        private static Color RandomColor()
        {
            return Random.ColorHSV(0, 1, 0, 1, 0, 1, 0.5f, 0.5f);
        }
    }
}