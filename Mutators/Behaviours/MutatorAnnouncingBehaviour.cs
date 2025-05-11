using Mutators.Managers;
using TMPro;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class MutatorAnnouncingBehaviour : SemiUI
    {
        private TextMeshProUGUI Text;
        internal static MutatorAnnouncingBehaviour instance;

        public override void Start()
        {
            base.Start();
            Text = GetComponent<TextMeshProUGUI>();
            instance = this;
            Text.text = MutatorManager.Instance.CurrentMutator.Name;
        }

        public override void Update()
        {
            base.Update();
        }

    }
}
