using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Mutators.Utility.Config;
using UnityEngine;

namespace Mutators.Settings
{
    internal class ModSettings
    {
        internal static readonly byte MaximumGeneratedActiveSubMutators = 10;
        
        internal enum MutatorNameToggleType
        {
            Keybind,
            WithDescription
        }

        internal enum MultiMutatorScalingType
        {
            None,
            Random,
            Moon
        }

        internal sealed class MoonSetting(ConfigRange<int> multiMutatorMoonRange, ConfigEntry<int> generatedChance)
        {
            private ConfigRange<int> MultiMutatorMoonRange => multiMutatorMoonRange;

            internal int GeneratedChance => generatedChance.Value;
            
            internal int MaximumMutators => MultiMutatorMoonRange.MaximumValue;
            internal int MinimumMutators => MultiMutatorMoonRange.MinimumValue;
        }

        internal sealed class MoonSettings
        {
            private readonly IDictionary<byte, MoonSetting> _multiMutatorMoonRanges = new Dictionary<byte, MoonSetting>();

            internal void LateBindMoonConfig(ConfigFile config)
            {
                int moonCount = RunManager.instance.moons.Count;
                for (byte i = 0; i <= moonCount; i++)
                {
                    ConfigEntry<int> generatedChance = config.Bind(
                        GetMoonSection(i),
                        "Generated Multi-Mutator Chance (%)",
                        50,
                        new ConfigDescription(
                            $"The chance that a Generated Multi-Mutator will occur when the moon level is {i}",
                            new AcceptableValueRange<int>(0, 100))
                    );
                
                    ConfigRange<int> range = new(CreateMoonRange(config, i, true), CreateMoonRange(config, i, false));
                    _multiMutatorMoonRanges[i] = new MoonSetting(range, generatedChance);
                }
                RepoMutators.Logger.LogDebug($"Late-bound moon config with {moonCount} moons.");
            }

            internal MoonSetting GetMultiMutatorMoonRange(int moon) => GetMultiMutatorMoonRange((byte)moon);
            internal MoonSetting GetMultiMutatorMoonRange(byte moon) => _multiMutatorMoonRanges[moon];
        }

        internal sealed class RandomSettings
        {
            private const string ConfigSection = "Multi-Mutators - Random";
            
            private readonly ConfigEntry<int> _minimumAmount;
            private readonly ConfigEntry<int> _maximumAmount;
            private readonly IDictionary<byte, ConfigEntry<int>> _weight = new Dictionary<byte, ConfigEntry<int>>(MaximumGeneratedActiveSubMutators);
            private readonly IDictionary<byte, ConfigEntry<int>> _generatedChance = new Dictionary<byte, ConfigEntry<int>>(MaximumGeneratedActiveSubMutators);
            
            internal RandomSettings(ConfigFile config)
            {
                _minimumAmount = config.Bind(
                    ConfigSection,
                    "Minimum amount of Mutators",
                    1,
                    new ConfigDescription(
                        "The minimum amount of Mutators that can be selected when using random scaling.",
                        new AcceptableValueRange<int>(1, MaximumGeneratedActiveSubMutators)
                    )
                );
                
                _maximumAmount = config.Bind(
                    ConfigSection,
                    "Maximum amount of Mutators",
                    4,
                    new ConfigDescription(
                        "The maximum amount of Mutators that can be selected when using random scaling.",
                        new AcceptableValueRange<int>(1, MaximumGeneratedActiveSubMutators)
                    )
                );
                
                for (byte i = 1; i <= MaximumGeneratedActiveSubMutators; i++)
                {
                    _weight[i] = config.Bind(
                        ConfigSection,
                        $"{i} Mutators - Weight",
                        50,
                        new ConfigDescription(
                            $"The weighted chance for {i} {(i == 1 ? "Mutator" : "Mutators")} to be selected when using random scaling.",
                            new AcceptableValueRange<int>(0, 1000)
                        )
                    );
                    
                    if (i <= 1) continue;
                    
                    _generatedChance[i] = config.Bind(
                        ConfigSection,
                        $"{i} Mutators - Generated Multi-Mutator Chance (%)",
                        50,
                        new ConfigDescription(
                            $"The chance that a Generated Multi-Mutator will occur when picking {i} Mutators.",
                            new AcceptableValueRange<int>(0, 100)
                        )
                    );
                }
            }
            internal int MinimumAmount => _minimumAmount.Value;
            internal int MaximumAmount => _maximumAmount.Value;

            internal int GetWeight(int amountOfMutators)
            {
                if (amountOfMutators < 0 || amountOfMutators > MaximumGeneratedActiveSubMutators)
                {
                    throw new ArgumentException(
                        $"Amount of Mutators should be between 0 and {MaximumGeneratedActiveSubMutators}",
                        nameof(amountOfMutators)
                    );
                }
                
                return _weight[(byte)amountOfMutators].Value;
            }

            internal int GetGeneratedChance(int amountOfMutators)
            {
                if (amountOfMutators < 2)
                {
                    return 0;
                }

                if (amountOfMutators <= MaximumGeneratedActiveSubMutators)
                {
                    return _generatedChance[(byte)amountOfMutators].Value;
                }

                throw new ArgumentException(
                    $"Amount of Mutators should not be greater than {MaximumGeneratedActiveSubMutators}",
                    nameof(amountOfMutators)
                );
            }
        }
        
        //Logging
        private readonly ConfigEntry<bool> _extendedLogging;

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
        
        // Multi
        private readonly ConfigEntry<string> _multiMutatorSelectionMode;
        
        public RandomSettings RandomMutatorSettings { get; }
        public MoonSettings MoonMutatorSettings { get; }

        // Logging
        public bool ExtendedLogging => _extendedLogging.Value;
        
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

        // MultiMutator Selection
        public MultiMutatorScalingType MutatorScalingType { get; private set; }

        internal ModSettings(ConfigFile config)
        {
            _extendedLogging = config.Bind(
                "Logging",
                "Extended Logging",
                false,
                "Enable extended logging."
            );
            
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
            
            _multiMutatorSelectionMode = config.Bind<string>(
                "Multi-Mutators",
                "Scaling Mode",
                "Moon",
                new ConfigDescription(
                    "The method used to scale the amount of mutators. None = No scaling, Random = Random scaling, Moon = Scaling based on moon",
                    new AcceptableValueList<string>("None", "Random", "Moon")
                )
            );
            
            RandomMutatorSettings = new RandomSettings(config);
            MoonMutatorSettings = new MoonSettings();
            
            CacheKeys();
            config.SettingChanged += (_, _) => CacheKeys();
        }

        internal void LateBindMoonConfig(ConfigFile config)
        {
            MoonMutatorSettings.LateBindMoonConfig(config);
        }

        internal void CacheKeys()
        {
            MutatorDisplayToggleKey = Enum.TryParse(typeof(KeyCode), _mutatorDisplayToggleKey.Value, out object toggle) ? (KeyCode)toggle : KeyCode.None;
            MutatorDisplayToggleType = Enum.TryParse(typeof(MutatorNameToggleType), _mutatorDisplayToggleType.Value.Replace(" ", ""), out object toggleType) ? (MutatorNameToggleType)toggleType : MutatorNameToggleType.Keybind;
            SpecialActionKey = Enum.TryParse(typeof(KeyCode), _specialActionKey.Value, out object specialAction) ? (KeyCode)specialAction : KeyCode.None;
            MutatorScalingType = Enum.TryParse(typeof(MultiMutatorScalingType), _multiMutatorSelectionMode.Value, out object selectionMode) ? (MultiMutatorScalingType)selectionMode : MultiMutatorScalingType.None;
        }

        private static ConfigEntry<int> CreateMoonRange(ConfigFile config, byte moon, bool min)
        {
            return config.Bind(
                GetMoonSection(moon),
                min ? "Minimum Mutators" : "Maximum Mutators",
                GetMoonDefaultValue(moon, min),
                new ConfigDescription(
                    $"The {(min ? "minimum" : "maximum")} amount of mutators that can be selected for moon {moon}",
                    new AcceptableValueRange<int>(0, MaximumGeneratedActiveSubMutators)
                )
            );
        }

        private static int GetMoonDefaultValue(int moon, bool min)
        {
            return min ? Math.Max(moon, 1) : Math.Min(4, moon + 1);
        }

        private static string GetMoonSection(byte moon) => "Multi-Mutators - " + (moon == 0 ? "No Moon" : $"Moon {moon}");
    }
}
