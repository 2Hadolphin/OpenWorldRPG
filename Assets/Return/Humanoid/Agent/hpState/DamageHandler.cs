using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;




    /// <summary>
    /// Injury traumas.
    /// </summary>
    public enum TraumaType
    {
        None,
        Limp,
        //tick
        Tremor,
    }

    /// <summary>
    /// DamageHandler simulates a part of the character's body and its vital properties, like vitality and regeneration, along the trauma characteristics when it gets damaged.
    /// </summary>
    [AddComponentMenu("FPS Builder/Controllers/Damage AdditiveHandler"), DisallowMultipleComponent]
    public class DamageHandler : MonoBehaviour//, IProjectileDamageable, IExplosionDamageable, IStunnable
    {
        /// <summary>
        /// Defines whether the character can survive if vitality is equal to 0.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines whether the character can survive if vitality is equal to 0.")]
        private bool m_Vital = true;

        /// <summary>
        /// Defines how many vitality units this body part has.
        /// </summary>
        [SerializeField]
        [Range(1, Mathf.Infinity)]
        [Tooltip("Defines how many vitality units this body part has.")]
        private float m_Vitality = 100;

        /// <summary>
        /// Allows the character to get injured, compromising basic functions such as running, jumping or aiming down sights.
        /// </summary>
        [SerializeField]
        [Tooltip("Allows the character to get injured, compromising basic functions such as running, jumping or aiming down sights.")]
        private bool m_AllowInjury;

        /// <summary>
        /// Defines the minimum damage required to make the character bleed.
        /// </summary>
        [SerializeField]
        [Range(0, 100)]
        [Tooltip("Defines the minimum damage required to make the character bleed.")]
        private float m_BleedThreshold = 35;

        /// <summary>
        /// Bleed Chance is the probability to start losing blood (if the character suffer damage above the threshold) after an arbitrary damage. 
        /// If a body part is bleeding, the whole structure will be affected, leading the character to death.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Bleed Chance is the probability to start losing blood (if the character suffer damage above the threshold) after an arbitrary damage. If a body part is bleeding, the whole structure will be affected, leading the character to death.")]
        private float m_BleedChance = 0.5f;

        /// <summary>
        /// Defines the threshold between injured and healthy.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines the threshold between injured and healthy.")]
        private float m_DamageThreshold = 50;

        /// <summary>
        /// Defines the trauma due the injury to the body part.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the trauma due the injury to the body part.")]
        private TraumaType m_TraumaType = TraumaType.None;

        /// <summary>
        /// Defines how much Vitality will be drain per second.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines how much Vitality will be drain per second.")]
        private float m_BleedRate = 0.5f;

        /// <summary>
        /// Enables regeneration functionality for this body part.
        /// </summary>
        [SerializeField]
        [Tooltip("Enables regeneration functionality for this body part.")]
        private bool m_AllowRegeneration;

        /// <summary>
        /// Defines how the delay to start regenerating its vitality, in seconds.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines how the delay to start regenerating its vitality, in seconds.")]
        private float m_StartDelay = 5;

        /// <summary>
        /// Defines how many vitality units will regenerate per second.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines how many vitality units will regenerate per second.")]
        private float m_RegenerationRate = 7.5f;

        private float m_NextRegenTime;
        private bool m_Healing;

        private HealthController m_HealthController;

        #region PROPERTIES

        /// <summary>
        /// The maximum vitality of this body part. (Read Only)
        /// </summary>
        public float Vitality => m_Vitality;

        /// <summary>
        /// Returns the current vitality of this body part.
        /// </summary>
        public float CurrentVitality
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns true if this body part can be regenerated, false otherwise.
        /// </summary>
        public bool CanRegenerate => m_AllowRegeneration;

        /// <summary>
        /// Returns true if this body part is functional (vitality > 0), false otherwise.
        /// </summary>
        public bool IsAlive => CurrentVitality > 0;

        /// <summary>
        /// Returns true if this body part is injured, false otherwise.
        /// </summary>
        public bool Injured
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns true if this body part is losing blood, false otherwise.
        /// </summary>
        public bool Bleeding
        {
            get;
            set;
        }

        /// <summary>
        /// Returns true if this body part is vital (character can't survive without this body part), false otherwise.
        /// </summary>
        public bool Vital => m_Vital;


        /// <summary>
        /// Returns the trauma type when this part gets injured. (Read Only)
        /// </summary>
        public TraumaType TraumaType => m_TraumaType;

        #endregion

        /// <summary>
        /// Initialize the body part.
        /// </summary>
        /// <param name="healthController">Character HealthController reference.</param>
        public void Init(HealthController healthController)
        {
            CurrentVitality = m_Vitality;
            m_HealthController = healthController;
        }

    // do helath change
        private void Update()
        {
            // Regeneration
            if (m_AllowRegeneration && CurrentVitality > 0 && CurrentVitality < m_Vitality && m_NextRegenTime < Time.time)
            {
                CurrentVitality = Mathf.MoveTowards(CurrentVitality, m_Vitality, Time.deltaTime * m_RegenerationRate);
            }

            // Bleeding
            if (!Bleeding)
                return;

            if (m_BleedRate > 0)
                CurrentVitality = Mathf.MoveTowards(CurrentVitality, 0, Time.deltaTime * m_BleedRate);
        }

        /// <summary>
        /// Decrease vitality units from this body part.
        /// </summary>
        /// <param name="damage">Amount to be decreased.</param>
        private void ApplyDamage(float damage)
        {
            CurrentVitality = Mathf.Max(CurrentVitality - damage, 0);
            m_NextRegenTime = Time.time + m_StartDelay;
            m_Healing = false;

            if (m_AllowInjury && !Injured && CurrentVitality <= m_DamageThreshold && !m_AllowRegeneration)
                Injured = true;
        }

        public void ApplyBleedEffect(float damage)
        {
            if (Random.Range(0.0f, 1.0f) <= m_BleedChance && m_BleedThreshold <= damage && !m_AllowRegeneration)
                Bleeding = true;
        }

        /// <summary>
        /// Deals the most general type of damage.
        /// </summary>
        /// <param name="damage">Amount of damage dealt to the body part.</param>
        public void Damage(float damage)
        {
            if (damage > 0)
                ApplyDamage(damage);
        }

        /// <summary>
        /// Deals generic damage on the body and register the owner position.
        /// </summary>
        /// <param name="damage">Amount of damage dealt to the body part.</param>
        /// <param name="targetPosition">The position of the damage owner.</param>
        /// <param name="hitPosition">The world position which the damage was applied.</param>
        public void Damage(float damage, Vector3 targetPosition, Vector3 hitPosition)
        {
            if (Math.Abs(damage) < Mathf.Epsilon)
                return;

            ApplyDamage(damage);
            m_HealthController.DamageEffect(targetPosition);
            m_HealthController.GenericDamageResponse();
            m_HealthController.BulletHitEffect();
        }

        /// <summary>
        /// Deals projectile damage on the body and register the owner position.
        /// </summary>
        /// <param name="damage">Amount of damage.</param>
        /// <param name="targetPosition">The position of the damage owner.</param>
        /// <param name="hitPosition">The world position which the damage was applied.</param>
        /// <param name="penetrationPower">The penetration power of the projectile.</param>
        public void ProjectileDamage(float damage, Vector3 targetPosition, Vector3 hitPosition, float penetrationPower)
        {
            Damage(damage, targetPosition, hitPosition);
            ApplyBleedEffect(damage);

            // penetrationPower can be used to calculate how the armor will react against projectiles.
            // It will be implemented in future updates.

            m_HealthController.HitDamageResponse();
            m_HealthController.BulletHitEffect();
        }

        /// <summary>
        /// Deals explosion damage on the body and register the explosion position.
        /// </summary>
        /// <param name="damage">Amount of damage.</param>
        /// <param name="targetPosition">The position of the explosion.</param>
        /// <param name="hitPosition">The world position which the damage was applied.</param>
        public void ExplosionDamage(float damage, Vector3 targetPosition, Vector3 hitPosition)
        {
            ApplyBleedEffect(damage);

            if (Math.Abs(damage) < Mathf.Epsilon)
                return;

            ApplyDamage(damage);
            m_HealthController.DamageEffect(targetPosition);
            m_HealthController.GenericDamageResponse();
        }

        /// <summary>
        /// Applies a temporary deafness effect to the character.
        /// </summary>
        /// <param name="intensity">Intensity of the explosion.</param>
        public void DeafnessEffect(float intensity)
        {
            if (intensity > 0.5f)
                m_HealthController.ExplosionEffect(intensity);
        }

        /// <summary>
        /// Regenerates vitality units in an amount of time.
        /// </summary>
        /// <param name="healthAmount">Amount of vitality to be healed.</param>
        /// <param name="healDuration">Healing duration.</param>
        public void Heal(float healthAmount, float healDuration = 2)
        {
            m_Healing = true;
            Bleeding = false;
            Injured = false;
            StartCoroutine(HealProgressively(healthAmount, healDuration));
        }

        /// <summary>
        /// Regenerates the character progressively over time.
        /// </summary>
        /// <param name="healthAmount">Amount of vitality to be healed.</param>
        /// <param name="duration">Time needed to regenerate the player.</param>
        /// <returns></returns>
        private IEnumerator HealProgressively(float healthAmount, float duration = 1)
        {
            float targetLife = Mathf.Min(m_Vitality, CurrentVitality + healthAmount);

            for (float t = 0; t <= duration && m_Healing; t += Time.deltaTime)
            {
                CurrentVitality = Mathf.Lerp(CurrentVitality, targetLife, t / duration);

                yield return new WaitForFixedUpdate();
            }
            m_Healing = false;
        }
    }
