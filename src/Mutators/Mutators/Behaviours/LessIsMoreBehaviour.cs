using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class LessIsMoreBehaviour : MonoBehaviour
    {
        private ValuableObject valuableObject = null!;
        private PhysGrabObjectImpactDetector physGrabObjectImpactDetector = null!;
        private float realValue = 0;
        void Awake()
        {
            valuableObject = GetComponent<ValuableObject>();
            physGrabObjectImpactDetector = GetComponent<PhysGrabObjectImpactDetector>();
            realValue = valuableObject.dollarValueCurrent;
        }

        void Update()
        {
            if (realValue < valuableObject.dollarValueOriginal * 0.15)
            {
                physGrabObjectImpactDetector.DestroyObject();
            }
        }

        internal void SubtractValue(float value)
        {
            realValue -= value;
        }
    }
}
