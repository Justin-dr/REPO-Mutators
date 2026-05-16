namespace Mutators.Settings
{
    internal class MultiMutatorSettings : AbstractMutatorSettings
    {
        private readonly string _name;
        private readonly string _description;
        private readonly uint _weight;
        private readonly uint _minimumLevel;
        private readonly uint _maximumLevel;
        public override string MutatorName => _name;

        public override string MutatorDescription => _description;

        public override uint Weight => _weight;

        public override uint MinimumLevel => _minimumLevel;

        public override uint MaximumLevel => _maximumLevel;

        public MultiMutatorSettings(string name, string description, uint weight = 0, uint minimumLevel = 0, uint maximumLevel = 1000)
        {
            _name = name;
            _description = description;
            _weight = weight;
            _minimumLevel = minimumLevel;
            _maximumLevel = maximumLevel;
        }
    }
}
