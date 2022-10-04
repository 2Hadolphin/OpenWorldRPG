using System;
using UnityEngine;


namespace VContainer
{
    [Serializable]
    public struct HandlerRegister : IRegister, IEquatable<HandlerRegister>
    {
        [SerializeField]
        UnityEngine.Object m_Object;
        public UnityEngine.Object Object { get => m_Object; set => m_Object = value; }

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


        public bool Equals(HandlerRegister other)
        {
            return Object == other.Object;
        }

        public override int GetHashCode()
        {
            return Object == null ? 0 : Object.GetHashCode();
        }

        public static implicit operator HandlerRegister(UnityEngine.Object obj)
        {
            return new HandlerRegister()
            {
                Object = obj,
            };
        }
    }


}