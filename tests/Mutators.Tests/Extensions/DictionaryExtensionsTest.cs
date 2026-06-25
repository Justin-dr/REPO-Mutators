using System.Reflection;
using Mutators.Extensions;
using Mutators.Tests.Logging.Loggers;

namespace Mutators.Tests.Extensions
{
    public class DictionaryExtensionsTest
    {
        private TestLogger _logger = null!;

        [SetUp]
        public void Setup()
        {
            _logger = new TestLogger();
            SetLogger(_logger);
        }

        [Test]
        public void LogMetadata_WithNestedMetadata_WritesIndentedDebugLogs()
        {
            IDictionary<string, object> metadata = new Dictionary<string, object>
            {
                { "name", "Xepos" },
                { "bag", new Dictionary<string, object>
                    {
                        { "keyItems", new List<string> { "runningShoes", "bicycle", "key"} },
                        { "pokéballs", new Dictionary<string, object>()
                        {
                            { "pokéball", 1 },
                            { "greatball", 2 },
                            { "ultraball", 3}
                        }},
                        { "something", true}
                    }
                }
            };

            metadata.LogMetadata();

            Assert.That(_logger.DebugLogs, Is.EqualTo([
                "name: Xepos",
                "bag:",
                " keyItems: [runningShoes, bicycle, key]",
                " pokéballs:",
                "  pokéball: 1",
                "  greatball: 2",
                "  ultraball: 3",
                " something: True"
            ]));
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
