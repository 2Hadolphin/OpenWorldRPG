using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;
using System;
using Return.Humanoid.Character;
using Return.Physical;
using Return;

[DisallowMultipleComponent]
public partial class LimbCollision : MonoBehaviour,ILimbCollision
{
    public static LimbCollision Create<T>(GameObject @object,out T collider ,ref HashSet<Collider> group) where T:Collider
    {
        Assert.IsNotNull(@object);
        var wrapper = @object.InstanceIfNull<LimbCollision>();

        Assert.IsNotNull(group);
        wrapper.IgnoreGroup = group;
        collider = @object.InstanceIfNull<T>();
        wrapper.Collider = collider;
        group.Add(collider);
        
        return wrapper;
    }
    public static LimbCollision Create(GameObject @object,ColliderType colliderType,ref HashSet<Collider> group)
    {
        Assert.IsNotNull(@object);
        var wrapper=@object.InstanceIfNull<LimbCollision>();

        Assert.IsNotNull(group);
        wrapper.IgnoreGroup = group;

        wrapper.Collider = colliderType.Create(@object);
        group.Add(wrapper.Collider);

        return wrapper;
    }


    public Collider Collider { get; protected set; }
    protected HashSet<Collider> IgnoreGroup;
    public event Action<Collision> Impact;


    public Dictionary<Type, Receiver> Receivers=new Dictionary<Type, Receiver>();

    public Receiver<T> RegisterCollisionArg<T>()
    {
        var receiver = new Receiver<T>(this);
        if (Receivers.TryGetValue(receiver, out var originReceiver))
            return originReceiver as Receiver<T>;
        else
            Receivers.Add(receiver, receiver);

        return receiver;
    }

    public void Raycast<T>(T arg)
    {
        if (Receivers.TryGetValue(typeof(T), out var reciver) && reciver is Receiver<T> target)
            target.Invoke(this, arg);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IgnoreGroup.Contains(collision.collider))
            return;

        Impact?.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (IgnoreGroup.Contains(collision.collider))
            return;

        Impact?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        
    }

    
}
