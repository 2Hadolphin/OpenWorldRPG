#define UniTask

using UnityEngine;
using Return;
using System;
using Cysharp.Threading.Tasks;

namespace Return.Audios
{
#if UniTask
    public static class AudioSourceExtension
    {


        /// <summary>
        /// Make audio source attach to target position.
        /// </summary>
        /// <param name="target">Transform to follow.</param>
        /// <param name="time">
        /// >0  =>  attach with time
        /// <0  =>  attach always
        /// =0  =>  attach once
        /// </param>
        public static AudioSource AttachTo(this AudioSource source, Transform target, float time, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            source.Attach(target, time, timing);
            return source;
        }


        static async void Attach(this AudioSource source, Transform target, float time,PlayerLoopTiming timing=PlayerLoopTiming.Update)
        {
            if (time == 0)
            {
                source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
                await UniTask.NextFrame(timing);
                source.AttachTo(target);
                return;
            }

            Transform tf = source.transform;

            Func<bool> valid;


            source.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;

            if (time > 0)
            {
                valid = () =>
                {
                    time -= ConstCache.deltaTime;
                    return time > 0;
                };
            }
            else
            {
                valid = () =>
                {
                    time += ConstCache.deltaTime;
                    return source.isPlaying && time<999;
                };
            }

            do
            {
                if (source == null || target == null)
                    return;

                tf.Copy(target);

                await UniTask.NextFrame(timing);
                time -= ConstCache.deltaTime;
            }
            while (valid());
        }
    }

#endif
}