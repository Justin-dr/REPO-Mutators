using BepInEx.Configuration;
using Mutators.Settings.Specific;

namespace Mutators.Settings
{
    /// <summary>
    /// Static class that provides access to all available (default) Mutator settings.
    /// </summary>
    public static class MutatorSettings
    {
        /// <summary>
        /// Settings for the <see cref="Mutators.NopMutator"/>.
        /// </summary>
        public static NopMutatorSettings NopMutator { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="ApolloEleven"/> Mutator.
        /// </summary>
        public static ApolloElevenMutatorSettings ApolloEleven { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="OutWithABang"/> Mutator.
        /// </summary>
        public static OutWithABangMutatorSettings OutWithABang { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="DuckThis"/> Mutator.
        /// </summary>
        public static DuckThisMutatorSettings DuckThis { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="UltraViolence"/> Mutator.
        /// </summary>
        public static UltraViolenceMutatorSettings UltraViolence { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="ProtectThePresident"/> Mutator.
        /// </summary>
        public static ProtectThePresidentMutatorSettings ProtectThePresident { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="OneShotOneKill"/> Mutator.
        /// </summary>
        public static OneShotOneKillMutatorSettings OneShotOneKill { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="RustyServos"/> Mutator.
        /// </summary>
        public static GenericMutatorSettings RustyServos { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="HandleWithCare"/> Mutator.
        /// </summary>
        public static HandleWithCareMutatorSettings HandleWithCare { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="HuntingSeason"/> Mutator.
        /// </summary>
        public static EnemyDisablingMutatorSettings HuntingSeason { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="ThereCanOnlyBeOne"/> Mutator.
        /// </summary>
        public static ThereCanOnlyBeOneMutatorSettings ThereCanOnlyBeOne { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="VolatileCargo"/> Mutator.
        /// </summary>
        public static GenericMutatorSettings VolatileCargo { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="SealedAway"/> Mutator.
        /// </summary>
        public static SealedAwayMutatorSettings SealedAway { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="ProtectTheWeak"/> Mutator.
        /// </summary>
        public static GenericMutatorSettings ProtectTheWeak { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="FiringMyLaser"/> Mutator.
        /// </summary>
        public static FiringMyLaserMutatorSettings FiringMyLaser { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="Voiceover"/> Mutator.
        /// </summary>
        public static VoiceoverMutatorSettings Voiceover { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="TheFloorIsLava"/> Mutator.
        /// </summary>
        public static TheFloorIsLavaMutatorSettings TheFloorIsLava { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="LessIsMore"/> Mutator.
        /// </summary>
        public static LessIsMoreMutatorSettings LessIsMore { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="Amalgam"/> Mutator.
        /// </summary>
        public static AmalgamMutatorSettings Amalgam { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="NullSignal"/> Mutator.
        /// </summary>
        public static GenericMutatorSettings NullSignal { get; private set; } = null!;

        /// <summary>
        /// Settings for the <see cref="SizeMatters"/> Mutator.
        /// </summary>
        public static SizeMattersMutatorSettings SizeMatters { get; private set; } = null!;

        internal static void Initialize(ConfigFile config)
        {
            NopMutator = new NopMutatorSettings(config);
            ApolloEleven = new ApolloElevenMutatorSettings(Mutators.Mutators.ApolloElevenName, Mutators.Mutators.ApolloElevenDescription, config);
            OutWithABang = new OutWithABangMutatorSettings(Mutators.Mutators.OutWithABangName, Mutators.Mutators.OutWithABangDescription, config);
            DuckThis = new DuckThisMutatorSettings(Mutators.Mutators.DuckThisName, Mutators.Mutators.DuckThisDescription, config);
            UltraViolence = new UltraViolenceMutatorSettings(Mutators.Mutators.UltraViolenceName, Mutators.Mutators.UltraViolenceDescription, config);
            ProtectThePresident = new ProtectThePresidentMutatorSettings(Mutators.Mutators.ProtectThePresidentName, Mutators.Mutators.ProtectThePresidentDescription, config);
            OneShotOneKill = new OneShotOneKillMutatorSettings(Mutators.Mutators.OneShotOneKillName, Mutators.Mutators.OneShotOneKillDescription, config);
            RustyServos = new GenericMutatorSettings(MyPluginInfo.PLUGIN_GUID, Mutators.Mutators.RustyServosName, Mutators.Mutators.RustyServosDescription, config);
            HandleWithCare = new HandleWithCareMutatorSettings(Mutators.Mutators.HandleWithCareName, Mutators.Mutators.HandleWithCareDescription, config);
            HuntingSeason = new EnemyDisablingMutatorSettings(MyPluginInfo.PLUGIN_GUID, Mutators.Mutators.HuntingSeasonName, Mutators.Mutators.HuntingSeasonDescription, config, "Voodoo", "Weeping Angel");
            ThereCanOnlyBeOne = new ThereCanOnlyBeOneMutatorSettings(Mutators.Mutators.ThereCanOnlyBeOneName, Mutators.Mutators.ThereCanOnlyBeOneDescription, config);
            VolatileCargo = new GenericMutatorSettings(MyPluginInfo.PLUGIN_GUID, Mutators.Mutators.VolatileCargoName, Mutators.Mutators.VolatileCargoDescription, config);
            SealedAway = new SealedAwayMutatorSettings(Mutators.Mutators.SealedAwayName, Mutators.Mutators.SealedAwayDescription, config);
            ProtectTheWeak = new ProtectTheWeakMutatorSettings(Mutators.Mutators.ProtectTheWeakName, Mutators.Mutators.ProtectTheWeakDescription, config);
            FiringMyLaser = new FiringMyLaserMutatorSettings(Mutators.Mutators.FiringMyLaserName, Mutators.Mutators.FiringMyLaserDescription, config);
            Voiceover = new VoiceoverMutatorSettings(Mutators.Mutators.VoiceoverName, Mutators.Mutators.VoiceoverDescription, config);
            TheFloorIsLava = new TheFloorIsLavaMutatorSettings(Mutators.Mutators.TheFloorIsLavaName, Mutators.Mutators.TheFloorIsLavaDescription, config);
            LessIsMore = new LessIsMoreMutatorSettings(Mutators.Mutators.LessIsMoreName, Mutators.Mutators.LessIsMoreDescription, config);
            Amalgam = new AmalgamMutatorSettings(Mutators.Mutators.AmalgamName, Mutators.Mutators.AmalgamDescription, config);
            NullSignal = new GenericMutatorSettings(MyPluginInfo.PLUGIN_GUID, Mutators.Mutators.NullSignalName, Mutators.Mutators.NullSignalDescription, config);
            SizeMatters = new SizeMattersMutatorSettings(Mutators.Mutators.SizeMattersName, Mutators.Mutators.SizeMattersDescription, config);
        }
    }
}
