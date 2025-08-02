using Mutators.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutators.Mutators.Multi
{
    internal class JsonMultiMutator
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public IDictionary<string, IDictionary<string, object>> Mutators { get; set; } = new Dictionary<string, IDictionary<string, object>>();

        internal JsonMultiMutator()
        {
        
        }
    }
}
