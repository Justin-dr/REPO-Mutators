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
        private bool _isVisible = true;

        public override void Start()
        {
            animateTheEntireObject = true;
            hidePosition = new Vector2(300, 0);
            base.Start();
            Text = GetComponent<TextMeshProUGUI>();
            instance = this;
            Text.text = "No valuables spawn; weapons spawn instead. Enemy respawn time is reduced to 10 seconds, and the orb drop cap is removed.";
        }

        public override void Update()
        {
            base.Update();
            if (!ChatManager.instance.StateIsActive() && Input.GetKeyDown(RepoMutators.Settings.MutatorDisplayToggleKey))
            {
                _isVisible = !_isVisible;
            }
            if (!_isVisible)
            {
                base.Hide();
            }
        }

    }
}
