using Mutators.Services.Selection;

namespace Mutators.Tests.Services.Selection
{
    internal class RepeatSelectionTrackerTest : SelectionTestBase
    {
        [Test]
        public void TrackSelectedMutator_UpdatesPreviousMutator()
        {
            SelectionTestMutator first = Mutator("First", 1);
            SelectionTestMutator second = Mutator("Second", 1);
            RepeatSelectionTracker tracker = new();

            tracker.TrackSelectedMutator(first);
            tracker.TrackSelectedMutator(second);

            Assert.That(tracker.PreviousMutator, Is.SameAs(second));
        }

        [Test]
        public void ShouldBlockRepeat_WhenRepeatProbabilityFallsBelowThreshold_ReturnsTrue()
        {
            SelectionTestMutator mutator = Mutator("Repeated", 1);
            RepeatSelectionTracker tracker = new();

            tracker.TrackSelectedMutator(mutator);

            Assert.That(tracker.ShouldBlockRepeat(mutator, 0.01f), Is.True);
        }

        [Test]
        public void ShouldBlockRepeat_ForDifferentMutator_ReturnsFalse()
        {
            SelectionTestMutator previous = Mutator("Previous", 1);
            SelectionTestMutator current = Mutator("Current", 1);
            RepeatSelectionTracker tracker = new();

            tracker.TrackSelectedMutator(previous);

            Assert.That(tracker.ShouldBlockRepeat(current, 0.01f), Is.False);
        }
    }
}
