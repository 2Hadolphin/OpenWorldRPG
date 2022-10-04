
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;
using Return.Audios;
using Return;
using Return.Preference;

[Serializable]
public sealed class GunAnimator : IWeaponAnimator
{
    /// <summary>
    /// Defines the order of execution of the animations.
    /// </summary>
    public enum AnimationType
    {
        Sequential,
        Random
    }

    #region RUNNING

    /// <summary>
    /// Enables running animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables running animation.")]
    private bool m_RunAnimation;

    /// <summary>
    /// GUIDs of the transform when the character starts running.
    /// </summary>
    [SerializeField]
    [Tooltip("Position of the transform when the character starts running")]
    private Vector3 m_RunningPosition;

    /// <summary>
    /// Rotation of the transform when the character starts running.
    /// </summary>
    [SerializeField]
    [Tooltip("Rotation of the transform when the character starts running.")]
    private Vector3 m_RunningRotation;

    /// <summary>
    /// Speed of interpolation between HIP and Running position/rotation.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("Speed of interpolation between HIP and Running position/rotation.")]
    private float m_RunningSpeed = 10;

    #endregion

    #region SLIDING

    /// <summary>
    /// Enables slide animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables slide animation.")]
    private bool m_SlidingAnimation;

    /// <summary>
    /// GUIDs of the transform when the character starts sliding.
    /// </summary>
    [SerializeField]
    [Tooltip("Position of the transform when the character starts sliding.")]
    private Vector3 m_SlidingPosition;

    /// <summary>
    /// Rotation of the transform when the character starts sliding.
    /// </summary>
    [SerializeField]
    [Tooltip("Rotation of the transform when the character starts sliding.")]
    private Vector3 m_SlidingRotation;

    /// <summary>
    /// Speed of interpolation between HIP and Sliding position/rotation.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("Speed of interpolation between HIP and Sliding position/rotation.")]
    private float m_SlidingSpeed = 10;

    #endregion

    #region AIMING

    /// <summary>
    /// Enables aiming animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables aiming animation.")]
    private bool m_AimAnimation;

    /// <summary>
    /// ReadOnlyTransform aiming position.
    /// </summary>
    [SerializeField]
    [Tooltip("ReadOnlyTransform aiming position.")]
    private Vector3 m_AimingPosition;

    /// <summary>
    /// ReadOnlyTransform aiming rotation.
    /// </summary>
    [SerializeField]
    [Tooltip("ReadOnlyTransform aiming rotation.")]
    private Vector3 m_AimingRotation;

    /// <summary>
    /// Sound played when aiming the gun.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played when aiming the gun.")]
    private AudioClip m_AimInSound;

    /// <summary>
    /// Sound played when stop aiming the gun.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played when stop aiming the gun.")]
    private AudioClip m_AimOutSound;

    /// <summary>
    /// Enables camera FOV zoom animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables camera FOV zoom animation.")]
    private bool m_ZoomAnimation;

    /// <summary>
    /// Camera field of view while aiming.
    /// </summary>
    [SerializeField]
    [Range(1, 179)]
    [Tooltip("Camera field of view while aiming.")]
    private float m_AimFOV = 50;

    /// <summary>
    /// Camera zoom speed.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("Camera zoom speed.")]
    private float m_AimingSpeed = 10;

    /// <summary>
    /// Defines whether the character can hold his breath to stabilize the aiming.
    /// </summary>
    [SerializeField]
    [Tooltip("Defines whether the character can hold his breath to stabilize the aiming.")]
    private bool m_HoldBreath;

    #endregion

    #region STANDING / CROUCH

    /// <summary>
    /// Standing GUIDs is the default position of the weapon.
    /// </summary>
    [SerializeField]
    [Tooltip("Standing Position is the default position of the weapon.")]
    private Vector3 m_StandingPosition;

    /// <summary>
    /// Standing Rotation is the default rotation of the weapon.
    /// </summary>
    [SerializeField]
    [Tooltip("Standing Rotation is the default rotation of the weapon.")]
    private Vector3 m_StandingRotation;

    /// <summary>
    /// Crouch GUIDs will define the weapon position when the current state of the character is Crouch.
    /// </summary>
    [SerializeField]
    [Tooltip("Crouch Position will define the weapon position when the current state of the character is Crouch.")]
    private Vector3 m_CrouchPosition;

    /// <summary>
    /// Crouch Rotation will define the weapon rotation when the current state of the character is Crouch.
    /// </summary>
    [SerializeField]
    [Tooltip("Crouch Rotation will define the weapon rotation when the current state of the character is Crouch.")]
    private Vector3 m_CrouchRotation;

    /// <summary>
    /// Determines how fast the weapon can change from standing to crouching.
    /// </summary>
    [SerializeField]
    [Tooltip("Determines how fast the weapon can change from standing to crouching.")]
    [Range(0.001f, Mathf.Infinity)]
    private float m_CrouchingSpeed = 5;

    #endregion

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
    private string m_SpeedParameter = "Speed";

    private int m_SpeedParameterHash;

    #region DRAW

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
    /// Execution speed of the draw animation.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Execution speed of the draw animation.")]
    private float m_DrawSpeed = 1;

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

    #endregion

    #region HIDE

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
    /// Execution speed of the hiding animation.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Execution speed of the hiding animation.")]
    private float m_HideSpeed = 1;

    /// <summary>
    /// The sound of hiding the weapon.
    /// </summary>
    [SerializeField]
    [Tooltip("The sound of hiding the weapon.")]
    private AudioClip m_HideSound;

    /// <summary>
    /// The volume of the sound of hiding the weapon.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The volume of the sound of hiding the weapon.")]
    private float m_HideVolume = 0.25f;

    #endregion

    #region FIRE

    /// <summary>
    /// Enables shooting animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables shooting animation.")]
    private bool m_Fire;

    /// <summary>
    /// List of shooting animations.
    /// </summary>
    [SerializeField]
    [Tooltip("List of shooting animations.")]
    private List<string> m_FireAnimationList = new List<string>();

    /// <summary>
    /// List of shooting animations while aiming.
    /// </summary>
    [SerializeField]
    [Tooltip("List of shooting animations while aiming.")]
    private List<string> m_AimedFireAnimationList = new List<string>();

    /// <summary>
    /// Defines the order of execution of the animations.
    /// </summary>
    [SerializeField]
    [Tooltip("Defines the order of execution of the animations.")]
    private AnimationType m_FireAnimationType = AnimationType.Sequential;

    /// <summary>
    /// List of shooting sounds.
    /// </summary>
    [SerializeField]
    [Tooltip("List of shooting sounds.")]
    private List<AudioClip> m_FireSoundList = new List<AudioClip>();

    /// <summary>
    /// Replaces last shot animation to simulate the slide stop animation.
    /// When you’ve fired the last round in your pistol’s magazine,
    /// the magazine’s follower pushes up against the slide stop and causes it to catch in a recess in the pistol’s slide.
    /// </summary>
    [SerializeField]
    [Tooltip("Replaces last shot animation to simulate the slide stop animation.")]
    private bool m_OverrideLastFire;

    /// <summary>
    /// Shooting and slide stop animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Shooting and slide stop animation.")]
    private string m_LastFireAnimation = "Last Fire";

    /// <summary>
    /// Defines the speed at which shooting animations are executed.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Defines the speed at which shooting animations are executed.")]
    private float m_FireSpeed = 1;

    /// <summary>
    /// Defines the speed at which aimed shooting animations are executed.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Defines the speed at which aimed shooting animations are executed.")]
    private float m_AimedFireSpeed = 1;

    /// <summary>
    /// Sound played when the gun is unloaded.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played when the gun is unloaded.")]
    private AudioClip m_OutOfAmmoSound;

    /// <summary>
    /// The shooting sound volume.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The shooting sound volume.")]
    private float m_FireVolume = 0.5f;

    /// <summary>
    /// Allows the firing sound to be additive, creating a new audio source instance at each shot, otherwise the gun will reset the audio source with each new shot.
    /// </summary>
    [SerializeField]
    [Tooltip("Allows the firing sound to be additive, creating a new audio source instance at each shot, otherwise the gun will reset the audio source with each new shot.")]
    private bool m_AdditiveSound = true;

    private int m_LastIndex;

    #endregion

    #region RELOAD

    #region MAGAZINES

    /// <summary>
    /// Enables reload animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables reload animation.")]
    private bool m_Reload;

    /// <summary>
    /// Name of the tactical reload animation.
    /// (Tactical reload is a faster recharging animation, executed when there are still bullets in the chamber)
    /// </summary>
    [SerializeField]
    [Tooltip("Name of the tactical reload animation.")]
    private string m_ReloadAnimation = "Reload";

    /// <summary>
    /// Reload animation speed.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Reload animation speed.")]
    private float m_ReloadSpeed = 1;

    /// <summary>
    /// Reload animation sound.
    /// </summary>
    [SerializeField]
    [Tooltip("Reload animation sound.")]
    private AudioClip m_ReloadSound;

    /// <summary>
    /// Name of the full reload animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Name of the full reload animation.")]
    private string m_ReloadEmptyAnimation = "FullReload";

    /// <summary>
    /// Full reload animation speed.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    private float m_ReloadEmptySpeed = 1;

    /// <summary>
    /// Full reload animation sound.
    /// </summary>
    [SerializeField]
    [Tooltip("Full reload animation sound.")]
    private AudioClip m_ReloadEmptySound;

    /// <summary>
    /// Reload sound volume.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Reload sound volume.")]
    private float m_ReloadVolume = 0.25f;

    #endregion

    #region BULLET BY BULLET

    /// <summary>
    /// Animation played at the start of the reload. (Used to position gun to receive bullets)
    /// </summary>
    [SerializeField]
    [Tooltip("Animation played at the start of the reload. (Used to position gun to receive bullets)")]
    private string m_StartReloadAnimation = "Start Reload";

    /// <summary>
    /// Speed of the reload start animation.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Speed of the reload start animation.")]
    private float m_StartReloadSpeed = 1;

    /// <summary>
    /// Sound played at the start of the reload.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played at the start of the reload.")]
    private AudioClip m_StartReloadSound;

    /// <summary>
    /// Volume of the sound played at the start of the reload.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Volume of the sound played at the start of the reload.")]
    private float m_StartReloadVolume = 0.25f;

    /// <summary>
    /// Animation of inserting a bullet in the chamber to start reloading.
    /// </summary>
    [SerializeField]
    [Tooltip("Animation of inserting a bullet in the chamber to start reloading.")]
    private string m_InsertInChamberAnimation = "Insert Chamber";

    /// <summary>
    /// Speed of the animation of inserting a bullet in the chamber.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Speed of the animation of inserting a bullet in the chamber.")]
    private float m_InsertInChamberSpeed = 1;

    /// <summary>
    /// Sound of the animation of inserting a bullet in the chamber.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound of the animation of inserting a bullet in the chamber.")]
    private AudioClip m_InsertInChamberSound;

    /// <summary>
    /// Volume of the sound of inserting a bullet in the chamber.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Volume of the sound of inserting a bullet in the chamber.")]
    private float m_InsertInChamberVolume = 0.25f;

    /// <summary>
    /// Bullet per Bullet reloading animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Bullet per Bullet reloading animation.")]
    private string m_InsertAnimation = "Insert Reload";

    /// <summary>
    /// Bullet per Bullet reloading animation speed.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Bullet per Bullet reloading animation speed.")]
    private float m_InsertSpeed = 1;

    /// <summary>
    /// Insert bullet in chamber sound.
    /// </summary>
    [SerializeField]
    [Tooltip("Insert bullet in chamber sound.")]
    private AudioClip m_InsertSound;

    /// <summary>
    /// Insert bullet in chamber volume.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Insert bullet in chamber volume.")]
    private float m_InsertVolume = 0.25f;

    /// <summary>
    /// Reload finalization animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Reload finalization animation.")]
    private string m_StopReloadAnimation = "Stop Reload";

    /// <summary>
    /// Reload finalization speed.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Reload finalization speed.")]
    private float m_StopReloadSpeed = 1;

    /// <summary>
    /// Reload finalization sound.
    /// </summary>
    [SerializeField]
    [Tooltip("Reload finalization sound.")]
    private AudioClip m_StopReloadSound;

    /// <summary>
    /// Reload finalization sound volume.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Reload finalization sound volume.")]
    private float m_StopReloadVolume = 0.25f;

    #endregion

    #endregion

    #region MELEE

    /// <summary>
    /// Enables melee animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables melee animation.")]
    private bool m_Melee;

    /// <summary>
    /// Name of the melee animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Name of the melee animation.")]
    private string m_MeleeAnimation = "Melee";

    /// <summary>
    /// Speed of melee animation.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Speed of melee animation.")]
    private float m_MeleeSpeed = 1;

    /// <summary>
    /// The time required to send the hit signal to the object the character is fighting with.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("The time required to send the hit signal to the object the character is fighting with.")]
    private float m_MeleeDelay = 0.1f;

    /// <summary>
    /// Sound of melee animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound of melee animation.")]
    private AudioClip m_MeleeSound;

    /// <summary>
    /// Volume of the sound of melee animation.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Volume of the sound of melee animation.")]
    private float m_MeleeVolume = 0.2f;

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
    private float m_HitVolume = 0.3f;

    #endregion

    #region SWITCH MODE

    /// <summary>
    /// Enables switch shooting mode animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Enables switch shooting mode animation.")]
    private bool m_SwitchMode;

    /// <summary>
    /// Name of the switch shooting mode animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Name of the switch shooting mode animation.")]
    private string m_SwitchModeAnimation = "SwitchMode";

    /// <summary>
    /// Speed of the switch shooting mode animation.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Speed of the switch shooting mode animation.")]
    private float m_SwitchModeSpeed = 1;

    /// <summary>
    /// Sound of the switch shooting mode animation.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound of the switch shooting mode animation.")]
    private AudioClip m_SwitchModeSound;

    /// <summary>
    /// Volume sound of the switch shooting mode animation.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Volume sound of the switch shooting mode animation.")]
    private float m_SwitchModeVolume = 0.2f;

    #endregion

    #region INTERACT

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
    /// Speed of the interaction animation.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("Speed of the interaction animation.")]
    private float m_InteractSpeed = 1;

    /// <summary>
    /// The time required to send the activation signal to the object the character is interacting with.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("The time required to send the activation signal to the object the character is interacting with.")]
    private float m_InteractDelay = 0.25f;

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

    #endregion

    #region VAULT

    /// <summary>
    /// Enables vault animation.
    /// </summary>
    [SerializeField]
    private bool m_Vault;

    /// <summary>
    /// The vault animation name.
    /// </summary>
    [SerializeField]
    [Tooltip("The vault animation name.")]
    private string m_VaultAnimation = "Vault";

    /// <summary>
    /// The vault animation speed.
    /// </summary>
    [SerializeField]
    [Range(0.001f, Mathf.Infinity)]
    [Tooltip("The vault animation speed.")]
    private float m_VaultSpeed = 1;

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

    #endregion

    private Camera m_Camera;
    private Transform m_TargetTransform;

    private Vector3 m_TargetPos;
    private Quaternion m_TargetRot;

    private Vector3 m_HIPPosition;
    private Vector3 m_HIPRotation;
    private float m_ReturningSpeed;

    private bool m_HasPlayedAimIn;
    private bool m_HasPlayedAimOut;

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
    /// Returns true if the weapon is in the aiming position/rotation, false otherwise.
    /// </summary>
    public bool IsAiming
    {
        get
        {
            if (!m_AimAnimation)
                return false;

            if ((m_AimingPosition - m_HIPPosition).sqrMagnitude < 0.001f && (m_AimingRotation - m_HIPRotation).sqrMagnitude < 0.001f && m_ZoomAnimation && Mathf.Abs(m_AimFOV - SettingsManager.Instance.FieldOfView) < 0.001f)
            {
                return false;
            }

            bool position = (m_AimingPosition - m_TargetTransform.localPosition).sqrMagnitude < 0.0001f;
            bool rotation = (m_AimingRotation - m_TargetTransform.localRotation.eulerAngles).sqrMagnitude < 0.001f;
            bool zoom = Mathf.Abs(m_AimFOV - m_Camera.fieldOfView) < 0.1f;

            return (position && rotation) || (zoom && Math.Abs(SettingsManager.Instance.FieldOfView - m_AimFOV) > 0.001f);
        }
    }

    /// <summary>
    /// Returns true if holding the breath is enable for this gun, false otherwise.
    /// </summary>
    public bool CanHoldBreath => m_HoldBreath;

    /// <summary>
    /// Returns true if melee is enable for this gun, false otherwise.
    /// </summary>
    public bool CanMeleeAttack => m_Melee;

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

            return m_DrawAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_DrawAnimation).length / m_DrawSpeed;
        }
    }

    /// <summary>
    /// The duration in seconds of the hiding animation.
    /// </summary>
    public float HideAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Hide)
                return 0;

            return m_HideAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_HideAnimation).length / m_HideSpeed;
        }
    }

    /// <summary>
    /// The duration in seconds of the reload animation.
    /// </summary>
    public float ReloadAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Reload)
                return 0;

            return m_ReloadAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_ReloadAnimation).length / m_ReloadSpeed;
        }
    }

    /// <summary>
    /// The duration in seconds of the full reload animation.
    /// </summary>
    public float FullReloadAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Reload)
                return 0;

            return m_ReloadEmptyAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_ReloadEmptyAnimation).length / m_ReloadEmptySpeed;
        }
    }

    /// <summary>
    /// The duration in seconds of the start reload animation.
    /// </summary>
    public float StartReloadAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Reload)
                return 0;

            return m_StartReloadAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_StartReloadAnimation).length / m_StartReloadSpeed;
        }
    }

    /// <summary>
    /// The duration in seconds of the insert in chamber animation.
    /// </summary>
    public float InsertInChamberAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Reload)
                return 0;

            return m_InsertInChamberAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_InsertInChamberAnimation).length / m_InsertInChamberSpeed;
        }
    }

    /// <summary>
    /// The duration in seconds of the inserting bullets animation.
    /// </summary>
    public float InsertAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Reload)
                return 0;

            return m_InsertAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_InsertAnimation).length / m_InsertSpeed;
        }
    }

    /// <summary>
    /// The duration in seconds of the finishing reload animation.
    /// </summary>
    public float StopReloadAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Reload)
                return 0;

            return m_StopReloadAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_StopReloadAnimation).length / m_StopReloadSpeed;
        }
    }

    /// <summary>
    /// The duration in seconds of the melee animation.
    /// </summary>
    public float MeleeAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_Melee)
                return 0;

            return m_MeleeAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_MeleeAnimation).length / m_MeleeSpeed;
        }
    }

    /// <summary>
    /// The time required to send the hit signal to the object the character is fighting with.
    /// </summary>
    public float MeleeDelay
    {
        get
        {
            if (!m_Melee)
                return 0;

            return m_MeleeDelay > MeleeAnimationLength ? MeleeAnimationLength : m_MeleeDelay;
        }
    }

    /// <summary>
    /// The duration in seconds of the switch shooting mode animation.
    /// </summary>
    public float SwitchModeAnimationLength
    {
        get
        {
            if (!m_Animator)
                return 0;

            if (!m_SwitchMode)
                return 0;

            return m_SwitchModeAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_SwitchModeAnimation).length / m_SwitchModeSpeed;
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

            return m_InteractAnimation.Length == 0 ? 0 : m_Animator.GetAnimationClip(m_InteractAnimation).length / m_InteractSpeed;
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

    #region WEAPON CUSTOMIZATION

    internal void UpdateAiming(Vector3 aimingPosition, Vector3 aimingRotation, bool zoomAnimation = false, float aimFOV = 50)
    {
        m_AimingPosition = aimingPosition;
        m_AimingRotation = aimingRotation;
        m_ZoomAnimation = zoomAnimation;
        m_AimFOV = aimFOV;
    }

    internal void UpdateFireSound(AudioClip[] fireSoundList)
    {
        m_FireSoundList.Clear();
        m_FireSoundList.AddRange(fireSoundList);
    }

    #endregion

    /// <summary>
    /// Initializes the component.
    /// </summary>
    /// <param name="targetTransform">The weapon's transform.</param>
    /// <param name="camera">The character's camera.</param>
    public void Init(Transform targetTransform, Camera camera)
    {
        m_Camera = camera;
        m_TargetTransform = targetTransform;

        if (m_SpeedParameterHash == 0)
            m_SpeedParameterHash = Animator.StringToHash(m_SpeedParameter);


        Initialized = true;
    }

    public void SetCrouchStatus(bool isCrouched)
    {
        if (isCrouched)
        {
            m_HIPPosition = m_CrouchPosition;
            m_HIPRotation = m_CrouchRotation;
            m_ReturningSpeed = m_CrouchingSpeed;
        }
        else
        {
            m_HIPPosition = m_StandingPosition;
            m_HIPRotation = m_StandingRotation;
            m_ReturningSpeed = m_RunningSpeed;
        }
    }

    /// <summary>
    /// Calculates the aiming position/rotation or return to the origin position.
    /// </summary>
    /// <param name="isAiming">Is the character aiming?</param>
    public void Aim(bool isAiming)
    {
        if (isAiming && m_AimAnimation)
        {
            if (!IsAiming && !m_HasPlayedAimIn)
            {
                m_HasPlayedAimIn = true;
                m_HasPlayedAimOut = false;
            }

            if (m_AimAnimation)
            {
                m_TargetPos = Vector3.Lerp(m_TargetPos, m_AimingPosition, Time.deltaTime * m_AimingSpeed);
                m_TargetRot = Quaternion.Slerp(m_TargetRot, Quaternion.Euler(m_AimingRotation), Time.deltaTime * m_AimingSpeed);

                if (m_ZoomAnimation)
                    m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, m_AimFOV, Time.deltaTime * m_AimingSpeed);
            }
        }
        else //Stop Sprint Animation
        {
            m_TargetPos = Vector3.Lerp(m_TargetPos, m_HIPPosition, Time.deltaTime * m_ReturningSpeed);
            m_TargetRot = Quaternion.Slerp(m_TargetRot, Quaternion.Euler(m_HIPRotation), Time.deltaTime * m_ReturningSpeed);
            m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, SettingsManager.Instance.FieldOfView, Time.deltaTime * m_AimingSpeed);
        }

        PerformAnimation();
    }

    /// <summary>
    /// Calculates the target position/rotation based on the character state or return to the origin position.
    /// </summary>
    /// <param name="isRunning">Is the character running?</param>
    /// <param name="isSliding">Is the character sliding?</param>
    public void Sprint(bool isRunning, bool isSliding)
    {
        if (m_RunAnimation && isRunning)
        {
            m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, SettingsManager.Instance.FieldOfView, Time.deltaTime * m_AimingSpeed);
            m_TargetPos = Vector3.Lerp(m_TargetPos, m_RunningPosition, Time.deltaTime * m_RunningSpeed);
            m_TargetRot = Quaternion.Slerp(m_TargetRot, Quaternion.Euler(m_RunningRotation), Time.deltaTime * m_RunningSpeed);
        }
        else if (m_SlidingAnimation && isSliding)
        {
            m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, SettingsManager.Instance.FieldOfView, Time.deltaTime * m_AimingSpeed);
            m_TargetPos = Vector3.Lerp(m_TargetPos, m_SlidingPosition, Time.deltaTime * m_SlidingSpeed);
            m_TargetRot = Quaternion.Slerp(m_TargetRot, Quaternion.Euler(m_SlidingRotation), Time.deltaTime * m_SlidingSpeed);
        }
        else //Stop Aiming Animation
        {
            m_TargetPos = Vector3.Lerp(m_TargetPos, m_HIPPosition, Time.deltaTime * m_ReturningSpeed);
            m_TargetRot = Quaternion.Slerp(m_TargetRot, Quaternion.Euler(m_HIPRotation), Time.deltaTime * m_ReturningSpeed);
            m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, SettingsManager.Instance.FieldOfView, Time.deltaTime * m_AimingSpeed);

            if (IsAiming && m_AimAnimation && !m_HasPlayedAimOut)
            {
                m_HasPlayedAimOut = true;
                m_HasPlayedAimIn = false;
            }

        }

        PerformAnimation();
    }

    /// <summary>
    /// Defines the position of the gun as the position/rotation calculated in the previous frame.
    /// </summary>
    private void PerformAnimation()
    {
        m_TargetTransform.localPosition = m_TargetPos;
        m_TargetTransform.localRotation = m_TargetRot;
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

        if (m_SpeedParameterHash == 0)
            m_SpeedParameterHash = Animator.StringToHash(m_SpeedParameter);

        m_Animator.SetFloat(m_SpeedParameterHash, m_DrawSpeed);

        m_Animator.Play(m_DrawAnimation);

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

        m_Animator.SetFloat(m_SpeedParameterHash, m_HideSpeed);

        m_Animator.CrossFadeInFixedTime(m_HideAnimation, 0.1f);

    }

    /// <summary>
    /// Plays the shooting animation.
    /// </summary>
    /// <param name="lastRound">Is the last round in the magazine?</param>
    internal void Shot(bool lastRound)
    {
        if (!m_Animator)
            return;

        if (!m_Fire)
            return;

        if (m_OverrideLastFire && lastRound)
        {
            m_Animator.SetFloat(m_SpeedParameterHash, m_FireSpeed);

            if (m_LastFireAnimation.Length > 0)
                m_Animator.CrossFadeInFixedTime(m_LastFireAnimation, 0.1f);
        }
        else
        {
            string currentAnim;

            if (IsAiming)
            {
                if (m_AimedFireAnimationList.Count > 0)
                {
                    m_Animator.SetFloat(m_SpeedParameterHash, m_AimedFireSpeed);

                    switch (m_FireAnimationType)
                    {
                        case AnimationType.Sequential:
                            {
                                if (m_LastIndex == m_AimedFireAnimationList.Count || m_LastIndex > m_AimedFireAnimationList.Count)
                                    m_LastIndex = 0;

                                currentAnim = m_AimedFireAnimationList[m_LastIndex++];

                                if (currentAnim.Length > 0)
                                    m_Animator.CrossFadeInFixedTime(currentAnim, 0.1f);

                                break;
                            }
                        case AnimationType.Random:

                            currentAnim = m_AimedFireAnimationList[Random.Range(0, m_AimedFireAnimationList.Count)];

                            if (currentAnim.Length > 0)
                                m_Animator.CrossFadeInFixedTime(currentAnim, 0.1f);

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                if (m_FireAnimationList.Count > 0)
                {
                    m_Animator.SetFloat(m_SpeedParameterHash, m_FireSpeed);

                    switch (m_FireAnimationType)
                    {
                        case AnimationType.Sequential:
                            {
                                if (m_LastIndex == m_FireAnimationList.Count || m_LastIndex > m_FireAnimationList.Count)
                                    m_LastIndex = 0;

                                currentAnim = m_FireAnimationList[m_LastIndex++];

                                if (currentAnim.Length > 0)
                                    m_Animator.CrossFadeInFixedTime(currentAnim, 0.1f);

                                break;
                            }
                        case AnimationType.Random:

                            currentAnim = m_FireAnimationList[Random.Range(0, m_FireAnimationList.Count)];

                            if (currentAnim.Length > 0)
                                m_Animator.CrossFadeInFixedTime(currentAnim, 0.1f);

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        if (m_FireSoundList.Count > 0)
        {
            if (m_FireSoundList.Count > 1)
            {
                int i = Random.Range(1, m_FireSoundList.Count);
                AudioClip a = m_FireSoundList[i];

                m_FireSoundList[i] = m_FireSoundList[0];
                m_FireSoundList[0] = a;

                //if (m_AdditiveSound)
                //    AudioManager.Instance.PlayClipAtPoint(a, m_Camera.transform.position, 10, 25, m_FireVolume, 0);
                //else
                //    m_PlayerWeaponSource.ForcePlay(a, m_FireVolume);

            }
            else
            {
                //if (m_AdditiveSound)
                //    AudioManager.Instance.PlayClipAtPoint(m_FireSoundList[0], m_Camera.transform.position, 10, 25, m_FireVolume, 0);
                //else
                //    m_PlayerWeaponSource.ForcePlay(m_FireSoundList[0], m_FireVolume);
            }
        }
    }

    /// <summary>
    /// Plays the dry fire sound.
    /// </summary>
    internal void OutOfAmmo()
    {
        //if (m_OutOfAmmoSound)
        //    m_PlayerBodySource.ForcePlay(m_OutOfAmmoSound, m_FireVolume);
    }

    /// <summary>
    /// Plays the reload magazine animation.
    /// </summary>
    /// <param name="roundInChamber">Is there a bullet in the chamber?</param>
    internal void Reload(bool roundInChamber)
    {
        if (!m_Animator)
            return;

        if (!m_Reload)
            return;

        if (!roundInChamber)
        {
            if (m_ReloadEmptyAnimation.Length == 0)
                return;

            m_Animator.SetFloat(m_SpeedParameterHash, m_ReloadEmptySpeed);

            m_Animator.CrossFadeInFixedTime(m_ReloadEmptyAnimation, 0.1f);

            //if (m_ReloadEmptySound)
            //    m_PlayerWeaponGenericSource.ForcePlay(m_ReloadEmptySound, m_ReloadVolume);
        }
        else
        {
            if (m_ReloadAnimation.Length == 0)
                return;

            m_Animator.SetFloat(m_SpeedParameterHash, m_ReloadSpeed);

            m_Animator.CrossFadeInFixedTime(m_ReloadAnimation, 0.1f);

            //if (m_ReloadSound)
            //    m_PlayerWeaponGenericSource.ForcePlay(m_ReloadSound, m_ReloadVolume);
        }
    }

    /// <summary>
    /// Plays the start reload animation.
    /// </summary>
    /// <param name="roundInChamber">Is there a bullet in the chamber?</param>
    internal void StartReload(bool roundInChamber)
    {
        if (!m_Animator)
            return;

        if (!m_Reload)
            return;

        if (!roundInChamber)
        {
            if (m_InsertInChamberAnimation.Length == 0)
                return;

            m_Animator.SetFloat(m_SpeedParameterHash, m_InsertInChamberSpeed);

            m_Animator.CrossFadeInFixedTime(m_InsertInChamberAnimation, 0.1f);
            //m_PlayerWeaponSource.Stop();

            //if (m_InsertInChamberSound)
            //    m_PlayerWeaponGenericSource.ForcePlay(m_InsertInChamberSound, m_InsertInChamberVolume);
        }
        else
        {
            if (m_StartReloadAnimation.Length == 0)
                return;

            m_Animator.SetFloat(m_SpeedParameterHash, m_StartReloadSpeed);

            m_Animator.CrossFadeInFixedTime(m_StartReloadAnimation, 0.1f);
            //m_PlayerWeaponSource.Stop();

            //if (m_StartReloadSound)
            //    m_PlayerWeaponGenericSource.ForcePlay(m_StartReloadSound, m_StartReloadVolume);
        }
    }

    /// <summary>
    /// Plays the insert bullet in chamber animation.
    /// </summary>
    internal void Insert()
    {
        if (!m_Animator)
            return;

        if (!m_Reload)
            return;

        if (m_InsertAnimation.Length == 0)
            return;

        m_Animator.SetFloat(m_SpeedParameterHash, m_InsertSpeed);

        m_Animator.CrossFadeInFixedTime(m_InsertAnimation, 0.1f);

        //if (m_InsertSound)
        //    m_PlayerWeaponGenericSource.ForcePlay(m_InsertSound, m_InsertVolume);
    }

    /// <summary>
    /// Finalizes the reload animation.
    /// </summary>
    internal void StopReload()
    {
        if (!m_Animator)
            return;

        if (!m_Reload)
            return;

        if (m_StopReloadAnimation.Length == 0)
            return;

        m_Animator.SetFloat(m_SpeedParameterHash, m_StopReloadSpeed);

        m_Animator.CrossFadeInFixedTime(m_StopReloadAnimation, 0.1f);

        //if (m_StopReloadSound)
        //    m_PlayerWeaponGenericSource.ForcePlay(m_StopReloadSound, m_StopReloadVolume);
    }

    /// <summary>
    /// Plays a melee attack animation.
    /// </summary>
    internal void Melee()
    {
        if (!m_Animator)
            return;

        if (!m_Melee)
            return;

        if (m_MeleeAnimation.Length == 0)
            return;

        m_Animator.SetFloat(m_SpeedParameterHash, m_MeleeSpeed);

        m_Animator.CrossFadeInFixedTime(m_MeleeAnimation, 0.1f);

        //if (m_MeleeSound)
        //    m_PlayerBodySource.ForcePlay(m_MeleeSound, m_MeleeVolume);
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
    /// Plays the switch fire mode animation.
    /// </summary>
    internal void SwitchMode()
    {
        if (!m_Animator)
            return;

        if (!m_SwitchMode)
            return;

        if (m_SwitchModeAnimation.Length == 0)
            return;

        m_Animator.SetFloat(m_SpeedParameterHash, m_SwitchModeSpeed);

        m_Animator.CrossFadeInFixedTime(m_SwitchModeAnimation, 0.1f);

        //if (m_SwitchModeSound)
        //    m_PlayerBodySource.ForcePlay(m_SwitchModeSound, m_SwitchModeVolume);
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

        m_Animator.SetFloat(m_SpeedParameterHash, m_InteractSpeed);

        m_Animator.CrossFadeInFixedTime(m_InteractAnimation, 0.1f);

        //if (m_InteractSound)
        //    m_PlayerBodySource.ForcePlay(m_InteractSound, m_InteractVolume);
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

        m_Animator.SetFloat(m_SpeedParameterHash, m_VaultSpeed);

        m_Animator.CrossFadeInFixedTime(m_VaultAnimation, 0.1f);

        //if (m_VaultSound)
        //    m_PlayerBodySource.ForcePlay(m_VaultSound, m_VaultVolume);
    }
}

public interface IWeaponAnimator
{
    void Draw();

    void Hide();

    void Hit(Vector3 position);

    void Interact();

    void Vault();
}
