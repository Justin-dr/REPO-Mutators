using System;
using Mutators.Mutators;

namespace Mutators.Services.Selection
{
    internal class RepeatSelectionTracker : IRepeatSelectionTracker
    {
        public IMutator? PreviousMutator { get; private set; }
        private int _repeatCount;
        
        public bool ShouldBlockRepeat(IMutator mutator, float probability)
        {
            const float outlierThreshold = 0.1f;

            return mutator == PreviousMutator && MathF.Pow(probability, _repeatCount + 1) < outlierThreshold;
        }

        public IMutator TrackSelectedMutator(IMutator mutator)
        {
            if (mutator == PreviousMutator)
            {
                _repeatCount++;
            }
            else
            {
                PreviousMutator = mutator;
                _repeatCount = 1;
            }

            return mutator;
        }
    }
}