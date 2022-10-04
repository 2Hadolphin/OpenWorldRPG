using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Agents;
using Return.Items;
using Return;
using Return.Humanoid.IK;
using System;
using UnityEngine.Assertions;
using Return.Creature;

[Obsolete]
public class mInv : MonoBehaviour
{
    [SerializeField]
    public HumanoidInteractSystem InteractSystem;

    [ShowInInspector]
    public IAgent Agent;

    [ShowInInspector]
    List<AbstractItem> Bag=new List<AbstractItem>();

    HashSet<CacheDebit> Load = new HashSet<CacheDebit>();

    private void Awake()
    {
        var a = gameObject.GetComponentInChildren<AbstractItem>();
        Bag.Add(a);
    }

    /// <summary>
    /// **Withdraw **Stolen
    /// </summary>
    /// <param name="agent">which agent execute this call</param>
    /// <param name="item">item to withdraw </param>
    public virtual void Drawout(IAgent agent, AbstractItem item)
    {
        var handle = Symmetry.Right;

        if (InteractSystem.ValidMission(handle, out handle))
        {
            var adapter = InteractSystem.SetupMission(this, handle);

            Debug.Log(adapter.PermissionInteractor);

            adapter.Init(Symmetry.Right);

            var arg = new TwoBoneIKTargetArg(1.5f)
            {
                End = item.GetHandle("RightHand"),
                Limb = Limb.RightHand
            };

            #region Callback
            Load.Add(new CacheDebit() { Agent = agent, Token = arg, Item = item });
            //arg.FinishMotion += Release;
            #endregion


            adapter.UpdateIK(arg);
        }
        else
            Debug.LogError("Failure to use interaction system");
    }

    [Button("AddItem")]
    public virtual void AddItem(ItemPreset preset)
    {
        var item=preset.CreateItem(transform);
        
        Bag.Add(item);
    }

    [Button("Drawout")]
    protected virtual void Drawout()
    {
        var item = Bag[0];
        Drawout(Agent, item);
        Bag.Remove(item);
    }


    [Obsolete]
    protected class CacheDebit : IEquatable<CacheDebit>
    {
        public AbstractItem Item;
        public IAgent Agent;
        public object Token;

        public bool Equals(CacheDebit other)
        {
            return other.Token == Token;
        }
    }

    //public virtual void Release<T>(T arg) where T:IDisposable
    //{
    //    Debug.Log(arg);
    //    if (Load.TryGetValue(new CacheDebit() { Token = arg }, out var debit))
    //    {
    //        if(Load.Remove(debit))
    //            debit.Item.Register(debit.Agent);
    //    }
 
    //    arg.Dispose();
    //}

    
}
