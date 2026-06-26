using Mutators.Services.Selection;

namespace Mutators.Tests.Services.Selection
{
    internal class RepeatSelectionTrackerTest : SelectionTestBase
    {
        [Test]
        public void TrackSelectedMutator_UpdatesPreviousMutator()
        {
            TestMutator first = Mutator("First", 1);
            TestMutator second = Mutator("Second", 1);
            RepeatSelectionTracker tracker = new();

            tracker.TrackSelectedMutator(first);
            tracker.TrackSelectedMutator(second);

            Assert.That(tracker.PreviousMutator, Is.SameAs(second));
        }

        [Test]
        public void ShouldBlockRepeat_WhenRepeatProbabilityFallsBelowThreshold_ReturnsTrue()
        {
            TestMutator mutator = Mutator("Repeated", 1);
            RepeatSelectionTracker tracker = new();

            tracker.TrackSelectedMutator(mutator);

            Assert.That(tracker.ShouldBlockRepeat(mutator, 0.01f), Is.True);
        }

        [Test]
        public void ShouldBlockRepeat_ForDifferentMutator_ReturnsFalse()
        {
            TestMutator previous = Mutator("Previous", 1);
            TestMutator current = Mutator("Current", 1);
            RepeatSelectionTracker tracker = new();

            tracker.TrackSelectedMutator(previous);

            Assert.That(tracker.ShouldBlockRepeat(current, 0.01f), Is.False);
        }
    }
}
