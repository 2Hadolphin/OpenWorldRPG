using Return.Humanoid.Motion;
using Return.Humanoid;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Assertions;
using UnityEngine;
using System.Linq;
using Return;
using Cysharp.Threading.Tasks;
using Return.Modular;

namespace Return.Items
{
    // RegisterHandler to motion system
    /// <summary>
    /// **Pose adjust => set slot parent & item offset and finger override    
    /// </summary>
    public abstract class BaseItemPostureHandlerModule : ConfigurableItemModule, IItemPostureHandler, IItemPerformerHandle
    {
        #region Setup
        public override bool DisableAtRegister => false;

        /// <summary>
        /// Manually activate first for equip.
        /// </summary>
        public override ControlMode CycleOption => ControlMode.Register;


        /// <summary>
        /// Set item name and hierarchy.
        /// </summary>
        protected override void Register()
        {
            base.Register();

            SetItemName();

            SetPostureHierarchy(Item);
        }

        /// <summary>
        /// Posture module should be activated manually by item before other modules been register.
        /// </summary>
        protected override void Activate()
        {
            //base.SetHandler();

            this.CheckEnable();

            Item.resolver.TryGetModule(out performer);
            //Debug.LogError(performer);
            Assert.IsNotNull(performer);
        }


        #endregion


        protected virtual string ItemName { get; set; } = nameof(AbstractItem);

        /// <summary>
        /// Set item name in hierarchy.
        /// </summary>
        protected virtual void SetItemName()
        {
            gameObject.name = ItemName;
        }

        /// <summary>
        /// Move item to handle slot.
        /// </summary>
        protected virtual void SetPostureHierarchy(IItem item)
        {
            var go = item.gameObject;

            Assert.IsFalse(go == null);

            //Debug.Log(item.Agent.Transform.FindChild(HandleOption.ItemHandle_RightHand,out var handSlot)+" : "+handSlot);

            var slot=item.Agent.transform.Traverse().Where(x => x.name == HandleOption.ItemHandle_RightHand).FirstOrDefault();

            Debug.LogFormat("Set {0} posture slot {1}.",item,slot);

            Assert.IsNotNull(slot);

            go.transform.ParentOffset(slot);

            Debug.Log(nameof(SetPostureHierarchy)+ go.transform.position);
        }

        

        #region Performer

        public IPerformerHandler performer;

        //public abstract event Action<TimelinePerformerHandle> OnPerformerPlay;
        public abstract void Cancel(TimelinePreset preset);
        public virtual void Finish(TimelinePreset preset) { }

        //public abstract TimelinePreset[] LoadPerformers();

        public int CompareTo(IItemPerformerHandle other)
        {
            return 0;
        }



        #endregion


        #region Behaviour **Equip **Dequip **Inspect

        ///// <summary>
        ///// Melee
        ///// </summary>
        //[Obsolete]
        //public abstract void Hit();

        ///// <summary>
        ///// Pickup
        ///// </summary>
        //[Obsolete]
        //public abstract void Grab();

        ///// <summary>
        ///// 
        ///// </summary>
        //[Obsolete]
        //public abstract void Throw();

        /// <summary>
        /// Hoslter
        /// </summary>
        [Button]
        public abstract UniTask Equip();

        /// <summary>
        /// Storage
        /// </summary>
        [Button]
        public abstract UniTask Dequip();

        /// <summary>
        /// Look at item
        /// </summary>
        public abstract UniTask Inspect();


        #endregion

        //#region Motion System
        //public abstract void SetHandler(IMotionSystem motor);
        //#endregion



    }

}