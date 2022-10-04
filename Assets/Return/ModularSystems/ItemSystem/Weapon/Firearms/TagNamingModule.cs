using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "NamingModuleTag", menuName = "Return/Item/NamingModuleTag", order = 2)]
public class TagNamingModule : ScriptableObject
{
    [SerializeField] private List<string> m_tagNames;


    public List<string> TagNames { get { return m_tagNames; } }

    public void OnEnable()
    {
        if (m_tagNames == null || m_tagNames.Count != 32)
        {
            if (m_tagNames == null)
                m_tagNames = new List<string>(33);

            m_tagNames.Clear();

            for (int i = 1; i < 31; ++i)
            {
                m_tagNames.Add("Tag " + i.ToString());
            }

            m_tagNames.Add("DoNotUse");
            m_tagNames.Add("Reserved");
        }
    }
}