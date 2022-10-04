using System;
using UnityEngine;


namespace VContainer
{
    [Serializable]
    public struct TypeRegister : IRegister, IEquatable<TypeRegister>
    {
        //object IRegister.Object => null;

        [SerializeField]
        Lifetime m_LifeTime;
        public Lifetime LifeTime { get => m_LifeTime; set => m_LifeTime = value; }


        [SerializeField]
        Type m_CreateType;
        public Type CreateType { get => m_CreateType; set => m_CreateType = value; }

        /// <summary>
        /// Abstract type and Interfaces
        /// </summary>
        [SerializeField]
        Type[] m_bindingTypes;
        public Type[] BindingTypes { get => m_bindingTypes; set => m_bindingTypes = value; }


        public void Binding(IContainerBuilder builder)
        {
            //if (BindingTypes == null || BindingTypes.Length == 0)
            //    throw new KeyNotFoundException("Container register is null.");

            //Type typeToCreate = null;
            //var list = new List<Type>(BindingTypes.Length);

            //foreach (var type in register.BindingTypes)
            //{
            //    if (type.IsInterface || type.IsAbstract)
            //    {
            //        list.Add(type);
            //        continue;
            //    }

            //    Assert.IsNull(typeToCreate);

            //    typeToCreate = type;
            //}
            BindingType(builder, CreateType, LifeTime, BindingTypes);
        }

        public static void BindingType(IContainerBuilder builder, Type type,Lifetime lifetime,params Type[] interfaces)
        {
            if (interfaces != null && interfaces.Length > 0)
                builder.Register(type, lifetime).As(interfaces);
            else
                builder.Register(type, lifetime);
        }


        public bool Equals(TypeRegister other)
        {
            return CreateType == other.CreateType;
        }

        public override int GetHashCode()
        {
            return CreateType == null ? 0 : CreateType.GetHashCode();
        }
    }


}