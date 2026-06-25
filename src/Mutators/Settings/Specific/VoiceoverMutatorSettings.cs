using BepInEx.Configuration;
using Photon.Pun;
using Photon.Realtime;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Voiceover mutator.
    /// </summary>
    public class VoiceoverMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<int> _minimumPlayerCount;

        /// <summary>
        /// The minimum amount of players required for the Voiceover mutator to be eligible.
        /// </summary>
        public byte MinimumPlayerCount => (byte)_minimumPlayerCount.Value;
        internal VoiceoverMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config)
        {
            _minimumPlayerCount = config.Bind(
            GetSection(name),
            "Minimum player amount requirement",
            3,
            new ConfigDescription(
                $"The minimum amount of players required for the {name} Mutator to be available for selection.",
                new AcceptableValueRange<int>(0, 20))
            );
        }

        /// <inheritdoc cref="AbstractMutatorSettings.IsEligibleForSelection"/>
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
