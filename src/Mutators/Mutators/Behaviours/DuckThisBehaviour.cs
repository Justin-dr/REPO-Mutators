using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class DuckThisBehaviour : MonoBehaviour
    {
        public float NoticeCooldown { get; internal set; } = 0;

        private void Update()
        {
            if (NoticeCooldown > 0f && SemiFunc.IsMasterClientOrSingleplayer())
            {
                NoticeCooldown -= Time.deltaTime;
            }
        }

        internal bool CanNotice()
        {
            return NoticeCooldown <= 0f;
        }
    }
}
