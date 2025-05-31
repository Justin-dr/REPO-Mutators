using System;
using System.Collections.Generic;
using System.Text;

namespace Mutators.Network.Meta
{
    public interface IAllowedClientMeta
    {
        public bool PurgeAfterAccepting { get; }
        public IDictionary<string, IAllowedClientMeta> NestedMeta { get; }

        bool HasNested()
        {
            return NestedMeta != null && NestedMeta.Count > 0;
        }
    }
}
