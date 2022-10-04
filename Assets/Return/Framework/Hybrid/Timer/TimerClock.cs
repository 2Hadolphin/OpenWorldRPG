using System.Collections;
using UnityEngine;
using System;

namespace Return.Timers
{
    public class TimerClock : NullCheck, IAlarm
    {
        public TimerClock(float time)
        {
            Qualify = time > 0;
            DeltaTime = time;
        }

        public event Action<int> OnDispose;

        bool Qualify;

        public float DeltaTime;

        private event Action m_Bell;

        public event Action Bell
        {
            add => m_Bell += value;
            remove
            {
                m_Bell -= value;

                if (m_Bell == null)
                    Dispose();
            }
        }


        public event Action<float> m_GapBell;

        public event Action<float> GapBell
        {
            add => m_GapBell += value;
            remove
            {
                m_GapBell -= value;

                if (m_GapBell == null)
                    Dispose();
            }
        }



        public IEnumerator StartTick()
        {
            var wait = new WaitForSeconds(DeltaTime);

            while (Qualify)
            {
                yield return wait;
                m_Bell?.Invoke();
                m_GapBell?.Invoke(DeltaTime);
            }

            yield break;
        }

        public void Dispose()
        {
            if (m_Bell != null || m_GapBell != null)
            {
                Debug.Log("Failure to dispose timer due to user still subscribe event.");
                return;
            }

            Qualify = false;
            OnDispose?.Invoke(Mathf.RoundToInt(1f / DeltaTime));
        }
    }

}