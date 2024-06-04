using System;
using System.Collections.Generic;
using System.Linq;

namespace LabelSystem
{
    [Serializable]
    public class Labels
    {
        public List<Label> labels;

        public Labels(IEnumerable<Label> labels)
        {
            this.labels = labels.ToList();
        }
    }
}