using System;
using System.Collections.Generic;
using System.Text;

namespace Mutators.Mutators
{
    public class NopMutator : IMutator
    {
        private static readonly IReadOnlyList<Type> _patches = [];

        public string Name => Mutators.NopMutator;

        public bool Active { get; private set; }

        public uint Weight { get; private set; }

        public IReadOnlyList<Type> Patches => _patches;

        internal NopMutator(uint weight)
        {
            Weight = weight;
        }

        public void Patch()
        {
            Active = true;
        }

        public void Unpatch()
        {
            Active = false;
        }
    }
}
