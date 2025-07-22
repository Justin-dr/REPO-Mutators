using System;
using System.Collections.Generic;
using System.Linq;
using Mutators.Mutators;

namespace Mutators.Rules
{
    /// <summary>
    /// Premade rules for <see cref="IMultiMutator"/>s.
    /// </summary>
    public static class MultiMutatorRules
    {
        /// <summary>
        /// Helper method for creating a rule that requires a specific amount of other mutators to have been picked
        /// before the candidate mutator can be picked.
        /// </summary>
        /// <param name="namespacedName">The unique identifier of the affected mutator.</param>
        /// <param name="amount">The amount of other mutators that need to be active.</param>
        /// <returns>The created rule.</returns>
        public static Func<IReadOnlyCollection<string>, string, bool> RequiresAmountOfOtherMutatorsRule(string namespacedName, int amount)
        {
            return (pickedList, candidate) => namespacedName != candidate || pickedList.Count >= amount;
        }
        
        /// <summary>
        /// Helper method for creating a rule that makes the two provided mutators mutually exclusive.
        /// </summary>
        /// <param name="firstNamespacedName">The namespacedName of the first mutator to be mutually exclusive.</param>
        /// <param name="secondNamespacedName">The namespacedName of the second mutator to be mutually exclusive.</param>
        /// <returns>The created rule.</returns>
        public static Func<IReadOnlyCollection<string>, string, bool> MutualExclusionRule(string firstNamespacedName, string secondNamespacedName)
        {
            return (pickedList, candidate) => !(pickedList.Contains(firstNamespacedName) && string.Equals(candidate, secondNamespacedName, StringComparison.OrdinalIgnoreCase))
                   && !(pickedList.Contains(secondNamespacedName) && string.Equals(candidate, firstNamespacedName, StringComparison.OrdinalIgnoreCase));
        }
    }
}