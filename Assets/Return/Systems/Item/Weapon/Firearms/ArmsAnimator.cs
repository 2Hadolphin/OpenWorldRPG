using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;
using Return.Humanoid;
using Return;
using Return.Audios;

[Serializable]
public sealed class ArmsAnimator : IWeaponAnimator
{
    /// <summary>
    /// Defines the order of execution of the animations.
    /// </summary>
    public enum AnimationType
    {
        Sequential,
        Random
    }

    /// <summary>
    /// Animator's reference.
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("Animator's reference.")]
    private Animator m_Animator;

    /// <summary>
    /// The name of the anim parameter that defines the playing speed.
    /// </summary>
    [SerializeField]
    [Tooltip("The name of the anim parameter that defines the playing speed.")]
    private string m_VelocityParameter = "ModuleVelocity";

    private int m_VelocityParameterHash;

    /// <summary>
    /// Enables Draw animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables Draw animation.")]
    private bool m_Draw;

    /// <summary>
    /// The name of the draw animation.
    /// </summary>
    [SerializeField]
    [Tooltip("The name of the draw animation.")]
    private string m_DrawAnimation = "Draw";

    /// <summary>
    /// The sound played when drawing the weapon.
    /// </summary>
    [SerializeField]
    [Tooltip("The sound played when drawing the weapon.")]
    private AudioClip m_DrawSound;

    /// <summary>
    /// The volume of the sound of drawing the weapon.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The volume of the sound of drawing the weapon.")]
    private float m_DrawVolume = 0.25f;

    /// <summary>
    /// Enables the animation of hiding the weapon.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables the animation of hiding the weapon.")]
    private bool m_Hide;

    /// <summary>
    /// The name of the hide the weapon animation.
    /// </summary>
    [SerializeField]
    [Tooltip("The name of the hide the weapon animation.")]
    private string m_HideAnimation = "Hide";

    /// <summary>
    /// The sound of hiding the weapon.
    /// </summary>
    [SerializeField]
    private AudioClip m_HideSound;

    /// <summary>
    /// The volume of the sound of hiding the weapon.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The volume of the sound of hiding the weapon.")]
    private float m_HideVolume = 0.25f;

    /// <summary>
    /// Enables attack animations.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables attack animations.")]
    private bool m_Attack;

    /// <summary>
    /// List of right-handed attack animations.
    /// </summary>
    [SerializeField]
    [Tooltip("List of right-handed attack animations.")]
    private List<string> m_RightAttackAnimationList = new List<string>();

    /// <summary>
    /// List of left-handed attack animations.
    /// </summary>
    [SerializeField]
    [Tooltip("List of left-handed attack animations.")]
    private List<string> m_LeftAttackAnimationList = new List<string>();

    /// <summary>
    /// Defines the order of execution of the animations.
    /// </summary>
    [SerializeField]
    [Tooltip("Defines the order of execution of the animations.")]
    private AnimationType m_AttackAnimationType = AnimationType.Sequential;

    /// <summary>
    /// The list of attack sounds.
    /// </summary>
    [SerializeField]
    [Tooltip("The list of attack sounds.")]
    private List<AudioClip> m_AttackSoundList = new List<AudioClip>();

    /// <summary>
    /// The volume of the attack sound.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The volume of the attack sound.")]
    private float m_AttackVolume = 0.5f;

    /// <summary>
    /// List of hit sounds. (Played when the character hits something with the attacks)
    /// </summary>
    [SerializeField]
    [Tooltip("List of hit sounds. (Played when the character hits something with the attacks)")]
    private List<AudioClip> m_HitSoundList = new List<AudioClip>();

    /// <summary>
    /// The volume of the hit sound.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The volume of the hit sound.")]
    private float m_HitVolume = 0.3f;

    private int m_LastIndex;

    /// <summary>
    /// Enables interaction animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables interaction animation.")]
    private bool m_Interact;

    /// <summary>
    /// Name of the interaction animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Name of the interaction animation.")]
    private string m_InteractAnimation = "Interact";

    /// <summary>
    /// The time required to send the activation signal to the object the character is interacting with.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("The time required to send the activation signal to the object the character is interacting with.")]
    private float m_InteractDelay = 0.1f;

    /// <summary>
    /// Sound played when interacting with an object.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played when interacting with an object.")]
    private AudioClip m_InteractSound;

    /// <summary>
    /// The volume of interaction sound.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The volume of interaction sound.")]
    private float m_InteractVolume = 0.2f;

    /// <summary>
    /// Enables vault animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables vault animation.")]
    private bool m_Vault;

    /// <summary>
    /// The vault animation name.
    /// </summary>
    [SerializeField]
    [Tooltip("The vault animation name.")]
    private string m_VaultAnimation = "Vault";

    /// <summary>
    /// The sound of vaulting.
    /// </summary>
    [SerializeField]
    [Tooltip("The sound of vaulting.")]
    private AudioClip m_VaultSound;

    /// <summary>
    /// The volume of the sound of vaulting.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The volume of the sound of vaulting.")]
    private float m_VaultVolume = 0.2f;

    private float m_Velocity;
 
    //private AudioEmitter m_PlayerBodySource;

    #region PROPERTIES

    /// <summary>
    /// Returns true if the anim has already been initialized, false otherwise.
    /// </summary>
    public bool Initialized
    {
        get;
        private set;
    }

    /// <summary>
    /// The duration in seconds of the draw animation.
    /// </summary>
    public float DrawAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Draw)
                return 0;

            if (m_DrawAnimation.Length == 0)
                return 0;

            return m_Animator.GetAnimationClip(m_DrawAnimation).length;
        }
    }

    /// <summary>
    /// The duration in seconds of the hide animation.
    /// </summary>
    public float HideAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Hide)
                return 0;

            if (m_HideAnimation.Length == 0)
                return 0;

            return m_Animator.GetAnimationClip(m_HideAnimation).length;
        }
    }

    /// <summary>
    /// The duration in seconds of the interact animation.
    /// </summary>
    public float InteractAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Interact)
                return 0;

            if (m_InteractAnimation.Length == 0)
                return 0;

            return m_InteractAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_InteractAnimation).length;

        }
    }

    /// <summary>
    /// The time required to send the activation signal to the object the character is interacting with.
    /// </summary>
    public float InteractDelay
    {
        get
        {
            if (!m_Interact)
                return 0;

            return m_InteractDelay;

        }
    }

    #endregion


    /// <summary>
    /// Defines the animations execution speed.
    /// </summary>
    /// <param name="running">Is the character running?</param>
    internal void SetSpeed(bool running)
    {
        //m_Velocity = Mathf.MoveTowards(m_Velocity, m_FPController.IsSliding ? 0 : running ? 1 : 0, Time.deltaTime * 5);
        m_Animator.SetFloat(m_VelocityParameterHash, m_Velocity);

        // Updates anim speed to smoothly change between walking and running animations
        //anim.speed = Mathf.Max(m_FPController.CurrentTargetForce / (m_FPController.State == ObsoleteMotionState.Running ? 10 : m_FPController.CurrentTargetForce), 0.8f);
    }

    /// <summary>
    /// Executes an attack using the left arm.
    /// </summary>
    internal void LeftAttack()
    {
        if (!m_Animator)
            return;

        if (!m_Attack)
            return;

        // Normalizes the playing speed.
        m_Animator.speed = 1;

        // Choose an animation from the list according to the defined selection method.
        if (m_LeftAttackAnimationList.Count > 0)
        {
            switch (m_AttackAnimationType)
            {
                case AnimationType.Sequential:
                    {
                        if (m_LastIndex == m_LeftAttackAnimationList.Count || m_LastIndex > m_LeftAttackAnimationList.Count)
                            m_LastIndex = 0;

                        string currentAnim = m_LeftAttackAnimationList[m_LastIndex++];

                        if (currentAnim.Length > 0)
                            m_Animator.CrossFadeInFixedTime(currentAnim, 0.1f);
                        break;
                    }
                case AnimationType.Random:
                    {
                        string currentAnim = m_LeftAttackAnimationList[Random.Range(0, m_LeftAttackAnimationList.Count)];

                        if (currentAnim.Length > 0)
                            m_Animator.CrossFadeInFixedTime(currentAnim, 0.1f);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Attack sound.
        if (m_AttackSoundList.Count > 0)
        {
            if (m_AttackSoundList.Count > 1)
            {
                int i = Random.Range(1, m_AttackSoundList.Count);
                AudioClip a = m_AttackSoundList[i];

                m_AttackSoundList[i] = m_AttackSoundList[0];
                m_AttackSoundList[0] = a;

                //m_PlayerBodySource.ForcePlay(a, m_AttackVolume);
            }
            else
            {
                //m_PlayerBodySource.ForcePlay(m_AttackSoundList[0], m_AttackVolume);
            }
        }
    }

    /// <summary>
    /// Executes an attack using the right arm.
    /// </summary>
    internal void RightAttack()
    {
        if (!m_Animator)
            return;

        if (!m_Attack)
            return;

        // Normalizes the playing speed.
        m_Animator.speed = 1;

        // Choose an animation from the list according to the defined selection method.
        if (m_RightAttackAnimationList.Count > 0)
        {
            switch (m_AttackAnimationType)
            {
                case AnimationType.Sequential:
                    {
                        if (m_LastIndex == m_RightAttackAnimationList.Count || m_LastIndex > m_RightAttackAnimationList.Count)
                            m_LastIndex = 0;

                        string currentAnim = m_RightAttackAnimationList[m_LastIndex++];

                        if (currentAnim.Length > 0)
                            m_Animator.CrossFadeInFixedTime(currentAnim, 0.1f);
                        break;
                    }
                case AnimationType.Random:
                    {
                        string currentAnim = m_RightAttackAnimationList[Random.Range(0, m_RightAttackAnimationList.Count)];

                        if (currentAnim.Length > 0)
                            m_Animator.CrossFadeInFixedTime(currentAnim, 0.1f);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Attack sound.
        if (m_AttackSoundList.Count > 0)
        {
            if (m_AttackSoundList.Count > 1)
            {
                int i = Random.Range(1, m_AttackSoundList.Count);
                AudioClip a = m_AttackSoundList[i];

                m_AttackSoundList[i] = m_AttackSoundList[0];
                m_AttackSoundList[0] = a;

                //m_PlayerBodySource.ForcePlay(a, m_AttackVolume);
            }
            else
            {
                //m_PlayerBodySource.ForcePlay(m_AttackSoundList[0], m_AttackVolume);
            }
        }
    }

    /// <summary>
    /// Plays an impact sound when hitting a target.
    /// </summary>
    /// <param name="position">Hit position.</param>
    public void Hit(Vector3 position)
    {
        //if (m_HitSoundList.Count > 0)
        //    AudioManager.Instance.PlayClipAtPoint(m_HitSoundList[Random.Range(0, m_HitSoundList.Count)], position, 3, 10, m_HitVolume);
    }

    /// <summary>
    /// Plays the draw animation.
    /// </summary>
    public void Draw()
    {
        if (!m_Animator)
            return;

        if (!m_Draw)
            return;

        if (m_DrawAnimation.Length == 0)
            return;

        // Normalizes the playing speed.
        m_Animator.speed = 1;

        m_Animator.Play(m_DrawAnimation);
        //m_PlayerBodySource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", m_FPController.transform.root, spatialBlend: 0);
        //m_PlayerBodySource.ForcePlay(m_DrawSound, m_DrawVolume);
    }

    /// <summary>
    /// Plays the hiding animation.
    /// </summary>
    public void Hide()
    {
        if (!m_Animator)
            return;

        if (!m_Hide)
            return;

        if (m_HideAnimation.Length == 0)
            return;

        // Normalizes the playing speed.
        m_Animator.speed = 1;

        m_Animator.CrossFadeInFixedTime(m_HideAnimation, 0.1f);
        //m_PlayerBodySource.Stop();
        //m_PlayerBodySource.ForcePlay(m_HideSound, m_HideVolume);
    }

    /// <summary>
    /// Plays the interact animation.
    /// </summary>
    public void Interact()
    {
        if (!m_Animator)
            return;

        if (!m_Interact)
            return;

        if (m_InteractAnimation.Length == 0)
            return;

        // Normalizes the playing speed.
        m_Animator.speed = 1;

        m_Animator.CrossFadeInFixedTime(m_InteractAnimation, 0.1f);
        //m_PlayerBodySource.ForcePlay(m_InteractSound, m_InteractVolume);
    }

    /// <summary>
    /// Plays the vaulting animation.
    /// </summary>
    public void Vault()
    {
        if (!m_Animator)
            return;

        if (!m_Vault)
            return;

        if (m_VaultAnimation.Length == 0)
            return;

        // Normalizes the playing speed.
        m_Animator.speed = 1;

        m_Animator.CrossFadeInFixedTime(m_VaultAnimation, 0.1f);
        //m_PlayerBodySource.ForcePlay(m_VaultSound, m_VaultVolume);
    }
}
