using System;
using System.Collections;
using System.Collections.Generic;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using Return;
using Return.Cameras;
using Return.Items;
using Return.Items.Weapons;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-betterspringrecoilhandler.html")]
    public class BetterSpringRecoilHandler : BaseRecoilHandlerBehaviour, IAdditiveTransform
    {
        void activate()
        {
            //var recoils = item.Resolver.GetModules<IFirearmsRecoilPort>();

            //foreach (var recoil in recoils)
            //{
            //    recoil.OnRecoil += Recoil_OnRecoil;
            //}

            //if(item.Resolver.TryGetModule(out AdditivePlayer additivePlayer))
            //    additivePlayer.ApplyAdditiveEffect(this);

            // Get the weapon recoil effect (or add if not found
            //m_FirearmTransformHandler = firearm.GetComponentInChildren<IAdditiveTransformHandler>();
            //m_WeaponRecoilEffect = m_FirearmTransformHandler.CharacterRoot.GetComponent<FirearmRecoilEffect>();
            //if (m_WeaponRecoilEffect == null)
            //    m_WeaponRecoilEffect = m_FirearmTransformHandler.CharacterRoot.AddComponent<FirearmRecoilEffect>();

            // Subscribe to firearm wielder
            //firearm.onWielderChanged += OnWielderChanged;
            //OnWielderChanged(firearm.wielder);
        }

        private void Recoil_OnRecoil(bl_RecoilBase.RecoilData obj)
        {
            Recoil();
        }


        [Header("Hip Fire Recoil")]

        [SerializeField]
        private RecoilProfile m_HipFireRecoil = new RecoilProfile();

        [Header("Aiming Recoil")]

        [SerializeField]
        private RecoilProfile m_AimedRecoil = new RecoilProfile();
        
        private const float k_MaxJiggle = 5f;

        [Serializable]
        public class RecoilProfile
        {
#if UNITY_EDITOR
            [HideInInspector] public bool expanded = true;
#endif

            [Tooltip("The angle to rotate the gun on recoil (before modifiers).")]
            public float recoilAngle = 0.5f;
            [Range(0, 1f), Tooltip("The maximum amount of side to side movement during a recoil. At wander = 1, the gun can rotate anything up to 90 degrees from up. At 0, the gun will only rotate upwards.")]
            public float wander = 0.2f;
            [Tooltip("A multiplier applied to any horizontal rotation of the recoil.")]
            public float horizontalMultiplier = 1f;
            [Range(0, 1f), Tooltip("The split between head and weapon recoil. At 0, the head/body will rotate, and the firearm will move with it. At 1, the firearm will rotate and the head/body will remain still.")]
            public float verticalDivergence = 0.05f;
            [Range(-1, 1f), Tooltip("The split between head and weapon recoil. At 0, the head/body will rotate, and the firearm will move with it. At 1, the firearm will rotate and the head/body will remain still.")]
            public float horizontalDivergence = 0.2f;
            [Tooltip("The distance the firearm will be pushed backwards each shot.")]
            public float pushBack = 0.005f;
            [Tooltip("The maximum distance the firearm can be pushed back.")]
            public float maxPushBack = 0.03f;
            [Range(0, 1f), Tooltip("The amount of jiggle (spring rotation around the z-axis) to be applied to the firearm.")]
            public float jiggle = 0.2f;
            [Tooltip("The time taken for the head and firearm to return to their pre-recoil state.")]
            public float duration = 0.5f;

            [Tooltip("The animation curve used to drive the amount of recoil rotation over the duration of the recoil effect.")]
            public AnimationCurve recoilSpringCurve = new AnimationCurve(new Keyframe[] {
            new Keyframe(0f, 0f, 0f, 50f), new Keyframe(0.05f, 1f), new Keyframe(0.7f, -0.05f), new Keyframe(0.9f, 0.02f), new Keyframe(1f, 0f)
            });
            [Tooltip("The animation curve used to drive the firearm's jiggle rotation over the duration of the recoil effect.")]
            public AnimationCurve weaponJiggleCurve = new AnimationCurve(new Keyframe[] {
            new Keyframe(0f, 0f, 0f, 50f), new Keyframe(0.05f, 1f), new Keyframe(0.35f, -0.35f), new Keyframe(0.55f, 0.25f), new Keyframe(0.75f, -0.05f), new Keyframe(1f, 0f)
            });
            [Tooltip("The animation curve used to drive the firearm's push-back over the duration of the recoil effect.")]
            public AnimationCurve weaponPushCurve = new AnimationCurve(new Keyframe[] {
            new Keyframe(0f, 0f, 0f, 50f), new Keyframe(0.05f, 1f), new Keyframe(0.25f, -0.05f), new Keyframe(0.65f, 0.02f), new Keyframe(1f, 0f)
            });

            public void OnValidate()
            {
				recoilAngle = Mathf.Clamp(recoilAngle, 0f, 45f);
				horizontalMultiplier = Mathf.Clamp(horizontalMultiplier, 0f, 10f);
				pushBack = Mathf.Clamp(pushBack, 0f, 0.5f);
				maxPushBack = Mathf.Clamp(maxPushBack, 0f, 0.5f);
				duration = Mathf.Clamp(duration, 0.05f, 5f);
            }
        }

        private IAdditiveTransformHandler m_FirearmTransformHandler = null;
        //private FirearmRecoilEffect m_WeaponRecoilEffect = null;
        private CharacterRecoilEffect m_HeadRecoilEffect = null;
        private CharacterRecoilEffect m_BodyRecoilEffect = null;
        private float m_MoveMultiplier = 1f;
        private float m_RotateMultiplier = 1f;

        protected override void OnValidate()
        {
            base.OnValidate();

            m_HipFireRecoil.OnValidate();
            m_AimedRecoil.OnValidate();
        }

        public override bool isModuleValid
        {
            get { return true; }
        }


        //private void OnWielderChanged(ICharacter wielder)
        //{
        //    m_HeadRecoilEffect = null;
        //    m_BodyRecoilEffect = null;

        //    if (wielder != null)
        //    {
        //        // Get the head recoil effect (or add if not found)
        //        if (wielder.headTransformHandler != null)
        //        {
        //            m_HeadRecoilEffect = wielder.headTransformHandler.CharacterRoot.GetComponent<CharacterRecoilEffect>();
        //            if (m_HeadRecoilEffect == null)
        //                m_HeadRecoilEffect = wielder.headTransformHandler.CharacterRoot.AddComponent<CharacterRecoilEffect>();
        //        }

        //        // Get the body recoil effect (or add if not found)
        //        if (wielder.bodyTransformHandler != null)
        //        {
        //            m_BodyRecoilEffect = wielder.bodyTransformHandler.CharacterRoot.GetComponent<CharacterRecoilEffect>();
        //            if (m_BodyRecoilEffect == null)
        //                m_BodyRecoilEffect = wielder.bodyTransformHandler.CharacterRoot.AddComponent<CharacterRecoilEffect>();
        //        }
        //    }
        //}

        [Button]
        public override void Recoil()
        {
            base.Recoil();

            //if (firearm.aimer != null && firearm.aimer.isAiming)
            //    StartCoroutine(RecoilInternal(m_AimedRecoil));
            //else
                StartCoroutine(RecoilInternal(m_HipFireRecoil));
        }

        private IEnumerator RecoilInternal(RecoilProfile p)
        {
            yield return null;

            // Get the recoil angles
            Vector2 recoil = new(0f, p.recoilAngle);
            if (p.wander != 0f)
            {
                var random = UnityEngine.Random.Range(-1f, 1f);
                random = Mathf.Sign(random) * (1f - (random * random));
                float tilt = random * Mathf.Deg2Rad * 90f * p.wander;


                //float tilt = UnityEngine.Random.Distance(-90f, 90f) * p.wander;
                recoil = new Vector2(Mathf.Sin(tilt), Mathf.Cos(tilt));
                recoil.x *= p.recoilAngle * p.horizontalMultiplier;
                recoil.y *= p.recoilAngle;
            }

            if (m_BodyRecoilEffect != null)
            {
                Vector2 bodyRecoil = recoil;
                bodyRecoil.x *= 1f - p.horizontalDivergence;
                bodyRecoil.y *= 1f - p.verticalDivergence;
                m_BodyRecoilEffect.AddRecoil(bodyRecoil, p.duration, p.recoilSpringCurve);
            }

            //if (m_WeaponRecoilEffect != null)
            {
                // Get recoil angle
                Vector2 weaponRecoil = recoil;
                weaponRecoil.x *= p.horizontalDivergence;
                weaponRecoil.y *= p.verticalDivergence;

                // Get pushback...

                // Get jiggle
                float jiggle = 0f;
                if (p.jiggle > 0f)
                {
                    var random = UnityEngine.Random.Range(-1f, 1f);
                    random = Mathf.Sign(random) * (1f - (random * random));
                    jiggle = random * k_MaxJiggle * p.jiggle;
                }

                /*m_WeaponRecoilEffect.*/
                AddRecoil(
                    weaponRecoil, 
                    p.recoilSpringCurve,
                    p.pushBack,
                    p.maxPushBack, 
                    p.weaponPushCurve,
                    jiggle, 
                    p.weaponJiggleCurve,
                    p.duration
                    );
            }
        }
        
        public override void SetRecoilMultiplier(float move, float rotation)
        {
            m_MoveMultiplier = move;
            m_RotateMultiplier = rotation;

            // Apply to weapon
            m_FirearmTransformHandler.springPositionMultiplier = move;
            m_FirearmTransformHandler.springRotationMultiplier = rotation;

            // Apply to character
            //if (firearm.wielder != null)
            //{
            //    var t = firearm.wielder.headTransformHandler;
            //    if (t != null)
            //    {
            //        t.springPositionMultiplier = move;
            //        t.springRotationMultiplier = rotation;
            //    }

            //    t = firearm.wielder.bodyTransformHandler;
            //    if (t != null)
            //    {
            //        t.springPositionMultiplier = move;
            //        t.springRotationMultiplier = rotation;
            //    }
            //}
        }

        #region IRecoil

        private AnimationCurve m_RotationCurve = null;
        private AnimationCurve m_JiggleCurve = null;
        private AnimationCurve m_PushBackCurve = null;
        private Vector2 m_StartRotation = Vector2.zero;
        private Vector2 m_RecoilRotation = Vector2.zero;
        private float m_StartJiggle = 0f;
        private float m_RecoilJiggle = 0f;
        private float m_StartPushBack = 0f;
        private float m_RecoilPushBack = 0f;
        private float m_InverseDuration = 1f;
        private float m_Lerp = 1f;

        public IAdditiveTransformHandler transformHandler
        {
            get;
            private set;
        }

        public virtual Quaternion rotation
        {
            get
            {
                if (m_Lerp < 1f)
                    return Quaternion.Euler(currentRecoilAngle.x, currentRecoilAngle.y, currentJiggle);
                else
                    return Quaternion.identity;
            }
        }

        public virtual Vector3 position
        {
            get
            {
                if (m_Lerp < 1f)
                    return new Vector3(0f, 0f, -currentPushBack);
                else
                    return Vector3.zero;
            }
        }

        public virtual bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public virtual bool bypassRotationMultiplier
        {
            get { return true; }
        }

        public Vector2 currentRecoilAngle
        {
            get;
            private set;
        }

        public float currentJiggle
        {
            get;
            private set;
        }

        public float currentPushBack
        {
            get;
            private set;
        }

        protected override void Awake()
        {
            transformHandler = GetComponent<IAdditiveTransformHandler>();
        }

        protected override void OnEnable()
        {
            transformHandler.ApplyAdditiveEffect(this);
        }

        protected override void OnDisable()
        {
            transformHandler.RemoveAdditiveEffect(this);
        }

        public virtual void UpdateTransform()
        {
            if (m_Lerp < 1f)
            {
                m_Lerp += Time.deltaTime * m_InverseDuration;
                if (m_Lerp >= 1f)
                {
                    m_Lerp = 1f;
                    currentRecoilAngle = Vector2.zero;
                    currentJiggle = 0f;
                    currentPushBack = 0f;
                }
                else
                {
                    // Get the "spring from" values
                    float eased = 1f - EasingFunctions.EaseInOutQuadratic(m_Lerp);
                    Vector2 rotationFrom = m_StartRotation * eased;
                    float jiggleFrom = m_StartJiggle * eased;
                    float pushFrom = m_StartPushBack * eased;

                    // Evaluate the spring curves
                    currentRecoilAngle = Vector2.LerpUnclamped(rotationFrom, m_RecoilRotation, m_RotationCurve.Evaluate(m_Lerp));
                    currentJiggle = Mathf.LerpUnclamped(jiggleFrom, m_RecoilJiggle, m_JiggleCurve.Evaluate(m_Lerp));
                    currentPushBack = Mathf.LerpUnclamped(pushFrom, m_RecoilPushBack, m_PushBackCurve.Evaluate(m_Lerp));
                }
            }
        }

        protected void AddRecoil(Vector2 recoil, AnimationCurve angleCurve, float pushBack, float maxPushDistance, AnimationCurve pushCurve, float jiggle, AnimationCurve jiggleCurve, float duration)
        {
            // Set the animation curves
            m_RotationCurve = angleCurve;
            m_JiggleCurve = jiggleCurve;
            m_PushBackCurve = pushCurve;

            // Get the starting values
            m_StartRotation = currentRecoilAngle;
            m_StartJiggle = currentJiggle;
            m_StartPushBack = currentPushBack;

            // Calculate the new recoil rotation
            m_RecoilRotation = m_StartRotation + new Vector2(-recoil.y, recoil.x);

            // Calculate the new jiggle & pushback
            m_RecoilJiggle = jiggle;
            m_RecoilPushBack = m_StartPushBack + pushBack;
            if (m_RecoilPushBack > maxPushDistance)
                m_RecoilPushBack = maxPushDistance;

            // Sort timing
            m_InverseDuration = 1f / Mathf.Max(duration, 0.001f);
            m_Lerp = 0f;
        }

        #endregion


        //private static readonly NeoSerializationKey k_MoveKey = new NeoSerializationKey("move");
        //private static readonly NeoSerializationKey k_RotateKey = new NeoSerializationKey("rotate");

        //public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        //{
        //    base.WriteProperties(writer, nsgo, saveMode);
        //    if (saveMode == SaveMode.Default)
        //    {
        //        writer.WriteValue(k_MoveKey, m_MoveMultiplier);
        //        writer.WriteValue(k_RotateKey, m_RotateMultiplier);
        //    }
        //}

        //public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        //{
        //    base.ReadProperties(reader, nsgo);
        //    if (reader.TryReadValue(k_MoveKey, out m_MoveMultiplier, m_MoveMultiplier) ||
        //        reader.TryReadValue(k_RotateKey, out m_RotateMultiplier, m_RotateMultiplier))
        //    {
        //        SetRecoilMultiplier(m_MoveMultiplier, m_RotateMultiplier);
        //    }
        //}


    }
}