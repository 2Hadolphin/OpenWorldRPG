using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Return.Agents;
using UnityEngine.InputSystem;
using Return.Humanoid.Motion;
using Return.Humanoid;
using UnityEngine.Assertions;
using Return.Preference;
using Cysharp.Threading.Tasks;
using Return.Modular;
using Return.Inputs;
using Return.Motions;

namespace Return.Items
{
    public class ItemPostureHandlerModule : ItemPostureHandlerModule<ItemPostureHandlerPreset>//, IHumanoidMotionModule
    {
        protected override ConfigurableItemModulePreset Preset
        {
            get => pref;
            set => value.Fill(ref pref);
        }

     
        protected MotionModule_ItemHandle MotionModule;

        public override ControlMode CycleOption => ControlMode.Register;
        public override bool DisableAtRegister => false;

        protected override void Register()
        {
            base.Register();

            Activate();
        }


        protected override void Activate()
        {
            base.Activate();


            #region MotionModule

            pref.MotionModuleItemHandler.Create(gameObject).Fill(ref MotionModule);

            if (Item.Agent.Resolver.TryGetModule(out MotionSystem_Humanoid motionSystem))
                MotionModule.SetHandler(motionSystem);

            #endregion

            if (Item.Agent.IsLocalUser())
            {
                var input = new InputHandle();

                input.RegisterInput(InputManager.Input);
                Item.resolver.RegisterModule(input);
                input.SetHandler(this);
                input.enabled = true;
            }

            //this.ValidMonoHandle<InputHandle>(item).
            //    BindLocalUser().
            //    Instance().
            //    SetHandler().
            //    Register().
            //    DebugLog();

            //var handle = item.ValidLocalUserHandle<InputHandle, ItemPostureHandlerModule>(this);

        }



        public override async UniTask Equip()
        {
            Debug.Log(transform.parent);

            //OnPerformerPlay?.Invoke(pref.Equip);

            performer.PlayState(pref.Equip, this);

            if (pref.Equip.GetAsset().TryGetUTagMarker(pref.EquipID, out var equip))
            {
                await UniTask.Delay(TimeSpan.FromSeconds(equip.time));
            }
        }

        public override async UniTask Dequip()
        {
            performer.PlayState(pref.Unequip, this);

            if (pref.Unequip.GetAsset().TryGetUTagMarker(pref.UnequipID, out var dequip))
            {
                await UniTask.Delay(TimeSpan.FromSeconds(dequip.time));
            }
        }

        public override async UniTask Inspect()
        {

        }
 



        public class InputHandle : UserInputExtendHandle<ItemPostureHandlerModule>
        {
            #region Input

            #region Subscribe
            protected override void SubscribeInput()
            {

            }


            protected override void UnsubscribeInput()
            {

            }
            #endregion


            public virtual void Dequip(InputAction.CallbackContext ctx)
            {
                _ = module.Dequip();
            }


            #endregion
        }


        #region Unknow

        /// <summary>
        /// Operate collision wrapper via interact system(basic) **dequip->activate->use **
        /// </summary>
        public virtual void ParseInteractor()
        {

            /// Can use(**level **ability  ), how to use (**grab **handle **skill move )
            #region Parse user 

            #endregion

            /// Closed interactor
            #region Request Handle
            var symmetry = SettingsManager.Instance.PrimeHand.ToSymmetry();

            if (symmetry.HasFlag(Symmetry.Both))
                if (UnityEngine.Random.value > 0.5f)
                    symmetry ^= Symmetry.Right;
                else
                    symmetry ^= Symmetry.Left;


            //symmetry = interactor.Evalute(GetHandle(string.Format("{0}Hand", symmetry.ToString())));
            //if (interactor.SetupMission(this, symmetry, out var adapter))
            //{
            //    adapter.Init(Symmetry.Right);
            //    arg.FinishMotion += Release;
            //    adapter.UpdateIK(arg);

            //    item.RegisterAgent(InputManager.Inputs);
            //}
            #endregion
        }

        [Obsolete("Why here?")]
        protected virtual void PhysicSetting()
        {
            //Wrapper = item.CollisionWrapper;
            //Wrapper.ContactPost += ContactPost;
        }

        protected CollisionWrapper Wrapper;



        protected virtual void ContactPost(int length, ICollection<ContactPoint> contacts)
        {

        }

        #region Performer

        //public override event Action<TimelinePerformerHandle> OnPerformerPlay;


        public override void Cancel(TimelinePreset performer)
        {

        }




        //public override TimelinePreset[] LoadPerformers()
        //{
        //    return new[] { pref.Equip,pref.Unequip };
        //}
        #endregion

        #endregion
    }

    /// <summary>
    /// Custom posture module.
    /// </summary>
    public abstract class ItemPostureHandlerModule<T>: BaseItemPostureHandlerModule where T:ItemPostureHandlerPreset
    {
        [ShowInInspector,ReadOnly]
        protected T pref;

        public override void LoadModuleData(ConfigurableItemModulePreset preset)
        {
            base.LoadModuleData(preset);
            pref = preset as T;
        }
    }

}