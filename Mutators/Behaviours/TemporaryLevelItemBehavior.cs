using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class TemporaryLevelItemBehavior : MonoBehaviour
    {
        internal PhysGrabObject _physGrabObject = null!;
        internal string levelName = null!;
        private float _countdown = 5f;

        void Awake()
        {
            _physGrabObject = GetComponent<PhysGrabObject>();
            ItemAttributes itemAttributes = GetComponent<ItemAttributes>();
            if (itemAttributes != null)
            {
                itemAttributes.itemName += " (Temporary)";
            }
            levelName = RunManager.instance.levelCurrent.name;
        }

        void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && !SemiFunc.RunIsLevel() && levelName != RunManager.instance.levelCurrent.name)
            {

                if (_countdown > 0)
                {
                    _countdown -= Time.deltaTime;
                }
                else
                {
                    _physGrabObject.DestroyPhysGrabObject();
                }
            }
        }
    }
}
