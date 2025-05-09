using System;
using System.Collections.Generic;
using System.Text;

namespace Mutators.Mutators
{
    public interface IMutator
    {
        void Patch();

        void Unpatch();

        string Name { get; }

        bool Active { get; }

        uint Weight { get; }

        IReadOnlyList<Type> Patches { get; }

        IReadOnlyList<Func<bool>> Conditions { get; }
    }
}
