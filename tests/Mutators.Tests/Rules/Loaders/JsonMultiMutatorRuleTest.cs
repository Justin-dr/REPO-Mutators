using Mutators.Rules;
using Mutators.Rules.Loaders.Json;
using Newtonsoft.Json;

namespace Mutators.Tests.Rules.Loaders
{
    class JsonMutatorRuleTest
    {
        [Test]
        public void Deserialize_MultiMutator_WithMutator_NormalizesToSingleItemList()
        {
            const string json = """
                                {
                                  "Key": "single",
                                  "Type": "Mutual Exclusion",
                                  "Mutator": "test:single"
                                }
                                """;

            JsonMutatorRule? rule = JsonConvert.DeserializeObject<JsonMutatorRule>(json);

            Assert.That(rule, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(rule.Type, Is.EqualTo(MultiMutatorRuleType.MutualExclusion));
                Assert.That(rule.Mutators, Is.EqualTo(new[] { "test:single" }));
            });
        }

        [Test]
        public void Deserialize_MultiMutator_WithMutators_UsesProvidedList()
        {
            const string json = """
                                {
                                  "Key": "multiple",
                                  "Type": "Mutual Exclusion",
                                  "Mutators": ["test:first", "test:second"]
                                }
                                """;

            JsonMutatorRule? rule = JsonConvert.DeserializeObject<JsonMutatorRule>(json);

            Assert.That(rule, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(rule.Type, Is.EqualTo(MultiMutatorRuleType.MutualExclusion));
                Assert.That(rule.Mutators, Is.EqualTo(new[] { "test:first", "test:second" }));
            });
        }
        
        [Test]
        public void Deserialize_SingleMutator_WithMutator_NormalizesToSingleItemList()
        {
            const string json = """
                                {
                                  "Key": "single",
                                  "Type": "Exclusion",
                                  "Mutator": "test:single"
                                }
                                """;

            JsonMutatorRule? rule = JsonConvert.DeserializeObject<JsonMutatorRule>(json);

            Assert.That(rule, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(rule.Type, Is.EqualTo(SingleMutatorRuleType.Exclusion));
                Assert.That(rule.Mutators, Is.EqualTo(new[] { "test:single" }));
            });
        }

        [Test]
        public void Deserialize_SingleMutator_WithMutators_UsesProvidedList()
        {
            const string json = """
                                {
                                  "Key": "multiple",
                                  "Type": "Exclusion",
                                  "Mutators": ["test:first", "test:second"]
                                }
                                """;

            JsonMutatorRule? rule = JsonConvert.DeserializeObject<JsonMutatorRule>(json);

            Assert.That(rule, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(rule.Type, Is.EqualTo(SingleMutatorRuleType.Exclusion));
                Assert.That(rule.Mutators, Is.EqualTo(new[] { "test:first", "test:second" }));
            });
        }
    }
}
