

using UnityEngine;


/// <summary>
/// The Motion Preset Asset is a data container with all rules used to perform a procedural movement animation.
/// </summary>
[CreateAssetMenu(menuName = "Motion Preset", fileName = "Motion Preset", order = 201)]
public sealed class MotionData : ScriptableObject
{
    [SerializeField]
    private bool m_AnimatePosition = true;

    /// <summary>
    /// Determines how fast the animation will be played.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("Determines how fast the animation will be played.")]
    private float m_PositionSpeed = 1;

    /// <summary>
    /// Determines how far the transform can move horizontally.
    /// </summary>
    [SerializeField]
    [Tooltip("Determines how far the transform can move horizontally.")]
    private float m_HorizontalPositionAmplitude = 0.001f;

    /// <summary>
    /// Determines how far the transform can move vertically.
    /// </summary>
    [SerializeField]
    [Tooltip("Determines how far the transform can move vertically.")]
    private float m_VerticalPositionAmplitude = 0.001f;

    /// <summary>
    /// Defines how the vertical movement will behave.
    /// </summary>
    [SerializeField]
    [Tooltip("Defines how the vertical movement will behave.")]
    private AnimationCurve m_VerticalPositionAnimationCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

    /// <summary>
    /// Determines how notably the transform can be rotated on Z-axis.
    /// </summary>
    [SerializeField]
    [Tooltip("Determines how notably the transform can be rotated on Z-axis.")]
    private float m_DistalAmplitude;

    [SerializeField]
    private bool m_AnimateRotation = true;

    /// <summary>
    /// Determines how fast the animation will be played.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("Determines how fast the animation will be played.")]
    private float m_RotationSpeed = 1;

    /// <summary>
    /// Determines how far the transform can move horizontally.
    /// </summary>
    [SerializeField]
    [Tooltip("Determines how far the transform can move horizontally.")]
    private float m_HorizontalRotationAmplitude = 1;

    /// <summary>
    /// Determines how far the transform can move vertically.
    /// </summary>
    [SerializeField]
    [Tooltip("Determines how far the transform can move vertically.")]
    private float m_VerticalRotationAmplitude = 1;

    /// <summary>
    /// Defines how the vertical movement will behave.
    /// </summary>
    [SerializeField]
    [Tooltip("Defines how the vertical movement will behave.")]
    private AnimationCurve m_VerticalRotationAnimationCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

    /// <summary>
    /// Determines how notably the transform can be rotated on Z-axis.
    /// </summary>
    [SerializeField]
    [Tooltip("Determines how notably the transform can be rotated on Z-axis.")]
    private float m_TiltAmplitude;

    /// <summary>
    /// Determines how distinctly the animation will be vertically affected by the direction of the character's movement.
    /// </summary>
    [SerializeField]
    [Range(-2, 2)]
    [Tooltip("Determines how distinctly the animation will be vertically affected by the direction of the character's movement.")]
    private float m_VelocityInfluence = 1;

    /// <summary>
    /// Determines a position offset for the animation, moving it to a more appropriate position.
    /// </summary>
    [SerializeField]
    [Tooltip("Determines a position offset for the animation, moving it to a more appropriate position.")]
    private Vector3 m_PositionOffset;

    /// <summary>
    /// Determines a rotation offset for the animation, rotating it to a more appropriate angle.
    /// </summary>
    [SerializeField]
    [Tooltip("Determines a rotation offset for the animation, rotating it to a more appropriate angle.")]
    private Vector3 m_RotationOffset;

    #region PROPERTIES

    public bool AnimatePosition => m_AnimatePosition;

    /// <summary>
    /// Determines how fast the animation will be played. (Read Only)
    /// </summary>
    public float PositionSpeed => m_PositionSpeed;

    public float HorizontalPositionAmplitude => m_HorizontalPositionAmplitude;

    public float VerticalPositionAmplitude => m_VerticalPositionAmplitude;

    public AnimationCurve VerticalPositionAnimationCurve => m_VerticalPositionAnimationCurve;

    public float DistalAmplitude => m_DistalAmplitude;

    public bool AnimateRotation => m_AnimateRotation;

    public float RotationSpeed => m_RotationSpeed;

    public float HorizontalRotationAmplitude => m_HorizontalRotationAmplitude;

    public float VerticalRotationAmplitude => m_VerticalRotationAmplitude;

    public AnimationCurve VerticalRotationAnimationCurve => m_VerticalRotationAnimationCurve;

    public float TiltAmplitude => m_TiltAmplitude;

    /// <summary>
    /// Determines how much the animation will be vertically affected by the direction of the character's movement. (Read Only)
    /// </summary>
    public float VelocityInfluence => m_VelocityInfluence;

    /// <summary>
    /// Determines a position offset for the animation, moving it to a more appropriate position. (Read Only)
    /// </summary>
    public Vector3 PositionOffset => m_PositionOffset;

    /// <summary>
    /// Determines a rotation offset for the animation, rotating it to a more appropriate angle. (Read Only)
    /// </summary>
    public Vector3 RotationOffset => m_RotationOffset;

    #endregion
}

