using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace Return.Motions
{
    public partial class MotionSystem_Humanoid /*MxM Result*/: IMxMRootMotion
    {
        [TabGroup(MxM)]
        public bool EnableRootMotion = true;



        protected virtual void Register_MxMRootmotion()
        {
            mxm.OnIdleTriggered.AddListener(new UnityEngine.Events.UnityAction(OnIdleTag));
        }

        void OnIdleTag()
        {
            //mxm.SetFavourTag("IdleState",1f);
            Debug.Log("On mxm character idle.");
        }

        /// <summary>
        /// RegisterHandler ?? 
        /// </summary>
        protected virtual void Activate_MxMRootMotion()
        {
            //m_Trajectory = GetComponentInChildren<MxMTrajectoryGenerator>();
        }

        #region IMxMRootMotion (Controller Wrapper)

        public void HandleAngularErrorWarping(Quaternion a_warpRot)
        {
            Debug.Log("HandleAngularErrorWarping" + a_warpRot);
            if (Motor.NotNull())
            {
                Motor.RotateCharacter(transform.rotation * a_warpRot);
            }
            else
            {
                Transform.Rotate(a_warpRot.normalized.eulerAngles);
            }
        }

        public void Translate(Vector3 a_delta)
        {
            Motor.MoveCharacter(Transform.position + a_delta);
        }

        public void Rotate(Vector3 a_axis, float a_angle)
        {
            var rot=Quaternion.AngleAxis(a_angle, a_axis);
            Motor.RotateCharacter(rot * Transform.rotation);
        }

        public void HandleRootMotion(Vector3 a_rootDelta, Quaternion a_rootRotDelta, Vector3 a_warp, Quaternion a_warpRot, float a_deltaTime)
        {
            var moveDelta = a_rootDelta + a_warp;
            var rotDelta = a_rootRotDelta * a_warpRot;

            if (!EnableRootMotion)
            {
                //Debug.Log("Freze RootMotion");

                AnimationDeltaPosition = moveDelta;
                AnimationDeltaRotation = rotDelta;

                return;
            }

            //moveDelta.LerpTo(Parameters.GetVector3(Preset.ModuleVelocity), 0.8f);

            if (Motor.NotNull() || !Motor.enabled )
            {
                if (EnableGravity)
                {
                    moveDelta.y = UnityEngine.Physics.gravity.y * a_deltaTime * a_deltaTime;
                }

                Transform.Translate(moveDelta, Space.World);
                Transform.rotation *= rotDelta;
            }
            else 
            {

                //Motor.SetRotation(rotDelta*ReadOnlyTransform.rotation);
                moveDelta += AnimationDeltaPosition;
                rotDelta *= AnimationDeltaRotation;
                //Motor.SetTransientPosition(Motor.TransientPosition + moveDelta);
                //Motor.MoveCharacter(ReadOnlyTransform.position + moveDelta);

      
            }

            AnimationDeltaPosition = moveDelta;
            AnimationDeltaRotation = rotDelta;


            return;


            //rotDelta = rotDelta * ReadOnlyTransform.rotation;
            AnimationDeltaPosition = moveDelta;
            AnimationDeltaRotation = rotDelta;

            //return;
            Motor.MoveCharacter(Transform.position + moveDelta);
            Motor.RotateCharacter(Transform.rotation * a_rootRotDelta * a_warpRot);
            //Debug.Log("Rootmotion"+ ReadOnlyTransform.rotation*(a_rootRotDelta * a_warpRot).eulerAngles);
            //ReadOnlyTransform.Translate(moveDelta, Space.World);
            //ReadOnlyTransform.rotation *= a_rootRotDelta * a_warpRot;

        }

        public void SetPosition(Vector3 a_position)
        {
            //Parameters.SetVector3(Preset.AnimationVelocity, a_position);
            //Motor.SetPosition(a_position);
            Debug.LogError("SetPosition");
        }
        public void SetPositionAndRotation(Vector3 a_position, Quaternion a_rotation)
        {
            SetPositionAndRotation(a_position, a_rotation,true);
        }


        public void SetRotation(Quaternion a_rotation)
        {
            //Motor.SetRotation(a_rotation);
            Debug.LogError("SetRotation");
            //Parameters.SetQuaternion(Preset.AnimationDeltaRotation, a_rotation);
        }

        #endregion


    }
}