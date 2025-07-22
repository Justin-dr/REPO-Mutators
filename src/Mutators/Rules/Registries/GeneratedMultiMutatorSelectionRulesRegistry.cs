using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mutators.Mutators;

namespace Mutators.Rules.Registries
{
    /// <summary>
    /// Registry for generated <see cref="IMultiMutator"/> selection rules.
    /// <remarks>
    /// User-defined multi-mutators are not subject to these selection rules.
    /// </remarks>
    /// </summary>
    // It doesn't fit on my screen
    public class GeneratedMultiMutatorSelectionRulesRegistry : MutatorSelectionRulesRegistry<Func<IReadOnlyCollection<string>, string, bool>, Func<IReadOnlyCollection<IMutator>, IMutator, bool>>
    {
        internal GeneratedMultiMutatorSelectionRulesRegistry()
        {
            
        }
        
        internal bool RunRules(ICollection<IMutator> pickedMutators, IMutator mutator)
        {
            ReadOnlyCollection<string> pickedMutatorNamesReadOnly = pickedMutators
                .Select(m => m.NamespacedName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
            
            IReadOnlyCollection<IMutator> pickedMutatorsReadonly = new ReadOnlyCollection<IMutator>(pickedMutators.ToList());
            
            return mutatorSelectionRulesExtended.Values.All(rule => rule(pickedMutatorsReadonly, mutator)) &&
                   mutatorSelectionRules.Values.All(rule => rule(pickedMutatorNamesReadOnly, mutator.NamespacedName));
        }
    }
}
