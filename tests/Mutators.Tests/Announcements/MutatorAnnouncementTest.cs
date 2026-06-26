using Mutators.Announcements;

namespace Mutators.Tests.Announcements
{
    internal class MutatorAnnouncementTest
    {
        [Test]
        public void UpdateSegment_WithNoChanges_DoesNotNotifyChanged()
        {
            MutatorAnnouncement announcement = CreateAnnouncementWithSegment(priority: 10, value: "value", allowEditBaseDescription: true);
            int changedCount = 0;
            announcement.Changed += _ => changedCount++;

            announcement.UpdateSegment("segment", "value");
            announcement.UpdateSegment("segment", "value", 10);

            Assert.That(changedCount, Is.Zero);
        }

        [Test]
        public void UpdateSegment_WithNullPriority_KeepsCurrentPriority()
        {
            MutatorAnnouncement announcement = CreateAnnouncementWithSegment(priority: 10, value: "old", allowEditBaseDescription: true);

            announcement.UpdateSegment("segment", "new");

            MutatorAnnouncementDescriptionSegment? segment = announcement.GetDescriptionSegment("segment");
            Assert.That(segment?.Value, Is.EqualTo("new"));
            Assert.That(segment?.Priority, Is.EqualTo(10));
        }

        [Test]
        public void UpdateSegment_WithChangedPriority_UpdatesSegmentOrder()
        {
            MutatorAnnouncement announcement = CreateAnnouncementWithSegment(priority: 1, value: "first", allowEditBaseDescription: true);
            announcement.AddSegment(new MutatorAnnouncementDescriptionSegment("second", 2, "second"));

            announcement.UpdateSegment("segment", "first", 3);

            Assert.That(announcement.GetDescription(), Is.EqualTo("descriptionfirstsecond"));
        }

        [Test]
        public void UpdateBaseDescription_WithAllowedBaseDescriptionEdit_UpdatesDescription()
        {
            MutatorAnnouncement announcement = new("name", "description", true);

            announcement.UpdateBaseDescription("new description");

            Assert.That(announcement.DescriptionBase.Value, Is.EqualTo("new description"));
            Assert.That(announcement.GetDescription(), Is.EqualTo("new description"));
        }

        [Test]
        public void UpdateBaseDescription_WithDisallowedBaseDescriptionEdit_DoesNotUpdateDescription()
        {
            MutatorAnnouncement announcement = new ("name", "description", false);

            announcement.UpdateBaseDescription("new description");
            
            Assert.That(announcement.DescriptionBase.Value, Is.EqualTo("description"));
            Assert.That(announcement.GetDescription(), Is.EqualTo("description"));
        }

        private static MutatorAnnouncement CreateAnnouncementWithSegment(ushort priority, string value, bool allowEditBaseDescription)
        {
            MutatorAnnouncement announcement = new("name", "description", allowEditBaseDescription);
            announcement.AddSegment(new MutatorAnnouncementDescriptionSegment("segment", priority, value));
            return announcement;
        }
    }
}
