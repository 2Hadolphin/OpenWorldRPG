using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine.Assertions;
using TNet;

namespace Return
{
    /// <summary>
    /// Container to archive modules as DataNode. **MonoScriptAsset **Component
    /// </summary>
    public class ModularConfig : BaseModularPreset
    {
        public override void LoadModule(GameObject @object)
        {
            LoadModules(@object);
        }

        public virtual object[] LoadModules(GameObject obj)
        {
            Func<GameObject> getGameObj = () =>
            {
                // Create null
                if (obj.IsNull())
                    return new GameObject(this.name);
                else
                    return obj;
            };
   

            Debug.Log(Modules.Count());

            return Modules.Select(x =>
            {
                if (x.NotNull())
                {
                    if (x.Instance(out var module, getGameObj))
                        return module;

                    if (x.IsSubclassOf(typeof(Component)))
                    {
                        return getGameObj().InstanceIfNull(x);
                    }
                    else if (!x.IsSubclassOf(typeof(UnityEngine.Object)))
                        return Activator.CreateInstance(x);
                }

                Debug.LogError("Missing type to instance : " + x);
                return null;
            }).Where(x => x.NotNull()).ToArray();
        }

#if UNITY_EDITOR
        [Title("Editor Only", TitleAlignment = TitleAlignments.Centered)]
        [PropertySpace]

        [OnValueChanged(nameof(LoadNewType))]
        public UnityEngine.Object preLoadObj;

        System.Collections.Generic.List<Type> types;

        void LoadNewType()
        {
            if (preLoadObj is GameObject go)
                types = go.GetComponents<Component>().Select(x => x.GetType()).ToList();
            else if (preLoadObj is Component component)
                types = new() { component.GetType() };
            else if(preLoadObj is UnityEditor.MonoScript script)
            {
                SaveType(script.GetClass());
                preLoadObj = null;
            }
        }

        [OnInspectorGUI]
        void DrawSelector()
        {
            if (types.IsNull() || types.Count == 0)
                return;

            

            var index=UnityEditor.EditorGUILayout.Popup(0, types.Select(x => x.ToString()).ToArray());

            if (index == 0)
                return;

            SaveType(types[index]);
        }

        void AddNewType()
        {
            Component component = null;

            if (preLoadObj)
            {

                if (preLoadObj is UnityEditor.MonoScript mono)
                {
                    var type = mono.GetClass();
                    
                    try
                    {
                        Assert.IsNotNull(type);

                        Assert.IsTrue(type.IsSubclassOf(typeof(Component)) || !type.IsSubclassOf(typeof(UnityEngine.Object)));

                        SaveType(type);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(string.Format("{0} is not a valid script for instance.", mono.name));
                    }

                }
                else if (preLoadObj is GameObject go)
                {
                    var components = go.GetComponents<Component>();

                    foreach (var obj in components)
                    {
                        if (obj is Transform)
                            continue;

                        component = obj;
                        break;
                    }
                }
                else
                {
                    component = preLoadObj as Component;
                }

                if (component.IsNull())
                {
                    Debug.LogError(preLoadObj + " is not a component.");
                    preLoadObj = null;
                    return;
                }

                {
                    //TNet.Serialization.AddAllFields(Nodes,component);
                    
                    //Nodes.SetChild(nameof(component),component);
                }

                try
                {
                    var newPresetType = new UnityEditor.Presets.PresetType(component);

                    foreach (var preset in Presets)
                    {
                        if (preset.GetPresetType().Equals(newPresetType))
                            return;
                    }

                    //if (!Presets.Contains(preLoadObj))
                    {
                        //UnityEditor.Presets.Preset preset = new(component);
                        //var newPreset = Return.Editors.EditorUtitily.AddSubAsset(this, preset, component.GetType().ToString());
                        //Presets.Add(newPreset);


                    }
                }
                finally
                {
                    preLoadObj = null;
                    SaveType(component.GetType());
                }
            }
        }

        void SaveType(Type type)
        {
            var typeId = Serialization.TypeToName(type);

            if (m_Modules.IsNull())
                m_Modules = new() { typeId };
            else
                m_Modules.CheckAdd(typeId);



            Dirty();
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(this));
            UnityEditor.AssetDatabase.Refresh();
        }


        [ListDrawerSettings(CustomRemoveElementFunction = nameof(RemovePreset))]
        public System.Collections.Generic.List<UnityEditor.Presets.Preset> Presets = new();

        void RemovePreset(UnityEditor.Presets.Preset preset)
        {
            if (!preset)
                return;

            Presets.Remove(preset);
            UnityEditor.AssetDatabase.RemoveObjectFromAsset(preset);
            DestroyImmediate(preset);
            Dirty();
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(this));
            UnityEditor.AssetDatabase.Refresh();
        }

        [Button]
        void LoadPresets()
        {
            if (Presets == null)
                Presets = new();

            var presets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this));
            Presets.AddRange(presets.Where(x => x is UnityEditor.Presets.Preset).Select(x => x as UnityEditor.Presets.Preset));
        }


        [PropertyOrder(99)]
        public bool ShowBuildPreset;


        [ShowIf(nameof(ShowBuildPreset))]
        [PropertyOrder(100)]
        //[ShowInInspector]
        [Title("ModuleConfig")]
        [PropertySpace(10, 10)]
        [ListDrawerSettings(Expanded = true)]
#else
 
#endif
        [SerializeField]
        public System.Collections.Generic.List<string> m_Modules;

        public IEnumerable<Type> Modules => m_Modules.Select(x =>
        {
            //var typeId=Type.GetType(x);

            //if (typeId == null)
              var  type = Serialization.NameToType(x);

            if(type==null)
                Debug.LogError("Can't not found typeId of "+x);

            return type;

        });




        //[HideInInspector]
        //public List<Type> Modules
        //{
        //    get
        //    {
        //        if (m_Modules.IsNull())
        //        {
        //            Debug.LogError(m_Modules);
        //            m_Modules = new();
        //        }

        //        Debug.Log(m_Modules.Count);
        //        return m_Modules;
        //    }
        //}

        //        protected override void OnBeforeSerialize()
        //        {
        //            foreach (var module in Modules)
        //            {
        //                try
        //                {
        //                    Assert.IsNotNull(module);
        //                    Assert.IsNotNull(module.GetType());
        //                }
        //                catch (Exception e)
        //                {
        //#if UNITY_EDITOR
        //                    Debug.LogError(UnityEditor.AssetDatabase.GetAssetPath(this)+" ** "+e);
        //                    UnityEditor.EditorGUIUtility.PingObject(this);
        //                    continue;
        //#endif
        //                }

        //            }
        //        }

    }
}