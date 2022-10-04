using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Agents;
using System.Linq;
using System;
using Return.Modular;

namespace Return.Humanoid.Character
{
    /// <summary>
    /// Preset wrapper refer to preset which contains **mseh **materials **slot binding **prefab
    /// </summary>
    public class CharacterDataWrapper : ModuleHandler//HumanoidModularSystem
    {
        public CharacterPreset Preset;

        #region Resolver

        public override void InstallResolver(IModularResolver resolver)
        {

        }


        #endregion


        public bool SearchSlotBinding(Func<BodySlotBinding,bool> condition, out BodySlotBinding binding)
        {
            binding=Preset.BodySlotBindings.FirstOrDefault(x => condition(x));
            return binding;
        }

        [NonSerialized]
        public List<SkinnedMeshRenderer> CharacterRenderers=new();

    }
}