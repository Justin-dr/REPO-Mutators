using Sirenix.Utilities;
using Mutators.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mutators.Settings
{
    public abstract class AbstractMutatorSettings
    {
        protected const string WeightConfigKey = "Weight";
        protected const string MinimumLevelConfigKey = "Minimum level";
        protected const string MaximumLevelConfigKey = "Maximum level";
        private readonly IDictionary<string, object> _runtimeOverrides = new Dictionary<string, object>();

        public abstract string MutatorName { get; }
        public abstract string MutatorDescription { get; }
        public abstract uint Weight { get; }
        public abstract uint MinimumLevel { get; }
        public abstract uint MaximumLevel { get; }

        public virtual void ApplyRuntimeOverrides(IDictionary<string, object>? overrides)
        {
            if (overrides == null || overrides.Count == 0) return;

            foreach (KeyValuePair<string, object> item in overrides)
            {
                _runtimeOverrides[item.Key] = item.Value;
            }
        }

        public virtual void ClearRuntimeOverrides()
        {
            _runtimeOverrides.Clear();
        }

        /// <summary>
        /// Helper method to get a host-only override for a setting.
        /// This will primarily be used for MultiMutator purposes.
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

        public virtual bool IsEligibleForSelection()
        {
            int levelsCompleted = RunManager.instance.levelsCompleted;

            if (MaximumLevel > 0 && MinimumLevel > MaximumLevel)
            {
                RepoMutators.Logger.LogWarning($"{MutatorName} was configured with a minimum level larger than the maximum level!");
                RepoMutators.Logger.LogWarning($"This configuration is consider invalid, the level bounds will be ignored.");
                return true;
            }

            if (MaximumLevel == 0)
            {
                return levelsCompleted >= MinimumLevel;
            }

            return levelsCompleted >= MinimumLevel && levelsCompleted <= MaximumLevel;
        }

        public virtual IDictionary<string, object>? AsMetadata()
        {
            return null;
        }
    }
}
