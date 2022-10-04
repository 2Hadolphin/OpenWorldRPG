using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace Return
{
    /// <summary>
    /// Simple UTag container for developer to select.
    /// </summary>
    [System.Serializable]
    [HideLabel]
    public struct UTagPicker
    {
#if UNITY_EDITOR

        [HideInInspector]
        public bool HideDefinition;

        [SerializeField]
        public UniversalTagDefinition Definition;

#endif


        [SerializeField][HideInInlineEditors]
        public UTag Tag;

        public static implicit operator UTag(UTagPicker universalTagPicker)
        {
            return universalTagPicker.Tag;
        }
    }
}

