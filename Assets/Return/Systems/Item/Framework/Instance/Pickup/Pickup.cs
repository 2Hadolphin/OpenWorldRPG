using Return.Agents;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Return.InteractSystem;
using UnityEngine.Assertions;

namespace Return.Items
{
    /// <summary>
    /// Base class contains item info and handle inherit by showcase(pool) and item
    /// </summary>
    public abstract class Pickup : BaseComponent, IPickup, IInstruction
    {
        #region Preset
        [SerializeField]
        ItemPreset m_Preset;
        public virtual ItemPreset Preset { get => m_Preset; set => m_Preset = value; }
        #endregion

        #region IItem Modue

        public virtual IAgent Agent { get; protected set; }

        [BoxGroup("Develop")]
        [Button]
        public virtual void Register(IAgent agent)
        {
            Agent = agent;
        }

        [BoxGroup("Develop")]
        [Button]
        public virtual void Unregister(IAgent agent)
        {
            Assert.IsTrue(Agent == agent);
            Agent = null;
        }

        [BoxGroup("Develop")]
        [Button]
        public abstract UniTask Activate();

        [BoxGroup("Develop")]
        [Button]
        public abstract UniTask Deactivate();

        #endregion

        #region Intruct

        public virtual void Instruction(IAgent agent, object sender)
        {
            // valid user and game mode
            //if (Agent is ILocalHumanoidAgent user && Agent.isMine)
            if (Agent.IsLocalUser())
            {
                ICoordinate label;

                var slot = transform.Find("Label");

                if (slot.NotNull())
                    label = new Coordinate(slot);
                else
                    label = new Coordinate(transform);

                // bind view and UI

            }
        }

        #endregion


        #region IPickup

        public abstract void Drop(PR pr);

        public abstract void Drop(IPickupableProvider provider);




        /// <summary>
        /// Get the handle coordinate of this item
        /// </summary>
        /// <param name="keys">Enable offset option </param>
        public virtual ICoordinate GetHandle(string key)
        {
            ICoordinate coordinate;

            //if (Preset.TryGetModulePreset<IItemPostureHandler>(out var postureHandle))
            //{
            //    if (!postureHandle..GetOffset(key, out var pr))
            //        coordinate = new Coordinate(transform, Space.Self);
            //}
            //else
            //    coordinate = new Coordinate_Offset(transform, pr, pr, false);

            coordinate = new Coordinate(transform);

            Debug.Log(coordinate.OutputPositionSpace+" : "+coordinate.position);
            return coordinate;
        }

        /// <summary>
        /// Get the free-grab coordinate of this item
        /// </summary>
        public virtual ICoordinate Grab(ICoordinate subtraction = null)
        {
            if (subtraction == null)
                return new Coordinate(transform);
            else // caculate subtract handle
                return new Coordinate(transform);
        }


        //public CollisionWrapper CollisionWrapper { get; protected set; }
        //public Rigidbody RB => CollisionWrapper.RB;

        //protected virtual void Awake()
        //{
        //    CollisionWrapper = CharacterRoot.InstanceIfNull<CollisionWrapper>();
        //}
        #endregion


        #region to IPostureHandler

        /// <summary>
        /// ReadOnlyTransform which hold this item(hand or container).
        /// </summary>
        public virtual Transform Holster { get=>m_Holster; protected set=>m_Holster=value; }


        /// <summary>
        /// ???????
        /// </summary>
        protected Transform m_Holster;

        #endregion
    }
}