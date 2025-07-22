using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators.Patches;
using Mutators.Settings;
using UnityEngine;

namespace Mutators.Patches
{
    internal class MapBacktrackPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MapBacktrack), nameof(MapBacktrack.Backtrack), MethodType.Enumerator)]
        static IEnumerable<CodeInstruction> MapBacktrackBacktrackTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            FieldInfo truckDestinationField = AccessTools.Field(typeof(MapBacktrack), nameof(MapBacktrack.truckDestination)) ?? throw new Exception("MapBacktrack.truckDestination field not found");
            MethodInfo targetOverrideMethod = AccessTools.Method(typeof(MapBacktrackPatch), nameof(GetTargetPosition)) ?? throw new Exception($"{nameof(GetTargetPosition)} method not found");

            for (int i = 0; i < codes.Count - 1; i++)
            {
                CodeInstruction loadTruckDestination = codes[i];
                CodeInstruction storeTargetPosition = codes[i + 1];

                if (!loadTruckDestination.LoadsField(truckDestinationField) || !IsStoreLocal(storeTargetPosition))
                {
                    continue;
                }

                codes.InsertRange(i + 2, [
                    LoadStoredLocal(storeTargetPosition),
                    new CodeInstruction(OpCodes.Call, targetOverrideMethod),
                    new CodeInstruction(storeTargetPosition.opcode, storeTargetPosition.operand)
                ]);

                return codes;
            }

            throw new Exception("Could not find MapBacktrack truck target assignment");
        }

        private static Vector3 GetTargetPosition(Vector3 targetPosition)
        {
            if (!MutatorManager.Instance.HasCurrentMutator(MutatorSettings.UltraViolence.NamespacedName))
            {
                return targetPosition;
            }

            if (!UltraViolencePatch.ShouldHideFakeFinalState())
            {
                return targetPosition;
            }

            RoundDirector roundDirector = RoundDirector.instance;
            if (roundDirector.extractionPointCurrent)
            {
                return roundDirector.extractionPointCurrent.transform.position;
            }

            return PlayerController.instance.playerAvatarScript.LastNavmeshPosition;
        }

        private static bool IsStoreLocal(CodeInstruction instruction)
        {
            return instruction.opcode == OpCodes.Stloc ||
                   instruction.opcode == OpCodes.Stloc_S ||
                   instruction.opcode == OpCodes.Stloc_0 ||
                   instruction.opcode == OpCodes.Stloc_1 ||
                   instruction.opcode == OpCodes.Stloc_2 ||
                   instruction.opcode == OpCodes.Stloc_3;
        }

        private static CodeInstruction LoadStoredLocal(CodeInstruction storeInstruction)
        {
            if (storeInstruction.opcode == OpCodes.Stloc_0) return new CodeInstruction(OpCodes.Ldloc_0);
            if (storeInstruction.opcode == OpCodes.Stloc_1) return new CodeInstruction(OpCodes.Ldloc_1);
            if (storeInstruction.opcode == OpCodes.Stloc_2) return new CodeInstruction(OpCodes.Ldloc_2);
            if (storeInstruction.opcode == OpCodes.Stloc_3) return new CodeInstruction(OpCodes.Ldloc_3);
            if (storeInstruction.opcode == OpCodes.Stloc_S) return new CodeInstruction(OpCodes.Ldloc_S, storeInstruction.operand);
            return new CodeInstruction(OpCodes.Ldloc, storeInstruction.operand);
        }
    }
}
