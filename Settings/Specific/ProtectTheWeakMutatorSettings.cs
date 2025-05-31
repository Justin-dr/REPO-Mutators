using BepInEx.Configuration;
using Photon.Pun;
using Photon.Realtime;

namespace Mutators.Settings.Specific
{
    public class ProtectTheWeakMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<byte> _minimumPlayerCount;
        public byte MinimumPlayerCount => _minimumPlayerCount.Value;
        internal ProtectTheWeakMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config)
        {
            _minimumPlayerCount = config.Bind<byte>(
            GetSection(name),
            "Minimum player amount requirement",
            3,
            $"The minimum amount of players required for the {name} Mutator to be available for selection."
            );
        }

        public override bool IsEligibleForSelection()
        {
            if (base.IsEligibleForSelection())
            {
                Room room = PhotonNetwork.CurrentRoom;
                return room != null && room.PlayerCount >= MinimumPlayerCount;
            }
            return false;
        }
    }
}
