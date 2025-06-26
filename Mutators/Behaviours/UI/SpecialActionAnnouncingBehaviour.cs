using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Mutators.Mutators.Behaviours.UI
{
    public class SpecialActionAnnouncingBehaviour : SemiUI
    {
        public TextMeshProUGUI Text { get; private set; }
        public TextMeshProUGUI TextMax { get; private set; }
        internal static SpecialActionAnnouncingBehaviour instance;

        public override void Start()
        {
            animateTheEntireObject = true;
            hidePosition = new Vector2(-140, 0);
            // Quick fix so I can wait for humidifier update
            AccessTools.DeclaredField(typeof(SemiUI), "doNotDisable").SetValue(this, new System.Collections.Generic.List<GameObject>());
            base.Start();
            Text = GetComponent<TextMeshProUGUI>();
            TextMax = transform.Find("SpecialActionMax").GetComponent<TextMeshProUGUI>();
            instance = this;
            Text.text = string.Empty;
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
