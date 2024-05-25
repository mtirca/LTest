using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Model
{
    public class Label
    {
        public int Id;
        public string Text;
        public Color Color;
        public List<Vector3> Vertices;
        public static List<int> ExistingIds;

        public Label()
        {
            int newId;
            do
            {
                newId = Random.Range(Int32.MinValue, Int32.MaxValue);
            } while (!ExistingIds.Contains(newId));

            Id = newId;
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