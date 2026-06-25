using Mutators.Enums;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Settings;
using Mutators.Tests.Extensions.Test;
using Mutators.Tests.Managers;
using Mutators.Tests.Mutators.Patches;
using Mutators.Tests.Settings.Specific;

namespace Mutators.Tests.Mutators
{
    class MutatorTest
    {
        class ConsumeMetadata() : MutatorManagerTestBase(ModSettings.MultiMutatorScalingType.None)
        {
            [TearDown]
            public void TearDown()
            {
                TestPatch.Metadata = null;
            }

            [Test]
            public void Metadata_EmptyOnInstantiation()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [], MutatorDifficulty.Normal);

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Is.Empty);
            }

            [Test]
            public void Metadata_WithEmptyMetadata_IsEmpty()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [], MutatorDifficulty.Normal);

                mutator.ConsumeMetadata(new Dictionary<string, object>());

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Is.Empty);
            }

            [Test]
            public void Metadata_NoNamespacedName_IsEmpty()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [], MutatorDifficulty.Normal);

                mutator.ConsumeMetadata(new Dictionary<string, object> { { "Coins", 1 } });

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Is.Empty);
            }

            [Test]
            public void Metadata_WrongNamespacedName_IsEmpty()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [], MutatorDifficulty.Normal);

                mutator.ConsumeMetadata(new Dictionary<string, object>
                {
                    { "plugin:pokemon-mod", new Dictionary<string, object> { { "caught", 5 } } }
                });

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Is.Empty);
            }

            [Test]
            public void Metadata_Namespaced_Empty_IsEmpty()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [], MutatorDifficulty.Normal);

                mutator.ConsumeMetadata(new Dictionary<string, object>
                {
                    { "xepos.repo-mutators:test", new Dictionary<string, object>() }
                });

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Is.Empty);
            }

            [Test]
            public void Metadata_Namespaced_WithMetadata_IsNotEmpty()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [], MutatorDifficulty.Normal);

                MutatorManager.Instance.GameStateChanged += _ => { };
                MutatorManager.Instance.GameState = MutatorsGameState.LevelReady;

                mutator.ConsumeMetadata(new Dictionary<string, object>
                {
                    { "xepos.repo-mutators:test", new Dictionary<string, object> { { "caught", 5 } } }
                });

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Has.Count.EqualTo(1));
                Assert.That(metadata["caught"], Is.EqualTo(5));
            }
            
            [Test]
            public void Metadata_Namespaced_WithMetadata_OnMetadataChanged_IsCalled()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [typeof(TestPatch)], MutatorDifficulty.Normal);

                MutatorManager.Instance.GameStateChanged += _ => { };
                MutatorManager.Instance.GameState = MutatorsGameState.LevelReady;

                mutator.ConsumeMetadata(new Dictionary<string, object>
                {
                    { "xepos.repo-mutators:test", new Dictionary<string, object> { { "caught", 5 } } }
                });

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Has.Count.EqualTo(1));
                Assert.That(metadata["caught"], Is.EqualTo(5));
                
                Assert.That(TestPatch.Metadata, Is.Not.Null);
                Assert.That(TestPatch.Metadata, Has.Count.EqualTo(1));
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(TestPatch.Metadata["caught"], Is.EqualTo(5));
                    Assert.That(TestPatch.Metadata, Is.SameAs(metadata));
                }
            }

            [Test]
            public void Metadata_Namespaced_WithMetadata_OverridesExistingMetadata()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [], MutatorDifficulty.Normal);

                MutatorManager.Instance.GameStateChanged += _ => { };
                MutatorManager.Instance.GameState = MutatorsGameState.LevelReady;

                IDictionary<string, object> initialMetadata = mutator.GetMetadata();

                initialMetadata.Add("caught", 151);

                Assert.That(initialMetadata, Is.Not.Null);
                Assert.That(initialMetadata, Has.Count.EqualTo(1));
                Assert.That(initialMetadata["caught"], Is.EqualTo(151));

                mutator.ConsumeMetadata(new Dictionary<string, object>
                {
                    { "xepos.repo-mutators:test", new Dictionary<string, object> { { "caught", 251 } } }
                });

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Has.Count.EqualTo(1));
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(metadata["caught"], Is.EqualTo(251));
                    Assert.That(logger.DebugLogs, Has.One.Contains("Metadata key 'caught' immediate"));
                }
            }

            [Test]
            public void Metadata_Namespaced_WithMetadata_ShallowMerges()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [], MutatorDifficulty.Normal);

                MutatorManager.Instance.GameStateChanged += _ => { };
                MutatorManager.Instance.GameState = MutatorsGameState.LevelReady;

                IDictionary<string, object> initialMetadata = mutator.GetMetadata();

                initialMetadata.Add("caught", 251);

                Assert.That(initialMetadata, Is.Not.Null);
                Assert.That(initialMetadata, Has.Count.EqualTo(1));
                Assert.That(initialMetadata["caught"], Is.EqualTo(251));

                mutator.ConsumeMetadata(new Dictionary<string, object>
                {
                    { "xepos.repo-mutators:test", new Dictionary<string, object> { { "seen", 386 } } }
                });

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Has.Count.EqualTo(2));
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(metadata["caught"], Is.EqualTo(251));
                    Assert.That(metadata["seen"], Is.EqualTo(386));
                    Assert.That(logger.DebugLogs, Has.One.Contains("Metadata key 'seen' immediate"));
                }
            }

            [Test]
            public void Metadata_Namespaced_WithMetadata_DeepMerges()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [typeof(TestPatch)], MutatorDifficulty.Normal);

                MutatorManager.Instance.GameStateChanged += _ => { };
                MutatorManager.Instance.GameState = MutatorsGameState.LevelReady;

                IDictionary<string, object> initialMetadata = mutator.GetMetadata();

                initialMetadata.Add("bag", new Dictionary<string, object>
                {
                    {
                        "pokéballs", new Dictionary<string, object>
                        {
                            { "pokéball", 10 },
                            { "greatball", 6 }
                        }
                    }
                });
                initialMetadata.Add("team", new Dictionary<string, object>
                {
                    { "Squirtle", 15 },
                    { "Mudkip", 11 },
                });

                mutator.ConsumeMetadata(new Dictionary<string, object>
                {
                    {
                        "xepos.repo-mutators:test", new Dictionary<string, object>
                        {
                            {
                                "bag", new Dictionary<string, object>
                                {
                                    {
                                        "pokéballs", new Dictionary<string, object>
                                        {
                                            { "ultraball", 21 },
                                            { "netball", 1 }
                                        }
                                    }
                                }
                            },
                            {
                                "team", new Dictionary<string, object>
                                {
                                    { "Mudkip", 13 },
                                    { "Mewtwo", 100 },
                                }
                            }
                        }
                    }
                });

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Has.Count.EqualTo(2));
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(metadata["bag"], Is.Not.Null);
                    Assert.That(metadata["team"], Is.Not.Null);

                    IDictionary<string, object>? bag = metadata.Get<IDictionary<string, object>>("bag");
                    IDictionary<string, object>? team = metadata.Get<IDictionary<string, object>>("team");

                    Assert.That(bag, Is.Not.Null);
                    Assert.That(bag, Has.Count.EqualTo(1));

                    IDictionary<string, object>? pokeballs = bag.Get<IDictionary<string, object>>("pokéballs");
                    
                    Assert.That(pokeballs, Is.Not.Null);
                    Assert.That(pokeballs, Has.Count.EqualTo(4));

                    Assert.That(pokeballs, Contains.Key("pokéball").WithValue(10));
                    Assert.That(pokeballs, Contains.Key("greatball").WithValue(6));
                    Assert.That(pokeballs, Contains.Key("ultraball").WithValue(21));
                    Assert.That(pokeballs, Contains.Key("netball").WithValue(1));

                    Assert.That(team, Is.Not.Null);
                    Assert.That(team, Has.Count.EqualTo(3));

                    Assert.That(team, Contains.Key("Squirtle").WithValue(15));
                    Assert.That(team, Contains.Key("Mudkip").WithValue(13));
                    Assert.That(team, Contains.Key("Mewtwo").WithValue(100));
                    
                    Assert.That(logger.DebugLogs, Has.One.Contains("Metadata key 'bag' immediate"));
                    Assert.That(logger.DebugLogs, Has.One.Contains("Metadata key 'team' immediate"));
                }
                
                Assert.That(TestPatch.Metadata, Is.Not.Null);
                Assert.That(TestPatch.Metadata, Has.Count.EqualTo(2));
                Assert.That(TestPatch.Metadata, Is.SameAs(metadata));
            }
            
            [Test]
            public void Metadata_Namespaced_WithMetadata_ShallowMerges_NullRemovesKey()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [], MutatorDifficulty.Normal);

                MutatorManager.Instance.GameStateChanged += _ => { };
                MutatorManager.Instance.GameState = MutatorsGameState.LevelReady;

                IDictionary<string, object> initialMetadata = mutator.GetMetadata();

                initialMetadata.Add("caught", 251);

                Assert.That(initialMetadata, Is.Not.Null);
                Assert.That(initialMetadata, Has.Count.EqualTo(1));
                Assert.That(initialMetadata["caught"], Is.EqualTo(251));

                mutator.ConsumeMetadata(new Dictionary<string, object>
                {
                    { "xepos.repo-mutators:test", new Dictionary<string, object> { { "seen", 386 }, { "caught", null! } } }
                });

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Has.Count.EqualTo(1));
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(metadata["seen"], Is.EqualTo(386));
                    Assert.That(logger.DebugLogs, Has.One.Contains("Metadata key 'seen' immediate"));
                }
            }
            
            [Test]
            public void Metadata_Namespaced_WithMetadata_DeepMerges_NullRemovesKey()
            {
                AbstractMutatorSettings abstractMutatorSettings = new TestMutatorSettings("Test", 1);
                IMutator mutator = new Mutator(abstractMutatorSettings, [], MutatorDifficulty.Normal);

                MutatorManager.Instance.GameStateChanged += _ => { };
                MutatorManager.Instance.GameState = MutatorsGameState.LevelReady;

                IDictionary<string, object> initialMetadata = mutator.GetMetadata();

                initialMetadata.Add("bag", new Dictionary<string, object>
                {
                    {
                        "pokéballs", new Dictionary<string, object>
                        {
                            { "pokéball", 10 },
                            { "greatball", 6 }
                        }
                    }
                });
                initialMetadata.Add("team", new Dictionary<string, object>
                {
                    { "Squirtle", 15 },
                    { "Mudkip", 11 },
                });

                mutator.ConsumeMetadata(new Dictionary<string, object>
                {
                    {
                        "xepos.repo-mutators:test", new Dictionary<string, object>
                        {
                            {
                                "bag", new Dictionary<string, object>
                                {
                                    {
                                        "pokéballs", new Dictionary<string, object>
                                        {
                                            { "greatball", null! },
                                            { "ultraball", 21 },
                                            { "netball", 1 }
                                        }
                                    }
                                }
                            },
                            {
                                "team", new Dictionary<string, object>
                                {
                                    { "Mudkip", 13 },
                                    { "Mewtwo", 100 },
                                }
                            }
                        }
                    }
                });

                IDictionary<string, object> metadata = mutator.GetMetadata();

                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata, Has.Count.EqualTo(2));
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(metadata["bag"], Is.Not.Null);
                    Assert.That(metadata["team"], Is.Not.Null);

                    IDictionary<string, object>? bag = metadata.Get<IDictionary<string, object>>("bag");
                    IDictionary<string, object>? team = metadata.Get<IDictionary<string, object>>("team");

                    Assert.That(bag, Is.Not.Null);
                    Assert.That(bag, Has.Count.EqualTo(1));

                    IDictionary<string, object>? pokeballs = bag.Get<IDictionary<string, object>>("pokéballs");
                    
                    Assert.That(pokeballs, Is.Not.Null);
                    Assert.That(pokeballs, Has.Count.EqualTo(3));

                    Assert.That(pokeballs, Contains.Key("pokéball").WithValue(10));
                    Assert.That(pokeballs, Does.Not.ContainKey("greatball"));
                    Assert.That(pokeballs, Contains.Key("ultraball").WithValue(21));
                    Assert.That(pokeballs, Contains.Key("netball").WithValue(1));

                    Assert.That(team, Is.Not.Null);
                    Assert.That(team, Has.Count.EqualTo(3));

                    Assert.That(team, Contains.Key("Squirtle").WithValue(15));
                    Assert.That(team, Contains.Key("Mudkip").WithValue(13));
                    Assert.That(team, Contains.Key("Mewtwo").WithValue(100));
                    
                    Assert.That(logger.DebugLogs, Has.One.Contains("Metadata key 'bag' immediate"));
                    Assert.That(logger.DebugLogs, Has.One.Contains("Metadata key 'team' immediate"));
                }
            }
        }
    }
}