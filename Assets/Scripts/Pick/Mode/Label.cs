using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pick.Mode
{
    public class Label
    {
        public string Text;
        public Color Color;
        public List<Vector3> Vertices;

        public Label()
        {
            Color = RandomColor();
            Text = Color.ToString();
            Vertices = new List<Vector3>();
        }

        private static Color RandomColor()
        {
            return Random.ColorHSV(0, 1, 0, 1, 0, 1, 0.5f, 0.5f);
        }
    }
}