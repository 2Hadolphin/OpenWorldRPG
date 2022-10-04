using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Return.Creature;

namespace Return.Humanoid.IK
{
    public abstract class TwoBoneIKDataArg : EventArgs,IDisposable
    {
        
        public TwoBoneIKDataArg()
        {
            GoalSpace = Space.World;
            GoalUpdate = true;
            GoalWeight = 1f;

            HintSpace = Space.World;
            HintUpdate = true;
            HintWeight = 1f;

        }

        /// <summary>
        /// Valid target coordinates(start/end), call form motion module 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="tip"></param>
        public virtual void Init(Transform root, Transform tip)
        {
            return;
        }



        public Limb Limb;
        public virtual event Action<TwoBoneIKDataArg> FinishMotion;


        #region Goal

        public virtual bool GoalUpdate { get; set; }
        public virtual float GoalWeight { get; set; }
        public virtual float GoalPositionWeight { get; set; } = 1f;
        public virtual float GoalRotationWeight { get; set; } = 1f;
        /// <summary>
        /// Local space align to root bone of IK, e.g. shoulder or upper arm
        /// </summary>
        public virtual Space GoalSpace { get; set; } 
        public virtual PR GoalPR { get; set; }
        //public abstract void LoadIKGoal(ReadOnlyTransform target);
        #endregion

        #region Hint

        public virtual bool HintUpdate { get; set; }
        public virtual float HintWeight { get; set; } = 1f;
        public virtual Space HintSpace { get; set; }
        public virtual Vector3 HintPosition { get; set; }

        //public abstract void LoadIKHint(ReadOnlyTransform hint);
        #endregion

        /// <summary>
        /// Return true if IK routine is still running
        /// </summary>
        /// <param name="time">time since last call</param>
        /// <returns></returns>
        public virtual bool Evaluate(float time)
        {
            FinishMotion?.Invoke(this);
            return true;
        }

        public virtual void Dispose()
        {
            FinishMotion = null;
        }
    }

    public class TwoBoneIKFrameArg : TwoBoneIKDataArg
    {

        //public override void LoadIKGoal(ReadOnlyTransform tf)
        //{
        //    switch (GoalSpace)
        //    {
        //        case Space.World:
        //            tf.SetWorldPR(GoalPR);
        //            break;
        //        case Space.Self:
        //            tf.SetLocalPR(GoalPR);
        //            break;
        //    }
        //}

        //public override void LoadIKHint(ReadOnlyTransform tf)
        //{
        //    switch (HintSpace)
        //    {
        //        case Space.World:
        //            tf.position = HintPosition;
        //            break;
        //        case Space.Self:
        //            tf.localPosition = HintPosition;
        //            break;
        //    }
        //}
    }

    [Serializable]
    public class TwoBoneIKTargetArg : TwoBoneIKDataArg
    {
        public TwoBoneIKTargetArg(float time):base()
        {
            Time = time;
            HintUpdate = false;

        }

        public override void Init(Transform root, Transform tip)
        {
            if(Start==null)
            {
                var offset_pos = root.InverseTransformPoint(tip.position);
                var offset_rot = root.InverseTransformRotation(tip.rotation);

                Debug.Log(offset_pos);
                Debug.Log(offset_rot);

                Start = new Coordinate_Offset(root)
                {
                    OffsetPosition = offset_pos,
                    OffsetRotation = offset_rot,
                    OffsetSpace = Space.Self,
                    OutputPositionSpace = Space.World
                };

            }

            End.OutputPositionSpace = Space.World;
        }


        public override event Action<TwoBoneIKDataArg> FinishMotion;
        #region Parameter
        [ShowInInspector]
        [ReadOnly]
        public ICoordinate Start;
        [ShowInInspector]
        [ReadOnly]
        public ICoordinate End;
        public readonly float Time;
        [ShowInInspector]
        [ReadOnly]
        protected float CurrentTime;

        public override Space GoalSpace { get; set; } = Space.World;

        public override bool Evaluate(float time)
        {
            CurrentTime += time;
            time = CurrentTime / Time;

            var pos = Vector3.Lerp( Start.position,End.position, time);
            var rot = Quaternion.Lerp(Start.rotation, End.rotation, time);
            GoalPR = new PR() { Position = pos, Rotation = rot };

            GoalUpdate = time <= 1f;

            if (!GoalUpdate)
            {
                Debug.Log(time);
                FinishMotion?.Invoke(this);
            }

            return GoalUpdate;
        }





        #endregion
    }

}

