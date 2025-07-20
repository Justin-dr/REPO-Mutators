using Mutators.Managers;
using TMPro;
using UnityEngine;

namespace Mutators.Mutators.Behaviours.UI
{
    internal class MutatorAnnouncingBehaviour : SemiUI
    {
        private TextMeshProUGUI Text;
        internal static MutatorAnnouncingBehaviour instance;
        private bool _isVisible = true;
        private float _showTimer = IsToggleWithDescription() ? RepoMutators.Settings.MutatorDescriptionInitialDisplayTime : 0f;

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
            if (RepoMutators.Settings.MutatorDisplayToggleType == Settings.ModSettings.MutatorNameToggleType.WithDescription)
            {
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
                return;
            }

            if (!ChatManager.instance.StateIsActive() && RepoMutators.Settings.MutatorDisplayToggleType == Settings.ModSettings.MutatorNameToggleType.Keybind && Input.GetKeyDown(RepoMutators.Settings.MutatorDisplayToggleKey))
            {
                _isVisible = !_isVisible;
            }
            if (!_isVisible)
            {
                Hide();
            }
        }

        private static bool IsToggleWithDescription()
        {
            return RepoMutators.Settings.MutatorDisplayToggleType == Settings.ModSettings.MutatorNameToggleType.WithDescription;
        }
    }
}
