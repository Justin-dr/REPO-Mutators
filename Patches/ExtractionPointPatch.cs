using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators.Behaviours;
using Mutators.Network;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(ExtractionPoint))]
    internal class ExtractionPointPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ExtractionPoint.ActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruck))]
        static void ExtractionPointActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruckPostfix()
        {
            string currentMutatorName = MutatorManager.Instance.CurrentMutator.Name;

            RepoMutators.Logger.LogInfo("Checking to send big UI message");
            if (currentMutatorName != Mutators.Mutators.NopMutator)
            {
                RepoMutators.Logger.LogInfo("Sending mutator message");
                MutatorsNetworkManager.Instance.SendBigMessage(currentMutatorName, "{!}");
            }

            test();
        }


        private static void test()
        {
            GameObject hud = GameObject.Find("Game Hud");
            GameObject health = GameObject.Find("Health");
            GameObject mutatorObject = new GameObject("Mutator");

            mutatorObject.transform.SetParent(hud.transform, false);
            TextMeshProUGUI textMeshPro = mutatorObject.AddComponent<TextMeshProUGUI>();

            TextMeshProUGUI healthTextMesh = health.GetComponent<TextMeshProUGUI>();

            textMeshPro.font = healthTextMesh.font;
            textMeshPro.fontMaterial = healthTextMesh.fontMaterial;

            RectTransform rectTransform = mutatorObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
            rectTransform.anchorMin = new Vector2(1, 1); // top-left
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);

            rectTransform.anchoredPosition = new Vector2(0, -75);
            textMeshPro.alignment = TextAlignmentOptions.Right;

            mutatorObject.AddComponent<MutatorAnnouncingBehaviour>();
            textMeshPro.text = MutatorManager.Instance.CurrentMutator.Name;
            textMeshPro.fontSize = 30;
            textMeshPro.enabled = true;
        }
    }
}
