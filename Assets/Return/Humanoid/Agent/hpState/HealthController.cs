using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Return.Audios;
using Return.Preference;

/// <summary>
/// HealthController is responsible for handling all character health related functions.
/// </summary>
public class HealthController : MonoBehaviour
{
    /// <summary>
    /// List of objects that constitute the character structure.
    /// </summary>
    [SerializeField]
    protected List<DamageHandler> m_BodyParts = new List<DamageHandler>();

    #region SOUNDS

    /// <summary>
    /// Sound played when the character is healing or affected by a bonus.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played when the character is healing or affected by a bonus.")]
    protected AudioClip m_HealSound;

    /// <summary>
    /// Defines the Heal Sound volume when the character is healing or affected by a bonus.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Defines the Heal Sound volume when the character is healing or affected by a bonus.")]
    protected float m_HealVolume = 0.3f;

    /// <summary>
    /// Sound played when the character is heavily damaged.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played when the character is heavily damaged.")]
    protected AudioClip m_CoughSound;

    /// <summary>
    /// Defines the Cough Sound volume when the character is heavily damaged.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Defines the Cough Sound volume when the character is heavily damaged.")]
    protected float m_CoughVolume = 0.5f;

    /// <summary>
    /// Sound played when the player suffer a heavy fall damage.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played when the player suffer a heavy fall damage.")]
    protected AudioClip m_BreakLegsSound;

    /// <summary>
    /// Defines the Break Legs Sound volume when the player suffer a heavy fall damage.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Defines the Break Legs Sound volume when the player suffer a heavy fall damage.")]
    protected float m_BreakLegsVolume = 0.3f;

    /// <summary>
    /// List of sounds randomly played when the character is hit by an enemy.
    /// </summary>
    [SerializeField]
    protected List<AudioClip> m_HitSounds = new List<AudioClip>();

    /// <summary>
    /// List of sounds randomly played when the character is damaged.
    /// </summary>
    [SerializeField]
    protected List<AudioClip> m_DamageSounds = new List<AudioClip>();

    /// <summary>
    /// Defines the Hit Sound volume when the character is hit by an enemy.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Defines the Hit Sound volume when the character is hit by an enemy.")]
    protected float m_HitVolume = 0.3f;

    /// <summary>
    /// Sound played when the player is hit by a near explosion, simulating a temporary deafness effect.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played when the player is hit by a near explosion, simulating a temporary deafness effect.")]
    protected AudioClip m_ExplosionNoise;

    /// <summary>
    /// Defines the Explosion Noise volume when the player is hit by a near explosion, simulating a temporary deafness effect.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Defines the Explosion Noise volume when the player is hit by a near explosion, simulating a temporary deafness effect.")]
    protected float m_ExplosionNoiseVolume = 0.3f;

    /// <summary>
    /// Defines the minimum and maximum duration of the deafness effect when hit by an explosion.
    /// </summary>
    [SerializeField]
    [m_MinMaxSliderAttribute(0, 10, "Defines the minimum and maximum duration of the deafness effect when hit by an explosion.")]
    protected Vector2 m_DeafnessDuration = new Vector2(2, 5);

    /// <summary>
    /// Default Audio Mixer snapshot.
    /// </summary>
    [SerializeField]
    [Tooltip("Default Audio Mixer snapshot.")]
    protected AudioMixerSnapshot m_NormalSnapshot;

    /// <summary>
    /// Snapshot used to simulate echoing and temporary deafness.
    /// </summary>
    [SerializeField]
    [Tooltip("Snapshot used to simulate echoing and temporary deafness.")]
    protected AudioMixerSnapshot m_StunnedSnapshot;

    #endregion

    /// <summary>
    /// The dead character prefab. (Instantiated when the character dies).
    /// </summary>
    [SerializeField]
    [Tooltip("The dead character prefab. (Instantiated when the character dies).")]
    protected GameObject m_DeadCharacter;

    #region EVENTS

    /// <summary>
    /// The DamageEvent is called right after the character takes damage.
    /// </summary>
    public event Action<Vector3> DamageEvent;

    /// <summary>
    /// The ExplosionEvent is called right after the character takes damage by an explosion.
    /// </summary>
    public event Action ExplosionEvent;

    /// <summary>
    /// The HitEvent is called right after the character gets shot by a projectile.
    /// </summary>
    public event Action HitEvent;

    #endregion

    //private BloodSplashEffect m_BloodSplashEffect;
    //private m_MotionSystem m_FPController;


    private float m_TotalVitality;
    private float m_CurrentVitality;
    private float m_TempVitality;

    #region PROPERTIES

    /// <summary>
    /// Returns true if the character is bleeding, false otherwise.
    /// </summary>
    public bool Bleeding
    {
        get;
        private set;
    }

    /// <summary>
    /// Returns true if the character is limping, false otherwise.
    /// </summary>
    public bool Limping
    {
        get;
        private set;
    }

    /// <summary>
    /// Returns true if the character is trembling, false otherwise.
    /// </summary>
    public bool Trembling
    {
        get;
        private set;
    }

    /// <summary>
    /// Returns true if the character is injured, false otherwise.
    /// </summary>
    public bool Injured
    {
        get;
        private set;
    }

    /// <summary>
    /// Returns true if the character is alive (vitality > 0), false otherwise.
    /// </summary>
    public virtual bool IsAlive
    {
        get
        {
            if (m_BodyParts.Count == 0)
                return true;

            return m_CurrentVitality > 0;
        }
    }

    /// <summary>
    /// Returns the remaining vitality percentage.
    /// </summary>
    public float HealthPercent
    {
        get
        {
            if (m_BodyParts.Count == 0)
                return 1;
            return m_CurrentVitality / m_TotalVitality;
        }
    }

    #endregion

    #region EDITOR

    /// <summary>
    /// Calculates the value of the character's vitality based on all registered body parts.
    /// </summary>
    public float EditorHealthPercent
    {
        get
        {
            if (Math.Abs(m_CurrentVitality) < Mathf.Epsilon)
                return 1;
            return m_CurrentVitality / m_TotalVitality;
        }
    }

    /// <summary>
    /// Used to add body parts by the character creation script.
    /// </summary>
    /// <param name="bodyPart">The new body part.</param>
    public void RegisterBodyPart(DamageHandler bodyPart)
    {
        m_BodyParts.Add(bodyPart);
    }

    #endregion

    protected virtual void Start()
    {
        // References
        //m_FPController = GetComponent<FirstPersonCharacterController>();
        //m_BloodSplashEffect = GetComponentInChildren<BloodSplashEffect>();
        //m_FPController.LandingEvent += FallDamage;



        // Body parts
        for (int i = 0, bodyPartsCount = m_BodyParts.Count; i < bodyPartsCount; i++)
        {
            if (!m_BodyParts[i])
                continue;

            m_BodyParts[i].Init(this);
            m_TotalVitality += m_BodyParts[i].Vitality;
        }
        m_CurrentVitality = m_TotalVitality;
    }

    protected virtual void Update()
    {
        if (m_BodyParts.Count < 1)
            return;

        m_TempVitality = 0;
        for (int i = 0, bodyPartsCount = m_BodyParts.Count; i < bodyPartsCount; i++)
        {
            if (!m_BodyParts[i])
                continue;

            // Sets if the player is dead if any vital body part vitality is equal to 0.
            //SettingManager.m_Instance.IsDead |= (Mathf.Abs(m_BodyParts[i].CurrentVitality) < Mathf.Epsilon && m_BodyParts[i].Vital);

            if (m_BodyParts[i].Injured)
            {
                Injured = true;
                if (m_BodyParts[i].TraumaType == TraumaType.Limp)
                    Limping = true;
                if (m_BodyParts[i].TraumaType == TraumaType.Tremor)
                    Trembling = true;
            }

            if (m_BodyParts[i].Bleeding)
            {
                Bleeding = true;
            }

            // Make the character lose blood from the entire structure.
            if (Bleeding && !m_BodyParts[i].Bleeding)
            {
                m_BodyParts[i].Bleeding = true;
            }

            m_TempVitality += m_BodyParts[i].CurrentVitality;
        }

        m_CurrentVitality = m_TempVitality;
        //SettingManager.m_Instance.IsDead |= Mathf.Abs(m_CurrentVitality) < Mathf.Epsilon;

        // Sets whether or not the character has broken their legs.
        //m_FPController.LowerBodyDamaged = Limping;
        //m_FPController.TremorTrauma = Trembling;

        //if (m_BloodSplashEffect)
        //{
        //    m_BloodSplashEffect.BloodAmount = 1 - m_CurrentVitality / m_TotalVitality;
        //}

        if (SettingsManager.Instance.IsDead && m_DeadCharacter != null)
        {
            Die();
        }
    }

    protected void LateUpdate()
    {

    }

    /// <summary>
    /// Instantly regenerates vitality and applies an adrenaline bonus.
    /// </summary>
    /// <param name="healthAmount">Amount of vitality to be healed.</param>
    /// <param name="healInjuries">Should the character injuries be healed?</param>
    /// <param name="bonus">Allow adrenaline bonus?</param>
    /// <param name="bonusDuration">Duration of the adrenaline bonus.</param>
    public virtual void Heal(float healthAmount, bool healInjuries, bool bonus = false, float bonusDuration = 10)
    {
        if (healthAmount > 0 && m_BodyParts.Count > 0)
        {
            for (int i = 0, bodyPartsCount = m_BodyParts.Count; i < bodyPartsCount; i++)
            {
                if (m_BodyParts[i])
                {
                    m_BodyParts[i].Heal(healthAmount / bodyPartsCount); // Heals each body part by the same amount.
                }
            }

            Invoke(nameof(SetNormalSnapshot), 0);

            if (healInjuries)
            {
                Limping = false;
                Trembling = false;
                Bleeding = false;
            }

            //if (bonus)
            //    StartCoroutine(m_FPController.AdrenalineShot(bonusDuration));
        }
    }

    /// <summary>
    /// Deals damage on the lower body parts (legs).
    /// </summary>
    protected virtual void FallDamage(float damage)
    {
        for (int i = 0, bodyPartsCount = m_BodyParts.Count; i < bodyPartsCount; i++)
        {
            if (!m_BodyParts[i])
                continue;

            //if (m_BodyParts[i].BodyPart == BodyPart.Leg && !m_BodyParts[i].CanRegenerate)
            {
                m_BodyParts[i].Damage(damage);

                if (m_BodyParts[i].Injured)
                {
                    Limping = true;
                    if (!m_BodyParts[i].CanRegenerate)
                        m_BodyParts[i].ApplyBleedEffect(damage);

                    //m_PlayerHealthSource.Stop();
                    //AudioManager.Instance.PlayClipAtPoint(m_BreakLegsSound, transform.position, 5, 10, m_BreakLegsVolume);
                }
            }



            m_BodyParts[i].Damage(damage);
            if (damage > m_BodyParts[i].Vitality * 0.7f && !m_BodyParts[i].CanRegenerate)
            {
                Limping = true;
                m_BodyParts[i].ApplyBleedEffect(damage);

                //m_PlayerHealthSource.Stop();
                //AudioManager.Instance.PlayClipAtPoint(m_BreakLegsSound, transform.position, 5, 10, m_BreakLegsVolume);
            }
        }
    }

    /// <summary>
    /// Call the DamageEvent to execute methods related to damage taking.
    /// </summary>
    /// <param name="targetPosition">The position of the damage author.</param>
    public virtual void DamageEffect(Vector3 targetPosition)
    {
        DamageEvent?.Invoke(targetPosition);
    }

    /// <summary>
    /// Plays the character's responses when suffering damage from projectiles.
    /// </summary>
    public virtual void HitDamageResponse()
    {
        if (m_HitSounds.Count <= 0)
            return;

        if (m_HitSounds.Count == 1)
        {
            //m_PlayerHealthSource.Play(m_HitSounds[0], m_HitVolume);
            return;
        }

        int i = UnityEngine.Random.Range(1, m_HitSounds.Count);
        AudioClip a = m_HitSounds[i];

        m_HitSounds[i] = m_HitSounds[0];
        m_HitSounds[0] = a;

        //m_PlayerHealthSource.Play(a, m_HitVolume);
    }

    /// <summary>
    /// Plays the character's responses when suffering damage from generic sources.
    /// </summary>
    public virtual void GenericDamageResponse()
    {
        if (m_DamageSounds.Count <= 0)
            return;

        if (m_DamageSounds.Count == 1)
        {
            //m_PlayerHealthSource.Play(m_DamageSounds[0], m_HitVolume);
            return;
        }

        int i = UnityEngine.Random.Range(1, m_DamageSounds.Count);
        AudioClip a = m_DamageSounds[i];

        m_DamageSounds[i] = m_DamageSounds[0];
        m_DamageSounds[0] = a;

        //m_PlayerHealthSource.Play(a, m_HitVolume);
    }

    /// <summary>
    /// Simulates the effect of getting hit by a projectile.
    /// </summary>
    public virtual void BulletHitEffect()
    {
        HitEvent?.Invoke();
    }

    /// <summary>
    /// Simulates the effect of getting hit by an explosion.
    /// </summary>
    public virtual void ExplosionEffect(float intensity)
    {
        //m_PlayerHealthSource.ForcePlay(m_ExplosionNoise, m_ExplosionNoiseVolume);
        m_StunnedSnapshot.TransitionTo(0.1f);

        float duration = m_DeafnessDuration.x + (m_DeafnessDuration.y - m_DeafnessDuration.x) * intensity;
        Invoke(nameof(SetNormalSnapshot), duration);

        ExplosionEvent?.Invoke();
    }

    /// <summary>
    /// Disable effects of echoing and temporary deafness.
    /// </summary>
    protected virtual void SetNormalSnapshot()
    {
        m_NormalSnapshot.TransitionTo(0.3f);
    }

    /// <summary>
    /// Disables the character and instantiates its dead Version.
    /// </summary>
    protected virtual void Die()
    {
        SetNormalSnapshot();
        gameObject.SetActive(false);

        Transform t = transform;
        Instantiate(m_DeadCharacter, t.position, t.rotation);
    }
}
