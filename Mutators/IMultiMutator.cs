using System.Collections.Generic;

namespace Mutators.Mutators
{
    public interface IMultiMutator : IMutator
    {
        IReadOnlyDictionary<IMutator, IDictionary<string, object>> SubMutators { get; }
    }
}