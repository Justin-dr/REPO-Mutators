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
