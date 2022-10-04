using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System;
using System.Linq;
using Return;

namespace Return.Editors
{
    public partial class FirearmsAnimationToolkit : OdinEditorWindow, IModularTagUser
    {
        [MenuItem("Tools/Animation/FirearmsAnimationToolkit")]
        static void OpenWindow()
        {
            var window = GetWindow<FirearmsAnimationToolkit>();

            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
        }

        const string Anim = "Anim";

        [PropertyOrder(-2)]
        [SerializeField]
        public TagNamingModule FirearmsAnexTag;
        public TagNamingModule GetModularTags => FirearmsAnexTag;

        [PropertyOrder(-1.9f)]
        public GameObject Root;
        bool HasTarget => Root;

        [BoxGroup(Anim)]
        [Button(nameof(CreateAnim))]
        public virtual void CreateAnim(string name)
        {
            var clip = new AnimationClip
            {
                name = name,
            };

            if (GetWindow<AnimationWindow>() is AnimationWindow window)
            {
                window.animationClip = clip;
            }
            Anims.Add(clip);
        }

        [PropertyOrder(-1)]
        [BoxGroup(Anim)]
        [ListDrawerSettings(Expanded = true, NumberOfItemsPerPage = 7)]
        public List<AnimationClip> Anims;

        [BoxGroup(Anim)]
        [Button(nameof(Archive))]
        public void Archive()
        {

        }

        //[PropertyOrder(-1.7f)]
        [BoxGroup("AddAnex")]
        public GameObject AnexModel;

        //[PropertyOrder(-1.5f)]
        [BoxGroup("AddAnex")]
        [HideLabel]
        [ValueDropdown(nameof(DrawTag))]
        public string Tag;
        IEnumerable DrawTag()
        {
            foreach (var tag in FirearmsAnexTag.TagNames)
            {
                yield return tag;
            }
        }

        [BoxGroup("AddAnex", ShowLabel = false)]
        //[PropertyOrder(-1)]
        [EnableIf(nameof(HasTarget))]
        [HideLabel]
        [PropertySpace(5, 10)]
        [Button(nameof(AddAnex), Expanded = true)]
        public virtual void AddAnex()
        {
            if (!AnexModel || !Root)
                return;

            var anex = new AnexConfig(Root.transform, AnexModel)
            {
                ID = Tag,
            };
            anex.SetDefaultConfig();
            anex.KeyAnexFrame += Anex_KeyAnexFrame;
            Anexs.Add(anex);
        }

        private void Anex_KeyAnexFrame(List<AnimationClipCurveData> curveDatas)
        {
            var window = GetWindow<AnimationWindow>();
            if (!window)
                return;



            var clip = window.animationClip;

            if (!clip || !Anims.Contains(clip))
                return;


            var bindings = AnimationUtility.GetCurveBindings(clip);

            foreach (var binding in bindings)
            {
                Debug.Log(binding.path + "\n" + binding.propertyName + "\n" + binding.type);
            }

            foreach (var data in curveDatas)
            {
                clip.SetCurve(data.path, data.type, data.propertyName, data.curve);
            }
        }

        [PropertyOrder(1)]
        [BoxGroup("AddAnex")]
        [ListDrawerSettings(Expanded = true, HideAddButton = true, NumberOfItemsPerPage = 1, ShowPaging = true)]
        public List<AnexConfig> Anexs;


        [Serializable]
        public partial class AnexConfig
        {
            public AnexConfig(Transform root, GameObject model)
            {
                KeyFrames = new List<PR>(1);

                Root = root;
                AnexModel = model;
                Wrapper = model.transform.parent;
            }
            const string Position = "m_LocalPosition";
            const string Rotation = "localEulerAnglesRaw";
            public string ID;

            [ReadOnly]
            public Transform Root;

            [OnValueChanged(nameof(SetParent))]
            public Transform Wrapper;

            void SetParent()
            {
                if (!Wrapper)
                    return;

                AnexModel.transform.SetParent(Wrapper, true);
                ModelOffset = AnexModel.transform.GetLocalPR();
                KeyFrames[0] = new PR
                    (
                          Wrapper.InverseTransformPoint(AnexModel.transform.position),
                          AnexModel.transform.rotation * Quaternion.Inverse(Wrapper.rotation)
                    );
            }

            //[OnValueChanged(nameof(SetDefaultConfig))]
            [ReadOnly]
            public GameObject AnexModel;
            [ReadOnly]
            public EditorTransoformPoster OnMove;


            bool EditrWrapper;

            [ShowInInspector]
            public bool EditViaWrapper
            {
                get => EditrWrapper;
                set
                {
                    if (OnMove)
                        DestroyImmediate(OnMove);

                    if (value && Wrapper)
                        OnMove = Wrapper.gameObject.AddComponent<EditorTransoformPoster>();
                    else
                        OnMove = AnexModel.gameObject.AddComponent<EditorTransoformPoster>();

                    if (value != EditrWrapper && KeyFrames != null)
                    {
                        KeyFrames.Clear();

                        //var length = KeyFrames.Count;
                        //Transform from;
                        //Transform to;
                        //if (value) // wrapper to model
                        //{
                        //    from = Wrapper;
                        //    to = AnexModel.transform;
                        //}
                        //else
                        //{
                        //    from = AnexModel.transform;
                        //    to = Wrapper;
                        //}
                        //for (int i = 0; i < length; i++)
                        //{
                        //    var keyFrame = KeyFrames[i];

                        //    keyFrame.GUIDs = to.InverseTransformVector(from.TransformVector(keyFrame.GUIDs));
                        //    keyFrame.Rotation = to.rotation * (keyFrame.Rotation * Quaternion.Inverse(from.rotation));

                        //    KeyFrames[i] = keyFrame;
                        //}
                    }




                    EditrWrapper = value;
                }
            }

            public Color GizmosColor;


            public PR ModelOffset;


            public void SetDefaultConfig()
            {
                if (!AnexModel)
                {
                    if (OnMove)
                        DestroyImmediate(OnMove);

                    return;
                }


                if (!AnexModel.TryGetComponent(out OnMove))
                    OnMove = AnexModel.AddComponent<EditorTransoformPoster>();

                OnMove.OnMove -= Update;
                OnMove.OnMove += Update;

                GizmosColor = UnityEngine.Random.ColorHSV();

                Action<Transform> draw = null;

                if (AnexModel.TryGetComponent<MeshFilter>(out var ren))
                    draw = (x) =>
                    {
                        var color = Gizmos.color;
                        Gizmos.color = GizmosColor;
                        Gizmos.DrawWireMesh(ren.sharedMesh, 0, x.position, x.rotation, x.lossyScale);//,Quaternion.Euler(AnchorRotation))
                        Gizmos.color = color;
                    };
                //else
                //    draw = (Transform width) =>
                //    {
                //        var color = Gizmos.color;
                //        Gizmos.color = GizmosColor;
                //        Gizmos.DrawWireCube(width.TransformPoint(AnchorPosition), Vector3.one * 0.2f);//,Quaternion.Euler(AnchorRotation))
                //        Gizmos.color = color;
                //    };


                KeyFrames = new List<PR>()
            {
                new PR(),
            };
                SetParent();

                EditViaWrapper = true;
            }




            void Update(Transform tf)
            {
                CurrentFrame = tf.GetLocalPR();
                KeyFrames[FrameIndex] = CurrentFrame;
            }
            const string Anim = "Animation";

            [BoxGroup(Anim)]
            //[ListDrawerSettings(Expanded =true,CustomAddFunction =nameof(AddNewFrame),ShowIndexLabels =true)]
            List<PR> KeyFrames;


            [HorizontalGroup(Anim + "/KeyFrame")]
            [OnValueChanged(nameof(UpdateFrame))]
            [Index(nameof(max))]
            public int FrameIndex;

            int min
            {
                get
                {
                    if (KeyFrames == null)
                        return -1;

                    return 0;
                }
            }

            int max
            {
                get
                {
                    if (KeyFrames == null)
                        return -1;

                    return KeyFrames.Count - 1;
                }
            }

            [HorizontalGroup(Anim + "/EditKeyFrame")]
            [Button(nameof(AddKey))]
            void AddKey()
            {
                if (KeyFrames.Count > 0)
                {
                    KeyFrames.Insert(FrameIndex, KeyFrames[^1]);
                }
                else
                {
                    KeyFrames.Add(default);
                    FrameIndex = max;
                }
            }

            [HorizontalGroup(Anim + "/EditKeyFrame")]
            [Button(nameof(RemoveKey))]
            void RemoveKey()
            {
                if (KeyFrames.Count > FrameIndex)
                    KeyFrames.RemoveAt(FrameIndex);

                FrameIndex = Mathf.Clamp(FrameIndex - 1, min, max);
            }
            void UpdateFrame()
            {
                //Frame = Frame >= KeyFrames.Count ? 0 : Frame < 0 ? KeyFrames.Count - 1 : Frame;
                if (FrameIndex < 0)
                    FrameIndex = 0;

                CurrentFrame = KeyFrames[FrameIndex];
            }

            [BoxGroup(Anim)]
            [SerializeField]
            [HideLabel]
            [OnValueChanged(nameof(UpdateFrameData))]
            public PR CurrentFrame;


            void UpdateFrameData()
            {
                if (EditViaWrapper && Wrapper)
                    Wrapper.SetLocalPR(CurrentFrame);
                else
                    AnexModel.transform.SetLocalPR(CurrentFrame);

                KeyFrames[FrameIndex] = CurrentFrame;
            }


            [HorizontalGroup(Anim + "EditKeyFrame")]
            [Button(nameof(PreviewFrame))]
            void PreviewFrame()
            {
                var tf = AnexModel.transform;
                tf.localPosition = KeyFrames[FrameIndex];
                tf.localRotation = KeyFrames[FrameIndex];
                tf.hasChanged = false;
            }

            public event Action<List<AnimationClipCurveData>> KeyAnexFrame;

            List<AnimationClipCurveData> Cureves;

            [BoxGroup(Anim)]
            [Tooltip("Convert keyFrame data to curve data")]
            [Button(nameof(ParseCurve))]
            void ParseCurve()
            {
                Cureves = new List<AnimationClipCurveData>(6);

                Cureves.AddRange(BindVector3Curve(Root, AnexModel.transform, Position, 1, KeyFrames.Where(x => x.Position != default).Select(x => x.Position).ToArray()));
                Cureves.AddRange(BindVector3Curve(Root, AnexModel.transform, Rotation, 1, KeyFrames.Where(x => x.Rotation != default).Select(x => x.eulerAngles).ToArray()));
            }

            [BoxGroup(Anim)]
            [Tooltip("Key curve to current animation clip")]
            [Button(nameof(KeyCurve))]
            void KeyCurve()
            {
                KeyAnexFrame?.Invoke(Cureves);
            }

        }

        public static AnimationClipCurveData[] BindVector3Curve(Transform root, Transform target, string propertyName, float time, params Vector3[] values)
        {
            var count = 3;
            var curves = new List<AnimationClipCurveData>(count);
            var length = values.Length;
            var dic = new Dictionary<int, string>(count);
            dic.Add(0, ".width");
            dic.Add(1, ".height");
            dic.Add(2, ".z");

            var name = target.name;
            var parent = target.parent;
            while (parent != root)
            {
                name = parent.name + name;
                parent = parent.parent;
            }

            for (int n = 0; n < count; n++)
            {
                var keyFrames = new Keyframe[length];
                bool isDefault = true;
                for (int i = 0; i < length; i++)
                {
                    var value = values[i][n];
                    isDefault = isDefault && value == default;
                    keyFrames[i] = new Keyframe((float)i / length * time, value);
                }


                if (!isDefault)
                    curves.Add(
                        new AnimationClipCurveData
                        (
                            new EditorCurveBinding() { path = name, type = typeof(Transform), propertyName = propertyName + dic[n] })
                        {
                            curve = new AnimationCurve(keyFrames)
                        }
                        );
                else
                    curves.Add(default);
            }
            return curves.ToArray();
        }

    }
}