using Mutators.Mutators.Behaviours.UI;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Mutators.Utility
{
    internal static class DescriptionUtils
    {
        internal enum DescriptionReplacementType
        {
            REPLACE,
            PREPEND,
            APPEND
        }

        internal static IEnumerator LateUpdateDescription(string description, DescriptionReplacementType replacementType = DescriptionReplacementType.REPLACE)
        {
            while (!MutatorDescriptionAnnouncingBehaviour.Instance)
            {
                yield return new WaitForSeconds(0.1f);
            }

            TextMeshProUGUI text = MutatorDescriptionAnnouncingBehaviour.Instance.Text;
            switch (replacementType)
            {
                case DescriptionReplacementType.REPLACE:
                    text.text = description;
                    break;
                case DescriptionReplacementType.PREPEND:
                    text.text = description + "\n" + text.text;
                    break;
                case DescriptionReplacementType.APPEND:
                    text.text += $"\n{description}";
                    break;
                default:
                    break;
            }
            
        }
    }
}
