using System;
using Cysharp.Threading.Tasks;
using Return.Framework.Parameter;

namespace Return
{
    [Serializable]
    public class Parameter<T> : Parameters<T>, IParameter<T>
    {
        public Parameter() { }

        public Parameter(Func<T> _setter)
        {
            getter = _setter;
        }

        public T Value { get => m_Value; set => m_Value = value; }

        bool hasUpdate;

        public override event Action<T> OnValueChange;

        public Func<T> getter;

        public override T GetValue()
        {
            if (!hasUpdate.SetAs(true))
                return m_Value;

            if (getter.NotNull())
                m_Value = getter();

            Clean();

            return m_Value;
        }

        async void Clean()
        {
            await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
            hasUpdate = false;
        }

    }
}