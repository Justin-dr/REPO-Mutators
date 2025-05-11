using Mutators.Settings;
using System;
using System.Collections.Generic;

namespace Mutators.Mutators
{
    public interface IMutator
    {
        void Patch();

        void Unpatch();

        string Name { get; }

        bool Active { get; }

        AbstractMutatorSettings Settings { get; }

        IReadOnlyList<Type> Patches { get; }

        IReadOnlyList<Func<bool>> Conditions { get; }
    }
}
