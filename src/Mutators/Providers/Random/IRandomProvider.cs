namespace Mutators.Providers.Random
{
    /// <summary>
    /// Wrapper around UnityEngine.Random to keep UnityEngine references out of the MutatorManager.
    /// </summary>
    internal interface IRandomProvider
    {
        float Range(float minInclusive, float maxInclusive);
        int RandomRangeInt(int minInclusive, int maxExclusive);
    }
}
