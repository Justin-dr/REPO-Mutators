using Mutators.Enums;
using Mutators.Mutators;
using Mutators.Rules.Registries;
using Mutators.Settings;

namespace Mutators.Tests.Rules.Registries
{
    class GeneratedMultiMutatorSelectionRulesRegistryTest
    {
        [Test]
        public void RunRules_WithNoRules_ReturnsTrue()
        {
            GeneratedMultiMutatorSelectionRulesRegistry registry = new();
            TestMutator current = new("Current");

            Assert.That(registry.RunRules(new List<IMutator>(), current), Is.True);
        }

        [Test]
        public void RunRules_WithOnlyNamespacedNameRules_AndAllRulesPass_ReturnsTrue()
        {
            GeneratedMultiMutatorSelectionRulesRegistry registry = new();
            TestMutator picked = new("Picked");
            TestMutator current = new("Current");
            registry.Register(
                "PickedAllowsCurrent",
                (pickedNames, currentName) => pickedNames.Contains(picked.NamespacedName) && currentName == current.NamespacedName
            );

            Assert.That(registry.RunRules(new List<IMutator> { picked }, current), Is.True);
        }

        [Test]
        public void RunRules_WithOnlyExtendedRules_AndAllRulesPass_ReturnsTrue()
        {
            GeneratedMultiMutatorSelectionRulesRegistry registry = new();
            TestMutator picked = new("Picked");
            TestMutator current = new("Current");
            registry.Register(
                "PickedAllowsCurrent",
                (pickedMutators, candidate) => pickedMutators.Contains(picked) && candidate == current
            );

            Assert.That(registry.RunRules(new List<IMutator> { picked }, current), Is.True);
        }

        [Test]
        public void RunRules_WithBothRuleTypes_AndAllRulesPass_ReturnsTrue()
        {
            GeneratedMultiMutatorSelectionRulesRegistry registry = new();
            TestMutator picked = new("Picked");
            TestMutator current = new("Current");
            registry.Register(
                "NamespacedNameRule",
                (pickedNames, currentName) => pickedNames.Contains(picked.NamespacedName) && currentName == current.NamespacedName
            );
            registry.Register(
                "ExtendedRule",
                (pickedMutators, candidate) => pickedMutators.Contains(picked) && candidate == current
            );

            Assert.That(registry.RunRules(new List<IMutator> { picked }, current), Is.True);
        }

        [Test]
        public void RunRules_WithNamespacedNameRules_AndAnyRuleFails_ReturnsFalse()
        {
            GeneratedMultiMutatorSelectionRulesRegistry registry = new();
            TestMutator current = new("Current");
            registry.Register("PassingRule", (IReadOnlyCollection<string> _, string _) => true);
            registry.Register("FailingRule", (IReadOnlyCollection<string> _, string _) => false);

            Assert.That(registry.RunRules(new List<IMutator>(), current), Is.False);
        }

        [Test]
        public void RunRules_WithExtendedRules_AndAnyRuleFails_ReturnsFalse()
        {
            GeneratedMultiMutatorSelectionRulesRegistry registry = new();
            TestMutator current = new("Current");
            registry.Register("PassingRule", (IReadOnlyCollection<IMutator> _, IMutator _) => true);
            registry.Register("FailingRule", (IReadOnlyCollection<IMutator> _, IMutator _) => false);

            Assert.That(registry.RunRules(new List<IMutator>(), current), Is.False);
        }

        private sealed class TestMutator : IMutator
        {
            private static readonly IReadOnlyList<Type> EmptyPatches = [];
            private static readonly IReadOnlyList<Func<bool>> EmptyConditions = [];

            public TestMutator(string name, MutatorDifficulty difficulty = MutatorDifficulty.Easy)
            {
                Name = name;
                NamespacedName = $"test:{name.ToLowerInvariant()}";
                Description = $"{name} Description";
                
                Difficulty = difficulty;
            }

            public string NamespacedName { get; }
            public string Name { get; }
            public string Description { get; }
            public MutatorDifficulty Difficulty { get; }
            public MutatorSource Source => MutatorSource.Mod;
            public bool Active => false;
            public bool HasSpecialAction => false;
            public AbstractMutatorSettings Settings => null!;
            public IReadOnlyList<Type> Patches => EmptyPatches;
            public IReadOnlyList<Func<bool>> Conditions => EmptyConditions;

            public void Patch()
            {
            }

            public void Unpatch()
            {
            }

            public void ConsumeMetadata(IDictionary<string, object> metadata)
            {
            }
        }
    }
}
