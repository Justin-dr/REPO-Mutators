using BepInEx.Configuration;
using System;
using UnityEngine;

namespace Mutators.Settings
{
    internal class ModSettings
    {
        private readonly ConfigEntry<float> _mutatorDisplayY;
        private readonly ConfigEntry<float> _mutatorDisplaySize;
        private readonly ConfigEntry<string> _mutatorDisplayToggleKey;
        private readonly ConfigEntry<float> _targetDisplaySize;
        public float MutatorDisplayY => _mutatorDisplayY.Value;
        public float MutatorDisplaySize => _mutatorDisplaySize.Value;
        public float TargetDisplaySize => _targetDisplaySize.Value;
        public KeyCode MutatorDisplayToggleKey { get; private set; }
        internal ModSettings(ConfigFile config)
        {
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

            _targetDisplaySize = config.Bind(
                    "Mutator Interface",
                    "Target Size",
                    40f,
                    "The size of the target (e.g. president health) overlay"
            );

            _mutatorDisplayToggleKey = config.Bind(
                    "Mutator Interface",
                    "Mutator display toggle key",
                    "H",
                    "The key use to toggle the active Mutator overlay"
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
