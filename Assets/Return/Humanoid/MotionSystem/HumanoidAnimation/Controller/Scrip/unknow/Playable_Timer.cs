using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System;

namespace Return.Humanoid.Animation
{
    public class Playable_Timer : PlayableBehaviour, ITimer
    {
        public IAnimationBehaviour ClockTarget;
        protected float TargetTime;
        protected float _TimeLeft;
        public event Action<float> TimeLeft;
        /// <summary>
        /// 1 to 0
        /// </summary>
        public event Action<float> TimeLeft_Rate;

        public event EventHandler TimeUp;

        public override void PrepareData(Playable playable, FrameData info)
        {
            _TimeLeft -= info.deltaTime;

            if (_TimeLeft > 0f)
            {
                TimeLeft?.Invoke(_TimeLeft);
                TimeLeft_Rate?.Invoke(_TimeLeft / TargetTime);
            }
            else
            {
                TimeUp?.Invoke(this, null);
                playable.Destroy();
            }
        }

        public override void OnPlayableCreate(Playable playable)
        {
            _TimeLeft = TargetTime;
        }
    }

    public interface ITimer
    {
        public event Action<float> TimeLeft;
        public event Action<float> TimeLeft_Rate;
        public event EventHandler TimeUp;
    }
}
