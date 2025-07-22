using TMPro;
using UnityEngine;

namespace Mutators.Mutators.Behaviours.UI
{
    /// <summary>
    /// The behaviour that controls the special action "stat" UI.
    /// </summary>
    public class SpecialActionAnnouncingBehaviour : SemiUI
    {
        /// <summary>
        /// The TextMeshProUGUI that controls the current value of the special action UI element.
        /// </summary>
        public TextMeshProUGUI Text { get; private set; } = null!;
        /// <summary>
        /// The TextMeshProUGUI that controls the maximum value of the special action UI element.
        /// </summary>
        public TextMeshProUGUI TextMax { get; private set; } = null!;
        
        /// <summary>
        /// The singleton instance of the <see cref="SpecialActionAnnouncingBehaviour"/>.
        /// </summary>
        public static SpecialActionAnnouncingBehaviour Instance { get; private set; } = null!;

        private void Awake()
        {
            Instance = this;
            Text = GetComponent<TextMeshProUGUI>();
            TextMax = transform.Find("SpecialActionMax").GetComponent<TextMeshProUGUI>();
        }

        public override void Start()
        {
            animateTheEntireObject = true;
            hidePosition = new Vector2(-140, 0);
            doNotDisable = [];
            base.Start();
            Text.text = string.Empty;
        }
    }
}
