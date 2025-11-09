namespace Mutators.Mutators
{
    public static class Mutators
    {
        // Names
        public const string NopMutatorName = "None";
        public const string OutWithABangName = "Out With a Bang!";
        public const string ApolloElevenName = "Apollo 11";
        public const string UltraViolenceName = "Ultra-Violence";
        public const string DuckThisName = "Duck This";
        public const string ProtectThePresidentName = "Protect the President";
        public const string OneShotOneKillName = "One Shot, One Kill";
        public const string RustyServosName = "Rusty Servos";
        public const string HandleWithCareName = "Handle With Care";
        public const string HuntingSeasonName = "Hunting Season";
        public const string ThereCanOnlyBeOneName = "There Can Only Be One";
        public const string VolatileCargoName = "Volatile Cargo";
        public const string SealedAwayName = "Sealed Away";
        public const string ProtectTheWeakName = "Protect the Weak";
        public const string FiringMyLaserName = "Firing My Laser";
        public const string VoiceoverName = "Voiceover";
        public const string TheFloorIsLavaName = "The Floor Is Lava";
        public const string LessIsMoreName = "Less Is More";
        public const string FragmentationProtocolName = "Fragmentation Protocol";
        public const string AmalgamName = "Amalgam";

        // Descriptions
        public const string NopMutatorDescription = "A normal run, no special effects";
        public const string OutWithABangDescription = "Monsters explode on death";
        public const string ApolloElevenDescription = "Level-wide Zero-Gravity";
        public const string UltraViolenceDescription = "Immediately activates the final extraction phase";
        public const string DuckThisDescription = "Ducks aggro on sight instead of on interaction\nAlways spawn at least 1 duck";
        public const string ProtectThePresidentDescription = "A random player becomes the \"President\"\nIf they die, everyone else self-destructs";
        public const string OneShotOneKillDescription = "Any damage taken by a player is lethal";
        public const string RustyServosDescription = "Players cannot jump\n+3 Grab Range";
        public const string HandleWithCareDescription = "Valuables are worth more but break on any impact";
        public const string HuntingSeasonDescription = "No valuables spawn, weapons spawn instead\nEnemy respawn time reduced to 10 seconds";
        public const string ThereCanOnlyBeOneDescription = "All monster spawns are of the same type";
        public const string VolatileCargoDescription = "Valuables explode on destruction\nExplosion radius and strength based on value";
        public const string SealedAwayDescription = "Breaking valuables has a chance to spawn monsters";
        public const string ProtectTheWeakDescription = "Protect your weaker friends!";
        public const string FiringMyLaserDescription = "Fire your laser by pressing {specialActionKey}\nUncontrollably fire your laser when taking damage";
        public const string VoiceoverDescription = "Player voices are shuffled";
        public const string TheFloorIsLavaDescription = "You take damage while standing on the floor";
        public const string LessIsMoreDescription = "Valuables are worth less but gain value when hit\nNormal breaking mechanics apply";
        public const string AmalgamDescription = "You’ve been here before — just not all at once";

        internal static string[] All() => [
            NopMutatorName,
            OutWithABangName,
            ApolloElevenName,
            UltraViolenceName,
            DuckThisName,
            ProtectThePresidentName,
            RustyServosName,
            HandleWithCareName,
            HuntingSeasonName,
            ThereCanOnlyBeOneName,
            VolatileCargoName,
            SealedAwayName,
            ProtectTheWeakName,
            FiringMyLaserName,
            VoiceoverName,
            TheFloorIsLavaName,
            LessIsMoreName,
            // FragmentationProtocolName
            AmalgamName
        ];
    }
}
