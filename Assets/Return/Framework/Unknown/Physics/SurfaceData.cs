
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Class that represent the properties of a surface.
/// </summary>
[System.Serializable]
public class SurfaceData
{
    /// <summary>
    /// The type of the surface.
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("The type of the surface.")]
    private SurfaceType m_SurfaceType;

    /// <summary>
    /// Allows bullet marks to be rendered on this surface.
    /// </summary>
    [SerializeField]
    [Tooltip("Allows bullet marks to be rendered on this surface.")]
    private bool m_AllowDecals = true;

    /// <summary>
    /// Allows projectiles to penetrate this surface.
    /// </summary>
    [SerializeField]
    [Tooltip("Allows projectiles to penetrate this surface.")]
    private bool m_Penetration;

    /// <summary>
    /// Defines surface density. A higher value will make it harder for a projectile to get through the object.
    /// (This value is directly related to the penetration power of each weapon)
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Defines surface density. A higher value will make it harder for a projectile to get through the object." +
             "(This value is directly related to the penetration power of each weapon)")]
    private float m_Density = 1;

    #region PROPERTIES

    /// <summary>
    /// Returns the Surface Type.
    /// </summary>
    public SurfaceType SurfaceType => m_SurfaceType;

    /// <summary>
    /// Returns whether or not bullet marks can be rendered on this surface.
    /// </summary>
    public bool AllowDecals => m_AllowDecals;

    /// <summary>
    /// Returns whether or not projectiles can penetrate this surface.
    /// </summary>
    public bool Penetration => m_Penetration;

    /// <summary>
    /// Returns the density of this surface.
    /// </summary>
    public float Density => m_Density;

    #endregion
}
