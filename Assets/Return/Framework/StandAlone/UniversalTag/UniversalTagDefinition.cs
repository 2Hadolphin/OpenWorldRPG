using Return.Database;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Return
{
    [Serializable]
    [CreateAssetMenu(fileName = "UniversalTagDefition", menuName = "Return/Module/TagModule", order = 1)]
    public class UniversalTagDefinition : PresetDatabase
    {
        [HideInInspector]
        [Tooltip("Is enable to multi selected.")]
        public bool IsFlag=true;

        [SerializeField,HideInEditorMode] 
        private List<string> m_tagNames;


//#if UNITY_EDITOR
//        [SerializeField]
//#endif
        public List<string> TagNames 
        {
            get 
            {
                SetTags();
                return m_tagNames; 
            }

#if UNITY_EDITOR
            set
            {
                m_tagNames = value;
            }
#endif
        }

        public string this[int index]
        {
            get
            {
                return TagNames[index];
            }
        }

        public void OnEnable()
        {
            SetTags();
        }

        void SetTags()
        {
            if (m_tagNames == null || m_tagNames.Count != 32)
            {
                if (m_tagNames == null)
                    m_tagNames = new List<string>(33);

                m_tagNames.Clear();

                for (int i = 1; i < 31; ++i)
                {
                    m_tagNames.Add("Tags " + i.ToString());
                }

                m_tagNames.Add("DoNotUse");
                m_tagNames.Add("Reserved");
            }
        }

        public bool Find(string tagName,out UTag tag)
        {
            var index = TagNames.IndexOf(tagName);

            var found = index > 0;

            tag = found ? (UTag)index+1 :default;

            return found;
        }
    }
}

