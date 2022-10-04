using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Cameras;

namespace Return.Items.Weapons
{
    public class MarkerPreset : FirearmsModulePreset<MarkerModule>
    {
        [Tooltip("Default aimming item")]
        [SerializeField]
        Sight m_Sight;
        public Sight Sight => m_Sight;

        [Title("Defalut Iron sightTransform")]

        [TabGroup("IdleState")]
        [SerializeField]
        public CameraConfig Idle;


        [TabGroup("Aim")]
        [SerializeField]
        public CameraConfig Aim;


        [TitleGroup("Animation Config", Alignment = TitleAlignments.Centered)]

        [BoxGroup("Animation Config/Aiming")]
        public TimelinePreset Aiming;

        [SerializeField]
        public FirearmsEvent Events;


        #region LocalPlayer
        [Title("Camera Config")]
        public PositionBobData PositionBobData;
        public RotationBobData RotationBobData;

        public AimAmplifierData SwayData;
        #endregion
    }
}
