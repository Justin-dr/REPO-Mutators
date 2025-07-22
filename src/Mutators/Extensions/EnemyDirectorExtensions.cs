using System;
using System.Collections.Generic;
using System.Linq;
using Mutators.Settings;

namespace Mutators.Extensions
{
    internal static class EnemyDirectorExtensions
    {
        public static void DisableEnemies(this EnemyDirector enemyDirector,
            EnemyDisablingMutatorSettings mutatorSettings, Predicate<EnemySetup>? condition = null)
        {
            Predicate<EnemySetup> predicate = setup =>
                (condition?.Invoke(setup) ?? false) ||
                setup.spawnObjects.Any(so =>
                    mutatorSettings.ExcludedEnemies.Any(excluded =>
                        excluded.Equals(so.Prefab.GetComponent<EnemyParent>()?.enemyName,
                            StringComparison.OrdinalIgnoreCase)));

            IList<EnemySetup> setupsToRemove = enemyDirector.enemiesDifficulty1
                .Concat(enemyDirector.enemiesDifficulty2)
                .Concat(enemyDirector.enemiesDifficulty3)
                .Where(setup => predicate(setup))
                .ToList();

            foreach (EnemySetup setup in setupsToRemove)
            {
                enemyDirector.enemiesDifficulty1.Remove(setup);
                enemyDirector.enemiesDifficulty2.Remove(setup);
                enemyDirector.enemiesDifficulty3.Remove(setup);
            }
        }
    }
}