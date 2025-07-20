using BepInEx.Configuration;
using Mutators.Settings.Specific;

namespace Mutators.Settings
{
    public static class MutatorSettings
    {
        public static NopMutatorSettings NopMutator { get; private set; } = null!;
        public static ApolloElevenMutatorSettings ApolloEleven { get; private set; } = null!;
        public static OutWithABangMutatorSettings OutWithABang { get; private set; } = null!;
        public static DuckThisMutatorSettings DuckThis { get; private set; } = null!;
        public static UltraViolenceMutatorSettings UltraViolence { get; private set; } = null!;
        public static ProtectThePresidentMutatorSettings ProtectThePresident { get; private set; } = null!;
        public static OneShotOneKillMutatorSettings OneShotOneKill { get; private set; } = null!;
        public static GenericMutatorSettings RustyServos { get; private set; } = null!;
        public static HandleWithCareMutatorSettings HandleWithCare { get; private set; } = null!;
        public static EnemyDisablingMutatorSettings HuntingSeason { get; private set; } = null!;
        public static ThereCanOnlyBeOneMutatorSettings ThereCanOnlyBeOne { get; private set; } = null!;
        public static GenericMutatorSettings VolatileCargo { get; private set; } = null!;
        public static SealedAwayMutatorSettings SealedAway { get; private set; } = null!;
        public static GenericMutatorSettings ProtectTheWeak { get; private set; } = null!;
        public static FiringMyLaserMutatorSettings FiringMyLaser { get; private set; } = null!;
        public static VoiceoverMutatorSettings Voiceover { get; private set; } = null!;
        public static TheFloorIsLavaMutatorSettings TheFloorIsLava { get; private set; } = null!;
        public static LessIsMoreMutatorSettings LessIsMore { get; private set; } = null!;
        //public static GenericMutatorSettings FragmentationProtocol { get; private set; } = null!;
        public static AmalgamMutatorSettings Amalgam { get; private set; } = null!;
        public static void Initialize(ConfigFile config)
        {
            NopMutator = new NopMutatorSettings(config);
            ApolloEleven = new ApolloElevenMutatorSettings(Mutators.Mutators.ApolloElevenName, Mutators.Mutators.ApolloElevenDescription, config);
            OutWithABang = new OutWithABangMutatorSettings(Mutators.Mutators.OutWithABangName, Mutators.Mutators.OutWithABangDescription, config);
            DuckThis = new DuckThisMutatorSettings(Mutators.Mutators.DuckThisName, Mutators.Mutators.DuckThisDescription, config);
            UltraViolence = new UltraViolenceMutatorSettings(Mutators.Mutators.UltraViolenceName, Mutators.Mutators.UltraViolenceDescription, config);
            ProtectThePresident = new ProtectThePresidentMutatorSettings(Mutators.Mutators.ProtectThePresidentName, Mutators.Mutators.ProtectThePresidentDescription, config);
            OneShotOneKill = new OneShotOneKillMutatorSettings(Mutators.Mutators.OneShotOneKillName, Mutators.Mutators.OneShotOneKillDescription, config);
            RustyServos = new GenericMutatorSettings(Mutators.Mutators.RustyServosName, Mutators.Mutators.RustyServosDescription, config);
            HandleWithCare = new HandleWithCareMutatorSettings(Mutators.Mutators.HandleWithCareName, Mutators.Mutators.HandleWithCareDescription, config);
            HuntingSeason = new EnemyDisablingMutatorSettings(Mutators.Mutators.HuntingSeasonName, Mutators.Mutators.HuntingSeasonDescription, config, "Voodoo", "Weeping Angel");
            ThereCanOnlyBeOne = new ThereCanOnlyBeOneMutatorSettings(Mutators.Mutators.ThereCanOnlyBeOneName, Mutators.Mutators.ThereCanOnlyBeOneDescription, config);
            VolatileCargo = new GenericMutatorSettings(Mutators.Mutators.VolatileCargoName, Mutators.Mutators.VolatileCargoDescription, config);
            SealedAway = new SealedAwayMutatorSettings(Mutators.Mutators.SealedAwayName, Mutators.Mutators.SealedAwayDescription, config);
            ProtectTheWeak = new ProtectTheWeakMutatorSettings(Mutators.Mutators.ProtectTheWeakName, Mutators.Mutators.ProtectTheWeakDescription, config);
            FiringMyLaser = new FiringMyLaserMutatorSettings(Mutators.Mutators.FiringMyLaserName, Mutators.Mutators.FiringMyLaserDescription, config);
            Voiceover = new VoiceoverMutatorSettings(Mutators.Mutators.VoiceoverName, Mutators.Mutators.VoiceoverDescription, config);
            TheFloorIsLava = new TheFloorIsLavaMutatorSettings(Mutators.Mutators.TheFloorIsLavaName, Mutators.Mutators.TheFloorIsLavaDescription, config);
            LessIsMore = new LessIsMoreMutatorSettings(Mutators.Mutators.LessIsMoreName, Mutators.Mutators.LessIsMoreDescription, config);
            //FragmentationProtocol = new GenericMutatorSettings(Mutators.Mutators.FragmentationProtocolName, Mutators.Mutators.LessIsMoreDescription, config);
            Amalgam = new AmalgamMutatorSettings(Mutators.Mutators.AmalgamName, Mutators.Mutators.AmalgamDescription, config);
        }
    }
}
