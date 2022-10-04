using System;
using UnityEngine;


namespace VContainer
{
    [Serializable]
    public struct DynamicRegister : IRegister, IEquatable<DynamicRegister>
    {
        public object Object { get ; set ; }

        /// <summary>
        /// Abstract type and Interfaces
        /// </summary>
        [SerializeField]
        Type[] m_bindingTypes;
        public Type[] BindingTypes { get => m_bindingTypes; set => m_bindingTypes = value; }


        public void Binding(IContainerBuilder builder)
        {
            builder.Binding(Object, BindingTypes);
        }

        public bool Equals(DynamicRegister other)
        {
            return Object == other.Object;
        }

        public override int GetHashCode()
        {
            return Object == null ? 0 : Object.GetHashCode();
        }
    }


}