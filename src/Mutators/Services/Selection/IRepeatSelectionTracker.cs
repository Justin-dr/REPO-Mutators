using Mutators.Mutators;

namespace Mutators.Services.Selection
{
    internal interface IRepeatSelectionTracker
    {
        IMutator? PreviousMutator { get; }
        bool ShouldBlockRepeat(IMutator mutator, float probability);
        
        IMutator TrackSelectedMutator(IMutator mutator);
    }
}