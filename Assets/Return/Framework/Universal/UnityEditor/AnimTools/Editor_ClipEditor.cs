using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using System.Linq;

public class Editor_ClipEditor : OdinEditorWindow
{
    [MenuItem("Tools/Animation/ClipEditor")]
    private static void OpenWindow()
    {
        GetWindow<Editor_ClipEditor>().position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        LoadClips();
    }

    public enum Tool { EditCurve, }

    [OnValueChanged(nameof(LoadClips))]
    public AnimationClip clip;

    [System.Serializable]
    public struct Binding:ISearchFilterable
    {
        [Tooltip("ReadOnlyTransform name")]
        public string ID;
        public string PropertyName;
        [HideInInspector]
        public string Path;
        [ShowInInspector][ReadOnly]
        public System.Type Type;
        public Binding(EditorCurveBinding editorBinding)
        {
            Path = editorBinding.path;
            PropertyName = editorBinding.propertyName;
            ID = editorBinding.path.LastSection('/');
            Type = editorBinding.type;
        }

        public EditorCurveBinding GetValue
        {
            get
            {
                return new EditorCurveBinding() { propertyName = PropertyName, path = Path, type = Type };
            }
        }

        public bool IsMatch(string searchString)
        {
            return searchString.ToLower() == ID.ToLower();
        }

        public bool IsFirstChild(string id)
        {
            id = id.ToLower();
            var mPath = Path.ToLower();
            if (mPath.Contains(id))
            {
                var sections = mPath.Split('/');
                if (sections.Length > 1)
                    if (sections[sections.Length - 2] == id)
                        return true;
            }

            return false;
        }
    }


    [Searchable(FilterOptions =SearchFilterOptions.ISearchFilterableInterface)]
    public Binding[] Bindings;
    [ShowInInspector]
    public AnimationCurve mCurve;
    public AvatarMask mask;

    public bool NameSpaceField=true;
    [ShowIf(nameof(NameSpaceField))]
    public UnityEngine.Object ob;
    public bool PingBack = true;
    public string Filter;
    Vector2 scroll;

    public void LoadClips()
    {

        if (clip)
        {
            Bindings = AnimationUtility.GetCurveBindings(clip).Select(x=>new Binding(x)).ToArray();
        }
        else
            Bindings = new Binding[0];
    }

    [Button("DeleteAndInheritByFirstChild")]
    public void DeleteTransform(string name)
    {
        var parentBindings = Bindings.Where(x => x.IsMatch(name)).Select(x => x.GetValue).ToArray();

        var childBindings = Bindings.Where(x => x.IsFirstChild(name)).Select(x=>x.GetValue).ToArray();

        var parent = GetDatas(parentBindings)[0];
        var childs = GetDatas(childBindings);
        foreach(var child in childs)
        {
            child.ResolveParent(parent);
        }
        var parentBinding = parentBindings[0];
        clip.SetCurve(parentBinding.path, parentBinding.type, Position, null);
        clip.SetCurve(parentBinding.path, parentBinding.type, Rotation, null);
        clip.SetCurve(parentBinding.path, parentBinding.type, EulerAngles, null);
        clip.SetCurve(parentBinding.path, parentBinding.type, Scale, null);
    }

    TransformCurveData[] GetDatas(EditorCurveBinding[] bindings)
    {
        var d = new Dictionary<string, TransformCurveData>(bindings.Length * 3);

        foreach (var binding in bindings)
        {
            if (d.TryGetValue(binding.path, out var data))
            {
                data.SetCurve(binding);
            }
            else
            {
                data = new TransformCurveData(binding.path, binding.type, clip);
                data.SetCurve(binding);
                d.Add(binding.path, data);
            }
        }
        return d.Select(x => x.Value).ToArray();

    }

    protected override void OnGUI()
    {
        base.OnGUI();


            return;




        if (GUILayout.Button("Archive"))
        {
            var newClip = new AnimationClip();
            var bindings = AnimationUtility.GetCurveBindings(clip);

            foreach (var binding in bindings)
            {
                newClip.SetCurve(binding.path, binding.type, binding.propertyName, AnimationUtility.GetEditorCurve(clip, binding));
            }
            string space = ob? ob.name:string.Empty;
            EditorHelper.WriteAsset(newClip,space+"_"+clip.name);

            if (PingBack)
                EditorGUIUtility.PingObject(clip);
        }


        if (mask)
        {
            if (GUILayout.Button("Filter by avatar"))
            {
                var curveBindings = AnimationUtility.GetCurveBindings(clip);
                var length = curveBindings.Length;
                Dictionary<string, string> bindings=new Dictionary<string, string>(length);
                for (int i = 0; i < length; i++)
                {
                    bindings.Add(curveBindings[i].propertyName,curveBindings[i].path);
                }

                var map = BindingMap;
                length = (int)AvatarMaskBodyPart.LastBodyPart;
                for (int i = 0; i < length; i++)
                {
                    var m = (AvatarMaskBodyPart)i;

                    if (mask.GetHumanoidBodyPartActive(m))
                        continue;

                    if(map.TryGetValue((AvatarMaskBodyPart)i, out var properties))
                    {
                        foreach(var property in properties)
                        {
                            if (bindings.TryGetValue(property, out var path))
                                clip.SetCurve(path, typeof(Animator), property, null);
                        }
                    }
                }
            }
        }

        Filter = GUILayout.TextField(Filter);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            if (!string.IsNullOrEmpty(Filter))
            {
                var name = binding.path;
                var index = name.LastIndexOf("/");
                if (index > 0)
                    name = name.Substring(index);

                if (!name.ToLower().Contains(Filter.ToLower()))
                    continue;
            }

            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            EditorGUILayout.BeginHorizontal();
     
            if (GUILayout.Button(binding.path + " => " + binding.propertyName + ", Keys: " + curve.keys.Length))
            {
                mCurve = curve;
            }
            if (GUILayout.Button("Delete"))
            {
                clip.SetCurve(binding.path, binding.type, binding.propertyName, null);
            }
            stringBuilder.Append("\"" + binding.propertyName + "\"" + "," + "\n");

            
            if (GUILayout.Button("Zero"))
            {
                var keys = curve.keys;
                var length = keys.Length;
                var diff= keys[0].value;
                for (int i = 0; i < length; i++)
                {
                    var key = keys[i];
                    keys[i] = new Keyframe(key.time, key.value - diff,key.inTangent,key.outTangent);
                }
                keys[0].value = 0;
                curve.keys = keys;
                clip.SetCurve(binding.path, binding.type, binding.propertyName, curve);
                Debug.Log(curve.keys[0].value);
                EditorUtility.SetDirty(clip);
            }

            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("CopyPropertyName"))
            UnityEditor.EditorGUIUtility.systemCopyBuffer = stringBuilder.ToString();
        EditorGUILayout.EndScrollView();


    }

    public static Dictionary<AvatarMaskBodyPart, string[]> BindingMap = new Dictionary<AvatarMaskBodyPart, string[]>
    {
        {
            AvatarMaskBodyPart.Root,
            new string[]
            {
                "RootQ.x",
                "RootQ.y",
                "RootQ.z",
                "RootQ.w",
                "RootT.x",
                "RootT.y",
                "RootT.z",
                "MotionQ.x",
                "MotionQ.y",
                "MotionQ.z",
                "MotionQ.w",
                "MotionT.x",
                "MotionT.y",
                "MotionT.z",
            }
        },

        {
            AvatarMaskBodyPart.Body,
            new string[]
            {
                "UpperChest Twist Left-Right",
                "UpperChest Left-Right",
                "UpperChest Front-Back",
                "Chest Twist Left-Right",
                "Chest Left-Right",
                "Chest Front-Back",
                "Spine Twist Left-Right",
                "Spine Left-Right",
                "Spine Front-Back",
            }
        },

        {
            AvatarMaskBodyPart.Head,
            new string[]
            {
                "Jaw Left-Right",
                "Jaw Close",
                "Right Eye In-Out",
                "Right Eye Down-Up",
                "Left Eye In-Out",
                "Left Eye Down-Up",
                "Head Turn Left-Right",
                "Head Tilt Left-Right",
                "Head Nod Down-Up",
                "Neck Turn Left-Right",
                "Neck Tilt Left-Right",
                "Neck Nod Down-Up",
            }
        },

        {
            AvatarMaskBodyPart.LeftLeg,
            new string[]
            {
"Left Toes Up-Down",
"Left Foot Twist In-Out",
"Left Foot Up-Down",
"Left Lower Leg Twist In-Out",
"Left Lower Leg Stretch",
"Left Upper Leg Twist In-Out",
"Left Upper Leg In-Out",
"Left Upper Leg Front-Back",
            }
        },

        {
            AvatarMaskBodyPart.RightLeg,
            new string[]
            {
"Right Toes Up-Down",
"Right Foot Twist In-Out",
"Right Foot Up-Down",
"Right Lower Leg Twist In-Out",
"Right Lower Leg Stretch",
"Right Upper Leg Twist In-Out",
"Right Upper Leg In-Out",
"Right Upper Leg Front-Back",
            }
        },

        {
            AvatarMaskBodyPart.LeftArm,
            new string[]
            {
"Left Hand In-Out",
"Left Hand Down-Up",
"Left Forearm Twist In-Out",
"Left Forearm Stretch",
"Left Arm Twist In-Out",
"Left Arm Front-Back",
"Left Arm Down-Up",
"Left Shoulder Front-Back",
"Left Shoulder Down-Up",
            }
        },

        {
            AvatarMaskBodyPart.RightArm,
            new string[]
            {
"Right Hand In-Out",
"Right Hand Down-Up",
"Right Forearm Twist In-Out",
"Right Forearm Stretch",
"Right Arm Twist In-Out",
"Right Arm Front-Back",
"Right Arm Down-Up",
"Right Shoulder Front-Back",
"Right Shoulder Down-Up",
            }
        },


        {
            AvatarMaskBodyPart.LeftFingers,
            new string[]
            {
"LeftHand.Little.3 Stretched",
"LeftHand.Little.2 Stretched",
"LeftHand.Little.Spread",
"LeftHand.Little.1 Stretched",
"LeftHand.Ring.3 Stretched",
"LeftHand.Ring.2 Stretched",
"LeftHand.Ring.Spread",
"LeftHand.Ring.1 Stretched",
"LeftHand.Middle.3 Stretched",
"LeftHand.Middle.2 Stretched",
"LeftHand.Middle.Spread",
"LeftHand.Middle.1 Stretched",
"LeftHand.Index.3 Stretched",
"LeftHand.Index.2 Stretched",
"LeftHand.Index.Spread",
"LeftHand.Index.1 Stretched",
"LeftHand.Thumb.3 Stretched",
"LeftHand.Thumb.2 Stretched",
"LeftHand.Thumb.Spread",
"LeftHand.Thumb.1 Stretched",
            }
        },

        {
            AvatarMaskBodyPart.RightFingers,
            new string[]
            {
                "RightHand.Little.3 Stretched",
"RightHand.Little.2 Stretched",
"RightHand.Little.Spread",
"RightHand.Little.1 Stretched",
"RightHand.Ring.3 Stretched",
"RightHand.Ring.2 Stretched",
"RightHand.Ring.Spread",
"RightHand.Ring.1 Stretched",
"RightHand.Middle.3 Stretched",
"RightHand.Middle.2 Stretched",
"RightHand.Middle.Spread",
"RightHand.Middle.1 Stretched",
"RightHand.Index.3 Stretched",
"RightHand.Index.2 Stretched",
"RightHand.Index.Spread",
"RightHand.Index.1 Stretched",
"RightHand.Thumb.3 Stretched",
"RightHand.Thumb.2 Stretched",
"RightHand.Thumb.Spread",
"RightHand.Thumb.1 Stretched",
            }
        },

        {
            AvatarMaskBodyPart.LeftFootIK,
            new string[]
            {
                "LeftFootQ.x",
                "LeftFootQ.y",
                "LeftFootQ.z",
                "LeftFootQ.w",
                "LeftFootT.x",
                "LeftFootT.y",
                "LeftFootT.z",
            }
        },

        {
            AvatarMaskBodyPart.RightFootIK,
            new string[]
            {
                "RightFootQ.x",
                "RightFootQ.y",
                "RightFootQ.z",
                "RightFootQ.w",
                "RightFootT.x",
                "RightFootT.y",
                "RightFootT.z",
            }
        },

        {
            AvatarMaskBodyPart.LeftHandIK,
            new string[]
            {
                "LeftHandQ.x",
                "LeftHandQ.y",
                "LeftHandQ.z",
                "LeftHandQ.w",
                "LeftHandT.x",
                "LeftHandT.y",
                "LeftHandT.z",
            }
        },

        {
            AvatarMaskBodyPart.RightHandIK,
            new string[]
            {
                "RightHandQ.x",
                "RightHandQ.y",
                "RightHandQ.z",
                "RightHandQ.w",
                "RightHandT.x",
                "RightHandT.y",
                "RightHandT.z",
            }
        },
    };

    public struct PRS
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

    }
    public const string Position = "m_LocalPosition";
    public const string Rotation = "m_LocalRotation";
    public const string EulerAngles = "m_localEulerAnglesRaw";
    public const string Scale = "m_LocalScale";

    public const string Position_x = "m_LocalPosition.x";
    public const string Position_y = "m_LocalPosition.y";
    public const string Position_z = "m_LocalPosition.z";

    public const string Rotation_x = "m_LocalRotation.x";
    public const string Rotation_y = "m_LocalRotation.y";
    public const string Rotation_z = "m_LocalRotation.z";
    public const string Rotation_w = "m_LocalRotation.w";

    public const string EulerAngles_x = "m_localEulerAnglesRaw.x";
    public const string EulerAngles_y = "m_localEulerAnglesRaw.y";
    public const string EulerAngles_z = "m_localEulerAnglesRaw.z";

    public const string Scale_x = "m_LocalScale.x";
    public const string Scale_y = "m_LocalScale.y";
    public const string Scale_z = "m_LocalScale.z";
    


    public class TransformCurveData
    {

        public TransformCurveData(string path, System.Type type,AnimationClip clip)
        {
            PropertyPath = path;
            Type = type;
            Clip=clip;
        }

        readonly string PropertyPath;
        readonly System.Type Type;
        AnimationClip Clip;
        public AnimationCurve Position_X;
        public AnimationCurve Position_Y;
        public AnimationCurve Position_Z;

        public AnimationCurve Rotation_X;
        public AnimationCurve Rotation_Y;
        public AnimationCurve Rotation_Z;
        public AnimationCurve Rotation_W;

        public AnimationCurve Scale_X;
        public AnimationCurve Scale_Y;
        public AnimationCurve Scale_Z;

        public void SetCurve( EditorCurveBinding binding)
        {
            var curve=AnimationUtility.GetEditorCurve(Clip, binding);
            switch (binding.propertyName)
            {
                case Position_x:
                    Position_X = curve;
                    break;
                case Position_y:
                    Position_Y = curve;
                    break;
                case Position_z:
                    Position_Z = curve;
                    break;

                case Rotation_x:
                    Rotation_X = curve;
                    break;
                case Rotation_y:
                    Rotation_Y = curve;
                    break;
                case Rotation_z:
                    Rotation_Z = curve;
                    break;
                case Rotation_w:
                    Rotation_W = curve;
                    break;


                case EulerAngles_x:
                    Rotation_X = curve;
                    break;
                case EulerAngles_y:
                    Rotation_Y = curve;
                    break;
                case EulerAngles_z:
                    Rotation_Z = curve;
                    break;

                    

                case Scale_x:
                    Scale_X = curve;
                    break;
                case Scale_y:
                    Scale_Y = curve;
                    break;
                case Scale_z:
                    Scale_Z = curve;
                    break;
            }
        }

        public Vector3 Position(float time)
        {
            var pos = Vector3.zero;
            if (Position_X!=null)
                pos.x = Position_X.Evaluate(time);

            if (Position_Y != null)
                pos.y = Position_Y.Evaluate(time);

            if (Position_Z != null)
                pos.z = Position_Z.Evaluate(time);
            return pos;
        }

        public Quaternion Rotation(float time)
        {
            if(Rotation_W==null)
            {
                var eulerAngles = Vector3.zero;

                if (Rotation_X != null)
                    eulerAngles.x = Rotation_X.Evaluate(time);

                if (Rotation_Y != null)
                    eulerAngles.y = Rotation_Y.Evaluate(time);

                if (Rotation_Z != null)
                    eulerAngles.z = Rotation_Z.Evaluate(time);

                return Quaternion.Euler(eulerAngles);
            }

            var quat = Quaternion.identity;

            if (Rotation_X != null)
                quat.x = Rotation_X.Evaluate(time);

            if (Rotation_Y != null)
                quat.y = Rotation_Y.Evaluate(time);

            if (Rotation_Z != null)
                quat.z = Rotation_Z.Evaluate(time);

            if (Rotation_W != null)
                quat.w = Rotation_W.Evaluate(time);

            return quat;
        }

        public Vector3 Scale(float time)
        {
            var pos = Vector3.one;
            if (Scale_X != null)
                pos.x = Scale_X.Evaluate(time);

            if (Scale_Y != null)
                pos.y = Scale_Y.Evaluate(time);

            if (Scale_Z != null)
                pos.z = Scale_Z.Evaluate(time);
            return pos;
        }

        public PRS Evaluate(float time)
        {
            return new PRS()
            {
                Position = Position(time),
                Rotation = Rotation(time),
                Scale = Scale(time)
            };
        }

        public Matrix4x4 Matrix(float time)
        {
            return Matrix4x4.TRS(Position(time), Rotation(time), Scale(time));
        }

        public void ResolveParent(TransformCurveData parent)
        {
            Dictionary<float, Matrix4x4> parentCatch = new Dictionary<float, Matrix4x4>();
            Dictionary<float, PRS> mCatch = new Dictionary<float, PRS>();

            if (Position_X != null)
            {
                var keys = Position_X.keys;
                var length = keys.Length;

                for (int i = 0; i < length; i++)
                {
                    var time = keys[i].time;

                    if (!parentCatch.TryGetValue(time, out var matrix))
                    {
                        matrix = parent.Matrix(time);
                        parentCatch.Add(time, matrix);
                    }



                    if (!mCatch.TryGetValue(time, out var prs))
                    {
                        prs = Evaluate(time);
                        mCatch.Add(time, prs);
                    }
                    keys[i].value = matrix.MultiplyPoint(prs.Position).x;
                }
                Position_X.keys = keys;

                Clip.SetCurve(PropertyPath, Type, Position_x, Position_X);
            }

            if (Position_Y != null)
            {
                var keys = Position_Y.keys;
                var length = keys.Length;

                for (int i = 0; i < length; i++)
                {
                    var time = keys[i].time;

                    if (!parentCatch.TryGetValue(time, out var matrix))
                    {
                        matrix = parent.Matrix(time);
                        parentCatch.Add(time, matrix);
                    }



                    if (!mCatch.TryGetValue(time, out var prs))
                    {
                        prs = Evaluate(time);
                        mCatch.Add(time, prs);
                    }
                    keys[i].value = matrix.MultiplyPoint(prs.Position).y;
                }
                Position_Y.keys = keys;
                Clip.SetCurve(PropertyPath, Type, Position_y, Position_Y);
            }

            if (Position_Z != null)
            {
                var keys = Position_Z.keys;
                var length = keys.Length;

                for (int i = 0; i < length; i++)
                {
                    var time = keys[i].time;

                    if (!parentCatch.TryGetValue(time, out var matrix))
                    {
                        matrix = parent.Matrix(time);
                        parentCatch.Add(time, matrix);
                    }



                    if (!mCatch.TryGetValue(time, out var prs))
                    {
                        prs = Evaluate(time);
                        mCatch.Add(time, prs);
                    }
                    keys[i].value = matrix.MultiplyPoint(prs.Position).z;
                }
                Position_Z.keys = keys;
                Clip.SetCurve(PropertyPath, Type, Position_z, Position_Z);
            }


            if (Rotation_X != null)
            {
                var keys = Rotation_X.keys;
                var length = keys.Length;

                for (int i = 0; i < length; i++)
                {
                    var time = keys[i].time;

                    if (!parentCatch.TryGetValue(time, out var matrix))
                    {
                        matrix = parent.Matrix(time);
                        parentCatch.Add(time, matrix);
                    }

                    if (!mCatch.TryGetValue(time, out var prs))
                    {
                        prs = Evaluate(time);
                        mCatch.Add(time, prs);
                    }
                    keys[i].value = (matrix.rotation*prs.Rotation).x;
                }
                Rotation_X.keys = keys;
                Clip.SetCurve(PropertyPath, Type, Rotation_x, Rotation_X);
            }


            if (Rotation_Y != null)
            {
                var keys = Rotation_Y.keys;
                var length = keys.Length;

                for (int i = 0; i < length; i++)
                {
                    var time = keys[i].time;

                    if (!parentCatch.TryGetValue(time, out var matrix))
                    {
                        matrix = parent.Matrix(time);
                        parentCatch.Add(time, matrix);
                    }

                    if (!mCatch.TryGetValue(time, out var prs))
                    {
                        prs = Evaluate(time);
                        mCatch.Add(time, prs);
                    }
                    keys[i].value = (matrix.rotation * prs.Rotation).y;
                }
                Rotation_Y.keys = keys;
                Clip.SetCurve(PropertyPath, Type, Rotation_y, Rotation_Y);
            }


            if (Rotation_Z != null)
            {
                var keys = Rotation_Z.keys;
                var length = keys.Length;

                for (int i = 0; i < length; i++)
                {
                    var time = keys[i].time;

                    if (!parentCatch.TryGetValue(time, out var matrix))
                    {
                        matrix = parent.Matrix(time);
                        parentCatch.Add(time, matrix);
                    }

                    if (!mCatch.TryGetValue(time, out var prs))
                    {
                        prs = Evaluate(time);
                        mCatch.Add(time, prs);
                    }
                    keys[i].value = (matrix.rotation * prs.Rotation).z;
                }
                Rotation_Z.keys = keys;
                Clip.SetCurve(PropertyPath, Type, Rotation_z, Rotation_Z);
            }

            if (Rotation_W != null)
            {
                var keys = Rotation_W.keys;
                var length = keys.Length;

                for (int i = 0; i < length; i++)
                {
                    var time = keys[i].time;

                    if (!parentCatch.TryGetValue(time, out var matrix))
                    {
                        matrix = parent.Matrix(time);
                        parentCatch.Add(time, matrix);
                    }

                    if (!mCatch.TryGetValue(time, out var prs))
                    {
                        prs = Evaluate(time);
                        mCatch.Add(time, prs);
                    }
                    keys[i].value = (matrix.rotation * prs.Rotation).w;
                }
                Rotation_W.keys = keys;
                Clip.SetCurve(PropertyPath, Type, Rotation_w, Rotation_W);
            }


        }
    }
}

