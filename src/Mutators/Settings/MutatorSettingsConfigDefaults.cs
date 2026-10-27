namespace Mutators.Settings
{
    /// <summary>
    /// Configured default values for mutator settings.
    /// </summary>
    /// <param name="weight">The default weight that will be set in the config.</param>
    /// <param name="minimumLevel">The default minimum level that will be set in the config.</param>
    /// <param name="maximumLevel">The default maximum level that will be set in the config.</param>
    public sealed class MutatorSettingsConfigDefaults(int weight, int minimumLevel = MutatorSettingsConfigDefaults.MinimumLevelDefault, int maximumLevel = MutatorSettingsConfigDefaults.MaximumLevelDefault)
    {
        internal const int MinimumLevelDefault = 0;
        internal const int MaximumLevelDefault = 1000;
        
        /// <summary>
        /// <inheritdoc cref="AbstractMutatorSettings.Weight"/>
        /// </summary>
        public int Weight => weight;
        /// <summary>
        /// <inheritdoc cref="AbstractMutatorSettings.MinimumLevel"/>
        /// </summary>
        public int MinimumLevel => minimumLevel;
        /// <summary>
        /// <inheritdoc cref="AbstractMutatorSettings.MaximumLevel"/>
        /// </summary>
        public int MaximumLevel => maximumLevel;
    }
}