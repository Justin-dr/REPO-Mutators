using BepInEx.Configuration;
using Mutators.Settings.Specific;

namespace Mutators.Settings
{
    public static class MutatorSettings
    {
        public static NopMutatorSettings NopMutator { get; private set; } = null!;
        public static ApolloElevenMutatorSettings ApolloEleven { get; private set; } = null!;
        public static GenericMutatorSettings OutWithABang { get; private set; } = null!;
        public static DuckThisMutatorSettings DuckThis { get; private set; } = null!;
        public static GenericMutatorSettings UltraViolence { get; private set; } = null!;
        public static ProtectThePresidentMutatorSettings ProtectThePresident { get; private set; } = null!;
        public static GenericMutatorSettings OneShotOneKill { get; private set; } = null!;
        public static GenericMutatorSettings RustyServos { get; private set; } = null!;
        public static HandleWithCareMutatorSettings HandleWithCare { get; private set; } = null!;
        public static GenericMutatorSettings HuntingSeason { get; private set; } = null!;
        public static ThereCanOnlyBeOneMutatorSettings ThereCanOnlyBeOne { get; private set; } = null!;
        public static GenericMutatorSettings VolatileCargo { get; private set; } = null!;
        public static void Initialize(ConfigFile config)
        {
            NopMutator = new NopMutatorSettings(config);
            ApolloEleven = new ApolloElevenMutatorSettings(Mutators.Mutators.ApolloEleven, config);
            OutWithABang = new GenericMutatorSettings(Mutators.Mutators.OutWithABang, config);
            DuckThis = new DuckThisMutatorSettings(Mutators.Mutators.DuckThis, config);
            UltraViolence = new GenericMutatorSettings(Mutators.Mutators.UltraViolence, config);
            ProtectThePresident = new ProtectThePresidentMutatorSettings(Mutators.Mutators.ProtectThePresident, config);
            OneShotOneKill = new GenericMutatorSettings(Mutators.Mutators.OneShotOneKill, config);
            RustyServos = new GenericMutatorSettings(Mutators.Mutators.RustyServos, config);
            HandleWithCare = new HandleWithCareMutatorSettings(Mutators.Mutators.HandleWithCare, config);
            HuntingSeason = new GenericMutatorSettings(Mutators.Mutators.HuntingSeason, config);
            ThereCanOnlyBeOne = new ThereCanOnlyBeOneMutatorSettings(Mutators.Mutators.ThereCanOnlyBeOne, config);
            VolatileCargo = new GenericMutatorSettings(Mutators.Mutators.VolatileCargo, config);
        }
    }
}
