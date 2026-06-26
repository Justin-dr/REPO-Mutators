using System.Collections.ObjectModel;
using Mutators.Enums;
using Mutators.Mutators;
using Mutators.Settings;
using Mutators.Tests.Settings.Specific;

namespace Mutators.Tests
{
    public class TestMutator : IMutator
    {
        private readonly bool _isEligible;
        public TestMutator(string name, string description = "description", int weight = 0, bool isEligible = true, IReadOnlyList<Func<bool>>? conditions = null!)
        {
            Settings = new TestMutatorSettings(name, description, weight);
            _isEligible = isEligible;
            Conditions = conditions ?? new ReadOnlyCollection<Func<bool>>(new List<Func<bool>>());
        }

        public TestMutator(string name, int weight, bool isEligible = true, IReadOnlyList<Func<bool>>? conditions = null!) : this(name, "description", weight, isEligible, conditions)
        {
            
        }

        public TestMutator(string name, MutatorDifficulty difficulty)
        {
            Settings = new TestMutatorSettings(name, $"{name} Description", 1);
            Conditions = new ReadOnlyCollection<Func<bool>>(new List<Func<bool>>());
            Difficulty = difficulty;
            _isEligible = true;
        }

        public string NamespacedName => Settings.NamespacedName;
        public string Name => Settings.MutatorName;
        public string Description => Settings.MutatorDescription;
        public MutatorDifficulty Difficulty { get; } = MutatorDifficulty.Normal;
        public MutatorSource Source => MutatorSource.Mod;
        public bool Active => false;
        public bool HasSpecialAction => false;
        public AbstractMutatorSettings Settings { get; }
        public IReadOnlyList<Type> Patches => [];
        public IReadOnlyList<Func<bool>> Conditions { get; }

        public void Patch()
        {
        }

        public void Unpatch()
        {
        }

        public void ConsumeMetadata(IDictionary<string, object> metadata)
        {
        }

        public bool IsEligibleForSelection()
        {
            if (Conditions.Count == 0)
            {
                return _isEligible;
            }
            return Conditions.All(condition => condition());
        }
    }
}