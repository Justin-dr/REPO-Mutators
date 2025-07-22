using System.Collections.Generic;

namespace Mutators.Mutators.MultiMutators
{
    internal class JsonMultiMutator
    {
        public string Name { get; set; }
        public string? Description { get; set; }

        public int Weight { get; set; }
        
        public int MinimumLevel { get; set; }

        public int MaximumLevel { get; set; } = 1000;

        public IDictionary<string, IDictionary<string, object>> Mutators { get; set; } = new Dictionary<string, IDictionary<string, object>>();

        internal JsonMultiMutator()
        {
        
        }
    }
}
