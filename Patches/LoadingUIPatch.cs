﻿using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators.Behaviours;
using TMPro;
using UnityEngine;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(LoadingUI))]
    internal class LoadingUIPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(nameof(LoadingUI.StopLoading))]
        static void LoadingUIStopLoadingPostfix()
        {
            if (!SemiFunc.RunIsLevel()) return;

            GameObject hud = GameObject.Find("Game Hud");
            GameObject health = GameObject.Find("Health");
            CreateMutatorText(hud, health);
            CreateMutatorDescriptionText(hud, health);
            CreateTargetPlayerText(hud, health);
        }


        private static void CreateMutatorText(GameObject hud, GameObject health)
        {
            GameObject mutatorObject = new GameObject("Mutator");

            mutatorObject.transform.SetParent(hud.transform, false);
            TextMeshProUGUI textMeshPro = mutatorObject.AddComponent<TextMeshProUGUI>();

            TextMeshProUGUI healthTextMesh = health.GetComponent<TextMeshProUGUI>();

            textMeshPro.font = healthTextMesh.font;
            textMeshPro.fontMaterial = healthTextMesh.fontMaterial;

            RectTransform rectTransform = mutatorObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);

            rectTransform.anchoredPosition = new Vector2(0, RepoMutators.Settings.MutatorDisplayY);
            textMeshPro.alignment = TextAlignmentOptions.Right;

            mutatorObject.AddComponent<MutatorAnnouncingBehaviour>();
            textMeshPro.text = MutatorManager.Instance.CurrentMutator.Name;
            textMeshPro.fontSize = RepoMutators.Settings.MutatorDisplaySize;
            textMeshPro.enabled = true;
        }

        private static void CreateMutatorDescriptionText(GameObject hud, GameObject health)
        {
            GameObject mutatorObject = new GameObject("Mutator Description");

            mutatorObject.transform.SetParent(hud.transform, false);
            TextMeshProUGUI textMeshPro = mutatorObject.AddComponent<TextMeshProUGUI>();

            TextMeshProUGUI healthTextMesh = health.GetComponent<TextMeshProUGUI>();

            textMeshPro.font = healthTextMesh.font;
            textMeshPro.fontMaterial = healthTextMesh.fontMaterial;

            RectTransform rectTransform = mutatorObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300, 100);
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);

            rectTransform.anchoredPosition = new Vector2(0, RepoMutators.Settings.MutatorDescriptionDisplayY);
            textMeshPro.alignment = TextAlignmentOptions.Right;
            textMeshPro.verticalAlignment = VerticalAlignmentOptions.Top;

            mutatorObject.AddComponent<MutatorDescriptionAnnouncingBehaviour>();
            textMeshPro.text = string.Empty;
            textMeshPro.fontSize = RepoMutators.Settings.MutatorDescriptionDisplaySize;
            textMeshPro.enabled = true;
        }

        private static void CreateTargetPlayerText(GameObject hud, GameObject health)
        {
            GameObject mutatorObject = new GameObject("ChosenPlayerTarget");

            mutatorObject.transform.SetParent(hud.transform, false);
            TextMeshProUGUI textMeshPro = mutatorObject.AddComponent<TextMeshProUGUI>();

            TextMeshProUGUI healthTextMesh = health.GetComponent<TextMeshProUGUI>();

            textMeshPro.font = healthTextMesh.font;
            textMeshPro.fontMaterial = healthTextMesh.fontMaterial;
            textMeshPro.color = healthTextMesh.color;

            RectTransform rectTransform = mutatorObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300, 50);
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);

            rectTransform.anchoredPosition = new Vector2(0, 10);
            textMeshPro.alignment = TextAlignmentOptions.Midline;

            mutatorObject.AddComponent<TargetPlayerAnnouncingBehaviour>();
            textMeshPro.text = string.Empty;
            textMeshPro.fontSize = RepoMutators.Settings.TargetDisplaySize;
            textMeshPro.enabled = true;
        }
    }
}
