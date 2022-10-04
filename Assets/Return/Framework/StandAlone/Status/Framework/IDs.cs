using System.Collections;
using Return.Database;
using UnityEngine;

namespace Return.Framework.Stats
{
    /// <summary>
    /// Recognizable unit.
    /// </summary>
    public abstract partial class IDs : DataEntity
    {
        [Tooltip("Display name on the ID Selection Context Button")]
        [SerializeField]
        string m_DisplayName;

        [Tooltip("Integer value to IDs IDs")]
        [SerializeField]
        int m_ID;


        public static implicit operator int(IDs reference) => reference != null ? reference.ID : 0; //  =>  reference.ID;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(DisplayName)) 
                DisplayName = name;
        }




        public string DisplayName { get => m_DisplayName; set => m_DisplayName = value; }
        public int ID { get => m_ID; set => m_ID = value; }
    }

}