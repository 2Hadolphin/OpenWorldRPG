using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using Return.Agents;

namespace Return.InteractSystem
{
    /// <summary>
    /// Cache interact object and sender
    /// </summary>
    public class InteractWrapper
    {
        protected InteractWrapper() { }

        public InteractWrapper(object interactable)
        {
            Interactable = interactable;
            hash = interactable == null ? 0 : interactable.GetHashCode();
        }


        public object Interactable;

        /// <summary>
        /// ??
        /// </summary>
        public int hash;

        public IAgent Agent;
    }

    public class BaseInteractMission : InteractWrapper
    {


        public bool valid { get; protected set; }

        public virtual event Action OnMissionDone;
        public virtual event Action OnMissionCancel;
        public virtual event Action<float> OnProgressChanged;

        public virtual void Finish()
        {
            OnMissionDone?.Invoke();
        }

        public virtual void Cancel()
        {
            valid = false;
            OnMissionCancel?.Invoke();
        }

        public virtual async UniTask Start(float time=-1)
        {
            valid = true;

            if (time < 0)
            {
                Finish();
                return;
            }


            using var clock = new TimeClock(time, OnProgressChanged);

            await clock.ToUniTask();

            if (!valid)
                return;

            Finish();
        }


    }
}