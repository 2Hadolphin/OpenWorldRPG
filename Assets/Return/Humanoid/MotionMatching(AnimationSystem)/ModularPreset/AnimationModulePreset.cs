using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Humanoid.Modular;
using MxM;

namespace Return.Humanoid.Animation
{
    public class AnimationModulePreset : HumanoidModularPreset
    {
        public override void LoadModule(GameObject @object)
        {
            var mxm = @object.InstanceIfNull<MxMAnimator>();

            mxm.AnimData =new[] { MxMAnimData };
            mxm.OverrideCalibration = CalibrationModule;
            mxm.RootMotion = EMxMRootMotion.RootMotionApplicator;
            mxm.SetWarpOverride(WarpModule);
            mxm.UpdateRate = 60f;
            

            //var trajector= @object.InstanceIfNull<MxMTrajectoryGenerator>();

            //trajector.RelativeCameraTransform = Camera.main.transform;

            //trajector.ControlMode = ETrajectoryControlMode.UserInput;
            //trajector.TrajectoryMode = ETrajectoryMoveMode.Normal;

            //trajector.PositionBias = 29f;
            //trajector.DirectionBias = 25f;
        }

        public MxMAnimData MxMAnimData;
        public CalibrationModule CalibrationModule;
        public WarpModule WarpModule;
    }
}