using BepInEx.Configuration;
using System;
using UnityEngine;

namespace Mutators.Settings
{
    internal class ModSettings
    {
        // Mutator Name
        private readonly ConfigEntry<float> _mutatorDisplayY;
        private readonly ConfigEntry<float> _mutatorDisplaySize;
        private readonly ConfigEntry<string> _mutatorDisplayToggleKey;

        // Mutator Description
        private readonly ConfigEntry<float> _mutatorDescriptionDisplayY;
        private readonly ConfigEntry<float> _mutatorDescriptionDisplaySize;
        private readonly ConfigEntry<float> _mutatorDescriptionInitialDisplayTime;
        private readonly ConfigEntry<bool> _mutatorDescriptionPinned;
        private readonly ConfigEntry<bool> _mutatorDescriptionInMapTool;

        // Target
        private readonly ConfigEntry<float> _targetDisplaySize;

        // Mutator Name
        public float MutatorDisplayY => _mutatorDisplayY.Value;
        public float MutatorDisplaySize => _mutatorDisplaySize.Value;
        public KeyCode MutatorDisplayToggleKey { get; private set; }

        // Mutator Description
        public float MutatorDescriptionDisplayY => _mutatorDescriptionDisplayY.Value;
        public float MutatorDescriptionDisplaySize => _mutatorDescriptionDisplaySize.Value;
        public float MutatorDescriptionInitialDisplayTime => _mutatorDescriptionInitialDisplayTime.Value;
        public bool MutatorDescriptionPinned => _mutatorDescriptionPinned.Value;
        public bool MutatorDescriptionInMapTool => _mutatorDescriptionInMapTool.Value;

        // Target
        public float TargetDisplaySize => _targetDisplaySize.Value;

        internal ModSettings(ConfigFile config)
        {
            // Mutator Name
            _mutatorDisplayY = config.Bind(
                    "Mutator Interface",
                    "Y position",
                    -75f,
                    "The Y position of the active Mutator overlay"
            );

            _mutatorDisplaySize = config.Bind(
                    "Mutator Interface",
                    "Size",
                    30f,
                    "The size of the active Mutator overlay"
            );

            _mutatorDisplayToggleKey = config.Bind(
                    "Mutator Interface",
                    "Mutator display toggle key",
                    "H",
                    "The key use to toggle the active Mutator overlay"
            );

            // Mutator Description
            _mutatorDescriptionDisplayY = config.Bind(
                    "Mutator Interface",
                    "Description Y position",
                    -110f,
                    "The Y position of the active Mutator's description overlay"
            );

            _mutatorDescriptionDisplaySize = config.Bind(
                    "Mutator Interface",
                    "Description size",
                    20f,
                    "The size of the active Mutator's description overlay"
            );

            _mutatorDescriptionInitialDisplayTime = config.Bind(
                    "Mutator Interface",
                    "Description initial display time",
                    8f,
                    "The time for which the mutator's description is on screen when starting the level"
            );

            _mutatorDescriptionPinned = config.Bind(
                    "Mutator Interface",
                    "Description always active",
                    false,
                    "If true, \"Description initial display time\" will be ignored and the description will always stay on screen"
            );

            _mutatorDescriptionInMapTool = config.Bind(
                    "Mutator Interface",
                    "Description active in map tool",
                    true,
                    "If true, the mutator's description will be displayed when opening the map tool"
            );

            // Target
            _targetDisplaySize = config.Bind(
                    "Mutator Interface",
                    "Target Size",
                    40f,
                    "The size of the target (e.g. president health) overlay"
            );

            CacheKey();
        }

        internal void CacheKey()
        {
            if (Enum.TryParse(typeof(KeyCode), _mutatorDisplayToggleKey.Value, out object result))
            {
                MutatorDisplayToggleKey = (KeyCode)result;
                return;
            }
            MutatorDisplayToggleKey = KeyCode.None;
        }
    }
}
