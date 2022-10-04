using System.Collections.Generic;
using UnityEngine;
using Return;
using Return.Database;

[CreateAssetMenu(fileName = "CameraMotionSettings", menuName = "Return/Camera/Motion Settings")]
public class bl_CameraMotionSettings : PresetDatabase
{
    [Header("Wiggle")]
    public float smooth = 4f;
    public float tiltAngle = 6f;
    public float sideMoveEffector = 3;
    public float aimMultiplier = 0.5f;

    [Header("Breathe")]
    public float breatheAmplitude = 0.5f;
    public float breathePeriod = 1f;

    [Header("FallEffect")]
    [Range(0.01f, 1.0f)]
    public float downMotionDuration = 1f;
    public float DownAmount = 8;
    public AnimationCurve downPitchMovement = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 1), new Keyframe(0.5f, 0), new Keyframe(1, 1) } };
}