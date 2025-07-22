using Mutators.Mutators;
using Mutators.Rules.Registries;
using Mutators.Services.Selection;
using Mutators.Services.Selection.Strategies;
using Mutators.Settings;
using Mutators.Tests.Providers.Random;

namespace Mutators.Tests.Services.Selection
{
    internal class MutatorSelectionServiceTest : SelectionTestBase
    {
        [Test]
        public void GetWeightedMutator_UsesConfiguredStrategyAndPassesNonNopMutators()
        {
            SelectionTestMutator expected = Mutator("Expected", 1);
            SelectionTestMutator registered = Mutator("Registered", 1);
            CapturingStrategy noneStrategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator, NopMutator);
            CapturingStrategy randomStrategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator, expected);
            MutatorSelectionService service = CreateSelectionService(new Dictionary<ModSettings.MultiMutatorScalingType, MutatorSelectionStrategy>
            {
                { ModSettings.MultiMutatorScalingType.None, noneStrategy },
                { ModSettings.MultiMutatorScalingType.Random, randomStrategy }
            });

            SetScalingType(ModSettings.MultiMutatorScalingType.Random);

            IMutator selected = service.GetWeightedMutator([NopMutator, registered]);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(expected));
                Assert.That(noneStrategy.ExecuteCount, Is.Zero);
                Assert.That(randomStrategy.ExecuteCount, Is.EqualTo(1));
                Assert.That(randomStrategy.CapturedMutators, Is.EquivalentTo(new[] { registered }));
            });
        }

        [Test]
        public void GetWeightedMutator_WhenNopChanceSucceeds_ReturnsNopWithoutExecutingStrategy()
        {
            SelectionTestMutator registered = Mutator("Registered", 1);
            CapturingStrategy strategy = new(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator, registered);
            MutatorSelectionService service = CreateSelectionService(new Dictionary<ModSettings.MultiMutatorScalingType, MutatorSelectionStrategy>
            {
                { ModSettings.MultiMutatorScalingType.None, strategy }
            });

            SetScalingType(ModSettings.MultiMutatorScalingType.None);
            SetNopChance(100);

            IMutator selected = service.GetWeightedMutator([registered]);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Is.SameAs(NopMutator));
                Assert.That(strategy.ExecuteCount, Is.Zero);
                Assert.That(RandomProvider.RangeCalls, Is.Empty);
            });
        }

        private sealed class CapturingStrategy : MutatorSelectionStrategy
        {
            private readonly IMutator _result;

            public CapturingStrategy(
                GeneratedMultiMutatorSelectionRulesRegistry multiRegistry,
                SingleMutatorSelectionRulesRegistry singleRegistry,
                IRepeatSelectionTracker repeatSelectionTracker,
                TestRandomProvider randomProvider,
                NopMutator fallbackMutator,
                IMutator result
            ) : base(multiRegistry, singleRegistry, repeatSelectionTracker, randomProvider, fallbackMutator)
            {
                _result = result;
            }

            public int ExecuteCount { get; private set; }
            public IList<IMutator> CapturedMutators { get; private set; } = [];

            public override IMutator Execute(IList<IMutator> mutators)
            {
                ExecuteCount++;
                CapturedMutators = mutators;
                return _result;
            }
        }
    }
}
