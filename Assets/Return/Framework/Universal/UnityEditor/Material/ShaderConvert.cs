using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using System.Linq;
using Object = UnityEngine.Object;

namespace TwoHaStudio.EditorTools
{
    public class ShaderConvert : OdinEditorWindow//ScriptableWizard
    {

        [MenuItem("Tools/Shader Swapper")]
        static void CreateWizard()
        {
            var window = EditorWindow.CreateWindow<ShaderConvert>();

        }

        #region Convert Config
        [Space]
        [BoxGroup("Convert Setting", ShowLabel = false)]
        [Tooltip("Renew = creat new instance. \nOverwrite = change material shader.")]
        public Mode ConvertMode;


        [HorizontalGroup("Convert Setting/Config", PaddingRight = 15, PaddingLeft = 15)]
        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        [SerializeField]
        [HideLabel]
        [TitleGroup("Convert Setting/Config/Source", Alignment = TitleAlignments.Centered)]
        [OnValueChanged(nameof(LoadConfig), IncludeChildren = true)]
        public ShaderSelector Source = new ShaderSelector();

        [HorizontalGroup("Convert Setting/Config", PaddingLeft = 15, PaddingRight = 15)]
        [PropertyOrder(10)]
        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        [SerializeField]
        [HideLabel]
        [TitleGroup("Convert Setting/Config/Target", Alignment = TitleAlignments.Centered)]
        [OnValueChanged(nameof(LoadConfig), IncludeChildren = true)]
        public ShaderSelector Target = new ShaderSelector();

        [Serializable]
        public class ShaderSelector
        {
            [BoxGroup]
            [LabelWidth(100)]
            [PropertySpace(SpaceBefore = 15)]
            [OnValueChanged(nameof(LoadShaders))]
            public UnityEngine.Object obj;

            [BoxGroup]
            [LabelWidth(100)]
            [PropertySpace(SpaceAfter = 15)]
            [ValueDropdown(nameof(Shaders), DropdownTitle = "Select Source Shader")]
            public Shader Shader;

            [HideInInspector]
            public Shader[] Shaders;
            public void LoadShaders()
            {
                if (!obj)
                {
                    Shader = null;
                    Shaders = null;
                    return;
                }

                var shaders = new HashSet<Shader>();
                var type = obj.GetType();




                if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                {
                    var child = EditorUtility.DisplayDialog("Inculde ChildFolder", "Search child folder ?", "Yes.", "No.");
                    shaders = new HashSet<Shader>(EditorHelper.GetAssetsAtPath<Shader>(AssetDatabase.GetAssetPath(obj), child));
                }
                if (type == (typeof(GameObject)))
                {
                    var ob = obj as GameObject;
                    var renderers = ob.GetComponentsInChildren<Renderer>();
                    foreach (var render in renderers)
                    {
                        var mats = render.sharedMaterials;
                        foreach (var mat in mats)
                        {
                            shaders.Add(mat.shader);
                        }
                    }
                }
                else if (type.IsSubclassOf(typeof(Renderer)))
                {
                    var render = obj as Renderer;
                    var mats = render.sharedMaterials;
                    foreach (var mat in mats)
                    {
                        shaders.Add(mat.shader);
                    }
                }
                else if (type == (typeof(Material)))
                {
                    shaders.Add((obj as Material).shader);
                }
                else if (type == (typeof(Shader)))
                {
                    shaders.Add(obj as Shader);
                }

                Shaders = shaders.ToArray();
            }

        }


        [ShowIf(nameof(VaildSetting))]
        [SerializeField]
        [LabelText("Shader Parameter Setup")]
        [BoxGroup("Convert Setting")]
        [Searchable]
        public List<Pair> Pairs = new List<Pair>();

        #region Func

        [PropertyTooltip("Reload all shader properties.")]
        [PropertySpace(SpaceBefore = 10)]
        [BoxGroup("Convert Setting")]
        [VerticalGroup("Convert Setting/Config/Options")]
        [TitleGroup("Convert Setting/Config/Options/Operate", Alignment = TitleAlignments.Centered)]
        [Button("Resolve Shader", ButtonHeight = 21)]
        public void LoadConfig()
        {
            Valid();

            if (!VaildSetting)
                return;

            var sourceShader = Source.Shader;
            if (sourceShader)
            {
                var sourceCount = Source.Shader.GetPropertyCount();
                Pairs = new List<Pair>(sourceCount);

                for (int i = 0; i < sourceCount; i++)
                {
                    var des = sourceShader.GetPropertyDescription(i);
                    var name = sourceShader.GetPropertyName(i);
                    var type = sourceShader.GetPropertyType(i);
                    Pairs.Add(new Pair() { Source = new Parameter { LockKey = true, Type = type, _Type = type.ToString(), Key = name, Description = des } });
                }


                var shader = Target.Shader;

                var targetCount = shader.GetPropertyCount();
                var targets = new string[targetCount + 1];
                targets[0] = Deprecated;
                for (int i = 0; i < targetCount; i++)
                {
                    var name = shader.GetPropertyName(i);
                    var type = shader.GetPropertyType(i);
                    var des = shader.GetPropertyDescription(i);


                    targets[i + 1] = name + "$" + des + "$" + type.ToString();

                    for (int k = 0; k < sourceCount; k++)
                    {
                        var p = Pairs[k].Source;
                        var t = new Parameter() { Keys = targets };
                        Pairs[k].Target = t;

                        if (p.Type.Equals(type))
                        {
                            if (p.Key == name)
                            {
                                t.Key = name;
                                t.Description = des;
                            }
                        }
                    }
                }
            }
        }



        #endregion

        #endregion



        #region Script Check
        void Valid()
        {
            VaildSetting = Source.Shader ? Target.Shader : false;
        }
        bool VaildSetting = false;

        #endregion






        #region Target Config


        [Searchable]
        [BoxGroup("Target Config")]
        [LabelText("Convert Queue")]
        public HashSet<Material> Mats = new HashSet<Material>();

        [HorizontalGroup("Target Config/Control")]
        [ShowInInspector]
        [Button("Scan Materials")]
        public void LoadMats(params UnityEngine.Object[] targets)
        {
            if (targets == null) return;
            var shader = Source.Shader;

            foreach (var obj in targets)
            {
                if (obj == null)
                    continue;

                var type = obj.GetType();
                if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                {
                    var child = EditorUtility.DisplayDialog("Inculde ChildFolder", "Search child folder ?", "Yes.", "No.");
                    LoadMats(EditorHelper.GetAssetsAtPath<Material>(AssetDatabase.GetAssetPath(obj), child));
                }
                else if (type == (typeof(GameObject)))
                {
                    var ob = obj as GameObject;
                    var renderers = ob.GetComponentsInChildren<Renderer>();
                    LoadMats(renderers);
                }
                else if (type.IsSubclassOf(typeof(Renderer)))
                {
                    var render = obj as Renderer;
                    var mats = render.sharedMaterials;
                    LoadMats(mats);
                }
                else if (type == (typeof(Material)))
                {
                    var mat = obj as Material;
                    if (mat.shader == shader)
                        Mats.Add(mat);
                }
            }
        }


        #endregion


        #region Execute
        [BoxGroup("Target Config")] 
        [Button("Convert")]
        public void ConvertMaterial()
        {
            var shader = Target.Shader;

            foreach (var mat in Mats)
            {
                if (mat == null)
                    continue;
                var textures = new List<TextureSetting>();
                var colors = new List<ColorSetting>();
                var values = new List<ValuesSetting>();

                foreach (var p in Pairs)
                {
                    if (string.IsNullOrEmpty(p.Target.Key) || p.Target.Key.Equals(Deprecated))
                        continue;

                    var older = p.Source.Key;
                    var newer = p.Target.Key;

                    switch (p.Source.Type)
                    {
                        case ShaderPropertyType.Color:
                            colors.Add(new ColorSetting(mat, older, newer));
                            break;
                        case ShaderPropertyType.Vector:

                            break;
                        case ShaderPropertyType.Float:
                            values.Add(new ValuesSetting(mat, older, newer));
                            break;
                        case ShaderPropertyType.Range:
                            values.Add(new ValuesSetting(mat, older, newer));
                            break;
                        case ShaderPropertyType.Texture:
                            textures.Add(new TextureSetting(mat, older, newer));
                            break;
                    }
                }


                switch (ConvertMode)
                {
                    case Mode.Renew:
                        var newMat = new Material(shader);

                        foreach (var t in textures)
                            t.Set(newMat);

                        foreach (var c in colors)
                            c.Set(newMat);

                        foreach (var v in values)
                            v.Set(newMat);

                        var path = AssetDatabase.GetAssetPath(mat);
                        path = AssetDatabase.GenerateUniqueAssetPath(path);
                        AssetDatabase.CreateAsset(newMat, path);

                        break;
                    case Mode.Overwrite:
                        mat.shader = shader;

                        foreach (var t in textures)
                            t.Set(mat);

                        foreach (var c in colors)
                            c.Set(mat);

                        foreach (var v in values)
                            v.Set(mat);

                        EditorUtility.SetDirty(mat);
                        break;
                }


            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        #endregion

        #region Define
        public enum Mode { Renew, Overwrite }
        const string Deprecated = "**Deprecated**";

        [Serializable]
        public class Pair
        {
            [SerializeField]
            [HorizontalGroup("Pair")]
            public Parameter Source;
            [SerializeField]
            [HorizontalGroup("Pair")]
            public Parameter Target;
        }

        [Serializable]
        [HideLabel]
        public class Parameter : ISerializationCallbackReceiver
        {
            [HorizontalGroup("Parameter")]
            [HideLabel]
            [LabelWidth(45)]
            [ReadOnly]
            [NonSerialized]
            [ShowInInspector]
            public string _Type;

            [HideInInspector]
            public ShaderPropertyType Type;

            [HorizontalGroup("Parameter")]
            [ReadOnly]
            [HideLabel]
            public string Description;

            [DisableIf(nameof(LockKey))]
            [HorizontalGroup("Parameter")]
            [HideLabel]
            [ValueDropdown(nameof(GetKeys), ExcludeExistingValuesInList = true, FlattenTreeView = true)]
            [ShowInInspector]
            public string Key
            {
                get => mKey;
                set
                {
                    if (string.IsNullOrEmpty(value))
                        return;
                    else if (value.Equals(Deprecated))
                    {
                        mKey = Deprecated;
                        Description = Deprecated;
                        _Type = Deprecated;
                        return;
                    }

                    if (LockKey)
                    {
                        mKey = value;
                        return;
                    }


                    value = Keys.Where(x => x.Split('$')[0].Equals(value)).First();

                    var ps = value.Split('$', '$');

                    if (ps.Length > 0 && !string.IsNullOrEmpty(ps[0]))
                    {
                        mKey = ps[0];
                    }

                    if (ps.Length > 1 && !string.IsNullOrEmpty(ps[1]))
                    {
                        Description = ps[1];
                    }

                    if (ps.Length > 2 && !string.IsNullOrEmpty(ps[2]))
                    {
                        if (Enum.TryParse(ps[2], out Type))
                            _Type = ps[2];
                    }
                }
            }
            [HideInInspector]
            public string mKey;



            [HideInInspector]
            public string[] Keys;
            [HideInInspector]
            public bool LockKey;
            string[] GetKeys
            {
                get
                {
                    if (LockKey)
                        return null;

                    return Keys.Select(x => x.Split('$')[0]).ToArray();
                }
            }

            public void OnBeforeSerialize()
            {

            }

            public void OnAfterDeserialize()
            {
                _Type = Type.ToString();
            }
        }


        abstract class matSetting
        {
            public string Target;
            public abstract void Set(Material mat);
        }
        class TextureSetting : matSetting
        {
            public Texture Texture;
            public Vector2 Offset;
            public Vector2 Scale;
            public TextureSetting(Material mat, string name, string target)
            {
                Texture = mat.GetTexture(name);
                Offset = mat.GetTextureOffset(name);
                Scale = mat.GetTextureScale(name);
                Target = target;
            }

            public override void Set(Material mat)
            {
                //var id=Shader.PropertyToID(Target);

                var index = mat.shader.FindPropertyIndex(Target);
                var id = mat.shader.GetPropertyNameId(index);
                mat.SetTexture(id, Texture);
                mat.SetTextureOffset(id, Offset);
                mat.SetTextureScale(id, Scale);
                //Debug.Log(Target + "_" + index + "_" + id + "_" + Texture + mat.GetTexture(id));

            }
        }
        class ColorSetting : matSetting
        {
            public Color Color;
            public ColorSetting(Material mat, string name, string target)
            {
                this.Color = mat.GetColor(name);
                Target = target;
            }

            public override void Set(Material mat)
            {
                var index = mat.shader.FindPropertyIndex(Target);
                var id = mat.shader.GetPropertyNameId(index);
                mat.SetColor(id, Color);
            }
        }
        class ValuesSetting : matSetting
        {
            public float Value;
            public ValuesSetting(Material mat, string name, string target)
            {
                Value = mat.GetFloat(name);
                Target = target;
            }

            public override void Set(Material mat)
            {
                var index = mat.shader.FindPropertyIndex(Target);
                var id = mat.shader.GetPropertyNameId(index);
                mat.SetFloat(id, Value);
            }
        }

        [Serializable]
        public class Config
        {
            public Mode ConvetMode;
            public string SourceShader;
            public string TargetShader;
            public List<Pair> Pairs;
        }

        #endregion

        [PropertyTooltip("Save convert setting as json file.")]
        [BoxGroup("Convert Setting")]
        [PropertySpace]
        [VerticalGroup("Convert Setting/Config/Options")]
        [TitleGroup("Convert Setting/Config/Options/Operate", Alignment = TitleAlignments.Centered)]
        [Button("Archive Config", ButtonHeight = 21)]
        public void ArchiveConfig()
        {
            var path = EditorHelper.ChoseFolder();
            if (string.IsNullOrEmpty(path))
                return;

            var config = new Config()
            {
                ConvetMode = this.ConvertMode,
                SourceShader = Source.Shader.name,
                TargetShader = Target.Shader.name,
                Pairs = this.Pairs
            };
            IOExtension.SaveJson(path, "ShaderConvertData_" + Source.Shader.name.Replace('/', '$') + "-" + Target.Shader.name.Replace('/', '$'), config);
            AssetDatabase.Refresh();
        }

        [PropertyTooltip("Load convert setting from json file.")]
        [BoxGroup("Convert Setting")]
        [PropertySpace]
        [VerticalGroup("Convert Setting/Config/Options")]
        [TitleGroup("Convert Setting/Config/Options/Operate", Alignment = TitleAlignments.Centered)]
        [Button("Load Config", ButtonHeight = 21)]
        public void LoadConfigData()
        {
            var path = EditorUtility.OpenFilePanel("Select Shader Convert Config", string.Empty, string.Empty);
            var p = IOExtension.LoadJson<Config>(path);

            if (p == null)
                return;


            foreach (var pairs in p.Pairs)
            {
                pairs.Source.OnAfterDeserialize();
                pairs.Target.OnAfterDeserialize();
            }

            Pairs = p.Pairs;
            Source.Shader = Shader.Find(p.SourceShader);
            Target.Shader = Shader.Find(p.TargetShader);
            Valid();
            AssetDatabase.Refresh();
        }

    }


}