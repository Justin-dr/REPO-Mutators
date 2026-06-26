using Mutators.Announcements;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Settings;
using Mutators.Tests.Managers;

namespace Mutators.Tests.Announcements
{
    internal class MutatorAnnouncingBagTest() : MutatorManagerTestBase(ModSettings.MultiMutatorScalingType.None)
    {
        [Test]
        public void Prime_WithSingleMutator_AllowsBaseDescriptionEdit()
        {
            TestMutator mutator = new TestMutator("Single", "single description");
            MutatorManager.Instance.CurrentMutator = mutator;
            MutatorAnnouncingBag bag = new MutatorAnnouncingBag();

            bag.Prime(mutator);

            Assert.That(bag.TryGetAnnouncement(mutator.NamespacedName, out MutatorAnnouncement? announcement), Is.True);

            announcement!.UpdateBaseDescription("updated description");

            Assert.That(announcement.GetDescription(), Is.EqualTo("updated description"));
        }

        [Test]
        public void Prime_WithGeneratedMultiMutatorWithoutDescription_AllowsSubMutatorBaseDescriptionEdit()
        {
            TestMutator subMutator = new TestMutator("Sub", "sub description");
            TestMultiMutator multiMutator = new TestMultiMutator("Generated Multi", string.Empty, subMutator);
            MutatorManager.Instance.CurrentMutator = multiMutator;
            MutatorAnnouncingBag bag = new MutatorAnnouncingBag();

            bag.Prime(multiMutator);

            Assert.That(bag.TryGetAnnouncement(subMutator.NamespacedName, out MutatorAnnouncement? announcement), Is.True);

            announcement!.UpdateBaseDescription("updated description");

            Assert.That(announcement.GetDescription(), Is.EqualTo("updated description"));
        }

        [Test]
        public void Prime_WithMultiMutatorDescription_BlocksSubMutatorBaseDescriptionEdit()
        {
            TestMutator subMutator = new TestMutator("Sub", "sub description");
            TestMultiMutator multiMutator = new TestMultiMutator("Custom Multi", "custom multi description", subMutator);
            MutatorManager.Instance.CurrentMutator = multiMutator;
            MutatorAnnouncingBag bag = new MutatorAnnouncingBag();

            bag.Prime(multiMutator);

            Assert.That(bag.TryGetAnnouncement(subMutator.NamespacedName, out MutatorAnnouncement? announcement), Is.True);

            announcement!.UpdateBaseDescription("updated description");

            Assert.That(announcement.GetDescription(), Is.EqualTo("custom multi description"));
        }

        [Test]
        public void Prime_WithMultiMutatorDescription_AllowsSubMutatorDescriptionSegments()
        {
            TestMutator subMutator = new TestMutator("Sub", "sub description");
            TestMultiMutator multiMutator = new TestMultiMutator("Custom Multi", "custom multi description", subMutator);
            MutatorManager.Instance.CurrentMutator = multiMutator;
            MutatorAnnouncingBag bag = new MutatorAnnouncingBag();

            bag.Prime(multiMutator);

            Assert.That(bag.TryGetAnnouncement(subMutator.NamespacedName, out MutatorAnnouncement? announcement), Is.True);

            announcement!.AddOrUpdateSegment(new MutatorAnnouncementDescriptionSegment("test-segment", 10, "\nsegment"));

            Assert.That(announcement.GetDescription(), Is.EqualTo("custom multi description\nsegment"));
        }

        

        private sealed class TestMultiMutator : TestMutator, IMultiMutator
        {
            public TestMultiMutator(string name, string description, params IMutator[] subMutators)
                : base(name, description)
            {
                SubMutators = subMutators.ToDictionary(
                    mutator => mutator,
                    _ => (IDictionary<string, object>)new Dictionary<string, object>()
                );
            }

            public IReadOnlyDictionary<IMutator, IDictionary<string, object>> SubMutators { get; }
        }

        private sealed class TestSettings : AbstractMutatorSettings
        {
            public TestSettings(string name, string description) : base("Plugin", name, description)
            {
            }

            public override int Weight => 1;
            public override int MinimumLevel => 0;
            public override int MaximumLevel => 1000;
        }
    }
}
