using System.Collections.Generic;

namespace Mutators.Mutators
{
    public interface IMultiMutator : IMutator
    {
        bool IsCustom { get; }
        IReadOnlyDictionary<IMutator, IDictionary<string, object>> SubMutators { get; }
    }
}