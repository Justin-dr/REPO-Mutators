using Mutators.Mutators;
using Mutators.Rules.Registries;

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
    }
}
