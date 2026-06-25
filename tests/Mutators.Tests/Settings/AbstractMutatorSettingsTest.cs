using Mutators.Settings;
using Is = NUnit.Framework.Is;

namespace Mutators.Tests.Settings
{
    class AbstractMutatorSettingsTest
    {
        [TestCase("Xepos.REPO-Mutators", "Out With a Bang!", "xepos.repo-mutators:out-with-a-bang")]
        [TestCase("Plugin", "Ultra-Violence", "plugin:ultra-violence")]
        [TestCase("Plugin", "One Shot, One Kill", "plugin:one-shot-one-kill")]
        [TestCase("Plugin", "multi: custom/mode\\name", "plugin:multi-custom-mode-name")]
        [TestCase("Plugin", "Crème Brûlée", "plugin:creme-brulee")]
        [TestCase("アドオン", "Pokémon mode", "アドオン:pokemon-mode")]
        [TestCase("Plugin", "東京", "plugin:hex-e69db1e4baac")]
        [TestCase("Plugin", "삼성", "plugin:hex-ec82bcec84b1")]
        [TestCase("Plugin", "比亚迪", "plugin:hex-e6af94e4ba9ae8bfaa")]
        [TestCase("Plugin", "Русский", "plugin:hex-d0a0d183d181d181d0bad0b8d0b9")]
        public void ToNamespacedName_FormatsNamespacedSlug(string @namespace, string name, string expected)
        {
            Assert.That(new TestSettings(@namespace, name, string.Empty).NamespacedName, Is.EqualTo(expected));
        }

        [Test]
        public void AsMetadata_WithFlatMetadata_WrapsMetadataInNamespacedName()
        {
            IDictionary<string, object> payload = new Dictionary<string, object>
            {
                { "setting", true }
            };

            IDictionary<string, object>? metadata = new MetadataSettings("Plugin", "Test", string.Empty, payload).AsMetadata();

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata, Contains.Key("plugin:test"));
            Assert.That(metadata!["plugin:test"], Is.SameAs(payload));
        }

        [Test]
        public void AsMetadata_WithAlreadyWrappedMetadata_ReturnsMetadataUnchanged()
        {
            IDictionary<string, object> payload = new Dictionary<string, object>
            {
                { "setting", true }
            };
            IDictionary<string, object> wrapped = new Dictionary<string, object>
            {
                { "plugin:test", payload }
            };

            IDictionary<string, object>? metadata = new MetadataSettings("Plugin", "Test", string.Empty, wrapped).AsMetadata();

            Assert.That(metadata, Is.SameAs(wrapped));
        }

        [Test]
        public void AsMetadata_WithNoMetadata_ReturnsNull()
        {
            Assert.That(new MetadataSettings("Plugin", "Test", string.Empty, null).AsMetadata(), Is.Null);
        }

        private sealed class TestSettings : AbstractMutatorSettings
        {
            public TestSettings(string @namespace, string name, string description) : base(@namespace, name, description)
            {
            }
            
            public override int Weight => 0;
            public override int MinimumLevel => 0;
            public override int MaximumLevel => 0;
        }

        private sealed class MetadataSettings : AbstractMutatorSettings
        {
            private readonly IDictionary<string, object>? _metadata;

            public MetadataSettings(string @namespace, string name, string description, IDictionary<string, object>? metadata) : base(@namespace, name, description)
            {
                _metadata = metadata;
            }

            public override int Weight => 0;
            public override int MinimumLevel => 0;
            public override int MaximumLevel => 0;

            protected override IDictionary<string, object>? CreateMetadata()
            {
                return _metadata;
            }
        }
    }
}
