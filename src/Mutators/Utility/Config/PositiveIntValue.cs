using BepInEx.Configuration;

namespace Mutators.Utility.Config
{
    internal class PositiveIntValue() : AcceptableValueBase(typeof(int))
    {
        private const int MinValue = 0;

        public override object Clamp(object value)
        {
            if (value is not int intValue) return MinValue;
            return intValue < MinValue ? MinValue : intValue;
        }

        public override bool IsValid(object value)
        {
            return value is >= MinValue;
        }

        public override string ToDescriptionString()
        {
            return "# Acceptable values are integers of 0 or higher";
        }
    }
}