using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Return
{
    /// <summary>
    /// Base scriptable unit to build mono modules.
    /// </summary>
    public abstract class BaseModularPreset : PresetDatabase
    {
        /// <summary>
        /// This function will been called on second time thread(after agent), used to loading module
        /// </summary>
        [Button("Build Modular Preset")]
        [PropertyOrder(200)]
        public abstract void LoadModule(GameObject go);
    }
}