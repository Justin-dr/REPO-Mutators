using TMPro;
using UnityEngine;

namespace Mutators.Mutators.Behaviours.UI
{
    internal class MutatorDescriptionAnnouncingBehaviour : SemiUI
    {
        public TextMeshProUGUI Text { get; private set; }
        public static MutatorDescriptionAnnouncingBehaviour Instance { get; private set; } = null!;
        private float _showTimer = RepoMutators.Settings.MutatorDescriptionInitialDisplayTime;

        public override void Start()
        {
            animateTheEntireObject = true;
            hidePosition = new Vector2(400, 0);
            base.Start();
            Text = GetComponent<TextMeshProUGUI>();
            Instance = this;
            Text.text = string.Empty;
        }

        public override void Update()
        {
            base.Update();
            Hide();
            if (_showTimer > 0f)
            {
                if (!RepoMutators.Settings.MutatorDescriptionPinned)
                {
                    _showTimer -= Time.deltaTime;
                }

                if (MenuPageEsc.instance?.menuPage == null)
                {
                    Show();
                }
            }
        }
        public void ShowDescription(float showTimerOverride = 0f)
        {
            SemiUISpringShakeY(20f, 10f, 0.3f);
            SemiUISpringScale(0.4f, 5f, 0.2f);

            float configShowTimer = RepoMutators.Settings.MutatorDescriptionInitialDisplayTime;
            _showTimer = showTimerOverride > configShowTimer ? showTimerOverride : configShowTimer;
        }

        private string GetDescription(string description)
        {
            return !description.Contains("{specialActionKey}") ? description : description.Replace("{specialActionKey}", RepoMutators.Settings.SpecialActionKey.ToString());
        }
        
        internal void SetText(string text)
        {
            Text.text = GetDescription(text);
        }
    }
}
