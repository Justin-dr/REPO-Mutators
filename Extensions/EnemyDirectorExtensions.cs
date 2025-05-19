using Mutators.Managers;
using Mutators.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutators.Extensions
{
    internal static class EnemyDirectorExtensions
    {
        internal static void DisableEnemies(this EnemyDirector enemyDirector, Predicate<EnemySetup>? condition = null)
        {
            Predicate<EnemySetup> predicate;

            if (MutatorManager.Instance.CurrentMutator.Settings is EnemyDisablingMutatorSettings enemyDisablingMutatorSettings)
            {
                predicate = setup =>
                    (condition?.Invoke(setup) ?? false) ||
                    setup.spawnObjects.Any(so =>
                        enemyDisablingMutatorSettings.ExcludedEnemies.Any(excluded =>
                            excluded.Equals(so.GetComponent<EnemyParent>()?.enemyName, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                predicate = condition ?? (_ => true);
            }

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
