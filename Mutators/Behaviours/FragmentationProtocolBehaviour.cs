using System.Collections.Generic;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    public class FragmentationProtocolBehaviour : MonoBehaviour
    {
        private EnemyParent _enemyParent;
        private ISet<EnemyParent> _fragmentations = new HashSet<EnemyParent>();
        void Awake()
        {
            _enemyParent = GetComponent<EnemyParent>();
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
