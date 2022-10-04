using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using Return.Humanoid;
using Return.Audios;
using Return.Preference;
using Return;

public class CameraAnimator : MonoBehaviour
{
    /// <summary>
    /// Defines the reference to the First Person Character Controller script.
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("Defines the reference to the First Person Character Controller script.")]
    protected mFPScontroller controller;

    /// <summary>
    /// Defines the reference to the Health Controller script.
    /// </summary>
    [SerializeField]
    [Required]
    [Tooltip("Defines the reference to the Health Controller script.")]
    protected HealthController m_HealthController;

    /// <summary>
    /// Defines the how the camera breath animation should be played.
    /// </summary>
    [SerializeField]
    [Tooltip("Defines the how the camera breath animation should be played.")]
    protected BreathAnimationMode m_BreathAnimationMode = BreathAnimationMode.EnableWhenAim;

    /// <summary>
    /// The MotionAnimation component.
    /// </summary>
    [SerializeField]
    protected MotionAnimation m_MotionAnimation = new MotionAnimation();

    /// <summary>
    /// Sound played when the character hold his breath to be more steady while aiming.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played when the character hold his breath to be more steady while aiming.")]
    protected AudioClip m_HoldBreath;

    /// <summary>
    /// Sound played when the character exhale after a long period holding his breath.
    /// </summary>
    [SerializeField]
    [Tooltip("Sound played when the character exhale after a long period holding his breath.")]
    protected AudioClip m_Exhale;

    /// <summary>
    /// Defines the volume of Hold Breath Sound and Exhale Sound.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Defines the volume of Hold Breath Sound and Exhale Sound.")]
    protected float m_HoldBreathVolume = 0.3f;

    private InputActionMap m_InputBindings;
    private InputAction m_SteadyAimAction;
    private InputAction m_LeaningAction;

    private float m_NextHoldBreathTime;
    private float m_HoldBreathDuration;

    private IEnumerator m_CurrentShakeCoroutine;

    #region PROPERTIES

    /// <summary>
    /// Defines if the camera can hold your breath to stabilize your vision while aiming.
    /// </summary>
    public bool HoldBreath
    {
        protected get;
        set;
    }

    /// <summary>
    /// Returns the current character leaning direction (left or right). Interval [-1, 1]
    /// </summary>
    public int LeanDirection
    {
        get;
        private set;
    }

    #endregion

    protected virtual void Start()
    {
        // Events callback
        /*
        m_FPController.PreJumpEvent += CameraBraceForJump;
        m_FPController.JumpEvent += CameraJump;
        m_FPController.LandingEvent += CameraLanding;
        m_FPController.VaultEvent += Vault;
        m_FPController.GettingUpEvent += Vault;
        */
        m_HealthController.ExplosionEvent += GrenadeExplosion;
        m_HealthController.HitEvent += Hit;

        // Inputs Bindings
        //m_InputBindings = SettingsManager.Instance.GetActionMap("Movement");
        //m_InputBindings.Enable();

        //m_SteadyAimAction = SettingsManager.Instance.GetActionMap("Weapons").FindAction("Steady Aim");
        //m_LeaningAction = m_InputBindings.FindAction("Leaning");

        // AudioSources
        //m_PlayerGenericSource = AudioManager.Instance.RegisterSource("[AudioEmitter] Generic", transform.root);
    }

    protected virtual void Update()
    {
        // Movement animation
        m_MotionAnimation.MovementAnimation(controller);
        m_MotionAnimation.StabiliseCameraRecoil();

        // Breath animation
        m_MotionAnimation.BreathingAnimation(((m_BreathAnimationMode == BreathAnimationMode.EnableWhenAim && controller.IsAiming)
            || m_BreathAnimationMode == BreathAnimationMode.Unrestricted || m_BreathAnimationMode == BreathAnimationMode.DisableWhenAim && !controller.IsAiming)
            && Math.Abs(m_HoldBreathDuration) < Mathf.Epsilon ? 1 : 0);

        if (HoldBreath)
        {
            // Hold breath
            if (m_SteadyAimAction.ReadValue<float>() > 0 && m_NextHoldBreathTime < Time.time && controller.IsAiming && !controller.TremorTrauma)
            {
                if (Math.Abs(m_HoldBreathDuration) < Mathf.Epsilon)
                    //m_PlayerGenericSource.Play(m_HoldBreath, m_HoldBreathVolume);

                m_HoldBreathDuration += Time.deltaTime;
                if (m_HoldBreathDuration > m_HoldBreath.length)
                {
                    m_NextHoldBreathTime = Time.time + 3 + m_HoldBreathDuration;
                    m_HoldBreathDuration = 0;
                    //m_PlayerGenericSource.Play(m_Exhale, m_HoldBreathVolume);
                }
            }
            //Release the breath
            else
            {
                if (m_HoldBreathDuration > 0)
                    //m_PlayerGenericSource.Stop();

                if (m_HoldBreathDuration > m_HoldBreath.length * 0.7f)
                {
                    m_NextHoldBreathTime = Time.time + 3 + m_HoldBreathDuration;
                    //m_PlayerGenericSource.Play(m_Exhale, m_HoldBreathVolume);
                }

                m_HoldBreathDuration = 0;
            }
        }

        // Leaning animations
        if (!m_MotionAnimation.Lean)
            return;

        if (controller.State != ObsoleteMotionState.Flying && controller.State != ObsoleteMotionState.Running && controller.State != ObsoleteMotionState.Climbing)
        {
            // Lean by holding the button
            if (SettingsManager.Instance.GamePlay.LeanStyle == ActionMode.Hold)
            {
                if (m_LeaningAction.ReadValue<float>() < 0)
                {
                    LeanDirection = CanLean(Vector3.left) ? -1 : 0;
                }
                else if (m_LeaningAction.ReadValue<float>() > 0)
                {
                    LeanDirection = CanLean(Vector3.right) ? 1 : 0;
                }
                else
                {
                    LeanDirection = 0;
                }

                m_MotionAnimation.LeanAnimation(LeanDirection);
            }
            else
            {
                // Lean by tapping the button
                if (m_LeaningAction.triggered)
                {
                    if (m_LeaningAction.ReadValue<float>() < 0 && LeanDirection != -1)
                    {
                        LeanDirection = LeanDirection == 1 ? 0 : -1;
                    }
                    else if (m_LeaningAction.ReadValue<float>() < 0 && LeanDirection == -1)
                    {
                        LeanDirection = 0;
                    }

                    if (m_LeaningAction.ReadValue<float>() > 0 && LeanDirection != 1)
                    {
                        LeanDirection = LeanDirection == -1 ? 0 : 1;
                    }
                    else if (m_LeaningAction.ReadValue<float>() > 0 && LeanDirection == 1)
                    {
                        LeanDirection = 0;
                    }
                }

                if (LeanDirection == -1 && !CanLean(Vector3.left) || LeanDirection == 1 && !CanLean(Vector3.right))
                    LeanDirection = 0;

                m_MotionAnimation.LeanAnimation(LeanDirection);
            }
        }
        else
        {
            LeanDirection = 0;
            m_MotionAnimation.LeanAnimation(0);
        }
    }

    /// <summary>
    /// Casts a ray to evaluate if the character can lean to a determinate direction.
    /// </summary>
    /// <param name="direction">The desired direction.</param>
    private bool CanLean(Vector3 direction)
    {
        Ray ray = new Ray(controller.transform.position, controller.transform.TransformDirection(direction));
        return !Physics.SphereCast(ray, m_MotionAnimation.LeanAmount, out _, m_MotionAnimation.LeanAmount * 2, Physics.AllLayers, QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// Event method that simulates the effect of preparing to jump on the motion animation component.
    /// </summary>
    protected virtual void CameraBraceForJump()
    {
        StartCoroutine(m_MotionAnimation.BraceForJumpAnimation.Play());
    }

    /// <summary>
    /// Event method that simulates the effect of jump on the motion animation component.
    /// </summary>
    protected virtual void CameraJump()
    {
        StartCoroutine(m_MotionAnimation.JumpAnimation.Play());
    }

    /// <summary>
    /// Event method that simulates the effect of landing on the motion animation component.
    /// </summary>
    protected virtual void CameraLanding(float fallDamage)
    {
        StartCoroutine(m_MotionAnimation.LandingAnimation.Play());
    }

    /// <summary>
    /// Updates the camera recoil settings and play a new recoil animation.
    /// </summary>
    public virtual void ApplyRecoil(CameraKickbackAnimation cameraKickbackAnimation)
    {
        // Update camera recoil properties
        m_MotionAnimation.CameraRecoilAnimation(cameraKickbackAnimation);
    }

    /// <summary>
    /// Event method that simulates the effect of explosion on the motion animation component.
    /// </summary>
    protected virtual void GrenadeExplosion()
    {
        if (m_CurrentShakeCoroutine != null)
        {
            StopCoroutine(m_CurrentShakeCoroutine);
        }

        m_CurrentShakeCoroutine = m_MotionAnimation.Shake(m_MotionAnimation.ExplosionShake);
        StartCoroutine(m_CurrentShakeCoroutine);
    }

    /// <summary>
    /// Event method that simulates the effect of character hit by a projectile on the motion animation component.
    /// </summary>
    protected virtual void Hit()
    {
        StartCoroutine(m_MotionAnimation.HitAnimation());
    }

    /// <summary>
    /// Event method that simulates the effect of vaulting on the motion animation component.
    /// </summary>
    protected virtual void Vault()
    {
        StartCoroutine(m_MotionAnimation.VaultAnimation.Play());
    }
}

