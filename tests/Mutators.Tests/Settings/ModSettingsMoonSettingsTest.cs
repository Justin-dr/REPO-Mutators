using System.Reflection;
using BepInEx.Configuration;
using Mutators.Settings;
using Mutators.Tests.Logging.Loggers;

namespace Mutators.Tests.Settings
{
    internal class ModSettingsMoonSettingsTest
    {
        [SetUp]
        public void Setup()
        {
            SetLogger(new TestLogger());
        }

        [Test]
        public void GetMultiMutatorMoonRange_WhenMoonLevelExceedsConfiguredMoonCount_UsesHighestConfiguredMoonRange()
        {
            ConfigFile config = CreateConfig();
            ModSettings settings = new ModSettings(config);

            settings.LateBindMoonConfig(config, 4);
            SetMoonRange(config, 4, minimumMutators: 3, maximumMutators: 6, generatedChance: 73);

            ModSettings.MoonSetting moonSetting = settings.MoonMutatorSettings.GetMultiMutatorMoonRange(7);

            Assert.Multiple(() =>
            {
                Assert.That(moonSetting.MinimumMutators, Is.EqualTo(3));
                Assert.That(moonSetting.MaximumMutators, Is.EqualTo(6));
                Assert.That(moonSetting.GeneratedChance, Is.EqualTo(73));
            });
        }

        private static ConfigFile CreateConfig()
        {
            return new ConfigFile(
                Path.Combine(TestContext.CurrentContext.WorkDirectory, $"mutators-moon-settings-test-{Guid.NewGuid():N}.cfg"),
                false
            )
            {
                SaveOnConfigSet = false
            };
        }

        private static void SetMoonRange(ConfigFile config, int moon, int minimumMutators, int maximumMutators, int generatedChance) {
            string section = "Multi-Mutators - " + (moon == 0 ? "No Moon" : $"Moon {moon}");

            config.Bind(section, "Minimum Mutators", 0).Value = minimumMutators;
            config.Bind(section, "Maximum Mutators", 0).Value = maximumMutators;
            config.Bind(section, "Generated Multi-Mutator Chance (%)", 0).Value = generatedChance;
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
}
