using UnityEngine;
using System;
using Sirenix.OdinInspector;

public abstract class UIElemant:MonoBehaviour
{
    [ShowInInspector,ReadOnly]
    protected bool p_IsFree = true;
    public virtual bool IsFree { get => p_IsFree; set => p_IsFree = value; }

    public event Action<UIElemant> Release;
    public RectTransform RectTransform;
    protected virtual void Awake()
    {
        RectTransform = transform as RectTransform;
    }

    public virtual void ReleaseElement()
    {
        Release?.Invoke(this);
        p_IsFree = true;
    }
}