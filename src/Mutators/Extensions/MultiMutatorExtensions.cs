using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mutators.Mutators;

namespace Mutators.Extensions
{
    internal static class MultiMutatorExtensions
    {
        internal static (IList<string> mutators, IDictionary<string, object> meta) Format(this IMultiMutator multiMutator)
        {
            IEnumerable<IMutator> subMutators = multiMutator.SubMutators.Keys;

            IDictionary<string, object> metadata = subMutators.Select(mutator => mutator.Settings.AsMetadata())
                .SelectMany(dict => dict ?? new Dictionary<string, object>())
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            metadata.Add(RepoMutators.MUTATOR_OVERRIDES, CreateOverrides(multiMutator));

            return (subMutators.Select(mutator => mutator.NamespacedName).ToList(), metadata);
        }

        private static IDictionary<string, object> CreateOverrides(IMultiMutator multiMutator)
        {
            IDictionary<string, object> mutatorOverrides = new Dictionary<string, object>
            {
                { "namespacedName", multiMutator.NamespacedName },
                { "name", multiMutator.Name },
                { "description", multiMutator.Description }
            };

            multiMutator.SubMutators.ForEach(mutator => mutatorOverrides.Add(mutator.Key.NamespacedName, mutator.Value));

            return mutatorOverrides;
        }

        internal static bool TryCoerce(this object? value, Type targetType, out object? coerced)
        {
            coerced = null;

            // Handle nulls and Nullable<T>
            var underlying = Nullable.GetUnderlyingType(targetType);
            bool isNullable = underlying != null;
            var effectiveTarget = underlying ?? targetType;

            if (value is null)
            {
                if (isNullable) { coerced = null; return true; }
                return false;
            }

            var valueType = value.GetType();
            if (effectiveTarget.IsAssignableFrom(valueType))
            {
                coerced = value;
                return true;
            }

            // Enums: allow string or numeric to enum
            if (effectiveTarget.IsEnum)
            {
                try
                {
                    if (value is string s)
                    {
                        coerced = Enum.Parse(effectiveTarget, s, ignoreCase: true);
                        return true;
                    }
                    var num = Convert.ChangeType(value, Enum.GetUnderlyingType(effectiveTarget), CultureInfo.InvariantCulture);
                    coerced = Enum.ToObject(effectiveTarget, num!);
                    return true;
                }
                catch { return false; }
            }

            // Numeric / convertible path (includes decimal)
            try
            {
                coerced = Convert.ChangeType(value, effectiveTarget, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
