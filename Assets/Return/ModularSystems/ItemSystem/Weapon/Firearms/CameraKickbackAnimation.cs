using System;
using UnityEngine;


[Serializable]
public struct CameraKickbackAnimation
{
    [SerializeField]
    private bool m_Enabled;

    [SerializeField]
    private float m_Kickback;

    [SerializeField]
    private float m_MaxKickback;

    [SerializeField]
    [m_MinMaxSliderAttribute(-1, 1)]
    private Vector2 m_KickbackRandomness;

    [SerializeField]
    private float m_HorizontalKickback;

    [SerializeField]
    [m_MinMaxSliderAttribute(-1, 1)]
    private Vector2 m_HorizontalKickbackRandomness;

    [SerializeField]
    [Range(0, 1)]
    private float m_KickbackRotation;

    [SerializeField]
    private float m_KickbackDuration;

    [SerializeField]
    private float m_KickbackSpeed;

    public CameraKickbackAnimation(bool enabled, float kickback, float maxKickback, Vector2 kickbackRandomness, float horizontalKickback, Vector2 horizontalKickbackRandomness, float kickbackRotation, float kickbackDuration, float kickbackSpeed)
    {
        m_Enabled = enabled;
        m_Kickback = kickback;
        m_MaxKickback = maxKickback;
        m_KickbackDuration = kickbackDuration;
        m_HorizontalKickback = horizontalKickback;
        m_HorizontalKickbackRandomness = horizontalKickbackRandomness;
        m_KickbackRandomness = kickbackRandomness;
        m_KickbackSpeed = kickbackSpeed;
        m_KickbackRotation = kickbackRotation;
    }

    #region PROPERTIES

    public bool Enabled => m_Enabled;

    public float Kickback => m_Kickback;

    public float MaxKickback => m_MaxKickback;

    public Vector2 HorizontalKickbackRandomness => m_HorizontalKickbackRandomness;

    public float KickbackDuration => m_KickbackDuration;

    public float HorizontalKickback => m_HorizontalKickback;

    public Vector2 KickbackRandomness => m_KickbackRandomness;

    public float KickbackRotation => m_KickbackRotation;

    public float KickbackSpeed => m_KickbackSpeed;

    #endregion
}