using Mutators.Mutators;

namespace Mutators.Tests.Services.Selection.Strategies
{
    internal class NoneScalingSelectionStrategyTest : SelectionTestBase
    {
        [Test]
        public void Execute_FiltersIneligibleMutatorsAndMultiMutatorsWithMoreThanOneSubMutator()
        {
            SelectionTestMutator ineligible = Mutator("Ineligible", 1_000, false);
            SelectionTestMultiMutator excludedMulti = MultiMutator("Excluded Multi", 1_000, 2);
            SelectionTestMutator expected = Mutator("Expected", 1);

            RandomProvider.QueueFloat(0.5f);

            IMutator selected = CreateNoneStrategy().Execute([ineligible, excludedMulti, expected]);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(expected));
                Assert.That(RandomProvider.RangeCalls, Has.One.EqualTo((0f, 1f)));
            });
        }

        [Test]
        public void Execute_FiltersMutatorsWithFailingConditions()
        {
            SelectionTestMutator conditionBlocked = Mutator("Condition Blocked", 1_000, conditions: [() => false]);
            SelectionTestMutator expected = Mutator("Expected", 1);

            RandomProvider.QueueFloat(0.5f);

            IMutator selected = CreateNoneStrategy().Execute([conditionBlocked, expected]);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(expected));
                Assert.That(RandomProvider.RangeCalls, Has.One.EqualTo((0f, 1f)));
            });
        }

        [Test]
        public void Execute_IncludesOneSubMutatorMultiMutatorInSingleSelectionPool()
        {
            SelectionTestMutator regular = Mutator("Regular", 1);
            SelectionTestMultiMutator expected = MultiMutator("One Sub Multi", 9, 1);

            RandomProvider.QueueFloat(1.01f);

            IMutator selected = CreateNoneStrategy().Execute([regular, expected]);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(expected));
                Assert.That(RandomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
            });
        }

        [Test]
        public void Execute_Deez()
        {
            
        }
    }
}
