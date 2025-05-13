using Mutators.Managers;
using Mutators.Settings;
using TMPro;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class MutatorAnnouncingBehaviour : SemiUI
    {
        private TextMeshProUGUI Text;
        internal static MutatorAnnouncingBehaviour instance;
        private bool _isVisible = true;

        public override void Start()
        {
            animateTheEntireObject = true;
            hidePosition = new Vector2(300, 0);
            base.Start();
            Text = GetComponent<TextMeshProUGUI>();
            instance = this;
            Text.text = MutatorManager.Instance.CurrentMutator.Name;
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(RepoMutators.Settings.MutatorDisplayToggleKey))
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
