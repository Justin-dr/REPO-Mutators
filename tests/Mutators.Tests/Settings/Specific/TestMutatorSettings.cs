using Mutators.Settings;

namespace Mutators.Tests.Settings.Specific
{
    internal sealed class TestMutatorSettings : AbstractMutatorSettings
    {
        public TestMutatorSettings(string name, int weight, bool isEligibleForSelection = true) : base(MyPluginInfo.PLUGIN_GUID, name, $"{name} Description")
        {
            Weight = weight;
            selectable = isEligibleForSelection;
        }

        public TestMutatorSettings(string name, string description, int weight, bool isEligibleForSelection = true) : base(MyPluginInfo.PLUGIN_GUID, name, description)
        {
            Weight = weight;
            selectable = isEligibleForSelection;
        }

        public override int Weight { get; }
        public override int MinimumLevel => 0;
        public override int MaximumLevel => 1000;
            
        private readonly bool selectable;

        public override bool IsEligibleForSelection()
        {
            return selectable;
        }
    }
}