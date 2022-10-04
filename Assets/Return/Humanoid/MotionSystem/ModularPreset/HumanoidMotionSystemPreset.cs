using UnityEngine;
using Return.Humanoid;
using Return.Humanoid.Motion;
using System;

namespace Return.Motions
{
    /// <summary>
    /// Create humanoid motion system.
    /// </summary>
    //[Obsolete]
    public class HumanoidMotionSystemPreset : MotionSystemPreset
    {
        public override void LoadModule(GameObject character)
        {
            //var controller = @character.InstanceIfNull<m_HumanoidController>();

            //var capsule = character.InstanceIfNull<CapsuleCollider>();

            //capsule.radius = 0.25f;

            // Solve character motion
            //var characterMotor = character.InstanceIfNull<KinematicCharacterMotor>();

            // order anim(MxM) => reveive root motion => odrer movement(KCC)=>_checkCache state & collision
            var system = character.InstanceIfNull<MotionSystem_Humanoid>();

            system.Preset = this;
        }
    }
}