using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.FullSerializer;

namespace Mutators
{
    public static class Settings
    {
        public static ConfigEntry<uint> NopMutatorWeight { get; private set; } = null!;
        public static ConfigEntry<uint> AppoloElevenWeight { get; private set; } = null!;
        public static ConfigEntry<uint> OutWithABangWeight { get; private set; } = null!;
        public static ConfigEntry<uint> DuckThisWeight { get; private set; } = null!;
        public static ConfigEntry<uint> UltraViolenceWeight { get; private set; } = null!;
        public static ConfigEntry<uint> ProtectThePresidentWeight { get; private set; } = null!;
        public static ConfigEntry<uint> OneShotOneKillWeight { get; private set; } = null!;
        public static ConfigEntry<uint> RustyServosWeight { get; private set; } = null!;
        public static ConfigEntry<uint> HandleWithCareWeight { get; private set; } = null!;
        public static ConfigEntry<float> HandleWithCareValueMultiplier { get; private set; } = null!;
        public static ConfigEntry<uint> HuntingSeasonWeight { get; private set; } = null!;
        public static void Initialize(ConfigFile config)
        {
            NopMutatorWeight = config.Bind<uint>(
            "No Mutator",
            "Weight",
            (uint)((Mutators.Mutators.All().Length - 1) * 100L),
            "Weighted chance for no mutator to be active."
            );

            AppoloElevenWeight = config.Bind<uint>(
            $"{Mutators.Mutators.ApolloEleven} Mutator",
            "Weight",
            100,
            $"Weighted chance for the {Mutators.Mutators.ApolloEleven} Mutator to be active."
            );

            OutWithABangWeight = config.Bind<uint>(
            $"{Mutators.Mutators.OutWithABang} Mutator",
            "Weight",
            100,
            $"Weighted chance for the {Mutators.Mutators.OutWithABang} Mutator to be active."
            );

            DuckThisWeight = config.Bind<uint>(
            $"{Mutators.Mutators.DuckThis} Mutator",
            "Weight",
            100,
            $"Weighted chance for the {Mutators.Mutators.DuckThis} Mutator to be active."
            );

            UltraViolenceWeight = config.Bind<uint>(
            $"{Mutators.Mutators.UltraViolence} Mutator",
            "Weight",
            100,
            $"Weighted chance for the {Mutators.Mutators.UltraViolence} Mutator to be active."
            );

            ProtectThePresidentWeight = config.Bind<uint>(
            $"{Mutators.Mutators.ProtectThePresident} Mutator",
            "Weight",
            100,
            $"Weighted chance for the {Mutators.Mutators.ProtectThePresident} Mutator to be active."
            );

            OneShotOneKillWeight = config.Bind<uint>(
            $"{Mutators.Mutators.OneShotOneKill} Mutator",
            "Weight",
            100,
            $"Weighted chance for the {Mutators.Mutators.OneShotOneKill} Mutator to be active."
            );

            RustyServosWeight = config.Bind<uint>(
            $"{Mutators.Mutators.RustyServos} Mutator",
            "Weight",
            100,
            $"Weighted chance for the {Mutators.Mutators.RustyServos} Mutator to be active."
            );

            HandleWithCareWeight = config.Bind<uint>(
            $"{Mutators.Mutators.HandleWithCare} Mutator",
            "Weight",
            100,
            $"Weighted chance for the {Mutators.Mutators.HandleWithCare} Mutator to be active."
            );

            HandleWithCareWeight = config.Bind<uint>(
            $"{Mutators.Mutators.HandleWithCare} Mutator",
            "Weight",
            100,
            $"Weighted chance for the {Mutators.Mutators.HandleWithCare} Mutator to be active."
            );

            HandleWithCareValueMultiplier = config.Bind<float>(
            $"{Mutators.Mutators.HandleWithCare} Mutator",
            "Value Multiplier",
            2,
            new ConfigDescription(
                $"The amount by which the value of valuables should be multiplier when {Mutators.Mutators.HandleWithCare} is active.",
                new AcceptableValueRange<float>(1f, 10f)
                )
            );

            HuntingSeasonWeight = config.Bind<uint>(
            $"{Mutators.Mutators.HuntingSeason} Mutator",
            "Weight",
            100,
            $"Weighted chance for the {Mutators.Mutators.HuntingSeason} Mutator to be active."
            );
        }
    }
}
