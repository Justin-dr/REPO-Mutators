namespace Mutators.Settings
{
    internal class MultiMutatorSettings : AbstractMutatorSettings
    {
        private readonly string _name;
        private readonly string _description;
        public override string MutatorName => _name;

        public override string MutatorDescription => _description;

        public override uint Weight => 0;

        public override uint MinimumLevel => 0;

        public override uint MaximumLevel => 1000;

        public MultiMutatorSettings(string name, string description)
        {
            _name = name;
            _description = description;
        }
    }
}
