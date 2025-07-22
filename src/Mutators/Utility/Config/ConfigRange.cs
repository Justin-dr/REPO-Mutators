using System;
using BepInEx.Configuration;

namespace Mutators.Utility.Config
{
    /// <summary>
    /// Represents a numeric configuration range backed by minimum and maximum config entries.
    /// </summary>
    /// <typeparam name="T">The numeric value type used by the range.</typeparam>
    public class ConfigRange<T> where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        private readonly ConfigEntry<T> _min;
        private readonly ConfigEntry<T> _max;

        internal ConfigRange(ConfigEntry<T> min, ConfigEntry<T> max)
        {
            _min = min;
            _max = max;
        }

        /// <summary>
        /// The configured minimum value.
        /// </summary>
        public T MinimumValue => _min.Value;

        /// <summary>
        /// The configured maximum value.
        /// </summary>
        public T MaximumValue => _max.Value;
    }
}
