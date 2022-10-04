using UnityEngine;
using System;
using Return.Items.Weapons;

[Serializable]//??
public class AmmoInstance
{
    [SerializeField]
    private AmmunitionData m_AmmoType;

    [SerializeField]
    [Range(0, Mathf.Infinity)]
    private int m_Amount;

    [SerializeField]
    [Range(0, Mathf.Infinity)]
    private int m_MaxAmount;

    public int Amount
    {
        get => m_Amount;
        set => m_Amount = Mathf.Clamp(value, 0, m_MaxAmount);
    }

    public int MaxAmount => m_MaxAmount;

    public AmmunitionData Instance => m_AmmoType;





    /// <summary>
    /// 
    /// </summary>
    public bool IsEmptySlot => m_AmmoType == null;
}