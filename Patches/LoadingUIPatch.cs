using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators.Behaviours.UI;
using Mutators.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            if (MutatorManager.Instance.CurrentMutator?.HasSpecialAction ?? false)
            {
                GameObject energy = GameObject.Find("Energy");
                CreateSpecialActionText(hud, health, energy);
            }
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
            textMeshPro.lineSpacing = -50;
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

        private static void CreateSpecialActionText(GameObject hud, GameObject health, GameObject energy)
        {
            GameObject specialActionObject = new GameObject("SpecialAction");

            specialActionObject.transform.SetParent(hud.transform, false);
            TextMeshProUGUI specialActionTMP = specialActionObject.AddComponent<TextMeshProUGUI>();

            Zap(specialActionObject, energy);

            GameObject specialActionMaxObject = new GameObject("SpecialActionMax");
            specialActionMaxObject.transform.SetParent(specialActionObject.transform, false);

            TextMeshProUGUI specialActionMaxTMP = specialActionMaxObject.AddComponent<TextMeshProUGUI>();

            TextMeshProUGUI healthTextMesh = health.GetComponent<TextMeshProUGUI>();

            // Special Action
            specialActionTMP.font = healthTextMesh.font;
            specialActionTMP.fontWeight = healthTextMesh.fontWeight;
            specialActionTMP.fontMaterial = healthTextMesh.fontMaterial;
            specialActionTMP.color = Color.red;

            RectTransform specialActionRectTransform = specialActionObject.GetComponent<RectTransform>();
            specialActionRectTransform.sizeDelta = new Vector2(120, 50);
            specialActionRectTransform.anchorMin = new Vector2(0, 1);
            specialActionRectTransform.anchorMax = new Vector2(0, 1);
            specialActionRectTransform.pivot = new Vector2(0, 1);

            specialActionRectTransform.anchoredPosition = new Vector2(29f, RepoMutators.Settings.SpecialActionY);
            specialActionTMP.alignment = TextAlignmentOptions.Left;

            specialActionTMP.text = string.Empty;
            specialActionTMP.fontStyle = FontStyles.Bold;
            specialActionTMP.fontSize = 40;
            specialActionTMP.enabled = true;

            // Special Action Max
            specialActionMaxTMP.font = healthTextMesh.font;
            specialActionMaxTMP.fontMaterial = healthTextMesh.fontMaterial;
            specialActionMaxTMP.color = Color.red;

            RectTransform specialActionMaxRectTransform = specialActionMaxObject.GetComponent<RectTransform>();
            specialActionMaxRectTransform.sizeDelta = new Vector2(120, 50);
            specialActionMaxRectTransform.anchorMin = new Vector2(0, 1);
            specialActionMaxRectTransform.anchorMax = new Vector2(0, 1);
            specialActionMaxRectTransform.pivot = new Vector2(0, 1);

            specialActionMaxRectTransform.anchoredPosition = new Vector2(45, 5);
            specialActionMaxTMP.alignment = TextAlignmentOptions.Left;

            specialActionObject.AddComponent<SpecialActionAnnouncingBehaviour>();

            specialActionMaxTMP.text = string.Empty;
            specialActionMaxTMP.fontSize = 20;
            specialActionMaxTMP.enabled = true;
        }

        private static void Zap(GameObject specialActionObject, GameObject energy)
        {
            GameObject specialActionZap = new GameObject("SpecialActionZap");
            specialActionZap.transform.SetParent(specialActionObject.transform, false);

            Image image = energy.transform.Find("Zap").GetComponent<Image>();
            Image specialActionZapImage = specialActionZap.AddComponent<Image>();
            specialActionZapImage.sprite = image.sprite;
            specialActionZapImage.color = Color.red;

            RectTransform zapRectTransform = specialActionZap.GetComponent<RectTransform>();
            zapRectTransform.sizeDelta = new Vector2(25, 25);
            zapRectTransform.anchoredPosition = new Vector2(-75f, 0);
        }
    }
}
