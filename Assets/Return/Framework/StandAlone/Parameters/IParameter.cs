using System;

namespace Return
{
    public interface IParameter<T> : IValue<T>
    {
        event Action<T> OnValueChange;
    }
}