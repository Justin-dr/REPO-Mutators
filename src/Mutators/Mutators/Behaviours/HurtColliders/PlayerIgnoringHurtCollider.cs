using System.Collections.Generic;

namespace Mutators.Mutators.Behaviours.HurtColliders
{
    internal class PlayerIgnoringHurtCollider : HurtCollider
    {
        internal readonly ISet<PlayerAvatar> ignoredPlayers = new HashSet<PlayerAvatar>();
    }
}
