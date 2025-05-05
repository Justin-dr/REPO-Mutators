using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class DuckThisBehaviour : MonoBehaviour
    {
        internal float _noticeCooldown = 0f;

        private void Update()
        {
            if (_noticeCooldown > 0f && SemiFunc.IsMasterClientOrSingleplayer())
            {
                _noticeCooldown -= Time.deltaTime;
            }
        }

        internal void OnNotice()
        {
            _noticeCooldown = 120f;
        }

        internal bool CanNotice()
        {
            return _noticeCooldown <= 0f;
        }
    }
}
