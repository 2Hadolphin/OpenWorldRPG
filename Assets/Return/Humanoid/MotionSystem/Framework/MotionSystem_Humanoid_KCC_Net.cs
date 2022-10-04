using System;
using TNet;
using UnityEngine;

namespace Return.Motions
{
    public partial class MotionSystem_Humanoid /* KCC Nets */
    {


        #region Sync Motor

        [RFC]
        protected virtual void PushVelocity(Vector3 currentVelocity)
        {
            //if (isMine)
            //    tno.SendQuickly(nameof(PushVelocity), Target.Others, currentVelocity);
            MotorVelocity = currentVelocity;
            //Parameters.SetVector3(Preset.MotorVelocity, currentVelocity);
        }

        [RFC]
        protected virtual void PushRotation(Quaternion currentRotation)
        {
            //if (isMine)
            //    tno.SendQuickly(nameof(PushRotation), Target.Others, currentRotation);
            MotorRotation = currentRotation;
            //Parameters.SetQuaternion(Preset.MotorRotation, currentRotation);
        }


        #endregion

        #region Motor Warpper

        /// <summary>
        /// Apply motor position and rotation 
        /// </summary>
        [RFC]
        public virtual void SetPositionAndRotation(Vector3 pos, Quaternion rot, bool bypassInterpolation = true)
        {
            if (isMine)
                tno.Send(nameof(SetPositionAndRotation), Target.OthersSaved, pos, rot, bypassInterpolation);

            Debug.Log(string.Format("SetPosition  {0} and Rotation {1} ", pos, rot));
            Motor.SetPositionAndRotation(pos, rot, bypassInterpolation);
        }


        #endregion


        #region ControllerFunc

        [RFC]
        public virtual void ForceGround(bool stableGround = true, bool anyGround = true)
        {
            if (isMine)
                tno.Send(nameof(ForceGround), Target.OthersSaved, stableGround, anyGround);

            var grounded = Motor.GroundingStatus;
            grounded.FoundAnyGround = anyGround;
            grounded.IsStableOnGround = stableGround;
            //Parameters.SetValue(Preset.GroundingStatus, grounded);
        }

        [RFC]
        public virtual void ForceUnground(float time)
        {
            if (isMine)
                tno.Send(nameof(ForceUnground), Target.OthersSaved, time);

            Motor.ForceUnground(time);
            var grounded = Motor.GroundingStatus;
            grounded.FoundAnyGround = false;
            grounded.IsStableOnGround = false;
            grounded.GroundCollider = null;
            //Parameters.SetValue(Preset.GroundingStatus, grounded);
        }

        [RFC]
        public virtual void SetControllerBounds(float height = -1, float radius = -1)
        {
            if (isMine)
            {
                if (height < 0)
                    height = MathF.Max(0.2f, CharacterBounds.size.y);

                if (radius < 0)
                    radius = MathF.Max(m_defaultControllerRadius, (CharacterBounds.size.x + CharacterBounds.size.z) * 0.5f);

                tno.Send(nameof(SetControllerBounds), Target.OthersSaved, height, radius);
            }

            Motor.SetCapsuleDimensions(
                radius,
                height,
                height * 0.5f
                );

            if (isMine)
                CacheControllerData();
        }

        [RFC]
        public virtual void SetControllerHeight(float height = -1)
        {
            if (isMine)
            {
                if (height < 0)
                {
                    height = MathF.Max(0.2f, CharacterBounds.size.y);
                }

                tno.Send(nameof(SetControllerHeight), Target.OthersSaved, height);
            }


            Motor.SetCapsuleDimensions(
                p_currentControllerRadius,
                height,
                height * 0.5f
                );

            if (isMine)
                CacheControllerData();
        }

        [RFC]
        public virtual void SetControllerRadius(float radius = -1)
        {
            if (isMine)
            {

                if (radius < 0)
                {
                    radius = MathF.Max(m_defaultControllerRadius, (CharacterBounds.size.x + CharacterBounds.size.z) * 0.5f);
                }

                tno.Send(nameof(SetControllerRadius), Target.OthersSaved, radius);
            }


            Motor.SetCapsuleDimensions(
                radius,
                p_currentControllerHeight,
                m_defaultControllerCenter
                );


            if (isMine)
                CacheControllerData();
        }

        [RFC]
        public virtual void ResetControllerBounds()
        {
            if (isMine)
                tno.Send(nameof(ResetControllerBounds), Target.OthersSaved);

            Motor.SetCapsuleDimensions(
                m_defaultControllerRadius,
                m_defaultControllerHeight,
                m_defaultControllerCenter
                );

            if (isMine)
                CacheControllerData();
        }
        #endregion

    }
}