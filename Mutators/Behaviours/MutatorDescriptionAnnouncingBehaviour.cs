using Mutators.Managers;
using Mutators.Settings;
using TMPro;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class MutatorDescriptionAnnouncingBehaviour : SemiUI
    {
        private TextMeshProUGUI Text;
        internal static MutatorDescriptionAnnouncingBehaviour instance;
        private float _showTimer = RepoMutators.Settings.MutatorDescriptionInitialDisplayTime;

        public override void Start()
        {
            animateTheEntireObject = true;
            hidePosition = new Vector2(400, 0);
            base.Start();
            Text = GetComponent<TextMeshProUGUI>();
            instance = this;
            Text.text = MutatorManager.Instance.CurrentMutator.Description;
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
                Show();
            }
        }
        public void ShowDescription()
        {
            SemiUISpringShakeY(20f, 10f, 0.3f);
            SemiUISpringScale(0.4f, 5f, 0.2f);
            _showTimer = RepoMutators.Settings.MutatorDescriptionInitialDisplayTime;
        }

    }
}
