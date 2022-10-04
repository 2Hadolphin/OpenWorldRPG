using MxM;
using Return.Creature;
using Return.Motions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Return.Humanoid
{
    public partial class Slide : MotionModule_Humanoid
    {
        //public MotionModulePreset_Humanoid pref { get; protected set; }
        public override MotionModulePreset_Humanoid GetData => pref;

        protected Slide_MotionPreset pref;

        public override void LoadData(MotionModulePreset data)
        {
            pref = data as Slide_MotionPreset;
            data.Parse(out pref);
            _Interrupt = new UnityAction(Interrupt);
        }

        #region Resolver

        [Inject]
        IMxMHandler IMxMHandler;

        #endregion

        #region Parameters

        [ShowInInspector]
        ETags p_SlideTags;

        #endregion

        public override void SetHandler(IMotionSystem motionSystem)
        {
            base.SetHandler(motionSystem);

         
        }

        protected override void Register()
        {
            base.Register();

            foreach (var tagName in pref.SlideTags)
            {
                var tag = IMotions.mxm.CurrentAnimData.FavourTagFromName(tagName);
                p_SlideTags |= tag;
            }
        }

        #region LimbSequence

        protected override void FinishModuleMotion()
        {
            base.FinishModuleMotion();
            Motions.ResetControllerBounds();
        }

        #endregion

      
        protected virtual void DoSlide()
        {
            if (!Motor.GroundingStatus.IsStableOnGround)
                return;

            if (IMotions.LocalSelfVelocity < 2f)
                return;


            if ((IMxMHandler.mxm.FavourTags & p_SlideTags) == 0)
                return;

            Debug.Log("Try Sliding");

            if (ValidModuleMotion(pref.GetMotionLimbs))
            {
                pref.SlideDefinition.ClearContacts();

                IMxMHandler.mxm.BeginEvent(pref.SlideDefinition);
                // prevent none anim match
                IMxMHandler.mxm.OnIdleTriggered.AddListener(_Interrupt);
                Motions.SetControllerBounds(pref.SlideHeight, pref.SlideRadius);
                Debug.Log("Start Sliding");
            }
        }

        UnityAction _Interrupt;
        public void Interrupt()
        {
            if (EnableMotion)
            {
                IMxMHandler.mxm.ForceExitEvent();
                FinishModuleMotion();
                IMxMHandler.mxm.OnIdleTriggered.RemoveListener(_Interrupt);
            }
            Debug.Log("Interrupt Sliding");
        }


        public override void InterruptMotion(Limb limb)
        {
            base.InterruptMotion(limb);
            Interrupt();
        }

        private void Update()
        {
            if (EnableMotion)
            {
                if (IMxMHandler.mxm.CheckEventPlaying(pref.SlideDefinition.Id))//&&Motor.mxm.IsEventComplete)
                {
                    // handle module speed and IK hand position +ragdoll position

                }
                else //if (Motor.mxm.IsEventComplete)
                {
                    Debug.LogError("Not Sliding");
                    IMxMHandler.mxm.ForceExitEvent();
                    FinishModuleMotion();
                }
            }


        }

    }
}