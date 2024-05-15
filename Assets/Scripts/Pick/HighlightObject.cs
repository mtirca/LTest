using UnityEngine;

namespace Pick
{
    public class HighlightObject
    {
        public GameObject Value;
        
        public HighlightObject()
        {
            Value = new("highlight", typeof(MeshFilter), typeof(MeshRenderer));
            Value.GetComponent<MeshFilter>().mesh = new Mesh();
            Value.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Highlight") as Material;
        }
    }
}