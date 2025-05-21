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
        private float _showTimer;

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
            Hide();
            if (_showTimer > 0f)
            {
                _showTimer -= Time.deltaTime;
                Show();
            }
        }
        public void ShowDescription()
        {
            SemiUISpringShakeY(20f, 10f, 0.3f);
            SemiUISpringScale(0.4f, 5f, 0.2f);
            _showTimer = 5f;
        }

    }
}
