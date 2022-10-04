
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

[Obsolete]
    public sealed class SurfaceType : ScriptableObject
    {
        /// <summary>
        /// List of Walking Footsteps Sounds.
        /// </summary>
        [SerializeField]
        [Tooltip("List of Walking Footsteps Sounds.")]
        private List<AudioClip> m_FootstepsSounds = new List<AudioClip>();

        /// <summary>
        /// List of Sprinting Footsteps Sounds.
        /// </summary>
        [SerializeField]
        [Tooltip("List of Sprinting Footsteps Sounds.")]
        private List<AudioClip> m_SprintingFootstepsSounds = new List<AudioClip>();

        /// <summary>
        /// List of Jump Sounds.
        /// </summary>
        [SerializeField]
        [Tooltip("List of Jump Sounds.")]
        private List<AudioClip> m_JumpSounds = new List<AudioClip>();

        /// <summary>
        /// List of Landing Sounds.
        /// </summary>
        [SerializeField]
        [Tooltip("List of Landing Sounds.")]
        private List<AudioClip> m_LandingSounds = new List<AudioClip>();

        /// <summary>
        /// List of Sliding Sounds.
        /// </summary>
        [SerializeField]
        [Tooltip("List of Sliding Sounds.")]
        private List<AudioClip> m_SlidingSounds = new List<AudioClip>();

        /// <summary>
        /// Defines the minimum and maximum size of a bullet mark decal.
        /// </summary>
        [SerializeField]
        [m_MinMaxSliderAttribute(0.01f, 3, "Defines the minimum and maximum size of a bullet mark decal.", "F2")]
        private Vector2 m_DecalSize = new Vector2(0.75f, 1.5f);

        /// <summary>
        /// List of bullet mark materials.
        /// </summary>
        [SerializeField]
        [Tooltip("List of bullet mark materials.")]
        private List<Material> m_BulletImpactMaterial = new List<Material>();

        /// <summary>
        /// List of particles emitted by the impact of projectiles.
        /// </summary>
        [SerializeField]
        [Tooltip("List of particles emitted by the impact of projectiles.")]
        private List<GameObject> m_BulletImpactParticle = new List<GameObject>();

        /// <summary>
        /// List of sounds emitted by the impact of projectiles.
        /// </summary>
        [SerializeField]
        [Tooltip("List of sounds emitted by the impact of projectiles.")]
        private List<AudioClip> m_BulletImpactSound = new List<AudioClip>();
        
        /// <summary>
        /// List of sounds of projectiles bouncing off a surface.
        /// </summary>
        [SerializeField]
        [Tooltip("List of sounds of projectiles bouncing off a surface.")]
        private List<AudioClip> m_BulletRicochetSound = new List<AudioClip>();

        /// <summary>
        /// Projectiles impact sound volume.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Projectiles impact sound volume.")]
        private float m_BulletImpactVolume = 0.25f;

        #region PROPERTIES

        /// <summary>
        /// Returns the volume of impact sounds.
        /// </summary>
        public float BulletImpactVolume => m_BulletImpactVolume;

        #endregion

        /// <summary>
        /// Returns a random walking sound. The same sound will not be returned twice in sequence.
        /// </summary>
        public AudioClip GetRandomWalkingFootsteps()
        {
            if (m_FootstepsSounds.Count <= 0)
                return null;

            if (m_FootstepsSounds.Count == 1)
                return m_FootstepsSounds[0];

            int i = Random.Range(1, m_FootstepsSounds.Count);
            AudioClip a = m_FootstepsSounds[i];

            m_FootstepsSounds[i] = m_FootstepsSounds[0];
            m_FootstepsSounds[0] = a;

            return a;
        }

        /// <summary>
        /// Returns a random sprinting sound. The same sound will not be returned twice in sequence.
        /// </summary>
        public AudioClip GetRandomSprintingFootsteps()
        {
            if (m_SprintingFootstepsSounds.Count <= 0)
                return null;

            if (m_SprintingFootstepsSounds.Count == 1)
                return m_SprintingFootstepsSounds[0];

            int i = Random.Range(1, m_SprintingFootstepsSounds.Count);
            AudioClip a = m_SprintingFootstepsSounds[i];

            m_SprintingFootstepsSounds[i] = m_SprintingFootstepsSounds[0];
            m_SprintingFootstepsSounds[0] = a;

            return a;
        }

        /// <summary>
        /// Returns a random landing sound. The same sound will not be returned twice in sequence.
        /// </summary>
        public AudioClip GetRandomLandingSound()
        {
            if (m_LandingSounds.Count <= 0)
                return null;

            if (m_LandingSounds.Count == 1)
                return m_LandingSounds[0];

            int i = Random.Range(1, m_LandingSounds.Count);
            AudioClip a = m_LandingSounds[i];

            m_LandingSounds[i] = m_LandingSounds[0];
            m_LandingSounds[0] = a;

            return a;
        }

        /// <summary>
        /// Returns a random jump sound. The same sound will not be returned twice in sequence.
        /// </summary>
        public AudioClip GetRandomJumpSound()
        {
            if (m_JumpSounds.Count <= 0)
                return null;

            if (m_JumpSounds.Count == 1)
                return m_JumpSounds[0];

            int i = Random.Range(1, m_JumpSounds.Count);
            AudioClip a = m_JumpSounds[i];

            m_JumpSounds[i] = m_JumpSounds[0];
            m_JumpSounds[0] = a;

            return a;
        }

        /// <summary>
        /// Returns a random sliding sound. The same sound will not be returned twice in sequence.
        /// </summary>
        public AudioClip GetRandomSlidingSound()
        {
            if (m_SlidingSounds.Count <= 0)
                return null;

            if (m_SlidingSounds.Count == 1)
                return m_SlidingSounds[0];

            int i = Random.Range(1, m_SlidingSounds.Count);
            AudioClip a = m_SlidingSounds[i];

            m_SlidingSounds[i] = m_SlidingSounds[0];
            m_SlidingSounds[0] = a;

            return a;
        }

        /// <summary>
        /// Returns a random material used by the decal system to render a bullet mark.
        /// </summary>
        public Material GetRandomDecalMaterial()
        {
            if (m_BulletImpactMaterial.Count <= 0)
                return null;

            if (m_BulletImpactMaterial.Count == 1)
                return m_BulletImpactMaterial[0];

            int i = Random.Range(1, m_BulletImpactMaterial.Count);
            Material m = m_BulletImpactMaterial[i];

            m_BulletImpactMaterial[i] = m_BulletImpactMaterial[0];
            m_BulletImpactMaterial[0] = m;

            return m;
        }

        /// <summary>
        /// Returns a random particle effect.
        /// </summary>
        public GameObject GetRandomImpactParticle()
        {
            if (m_BulletImpactParticle.Count <= 0)
                return null;

            if (m_BulletImpactParticle.Count == 1)
                return m_BulletImpactParticle[0];

            int i = Random.Range(1, m_BulletImpactParticle.Count);
            GameObject g = m_BulletImpactParticle[i];

            m_BulletImpactParticle[i] = m_BulletImpactParticle[0];
            m_BulletImpactParticle[0] = g;

            return g;
        }

        /// <summary>
        /// Returns a random impact sound.
        /// </summary>
        public AudioClip GetRandomImpactSound()
        {
            if (m_BulletImpactSound.Count <= 0)
                return null;

            if (m_BulletImpactSound.Count == 1)
                return m_BulletImpactSound[0];

            int i = Random.Range(1, m_BulletImpactSound.Count);
            AudioClip a = m_BulletImpactSound[i];

            m_BulletImpactSound[i] = m_BulletImpactSound[0];
            m_BulletImpactSound[0] = a;

            return a;
        }
        
        /// <summary>
        /// Returns a random ricochet sound.
        /// </summary>
        public AudioClip GetRandomRicochetSound()
        {
            if (m_BulletRicochetSound.Count <= 0)
                return null;

            if (m_BulletRicochetSound.Count == 1)
                return m_BulletRicochetSound[0];

            int i = Random.Range(1, m_BulletRicochetSound.Count);
            AudioClip a = m_BulletRicochetSound[i];

            m_BulletRicochetSound[i] = m_BulletRicochetSound[0];
            m_BulletRicochetSound[0] = a;

            return a;
        }

        /// <summary>
        /// Generates a random value for the bullet mark decal scale.
        /// </summary>
        public float GetRandomDecalSize()
        {
            return Random.Range(m_DecalSize.x, m_DecalSize.y);
        }
    }
