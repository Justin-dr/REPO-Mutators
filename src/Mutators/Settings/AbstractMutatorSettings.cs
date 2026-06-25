using System.Collections.Generic;
using System.Linq;
using Mutators.Extensions;

namespace Mutators.Settings
{
    /// <summary>
    /// Base class for all mutator settings.
    /// </summary>
    public abstract class AbstractMutatorSettings
    {
        /// <summary>
        /// Standard config key for the mutator's weight.'
        /// </summary>
        protected const string WeightConfigKey = "Weight";
        
        /// <summary>
        /// Standard config key for the mutator's minimum level requirement.'
        /// </summary>
        protected const string MinimumLevelConfigKey = "Minimum level";
        
        /// <summary>
        /// Standard config key for the mutator's maximum level.'
        /// </summary>
        protected const string MaximumLevelConfigKey = "Maximum level";
        private readonly IDictionary<string, object> _runtimeOverrides = new Dictionary<string, object>();

        /// <summary>
        /// The slugified and namespaced representation of the mutator's name.
        /// <para>
        /// This property combines the namespace and name into a standardized, unique identifier
        /// to ensure consistency across different contexts where the mutator is used.
        /// </para>
        /// </summary>
        public string NamespacedName { get; }
        
        /// <summary>
        /// Display name of the mutator.
        /// </summary>
        public string MutatorName { get; }
        
        /// <summary>
        /// Description of the mutator, in its most basic form.
        /// </summary>
        public string MutatorDescription { get; }
        
        /// <summary>
        /// The weight of the mutator, used for weighted random selection.
        /// </summary>
        public abstract int Weight { get; }
        
        /// <summary>
        /// The minimum level at which the mutator is eligible for selection.
        /// </summary>
        public abstract int MinimumLevel { get; }
        
        /// <summary>
        /// The maximum level at which the mutator is eligible for selection.
        /// </summary>
        public abstract int MaximumLevel { get; }

        /// <summary>
        /// Initializes the shared identity and display properties for a mutator.
        /// </summary>
        /// <param name="namespace">The namespace used to make the mutator's slug unique.</param>
        /// <param name="name">The display name of the mutator.</param>
        /// <param name="description">The base description shown for the mutator.</param>
        public AbstractMutatorSettings(string @namespace, string name, string description)
        {
            NamespacedName = name.ToSlug(@namespace);
            MutatorName = name;
            MutatorDescription = description;
        }

        /// <summary>
        /// Applies temporary setting overrides for the current mutator.
        /// </summary>
        /// <param name="overrides">The keyed override values to apply, or null to leave the current overrides unchanged.</param>
        public virtual void ApplyRuntimeOverrides(IDictionary<string, object>? overrides)
        {
            if (overrides == null || overrides.Count == 0) return;

            foreach (KeyValuePair<string, object> item in overrides)
            {
                _runtimeOverrides[item.Key] = item.Value;
            }
        }

        /// <summary>
        /// Clears all temporary setting overrides.
        /// </summary>
        public virtual void ClearRuntimeOverrides()
        {
            _runtimeOverrides.Clear();
        }

        /// <summary>
        /// Helper method to get a host-only override for a setting.
        /// This will primarily be used for multi-mutator purposes.
        /// </summary>
        /// <param name="key">The unique key of the setting value</param>
        /// <param name="fallback">The value that will be used when the key is not present or cannot be coerced</param>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <returns>The coerced value for the requested key, or a fallback in case it couldn't be found or coerced</returns>
        protected T GetRuntimeOverride<T>(string key, T fallback)
        {
            if (!_runtimeOverrides.TryGetValue(key, out object value)) return fallback;

            return value.TryCoerce(typeof(T), out object? coerced) && coerced is T typed ? typed : fallback;
        }

        /// <summary>
        /// Gets a runtime override as a list of values.
        /// </summary>
        /// <param name="key">The unique key of the setting value.</param>
        /// <param name="fallback">The list used when the key is not present or cannot be coerced.</param>
        /// <typeparam name="T">The expected element type.</typeparam>
        /// <returns>The coerced override list for the requested key, or the fallback list if coercion fails.</returns>
        protected IList<T> GetRuntimeOverrideList<T>(string key, IList<T> fallback)
        {
            if (!_runtimeOverrides.TryGetValue(key, out object value)) return fallback;

            if (value is IList<T> list) return list;
            if (value is IEnumerable<T> typedValues) return typedValues.ToList();

            if (value is string stringValue && typeof(T) == typeof(string))
            {
                return stringValue.Split(",")
                    .Select(item => item.Trim())
                    .Where(item => item.Length > 0)
                    .Cast<T>()
                    .ToList();
            }

            if (value is IEnumerable<object> values)
            {
                IList<T> converted = new List<T>();

                foreach (object item in values)
                {
                    if (item.TryCoerce(typeof(T), out object? coerced) && coerced is T typed)
                    {
                        converted.Add(typed);
                    }
                    else
                    {
                        return fallback;
                    }
                }

                return converted;
            }

            return value.TryCoerce(typeof(T), out object? single) && single is T singleTyped
                ? [singleTyped]
                : fallback;
        }

        /// <summary>
        /// Determines if the mutator is eligible for selection.
        /// <para>
        /// By default, this checks if the current level is within the mutator's configured level range.
        /// </para>
        /// </summary>
        /// <returns>True if the mutator is eligible for selection, otherwise false</returns>
        public virtual bool IsEligibleForSelection()
        {
            int levelsCompleted = RunManager.instance.levelsCompleted;

            if (MaximumLevel > 0 && MinimumLevel > MaximumLevel)
            {
                RepoMutators.Logger.LogWarning($"{MutatorName} was configured with a minimum level larger than the maximum level!");
                RepoMutators.Logger.LogWarning("This configuration is consider invalid, the level bounds will be ignored.");
                return true;
            }

            if (MaximumLevel == 0)
            {
                return levelsCompleted >= MinimumLevel;
            }

            return levelsCompleted >= MinimumLevel && levelsCompleted <= MaximumLevel;
        }

        /// <summary>
        /// Converts settings that must be synchronized to clients into metadata.
        /// </summary>
        /// <remarks>
        /// This should only be used for static settings and NOT for dynamic metadata. Keys under the <see cref="NamespacedName"/> of the returned dictionary
        /// are considered to always be immediate for clients and do not get deferred until the level starts.
        /// </remarks>
        /// <returns>
        /// A dictionary containing settings as metadata for this mutator, keyed under its <see cref="NamespacedName"/>, or null when the settings do not need synchronization.
        /// </returns>
        public IDictionary<string, object>? AsMetadata()
        {
            return CreateMetadata()?.WithMutator(NamespacedName);
        }

        /// <summary>
        /// Converts settings that must be synchronized to clients into metadata.
        /// </summary>
        /// <remarks>
        /// This should only be used for static settings and NOT for dynamic metadata. Keys of the returned dictionary
        /// are considered to always be immediate for clients and do not get deferred until the level starts.
        /// <para>
        /// The results of this method are fed into <see cref="AsMetadata"/> to ensure proper namespace handling.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A dictionary containing settings as metadata for this mutator, or null when the settings do not need synchronization.
        /// </returns>
        protected virtual IDictionary<string, object>? CreateMetadata()
        {
            return null;
        }
    }
}
