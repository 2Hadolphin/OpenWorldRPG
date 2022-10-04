using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using TNet;

namespace Return
{
    public class ModularConfigs : BaseModularPreset
    {

#if UNITY_EDITOR
        [Space]
        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/Config")]
        [ShowInInspector]
        [OnValueChanged(nameof(LoadOptions))]
        UnityEngine.Object Target;

        [PropertySpace(7,7)]
        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/Config")]
        [Button]
        void Clean()
        {
            Target = null;
        }

        System.Collections.Generic.List<Component> options;

        string[] options_str; 

        void LoadOptions()
        {
            if (Target is GameObject go)
                options = go.GetComponents<Component>().SkipWhile((x)=>x is Transform).ToList();

            var length = options.Count;
            options_str = new string[length + 2];
            options_str[0] = "Select module..";
            options_str[1] = "All";

            for (int i = 0; i < length; i++)
            {
                options_str[i+2] = options[i].GetType().ToString();
            }
        }

        [PropertySpace(3, 7)]
        [BoxGroup("Editor")]
        [OnInspectorGUI]
        void DrawOptions()
        {
            if (options.IsNull())
                return;

            var index = UnityEditor.EditorGUILayout.Popup(0, options_str);

            if (index == 0)
                return;
            else if (index == 1)
                Modules.AddRange(options);
            else
                Modules.Add(options[Mathf.Max(index - 2,0)]);
        }

#endif

        [SerializeField]
        [ShowInInspector]
        protected System.Collections.Generic.List<UnityEngine.Object> Modules;

        public override void LoadModule(GameObject @object)
        {
            LoadModules(@object);
        }

        public virtual object[] LoadModules(GameObject obj)
        {
            Debug.Log(Modules.Count());

            return Modules.Select(x =>
            {
                try
                {
                    if (x.NotNull())
                    {
                        if (x is IModularClass modular)
                            return modular.LoadModule();
                        else if(x is Component c)
                            return obj.AddComponent(c);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                Debug.LogError("Missing type to instance : " + x);
                return null;
            }).Where(x => x.NotNull()).ToArray();
        }
    }
}