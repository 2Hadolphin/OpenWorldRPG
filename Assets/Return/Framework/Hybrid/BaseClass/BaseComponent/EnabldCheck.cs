using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Return
{
    /// <summary>
    /// C# enable class.
    /// </summary>
    [Serializable]
    public abstract class EnabledCheck : NullCheck
    {
        //[ShowInInspector,ReadOnly]
        bool m_enabled = false;

#if UNITY_EDITOR
        [PropertyOrder(-888)]
        [OnInspectorGUI]
        void DrawEnable()
        {
            enabled=UnityEditor.EditorGUILayout.ToggleLeft(nameof(enabled), enabled);
        }
#endif
        /// <summary>
        /// Default as false.
        /// Set true to activate module.
        /// </summary>
#pragma warning disable IDE1006 
        public bool enabled
#pragma warning restore IDE1006
        {
            get => m_enabled;

            set
            {
                if (m_enabled.SetAs(value))
                {
                    //Debug.Log(this+" set active : "+ m_enabled);

                    if (value)
                        OnEnable();
                    else
                        OnDisable();
                }
            }
        }


        /// <summary>
        /// Invoke while enabled set as true.
        /// </summary>
        protected virtual void OnEnable() { }

        /// <summary>
        /// Invoke while enabled set as false.
        /// </summary>
        protected virtual void OnDisable() { }
    }
}