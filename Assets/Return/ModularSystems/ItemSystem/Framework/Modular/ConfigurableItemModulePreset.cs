using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using Return.Humanoid.Motion;
using Return.Agents;


namespace Return.Items
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class ConfigurableItemModulePreset : PresetDatabase
    {


        /// <summary>
        /// Instantiate modules according to agent requirements(or attach as extra handle while activate). **Effect all **Remote(tno.isMine) **Control by player or AI
        /// </summary>
        public abstract IConfigurableItemModule  LoadModule(GameObject @object,IAgent agent=null);

        /// <summary>
        /// Behaviour of instance module which can be overwrite by child class.
        /// </summary>
        protected virtual T InstanceModule<T>(GameObject @object) where T: ConfigurableItemModule
        {
            var module = @object.InstanceIfNull<T>();
            module.LoadModuleData(this);
            return module;
        }
    }

}