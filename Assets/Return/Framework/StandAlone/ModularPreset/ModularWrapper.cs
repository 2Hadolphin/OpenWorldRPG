using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TNet;
using UnityEngine;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
#endif

namespace Return
{
    

    public class ModularWrapper : PresetDatabase, IModularClass //, ISerializationCallbackReceiver, IDataNodeSerializable
    {
        [PropertyOrder(9)]
        [SerializeField]
        TextAsset m_DataNodeAsset;

        [NonSerialized]
        [IgnoredByTNet]
        DataNode Node;

        void SaveData()
        {
            if (Module.IsNull())
                return;

            if (Node.NotNull())
                Node.Clear();
            else
                Node = new DataNode();

            SavePersistentData(Node);
        }

        void SavePersistentData(DataNode node)
        {
            TextAsset textAsset;

            // test write node
            {
                node.value = Module;
                
                //node.ReflectionSerialize(Module, "BaseModularPreset");
                //var child=node.GetChild(Module.GetDeclaredType().Name, true);

                var stream = new MemoryStream();

                var writer = new StreamWriter(stream);
                node.Write(writer);

                var array = stream.ToArray();
                var text = Encoding.ASCII.GetString(array);

                writer.Close();
                stream.Close();

                textAsset = new TextAsset(text) { name= "DataNodeAsset" };
            }

            {
                //node.ReflectionSerialize(Module, "BaseModularPreset");

                //var buffer = node.WriteBuffer(out var str);

                //textAsset = new TextAsset(str)
                //{
                //    name = "DataNodeAsset"
                //};
            }


#if UNITY_EDITOR

            if (m_DataNodeAsset.NotNull())
            {
                AssetDatabase.RemoveObjectFromAsset(m_DataNodeAsset);
                UnityEngine.Object.DestroyImmediate(m_DataNodeAsset, true);

                //File.WriteAllText(AssetDatabase.GetAssetPath(m_DataNodeAsset), str);
            }

            textAsset = Editors.EditorAssetsUtility.AddSubAsset(this, textAsset);
            textAsset.hideFlags = hideSubAsset ? HideFlags.HideInHierarchy : HideFlags.None;

            EditorUtility.SetDirty(this);
#endif
            m_DataNodeAsset = textAsset;
        }

        void LoadNode()
        {
            if (m_DataNodeAsset.IsNull())
                return;

            Node = DataNode.Read(m_DataNodeAsset.bytes);

            Debug.Log("Node type : " + Node.type);
            Debug.Log("Node data : " + Node.ToString());

            //foreach (var node in Node.GetEnumerable())
            //    Debug.Log(string.Format("Name : {0} IValue : {1} Type {2}", node.name, node.value, node.type));

            if (!Node.ResolveValue())
                Debug.LogError("Failure resolve");

            //foreach (var node in Node.GetEnumerable())
            //    Debug.LogError(string.Format("Name : {0} IValue : {1} Type {2}", node.name, node.value, node.type));
        }

        void LoadDataToModule()
        {
            LoadNode();

            Module = Node.value;

            return;
            var target = Node.FindChild("Type");

            if (target.NotNull())
            {
                Module=target.CastClass();
                //Module = target.Get(Serialization.NameToType((string)target.value));
                
                Debug.Log(Serialization.NameToType((string)target.value));
                Node.CastFields(Module);
            }
            else
            {
                Debug.Log(Node.children.Count);
                Debug.LogError(Node.type + " : " + Node.value);
                Module = Node.value;
            }
        }



        // unfinish
        public object LoadModule()
        {
            object module;

            if (Node.IsNull())
            {
                LoadNode();
                module = Node.IsNull() ? null : Node.value;
            }
            else
            {
                if (Node.value is ICloneable cloneable)
                    module = cloneable.Clone();
                else
                    module = Node.IsNull() ? null : Node.value;
            }

            Debug.LogError(module);

            return module;

            //if (Module == null)
            //{
            //    var module = Node.FindChild(nameof(Module));
            //    //Debug.Log(module.value+" : "+module.type);
            //    //Module=Serialization.ConvertObject(module, module.type);

            //    if (!module.ResolveValue())
            //        Debug.LogError("ResolveFailure " + module.value + " " + module.type);

            //    Module = module.value;
            //}

            //return Module;
        }

        #region Serialize


        [Obsolete]
        protected override void OnBeforeSerialize()
        {
            //Debug.Log(nameof(OnBeforeSerialize));
            return;

#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsDirty(this))
            {
               
                UnityEditor.EditorUtility.ClearDirty(this);
            }
#endif
            // update DataNode to textAsset

            //Debug.Log(nameof(OnBeforeSerialize));

            return;

            if (Module.IsNull())
                return;

       

            Node = Node.ReflectionSerialize(Module,nameof(Module));
        }

        [Obsolete]
        protected override void OnAfterDeserialize()
        {
            //Debug.Log(nameof(OnAfterDeserialize));
            return;
        }

        [Obsolete]
        public void Serialize(DataNode node)
        {
            node.Merge(Node);

            return;

            //this.ReflectionSerialize()

            var c = Module;

            if (c.IsNull())
                return;

            //var type = c.GetType();

            //var cacheType = type.GetCache();

            //var fields = cacheType.GetSerializableFields();
            //var props = cacheType.GetSerializableProperties();
            var name = nameof(Module);//this.GetUniqueID().ToString();
            var newNode=node.ReflectionSerialize(c, name);

            //var data = node.FindChild(nameof(Module));

            //if (data.IsNull())
            //{
            //    node.AddChild(nameof(Module), Module);
            //}
            //else
            //{
            //    node.SetChild(nameof(Module), Module);
            //}

            Debug.Log(newNode);
        }
        [Obsolete]
        public void Deserialize(DataNode node)
        {
            Node = new();
            Node.Merge(node);
        }

        #endregion

#if UNITY_EDITOR


        [HideInInspector]
        [IgnoredByTNet]
        [SerializeField]
        private object m_Module;

        [PropertySpace(10,10)]
        [HideLabel]
        [OnValueChanged(nameof(UpdateNode),IncludeChildren =true)]
        [PropertyOrder(10)]
        [BoxGroup("Data", ShowLabel =false)]
        [ShowInInspector]
        public object Module { get => m_Module; set => m_Module = value; }

        void CleanModule()
        {
            Module = null;
        }

        void UpdateNode()
        {
            SaveData();
        }

        [Title("Module")]
        [PropertyOrder(8)]
        [ToggleLeft]
        [OnValueChanged(nameof(ChangeAssetVisiable))]
        [SerializeField]
        bool hideSubAsset=true;

        void ChangeAssetVisiable()
        {
            if (m_DataNodeAsset.IsNull())
                return;

            m_DataNodeAsset.hideFlags= hideSubAsset ? HideFlags.HideInHierarchy : HideFlags.None;
            //EditorUtility.SetDirty(m_DataNodeAsset);
            //EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Load object from script asset.
        /// </summary>
        [Title("Class Binding")]
        [PropertyOrder(3)]
        [OnValueChanged(nameof(LoadScript))]
        [IgnoredByTNet]
        [SerializeField]
        [Required("Require editor script asset binding.",InfoMessageType.Error)]
        internal UnityEditor.MonoScript scriptAsset;

        internal void LoadScript()
        {
            if (scriptAsset.IsNull())
                return;

            Debug.Log(nameof(LoadScript));
            Module = Activator.CreateInstance(scriptAsset.GetClass());
        }

        [PropertyOrder(4)]
        [OnInspectorGUI]
        void DrawInspector()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(nameof(LoadScript)))
                LoadScript();

            if (GUILayout.Button(nameof(CleanModule)))
                CleanModule();

            GUILayout.EndHorizontal();
        }

        [PropertyOrder(9)]
        [HorizontalGroup("Conftol")]
        [Button(nameof(LoadDataToModule), ButtonSizes.Large, Style = ButtonStyle.Box)]
        void Load()
        {
            LoadDataToModule();
        }

        [PropertyOrder(9.5f)]
        [HorizontalGroup("Conftol")]
        [Button(nameof(SaveData),ButtonSizes.Large,Style =ButtonStyle.Box)]
        void Save()
        {
            SaveData();
        }


        //[OnInspectorGUI]
        //void DrawModuleInspector()
        //{
        //    if (GUILayout.Button(nameof(SaveData)))
        //        SaveData();

        //    if (GUILayout.Button(nameof(LoadDataToModule)))
        //        LoadDataToModule();
        //}

        SerializedObject serializedObject;
        SerializedProperty sp_Module;

        [HideLabel]
        [OnInspectorGUI]
        void DrawModule()
        {
            return;

            if (m_Module.IsNull())
            {
                GUILayout.Label(new GUIContent(scriptAsset?scriptAsset.name:null+" is null"));
                return;
            }


            if (serializedObject == null)
                serializedObject = new SerializedObject(this);

            //Debug.LogError(serializedObject.targetObject);
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                var prop = serializedObject.FindProperty(nameof(m_Module));

                EditorGUILayout.LabelField("Module", GUILayout.Width(120));
                EditorGUILayout.PropertyField(prop, new GUIContent(""));

                GUILayout.EndHorizontal();
            }
            //serializedObject.DrawProperty(nameof(m_Module),"Module");

            if (sp_Module == null)
                sp_Module = serializedObject.FindProperty("m_Module"); //nameof(m_Module));

            if (sp_Module == null)
                return;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(sp_Module, true);

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }


        private void OnEnable()
        {
            //Debug.Log(nameof(OnEnable) + this);

            // get DataNode from textAsset

            if (Module.IsNull())
            {
                if (Node.IsNull())
                {
                    if (m_DataNodeAsset.NotNull())
                    {
                        LoadDataToModule();
                        return;
                    }
                }


                LoadScript();
            }
        }

        private void OnDisable()
        {
            //Debug.Log(nameof(OnDisable) + this);
        }

        private void OnValidate()
        {
            //Debug.Log(nameof(OnValidate) + this);

        }

#endif

    }

#if UNITY_EDITOR

    //[CustomEditor(typeof(ModularWrapper))]
    sealed class ModularWrapperEditor : OdinEditor
    {
        SerializedProperty scriptAsset;
        protected override void OnEnable()
        {
            Debug.Log("Enable wrapper");
            scriptAsset = serializedObject.FindProperty(nameof(ModularWrapper.scriptAsset));
        }

        protected override void OnDisable()
        {
            Debug.Log("Disable wrapper");
        }

        public override void OnInspectorGUI()
        {
            if (target is not ModularWrapper wrapper)
                return;

            

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            var prop = EditorGUILayout.PropertyField(scriptAsset, true);

            // on change
            if(EditorGUI.EndChangeCheck())
            {

                {
                    LoadScript(wrapper.scriptAsset);
                }            
            }

        }

        [Button]
        void LoadScript(MonoScript scriptAsset)
        {
            if (scriptAsset.IsNull())
                return;

            var wrapper = target as ModularWrapper;

            var type = scriptAsset.GetClass();

            if (type.IsSubclassOf(typeof(Component)))
            {
                Debug.LogError("WrongType " + type);
                return;
                //wrapper.Module = wrapper.gameObject.AddComponent(type);
            }
            else if (type.IsSubclassOf(typeof(ScriptableObject)))
            {
                Debug.LogError("WrongType " + type);
                return;
                wrapper.Module = ScriptableObject.CreateInstance(type);
            }
            else
            {
                wrapper.Module = Activator.CreateInstance(type);
            }



            var dataPath = Editors.EditorUtitily.GetEditorFolder(scriptAsset) + "/" + scriptAsset.name + "_preset.bytes";

            // archive via handler


        }
    }

    internal static class EditorExtension
    {
        [MenuItem("Assets/CreateWrapperAsset",true)]
        static bool ValidWrapperAsset()
        {
            var ob = Selection.activeObject;



            if (ob.IsNull())
                return false;


            if (ob is not MonoScript mono)
                return false;

            var type = mono.GetClass();


            if (type == null)
            {
                Debug.LogError(mono.name + " get type null");
                return false;
            }

            if (!type.IsClass || type.IsAbstract)
                return false;

            return !type.IsSubclassOf(typeof(UnityEngine.Object));
        }

        [MenuItem("Assets/CreateWrapperAsset",priority = 1000)]
        static void Create()
        {
            if (Selection.activeObject is MonoScript mono)
                Create(mono);
        }

        public static void Create(MonoScript mono)
        {
            var type = mono.GetClass();

            try
            {
                var path = Editors.EditorUtitily.GetEditorFolder(mono);

                var wrapper=
                    Editors.EditorAssetsUtility.CreateInstanceSO(typeof(ModularWrapper), false, path, mono.name) as ModularWrapper;

                wrapper.scriptAsset = mono;
                wrapper.LoadScript();

                EditorGUIUtility.PingObject(wrapper);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("{0} is not a custome class object script. {1}", mono.name, e));
            }
        }

    }

#endif
}