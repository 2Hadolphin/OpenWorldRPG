using UnityEngine;
using Return.Agents;
using Return.Cameras;
using Return.Humanoid;
using System;
using Return.Animations.IK;
using Return.Modular;

namespace Return.Items
{
    /// <summary>
    /// MonoModule to set item transform additive effects and apply to hand ik.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(50000)]
    [Obsolete]
    public class AdditivePlayer_HumanoidItem: AdditiveTransformHandler,IItemModule
    {
        #region Setup        

        public ControlMode CycleOption => ControlMode.Activate;

        IItem Item;

        public void SetHandler(IItem module)
        {
            Item = module;
        }

        public void Register()
        {

        }

        public void UnRegister()
        {

        }

        public virtual void Activate()
        {
            if (!ItemHandle)
                ItemHandle = transform;



            // _checkCache local player
            if (Item.Agent.IsLocalUser())
            {
                // get cam IAdditiveTransform
                var cam = CameraManager.mainCameraHandler;
                var additiveEffects = cam.GetComponents<IAdditiveTransform>();
                foreach (var effect in additiveEffects)
                    ApplyAdditiveEffect(effect);

                // bind motion system IK phase

                if (TryGetComponent(out Animator))
                {

                }
                else if (Item.Agent.Resolver.TryGetModule(out IHumanoidAnimator motionSystem))
                {
                    Animator = motionSystem.Animator;
                }
                else
                {
                    motionSystem = GetComponentInParent<IHumanoidAnimator>();

                    if (motionSystem == null)
                        throw new NotImplementedException();

                    Animator = motionSystem.Animator;
                }


                if (Animator)
                {
                    if (!RightHand)
                        RightHand = Animator.GetBoneTransform(HumanBodyBones.RightHand);

                    RightHandIK = new TwoBoneIKHandle(RightHand);

                    if (!LeftHand)
                        LeftHand = Animator.GetBoneTransform(HumanBodyBones.LeftHand);

                    LeftHandIK = new TwoBoneIKHandle(LeftHand);

                }
            }
        }
        

        public void Deactivate() 
        {
            enabled = false;
        }

        #endregion

        #region IK

        protected Animator Animator;

        public Transform RightHand;
        public Transform LeftHand;

        public TwoBoneIKHandle RightHandIK;
        public TwoBoneIKHandle LeftHandIK;

        protected virtual PR SolveIKGoal(PR additive, Transform goalTransform)
        {
            var pos = goalTransform.position;//Animator.GetIKPosition(goal);
            //Debug.Log(pos);

            pos = ItemHandle.InverseTransformPoint(pos);
            pos += additive;
            pos = additive.Rotation * pos;
            pos = ItemHandle.TransformPoint(pos);


            var rot = goalTransform.rotation;

            rot = ItemHandle.InverseTransformRotation(rot);
            rot = additive.Rotation * rot;
            rot = ItemHandle.TransformRotation(rot);


            return new(pos, rot);
        }

        #endregion

        /// <summary>
        /// Transform of item. 
        /// </summary>
        public Transform ItemHandle;

        [SerializeField]
        protected PR AdditiveEffect;


        protected override void ApplyAdditive(PR pr)
        {
            PostAdditiveChange(pr);

            AdditiveEffect = pr;

            if (AdditiveEffect.Equals(PR.Default))
                return;

            if (RightHandIK)
            {
                var right = SolveIKGoal(AdditiveEffect, RightHand);
                RightHandIK.Solve(right, right, 1f);
            }

            if (LeftHandIK)
            {
                var left = SolveIKGoal(AdditiveEffect, LeftHand);
                LeftHandIK.Solve(left, left, 1f);
            }

            ItemHandle.SetAdditiveLocalPR(AdditiveEffect);
        }


    }
}