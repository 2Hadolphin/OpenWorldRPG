using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Items;
using Return.Items.Weapons.Firearms;

public class FirearmsShowcase : ItemShowcase
{
    /// <summary>
    /// The Gun Preset Asset that this gun pickup represents.
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("The Gun Preset Asset that this gun pickup represents.")]
    private FirearmsPreset m_GunData;
}
