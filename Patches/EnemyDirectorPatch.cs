using HarmonyLib;
using Mutators.Managers;
using Mutators.Settings;
using Photon.Pun;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Mutators.Patches
{

    internal class EnemyDirectorPatch
    {
        [HarmonyPatch(typeof(EnemyDirector))]
        [HarmonyPatch(nameof(EnemyDirector.AddEnemyValuable))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var target = typeof(EnemyDirectorPatch).GetMethod(nameof(OverrideOrbLimit), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];

                // Match ldc.i4.s 10 or ldc.i4 10 depending on compiler
                if ((code.opcode == OpCodes.Ldc_I4_S && (sbyte)code.operand == 10) ||
                    (code.opcode == OpCodes.Ldc_I4 && (int)code.operand == 10))
                {
                    // Replace with call to our conditional method
                    codes[i] = new CodeInstruction(OpCodes.Call, target);
                }
            }

            return codes;
        }

        private static int OverrideOrbLimit()
        {
            if (MutatorManager.Instance.CurrentMutator.Name == MutatorSettings.HuntingSeason.MutatorName)
            {
                return 100;
            }
            return 10;
        }
    }
}
