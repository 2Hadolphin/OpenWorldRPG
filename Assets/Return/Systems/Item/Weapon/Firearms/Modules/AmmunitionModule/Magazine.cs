using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Return.Items.Weapons
{
    /// <summary>
    /// Container cotains ammunition as item
    /// </summary>
    public class Magazine : Container<AmmunitionData>
    {
        [SerializeField]
        private int m_MaxAmount;

        public override int Capacity { get => m_MaxAmount; set => m_MaxAmount = value; }

        public override int Amount
        {
            get => Content.Count;
        }

        //[Serializable]
        //public struct ammoLog
        //{
        //    public int count;
        //    public AmmunitionData data;
        //}

        [SerializeField]
        mStack<AmmunitionData> ammo=new();

        public mStack<AmmunitionData> Content { get=> ammo; protected set=> ammo=value; }

 
        public override void Clear()
        {
            Content.Clear();
        }

        public override bool Search(Func<AmmunitionData, bool> find, out int sn)
        {
            var length = Content.Count;
            for (int i = 0; i < length; i++)
            {
                if (find(Content[i]))
                {
                    sn = i;
                    return true;
                }
            }
            sn = -1;
            return false;
        }

        public override int Store(AmmunitionData ammo)
        {
            return Content.Push(ammo);

            //return Content.Push(new ammoLog() { data= ammo ,count=1});
        }

        [Button]
        public virtual void Store(AmmunitionData ammo,int num = -1)
        {
            if (num < 0)
                num = Capacity;
            else
                num = num.Min(Capacity - Content.Count);

            for (int i = 0; i < num; i++)
                Content.Add(ammo);
        }

        public override AmmunitionData Withdraw(int sn=-1)
        {
            AmmunitionData log;

            if (Content.ValidSN(sn))
                log = Content.Extract(sn);
            else
                log = Content.Pop();

            return log;

            //if (--log.count > 0)
            //    Content.Push(log);

            //return log.data;
        }


    }
}