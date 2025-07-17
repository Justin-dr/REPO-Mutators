using Mutators.Settings;
using System;
using System.Collections.Generic;

namespace Mutators.Mutators
{
    public sealed class NopMutator : IMutator
    {
        private static readonly IReadOnlyList<Type> _patches = [];
        private static readonly IReadOnlyList<Func<bool>> _conditions = [];

        public string Name => Mutators.NopMutatorName;

        public string Description => Mutators.NopMutatorDescription;

        public bool Active { get; private set; }

        public AbstractMutatorSettings Settings { get; private set; }

        public IReadOnlyList<Type> Patches => _patches;

        public IReadOnlyList<Func<bool>> Conditions => _conditions;

        public bool HasSpecialAction => false;

        internal NopMutator(AbstractMutatorSettings settings)
        {
            Settings = settings;
        }

        public void Patch()
        {
            Active = true;
        }

        public void Unpatch()
        {
            Active = false;
        }

        public void ConsumeMetadata(IDictionary<string, object> metadata)
        {

        }
    }
}
