using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArtefactSystem
{
    public class Label
    {
        public int Id;
        public string Text;
        public Color Color;
        public List<Vector3> Vertices;
        public static List<int> ExistingIds = new();

        public Label()
        {
            int newId;
            do
            {
                newId = Random.Range(int.MinValue, int.MaxValue);
            } while (ExistingIds.Contains(newId));

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