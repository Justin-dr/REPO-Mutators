using System.Reflection;
using BepInEx.Configuration;
using Mutators.Mutators;
using Mutators.Rules.Registries;
using Mutators.Services.Selection;
using Mutators.Services.Selection.Strategies;
using Mutators.Settings;
using Mutators.Tests.Logging.Loggers;
using Mutators.Tests.Providers.Random;
using Mutators.Tests.Providers.Semiwork;
using Mutators.Utility.Config;

namespace Mutators.Tests.Services.Selection
{
    internal abstract class SelectionTestBase
    {
        private const string RandomSection = "Multi-Mutators - Random";

        private ConfigFile _config = null!;
        private ConfigEntry<int> _nopChance = null!;
        private ConfigEntry<string> _scalingMode = null!;
        private ConfigEntry<int> _randomMinimumAmount = null!;
        private ConfigEntry<int> _randomMaximumAmount = null!;

        protected TestRandomProvider RandomProvider { get; private set; } = null!;
        protected TestSemiFuncProvider SemiFuncProvider { get; private set; } = null!;
        protected TestLogger Logger { get; private set; } = null!;
        protected NopMutator NopMutator { get; private set; } = null!;
        protected IRepeatSelectionTracker RepeatSelectionTracker { get; private set; } = null!;
        protected GeneratedMultiMutatorSelectionRulesRegistry MultiMutatorSelectionRulesRegistry { get; private set; } = null!;
        protected SingleMutatorSelectionRulesRegistry SingleMutatorSelectionRulesRegistry { get; private set; } = null!;

        [SetUp]
        public void SetupSelectionTestBase()
        {
            _config = new ConfigFile(
                Path.Combine(TestContext.CurrentContext.WorkDirectory, $"mutators-selection-test-{Guid.NewGuid():N}.cfg"),
                false
            )
            {
                SaveOnConfigSet = false
            };

            _nopChance = _config.Bind("No Mutator", "Chance (%)", 0);
            _scalingMode = _config.Bind("Multi-Mutators", "Scaling Mode", "None");
            _randomMinimumAmount = _config.Bind(RandomSection, "Minimum amount of Mutators", 1);
            _randomMaximumAmount = _config.Bind(RandomSection, "Maximum amount of Mutators", 5);

            MutatorSettings.Initialize(_config);
            SetRepoMutatorsSettings(new ModSettings(_config));

            _nopChance.Value = 0;

            RandomProvider = new TestRandomProvider();
            SemiFuncProvider = new TestSemiFuncProvider();
            Logger = new TestLogger();
            SetLogger(Logger);

            NopMutator = new NopMutator(MutatorSettings.NopMutator);
            RepeatSelectionTracker = new RepeatSelectionTracker();
            MultiMutatorSelectionRulesRegistry = new GeneratedMultiMutatorSelectionRulesRegistry();
            SingleMutatorSelectionRulesRegistry = new SingleMutatorSelectionRulesRegistry();
        }

        protected void SetScalingType(ModSettings.MultiMutatorScalingType scalingType)
        {
            _scalingMode.Value = scalingType.ToString();
            RepoMutators.Settings.CacheKeys();
        }

        protected void SetNopChance(int chance)
        {
            _nopChance.Value = chance;
        }

        protected void ConfigureMoonRange(int moonLevel, int minimumMutators, int maximumMutators, int generatedChance)
        {
            byte moon = (byte)moonLevel;

            ConfigEntry<int> minimum = _config.Bind($"Test Multi-Mutators - Moon {moon}", "Minimum Mutators", minimumMutators);
            ConfigEntry<int> maximum = _config.Bind($"Test Multi-Mutators - Moon {moon}", "Maximum Mutators", maximumMutators);
            ConfigEntry<int> chance = _config.Bind($"Test Multi-Mutators - Moon {moon}", "Generated Multi-Mutator Chance (%)", generatedChance);

            minimum.Value = minimumMutators;
            maximum.Value = maximumMutators;
            chance.Value = generatedChance;

            ConfigRange<int> range = new(minimum, maximum);
            ModSettings.MoonSetting moonSetting = new(range, chance);

            FieldInfo moonRangesField = typeof(ModSettings.MoonSettings).GetField(
                "_multiMutatorMoonRanges",
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;

            Dictionary<byte, ModSettings.MoonSetting> ranges =
                (Dictionary<byte, ModSettings.MoonSetting>)moonRangesField.GetValue(RepoMutators.Settings.MoonMutatorSettings)!;

            ranges[moon] = moonSetting;
        }

        protected void ConfigureRandomAmountRange(int minimumMutators, int maximumMutators)
        {
            _randomMinimumAmount.Value = minimumMutators;
            _randomMaximumAmount.Value = maximumMutators;
        }

        protected void SetAllRandomAmountWeights(int weight)
        {
            for (byte amount = 1; amount <= ModSettings.MaximumGeneratedActiveSubMutators; amount++)
            {
                GetRandomAmountWeightEntry(amount).Value = weight;
            }
        }

        protected void SetRandomAmountWeight(int amount, int weight)
        {
            GetRandomAmountWeightEntry((byte)amount).Value = weight;
        }

        protected void SetRandomGeneratedChance(int amount, int chance)
        {
            _config.Bind(RandomSection, $"{amount} Mutators - Generated Multi-Mutator Chance (%)", 50).Value = chance;
        }

        protected NoneScalingSelectionStrategy CreateNoneStrategy()
        {
            return new NoneScalingSelectionStrategy(MultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry, RepeatSelectionTracker, RandomProvider, NopMutator);
        }

        protected RandomScalingSelectionStrategy CreateRandomStrategy()
        {
            return new RandomScalingSelectionStrategy(
                MultiMutatorSelectionRulesRegistry,
                SingleMutatorSelectionRulesRegistry,
                RepoMutators.Settings.RandomMutatorSettings,
                RepeatSelectionTracker,
                RandomProvider,
                NopMutator
            );
        }

        protected MoonScalingSelectionStrategy CreateMoonStrategy()
        {
            return new MoonScalingSelectionStrategy(
                MultiMutatorSelectionRulesRegistry,
                SingleMutatorSelectionRulesRegistry,
                RepoMutators.Settings.MoonMutatorSettings,
                SemiFuncProvider,
                RepeatSelectionTracker,
                RandomProvider,
                NopMutator
            );
        }

        protected MutatorSelectionService CreateSelectionService(
            IDictionary<ModSettings.MultiMutatorScalingType, MutatorSelectionStrategy> strategies)
        {
            MutatorSelectionService service = new(
                CreateNoneStrategy(),
                CreateMoonStrategy(),
                CreateRandomStrategy(),
                RepeatSelectionTracker,
                RandomProvider,
                NopMutator
            );

            FieldInfo strategiesField = typeof(MutatorSelectionService).GetField(
                "_selectionStrategies",
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;
            IDictionary<ModSettings.MultiMutatorScalingType, MutatorSelectionStrategy> configuredStrategies =
                (IDictionary<ModSettings.MultiMutatorScalingType, MutatorSelectionStrategy>)strategiesField.GetValue(service)!;

            configuredStrategies.Clear();
            foreach ((ModSettings.MultiMutatorScalingType scalingType, MutatorSelectionStrategy strategy) in strategies)
            {
                configuredStrategies.Add(scalingType, strategy);
            }

            return service;
        }

        protected static TestMutator Mutator(
            string name,
            int weight,
            bool eligible = true,
            IReadOnlyList<Func<bool>>? conditions = null
        )
        {
            return new TestMutator(name, weight, eligible, conditions: conditions);
        }

        protected static SelectionTestMultiMutator MultiMutator(string name, int weight, int subMutatorCount, bool eligible = true)
        {
            Dictionary<IMutator, IDictionary<string, object>> subMutators = Enumerable.Range(0, subMutatorCount)
                .Select(IMutator (index) => new TestMutator($"{name} Sub {index + 1}", 1))
                .ToDictionary(mutator => mutator, IDictionary<string, object> (_) => new Dictionary<string, object>());

            return new SelectionTestMultiMutator(name, weight, subMutators, eligible);
        }

        private ConfigEntry<int> GetRandomAmountWeightEntry(byte amount)
        {
            return _config.Bind(RandomSection, $"{amount} Mutators - Weight", 50);
        }

        private static void SetRepoMutatorsSettings(ModSettings settings)
        {
            FieldInfo repoSettingsField = typeof(RepoMutators).GetField(
                "<Settings>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic
            )!;

            repoSettingsField.SetValue(null, settings);
        }

        private static void SetLogger(TestLogger testLogger)
        {
            PropertyInfo loggerProperty = typeof(RepoMutators).GetProperty(
                "Logger",
                BindingFlags.Static | BindingFlags.NonPublic
            )!;

            loggerProperty.GetSetMethod(true)!.Invoke(null, [testLogger]);
        }
    }

    internal sealed class SelectionTestMultiMutator : TestMutator, IMultiMutator
    {
        public SelectionTestMultiMutator(
            string name,
            int weight,
            IReadOnlyDictionary<IMutator, IDictionary<string, object>> subMutators,
            bool eligible = true
        ) : base(name, weight, eligible)
        {
            SubMutators = subMutators;
        }

        public IReadOnlyDictionary<IMutator, IDictionary<string, object>> SubMutators { get; }
    }
}
