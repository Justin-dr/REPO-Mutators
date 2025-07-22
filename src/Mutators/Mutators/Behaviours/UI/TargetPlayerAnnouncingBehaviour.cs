using TMPro;

namespace Mutators.Mutators.Behaviours.UI
{
    /// <summary>
    /// The behaviour that controls the current target UI.
    /// </summary>
    public class TargetPlayerAnnouncingBehaviour : SemiUI
    {
        /// <summary>
        /// The TextMeshProUGUI that controls the current target UI element.
        /// </summary>
        public TextMeshProUGUI Text { get; private set; }
        
        /// <summary>
        /// The singleton instance of the <see cref="TargetPlayerAnnouncingBehaviour"/>.
        /// </summary>
        public static TargetPlayerAnnouncingBehaviour Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Instance.doNotDisable = [];
            Text = GetComponent<TextMeshProUGUI>();
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            base.Update();
            if (Text.text == string.Empty)
            {
                Hide();
            }
        }
    }
}
