using Mutators.Mutators;

namespace Mutators.Rules
{
    /// <summary>
    /// Default rule types for generated <see cref="IMultiMutator"/>s.
    /// </summary>
    public static class MultiMutatorRuleType
    {
        public const string Exclusion = SingleMutatorRuleType.Exclusion;
        public const string MutualExclusion = "Mutual Exclusion";
        public const string RequiresAmountOfOtherMutators = "Requires Amount Of Other Mutators";
    }
}