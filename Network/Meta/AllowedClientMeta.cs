using System.Collections.Generic;

namespace Mutators.Network.Meta
{
    public class AllowedClientMeta : IAllowedClientMeta
    {
        public bool PurgeAfterAccepting { get; private set; }

        public IDictionary<string, IAllowedClientMeta> NestedMeta { get; private set; }

        public AllowedClientMeta(IDictionary<string, IAllowedClientMeta> meta, bool purgeAfterAccepting = false)
        {
            PurgeAfterAccepting = purgeAfterAccepting;
            NestedMeta = meta;
        }

        public AllowedClientMeta(bool purgeAfterAccepting = false)
        {
            PurgeAfterAccepting = purgeAfterAccepting;
            NestedMeta = new Dictionary<string, IAllowedClientMeta>();
        }
    }
}
