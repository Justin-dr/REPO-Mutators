using System.Collections.Generic;

namespace Mutators.Mutators
{
    public interface IMultiMutator : IMutator
    {
        IReadOnlyList<IMutator> SubMutators { get; }
    }
}