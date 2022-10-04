using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using System.Linq;
using UnityEngine.Playables;
using Return.Humanoid.Animation;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using UnityEngine.UIElements;
using UnityEngine.Animations.Rigging;
using Return;

namespace Return.Editors
{
    public partial class CharacterDirector : OdinEditorWindow
    {
        CharacterDirector()
        {
            titleContent = new GUIContent("CharacterDirector");
        }
        [MenuItem("Tools/Animation/Rigs/CharacterDirector")]
        private static void Init()
        {
            var window = (CharacterDirector)GetWindow(typeof(CharacterDirector));
            window.Show();
        }

        #region Layout
        public const string Layout_Top = "Panel";
        public const string Layout_Top_State = "Panel/State";
        public const string Layout_State_Target = "Panel/State/Target";
        public const string Layout_State_Tab = "Panel/State/Tab";

        public const string Layout_Tab_IK = "Panel/State/IK";
        public const string Layout_IK_Handle = "Panel/State/Tab/IK/Handle";
        public const string Layout_Handle_Rig = "Panel/State/Tab/IK/Handle/HandIK";
        public const string Layout_Rig_Config = "Panel/State/Tab/IK/Handle/Config";

        public const string Layout_Tab_Muscle = "Panel/State/Muscle";
        public const string Layout_Muscle_Handle = "Panel/State/Tab/Muscle/Handle";
        public const string Layout_Handle_Group = "Panel/State/Tab/Muscle/Handle/GroupHandles";
        public const string Layout_Handle_Target = "Panel/State/Tab/Muscle/Handle/GroupHandles/Target";

        public enum PreviewType
        {
            /// <summary>
            /// Animator controller graph => Animator Controller
            /// </summary>
            InjectAnimatorGraph,
            /// <summary>
            /// Customize playable graph => Clip or TPose
            /// </summary>
            CustomizeGraph
        }
        public const string PreviewPref = "CharacterDirctorPreviewPref";
        #endregion

        #region Target

        [PropertyOrder(-5)]
        [PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
        [BoxGroup(Layout_Top_State, ShowLabel = false)]
        [HorizontalGroup(Layout_State_Target)]
        [ShowInInspector]
        [OnValueChanged(nameof(ResetCharacter))]
        [DisableIf(nameof(VaildAnimator))]
        [ValidateInput(nameof(VaildAnimator),
            MessageType = InfoMessageType.Error,
            ContinuousValidationCheck = true,
            DefaultMessage = "Please select a animator which contains humanoid avatar.")]
        Animator Animator
        {
            get => m_Animator;
            set => m_Animator = value;
        }

        Animator m_Animator;

        [ShowIf(nameof(VaildAnimator))]
        [HorizontalGroup(Layout_State_Target)]
        [Button("Clear")]
        void ClearAnimator()
        {
            ResetCharacter();
            m_Animator = null;
        }

        bool VaildAnimator
        {
            get => m_Animator && m_Animator.isHuman;
        }

        [PropertyOrder(-4)]
        [BoxGroup(Layout_Top_State)]
        [OnValueChanged(nameof(ValidGraph))]
        public PreviewType m_PreviewType;
        void ValidGraph()
        {
            EditorPrefs.SetString(PreviewPref, m_PreviewType.ToString());
        }


        bool DisableFunction;
        #endregion

        #region Seletor
        AvatarSelector Selector;
        WrapWindow Wrapper;


        [PropertyOrder(-10)]
        [OnInspectorGUI]
        [HorizontalGroup(Layout_Top)]
        void DrawAvatar()
        {
            var rect = new Rect(new Vector2(15, 0), new Vector2(150, 300));
            Selector.DrawPreview(rect);
            style_Handle = GUI.skin.GetStyle("Box");
        }
        #endregion


        #region IK
        bool usingIK;

        [PropertyOrder(-3)]
        [DisableIf(nameof(DisableFunction))]
        [TabGroup(Layout_State_Tab, "IK")]
        [BoxGroup(Layout_IK_Handle, ShowLabel = false)]
        [OnInspectorGUI]
        void DrawIKConfigs()
        {
            usingIK = true;

            EditOption("IK Rigging");
        }

        [ReadOnly]
        [HorizontalGroup(Layout_Handle_Rig)]
        public Rig HandIK;
        [DisableIf(nameof(ValidHandIK))]
        [HorizontalGroup(Layout_Handle_Rig)]
        [Button("Create Hand IK")]
        void CreateIK_Hand()
        {
            var twoBoneIKs = m_Animator.GetComponentsInChildren<TwoBoneIKConstraint>();
            foreach (var ik in twoBoneIKs)
            {
                if (ik.name.ToUpper().Contains("Hand"))
                {
                    HandIK = ik.GetComponentInParent<Rig>();
                    if (HandIK)
                        return;
                }
            }

            HandIK = CreateRig.BuildHandRigs(m_Animator, true);

            builder = m_Animator.GetComponent<RigBuilder>();
            InitIK();
        }

        void InitIK()
        {
            mDicIKUpdate = new Dictionary<Transform, AvatarMaskBodyPart>();
            if (builder)
            {
                var hands = HandIK.GetComponentsInChildren<TwoBoneIKConstraint>();
                foreach (var hand in hands)
                {
                    var id = hand.gameObject.name.ToUpper();
                    var maskPart = AvatarMaskBodyPart.LastBodyPart;
                    if (id.Contains("RIGHT"))
                    {
                        RightHandIKRig = hand;
                        maskPart = AvatarMaskBodyPart.RightArm;
                    }
                    else
                    {
                        LeftHandIKRig = hand;
                        maskPart = AvatarMaskBodyPart.LeftArm;
                    }

                    SubscribeTransformUpdate(hand.data.target.gameObject, maskPart);
                    SubscribeTransformUpdate(hand.data.hint.gameObject, maskPart);
                }
                builder.runInEditMode = true;
            }
        }
        void SubscribeTransformUpdate(GameObject @object, AvatarMaskBodyPart maskPart)
        {
            var checkUpdate = @object.InstanceIfNull<EditorTransformUpdate>();
            mDicIKUpdate.SafeAdd(checkUpdate.transform, maskPart);
            checkUpdate.TransformUpdate += UpdateIK;
        }

        [ShowInInspector]
        Dictionary<Transform, AvatarMaskBodyPart> mDicIKUpdate;

        void UpdateIK(Transform tf)
        {
            if (mDicIKUpdate.TryGetValue(tf, out var maskPart))
                SetMuscleIK(_IKOverride, maskPart.ToHumanDof());
        }

        //[HideLabel]
        //[ReadOnly]
        //[HorizontalGroup(Layout_Rig_Config)]
        public TwoBoneIKConstraint RightHandIKRig;


        //[HideLabel]
        //[ReadOnly]
        //[HorizontalGroup(Layout_Rig_Config)]
        public TwoBoneIKConstraint LeftHandIKRig;


        bool ValidHandIK => HandIK;
        Playable IKPlayable;
        RigBuilder builder;
        void ValidIK()
        {
            var graph = GetGraph;
            if (_IKOverride && graph.IsPlaying() && graph.IsPlaying())
            {
                if (builder)
                {
                    RightHandIKRig.data.target.Copy(RightHandIKRig.data.tip);
                    LeftHandIKRig.data.target.Copy(LeftHandIKRig.data.tip);

                    builder.Build();

                    if (musclePlayable.IsValid())
                        musclePlayable.AddInput(IKPlayable, 0, 1);
                }
            }
            else
            {
                if (m_Animator)
                {
                    var builder = m_Animator.GetComponent<RigBuilder>();
                    if (builder)
                        builder.Clear();
                }
            }
        }
        HumanPoseHandler PoseHandler;
        HumanPose IKPose;
        void SetMuscleIK(bool enable, HumanPartDof humanDof)
        {
            var group = Groups[humanDof];

            PoseHandler.GetHumanPose(ref IKPose);
            var muscles = IKPose.muscles;
            var length = group.Count;
            var index = group.Min;
            for (int i = 0; i < length; i++)
            {
                var data = group[i];
                data.m_Value = muscles[i + index];
                group[i] = data;
            }


            //var length = group.Max - group.Min;
            //for (int i = 0; i < length; i++)
            //{
            //    var data= group[i];
            //    data.InheritIKOverwrite = enable;
            //    group[i] = data;
            //}
        }

        #endregion




        [Serializable]
        public class MuscleGroup
        {
            public event Action UpdateMuscle;
            public MuscleGroup(HumanPartDof dof, MuscleData[] datas)
            {
                Datas = datas;
                GroupName = dof;
                m_Datas = new List<MuscleData>();
            }
            [HideInInspector]
            public readonly HumanPartDof GroupName;
            [HideInInspector]
            public readonly MuscleData[] Datas;

            [HideInInspector]
            public int Max = int.MinValue;
            [HideInInspector]
            public int Min = int.MaxValue;
            public MuscleData this[int index]
            {
                get
                {
                    return Datas[index + Min];
                }
                set
                {
                    var sn = index + Min;
                    var old = Datas[sn];
                    if (Datas[sn].Equals(value))
                        return;
                    Datas[index + Min] = value;
                    UpdateMuscle?.Invoke();
                }
            }

            public int Count => Max - Min + 1;

            public void KeyVolume(int sn, MuscleData muscleData)
            {
                Max = Mathf.Max(sn, Max);
                Min = Mathf.Min(sn, Min);
                m_Datas.Add(muscleData);
            }

            [HideLabel]
            [ShowInInspector]
            [OnValueChanged(nameof(Update), IncludeChildren = true, InvokeOnInitialize = false)]
            protected List<MuscleData> m_Datas;

            void Update()
            {
                var length = m_Datas.Count;

                for (int i = 0; i < length; i++)
                {
                    Debug.Log(Datas[Min + i]);
                    Datas[Min + i].m_Value = m_Datas[i].m_Value;
                }

                UpdateMuscle?.Invoke();
            }
        }





        #region Muscles
        [ReadOnly]
        [HideInInspector]
        [ListDrawerSettings(Expanded = true)]
        MuscleData[] muscleDatas;

        //    [HideInInspector]
        public Dictionary<HumanPartDof, MuscleGroup> Groups;
        bool _IKOverride;

        bool SyncEdit;
        bool IKOverride
        {
            get => _IKOverride;
            set
            {
                if (_IKOverride == value)
                    return;

                _IKOverride = value;
                ValidIK();
            }
        }
        bool MuscleClamp = true;

        #region Handle

        [HideInInspector]
        public Mesh HandleMesh;

        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        [DisableIf(nameof(DisableFunction))]
        [TabGroup(Layout_State_Tab, "Muscle")]
        [BoxGroup(Layout_Muscle_Handle, ShowLabel = false)]
        [OnInspectorGUI]
        void DrawHandles()
        {

            layout_Slider = new[] { GUILayout.Width(200) };
            layout_Toggle = new[] { GUILayout.Width(20) };
            var dof = Selector.PartDof;
            switch (dof)
            {
                case AvatarMaskBodyPart.LeftFingers:
                    Handle_Fingers("Left");
                    Handle_LeftFingers();
                    break;

                case AvatarMaskBodyPart.RightFingers:
                    Handle_Fingers("Right");
                    Handle_RightFingers();
                    break;

                case AvatarMaskBodyPart.Body:
                    Handle_Spine();
                    Handle_Body();
                    break;
                case AvatarMaskBodyPart.Head:
                    Handle_Spine();
                    Handle_Head();
                    break;

                case AvatarMaskBodyPart.LeftLeg:
                    Handle_Legs();
                    Handle_RightLeg();
                    break;

                case AvatarMaskBodyPart.RightLeg:
                    Handle_Legs();
                    Handle_LeftLeg();
                    break;

                case AvatarMaskBodyPart.LeftArm:
                    Handle_Arms();
                    Handle_LeftArm();
                    break;

                case AvatarMaskBodyPart.RightArm:
                    Handle_Arms();
                    Handle_RightArm();
                    break;

                default:
                    return;
            }


        }
        GUIStyle style_Handle;
        GUILayoutOption[] layout_Slider;
        GUILayoutOption[] layout_Toggle;

        #region Top
        [BoxGroup(Layout_Handle_Group)]
        void Handle_Spine()
        {
            GUILayout.BeginVertical(style_Handle);
            GUILayout.Label(Selector.PartDof.ToString());
            GUILayout.EndVertical();
        }
        [BoxGroup(Layout_Handle_Target)]
        void Handle_Head()
        {
            var group = Groups[HumanPartDof.Head];
            var length = (int)HeadDof.LastHeadDof;
            LoadMuscle(group, length);
        }
        [BoxGroup(Layout_Handle_Target)]
        void Handle_Body()
        {
            var group = Groups[HumanPartDof.Body];
            var length = (int)BodyDof.LastBodyDof;
            LoadMuscle(group, length);
        }

        #endregion

        #region Hand
        [BoxGroup(Layout_Handle_Group)]
        void Handle_Arms()
        {
            EditOption(Selector.PartDof.ToString());
        }
        [BoxGroup(Layout_Handle_Target)]
        void Handle_RightArm()
        {
            var group = Groups[HumanPartDof.RightArm];
            Handle_Arm(group);
        }
        [BoxGroup(Layout_Handle_Target)]
        void Handle_LeftArm()
        {
            var group = Groups[HumanPartDof.LeftArm];
            Handle_Arm(group);
        }


        void Handle_Arm(MuscleGroup group)
        {
            var length = (int)ArmDof.LastArmDof;
            LoadMuscle(group, length);
        }

        #endregion

        void LoadMuscle(MuscleGroup group, int muscleCount)
        {
            GUILayout.BeginVertical(style_Handle);
            EditorGUILayout.Space();

            for (int i = 0; i < muscleCount; i++)
            {
                var muscle = group[i];

                GUILayout.BeginHorizontal();
                GUILayout.Label(muscle.Name);

                if (muscle.m_Clamp)
                    muscle.m_Value = EditorGUILayout.Slider(muscle.m_Value, -1, 1, layout_Slider);
                else
                    muscle.m_Value = EditorGUILayout.Slider(muscle.m_Value, float.MinValue, float.MaxValue, layout_Slider);


                muscle.m_Clamp = EditorGUILayout.Toggle(muscle.m_Clamp, layout_Toggle);
                GUILayout.EndHorizontal();
                group[i] = muscle;
            }
            GUILayout.EndVertical();
        }

        #region Finger
        [BoxGroup(Layout_Handle_Group)]
        void Handle_Fingers(string side)
        {
            GUILayout.BeginVertical(style_Handle);
            GUILayout.Label(string.Format("{0} hand fingers", side));
            GUILayout.EndVertical();
        }
        [BoxGroup(Layout_Handle_Target)]
        void Handle_RightFingers()
        {
            var groups = FingerGroup(HumanPartDof.RightThumb);

            LoadFingers(groups);
        }
        [BoxGroup(Layout_Handle_Target)]
        void Handle_LeftFingers()
        {
            var groups = FingerGroup(HumanPartDof.LeftThumb);

            LoadFingers(groups);
        }

        MuscleGroup[] FingerGroup(HumanPartDof partDof)
        {
            if (Groups == null)
                return new MuscleGroup[0];
            var list = new List<MuscleGroup>(5);

            return new[]
            {
            Groups[partDof],
            Groups[partDof+1],
            Groups[partDof+2],
            Groups[partDof+3],
            Groups[partDof+4],
        };
        }

        float finger_downup;
        float finger_inout;
        void LoadFingers(params MuscleGroup[] groups)
        {
            var count = (int)FingerDof.LastFingerDof;

            var length = groups.Length;

            foreach (var group in groups)
            {
                LoadMuscle(group, count);
            }

            return;
            for (int c = 0; c < length; c++)
            {
                var group = groups[c];
                GUILayout.BeginVertical();
                for (int i = 0; i < count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(group.Datas[i].m_Handle.name);
                    group.Datas[i].m_Value = GUILayout.HorizontalSlider(group.Datas[i].m_Value, -1, 1);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }


            return;

            //float inout=default;
            //float downup = default;

            //for (int i = 0; i < count; i++)
            //{
            //    inout += groups[i].Datas[1].m_Value;
            //    downup += groups[i].Datas[0].m_Value;
            //}

            //inout /= count;
            //downup /= count;

            GUILayout.BeginVertical();

            finger_inout = GUILayout.HorizontalSlider(finger_inout, -1, 1, GUILayout.Width(70));
            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal();

            finger_downup = GUILayout.VerticalSlider(finger_downup, -1, 1, GUILayout.Height(40));
            EditorGUILayout.Space(10);

            for (int c = 0; c < length; c++)
            {
                var group = groups[c];

                group.Datas[0].m_Value += finger_downup;

                group.Datas[1].m_Value += finger_inout;

                GUILayout.BeginVertical();
                for (int i = count - 1; i >= 0; i--)
                {

                    switch ((FingerDof)i)
                    {
                        case FingerDof.ProximalDownUp:
                            group.Datas[c].m_Value = GUILayout.VerticalSlider(group.Datas[c].m_Value, -1, 1, GUILayout.Height(30));
                            break;

                        case FingerDof.ProximalInOut:
                            group.Datas[c].m_Value = GUILayout.HorizontalSlider(group.Datas[c].m_Value, -1, 1, GUILayout.Width(30));
                            break;

                        case FingerDof.IntermediateCloseOpen:
                            group.Datas[c].m_Value = GUILayout.VerticalSlider(group.Datas[c].m_Value, -1, 1, GUILayout.Height(30));
                            break;

                        case FingerDof.DistalCloseOpen:
                            group.Datas[c].m_Value = GUILayout.VerticalSlider(group.Datas[c].m_Value, -1, 1, GUILayout.Height(30));
                            break;
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }


        #endregion

        #region Leg
        [BoxGroup(Layout_Handle_Group)]
        void Handle_Legs()
        {
            GUILayout.BeginVertical(style_Handle);
            GUILayout.Label(Selector.PartDof.ToString());
            GUILayout.EndVertical();
        }
        [BoxGroup(Layout_Handle_Target)]
        void Handle_RightLeg()
        {
            var group = Groups[HumanPartDof.RightLeg];
            var length = (int)LegDof.LastLegDof;
            LoadMuscle(group, length);
        }
        [BoxGroup(Layout_Handle_Target)]
        void Handle_LeftLeg()
        {
            var group = Groups[HumanPartDof.LeftLeg];
            var length = (int)LegDof.LastLegDof;
            LoadMuscle(group, length);
        }

        #endregion

        void EditOption(string title)
        {
            var style = new GUIStyle(GUI.skin.toggle) { alignment = TextAnchor.MiddleCenter };

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(style_Handle);
            GUILayout.Label(title);
            GUILayout.EndVertical();

            SyncEdit = GUILayout.Toggle(SyncEdit, "SyncEdit", style);

            EditorGUI.BeginDisabledGroup(!ValidHandIK);
            IKOverride = GUILayout.Toggle(IKOverride, "EnableIK", style);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!IKOverride);
            MuscleClamp = GUILayout.Toggle(MuscleClamp, "ClampMuscle", style);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        #endregion

        #endregion






        #region Preview
        PlayableGraph mGraph;
        PlayableGraph GetGraph
        {
            get
            {
                if (!mGraph.IsValid())
                {
                    switch (m_PreviewType)
                    {
                        case PreviewType.InjectAnimatorGraph:
                            mGraph = Animator ? Animator.playableGraph : default;
                            break;
                        case PreviewType.CustomizeGraph:
                            mGraph = PlayableGraph.Create();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                return mGraph;
            }
        }

        AnimationScriptPlayable musclePlayable;
        AnimationClipPlayable _TPosePlayable;

        Playable TPosePlayable
        {
            get
            {
                if (!_TPosePlayable.IsValid())
                    _TPosePlayable = AnimationClipPlayable.Create(GetGraph, mEditorAnimationUtility.TPoseClip(Animator));


                return _TPosePlayable;
            }
        }

        HumanPose Pose;
        Vector3 HipPosition;
        Quaternion HipRotation;
        TransformStreamHandle HipStreamHandle;
        AnimationPlayableOutput output;

        [Button(nameof(Reset))]
        [HorizontalGroup("Control")]
        void Reset()
        {




            if (Playing)
            {
                Stop();
                PoseHandler.SetHumanPose(ref Pose);
                Play();
            }
            else
            {
                if (PoseHandler != null && Pose.muscles?.Length > 0)
                    PoseHandler.SetHumanPose(ref Pose);
            }

            Debug.Log(muscleDatas + " has been reset.");
        }

        void ResetCharacter()
        {
            if (m_Animator)
            {
                PoseHandler = new HumanPoseHandler(m_Animator.avatar, m_Animator.transform);

                if (Pose.muscles == null)
                {
                    PoseHandler.GetHumanPose(ref Pose);
                    Pose.bodyPosition = new Vector3(0, Pose.bodyPosition.y, 0);
                    Debug.Log(Pose.bodyPosition + "-" + Pose.bodyRotation.eulerAngles);
                    var muscles = Pose.muscles;
                    var length = muscles.Length;

                    if (muscleDatas?.Length == length)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            muscleDatas[i].m_Value = muscles[i];
                        }
                    }
                }
                else
                    Reset();


                var hips = m_Animator.GetBoneTransform(HumanBodyBones.Hips);
                HipPosition = hips.localPosition;
                HipRotation = hips.localRotation;
                HipStreamHandle = m_Animator.BindStreamTransform(hips);



                m_Animator.gameObject.InstanceIfNull(ref Wrapper);
                Wrapper.InjectPlayable = CreatePlayable;

            }
            else
            {
                if (Wrapper)
                    DestroyImmediate(Wrapper);
                if (PoseHandler != null)
                {
                    PoseHandler.SetHumanPose(ref Pose);
                    PoseHandler.Dispose();
                    PoseHandler = null;
                }

            }
        }

        void UpdateParameter()
        {
            if (musclePlayable.IsValid())
            {
                var job = musclePlayable.GetJobData<Job_Humanoid_MuscleRigging>();

                job.BodyPosition = HipPosition;
                job.BodyRotation = HipRotation;
                job.Root = HipStreamHandle;

                musclePlayable.SetJobData(job);

                if (!mGraph.IsValid() || !mGraph.IsPlaying())
                    GetGraph.Evaluate();
            }

            if (Wrapper.Override)
            {
                var job = new Job_Humanoid_MuscleRigging();
                job.Datas = muscleDatas;
                job.BodyPosition = HipPosition;
                job.BodyRotation = HipRotation;
                job.Root = HipStreamHandle;
                Wrapper.SetJob(job);
                Wrapper.job = job;
            }



        }

        bool Playing => (Wrapper ? Wrapper.Override : false) || mGraph.IsValid() && mGraph.IsPlaying();

        [HorizontalGroup("Control")]
        [Button(nameof(Play))]
        [HideIf(nameof(Playing))]
        [PropertyOrder(1f)]
        void Play()
        {
            if (Wrapper && Wrapper.Override)
            {
                Debug.Log(Wrapper.AnimationEditorGraph);
            }

            else
            {

                output = AnimationPlayableOutput.Create(GetGraph, "MuscleRiggingOutput", m_Animator);
                musclePlayable = CreatePlayable(mGraph);
                output.SetSourcePlayable(musclePlayable);

                mGraph.Play();
                mGraph.Evaluate();

            }

            //mGraph.SetResolver(this);



            UpdateParameter();

            ValidIK();

        }

        AnimationScriptPlayable CreatePlayable(PlayableGraph graph)
        {
            if (Wrapper)
            {
                Wrapper.Start -= EnableWrapper;
                Wrapper.Stop -= DisableWrapper;
                Wrapper.Start += EnableWrapper;
                Wrapper.Stop += DisableWrapper;
            }
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var job = new Job_Humanoid_MuscleRigging()
            {
                Root = HipStreamHandle,
                BodyPosition = HipPosition,
                BodyRotation = HipRotation,
                Datas = muscleDatas
            };
            return AnimationScriptPlayable.Create(graph, job, 1);
        }

        void EnableWrapper()
        {
            Debug.Log(nameof(EnableWrapper));
        }

        void DisableWrapper()
        {
            Debug.Log(nameof(DisableWrapper));
        }

        [PropertyOrder(0.9f)]
        [HorizontalGroup("Control")]
        [Button(nameof(Stop))]
        [ShowIf(nameof(Playing))]
        void Stop()
        {
            if (mGraph.IsValid())
            {
                if (mGraph.IsPlaying())
                    mGraph.Stop();
                if (!output.IsOutputNull())
                    if (output.IsOutputValid())
                        mGraph.DestroyOutput(output);

                if (m_Animator && !m_Animator.playableGraph.Equals(mGraph))
                    mGraph.Destroy();
            }
            else if (Wrapper && Wrapper.Override)
            {
                var graph = Wrapper.AnimationEditorGraph;
                if (graph.IsValid())
                    if (graph.IsPlaying())
                        graph.Stop();
            }

            if (musclePlayable.IsValid())
                if (musclePlayable.CanDestroy())
                    musclePlayable.Destroy();

            ValidIK();
            Reset();
        }

        [Button(nameof(VaildGraph))]
        void VaildGraph()
        {
            if (!m_Animator)
                return;

            Debug.Log(m_Animator.playableGraph.Equals(mGraph));
            Debug.Log("Vaild : " + mGraph.IsValid());
            Debug.Log("Play : " + mGraph.IsPlaying());
            Debug.Log("Count : " + mGraph.GetOutputCount());
            //Debug.Log(m_Animator.playableGraph.GetEditorName());
            //Debug.Log(m_Animator.playableGraph.GetRootPlayable(0).ToString());


        }

        #endregion






        [Button(nameof(TPose))]
        void TPose()
        {
            Animator.ForceTPose();
        }




        #region Record
        public float FrameTime;
        [OnValueChanged(nameof(PushBinding))]
        public AnimationClip Clip;
        Dictionary<string, EditorCurveBinding> Bindings;


        void PushBinding()
        {
            if (Clip)
            {
                var bindings = AnimationUtility.GetCurveBindings(Clip);
                Bindings = new Dictionary<string, EditorCurveBinding>(bindings.Length);
                foreach (var binding in bindings)
                {
                    if (Bindings.TryGetValue(binding.propertyName, out var curveBinding))
                    {
                        Debug.LogError(curveBinding.propertyName);
                    }
                    else
                    {
                        Bindings.Add(binding.propertyName, binding);
                    }
                }
            }
            else if (Bindings != null)
                Bindings.Clear();


        }


        [Button("KeyFrame")]
        public void KeyFrame()
        {
            if (Bindings == null)
                return;

            foreach (var muscle in muscleDatas)
            {
                var name = muscle.m_Handle.name;
                AnimationCurve curve;
                bool dirty = false;
                if (Bindings.TryGetValue(name, out var binding))
                {
                    curve = AnimationUtility.GetEditorCurve(Clip, binding);
                }
                else
                {
                    curve = new AnimationCurve();
                    binding = new EditorCurveBinding() { propertyName = name, type = typeof(Animator) };
                    if (Bindings.TryAdd(name, binding))
                        dirty = true;
                    else
                        Debug.LogError(name + binding.type);
                }

                if (curve.Evaluate(FrameTime) != muscle.m_Value)
                {
                    curve.AddKey(FrameTime, muscle.m_Value);
                    dirty = true;
                }
                if (dirty)
                    AnimationUtility.SetEditorCurve(Clip, binding, curve);
            }

            UnityEditor.EditorUtility.SetDirty(Clip);
        }

        [Button("KeyCurrentPart")]
        public void KeyCurrentPart()
        {
            if (Bindings == null)
                return;

            var dof = GetPartDof;
            if ((int)dof < 6)
            {
                var group = Groups[dof];
                PushGroupCurve(group);

            }
            else
            {
                var groups = FingerGroup(dof);
                foreach (var group in groups)
                {
                    PushGroupCurve(group);
                }
            }

            UnityEditor.EditorUtility.SetDirty(Clip);
        }

        HumanPartDof GetPartDof
        {
            get
            {
                return Selector.PartDof.ToHumanDof();
            }
        }



        void PushGroupCurve(MuscleGroup muscles)
        {
            var length = muscles.Count;
            for (int i = 0; i < length; i++)
            {
                var muscle = muscles[i];

                var name = muscle.m_Handle.name;
                AnimationCurve curve = null;
                bool dirty = false;
                if (Bindings.TryGetValue(name, out var binding))
                {
                    curve = AnimationUtility.GetEditorCurve(Clip, binding);
                }

                if (curve == null)
                {
                    curve = new AnimationCurve();
                    binding = new EditorCurveBinding() { propertyName = name, type = typeof(Animator) };
                    if (Bindings.TryAdd(name, binding))
                        dirty = true;
                    else
                        Debug.LogError(name + binding.type);
                }

                if (curve.Evaluate(FrameTime) != muscle.m_Value)
                {
                    if (curve.AddKey(FrameTime, muscle.m_Value) < 0)
                    {

                        var keys = curve.keys;
                        var count = keys.Length;
                        for (int f = 0; f < count; f++)
                        {
                            if (!keys[f].time.Equals(FrameTime))
                                continue;

                            keys[f] = new Keyframe(FrameTime, muscle.m_Value);
                            curve.keys = keys;
                            break;
                        }
                    }
                    dirty = true;
                }
                if (dirty)
                    AnimationUtility.SetEditorCurve(Clip, binding, curve);
            }
        }


        #endregion












        private void Awake()
        {
            Selector = CreateInstance<AvatarSelector>();
            Enum.TryParse(EditorPrefs.GetString(PreviewPref), out m_PreviewType);


        }


        protected override void OnEnable()
        {
            if (!m_Animator)
                mEditorAnimationUtility.SelectedAnimator(out m_Animator);

            muscleDatas = MuscleData.CreateAllMuscleDatas;
            var groups = new Dictionary<HumanPartDof, MuscleGroup>((int)HumanPartDof.LastHumanPartDof);
            var length = muscleDatas.Length;
            for (int i = 0; i < length; i++)
            {
                var handle = muscleDatas[i].m_Handle;
                var partDof = handle.humanPartDof;
                var dof = string.Empty;

                if (!groups.TryGetValue(partDof, out var group))
                {
                    group = new MuscleGroup(partDof, muscleDatas);
                    groups.Add(partDof, group);
                    group.UpdateMuscle += UpdateParameter;
                }

                group.KeyVolume(i, muscleDatas[i]);
            }

            Groups = groups;
            InitIK();

            ResetCharacter();

        }


        protected override void OnGUI()
        {
            DisableFunction = !VaildAnimator;

            base.OnGUI();

        }

        protected override void OnDestroy()
        {
            if (HandIK)
                DestroyImmediate(HandIK.gameObject);

            if (Wrapper)
                DestroyImmediate(Wrapper);
            Stop();
            DestroyImmediate(Selector);
            base.OnDestroy();
        }


    }
}