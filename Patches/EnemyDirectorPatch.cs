using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(EnemyDirector))]
    internal class EnemyDirectorPatch
    {
        private static List<EnemySetup> _startingEnemyList = new List<EnemySetup>();
        private static bool _isListSet;

        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        [HarmonyPatch(nameof(EnemyDirector.GetEnemy))] // Credit: https://github.com/SoundedSquash/REPO-SpawnManager/blob/5e9261bbd74952e47a986c6b7411c91c5edcf782/Patches/EnemyDirector.cs#L40
        static void EnemyDirectorGetEnemyPrefix(ref List<EnemySetup> ___enemyList, int ___enemyListIndex)
        {
            if (SemiFunc.IsMultiplayer() && SemiFunc.IsNotMasterClient()) return;
            ___enemyList.RemoveAll(x => x == null);

            if (!_isListSet) // Only run once.
            {
                _startingEnemyList = new List<EnemySetup>(___enemyList);
                _isListSet = true;
            }

            // Make sure at least one "enemy" exists.
            if (___enemyList.Count == 0)
            {
                var emptyEnemySetup = ScriptableObject.CreateInstance<EnemySetup>();
                emptyEnemySetup.spawnObjects = new List<PrefabRef>();
                ___enemyList.Add(emptyEnemySetup);
            }

            // Make sure the enemy list is long enough to prevent index out of range.
            while (___enemyList.Count < ___enemyListIndex + 1)
            {
                var idxToCopy = Random.Range(0, _startingEnemyList.Count);
                ___enemyList.Add(___enemyList[idxToCopy]);
            }
        }
    }
}
