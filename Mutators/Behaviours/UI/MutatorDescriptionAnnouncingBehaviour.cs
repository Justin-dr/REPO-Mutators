using Mutators.Managers;
using TMPro;
using UnityEngine;

namespace Mutators.Mutators.Behaviours.UI
{
    public class MutatorDescriptionAnnouncingBehaviour : SemiUI
    {
        public TextMeshProUGUI Text { get; private set; }
        public static MutatorDescriptionAnnouncingBehaviour Instance { get; private set; }
        private float _showTimer = RepoMutators.Settings.MutatorDescriptionInitialDisplayTime;

        public override void Start()
        {
            animateTheEntireObject = true;
            hidePosition = new Vector2(400, 0);
            base.Start();
            Text = GetComponent<TextMeshProUGUI>();
            Instance = this;
            Text.text = GetDescription();
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

        private string GetDescription()
        {
            string description = MutatorManager.Instance.CurrentMutator.Description;
            if (!description.Contains("{specialActionKey}"))
            {
                return description;
            }
            return description.Replace("{specialActionKey}", RepoMutators.Settings.SpecialActionKey.ToString());
        }
    }
}
