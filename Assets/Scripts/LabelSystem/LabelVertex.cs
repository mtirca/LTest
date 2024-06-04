using System;
using UnityEngine;

namespace LabelSystem
{
    [Serializable]
    public class LabelVertex
    {
        public int index;
        public Vector3 value;

        public LabelVertex(int index, Vector3 value)
        {
            this.index = index;
            this.value = value;
        }
    }
}