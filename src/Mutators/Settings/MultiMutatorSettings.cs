namespace Mutators.Settings
{
    internal class MultiMutatorSettings : AbstractMutatorSettings
    {
        public override int Weight { get; }

        public override int MinimumLevel { get; }

        public override int MaximumLevel { get; }

        public MultiMutatorSettings(string @namespace, string name, string description, int weight = 0, int minimumLevel = 0, int maximumLevel = 1000) :  base(@namespace, name, description)
        {
            Weight = weight;
            MinimumLevel = minimumLevel;
            MaximumLevel = maximumLevel;
        }
    }
}
