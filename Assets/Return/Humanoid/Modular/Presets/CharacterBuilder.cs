using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Humanoid.Character;
using System;
using Sirenix.OdinInspector;
using Return.Humanoid.Modular;
using Return.Agents;

namespace Return.Humanoid
{
    public abstract class ReturnObjectBuilder : PresetDatabase
    {
        [Button("Build Object")]
        public abstract GameObject BuildObject(PR pr,Transform transform=null);
    }
    public class CharacterBuilder : ReturnObjectBuilder
    {
        public override GameObject BuildObject(PR pr,Transform transform=null)
        {
            //var cha=CharacterPreset.BuildCharacter(pr,transform);
            var character = CharacterPreset.CreateCharacter();

            AgentPreset.LoadModule(character);

            foreach (var module in CharacterModules)
            {
                if (module is BaseModularPreset preset)
                    preset.LoadModule(character);
                else if (module is IModularClass modular)
                {
                    modular.LoadModule();
                }
            }

            return character;
        }

        public CharacterPreset CharacterPreset;
        public AgentModularPreset AgentPreset;

#if UNITY_EDITOR
        void AddPreset(UnityEngine.Object obj)
        {
            if (obj is BaseModularPreset)
                CharacterModules.Add(obj);
            else if (obj is IModularClass)
                CharacterModules.Add(obj);
            else
                Debug.LogError("Error preset type");
        }

        Type[] filiter()
        {
            return new Type[] { typeof(BaseModularPreset), typeof(ModularWrapper) };
        }

        [TypeFilter(nameof(filiter))]
        [ListDrawerSettings(Expanded =true, NumberOfItemsPerPage = 10, DraggableItems = true,CustomAddFunction =nameof(AddPreset))]
#endif
        public HashSet<UnityEngine.Object> CharacterModules=new();


    }
}