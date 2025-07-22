using UnityRandom = UnityEngine.Random;

namespace Mutators.Providers.Random
{
    /// <summary>
    /// <inheritdoc cref="IRandomProvider"/>
    /// </summary>
    internal class RandomProvider : IRandomProvider
    {
        public float Range(float minInclusive, float maxInclusive)
        {
            return UnityRandom.Range(minInclusive, maxInclusive);
        }

        public int RandomRangeInt(int minInclusive, int maxExclusive)
        {
            return UnityRandom.RandomRangeInt(minInclusive, maxExclusive);
        }
    }
}