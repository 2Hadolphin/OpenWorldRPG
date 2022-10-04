using System;
using UnityEngine.Assertions;
using UnityEngine;

namespace Return
{
    /// <summary>
    /// Wrapper of unity object, which can use to register behaviour collection
    /// </summary>
    public class ClassWrapper : IEquatable<ClassWrapper>
    {
        protected ClassWrapper() { }

        public ClassWrapper(object system)
        {
            Assert.IsNotNull(system);
            System = system;
        }

        public readonly object System;

        public override int GetHashCode()
        {
            return System.GetType().GetHashCode();
        }

        public bool Equals(ClassWrapper other)
        {
            return ValueType == other.ValueType;
        }

        public virtual Type ValueType => System.GetType();

        

        /// <summary>
        /// Null CheckAdd
        /// </summary>
        /// <param name="wrapper"></param>
        public static implicit operator bool(ClassWrapper wrapper)
        {
            return wrapper.System is not null;
        }
    }

    /// <summary>
    /// Use this shell to using IEquatable interface
    /// </summary>
    public class ClassWrapperInjector : ClassWrapper
    {
        public ClassWrapperInjector(Type type)
        {
            Type = type;
        }
        private readonly Type Type;

        public override Type ValueType => Type;

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
    }
}