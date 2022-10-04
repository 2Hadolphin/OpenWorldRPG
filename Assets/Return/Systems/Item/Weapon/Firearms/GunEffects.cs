using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public sealed class GunEffects
{
    #region MUZZLEBLAST

    /// <summary>
    /// Enables muzzle blast particles.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables muzzle blast particles.")]
    private bool m_MuzzleFlash;

    /// <summary>
    /// The muzzle blast particle.
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("The muzzle blast particle.")]
    private ParticleSystem m_MuzzleParticle;

    #endregion

    #region TRACER

    /// <summary>
    /// Enables tracer ammunition for this gun.
    /// </summary>
    [SerializeField]
    private bool m_Tracer;

    /// <summary>
    /// The tracer prefab.
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("The tracer prefab.")]
    private Rigidbody m_TracerPrefab;

    /// <summary>
    /// Duration of the tracer.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Duration of the tracer.")]
    private float m_TracerDuration = 1f;

    /// <summary>
    /// The projectile speed.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("The projectile speed.")]
    private float m_TracerSpeed = 450;

    /// <summary>
    /// GUIDs of origin of the tracer. (GUIDs it will be instantiated)
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("Position of origin of the tracer. (Position it will be instantiated)")]
    private Transform m_TracerOrigin;

    #endregion

    #region SHELL 

    /// <summary>
    /// Enables shell ejection when firing.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables shell ejection when firing.")]
    private bool m_Shell;

    /// <summary>
    /// Bullet shell emitter.
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("Bullet shell emitter.")]
    private ParticleSystem m_ShellParticle;

    /// <summary>
    /// Defines the minimum and maximum force the shell will be ejected.
    /// </summary>
    [SerializeField]
    [m_MinMaxSliderAttribute(0, 5, "Defines the minimum and maximum force the shell will be ejected.")]
    private Vector2 m_ShellSpeed = new Vector2(1, 3);

    /// <summary>
    /// Delay in seconds for the shell to be ejected.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("Delay in seconds for the shell to be ejected.")]
    private float m_StartDelay;

    // Current cartridge particle
    private ParticleSystem[] m_CachedParticle;

    #endregion

    #region MAGAZINE DROP

    /// <summary>
    /// Allows the character to Schedule_Drop a magazine by reloading the gun.
    /// </summary>
    [SerializeField]
    [Tooltip("Allows the character to Schedule_Drop a magazine by reloading the gun.")]
    private bool m_MagazineDrop;

    /// <summary>
    /// The magazine prefab to be instantiated.
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("The magazine prefab to be instantiated.")]
    private Rigidbody m_MagazinePrefab;

    /// <summary>
    /// GUIDs that the magazine will eject.
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("Position that the magazine will eject.")]
    private Transform m_DropOrigin;

    /// <summary>
    /// Allows magazines to be ejected when tactically reloading.
    /// </summary>
    [SerializeField]
    [Tooltip("Allows magazines to be ejected when tactically reloading.")]
    private bool m_TacticalReloadDrop = true;

    /// <summary>
    /// Allows magazines to be ejected when full reloading.
    /// </summary>
    [SerializeField]
    [Tooltip("Allows magazines to be ejected when full reloading.")]
    private bool m_FullReloadDrop = true;

    /// <summary>
    /// Delay in seconds to eject the magazine when tactically reloading. (To match the animation)
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("Delay in seconds to eject the magazine when tactically reloading. (To match the animation)")]
    private float m_TacticalDropDelay;

    /// <summary>
    /// Delay in seconds to eject the magazine when full reloading. (To match the animation)
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("Delay in seconds to eject the magazine when full reloading. (To match the animation)")]
    private float m_FullDropDelay;

    /// <summary>
    /// Maximum number of magazines that can be instantiated by this gun.
    /// </summary>
    [SerializeField]
    [Range(0, 10)]
    [Tooltip("Maximum number of magazines that can be instantiated by this gun.")]
    private int m_MaxMagazinesPrefabs = 5;

    #endregion

    #region PROPERTIES

    /// <summary>
    /// It returns the velocity of the projectile launched by that weapon.
    /// </summary>
    public float TracerSpeed => m_TracerSpeed;

    /// <summary>
    /// Returns the duration of the tracer launched by that weapon.
    /// </summary>
    public float TracerDuration => m_TracerDuration;

    /// <summary>
    /// Returns true if this gun ejects magazines when tactically reloading, false otherwise.
    /// </summary>
    public bool TacticalReloadDrop => m_TacticalReloadDrop;

    /// <summary>
    /// Returns the delay in seconds to eject magazines when tactically reloading.
    /// </summary>
    public float TacticalDropDelay => m_TacticalDropDelay;

    /// <summary>
    /// Returns true if this gun ejects magazines when full reloading, false otherwise.
    /// </summary>
    public bool FullReloadDrop => m_FullReloadDrop;

    /// <summary>
    /// Returns the delay in seconds to eject magazines when full reloading.
    /// </summary>
    public float FullDropDelay => m_FullDropDelay;

    private int m_LastMagazine;
    private List<GameObject> m_MagazineList = new List<GameObject>();

    #endregion

    #region WEAPON CUSTOMIZATION

    /// <summary>
    /// Updates muzzle blast particle.
    /// </summary>
    /// <param name="muzzleParticle">The new muzzle blast particle</param>
    internal void UpdateMuzzleBlastParticle(ParticleSystem muzzleParticle)
    {
        m_MuzzleParticle = muzzleParticle;
    }

    #endregion

    public void Init()
    {
        if (!m_Shell || !m_ShellParticle)
            return;

        if (m_CachedParticle == null || m_CachedParticle.Length == 0)
            m_CachedParticle = m_ShellParticle.GetComponentsInChildren<ParticleSystem>();
    }

    /// <summary>
    /// Plays the effects emitted at the instant the gun fires.
    /// </summary>
    public void Play()
    {
        if (m_MuzzleFlash && m_MuzzleParticle)
        {
            m_MuzzleParticle.Play();
        }

        if (!m_Shell || !m_ShellParticle)
            return;

        ParticleSystem.MainModule mainModule = m_ShellParticle.main;
        mainModule.startSpeed = Random.Range(m_ShellSpeed.x, m_ShellSpeed.y);
        mainModule.startDelay = m_StartDelay;

        if (m_CachedParticle.Length > 0)
        {
            for (int i = 0, l = m_CachedParticle.Length; i < l; i++)
            {
                ParticleSystem.MainModule childrenModule = m_CachedParticle[i].main;
                childrenModule.startDelay = m_StartDelay;
            }
        }

        m_ShellParticle.Play();
    }

    /// <summary>
    /// Ejects a magazine after reloading the weapon.
    /// </summary>
    /// <param name="character">ColliderBounds to be ignored the magazine.</param>
    public void DropMagazine(Collider character)
    {
        if (!m_MagazineDrop)
            return;

        if (!m_MagazinePrefab || !m_DropOrigin)
            return;

        // Object pooling
        if (m_MagazineList.Count == m_MaxMagazinesPrefabs)
        {
            int magazine = m_LastMagazine++ % m_MaxMagazinesPrefabs;
            m_MagazineList[magazine].transform.position = m_DropOrigin.position;
            m_MagazineList[magazine].transform.rotation = m_DropOrigin.rotation;
            m_MagazineList[magazine].GetComponent<Rigidbody>().velocity = Physics.gravity;
        }
        else
        {
            Rigidbody magazine = Object.Instantiate(m_MagazinePrefab, m_DropOrigin.position, m_DropOrigin.rotation);
            magazine.velocity = Physics.gravity;

            Physics.IgnoreCollision(magazine.GetComponent<Collider>(), character, true);
            m_MagazineList.Add(magazine.gameObject);
        }
    }

    /// <summary>
    /// Creates a projectile tracer and instantiates with the direction of shot.
    /// </summary>
    /// <param name="origin">GUIDs where the tracer will come from.</param>
    /// <param name="direction">StartDirection where the tracer goes.</param>
    /// <param name="duration">Tracer lifespan.</param>
    public void CreateTracer(Transform origin, Vector3 direction, float duration)
    {
        if (!m_Tracer)
            return;

        if (!m_TracerPrefab || !origin)
            return;

        Rigidbody tracer = Object.Instantiate(m_TracerPrefab, m_TracerOrigin.position, origin.rotation);
        tracer.velocity = direction * m_TracerSpeed;
        Object.Destroy(tracer.gameObject, duration);
    }
}
