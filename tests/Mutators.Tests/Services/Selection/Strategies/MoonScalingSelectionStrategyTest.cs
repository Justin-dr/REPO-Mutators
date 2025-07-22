using Mutators.Mutators;

namespace Mutators.Tests.Services.Selection.Strategies
{
    internal class MoonScalingSelectionStrategyTest : SelectionTestBase
    {
        [Test]
        public void Execute_UsesInjectedMoonProviderForAmountAndGeneratedChance()
        {
            const int moonLevel = 4;
            SelectionTestMutator regular = Mutator("Regular", 1_000);
            SelectionTestMultiMutator expected = MultiMutator("Expected Multi", 10, 2);

            ConfigureMoonRange(moonLevel, 2, 2, 20);
            SemiFuncProvider.QueueMoonLevel(moonLevel);
            SemiFuncProvider.QueueMoonLevel(moonLevel);
            RandomProvider.QueueInt(2);
            RandomProvider.QueueInt(21);
            RandomProvider.QueueFloat(5f);

            IMutator selected = CreateMoonStrategy().Execute([regular, expected]);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(expected));
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                Assert.That(RandomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
            });
        }

        [Test]
        public void Execute_WhenGenerationChanceFailsAndNoMatchingRegisteredMultiMutatorExists_BuildsGeneratedMultiMutator()
        {
            const int moonLevel = 4;
            SelectionTestMultiMutator wrongCount = MultiMutator("Wrong Count", 1_000, 3);
            SelectionTestMutator first = Mutator("First", 1);
            SelectionTestMutator second = Mutator("Second", 1);

            ConfigureMoonRange(moonLevel, 2, 2, 20);
            SemiFuncProvider.QueueMoonLevel(moonLevel);
            SemiFuncProvider.QueueMoonLevel(moonLevel);
            RandomProvider.QueueInt(2);
            RandomProvider.QueueInt(21);
            RandomProvider.QueueFloat(0.5f);
            RandomProvider.QueueFloat(0.5f);

            IMutator selected = CreateMoonStrategy().Execute([wrongCount, first, second]);

            Assert.That(selected, Is.AssignableTo<IMultiMutator>());
            IMultiMutator generated = (IMultiMutator)selected;
            Assert.Multiple(() =>
            {
                Assert.That(generated.Name, Is.EqualTo(string.Empty));
                Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, second }));
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                Assert.That(RandomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 2f), (0f, 1f) }));
            });
        }

        [Test]
        public void Execute_WhenMoonRangePicksOne_PicksFromSingleMutatorPool()
        {
            const int moonLevel = 2;
            SelectionTestMutator regular = Mutator("Regular", 1);
            SelectionTestMultiMutator expected = MultiMutator("One Sub Multi", 9, 1);

            ConfigureMoonRange(moonLevel, 1, 1, 100);
            SemiFuncProvider.QueueMoonLevel(moonLevel);
            RandomProvider.QueueInt(1);
            RandomProvider.QueueFloat(1.01f);

            IMutator selected = CreateMoonStrategy().Execute([regular, expected]);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(expected));
                Assert.That(RandomProvider.RandomRangeIntCalls, Has.One.EqualTo((1, 2)));
                Assert.That(RandomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
            });
        }

        [Test]
        public void Execute_WhenGenerationChanceSucceeds_BuildsGeneratedMultiFromRegularMutators()
        {
            const int moonLevel = 5;
            SelectionTestMultiMutator existingMulti = MultiMutator("Existing Multi", 1_000, 2);
            SelectionTestMutator first = Mutator("First", 1);
            SelectionTestMutator second = Mutator("Second", 1);

            ConfigureMoonRange(moonLevel, 2, 2, 100);
            SemiFuncProvider.QueueMoonLevel(moonLevel);
            SemiFuncProvider.QueueMoonLevel(moonLevel);
            RandomProvider.QueueInt(2);
            RandomProvider.QueueFloat(0.5f);
            RandomProvider.QueueFloat(0.5f);

            IMutator selected = CreateMoonStrategy().Execute([existingMulti, first, second]);

            Assert.That(selected, Is.AssignableTo<IMultiMutator>());
            IMultiMutator generated = (IMultiMutator)selected;
            Assert.Multiple(() =>
            {
                Assert.That(generated.Name, Is.EqualTo(string.Empty));
                Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, second }));
                Assert.That(RandomProvider.RandomRangeIntCalls, Has.One.EqualTo((2, 3)));
                Assert.That(RandomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 2f), (0f, 1f) }));
            });
        }
    }
}
