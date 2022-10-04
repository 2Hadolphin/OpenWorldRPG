using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Agents;
using System;

namespace Return.Items
{

    /// <summary>
    /// Basic class to drive modules of item (not controlable module).
    /// </summary>
    [DefaultExecutionOrder(500)]
    public abstract class ConfigurableItemModule : MonoItemModule,IConfigurableItemModule 
    {
        #region Preset
        public virtual void LoadModuleData(ConfigurableItemModulePreset preset)
        {
            Preset = preset;
        }

        protected virtual ConfigurableItemModulePreset Preset { get; set; }

        #endregion




    }
}