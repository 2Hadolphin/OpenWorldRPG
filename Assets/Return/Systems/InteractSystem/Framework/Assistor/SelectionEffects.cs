using UnityEngine;
using EPOOutline;

namespace Return.InteractSystem
{
    /// <summary>
    /// Selection outline configs.
    /// </summary>
    public class SelectionEffects : PresetDatabase
    {
        #region Outlines
        [SerializeField]
        Outlinable m_Selected;

        public Outlinable Selected=> m_Selected;

        [SerializeField]
        Outlinable m_Locked;

        public Outlinable Locked => m_Locked;
        #endregion

        #region Selection
        [SerializeField]
        SelectionCorner m_Mark;

        public SelectionCorner Mark => m_Mark;

        #endregion
    }
}