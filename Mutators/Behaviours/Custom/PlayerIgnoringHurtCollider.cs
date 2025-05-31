using System.Collections.Generic;

namespace Mutators.Mutators.Behaviours.Custom
{
    internal class PlayerIgnoringHurtCollider : HurtCollider
    {
        internal readonly ISet<PlayerAvatar> ignoredPlayers = new HashSet<PlayerAvatar>();
    }
}
