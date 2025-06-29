using BepInEx.Configuration;
using Photon.Pun;
using Photon.Realtime;

namespace Mutators.Settings.Specific
{
    public class UltraViolenceMutatorSettings : EnemyDisablingMutatorSettings
    {
        private ConfigEntry<bool> _keepOnLight;
        public bool KeepOnLight => _keepOnLight.Value;

        private readonly ConfigEntry<byte> _minimumPlayerCount;
        public byte MinimumPlayerCount => _minimumPlayerCount.Value;

        internal UltraViolenceMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config, [])
        {
            _keepOnLight = config.Bind(
            GetSection(name),
            "Keep on lights",
            false,
            $"Keep on the level lighting while the {name} Mutator is active."
            );

            _minimumPlayerCount = config.Bind<byte>(
            GetSection(name),
            "Minimum player amount requirement",
            0,
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
