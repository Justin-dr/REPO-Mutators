using Mutators.Managers;
using Mutators.Settings;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mutators.Mutators
{
    public class MultiMutator : IMultiMutator
    {
        private readonly IList<Type> _patches;
        private readonly IList<Func<bool>> _conditions;
        private readonly IList<IMutator> _subMutators;

        public string Name => Settings.MutatorName;

        public string Description => Settings.MutatorDescription;

        public bool Active { get; private set; }

        public bool HasSpecialAction { get; private set; }

        public AbstractMutatorSettings Settings { get; private set; }

        public IReadOnlyList<Type> Patches => new ReadOnlyCollection<Type>(_patches);

        public IReadOnlyList<Func<bool>> Conditions => new ReadOnlyCollection<Func<bool>>(_conditions);

        public IReadOnlyList<IMutator> SubMutators => new ReadOnlyCollection<IMutator>(_subMutators);

        public MultiMutator(AbstractMutatorSettings settings, IList<IMutator> mutators, IList<Func<bool>> conditions = null!)
        {
            Settings = settings;
            _subMutators = mutators;

            HasSpecialAction = _subMutators.Any(mutator => mutator.HasSpecialAction);

            _conditions = mutators.SelectMany(mut => mut.Conditions).ToList();
            if (conditions != null)
            {
                _conditions.AddRange(conditions);
            }

            _patches = mutators.SelectMany(mut => mut.Patches).ToList();
        }

        public void Patch()
        {
            if (Active) return;

            Active = true;

            foreach (IMutator subMutator in _subMutators)
            {
                subMutator.Patch();
            }
        }

        public void Unpatch()
        {
            if (!Active) return;

            if (SemiFunc.IsMultiplayer() && !SemiFunc.IsMasterClient())
            {
                MutatorManager.Instance.UnregisterMutator(this);
            }

            foreach (IMutator subMutator in _subMutators)
            {
                subMutator.Unpatch();
            }

            Active = false;
        }

        public void ConsumeMetadata(IDictionary<string, object> metadata)
        {
            foreach (IMutator subMutator in _subMutators)
            {
                subMutator.ConsumeMetadata(metadata);
            }
        }
    }
}