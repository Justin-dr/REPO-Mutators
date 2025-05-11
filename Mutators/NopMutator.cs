using System;
using System.Collections.Generic;

namespace Mutators.Mutators
{
    public class NopMutator : IMutator
    {
        private static readonly IReadOnlyList<Type> _patches = [];
        private static readonly IReadOnlyList<Func<bool>> _conditions = [];

        public string Name => Mutators.NopMutator;

        public bool Active { get; private set; }

        public uint Weight { get; private set; }

        public IReadOnlyList<Type> Patches => _patches;

        public IReadOnlyList<Func<bool>> Conditions => _conditions;

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
