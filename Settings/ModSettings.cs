using BepInEx.Configuration;
using System;
using UnityEngine;

namespace Mutators.Settings
{
    internal class ModSettings
    {
        internal enum MutatorNameToggleType {
            Keybind,
            WithDescription
        }

        // Mutator Name
        private readonly ConfigEntry<float> _mutatorDisplayY;
        private readonly ConfigEntry<float> _mutatorDisplaySize;
        private readonly ConfigEntry<string> _mutatorDisplayToggleKey;
        private readonly ConfigEntry<string> _mutatorDisplayToggleType;

        // Mutator Description
        private readonly ConfigEntry<float> _mutatorDescriptionDisplayY;
        private readonly ConfigEntry<float> _mutatorDescriptionDisplaySize;
        private readonly ConfigEntry<float> _mutatorDescriptionInitialDisplayTime;
        private readonly ConfigEntry<bool> _mutatorDescriptionPinned;
        private readonly ConfigEntry<bool> _mutatorDescriptionInMapTool;

        // Target
        private readonly ConfigEntry<float> _targetDisplaySize;

        // Special action
        private readonly ConfigEntry<float> _specialActionY;
        private readonly ConfigEntry<string> _specialActionKey;

        // Mutator Name
        public float MutatorDisplayY => _mutatorDisplayY.Value;
        public float MutatorDisplaySize => _mutatorDisplaySize.Value;
        public KeyCode MutatorDisplayToggleKey { get; private set; }
        public MutatorNameToggleType MutatorDisplayToggleType { get; private set; }

        // Mutator Description
        public float MutatorDescriptionDisplayY => _mutatorDescriptionDisplayY.Value;
        public float MutatorDescriptionDisplaySize => _mutatorDescriptionDisplaySize.Value;
        public float MutatorDescriptionInitialDisplayTime => _mutatorDescriptionInitialDisplayTime.Value;
        public bool MutatorDescriptionPinned => _mutatorDescriptionPinned.Value;
        public bool MutatorDescriptionInMapTool => _mutatorDescriptionInMapTool.Value;

        // Target
        public float TargetDisplaySize => _targetDisplaySize.Value;

        // Special Action
        public float SpecialActionY => _specialActionY.Value;
        public KeyCode SpecialActionKey { get; private set; }

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

            _mutatorDisplayToggleType = config.Bind<string>(
                    "Mutator Interface",
                    "Mutator display toggle type",
                    "Keybind",
                    new ConfigDescription(
                        "The method used to toggle the active Mutator overlay",
                        new AcceptableValueList<string>("Keybind", "With Description")
                    )
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

            // Special Action
            _specialActionY = config.Bind(
                    "Special Action",
                    "Special Action Y position",
                    -50f,
                    "The Y position of the Special Action overlay"
            );

            _specialActionKey = config.Bind<string>(
                    "Special Action",
                    "Special Action Key",
                    "R",
                    "Keybind that activates the Special Action"
            );

            CacheKeys();
        }

        internal void CacheKeys()
        {
            MutatorDisplayToggleKey = Enum.TryParse(typeof(KeyCode), _mutatorDisplayToggleKey.Value, out object toggle) ? (KeyCode)toggle : KeyCode.None;
            MutatorDisplayToggleType = Enum.TryParse(typeof(MutatorNameToggleType), _mutatorDisplayToggleType.Value.Replace(" ", ""), out object toggleType) ? (MutatorNameToggleType)toggleType : MutatorNameToggleType.Keybind;
            SpecialActionKey = Enum.TryParse(typeof(KeyCode), _specialActionKey.Value, out object specialAction) ? (KeyCode)specialAction : KeyCode.None;
        }
    }
}
