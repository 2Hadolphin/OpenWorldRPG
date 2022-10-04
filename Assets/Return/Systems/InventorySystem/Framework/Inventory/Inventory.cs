using Return.Agents;
using Return.InteractSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Framework.Grids;
using Return.Items;
using UnityEngine.Assertions;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

namespace Return.Inventory
{
    /// <summary>
    /// Dynamic storage, attach to item handler as inventory bag.
    /// </summary>
    public class Inventory : BaseInventory, IInventory //, IInteractSource //**vehicle storage interactable option
    {
        #region UI

        [SerializeField]
        InventoryUIHandler m_UIHandler;
        public InventoryUIHandler UIHandler { get => m_UIHandler; set => m_UIHandler = value; }

        public virtual void ShowUI()
        {
            if (!UIHandler.IsInstance())
            {
                UIHandler = Instantiate(UIHandler);
                UIHandler.Setup(Capacity.width, Capacity.height);
            }
            else if(UIHandler.isActiveAndEnabled)
            {
                CloseUI();
                return;
            }

            UIHandler.UpdateContents(Storages);

            //foreach (var content in Storages)
            //{

            //}


            UIHandler.gameObject.SetActive(true);
        }


        public virtual void CloseUI()
        {
            if (UIHandler && UIHandler.IsInstance())
                UIHandler.gameObject.SetActive(false);
        }


        #endregion


        #region Hanlder

        public IInventoryManager Handler { get; protected set; }

        #endregion


        #region Content

        [ListDrawerSettings(Expanded =true,NumberOfItemsPerPage =5)]
        [NonSerialized]
        [ShowInInspector]
        HashSet<IArchiveContent> m_storages;
        public HashSet<IArchiveContent> Storages { get => m_storages; set => m_storages = value; }

        [SerializeField]
        Volume m_capacity;

        public Volume Capacity { get => m_capacity; set => m_capacity = value; }

        [NonSerialized]
        uint currentVolume;
        public uint CurrentVolume { get => currentVolume; set => currentVolume = value; }

        #endregion

        #region IInventory

        [Inject(Optional = true)]
        public virtual void RegisterHandler(IInventoryManager handler)
        {
            if (handler == null)
            {

            }
            else
            {

            }

            Handler = handler;

            // set capacity nums
            Storages = new(Capacity.capacity);

            Debug.Log($"Inventory load handler : {handler}");
        }

        /// <summary>
        /// Return content target position.
        /// </summary>
        /// <param name="obj">???</param>
        /// <returns>Coordinate to approach.</returns>
        public virtual ICoordinate GetHandle(object obj)
        {
            return new Coordinate(transform);
        }


        public override bool Search(Func<IArchiveContent, bool> method, out object obj)
        {
            foreach (var item in Storages)
            {
                try
                {
                    if (method(item))
                    {
                        obj = item;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            obj = null;
            return false;
        }

        public override bool CanStorage(object obj)
        {
            Debug.LogError($"Checking inventory storage for {obj}.");

            // check slot
            if (obj is IArchiveContent archive)
            {


                return true;
            }

            if (obj is IPickup pickup)
            {
                // check volume
                var volume = pickup.Preset.Icon.Volume;

                if (volume.capacity + currentVolume > Capacity.capacity)
                    return false;

                if (volume.max > Capacity.max || volume.min > Capacity.min)
                    return false;

                return true;
            }


            Debug.LogError($"Storage target has not convert to content archive : {obj}.");
            return false;
        }

        public override void Store(object obj)
        {
            Assert.IsTrue(CanStorage(obj));
            // overload inventory ignore grid typography

            if (obj is not IArchiveContent archive)
            {
                archive = new ArchiveContent() { content = obj };
            }

            if (Storages.Add(archive))
                Debug.Log($"Store {obj} to {this}.");

            if (obj is Component c)
            {
                c.gameObject.SetActive(false);
            }

        }


        public override bool CanExtract(object obj)
        {
            if (obj is IArchiveContent archive)
            {
                return Storages.Contains(archive);
            }
            else
            {
                foreach (var storage in Storages)
                {
                    if (storage.content.Equals(obj))
                        return true;
                }
            }

            return false;
        }


        public override void Extract(object obj)
        {
            Assert.IsTrue(CanExtract(obj));

            if (obj is IArchiveContent archive)
            {
                Storages.Remove(archive);
            }
            else
            {
               var valid = Storages.RemoveWhere(x => x.content.Equals(obj));
                Assert.IsTrue(valid == 1);
            }
        }




        #endregion


        #region IModule

        protected override void Register()
        {
            base.Register();


            if (!UIHandler.IsInstance())
                UIHandler = Instantiate(UIHandler);

            UIHandler.gameObject.SetActive(false);
        }

        protected override void Unregister()
        {
            base.Unregister();

            Destroy(UIHandler.GetRoot().gameObject);
        }

        #endregion
    }


}