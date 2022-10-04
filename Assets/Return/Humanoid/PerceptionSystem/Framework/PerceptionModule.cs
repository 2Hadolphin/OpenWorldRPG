using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Modular;

namespace Return.Perception
{
    /// <summary>
    /// MonoModule to perceive environment. **User camera   **AI sensor
    /// </summary>
    public abstract class PerceptionModule : ModuleHandler
    {
        public override bool DisableAtRegister => false;
        public override ControlMode CycleOption => ControlMode.Register;

        public override void InstallResolver(IModularResolver resolver)
        {
            resolver.RegisterModule<IPerceptionModule>(this);
        }
    }
}
