using Mutators.Mutators;
using Mutators.Settings;

namespace Mutators.Tests.Services.Selection.Strategies
{
    internal class RandomScalingSelectionStrategyTest : SelectionTestBase
    {
        [Test]
        public void Execute_WhenConfiguredAmountIsOne_PicksFromSingleMutatorPool()
        {
            SelectionTestMutator regular = Mutator("Regular", 1);
            SelectionTestMultiMutator expected = MultiMutator("One Sub Multi", 9, 1);

            ConfigureRandomAmountRange(1, 1);
            SetAllRandomAmountWeights(0);
            SetRandomAmountWeight(1, 10);
            RandomProvider.QueueInt(0);
            RandomProvider.QueueFloat(1.01f);

            IMutator selected = CreateRandomStrategy().Execute([regular, expected]);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(expected));
                Assert.That(RandomProvider.RandomRangeIntCalls, Has.One.EqualTo((0, 10)));
                Assert.That(RandomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
            });
        }

        [Test]
        public void Execute_WhenGenerationChanceFails_PicksMatchingRegisteredMultiMutator()
        {
            SelectionTestMutator regular = Mutator("Regular", 1_000);
            SelectionTestMultiMutator expected = MultiMutator("Expected Multi", 10, 2);

            ConfigureRandomAmountRange(2, 2);
            SetAllRandomAmountWeights(0);
            SetRandomAmountWeight(2, 10);
            SetRandomGeneratedChance(2, 20);
            RandomProvider.QueueInt(0);
            RandomProvider.QueueInt(21);
            RandomProvider.QueueFloat(5f);

            IMutator selected = CreateRandomStrategy().Execute([regular, expected]);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(expected));
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (0, 10), (1, 101) }));
                Assert.That(RandomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
            });
        }

        [Test]
        public void Execute_WhenGenerationChanceFailsAndNoMatchingRegisteredMultiMutatorExists_BuildsGeneratedMultiMutator()
        {
            SelectionTestMultiMutator wrongCount = MultiMutator("Wrong Count", 1_000, 3);
            SelectionTestMutator first = Mutator("First", 1);
            SelectionTestMutator second = Mutator("Second", 1);

            ConfigureRandomAmountRange(2, 2);
            SetAllRandomAmountWeights(0);
            SetRandomAmountWeight(2, 10);
            SetRandomGeneratedChance(2, 20);
            RandomProvider.QueueInt(0);
            RandomProvider.QueueInt(21);
            RandomProvider.QueueFloat(0.5f);
            RandomProvider.QueueFloat(0.5f);

            IMutator selected = CreateRandomStrategy().Execute([wrongCount, first, second]);

            Assert.That(selected, Is.AssignableTo<IMultiMutator>());
            IMultiMutator generated = (IMultiMutator)selected;
            Assert.Multiple(() =>
            {
                Assert.That(generated.Name, Is.EqualTo(string.Empty));
                Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, second }));
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (0, 10), (1, 101) }));
                Assert.That(RandomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 2f), (0f, 1f) }));
            });
        }

        [Test]
        public void Execute_WhenGenerationChanceSucceeds_BuildsGeneratedMultiFromRegularMutators()
        {
            SelectionTestMultiMutator existingMulti = MultiMutator("Existing Multi", 1_000, 2);
            SelectionTestMutator first = Mutator("First", 1);
            SelectionTestMutator second = Mutator("Second", 1);

            ConfigureRandomAmountRange(2, 2);
            SetAllRandomAmountWeights(0);
            SetRandomAmountWeight(2, 10);
            SetRandomGeneratedChance(2, 100);
            RandomProvider.QueueInt(0);
            RandomProvider.QueueFloat(0.5f);
            RandomProvider.QueueFloat(0.5f);

            IMutator selected = CreateRandomStrategy().Execute([existingMulti, first, second]);

            Assert.That(selected, Is.AssignableTo<IMultiMutator>());
            IMultiMutator generated = (IMultiMutator)selected;
            Assert.Multiple(() =>
            {
                Assert.That(generated.Name, Is.EqualTo(string.Empty));
                Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, second }));
                Assert.That(RandomProvider.RandomRangeIntCalls, Has.One.EqualTo((0, 10)));
                Assert.That(RandomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 2f), (0f, 1f) }));
            });
        }

        [Test]
        public void Execute_WhenMinimumAmountIsLessThanOne_ThrowsArgumentException()
        {
            ConfigureRandomAmountRange(0, 2);

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => CreateRandomStrategy().Execute([Mutator("Regular", 1)])
            )!;

            Assert.Multiple(() =>
            {
                Assert.That(exception.ParamName, Is.EqualTo("minimumAmount"));
                Assert.That(exception.Message, Does.Contain($"between 1 and {ModSettings.MaximumGeneratedActiveSubMutators}"));
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.Empty);
                Assert.That(RandomProvider.RangeCalls, Is.Empty);
            });
        }

        [Test]
        public void Execute_WhenMaximumAmountExceedsConfiguredLimit_ThrowsArgumentException()
        {
            ConfigureRandomAmountRange(1, ModSettings.MaximumGeneratedActiveSubMutators + 1);

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => CreateRandomStrategy().Execute([Mutator("Regular", 1)])
            )!;

            Assert.Multiple(() =>
            {
                Assert.That(exception.ParamName, Is.EqualTo("maximumAmount"));
                Assert.That(exception.Message, Does.Contain($"between 1 and {ModSettings.MaximumGeneratedActiveSubMutators}"));
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.Empty);
                Assert.That(RandomProvider.RangeCalls, Is.Empty);
            });
        }

        [Test]
        public void Execute_WhenMinimumAmountIsGreaterThanMaximumAmount_ThrowsArgumentException()
        {
            ConfigureRandomAmountRange(3, 2);

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => CreateRandomStrategy().Execute([Mutator("Regular", 1)])
            )!;

            Assert.Multiple(() =>
            {
                Assert.That(exception.ParamName, Is.EqualTo("minimumAmount"));
                Assert.That(exception.Message, Does.Contain("should not be greater than maximum amount"));
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.Empty);
                Assert.That(RandomProvider.RangeCalls, Is.Empty);
            });
        }
    }
}
