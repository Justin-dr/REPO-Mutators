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

        public uint Weight { get; set; }
        
        public uint MinimumLevel { get; set; }

        public uint MaximumLevel { get; set; } = 1000;

        public IDictionary<string, IDictionary<string, object>> Mutators { get; set; } = new Dictionary<string, IDictionary<string, object>>();

        internal JsonMultiMutator()
        {
        
        }
    }
}
