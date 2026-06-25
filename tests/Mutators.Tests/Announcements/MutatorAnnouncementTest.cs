using Mutators.Announcements;

namespace Mutators.Tests.Announcements
{
    internal class MutatorAnnouncementTest
    {
        [Test]
        public void UpdateSegment_WithNoChanges_DoesNotNotifyChanged()
        {
            MutatorAnnouncement announcement = CreateAnnouncementWithSegment(priority: 10, value: "value");
            int changedCount = 0;
            announcement.Changed += _ => changedCount++;

            announcement.UpdateSegment("segment", "value");
            announcement.UpdateSegment("segment", "value", 10);

            Assert.That(changedCount, Is.Zero);
        }

        [Test]
        public void UpdateSegment_WithNullPriority_KeepsCurrentPriority()
        {
            MutatorAnnouncement announcement = CreateAnnouncementWithSegment(priority: 10, value: "old");

            announcement.UpdateSegment("segment", "new");

            MutatorAnnouncementDescriptionSegment? segment = announcement.GetDescriptionSegment("segment");
            Assert.That(segment?.Value, Is.EqualTo("new"));
            Assert.That(segment?.Priority, Is.EqualTo(10));
        }

        [Test]
        public void UpdateSegment_WithChangedPriority_UpdatesSegmentOrder()
        {
            MutatorAnnouncement announcement = CreateAnnouncementWithSegment(priority: 1, value: "first");
            announcement.AddSegment(new MutatorAnnouncementDescriptionSegment("second", 2, "second"));

            announcement.UpdateSegment("segment", "first", 3);

            Assert.That(announcement.GetDescription(), Is.EqualTo("descriptionfirstsecond"));
        }

        private static MutatorAnnouncement CreateAnnouncementWithSegment(ushort priority, string value)
        {
            MutatorAnnouncement announcement = new("name", "description");
            announcement.AddSegment(new MutatorAnnouncementDescriptionSegment("segment", priority, value));
            return announcement;
        }
    }
}
