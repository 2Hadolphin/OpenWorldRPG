using Return.Motions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Return.Humanoid.Motion
{
    public class MotionSystemIKGraphBehaviour : PlayableBehaviour
    {
        public static MotionSystemIKGraphBehaviour Create(MotionSystem_Humanoid motionSystem)
        {
            var sp =ScriptPlayable<MotionSystemIKGraphBehaviour>.Create(motionSystem.GetGraph, 1);

            var behaviour = sp.GetBehaviour();
            behaviour.MotionSystem = motionSystem;
            behaviour.GetPlayable = sp;

            return behaviour;
        }


        MotionSystem_Humanoid MotionSystem;
        public Playable GetPlayable { get; protected set; }

        Playable LastPlayable;
        public List<Playable> IKPhase = new();

        public virtual void SetSystem(MotionSystem_Humanoid  motionSystem)
        {
            MotionSystem = motionSystem;
        }

        /// <summary>
        /// Bind MotionSystem stream to IK playable
        /// </summary>
        public virtual void ConnecntSource()
        {
            if (IKPhase.Count == 0 && !LastPlayable.IsValid())
                GetPlayable.ConnectInput(0, MotionSystem.MotionStream, 0, 1);
        }

        public void AddIK(Playable playable)
        {
            if (!GetPlayable.IsValid()|| IKPhase.Contains(playable))
                return;

            if(IKPhase.Count==0&&!LastPlayable.IsValid())
                GetPlayable.InsertPlayable(MotionSystem.MotionStream, playable);
            else
            {
                GetPlayable.InsertPlayable(LastPlayable, playable);
                LastPlayable = playable;
            }
        }

    }
}