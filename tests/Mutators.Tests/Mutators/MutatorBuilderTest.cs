using Mutators.Enums;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Settings;
using Mutators.Tests.Extensions.Test;
using Mutators.Tests.Managers;
using Mutators.Tests.Mutators.Patches;
using Mutators.Tests.Settings.Specific;

namespace Mutators.Tests.Mutators
{
    class MutatorBuilderTest() : MutatorManagerTestBase(ModSettings.MultiMutatorScalingType.None)
    {
        [TearDown]
        public void TearDown()
        {
            HookOrderTestPatch.Calls.Clear();
            TestPatch.Metadata = null;
        }

        [Test]
        public void Build_CopiesConfiguration()
        {
            AbstractMutatorSettings settings = new TestMutatorSettings("Test", 1);
            Func<bool> condition = () => true;

            Mutator mutator = new MutatorBuilder(settings, MutatorDifficulty.Hard)
                .WithPatch(typeof(TestPatch))
                .WithCondition(condition)
                .WithSpecialActionOverlay()
                .Build();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(mutator.Settings, Is.SameAs(settings));
                Assert.That(mutator.Difficulty, Is.EqualTo(MutatorDifficulty.Hard));
                Assert.That(mutator.Patches, Is.EqualTo([typeof(TestPatch)]));
                Assert.That(mutator.Conditions, Is.EqualTo([condition]));
                Assert.That(mutator.HasSpecialAction, Is.True);
            }
        }

        [Test]
        public void Build_ChangingBuilderAfterwards_DoesNotAffectMutator()
        {
            List<string> calls = [];
            MutatorBuilder builder = new MutatorBuilder(new TestMutatorSettings("Test", 1), MutatorDifficulty.Normal)
                .BeforePatchAll(() => calls.Add("first"));

            Mutator mutator = builder.Build();

            builder.WithPatch(typeof(TestPatch))
                .WithCondition(() => false)
                .BeforePatchAll(() => calls.Add("second"));

            mutator.Patch();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(mutator.Patches, Is.Empty);
                Assert.That(mutator.Conditions, Is.Empty);
                Assert.That(calls, Is.EqualTo(["first"]));
            }
        }

        [Test]
        public void Hooks_RunAfterPatchTypeHooks_InRegistrationOrder()
        {
            List<string> calls = HookOrderTestPatch.Calls;
            Mutator mutator = new MutatorBuilder(new TestMutatorSettings("Test", 1), MutatorDifficulty.Normal)
                .WithPatch(typeof(HookOrderTestPatch))
                .BeforePatchAll(() => calls.Add("builder:BeforePatchAll:1"))
                .BeforePatchAll(() => calls.Add("builder:BeforePatchAll:2"))
                .AfterPatchAll(() => calls.Add("builder:AfterPatchAll"))
                .BeforeUnpatchAll(() => calls.Add("builder:BeforeUnpatchAll"))
                .AfterUnpatchAll(() => calls.Add("builder:AfterUnpatchAll"))
                .Build();

            mutator.Patch();
            mutator.Unpatch();

            Assert.That(calls, Is.EqualTo([
                "patch:BeforePatchAll",
                "builder:BeforePatchAll:1",
                "builder:BeforePatchAll:2",
                "patch:AfterPatchAll",
                "builder:AfterPatchAll",
                "patch:BeforeUnpatchAll",
                "builder:BeforeUnpatchAll",
                "patch:AfterUnpatchAll",
                "builder:AfterUnpatchAll"
            ]));
        }

        [Test]
        public void OnMetadataChanged_ReceivesMergedMetadata()
        {
            IDictionary<string, object>? received = null;
            Mutator mutator = new MutatorBuilder(new TestMutatorSettings("Test", 1), MutatorDifficulty.Normal)
                .WithPatch(typeof(TestPatch))
                .OnMetadataChanged(metadata => received = metadata)
                .Build();

            MutatorManager.Instance.GameStateChanged += _ => { };
            MutatorManager.Instance.GameState = MutatorsGameState.LevelReady;

            mutator.ConsumeMetadata(new Dictionary<string, object>
            {
                { "xepos.repo-mutators:test", new Dictionary<string, object> { { "caught", 5 } } }
            });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(received, Is.Not.Null);
                Assert.That(received, Is.SameAs(mutator.GetMetadata()));
                Assert.That(received, Contains.Key("caught").WithValue(5));
                Assert.That(TestPatch.Metadata, Is.SameAs(received));
            }
        }

        [Test]
        public void NullArguments_Throw()
        {
            MutatorBuilder builder = new MutatorBuilder(new TestMutatorSettings("Test", 1), MutatorDifficulty.Normal);

            using (Assert.EnterMultipleScope())
            {
                Assert.Throws<ArgumentNullException>(() => _ = new MutatorBuilder(null!, MutatorDifficulty.Normal));
                Assert.Throws<ArgumentNullException>(() => builder.WithPatch(null!));
                Assert.Throws<ArgumentNullException>(() => builder.WithCondition(null!));
                Assert.Throws<ArgumentNullException>(() => builder.BeforePatchAll(null!));
                Assert.Throws<ArgumentNullException>(() => builder.OnMetadataChanged(null!));
            }
        }
    }
}
