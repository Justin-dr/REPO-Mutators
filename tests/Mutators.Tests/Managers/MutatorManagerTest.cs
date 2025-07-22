using Mutators.Enums;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Rules;
using Mutators.Settings;
using Mutators.Tests.Settings.Specific;
using Is = NUnit.Framework.Is;
using MutatorNames = Mutators.Mutators.Mutators;

namespace Mutators.Tests.Managers
{
    class MutatorManagerTest
    {
        class GetWeightedMutator_NoneScaling() : MutatorManagerTestBase(ModSettings.MultiMutatorScalingType.None)
        {
            [Test]
            public void WithSingleRegisteredMutator_ReturnsThatMutator()
            {
                TestMutator expected = new("Expected", 10);

                SetRandomFloatValue(0.5f);
                mutatorManager.RegisterMutator(expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void WithOnlyFirstMutatorWeighted_ReturnsFirstMutator()
            {
                TestMutator expected = new("Expected", 10);
                TestMutator zeroWeight = new("Zero", 0);

                SetRandomFloatValue(0.5f);
                Register(expected, zeroWeight);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void WithOnlySecondMutatorWeighted_ReturnsSecondMutator()
            {
                TestMutator zeroWeight = new("Zero", 0);
                TestMutator expected = new("Expected", 10);

                SetRandomFloatValue(0.5f);
                Register(zeroWeight, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }
            
            [Test]
            public void WithOnlySecondMutatorWeighted_EligibilityFalse_ReturnsNopMutator()
            {
                TestMutator zeroWeight = new("Zero", 0);
                TestMutator ineligible = new("Ineligible", 10, false);

                SetRandomFloatValue(0.5f);
                Register(zeroWeight, ineligible);
                
                Assert.That(mutatorManager.GetWeightedMutator(), Is.TypeOf<NopMutator>());
                Assert.That(logger.WarningLogs[0], Is.EqualTo("Fell back to None mutator, invalid total weight: 0"));
            }
            
            [Test]
            public void WithOnlySecondMutatorWeighted_ZeroRandom_ReturnsSecondMutator()
            {
                TestMutator zeroWeight = new("Zero", 0);
                TestMutator expected = new("Expected", 10);

                SetRandomFloatValue(0f);
                Register(zeroWeight, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void WithZeroWeightMutatorsBeforeAndAfterWeightedMutator_ReturnsWeightedMutator()
            {
                TestMutator firstZero = new("First Zero", 0);
                TestMutator expected = new("Expected", 10);
                TestMutator secondZero = new("Second Zero", 0);

                SetRandomFloatValue(0.5f);
                Register(firstZero, expected, secondZero);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void WithOnlyOnePositiveEligibleMutator_ReturnsSameMutatorAcrossRepeatedCalls()
            {
                TestMutator expected = new("Expected", 10);

                mutatorManager.RegisterMutator(expected);
                SetRandomFloatValues(Enumerable.Repeat(0.5f, 25).ToArray());

                for (int i = 0; i < 25; i++)
                {
                    Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                }
            }

            [Test]
            public void DoesNotPatchReturnedMutator()
            {
                TestMutator expected = new("Expected", 10);

                SetRandomFloatValue(0.5f);
                mutatorManager.RegisterMutator(expected);

                mutatorManager.GetWeightedMutator();

                Assert.Multiple(() =>
                {
                    Assert.That(expected.Active, Is.False);
                    Assert.That(expected.PatchCount, Is.Zero);
                    Assert.That(expected.UnpatchCount, Is.Zero);
                });
            }

            [Test]
            public void DoesNotChangeCurrentMutator()
            {
                IMutator currentBeforeSelection = mutatorManager.CurrentMutator;
                TestMutator expected = new("Expected", 10);

                SetRandomFloatValue(0.5f);
                mutatorManager.RegisterMutator(expected);
                IMutator selected = mutatorManager.GetWeightedMutator();

                Assert.Multiple(() =>
                {
                    Assert.That(selected, Is.SameAs(expected));
                    Assert.That(mutatorManager.CurrentMutator, Is.SameAs(currentBeforeSelection));
                });
            }

            [Test]
            public void ExcludesMultiMutatorWithZeroSubMutators()
            {
                TestMultiMutator excluded = CreateMultiMutator("Excluded Multi", 10_000, 0);
                TestMutator expected = new("Expected", 1);

                SetRandomFloatValue(0.5f);
                Register(excluded, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void ExcludesMultiMutatorWithTwoSubMutators()
            {
                TestMultiMutator excluded = CreateMultiMutator("Excluded Multi", 10_000, 2);
                TestMutator expected = new("Expected", 1);

                SetRandomFloatValue(0.5f);
                Register(excluded, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void ExcludesMultiMutatorWithManySubMutators()
            {
                TestMultiMutator excluded = CreateMultiMutator("Excluded Multi", 10_000, 5);
                TestMutator expected = new("Expected", 1);

                SetRandomFloatValue(0.5f);
                Register(excluded, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void WithOnlyOneSubMutatorMulti_ReturnsThatMultiMutator()
            {
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 10, 1);

                SetRandomFloatValue(0.5f);
                mutatorManager.RegisterMutator(expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void IncludesOneSubMutatorMultiWhenRegularMutatorHasNoWeight()
            {
                TestMutator zeroWeightRegular = new("Zero Regular", 0);
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 10, 1);

                SetRandomFloatValue(0.5f);
                Register(zeroWeightRegular, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void IncludesOneSubMutatorMultiAlongsideRegularMutators()
            {
                TestMutator zeroWeightRegular = new("Zero Regular", 0);
                TestMutator anotherZeroWeightRegular = new("Another Zero Regular", 0);
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 10, 1);

                SetRandomFloatValue(0.5f);
                Register(zeroWeightRegular, expected, anotherZeroWeightRegular);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void
                WithOneSubMutatorMultiAndExcludedMulti_ReturnsOneSubMutatorMulti()
            {
                TestMultiMutator excluded = CreateMultiMutator("Excluded Multi", 10_000, 3);
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 10, 1);

                SetRandomFloatValue(0.5f);
                Register(excluded, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void
                WithRegularMutatorAndExcludedMulti_ReturnsRegularMutatorEvenWhenExcludedHasHigherWeight()
            {
                TestMutator expected = new("Expected", 1);
                TestMultiMutator excluded = CreateMultiMutator("Excluded Multi", 1_000_000, 2);

                SetRandomFloatValue(0.5f);
                Register(expected, excluded);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void NeverSelectsExcludedMultiMutatorsAcrossManyPicks()
            {
                const int iterations = 250;
                IList<string> selectedNames = [];

                SetRandomFloatValues(Enumerable.Repeat(0.5f, iterations).ToArray());

                for (int i = 0; i < iterations; i++)
                {
                    MutatorManager manager = CreateManager(
                        CreateMultiMutator("Excluded Multi", 100_000, 2),
                        new TestMutator("Expected", 1)
                    );

                    selectedNames.Add(manager.GetWeightedMutator().Name);
                }

                Assert.Multiple(() =>
                {
                    Assert.That(selectedNames, Does.Not.Contain("Excluded Multi"));
                    Assert.That(selectedNames, Has.All.EqualTo("Expected"));
                });
            }

            [Test]
            public void NeverSelectsZeroWeightMutatorAcrossManyPicksWhenPositiveAlternativeExists()
            {
                const int iterations = 250;
                IList<string> selectedNames = [];

                SetRandomFloatValues(Enumerable.Repeat(0.5f, iterations).ToArray());

                for (int i = 0; i < iterations; i++)
                {
                    MutatorManager manager = CreateManager(
                        new TestMutator("Zero", 0),
                        new TestMutator("Expected", 10)
                    );

                    selectedNames.Add(manager.GetWeightedMutator().Name);
                }

                Assert.Multiple(() =>
                {
                    Assert.That(selectedNames, Does.Not.Contain("Zero"));
                    Assert.That(selectedNames, Has.All.EqualTo("Expected"));
                });
            }

            [Test]
            public void SelectsFirstMutatorWhenRandomValueIsInsideFirstWeightBucket()
            {
                TestMutator expected = new("First", 1);
                TestMutator second = new("Second", 9);

                SetRandomFloatValue(0.5f);
                Register(expected, second);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void SelectsFirstMutatorWhenRandomValueEqualsFirstWeightBoundary()
            {
                TestMutator expected = new("First", 1);
                TestMutator second = new("Second", 9);

                SetRandomFloatValue(1f);
                Register(expected, second);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void SelectsSecondMutatorWhenRandomValueIsAboveFirstWeightBoundary()
            {
                TestMutator first = new("First", 1);
                TestMutator expected = new("Second", 9);

                SetRandomFloatValue(1.01f);
                Register(first, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void SelectsSecondMutatorWhenRandomValueEqualsSecondWeightBoundary()
            {
                TestMutator first = new("First", 1);
                TestMutator expected = new("Second", 9);

                SetRandomFloatValue(10f);
                Register(first, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void SelectsThirdMutatorWhenRandomValueIsAboveSecondWeightBoundary()
            {
                TestMutator first = new("First", 2);
                TestMutator second = new("Second", 3);
                TestMutator expected = new("Third", 5);

                SetRandomFloatValue(5.01f);
                Register(first, second, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void SelectsOneSubMutatorMultiWhenRandomValueIsInsideItsWeightBucket()
            {
                TestMutator regular = new("Regular", 1);
                TestMultiMutator expected = CreateMultiMutator("One Sub Multi", 9, 1);

                SetRandomFloatValue(1.01f);
                Register(regular, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
            }

            [Test]
            public void UsesOnlyEligibleMutatorWeightsAsRandomRangeMaximum()
            {
                TestMutator expected = new("Expected", 1);
                TestMultiMutator excluded = CreateMultiMutator("Excluded Multi", 999, 2);

                SetRandomFloatValue(0.5f);
                Register(expected, excluded);
                mutatorManager.GetWeightedMutator();

                Assert.That(randomProvider.RangeCalls, Has.One.EqualTo((0f, 1f)));
            }

            [Test]
            public void IncludesOneSubMutatorMultiWeightInRandomRangeMaximum()
            {
                TestMutator regular = new("Regular", 1);
                TestMultiMutator expected = CreateMultiMutator("One Sub Multi", 9, 1);

                SetRandomFloatValue(0.5f);
                Register(regular, expected);
                mutatorManager.GetWeightedMutator();

                Assert.That(randomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
            }

            [Test]
            public void WithConfiguredNop_WhenChanceRollSelectsNop_ReturnsNop()
            {
                TestMutator weighted = new("Weighted", 10);

                SetConfiguredNopChance(50);
                SetRandomFloatValue(50f);
                Register(weighted);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.TypeOf<NopMutator>());
                Assert.That(randomProvider.RangeCalls, Has.One.EqualTo((0f, 100f)));
            }

            [Test]
            public void WithConfiguredNop_WhenChanceRollFails_WeightsOnlyNonNopMutators()
            {
                TestMutator first = new("First", 1);
                TestMutator expected = new("Second", 9);

                SetConfiguredNopChance(50);
                SetRandomFloatValues(75f, 1.01f);
                Register(first, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 100f), (0f, 10f) }));
            }

            [Test]
            public void WithConfiguredNop_WhenRepeatThresholdReached_SkipsNopChanceRoll()
            {
                TestMutator expected = new("Expected", 10);

                SetConfiguredNopChance(50);
                SetRandomFloatValues(25f, 25f, 25f, 5f);
                Register(expected);

                IMutator firstPick = mutatorManager.GetWeightedMutator();
                IMutator secondPick = mutatorManager.GetWeightedMutator();
                IMutator thirdPick = mutatorManager.GetWeightedMutator();
                IMutator fourthPick = mutatorManager.GetWeightedMutator();

                Assert.Multiple(() =>
                {
                    Assert.That(firstPick, Is.TypeOf<NopMutator>());
                    Assert.That(secondPick, Is.SameAs(firstPick));
                    Assert.That(thirdPick, Is.SameAs(firstPick));
                    Assert.That(fourthPick, Is.SameAs(expected));
                });
                Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 100f), (0f, 100f), (0f, 100f), (0f, 10f) }));
            }
        }

        class GetWeightedMutator_RandomScaling() : MutatorManagerTestBase(ModSettings.MultiMutatorScalingType.Random)
        {
            [Test]
            public void UsesConfiguredAmountWeightsWithinConfiguredRange()
            {
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 7, 3);
                TestMultiMutator otherAmount = CreateMultiMutator("Other Amount", 1_000, 4);

                ConfigureRandomAmountRange(2, 4);
                SetAllRandomAmountWeights(0);
                SetRandomAmountWeight(1, 1_000);
                SetRandomAmountWeight(3, 10);
                SetRandomAmountWeight(4, 30);
                SetRandomAmountWeight(5, 1_000);
                SetRandomGeneratedChance(3, 0);
                SetRandomIntValue(5);
                SetRandomFloatValue(3f);
                Register(expected, otherAmount);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Has.One.EqualTo((0, 40)));
                    Assert.That(randomProvider.RangeCalls, Has.One.EqualTo((0f, 7f)));
                });
            }

            [Test]
            public void WithAmountOne_IncludesOneSubMutatorMulti()
            {
                TestMutator regular = new("Regular", 1);
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 9, 1);

                ConfigureRandomAmountRange(1, 1);
                SetAllRandomAmountWeights(0);
                SetRandomAmountWeight(1, 10);
                SetRandomIntValue(0);
                SetRandomFloatValue(1.01f);
                Register(regular, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Has.One.EqualTo((0, 10)));
                    Assert.That(randomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
                });
            }

            [Test]
            public void WithMatchingRegisteredMulti_WhenGenerationChanceRollFails_ReturnsRegisteredMulti()
            {
                TestMutator regular = new("Regular", 1_000);
                TestMultiMutator wrongCount = CreateMultiMutator("Wrong Count", 1_000, 3);
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 10, 2);

                ConfigureRandomAmountRange(2, 2);
                SetAllRandomAmountWeights(0);
                SetRandomAmountWeight(2, 10);
                SetRandomGeneratedChance(2, 20);
                SetRandomIntValues(0, 21);
                SetRandomFloatValue(5f);
                Register(regular, wrongCount, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (0, 10), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
                });
            }

            [Test]
            public void WithGenerationSelected_GeneratesMultiFromRegularMutators()
            {
                TestMultiMutator existingMulti = CreateMultiMutator("Existing Multi", 1_000, 2);
                TestMutator first = new("First", 1);
                TestMutator second = new("Second", 1);

                ConfigureRandomAmountRange(2, 2);
                SetAllRandomAmountWeights(0);
                SetRandomAmountWeight(2, 10);
                SetRandomGeneratedChance(2, 100);
                SetRandomIntValue(0);
                SetRandomFloatValues(0.5f, 0.5f);
                Register(existingMulti, first, second);

                IMutator selected = mutatorManager.GetWeightedMutator();

                Assert.That(selected, Is.AssignableTo<IMultiMutator>());
                IMultiMutator generated = (IMultiMutator)selected;
                Assert.Multiple(() =>
                {
                    Assert.That(generated.Name, Is.EqualTo(string.Empty));
                    Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, second }));
                    Assert.That(randomProvider.RandomRangeIntCalls, Has.One.EqualTo((0, 10)));
                    Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 2f), (0f, 1f) }));
                });
            }

            [Test]
            public void WithNoMatchingRegisteredMulti_WhenGenerationChanceRollFails_GeneratesMultiMutator()
            {
                TestMultiMutator wrongCount = CreateMultiMutator("Wrong Count", 1_000, 3);
                TestMutator first = new("First", 1);
                TestMutator second = new("Second", 1);

                ConfigureRandomAmountRange(2, 2);
                SetAllRandomAmountWeights(0);
                SetRandomAmountWeight(2, 10);
                SetRandomGeneratedChance(2, 0);
                SetRandomIntValue(0);
                SetRandomFloatValues(0.5f, 0.5f);
                Register(wrongCount, first, second);

                IMutator selected = mutatorManager.GetWeightedMutator();

                Assert.That(selected, Is.AssignableTo<IMultiMutator>());
                IMultiMutator generated = (IMultiMutator)selected;
                Assert.Multiple(() =>
                {
                    Assert.That(generated.Name, Is.EqualTo(string.Empty));
                    Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, second }));
                    Assert.That(randomProvider.RandomRangeIntCalls, Has.One.EqualTo((0, 10)));
                    Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 2f), (0f, 1f) }));
                    Assert.That(logger.WarningLogs, Is.Empty);
                });
            }

            [Test]
            public void WithZeroAmountWeights_FallsBackToUniformAmountPick()
            {
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 10, 3);

                ConfigureRandomAmountRange(2, 4);
                SetAllRandomAmountWeights(0);
                SetRandomGeneratedChance(3, 0);
                SetRandomIntValue(3);
                SetRandomFloatValue(5f);
                Register(expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Has.One.EqualTo((2, 5)));
                    Assert.That(randomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
                });
            }
        }

        class GetWeightedMutator_MoonScaling() : MutatorManagerTestBase(ModSettings.MultiMutatorScalingType.Moon)
        {
            private const int MoonLevel = 3;

            [Test]
            public void WithSingleRegisteredMutator_WhenMoonRangePicksOne_ReturnsThatMutator()
            {
                TestMutator expected = new("Expected", 10);

                ConfigureMoonRange(MoonLevel, 1, 1, 100);
                SetMoonLevel(MoonLevel);
                SetRandomIntValue(1);
                SetRandomFloatValue(0.5f);
                mutatorManager.RegisterMutator(expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Has.One.EqualTo((1, 2)));
                    Assert.That(randomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
                });
            }

            [Test]
            public void WithMoonRangePickingOne_IncludesOneSubMutatorMulti()
            {
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 10_000, 1);
                TestMutator regular = new("Regular", 1);

                ConfigureMoonRange(MoonLevel, 1, 1, 100);
                SetMoonLevel(MoonLevel);
                SetRandomIntValue(1);
                SetRandomFloatValue(0.5f);
                Register(expected, regular);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.That(randomProvider.RangeCalls, Has.One.EqualTo((0f, 10_001f)));
            }

            [Test]
            public void WithMoonRangePickingOne_AndOnlyOneSubMutatorMulti_ReturnsThatMultiMutator()
            {
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 10, 1);

                ConfigureMoonRange(MoonLevel, 1, 1, 100);
                SetMoonLevel(MoonLevel);
                SetRandomIntValue(1);
                SetRandomFloatValue(0.5f);
                mutatorManager.RegisterMutator(expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Has.One.EqualTo((1, 2)));
                    Assert.That(randomProvider.RangeCalls, Has.One.EqualTo((0f, 10f)));
                    Assert.That(logger.WarningLogs, Is.Empty);
                });
            }

            [Test]
            public void UsesCurrentMoonRangeForAmountAndGeneratedChance()
            {
                const int currentMoonLevel = 5;
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 7, 3);
                TestMutator regular = new("Regular", 100);

                ConfigureMoonRange(currentMoonLevel, 2, 4, 13);
                SetMoonLevels(currentMoonLevel, currentMoonLevel);
                SetRandomIntValues(3, 14);
                SetRandomFloatValue(3f);
                Register(expected, regular);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 5), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 7f) }));
                });
            }

            [Test]
            public void WithMatchingRegisteredMulti_WhenGenerationChanceRollFails_ReturnsRegisteredMulti()
            {
                TestMutator regular = new("Regular", 1_000);
                TestMultiMutator wrongCount = CreateMultiMutator("Wrong Count", 1_000, 3);
                TestMultiMutator expected = CreateMultiMutator("Expected Multi", 10, 2);

                ConfigureMoonRange(MoonLevel, 2, 2, 20);
                SetMoonLevels(MoonLevel, MoonLevel);
                SetRandomIntValues(2, 21);
                SetRandomFloatValue(5f);
                Register(regular, wrongCount, expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 10f) }));
                });
            }

            [Test]
            public void WithGenerationSelected_GeneratesMultiFromRegularMutators()
            {
                TestMultiMutator wrongCount = CreateMultiMutator("Wrong Count", 1_000, 3);
                TestMultiMutator existingMulti = CreateMultiMutator("Existing Multi", 1_000, 2);
                TestMutator first = new("First", 1);
                TestMutator second = new("Second", 1);

                ConfigureMoonRange(MoonLevel, 2, 2, 20);
                SetMoonLevels(MoonLevel, MoonLevel);
                SetRandomIntValues(2, 20);
                SetRandomFloatValues(0.5f, 0.5f);
                Register(wrongCount, existingMulti, first, second);

                IMutator selected = mutatorManager.GetWeightedMutator();

                Assert.That(selected, Is.AssignableTo<IMultiMutator>());
                IMultiMutator generated = (IMultiMutator)selected;
                Assert.Multiple(() =>
                {
                    Assert.That(generated.Name, Is.EqualTo(string.Empty));
                    Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, second }));
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 2f), (0f, 1f) }));
                });
            }

            [Test]
            public void WithNoMatchingRegisteredMulti_WhenGenerationChanceRollFails_GeneratesMultiMutator()
            {
                TestMultiMutator wrongCount = CreateMultiMutator("Wrong Count", 1_000, 3);
                TestMutator first = new("First", 1);
                TestMutator second = new("Second", 1);

                ConfigureMoonRange(MoonLevel, 2, 2, 20);
                SetMoonLevels(MoonLevel, MoonLevel);
                SetRandomIntValues(2, 21);
                SetRandomFloatValues(0.5f, 0.5f);
                Register(wrongCount, first, second);

                IMutator selected = mutatorManager.GetWeightedMutator();

                Assert.That(selected, Is.AssignableTo<IMultiMutator>());
                IMultiMutator generated = (IMultiMutator)selected;
                Assert.Multiple(() =>
                {
                    Assert.That(generated.Name, Is.EqualTo(string.Empty));
                    Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, second }));
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 2f), (0f, 1f) }));
                    Assert.That(logger.WarningLogs, Is.Empty);
                });
            }

            [Test]
            public void WithGeneratedMutualExclusionRule_WhenFirstNamePickedFirst_DoesNotPickSecondName()
            {
                TestMutator lessIsMore = new(MutatorNames.LessIsMoreName, 1);
                TestMutator volatileCargo = new(MutatorNames.VolatileCargoName, 1_000);
                TestMutator expectedAlternative = new("Alternative", 1);

                mutatorManager.GeneratedMultiMutatorSelectionRulesRegistry.Register(
                    "TestLessIsMoreWithVolatileCargo",
                    MultiMutatorRules.MutualExclusionRule(
                        lessIsMore.NamespacedName,
                        volatileCargo.NamespacedName
                    )
                );
                
                ConfigureMoonRange(MoonLevel, 2, 2, 20);
                SetMoonLevels(MoonLevel, MoonLevel);
                SetRandomIntValues(2, 20);
                SetRandomFloatValues(0.5f, 0.5f);
                Register(lessIsMore, volatileCargo, expectedAlternative);

                IMutator selected = mutatorManager.GetWeightedMutator();

                Assert.That(selected, Is.AssignableTo<IMultiMutator>());
                IMultiMutator generated = (IMultiMutator)selected;
                Assert.Multiple(() =>
                {
                    Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { lessIsMore, expectedAlternative }));
                    Assert.That(generated.SubMutators.Keys, Does.Not.Contain(volatileCargo));
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 1002f), (0f, 1f) }));
                });
            }

            [Test]
            public void WithGeneratedMutualExclusionRule_WhenSecondNamePickedFirst_DoesNotPickFirstName()
            {
                TestMutator volatileCargo = new(MutatorNames.VolatileCargoName, 1);
                TestMutator lessIsMore = new(MutatorNames.LessIsMoreName, 1_000);
                TestMutator expectedAlternative = new("Alternative", 1);

                mutatorManager.GeneratedMultiMutatorSelectionRulesRegistry.Register(
                    "TestLessIsMoreWithVolatileCargo",
                    MultiMutatorRules.MutualExclusionRule(
                        lessIsMore.NamespacedName,
                        volatileCargo.NamespacedName
                    )
                );
                ConfigureMoonRange(MoonLevel, 2, 2, 20);
                SetMoonLevels(MoonLevel, MoonLevel);
                SetRandomIntValues(2, 20);
                SetRandomFloatValues(0.5f, 0.5f);
                Register(volatileCargo, lessIsMore, expectedAlternative);

                IMutator selected = mutatorManager.GetWeightedMutator();

                Assert.That(selected, Is.AssignableTo<IMultiMutator>());
                IMultiMutator generated = (IMultiMutator)selected;
                Assert.Multiple(() =>
                {
                    Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { volatileCargo, expectedAlternative }));
                    Assert.That(generated.SubMutators.Keys, Does.Not.Contain(lessIsMore));
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 1002f), (0f, 1f) }));
                });
            }

            [Test]
            public void WithGenerationSelected_AndMatchingRegisteredMulti_ReturnsGeneratedMulti()
            {
                TestMultiMutator registeredAlternative = CreateMultiMutator("Registered Alternative", 10, 2);
                TestMultiMutator otherRegisteredAlternative = CreateMultiMutator("Other Registered Alternative", 1_000, 2);
                TestMutator first = new("First", 1);
                TestMutator second = new("Second", 9);

                ConfigureMoonRange(MoonLevel, 2, 2, 20);
                SetMoonLevels(MoonLevel, MoonLevel);
                SetRandomIntValues(2, 10);
                SetRandomFloatValues(0.5f, 1f);
                Register(registeredAlternative, otherRegisteredAlternative, first, second);

                IMutator selected = mutatorManager.GetWeightedMutator();

                Assert.That(selected, Is.AssignableTo<IMultiMutator>());
                IMultiMutator generated = (IMultiMutator)selected;
                Assert.Multiple(() =>
                {
                    Assert.That(generated, Is.Not.SameAs(registeredAlternative));
                    Assert.That(generated.Name, Is.EqualTo(string.Empty));
                    Assert.That(generated.SubMutators.Keys, Is.EquivalentTo(new[] { first, second }));
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 10f), (0f, 9f) }));
                });
            }

            [Test]
            public void WithGenerationSelected_AndNoRegularMutators_ReturnsNopMutator()
            {
                TestMultiMutator registeredAlternative = CreateMultiMutator("Registered Alternative", 10, 2);

                ConfigureMoonRange(MoonLevel, 2, 2, 20);
                SetMoonLevels(MoonLevel, MoonLevel);
                SetRandomIntValues(2, 10);
                mutatorManager.RegisterMutator(registeredAlternative);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.TypeOf<NopMutator>());
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Is.Empty);
                    Assert.That(logger.WarningLogs, Has.Exactly(2).EqualTo("Fell back to None mutator, invalid total weight: 0"));
                });
            }

            [Test]
            public void WithGenerationSelected_AndOnlyOneRegularMutator_ReturnsThatMutator()
            {
                TestMutator expected = new("Expected", 10);

                ConfigureMoonRange(MoonLevel, 2, 2, 20);
                SetMoonLevels(MoonLevel, MoonLevel);
                SetRandomIntValues(2, 20);
                SetRandomFloatValue(5f);
                mutatorManager.RegisterMutator(expected);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.SameAs(expected));
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Is.EqualTo(new[] { (0f, 10f) }));
                    Assert.That(logger.WarningLogs, Has.One.EqualTo("Fell back to None mutator, invalid total weight: 0"));
                });
            }

            [Test]
            public void WithGenerationSelected_AndOnlyIneligibleRegularMutators_ReturnsNopMutator()
            {
                TestMutator ineligible = new("Ineligible", 10, false);

                ConfigureMoonRange(MoonLevel, 2, 2, 20);
                SetMoonLevels(MoonLevel, MoonLevel);
                SetRandomIntValues(2, 20);
                mutatorManager.RegisterMutator(ineligible);

                Assert.That(mutatorManager.GetWeightedMutator(), Is.TypeOf<NopMutator>());
                Assert.Multiple(() =>
                {
                    Assert.That(randomProvider.RandomRangeIntCalls, Is.EqualTo(new[] { (2, 3), (1, 101) }));
                    Assert.That(randomProvider.RangeCalls, Is.Empty);
                    Assert.That(logger.WarningLogs, Has.Exactly(2).EqualTo("Fell back to None mutator, invalid total weight: 0"));
                });
            }
        }

        private static TestMultiMutator CreateMultiMutator(string name, int weight, int subMutatorCount)
        {
            Dictionary<IMutator, IDictionary<string, object>> subMutators = Enumerable.Range(0, subMutatorCount)
                .Select(IMutator (index) => new TestMutator($"{name} Sub {index + 1}", 1))
                .ToDictionary(mutator => mutator, IDictionary<string, object> (_) => new Dictionary<string, object>());

            return new TestMultiMutator(name, weight, subMutators);
        }

        private class TestMutator : IMutator
        {
            private static readonly IReadOnlyList<Type> EmptyPatches = [];
            private static readonly IReadOnlyList<Func<bool>> EmptyConditions = [];

            public TestMutator(string name, int weight, bool isEligibleForSelection = true, MutatorDifficulty difficulty = MutatorDifficulty.Normal)
            {
                Settings = new TestMutatorSettings(name, weight, isEligibleForSelection);
                Difficulty = difficulty;
            }

            public int PatchCount { get; private set; }
            public int UnpatchCount { get; private set; }
            public string NamespacedName => Settings.NamespacedName;
            public string Name => Settings.MutatorName;
            public string Description => Settings.MutatorDescription;
            public MutatorDifficulty Difficulty { get; }
            public MutatorSource Source => MutatorSource.Mod;
            public bool Active { get; private set; }
            public bool HasSpecialAction => false;
            public AbstractMutatorSettings Settings { get; }
            public IReadOnlyList<Type> Patches => EmptyPatches;
            public IReadOnlyList<Func<bool>> Conditions => EmptyConditions;

            public void Patch()
            {
                Active = true;
                PatchCount++;
            }

            public void Unpatch()
            {
                Active = false;
                UnpatchCount++;
            }

            public void ConsumeMetadata(IDictionary<string, object> metadata)
            {
            }
        }

        private sealed class TestMultiMutator : TestMutator, IMultiMutator
        {
            public TestMultiMutator(
                string name,
                int weight,
                IReadOnlyDictionary<IMutator, IDictionary<string, object>> subMutators
            ) : base(name, weight)
            {
                SubMutators = subMutators;
            }

            public IReadOnlyDictionary<IMutator, IDictionary<string, object>> SubMutators { get; }
        }
    }
}
