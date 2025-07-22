using Mutators.Enums;
using Mutators.Extensions;
using Mutators.Mutators;
using Mutators.Settings;

namespace Mutators.Tests.Extensions
{
    class MultiMutatorExtensionsTest
    {
        [Test]
        public void Format_WithFlatSubMutatorMetadata_WrapsEachPayloadInSubMutatorNamespacedName()
        {
            IDictionary<string, object> alphaPayload = new Dictionary<string, object>
            {
                { "alpha-setting", 1 }
            };
            IDictionary<string, object> betaPayload = new Dictionary<string, object>
            {
                { "beta-setting", false }
            };
            IDictionary<string, object> alphaOverrides = new Dictionary<string, object>
            {
                { "alpha-setting", 2 }
            };
            IDictionary<string, object> betaOverrides = new Dictionary<string, object>
            {
                { "beta-setting", true }
            };

            TestMutator alpha = new("Alpha", alphaPayload);
            TestMutator beta = new("Beta", betaPayload);
            TestMultiMutator multiMutator = new(
                "Combo",
                new Dictionary<IMutator, IDictionary<string, object>>
                {
                    { alpha, alphaOverrides },
                    { beta, betaOverrides }
                }
            );

            (IList<string> mutators, IDictionary<string, object> metadata) = multiMutator.Format();

            Assert.That(mutators, Is.EquivalentTo(new[] { alpha.NamespacedName, beta.NamespacedName }));
            Assert.That(metadata[alpha.NamespacedName], Is.SameAs(alphaPayload));
            Assert.That(metadata[beta.NamespacedName], Is.SameAs(betaPayload));
            Assert.That(metadata.ContainsKey("alpha-setting"), Is.False);
            Assert.That(metadata.ContainsKey("beta-setting"), Is.False);

            IDictionary<string, object> overrides = (IDictionary<string, object>)metadata[RepoMutators.MUTATOR_OVERRIDES];
            Assert.That(overrides["namespacedName"], Is.EqualTo(multiMutator.NamespacedName));
            Assert.That(overrides[alpha.NamespacedName], Is.SameAs(alphaOverrides));
            Assert.That(overrides[beta.NamespacedName], Is.SameAs(betaOverrides));
        }

        private class TestMutator : IMutator
        {
            public TestMutator(string name, IDictionary<string, object>? metadata = null)
            {
                Settings = new TestSettings(name, metadata);
            }

            public string NamespacedName => Settings.NamespacedName;
            public string Name => Settings.MutatorName;
            public string Description => Settings.MutatorDescription;
            public MutatorDifficulty Difficulty => MutatorDifficulty.Normal;
            public MutatorSource Source => MutatorSource.Mod;
            public bool Active => false;
            public bool HasSpecialAction => false;
            public AbstractMutatorSettings Settings { get; }
            public IReadOnlyList<Type> Patches => [];
            public IReadOnlyList<Func<bool>> Conditions => [];

            public void Patch()
            {
            }

            public void Unpatch()
            {
            }

            public void ConsumeMetadata(IDictionary<string, object> metadata)
            {
                
            }
        }

        private sealed class TestMultiMutator : TestMutator, IMultiMutator
        {
            public TestMultiMutator(string name, IReadOnlyDictionary<IMutator, IDictionary<string, object>> subMutators) : base(name)
            {
                SubMutators = subMutators;
            }

            public IReadOnlyDictionary<IMutator, IDictionary<string, object>> SubMutators { get; }
        }

        private sealed class TestSettings : AbstractMutatorSettings
        {
            private readonly IDictionary<string, object>? _metadata;

            public TestSettings(string name, IDictionary<string, object>? metadata) : base("Plugin", name, $"{name} Description")
            {
                _metadata = metadata;
            }

            public override int Weight => 1;
            public override int MinimumLevel => 0;
            public override int MaximumLevel => 1000;

            protected override IDictionary<string, object>? CreateMetadata()
            {
                return _metadata;
            }
        }
    }
}
