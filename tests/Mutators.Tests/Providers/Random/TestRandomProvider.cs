using Mutators.Providers.Random;

namespace Mutators.Tests.Providers.Random
{
    public class TestRandomProvider : IRandomProvider
    {
        private Queue<float> _floatQueue = new Queue<float>();
        private Queue<int> _intQueue = new Queue<int>();
        public IList<(float MinInclusive, float MaxInclusive)> RangeCalls { get; } = new List<(float MinInclusive, float MaxInclusive)>();
        public IList<(int MinInclusive, int MaxExclusive)> RandomRangeIntCalls { get; } = new List<(int MinInclusive, int MaxExclusive)>();
        
        public float Range(float minInclusive, float maxInclusive)
        {
            RangeCalls.Add((minInclusive, maxInclusive));
            return _floatQueue.Dequeue();
        }

        public int RandomRangeInt(int minInclusive, int maxExclusive)
        {
            RandomRangeIntCalls.Add((minInclusive, maxExclusive));
            return _intQueue.Dequeue();
        }

        public void QueueFloat(float value)
        {
            _floatQueue.Enqueue(value);
        }

        public void QueueInt(int value)
        {
            _intQueue.Enqueue(value);
        }
    }
}
