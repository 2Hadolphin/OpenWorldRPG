using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Animations;
using Return.Humanoid.Animation;
using UnityEngine.Animations.Rigging;

namespace Return
{
    public partial struct Job_Humanoid_MuscleRigging : IAnimationJob
    {
        public Vector3 BodyPosition;
        public Quaternion BodyRotation;
        [SyncSceneToStream]
        public TransformStreamHandle Root;

        public MuscleData[] Datas;

        public void ProcessAnimation(AnimationStream stream)
        {

            var humanStream = stream.AsHuman();


            if (Root.IsValid(stream))//&& RootTransform.IsResolved(stream)
                Root.SetLocalTRS(stream, BodyPosition, BodyRotation, Vector3.one, false);


            if (Datas == null)
                return;

            var length = Datas.Length;
            for (int i = 0; i < length; i++)
            {
                //if (Datas[i].InheritIKOverwrite)
                //{

                //    Datas[i].m_Value = humanStream.GetMuscle(Datas[i].m_Handle);
                //    Datas[i].InheritIKOverwrite = false;

                //}
                //else
                humanStream.SetMuscle(Datas[i].m_Handle, Datas[i].m_Value);
            }




            // next m_duringTransit get muscle as anim cruve binding
        }

        public void ProcessRootMotion(AnimationStream stream)
        {
            var humanStream = stream.AsHuman();

            if (Root.IsValid(stream))//&& RootTransform.IsResolved(stream)
                Root.SetLocalTRS(stream, BodyPosition, BodyRotation, Vector3.one, false);

        }
    }
}