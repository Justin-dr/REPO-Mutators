using BepInEx.Configuration;
using Mutators.Extensions;

namespace Mutators.Settings
{
    /// <summary>
    /// Default implementation of <see cref="AbstractMutatorSettings"/> that provides common settings for all Mutators.
    /// </summary>
    public class GenericMutatorSettings : AbstractMutatorSettings
    {
        private readonly ConfigEntry<int> _weight;
        private readonly ConfigEntry<int> _minimumLevel;
        private readonly ConfigEntry<int> _maximumLevel;
        
        /// <summary>
        /// <inheritdoc cref="AbstractMutatorSettings.Weight"/>
        /// </summary>
        public override int Weight => _weight.Value;
        
        /// <summary>
        /// <inheritdoc cref="AbstractMutatorSettings.MinimumLevel"/>
        /// </summary>
        public override int MinimumLevel => _minimumLevel.Value;
        
        /// <summary>
        /// <inheritdoc cref="AbstractMutatorSettings.MaximumLevel"/>
        /// </summary>
        public override int MaximumLevel => _maximumLevel.Value;
        
        /// <summary>
        /// Initializes generic mutator settings with an explicit default selection weight.
        /// </summary>
        /// <param name="namespace">The namespace used to make the mutator's slug unique.</param>
        /// <param name="name">The display name of the mutator.</param>
        /// <param name="description">The base description shown for the mutator.</param>
        /// <param name="weight">The default weighted selection value for the mutator.</param>
        /// <param name="config">The config file used to bind the mutator settings.</param>
        public GenericMutatorSettings(string @namespace, string name, string description, int weight, ConfigFile config) : base(@namespace, name, description)
        {
            _weight = config.BindPositive(
                GetSection(name),
                WeightConfigKey,
                weight,
                $"Weighted chance for the {name} Mutator to be active."
            );

            _minimumLevel = config.BindPositive(
                GetSection(name),
                MinimumLevelConfigKey,
                0,
                $"The minimum level on which the {name} Mutator can show up"
            );

            _maximumLevel = config.BindPositive(
                GetSection(name),
                MaximumLevelConfigKey,
                1000,
                $"The maximum level on which the {name} mutator can show up (0 = no upper bound)"
            );
        }

        /// <summary>
        /// Initializes generic mutator settings using the default selection weight.
        /// </summary>
        /// <param name="namespace">The namespace used to make the mutator's slug unique.</param>
        /// <param name="name">The display name of the mutator.</param>
        /// <param name="description">The base description shown for the mutator.</param>
        /// <param name="config">The config file used to bind the mutator settings.</param>
        public GenericMutatorSettings(string @namespace, string name, string description, ConfigFile config) : this(@namespace, name, description, 100, config)
        {
            
        }
        
        /// <summary>
        /// Clamped wrapper for <see cref="AbstractMutatorSettings.GetRuntimeOverride{T}(string, T)"/>
        /// </summary>
        /// <param name="key"><inheritdoc cref="AbstractMutatorSettings.GetRuntimeOverride{T}(string, T)"/></param>
        /// <param name="entry">The BepInEx ConfigEntry. It's <see cref="BepInEx.Configuration.AcceptableValueBase"/> will be used to clamp the return value.</param>
        /// <typeparam name="T">The type of the entry.</typeparam>
        /// <returns>
        /// The value clamped between the minimum and maximum configured on the passed ConfigEntry, or the result of <see cref="AbstractMutatorSettings.GetRuntimeOverride{T}(string, T)"/>
        /// if the ConfigEntry does not have an AcceptableValueBase.
        /// </returns>
        /// <remarks>
        /// This method is mainly intended to be used for <see cref="BepInEx.Configuration.AcceptableValueRange{T}">AcceptableValueRanges</see> with numeric values.
        /// Although accepted, other types of AcceptableValueBase might have unexpected results.
        /// <para>
        /// Consult the BepInEx documentation for more information on clamping ConfigEntries.
        /// </para>
        /// </remarks>
        protected T GetClampedRuntimeOverride<T>(string key, ConfigEntry<T> entry)
        {
            T value = GetRuntimeOverride(key, entry.Value);
            if (entry.Description.AcceptableValues == null)
            {
                return value;
            }

            return (T)entry.Description.AcceptableValues.Clamp(value);
        }

        /// <summary>
        /// Gets the config section name used for the mutator.
        /// </summary>
        /// <param name="name">The display name of the mutator.</param>
        /// <returns>The config section name for the mutator. Formatted as <c>{name} Mutator</c>.</returns>
        protected static string GetSection(string name)
        {
            return $"{name} Mutator";
        }
    }
}
