using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Mutators.Mutators.Behaviours.UI
{
    public class TargetPlayerAnnouncingBehaviour : SemiUI
    {
        public TextMeshProUGUI Text { get; private set; }
        public static TargetPlayerAnnouncingBehaviour instance;

        public void Awake()
        {
            instance = this;
            instance.doNotDisable = [];
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
