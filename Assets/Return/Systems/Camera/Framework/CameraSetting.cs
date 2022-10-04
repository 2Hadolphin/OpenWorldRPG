using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Return;

public abstract class BaseCameraSetting : PresetDatabase 
{
    [SerializeField]
    LayerMask m_ViewMask;
    public LayerMask ViewMask { get => m_ViewMask; set => m_ViewMask = value; }



}



[Serializable]
public class CameraSetting : BaseCameraSetting
{
    public const float k_MaxConstraintsMatchMult = 20f;
    public const float k_MinConstraintsMatchMult = 1f;


    #region Parameter
    public float Sensitivity_H = 6;
    public float Sensitivity_V = 6;

    public float SmoothLerp_Position = 0.125f;
    public float SmoothLerp_Rotation = 0.125f;

    public float Fov = 75;

    #endregion

    #region Offset
    public Vector3 Offset_pos;
    public Quaternion Offset_rot = Quaternion.identity;
    #endregion

    public float SwitchDuringTime = 0.25f;


    #region Constraints

    public CamMatchData Constraint = new() 
    {
        Rotation=new CameraConstraint()
        {
            Clamp_Max = new(180, 85, 25),
            Clamp_Min= new(-180, -85, -25)
        },
    };

    [ShowInInspector, BoxGroup("Setting")]
    public Vector3 Clamp_P_Min = new(-180, -85, -25);
    [ShowInInspector, BoxGroup("Setting")]
    public Vector3 Clamp_P_Max = new(180, 85, 25);

    [SerializeField, Range(0f, 1f), Tooltip("The amount of damping applied when rotating the camera to match constraints.")]
    public float m_ConstraintsDamping = 0.5f;

    [SerializeField, Tooltip("Once the angle outside constraints goes below this value, the camera will snap to the constraints. Larger values will have a visible effect.")]
    public float m_ConstraintsTolerance = 0.25f;

    [SerializeField, Tooltip("An angle range from the yaw constraint limits where the Input falls off. This gives the effect of softer constraint limits instead of hitting an invisible wall.")]
    public float m_YawConstraintsFalloff = 10f;
    #endregion

    #region Config

    [Title("Mouse Smoothing")]

    [SerializeField, Tooltip("The number of frames to store and use for the mouse smoothing history")]
    public int m_MouseSmoothingBufferSize = 10;

    [SerializeField, Tooltip("The weight multiplier for the previous frame when averaging if the smoothing is set to minimum")]
    public float m_MouseSmoothingMultiplierMin = 0.05f;

    [SerializeField, Tooltip("The weight multiplier for the previous frame when averaging if the smoothing is set to maximum")]
    public float m_MouseSmoothingMultiplierMax = 0.5f;

    [Title("Mouse Acceleration")]

    [SerializeField, Tooltip("The base acceleration multiplier when acceleration is set to the minimum.")]
    public float m_MouseAccelSpeedMultiplyMin = 0.001f;

    [SerializeField, Tooltip("The base acceleration multiplier when acceleration is set to the maximum.")]
    public float m_MouseAccelSpeedMultiplyMax = 0.01f;

    [SerializeField, Tooltip("The maximum multiplier acceleration can apply to the mouse Input (0 means no maximum)")]
    public float m_MouseAccelerationMax = 0f;


    [Title("Mouse Turn")]
    [SerializeField, Tooltip("Number of degrees for 1 unit of mouse movement if sensitivity is set to 0")]
    public float m_MouseTurnAngleMin = 0.25f;

    [SerializeField, Tooltip("Number of degrees for 1 unit of mouse movement if sensitivity is set to 1")]
    public float m_MouseTurnAngleMax = 5f;

    [Title("Gamepad Turn")]

    [SerializeField, Tooltip("Number of degrees per second for the gamepad analog at its limit, if sensitivity is set to 0")]
    public float m_AnalogTurnAngleMin = 45f;

    [SerializeField, Tooltip("Number of degrees per second for the gamepad analog at its limit, if sensitivity is set to 1")]
    public float m_AnalogTurnAngleMax = 90f;

    [SerializeField, Tooltip("The Input curve for analog Input. This can be used to define a deadzone, and damp smaller movements")]
    public AnimationCurve m_AnalogCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.75f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f) });
    #endregion


    [SerializeField]
    public Token Priority;

}
