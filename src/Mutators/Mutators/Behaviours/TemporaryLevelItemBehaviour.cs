using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class TemporaryLevelItemBehaviour : MonoBehaviour
    {
        internal PhysGrabObject _physGrabObject = null!;
        internal string levelName = null!;

        void Awake()
        {
            _physGrabObject = GetComponent<PhysGrabObject>();
            ItemAttributes itemAttributes = GetComponent<ItemAttributes>();
            if (itemAttributes != null)
            {
                itemAttributes.itemName += " (Temporary)";
            }
        }
    }
}
