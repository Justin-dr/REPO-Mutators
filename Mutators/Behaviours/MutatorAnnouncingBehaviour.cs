using Mutators.Managers;
using TMPro;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class MutatorAnnouncingBehaviour : SemiUI
    {
        private TextMeshProUGUI Text;
        internal MutatorAnnouncingBehaviour instance;
        internal float myShowTimer = 10f;

        public override void Start()
        {
            base.Start();
            Text = GetComponent<TextMeshProUGUI>();
            instance = this;
            Text.text = MutatorManager.Instance.CurrentMutator.Name;
            showTimer = 10f;
            //textMaxHealth = base.transform.Find("HealthMax").GetComponent<TextMeshProUGUI>();
        }

        public override void Update()
        {
            base.Update();
            if (myShowTimer > 0)
            {
                myShowTimer -= Time.deltaTime;
                return;
            }
            Hide();
        }

    }
}
