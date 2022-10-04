using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Return.Items;
using Return.Framework.Pools;
using Return.Modular;
using UnityEngine.Assertions;
using Return.Agents;

namespace Return.Inventory
{
    /// <summary>
    /// Handler to manage inventories.
    /// </summary>
    [DisallowMultipleComponent]
    public partial class InventoryManager : ModuleHandler, IInventoryManager
    {
        InputHandle Input;

        #region Handler

        public override void InstallResolver(IModularResolver resolver)
        {
            resolver.RegisterModule<IInventoryManager>(this);
        }

        [Inject(Optional =true)]
        protected virtual void LoadAgent(IAgent agent)
        {
            if (agent.IsLocalUser())
            {
                if (Input == null)
                {
                    Input = new InputHandle();
                    Input.SetHandler(this);
                    Input.RegisterInput();
                }
            }
        }

        #endregion

        #region Setup

        protected override void Register()
        {
            base.Register();

            if (Inventories == null)
                Inventories = new(5);

            // dev add inventory
            {
                var backpack = Resources.Load<Item>("Dev_Inventory_bag");

                backpack = Instantiate(backpack, transform);
                var inv = backpack.GetComponentInChildren<IInventory>();
                Assert.IsFalse(inv == null);

                Register(inv);


            }

            //if(Resolver.TryGetModule<IAgent>(out var agent))
            //{
 
            //}
        }


        protected override void Activate()
        {
            base.Activate();

            if (Input)
                Input.enabled = true;
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            if (Input)
                Input.enabled = false;
        }


        #endregion

        #region Inventory

        #region Field
        //[Tooltip("Preset inventory")]
        //[SerializeField]
        //HashSet<IInventory> m_bags;

        [SerializeField]
        HashSet<IInventory> m_Inventories;

        Dictionary<Category, IInventory> m_quickSlot;

        #endregion



        #region Proporty

        //public HashSet<IInventory> Bags { get => m_bags; set => m_bags = value; }
        public HashSet<IInventory> Inventories { get => m_Inventories; set => m_Inventories = value; }
        public Dictionary<Category, IInventory> QuickSlot { get => m_quickSlot; set => m_quickSlot = value; }


        public ICoordinate EjectCoordinate { get; protected set; }

        #endregion


        #endregion



        #region IInventoryManager

        /// <summary>
        /// Register inventory to this manager.
        /// </summary>
        public virtual void Register(IInventory inventory)
        {
            if (Inventories.Add(inventory))
                inventory.RegisterHandler(this);
        }

        public virtual void Unregister(IInventory inventory)
        {
            if (Inventories.Remove(inventory))
                inventory.RegisterHandler(null);
        }

        public virtual bool CheckStore(object obj)
        {
            Debug.Log($"Inventory check storage valid with {obj}.");

            foreach (var inv in Inventories)
            {
                try
                {
                    if (inv.CanStorage(obj))
                    {
                        Debug.Log($"Found enable inventory for {obj}.");
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
      

            return false;
        }

        public virtual void Store(object obj)
        {
            // search inventory and store item

            foreach (var inv in Inventories)
            {
                try
                {
                    if (inv.CanStorage(obj))
                    {
                        inv.Store(obj);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public virtual async UniTask<object> Search(Func<object, bool> method)
        {
            object result=null;

            await UniTask.SwitchToTaskPool();

            foreach (var inv in Inventories)
            {
                try
                {
                    if (inv.Search(method, out result))
                        break;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            await UniTask.SwitchToMainThread();

            return result;
        }

        public virtual bool ExtractStock(object item)
        {
            // search item and remove from inventory

            foreach (var inv in Inventories)
            {
                try
                {
                    if (inv.CanExtract(item))
                        return true;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return false;
        }

        #endregion









        #region Obsolete

        public SlotSetting InventSetting { get; private set; }


        //private List<IInventory> EquimentSlots;

        public Dictionary<Category, List<ISingleSlot>> Slots = new ();


        [Obsolete]
        public HashSet<ItemInfo> ProcessingLock = new ();

        #endregion



    }



    [Obsolete]
    public enum SlotPlace { Clothes = 0, Front = 1, Side = 2, Top = 3, Back = 4 }

    [Obsolete]
    public struct SlotSetting
    {
        public bool Unfold;
        public bool Unique;
        public bool LoadSlotFirst;
        public bool MixedPlace;
    }


    [Obsolete]
    public struct InventoryData
    {
        public string Name;
        public Sprite Icon;
        public int itemHash;
        public int similarnumber;
        public int sn;
    }

    [Obsolete]
    public struct InventoryPost
    {
        public int slotSn;
        public InventoryData[] data;
    }
}