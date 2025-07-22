using System.Collections.Generic;
using BepInEx.Configuration;
using Mutators.Mutators.Patches;
using Photon.Pun;
using Photon.Realtime;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Ultra-Violence mutator.
    /// </summary>
    public class UltraViolenceMutatorSettings : EnemyDisablingMutatorSettings
    {
        private readonly ConfigEntry<bool> _keepOnLight;

        /// <summary>
        /// Whether the level lighting should stay on while the Ultra-Violence mutator is active.
        /// </summary>
        public bool KeepOnLight => _keepOnLight.Value;

        private readonly ConfigEntry<int> _minimumPlayerCount;

        /// <summary>
        /// The minimum amount of players required for the Ultra-Violence mutator to be eligible.
        /// </summary>
        public byte MinimumPlayerCount => (byte)_minimumPlayerCount.Value;

        internal UltraViolenceMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, 0, config)
        {
            _keepOnLight = config.Bind(
            GetSection(name),
            "Keep on lights",
            false,
            $"Keep on the level lighting while the {name} Mutator is active."
            );

            _minimumPlayerCount = config.Bind(
            GetSection(name),
            "Minimum player amount requirement",
            0,
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

        /// <inheritdoc cref="AbstractMutatorSettings.CreateMetadata"/>
        /// <returns>A dictionary holding <c>keep-lights-on</c></returns>
        protected override IDictionary<string, object>? CreateMetadata()
        {
            return new Dictionary<string, object>()
            {
                { UltraViolencePatch.KeepLightsOn, KeepOnLight }
            };
        }
    }
}
