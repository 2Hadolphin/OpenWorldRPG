//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Return.Humanoid;
//using System.Linq;

//namespace Return
//{
//    public class InventoryHandler : GearSystem, IInventory
//    {
//        [System.Serializable]
//        public struct InventoryRef
//        {
//            public int LimitNumber;
//            public int MaxWeight;
//            public int MaxVolume;
//            public int Weight;
//            public int Volume;
//        }
//        private InventoryManager manager;
//        private int callbackSN;
//        private InventoryManager.SlotPlace place = InventoryManager.SlotPlace.Clothes;
//        private IItem[] storeItems;
//        private Dictionary<int, ReadOnlyTransform> targetPosition;
//        [SerializeField]
//        protected InventoryRef Temp = default;

//        public int getSN { get { return callbackSN; } }

//        public enum SlotType { Mixed = 0, Arms = 1, Throwable = 2, Melee = 3, Tool = 4, Consumable = 5 }

//        [SerializeField]
//        private SlotType type = SlotType.Mixed;
//        public SlotType category { get { return type; } }

//        public Vector3Int Capacity => throw new System.NotImplementedException();

//        public InventoryManager.SlotPlace getPlace => throw new System.NotImplementedException();

//        public InventoryRef GetTemp => throw new System.NotImplementedException();


//        public bool Initialization(InventoryManager m)
//        {
//            if (m != null)
//                manager = m;
//            else
//                return false;

//            storeItems = new IItem[Temp.LimitNumber];
//            targetPosition = new Dictionary<int, ReadOnlyTransform>();
//            loadSlotTF();

//            //Temp = GDR.manager.TakeData(); 
//            return true;
//        }

//        private void loadSlotTF()
//        {
//            targetPosition.Add(-1, transform);
//            ReadOnlyTransform[] transforms = transform.GetComponentsInChildren<ReadOnlyTransform>();
//            int lenght = transforms.Length;
//            string tag = "AbstractValue";
//            for (int i = 0; i < lenght; i++)
//            {
//                if (transforms[i].CompareTag(tag))
//                    targetPosition.Add(targetPosition.Count, transforms[i]);
//            }
//        }

//        private int EmptySlotSN;
//        private int[] QuickList;


//        public bool CheckSlot(int weight, int volume, out IInventory slot, out InventoryManager.SlotPlace slotPlace)
//        {
//            slot = null;
//            slotPlace = place;
//            for (int i = 0; i < Temp.LimitNumber; i++)
//            {
//                if (storeItems[i] != null)
//                    continue;

//                if (Temp.MaxWeight - Temp.Weight > weight)
//                {
//                    if (Temp.MaxVolume - Temp.Volume > volume)
//                    {
//                        slot = this;
//                        EmptySlotSN = i;
//                        return true;
//                    }
//                    else
//                    {
//                        manager.LogIssue(InventoryManager.Issue.NonPlace);
//                    }
//                }
//                else
//                {
//                    manager.LogIssue(InventoryManager.Issue.OverWeight);
//                }
//            }



//            return false;
//        }


//        public bool CheckSlot(int weight, int volume)
//        {
//            for (int i = 0; i < Temp.LimitNumber; i++)
//            {
//                if (storeItems[i] != null)
//                    continue;

//                if (Temp.MaxWeight - Temp.Weight > weight)
//                {
//                    if (Temp.MaxVolume - Temp.Volume > volume)
//                    {
//                        return true;
//                    }
//                    else
//                    {
//                        manager.LogIssue(InventoryManager.Issue.NonPlace);
//                    }
//                }
//                else
//                {
//                    manager.LogIssue(InventoryManager.Issue.OverWeight);
//                }
//            }
//            return false;
//        }

//        private struct AccessLock { public int HashLock; public int weight; public int volume; }
//        private AccessLock locker;
//        public void Store(IItem item)
//        {
//            /*
//            if (!lockhash.Equals(locker.HashLock))
//                Debug.LogError("Store Lock Missing !");

//            */
//            //item.SetParent(this.transform,default, true);
//            item.DisposePlayingPerformer();
//            if (EmptySlotSN != -1)
//                storeItems[EmptySlotSN] = item;

//            EmptySlotSN = -1;

//            updatePost();
//        }

//        public ReadOnlyTransform GetOriginTargetTF()
//        {
//            ReadOnlyTransform tf = targetPosition[-1];
//            if (tf == null)
//                return transform;
//            else
//                return tf;
//        }

//        public ICoordinate PreDrawout(int sn, out InventoryManager.SlotPlace _place)        // if sn==-1 return slot[i]'s transform
//        {
//            _place = place;
//            if (sn == -1)
//            {
//                if (EmptySlotSN != -1)     //empty slot
//                {
//                    if (targetPosition.ContainsKey(EmptySlotSN))
//                        return new Coordinate(targetPosition[EmptySlotSN]);
//                }
//                else
//                {
//                    Debug.LogError("EmptySlotSN has missing !");
//                }
//            }
//            else
//            {
//                if (sn < storeItems.Length)
//                {
//                    if (storeItems[sn] != null)
//                        return storeItems[sn].Grab();
//                }
//                else
//                {
//                    Debug.LogError("SN has missing !");
//                }
//            }
//            return new Coordinate(targetPosition[-1]);    // out of bounce
//        }


//        public void Withdraw(int sn)
//        {
//            storeItems[sn] = null;
//            updatePost();
//        }

//        private void updatePost()
//        {
//            if (callbackSN >= 0)
//                manager.updateLog(callbackSN);
//        }


//        public void ApplySN(int sn)
//        {
//            callbackSN = sn;
//            updatePost();
//        }

//        public IItem ConfirmWithDraw(int sn)
//        {

//            if (sn == -1)       // load quick list
//            {
//                sn = QuickList.Length;
//                if (sn > 0)     //empty slot
//                {
//                    for (int i = 0; i < sn; i++)
//                    {
//                        if (QuickList[i] >= 0)
//                            return storeItems[i];
//                    }
//                    return storeItems[0];           //??? unfinish
//                }
//                else
//                {
//                    Debug.LogError("QuickList has missing !");
//                }
//            }
//            else
//            {
//                if (sn < storeItems.Length)
//                {
//                    if (storeItems[sn] != null)
//                        return storeItems[sn];
//                }
//                else
//                {
//                    Debug.LogError("SN has missing !");
//                }
//            }

//            return storeItems[0];
//        }

//        public void SetPlace(InventoryManager.SlotPlace slotPlace)
//        {
//            place = slotPlace;
//        }

//        public void Store(IItem item, int SN)
//        {
//            throw new System.NotImplementedException();
//        }

//        public void Withdraw(int sn, out IItem item)
//        {
//            throw new System.NotImplementedException();
//        }

//        public bool ConfirmTarget(int coordnate, out int confirmHash)
//        {
//            throw new System.NotImplementedException();
//        }



//    }
//}