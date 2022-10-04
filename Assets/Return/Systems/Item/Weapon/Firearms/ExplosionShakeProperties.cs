using UnityEngine;


/// <summary>
/// Stores the characteristics of the camera shake animation in reaction to an explosion near to the character.
/// </summary>
[System.Serializable]
public struct ExplosionShakeProperties
{
    /// <summary>
    /// Determines the initial direction of the shake caused by the explosion.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("Determines the initial direction of the shake caused by the explosion.")]
    private float m_Angle;

    /// <summary>
    /// How far the shake can move the camera.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("How far the shake can move the camera.")]
    private float m_Strength;

    /// <summary>
    /// The minimum speed of how fast the camera can move during the shake.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("The minimum speed of how fast the camera can move during the shake.")]
    private float m_MinSpeed;

    /// <summary>
    /// The maximum speed of how fast the camera can move during the shake.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("The maximum speed of how fast the camera can move during the shake.")]
    private float m_MaxSpeed;

    /// <summary>
    /// How long the explosion 'shake' lasts.
    /// </summary>
    [SerializeField]
    [Range(0, Mathf.Infinity)]
    [Tooltip("How long the explosion 'shake' lasts.")]
    private float m_Duration;

    /// <summary>
    /// A higher value will result in a more randomized direction for the shake animation.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("A higher value will result in a more randomized direction for the shake animation.")]
    private float m_NoisePercent;

    /// <summary>
    /// Sets how fast the shake intensity will decrease.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Sets how fast the shake intensity will decrease.")]
    private float m_DampingPercent;

    /// <summary>
    /// The magnitude at which camera rotation is affected during the shake animation.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The magnitude at which camera rotation is affected during the shake animation.")]
    private float m_RotationPercent;

    #region PROPERTIES

    /// <summary>
    /// The initial direction of the shake caused by the explosion.
    /// </summary>
    public float Angle => m_Angle;

    /// <summary>
    /// How far the shake can move the camera.
    /// </summary>
    public float Strength => m_Strength;

    /// <summary>
    /// The minimum speed of how fast the camera can move during the shake.
    /// </summary>
    public float MinSpeed => m_MinSpeed;

    /// <summary>
    /// The maximum speed of how fast the camera can move during the shake.
    /// </summary>
    public float MaxSpeed => m_MaxSpeed;

    /// <summary>
    /// How long the explosion 'shake' lasts.
    /// </summary>
    public float Duration => m_Duration;

    /// <summary>
    /// How randomized is the direction of the shake animation.
    /// </summary>
    public float NoisePercent => m_NoisePercent;

    /// <summary>
    /// Sets how fast the shake intensity will decrease.
    /// </summary>
    public float DampingPercent => m_DampingPercent;

    /// <summary>
    /// The magnitude at which camera rotation is affected during the shake animation.
    /// </summary>
    public float RotationPercent => m_RotationPercent;

    #endregion

    public ExplosionShakeProperties(float angle, float strength, float minSpeed, float maxSpeed, float duration, float noisePercent, float dampingPercent, float rotationPercent)
    {
        m_Angle = angle;
        m_Strength = strength;
        m_MaxSpeed = minSpeed;
        m_MinSpeed = maxSpeed;
        m_Duration = duration;
        m_NoisePercent = Mathf.Clamp01(noisePercent);
        m_DampingPercent = Mathf.Clamp01(dampingPercent);
        m_RotationPercent = Mathf.Clamp01(rotationPercent);
    }
}
