using HarmonyLib;
using Mutators.Network;
using Mutators.Settings;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Mutators.Mutators.Patches
{
    internal class SealedAwayPatch
    {
        private static uint _currentSpawns = 0;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ValuableObject))]
        [HarmonyPatch(nameof(ValuableObject.DollarValueSetLogic))]
        static void ValuableObjectDollarValueSetLogicPostfix(ValuableObject __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;


            __instance.physGrabObject.impactDetector.onDestroy.AddListener(new UnityAction(() => Spawn(__instance)));
        }

        private static void Spawn(ValuableObject valuableObject)
        {
            if (_currentSpawns >= MutatorSettings.SealedAway.MaximumMonsterSpawns) return;

            if (UnityEngine.Random.Range(0f, 100f) <= MutatorSettings.SealedAway.MonsterSpawnChance)
            {
                EnemyDirector enemyDirector = EnemyDirector.instance;

                EnemySetup[] setups = enemyDirector.enemiesDifficulty1
                    .Concat(enemyDirector.enemiesDifficulty2)
                    .Concat(enemyDirector.enemiesDifficulty3)
                    .Where(setup => setup.spawnObjects.Count == 1 && !setup.spawnObjects.Any(so => {
                        EnemyParent? enemyParent = so.GetComponent<EnemyParent>();

                        if (enemyParent == null || !(so.GetComponentInChildren<EnemyHealth>()?.spawnValuable ?? false))
                        {
                            return true;
                        }

                        return MutatorSettings.SealedAway.ExcludedEnemies.Any(excluded => excluded.Equals(enemyParent?.enemyName, StringComparison.OrdinalIgnoreCase));
                        })
                    ).ToArray();

                if (setups.Length == 0) return;

                EnemySetup enemySetup = setups[UnityEngine.Random.RandomRangeInt(0, setups.Length)];

                List<EnemyParent>? enemyParents = REPOLib.Modules.Enemies.SpawnEnemy(enemySetup, valuableObject.transform.position, Quaternion.identity, false);

                if (enemyParents != null)
                {
                    PlayerAvatar lastGrabber = valuableObject.physGrabObject.lastPlayerGrabbing;

                    foreach (EnemyParent enemyParent in enemyParents)
                    {
                        RepoMutators.Logger.LogDebug($"Valuable broken - Spawned {enemyParent.enemyName}");

                        EnemyVision? enemyVision = enemyParent.Enemy.GetComponent<EnemyVision>();
                        EnemyHealth? enemyHealth = enemyParent.Enemy.GetComponent<EnemyHealth>();

                        if (enemyHealth != null)
                        {
                            enemyHealth.health /= 2;
                            enemyHealth.healthCurrent = enemyHealth.health;
                            enemyHealth.spawnValuableMax = int.MaxValue;
                        }

                        if (SemiFunc.IsMultiplayer())
                        {
                            PhotonView photonView = enemyParent.GetComponent<PhotonView>();

                            if (photonView != null)
                            {
                                MutatorsNetworkManager.Instance.SendScaleChange(photonView.ViewID, 0.6f);
                            }
                        }
                        else
                        {
                            enemyParent.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                        }

                        // This block under the if check seems to be needed to prevent Enemy Vision errors.
                        if (enemyVision == null) continue;

                        enemyVision.enabled = false;

                        MutatorsNetworkManager.Instance.Run(ActivateVisionLater(enemyVision, lastGrabber));
                    }

                    _currentSpawns++;
                }
            }
        }

        private static IEnumerator ActivateVisionLater(EnemyVision enemyVision, PlayerAvatar? playerAvatar)
        {
            yield return new WaitForSeconds(0.5f);

            enemyVision.enabled = true;

            if (playerAvatar && playerAvatar != null)
            {
                enemyVision.Enemy.SetChaseTarget(playerAvatar);
            }
        }

        private static void BeforeUnpatchAll()
        {
            _currentSpawns = 0;
            RepoMutators.Logger.LogDebug($"[Sealed Away] Set tracked monster spawns to {_currentSpawns}");
        }
    }
}
