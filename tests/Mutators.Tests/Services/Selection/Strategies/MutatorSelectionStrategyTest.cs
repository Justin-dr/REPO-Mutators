using Mutators.Mutators;
using Mutators.Rules;
using Mutators.Rules.Registries;
using Mutators.Services.Selection;
using Mutators.Services.Selection.Strategies;
using Mutators.Tests.Providers.Random;

namespace Mutators.Tests.Services.Selection.Strategies
{
    internal class MutatorSelectionStrategyTest : SelectionTestBase
    {
        [Test]
        public void PickGeneratedMultiMutator_AppliesGenerationRulesAndAvoidsDuplicates()
        {
            TestMutator first = Mutator("First", 1);
            TestMutator excluded = Mutator("Excluded", 1_000);
            TestMutator alternative = Mutator("Alternative", 1);
            ExposedStrategy strategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator);

            MultiMutatorSelectionRulesRegistry.Register(
                "FirstWithExcluded",
                MultiMutatorRules.MutualExclusionRule(
                    first.NamespacedName,
                    excluded.NamespacedName
                )
            );
            RandomProvider.QueueFloat(0.5f);
            RandomProvider.QueueFloat(0.5f);

            IMutator selected = strategy.PickGenerated([first, excluded, alternative], 2);

            Assert.That(selected, Is.AssignableTo<IMultiMutator>());
            IMultiMutator generated = (IMultiMutator)selected;
            Assert.Multiple(() =>
            {
                Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, alternative }));
                Assert.That(generated.SubMutators.Keys, Does.Not.Contain(excluded));
                Assert.That(RandomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 1002f), (0f, 1f) }));
            });
        }

        [Test]
        public void PickUserDefinedMultiMutator_ExcludesIneligibleMatchingMultiMutators()
        {
            SelectionTestMultiMutator ineligible = MultiMutator("Ineligible Match", 1_000, 2, false);
            SelectionTestMultiMutator expected = MultiMutator("Expected Match", 10, 2);
            SelectionTestMultiMutator wrongCount = MultiMutator("Wrong Count", 1_000, 3);
            ExposedStrategy strategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator);

            RandomProvider.QueueFloat(5f);

            IMutator selected = strategy.PickUserDefined([ineligible, expected, wrongCount], 2);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(expected));
                Assert.That(RandomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
            });
        }

        [Test]
        public void PickUserDefinedMultiMutator_WhenNoEligibleMatchingMultiMutatorExists_BuildsGeneratedMultiMutator()
        {
            SelectionTestMultiMutator ineligible = MultiMutator("Ineligible Match", 1_000, 2, false);
            SelectionTestMultiMutator wrongCount = MultiMutator("Wrong Count", 1_000, 3);
            TestMutator first = Mutator("First", 1);
            TestMutator second = Mutator("Second", 1);
            ExposedStrategy strategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator);

            RandomProvider.QueueFloat(0.5f);
            RandomProvider.QueueFloat(0.5f);

            IMutator selected = strategy.PickUserDefined([ineligible, wrongCount, first, second], 2);

            Assert.That(selected, Is.AssignableTo<IMultiMutator>());
            IMultiMutator generated = (IMultiMutator)selected;
            Assert.Multiple(() =>
            {
                Assert.That(generated.Name, Is.EqualTo(string.Empty));
                Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, second }));
                Assert.That(RandomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 2f), (0f, 1f) }));
                Assert.That(Logger.WarningLogs, Is.Empty);
            });
        }

        [Test]
        public void PickUserDefinedMultiMutator_WhenGeneratedFallbackHasNoRegularMutators_ReturnsFallback()
        {
            SelectionTestMultiMutator ineligible = MultiMutator("Ineligible Match", 1_000, 2, false);
            SelectionTestMultiMutator wrongCount = MultiMutator("Wrong Count", 1_000, 3);
            ExposedStrategy strategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator);

            IMutator selected = strategy.PickUserDefined([ineligible, wrongCount], 2);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(NopMutator));
                Assert.That(RandomProvider.RangeCalls, Is.Empty);
                Assert.That(Logger.WarningLogs, Has.Exactly(2).EqualTo("Fell back to None mutator, invalid total weight: 0"));
            });
        }

        [Test]
        public void ShouldGenerateMulti_UsesChanceRollWithinInclusivePercentageRange()
        {
            ExposedStrategy strategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator);

            RandomProvider.QueueInt(20);

            Assert.That(strategy.ShouldGenerate(20), Is.True);
            Assert.That(RandomProvider.RandomRangeIntCalls, Has.One.EqualTo((1, 101)));
        }

        [Test]
        public void ShouldGenerateMulti_WhenChanceIsZero_DoesNotGenerateOrRoll()
        {
            ExposedStrategy strategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator);

            bool shouldGenerate = strategy.ShouldGenerate(0);

            Assert.Multiple(() =>
            {
                Assert.That(shouldGenerate, Is.False);
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.Empty);
                Assert.That(Logger.WarningLogs, Is.Empty);
            });
        }

        [Test]
        public void ShouldGenerateMulti_WhenChanceIsNegative_DoesNotGenerateAndLogsMisconfiguration()
        {
            ExposedStrategy strategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator);

            bool shouldGenerate = strategy.ShouldGenerate(-1);

            Assert.Multiple(() =>
            {
                Assert.That(shouldGenerate, Is.False);
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.Empty);
                Assert.That(Logger.WarningLogs, Is.EqualTo(new[]
                {
                    "Generated multi-mutator chance is misconfigured: -1.",
                    "The minimum allowed value is 0."
                }));
            });
        }

        [Test]
        public void ShouldGenerateMulti_WhenChanceIsOverOneHundred_GeneratesAndLogsMisconfiguration()
        {
            ExposedStrategy strategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator);

            bool shouldGenerate = strategy.ShouldGenerate(101);

            Assert.Multiple(() =>
            {
                Assert.That(shouldGenerate, Is.True);
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.Empty);
                Assert.That(Logger.WarningLogs, Is.EqualTo(new[]
                {
                    "Generated multi-mutator chance is misconfigured: 101.",
                    "The maximum allowed value is 100."
                }));
            });
        }

        [Test]
        public void ShouldGenerateMulti_WhenChanceIsOneHundred_GeneratesWithoutRolling()
        {
            ExposedStrategy strategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator);

            bool shouldGenerate = strategy.ShouldGenerate(100);

            Assert.Multiple(() =>
            {
                Assert.That(shouldGenerate, Is.True);
                Assert.That(RandomProvider.RandomRangeIntCalls, Is.Empty);
                Assert.That(Logger.WarningLogs, Is.Empty);
            });
        }

        private sealed class ExposedStrategy : MutatorSelectionStrategy
        {
            public ExposedStrategy(
                GeneratedMultiMutatorSelectionRulesRegistry multiRegistry,
                SingleMutatorSelectionRulesRegistry singleRegistry,
                IRepeatSelectionTracker repeatSelectionTracker,
                TestRandomProvider randomProvider,
                NopMutator fallbackMutator
            ) : base(multiRegistry, singleRegistry, repeatSelectionTracker, randomProvider, fallbackMutator)
            {
            }

            public override IMutator Execute(IList<IMutator> mutators)
            {
                throw new NotSupportedException();
            }

            public IMutator PickGenerated(IList<IMutator> mutators, int amountToPick)
            {
                return PickGeneratedMultiMutator(mutators, amountToPick);
            }

            public bool ShouldGenerate(int generatedChance)
            {
                return ShouldGenerateMulti(generatedChance);
            }

            public IMutator PickUserDefined(IList<IMutator> mutators, int amountOfSubMutators)
            {
                return TryPickUserDefinedMultiMutatorOrElseGenerate(mutators, amountOfSubMutators);
            }
        }
    }
}
