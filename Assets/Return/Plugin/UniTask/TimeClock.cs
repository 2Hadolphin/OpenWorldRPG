using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Cysharp.Threading.Tasks
{
    public class TimeClock:IDisposable
    {
        public TimeClock(float time,Action<float> callback)
        {
            fullTime = time;
            m_callback = callback;
        }

        public float fullTime { get; protected set; }
        public float curTime { get; protected set; }

        protected Action<float> m_callback;

        public async UniTask ToUniTask(float gapTime = -1)
        {
            while (curTime < fullTime)
            {
                if(gapTime>0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(gapTime));
                    curTime += gapTime;
                }
                else
                {
                    await UniTask.NextFrame();
                    curTime += Time.deltaTime;
                }

                m_callback?.Invoke(curTime / fullTime);
            }
        }

        public void Dispose()
        {
            m_callback = null;
        }
    }
}