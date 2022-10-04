using UnityEngine;

namespace Return
{
    public interface IFactory<T> where T : class//Component
    {
        public T Create();
    }

}
