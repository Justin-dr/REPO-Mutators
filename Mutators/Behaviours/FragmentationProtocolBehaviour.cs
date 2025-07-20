using System.Collections.Generic;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    public class FragmentationProtocolBehaviour : MonoBehaviour
    {
        private EnemyParent _enemyParent;
        private ISet<EnemyParent> _fragmentations = new HashSet<EnemyParent>();

        internal float FragmentWindow { get; set; } = 0;

        internal bool IsInFragmentWindow => FragmentWindow > 0;
        void Awake()
        {
            _enemyParent = GetComponent<EnemyParent>();
        }

        void Update()
        {
            if (FragmentWindow > 0)
            {
                FragmentWindow -= Time.deltaTime;
            }
        }

        public void AddFragmentation(EnemyParent fragmentation)
        {
            _fragmentations.Add(fragmentation);
        }

        internal ISet<EnemyParent> GetFragmentations()
        {
            return _fragmentations;
        }
    }
}
