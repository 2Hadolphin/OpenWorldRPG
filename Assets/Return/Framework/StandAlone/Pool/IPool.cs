using UnityEngine;

namespace Return
{
    public interface IPool<T> where T: class//Component
    {

        T Request(bool forceGet = false);
        T Request(bool activate, bool forceGet);

        void Return(T item);
    }
}
