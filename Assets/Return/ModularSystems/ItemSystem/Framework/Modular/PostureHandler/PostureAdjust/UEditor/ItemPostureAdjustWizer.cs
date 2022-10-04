#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Return.Items;
using Return.Animations.IK;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;
using System.Linq;
using Return.Animations;

namespace Return.Editors
{
    //[CustomPropertyDrawer(typeof(ItemHandPoseAdjustData))]

    public class ItemPostureAdjustWizer : OdinEditorWindow
    {
        protected override void Initialize()
        {
            Debug.Log(nameof(Initialize));
        }
        
        


        #region Editor
        [MenuItem("Tools/Return/Item/PostureAdjustWizer")]
        static void CreateWindow()
        {
            Open();
        }

        public static ItemPostureAdjustWizer Open()
        {
            var window= GetWindow<ItemPostureAdjustWizer>("ItemPostureAdjustWizer");
            
            //SceneView.duringSceneGui -= window.OnSceneGUI;
            //SceneView.duringSceneGui += window.OnSceneGUI;

            return window;
        }


        #endregion

        Color color_Valid=Color.white;
        Color color_Change => Color.red;
        Color color_Normal => Color.white;
        GUIStyle ButtonStyle;
        GUIContent[] m_FingerLabels = null;
        GUIStyle m_FoldoutStyle = null;


        ItemPostureAdjustData Posture;
        ItemHandPostureConfig data => Posture==null?null: Posture.AdjustData;
        bool valid;

        #region Preview

        Animator animator;

        Transform RightHand;
        Transform LeftHand;
        

        AnimationClip Clip;
        EditorAnimPlayer Player;

        [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        m_IKAdjustJob Job;
        AnimationScriptPlayable Playable;

        //TwoBoneIKAnimationJob IK_Right;
        //TwoBoneIKAnimationJob IK_Left;

        //AnimationScriptPlayable Playable_RightHand;
        //AnimationScriptPlayable Playable_LeftHand;

        #endregion

        Transform ItemHandle;
        Transform Item;
        //Vector3 ItemPosition;
        //Vector3 ItemRotation;

        #region Fingers
        const int FingerStartIndex = 24;

        static Vector3 m_ClipBoard = Vector3.zero;

        bool m_OffsetLeftFingers = true;
        bool m_OffsetRightFingers = true;

        #region FingerCatch
        HashSet<int> RotationHasChange = new();
     
        #endregion

        Vector3[] FingerRotations = new Vector3[30];
        #endregion

        [BoxGroup("PostureData",ShowLabel =false)]
        [PropertyOrder(10)]
        [OnInspectorGUI]
        void Draw()
        {
            #region Data

            var adjustData = EditorGUILayout.ObjectField(Posture, typeof(ItemPostureAdjustData), false);

            if (!valid &&Posture!=adjustData &&adjustData is ItemPostureAdjustData postureData)
                LoadData(postureData);

            #endregion

            ButtonStyle = new GUIStyle(GUI.skin.button);

            if (!EditorGUIUtility.wideMode)
                EditorGUIUtility.wideMode = true;

            InitialiseLabels();

            if (RotationHasChange.Count > 0)
                ButtonStyle.normal.textColor = color_Change;
            else
                ButtonStyle.normal.textColor = color_Normal;

            DrawTools();
        }


        protected virtual void DrawTools()
        {
            if(valid)
                if (GUILayout.Button("Save", ButtonStyle))
                    Save();
        }


        [TabGroup("LeftHand")]
        [OnInspectorGUI, PropertyOrder(30)]
        void DrawLeftHand()
        {
            if(valid)
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawHandleTarget(LeftHand);
                data.UseLeftHand = EditorGUILayout.ToggleLeft(new GUIContent("AdjustLeftHand", "Adjust left hand handle."), data.UseLeftHand);
                if (data.UseLeftHand)
                    DrawHandleModule(ref data.LeftHand,LeftHand, nameof(data.LeftHand));
                DrawLeftFingers();
            }
        }


        [TabGroup("RightHand")]
        [OnInspectorGUI, PropertyOrder(40)]
        void DrawRightHand()
        {
            if (valid)
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawHandleTarget(RightHand);
                data.UseRightHand = EditorGUILayout.ToggleLeft(new GUIContent("AdjustRightHand", "Adjust right hand handle."), data.UseRightHand);
                if (data.UseRightHand)
                    DrawHandleModule(ref data.RightHand,RightHand, nameof(data.RightHand));
                DrawRightFingers();
            }
        }

        #region Database

        public void LoadData(ItemPostureAdjustData posture)
        {
            valid = posture;
            color_Valid = valid ? Color.white : Color.red;
            Posture = posture;
            LoadData();
        }

        void LoadData()
        {
            if (!valid)
                return;

            //ItemPosition = data.m_items.OffsetPosition;
            //ItemRotation = data.m_items.OffsetRotation;

            const int fingerCount = 30;

            RotationHasChange = new(fingerCount);

            #region Hand
            {


            }
            #endregion

            #region Fingers
            {
                FingerRotations = new Vector3[fingerCount];
                var dic = data.GetFingerOffsetCatch;

                const int start = 24;



                for (int i = 0; i < fingerCount; ++i)
                {
                    var bone = (HumanBodyBones)i + start;

                    Assert.IsTrue(dic.ContainsKey(bone));

                    var o = dic[bone];

                    if (o != default)
                    {
                        if (i < 15&&!m_OffsetLeftFingers)
                            m_OffsetLeftFingers = true;
                        else if(!m_OffsetRightFingers)
                            m_OffsetRightFingers = true;
                    }

                    var rotation = o.WrapAngle();

                    FingerRotations[i] = rotation;
                }
            }
            #endregion

        }

        void LoadItem(Transform tf)
        {
            if (!tf.TryGetComponent<AbstractItem>(out var item))
            {
                item=tf.GetComponentInChildren<AbstractItem>();
                if (item)
                    tf = item.transform;
                else
                {
                    Debug.LogError(new KeyNotFoundException(nameof(Item)));
                    return;
                }
            }


            tf.SetLocalPR(data.Item);
            //var offset = tf.GetLocalPR();
            //data.m_items.OffsetPosition = offset;
            //data.m_items.OffsetRotation = offset;
            //ItemPosition = offset;
            //ItemRotation = offset.eulerAngles;

            animator = tf.GetComponentInParent<Animator>();


            Item = tf;
            ItemHandle = tf.parent;

            RightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            LeftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

            var IK_Right = TwoBoneIKAnimationJob.Create(animator, RightHand);
            IK_Right.Position = data.RightHand.OffsetPosition;
            IK_Right.Rotation = data.RightHand.OffsetRotation.ToEuler();

            var IK_Left = TwoBoneIKAnimationJob.Create(animator, LeftHand);
            IK_Left.Position = data.LeftHand.OffsetPosition;
            IK_Left.Rotation = data.LeftHand.OffsetRotation.ToEuler();

            LoadFingersJob();

            //FingerJob = new NativeArray<RotationAdjustJob>(handles.ToArray(), Allocator.Persistent);


            Job = new()
            {
                IK_Left = IK_Left,
                IK_Right=IK_Right,
                Fingers= FingerJob,
            };

            Debug.Log(IK_Left);
        }

        void LoadFingersJob()
        {
            //var handles = data.FingersData.Select(width =>
            //new RotationAdjustJob()
            //{
            //    Handle = anim.GetStreamHandle(width.Bone),
            //    LocalRotation = width.Rotation.ToEuler()
            //}); 

   

            var i = FingerStartIndex;
            var handles = FingerRotations.Select(x =>
            new RotationAdjustJob()
            {
                Handle = animator.GetStreamHandle((HumanBodyBones)i++),
                LocalRotation = x.ToEuler()
            });

            FingerJob = handles.ToArray();
            Job.Fingers = FingerJob;
        }

        //NativeArray<RotationAdjustJob> FingerJob;
        RotationAdjustJob[] FingerJob;
        void Save()
        {
            var startIndex = 24;

            var length = 30;

            List<PostureRotationData> newConfigs = new(length);

            Debug.Log("BindLeftFingers : " + m_OffsetLeftFingers);
            Debug.Log("BindRightFingers : " + m_OffsetRightFingers);

            for (int i = 0; i < length; i++)
            {
                if (i < 15 && !m_OffsetLeftFingers)
                    continue;
                else if (i>14 && !m_OffsetRightFingers)
                    continue;

                var rot=FingerRotations[i];

                var additive = i < 15 ? data.isLeftFingerAdditive : data.isRightFingerAdditive;

                if (additive && rot == default)
                    continue;

                var posture = new PostureRotationData()
                {
                    Bone = (HumanBodyBones)(i + startIndex),
                    Rotation=rot,
                };

                newConfigs.Add(posture);
            }
            Debug.Log(newConfigs.Count);
            data.FingersData = newConfigs;

            RotationHasChange.Clear();

            if (Playable.IsQualify())
            {
                LoadFingersJob();
                UpdateJob();
            }
        }

        #endregion


        #region Item

        const string field_Item = "m_items";

        [BoxGroup(field_Item, ShowLabel = false)]
        [OnInspectorGUI, PropertyOrder(20)]
        void DrawItemHandle()
        {
            if (!valid)
                return;

            if (animator)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(animator, typeof(Animator), true);
                EditorGUI.EndDisabledGroup();

                var clip = EditorGUILayout.ObjectField(Clip, typeof(AnimationClip), false);
                if (clip != Clip && clip is AnimationClip newClip)
                {
                    Clip = newClip;
                    animator.gameObject.InstanceIfNull(ref Player);
                    Player.Play(Clip);

                    var graph = Player.GetGraph;

                    //Playable_LeftHand = AnimationScriptPlayable.Create(graph, IK_Left);
                    //Playable_RightHand = AnimationScriptPlayable.Create(graph, IK_Right);
                    Playable = AnimationScriptPlayable.Create(graph, Job);
                    Player.Insert(Playable);
                    //Player.Insert(Playable_RightHand);
                    //Player.Insert(Playable_LeftHand);
                }
            }

            var zoneRect = EditorGUILayout.GetControlRect(false, 50f, GUILayout.ExpandWidth(true));

            var style = new GUIStyle("CurveEditorBackground");
            style.normal.textColor = Color.green;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 25;

            EditorGUI.LabelField(zoneRect, "Release m_items ReadOnlyTransform Here", style);
            var value = DragAndDropUtilities.DropZone<Transform>(zoneRect, null, true);

            if (value)
                LoadItem(value);

            //ItemPosition = EditorGUILayout.Vector3Field("m_items Position", ItemPosition);
            //ItemRotation = EditorGUILayout.Vector3Field("m_items Rotation", ItemRotation).WrapAngle();

            var itemConfig = data.Item;

            //itemConfig.OffsetPosition = ItemPosition;
            //itemConfig.OffsetRotation = ItemPosition;
            itemConfig.isAdditive = EditorGUILayout.ToggleLeft(nameof(itemConfig.isAdditive), itemConfig.isAdditive);

            //data.m_items = itemConfig;
        }

        [BoxGroup(field_Item)]
        [OnInspectorGUI, PropertyOrder(25)]
        void DrawItem()
        {
            if (!valid)
                return;
                if (!EditorGUIUtility.wideMode)
                EditorGUIUtility.wideMode = true;
            DrawHandleTarget(ItemHandle);

            DrawHandleModule(ref data.Item,ItemHandle, nameof(data.Item));

            Transform handle=null;


            switch (BindingHandle)
            {
                case Symmetry.Right:
                    if(RightHand)
                        handle = RightHand.Find("RightHandHandle");

                    break;
                case Symmetry.Left:
                    if (LeftHand)
                        handle = LeftHand.Find("LeftHandHandle");
                    break;
            }

            if (ItemHandle&&handle)
                ItemHandle.Copy(handle);

            if (Item)
                Item.SetLocalPR(data.Item);

            //ItemPosition = data.m_items.OffsetPosition;
            //ItemRotation = data.m_items.OffsetRotation;
        }

        [OnValueChanged(nameof(AlignItemHandle))]
        [BoxGroup(field_Item), PropertyOrder(23)]
        [HorizontalGroup("m_items/Slot")]
        public Symmetry BindingHandle = Symmetry.Right;

        [HorizontalGroup("m_items/Slot")]
        [Button]
        void AlignItemHandle()
        {
            if (!animator)
                return;

            Transform bone;
            switch (BindingHandle)
            {
                case Symmetry.Right:
                    bone = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    break;

                case Symmetry.Left:
                    bone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    break;

                default:
                    BindingHandle = Symmetry.Right;
                    goto case Symmetry.Right;
            }

            ItemHandle.Copy(bone);
        }

        [BoxGroup(field_Item), PropertyOrder(24)]
        [Range(0, 1f)]
        [OnValueChanged(nameof(SetTime))]
        [GUIColor(nameof(color_Valid))]
        public float Time;

        void SetTime()
        {
            if (valid && Player)
            {
                Player.SetTime(Time);
                Player.isPause = true;
                UpdateHandle();
            }
        }

        #endregion

        public bool Preview=true;

        

        [ShowIf(nameof(Preview))]
        [OnInspectorGUI]
        void UpdateHandle()
        {
            if (!valid)
                return;

            UpdateJob();


            //if(!Playable_RightHand.IsNull()&& Playable_RightHand.IsValid())
            //{
            //    IK_Right.Position = data.RightHand.OffsetPosition;
            //    IK_Right.Rotation = Quaternion.Euler(data.RightHand.OffsetRotation);
            //    Playable_RightHand.SetJobData(IK_Right);
            //}

            //if (!Playable_LeftHand.IsNull() && Playable_LeftHand.IsValid())
            //{
            //    IK_Left.Position = data.LeftHand.OffsetPosition;
            //    IK_Left.Rotation = Quaternion.Euler(data.LeftHand.OffsetRotation);
            //    Playable_LeftHand.SetJobData(IK_Left);
            //}
        }

        void UpdateJob()
        {
            if (!Playable.IsQualify())
                return;

            var rIK = Job.IK_Right;
            rIK.Position = data.RightHand.OffsetPosition;
            rIK.Rotation = data.RightHand.OffsetRotation.ToEuler();
            Job.IK_Right = rIK;

            var lIK = Job.IK_Left;
            lIK.Position = data.LeftHand.OffsetPosition;
            lIK.Rotation = data.LeftHand.OffsetRotation.ToEuler();
            Job.IK_Left = lIK;

            var length = Job.Fingers.Length;

            var leftAdditive = data.isLeftFingerAdditive;
            var rightAdditive = data.isRightFingerAdditive;

            for (int i = 0; i < length; i++)
            {
                var finger=Job.Fingers[i];

                if (i < 15)
                    finger.isAdditive = leftAdditive;
                else
                    finger.isAdditive = rightAdditive;

                Job.Fingers[i] = finger;
            }

            Playable.SetJobData(Job);
        }

        static PR CalculateAdditive(Transform tf,Vector3 pos,Quaternion rot)
        {
            pos = tf.position + tf.rotation * pos;
            rot = (rot * tf.rotation)*tf.rotation;
            return new(pos, rot);
        }

        #region Handle

        public static void DrawHandle(HandleConfig config, Transform handle, string handleName)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var adjustHandle = EditorGUILayout.ObjectField(config.AdjustMode.GetType().Name, config.AdjustMode, typeof(PostureAdjustHandle), false);
                
                if (adjustHandle != config.AdjustMode)
                    adjustHandle.Parse(out config.AdjustMode);

                config.OffsetPosition =EditorGUILayout.Vector3Field(handleName + "PositionOffset", config.OffsetPosition);
                config.OffsetRotation = EditorGUILayout.Vector3Field(handleName + "RotationOffset", config.OffsetRotation);
                //config.OffsetRotation = EditorGUILayout.Vector3Field(handleName + "RotationOffset", config.OffsetRotation);
            }
        }

        /// <summary>
        /// Draw handle data.
        /// </summary>
        void DrawHandleModule(ref HandleConfig config,Transform handle, string handleName, MessageType messageType = MessageType.Warning)
        {
            if (config && config.AdjustMode)
            {
                config.AdjustMode.DrawPostureWizer(this, config,handle, handleName);
            }
            else if (config)
            {
                EditorGUILayout.HelpBox(string.Format("Missing {0} adjust module.", handleName), messageType, true);

                var newAdjustHandle = EditorGUILayout.ObjectField("Adjust Modlue", null, typeof(PostureAdjustHandle), false);

                if (newAdjustHandle)
                    config.AdjustMode = newAdjustHandle as PostureAdjustHandle;
            }
            else
            {
                EditorGUILayout.HelpBox(string.Format("Missing {0}.", handleName), MessageType.Error, true);
            }
        }

        /// <summary>
        /// Draw object field.
        /// </summary>
        void DrawHandleTarget(Transform handle)
        {
            if (!handle)
                return;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Handle", handle, typeof(UnityEngine.Object), true);
            EditorGUI.EndDisabledGroup();
        }

        #endregion

        #region Fingers

        void InitialiseLabels()
        {
            if (m_FoldoutStyle == null)
            {
                m_FoldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold
                };
            }

            if (m_FingerLabels == null)
            {
                m_FingerLabels = new GUIContent[] {
                    new GUIContent("Thumb 1 (Proximal)"),
                    new GUIContent("Thumb 2 (Intermediate)"),
                    new GUIContent("Thumb 3 (Distal)"),
                    new GUIContent("Index 1 (Proximal)"),
                    new GUIContent("Index 2 (Intermediate)"),
                    new GUIContent("Index 3 (Distal)"),
                    new GUIContent("Middle 1 (Proximal)"),
                    new GUIContent("Middle 2 (Intermediate)"),
                    new GUIContent("Middle 3 (Distal)"),
                    new GUIContent("Ring 1 (Proximal)"),
                    new GUIContent("Ring 2 (Intermediate)"),
                    new GUIContent("Ring 3 (Distal)"),
                    new GUIContent("Pinky 1 (Proximal)"),
                    new GUIContent("Pinky 2 (Intermediate)"),
                    new GUIContent("Pinky 3 (Distal)")
                };
            }
        }

        void DrawLeftFingers()
        {
            m_OffsetLeftFingers = EditorGUILayout.ToggleLeft("Control Finger", m_OffsetLeftFingers);

            if (m_OffsetLeftFingers)
            {
                SetFingerAdditive(ref data.isLeftFingerAdditive);

                if (GUILayout.Button("ResetFingers"))
                {
                    var length = 15;
                    for (int i = 0; i < length; i++)
                        ResetFinger(i);
                }

                bool update = false;

                for (int i = 0; i < 15; ++i)
                {
                    var o = FingerRotations[i];
                    var rotation = o;

                    rotation = InspectFingerOffset(rotation, m_FingerLabels[i], i);

                    if (o == rotation)
                        continue;

                    RotationHasChange.Add(i);
                    rotation= rotation.WrapAngle();
                    FingerRotations[i] = rotation;

                    if (Playable.IsQualify())
                    {
                        var job = Job.Fingers[i];
                        job.LocalRotation = rotation.ToEuler();
                        Job.Fingers[i] = job;
                        update = true;
                    }
                }
                if(update)
                    UpdateJob();
            }
        }

        void SetFingerAdditive(ref bool isAdditive)
        {
            var newAdditive = EditorGUILayout.ToggleLeft("isAdditive", isAdditive);
            if (newAdditive != isAdditive)
            {
                isAdditive = newAdditive;
                UpdateJob();
            }
        }

        void DrawRightFingers()
        {
            m_OffsetRightFingers = EditorGUILayout.ToggleLeft("Control Finger", m_OffsetRightFingers);

            if (m_OffsetRightFingers)
            {
                SetFingerAdditive( ref data.isRightFingerAdditive);


                if (GUILayout.Button("ResetFingers"))
                {
                    var length = 30;
                    for (int i = 15; i < length; i++)
                        ResetFinger(i);
                }

                bool update = false;

                for (int i = 0; i < 15; ++i)
                {
                    var o = FingerRotations[i + 15];
                    var rot = InspectFingerOffset(o, m_FingerLabels[i], i + 15);

                    if (o == rot)
                        continue;

                    RotationHasChange.Add(i + 15);
                    rot = rot.WrapAngle();
                    FingerRotations[i + 15] = rot;

                    if (Playable.IsQualify())
                    {
                        var job = Job.Fingers[i + 15];
                        job.LocalRotation = rot.ToEuler();
                        Job.Fingers[i + 15] = job;
                        update = true;
                    }
                }

                if(update)
                    UpdateJob();
            }
        }
       
        void ResetFinger(int i)
        {
            if (FingerRotations[i]!=default)
            {
                FingerRotations[i] = default;
                RotationHasChange.Add(i);
            }    
        }

        Vector3 InspectFingerOffset(Vector3 prop, GUIContent label, int index)
        {
            //EditorGUILayout.BeginHorizontal();

            var hasChange = RotationHasChange.Contains(index);
            var color = hasChange ? color_Change : color_Normal;

            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
            rect.width -= 2f * EditorGUIUtility.singleLineHeight + 4f;

            var guiColor = GUI.color;
            if (hasChange)
                GUI.color = color;

            // Property field
            prop = EditorGUI.Vector3Field(rect, label, prop);

            if (hasChange)
                GUI.color = guiColor;


            rect.x += rect.width + 2f;
            rect.width = EditorGUIUtility.singleLineHeight;

            var icon = new GUIContent(EditorGUIUtility.FindTexture("SceneLoadIn")) { tooltip = "Copy" };

            //var lines = GUILayout.ExpandWidth(false);

            // Copy button
            //if (GUILayout.Button( icon, EditorStyles.label, lines))
            if (GUI.Button(rect, icon, EditorStyles.label))
            {
                m_ClipBoard = prop;
            }

            rect.x += EditorGUIUtility.singleLineHeight + 2f;


            icon = new GUIContent(EditorGUIUtility.FindTexture("SceneLoadOut")) { tooltip = "Paste" };
            // Paste button
            //if (GUILayout.Button( icon, EditorStyles.label, lines))
            if (GUI.Button(rect, icon, EditorStyles.label))
            {
                prop = m_ClipBoard;
            }
            //EditorGUILayout.EndHorizontal();
            return prop;
        }

        #endregion

        #region Handles

        static Quaternion CatchGlobalRot=Quaternion.identity;



        /// <summary>
        /// Draw handle gizmos.
        /// </summary>
        bool DrawHandle(Transform handle,HandleConfig config,Vector3 streamPos,Quaternion streamRot)
        {
            var curPos = handle.position;
            var curRot = handle.rotation;

            //UnityHandles.xAxisColor;

            bool updateJob = false;


            var color = new Color(1, 0.8f, 0.4f, 1);
            Handles.color = color;


            GUI.color = color;
            Handles.Label(curPos, handle.ToString());


            var global = Tools.pivotRotation == PivotRotation.Global;

            switch (Tools.current)
            {
                case Tool.Move:

                    if(global)
                        curRot = Quaternion.identity;

                    var doPos = Handles.PositionHandle(curPos, curRot);

                    if (doPos != curPos)
                    {
                        var gap = doPos - streamPos;
                        gap = Quaternion.Inverse(streamRot) * gap;
                        config.OffsetPosition = gap;
                        updateJob = true;
                    }
                    break;

                case Tool.Rotate:

                    //if (EditorGUI.EndChangeCheck())
                    {
                        //if (doRot != drawRot)
                        {
                            if (global)
                            {
                                //EditorGUI.BeginChangeCheck();

                                var drawRot = Quaternion.identity;
                                var doRot = Handles.DoRotationHandle(drawRot, curPos);

               

                                if (doRot != drawRot)
                                {
                                    handle.rotation = doRot * Quaternion.Inverse(CatchGlobalRot) * handle.rotation;

                                    CatchGlobalRot = doRot;

                                    var gap = handle.rotation * Quaternion.Inverse(curRot) * config.OffsetRotation.ToEuler();

                                    config.OffsetRotation = gap.eulerAngles.WrapAngle();

                                    updateJob = true;
                                }

                                //if (EditorGUI.EndChangeCheck())
                                //    CatchGlobalRot = Quaternion.identity;

                                //doRot *= Quaternion.Inverse(Tools.handleRotation);

                                //var gap = (doRot * curRot);

                                //gap = gap * Quaternion.Inverse(curRot) * config.OffsetRotation.ToEuler();
                                //Debug.Log(gap.eulerAngles);
                                //config.OffsetRotation = gap.eulerAngles.WrapAngle();

                            }
                            else
                            {
                                var drawRot = global ? Quaternion.identity : curRot;
                                var doRot = Handles.DoRotationHandle(drawRot, curPos);

                                if (doRot != drawRot)
                                {
                                    var gap = doRot * Quaternion.Inverse(streamRot);
                                    config.OffsetRotation = gap.eulerAngles;
                                    updateJob = true;
                                }
                            }
                        }
                    }
                    ReleaseRotationHandle();

                    break;

                default:
                    return false;
            }

            return updateJob;
        }

        static void ReleaseRotationHandle()
        {
            if (Event.current.OnMouseUp(0))
            {
                //Debug.Log("Mouse Up");
                CatchGlobalRot = Quaternion.identity;
                Event.current.Use();
            }
        }

        public void OnSceneGUI(SceneView view)
        {
            if (Item.IsNull())
                return;

            if (!ItemHandle)
                return;



            bool updateJob=false;

            if (Item)
            {
                var handle = Item;

                var curPos = handle.position;
                var curRot = handle.rotation;

                var color = new Color(1, 0.8f, 0.4f, 1);
                Handles.color = color;

                var global = Tools.pivotRotation == PivotRotation.Global;

                switch (Tools.current)
                {
                    case Tool.Move:

                        if (global)
                            curRot = Quaternion.identity;

                        var doPos = Handles.PositionHandle(curPos, curRot);
                        if (doPos != curPos)
                        {
                            data.Item.OffsetPosition = ItemHandle.InverseTransformPoint(doPos);
                            updateJob = true;
                        }
                        break;

                    case Tool.Rotate:

                        var drawRot = global ? Quaternion.identity : curRot;
                        var doRot = Handles.DoRotationHandle(drawRot, curPos);

                        if (doRot != drawRot)
                        {
                            if (global)
                            {
                                //doRot *= Quaternion.Inverse(Tools.handleRotation);
                                //var gap = (doRot * curRot);
                                //gap = gap * Quaternion.Inverse(curRot) * config.OffsetRotation.ToEuler();
                                data.Item.OffsetRotation = ItemHandle.InverseTransformRotation(doRot * curRot).eulerAngles;
                            }
                            else
                            {
                                data.Item.OffsetRotation = ItemHandle.InverseTransformRotation(doRot * Quaternion.Inverse(curRot) * curRot).eulerAngles;

                                //var gap = doRot * Quaternion.Inverse(streamRot);
                                //data.m_items.OffsetRotation = gap.eulerAngles;

                            }
                            updateJob = true;
                            ReleaseRotationHandle();
                        }


                        break;
                }

                Item.SetLocalPR(data.Item);
                GUI.color = color;
                Handles.Label(curPos, handle.ToString());
            }


            if (Playable.IsQualify())
                Job = Playable.GetJobData<m_IKAdjustJob>();
            else
                return;

            updateJob |= DrawHandle(RightHand,data.RightHand,Job.RightHand.Position,Job.RightHand.Rotation);
            updateJob |= DrawHandle(LeftHand, data.LeftHand, Job.LeftHand.Position, Job.LeftHand.Rotation);

            if (updateJob)
                UpdateJob();
        }


        protected override void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            //if (FingerJob.IsCreated)
            //    FingerJob.DisposePlayingPerformer();
        }

        protected override void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }


        #endregion
    }



    public struct RotationAdjustJob
    {
        public TransformStreamHandle Handle;
        public Quaternion LocalRotation;

        public bool isAdditive;

        public void ProcessAnimation(AnimationStream stream)
        {
            if (isAdditive)
                SetHandleAdditive(stream);
            else
                SetHandle(stream);
        }


        public void SetHandle(AnimationStream stream)
        {
            if (Handle.IsValid(stream))
                Handle.SetLocalRotation(stream, LocalRotation);
        }

        public void SetHandleAdditive(AnimationStream stream)
        {
            if (!Handle.IsValid(stream))
                return;

            var rot = LocalRotation * Handle.GetLocalRotation(stream);
            Handle.SetLocalRotation(stream, rot);
        }

        //public NativeArray<TransformStreamHandle> UnityHandles;
        //public NativeArray<Quaternion> LocalRotation;

        //public void ProcessAnimation(AnimationStream stream)
        //{
        //    Assert.IsTrue(UnityHandles.IsCreated);
        //    Assert.IsTrue(LocalRotation.IsCreated);

        //    var length = UnityHandles.Length;
        //    for (int i = 0; i < length; i++)
        //    {
        //        if (UnityHandles[i].IsValid(stream))
        //            UnityHandles[i].SetLocalRotation(stream, LocalRotation[i]);
        //    }
        //}
    }

    public struct m_IKAdjustJob : IAnimationJob
    {
        public PR RightHand;
        public PR LeftHand;

        public TwoBoneIKAnimationJob IK_Right;
        public TwoBoneIKAnimationJob IK_Left;
        //public NativeArray<RotationAdjustJob> Fingers;
        public RotationAdjustJob[] Fingers;

    

        public void ProcessAnimation(AnimationStream stream)
        {

            if (IK_Right.End.IsValid(stream))
            {
                IK_Right.End.GetGlobalTR(stream, out var r_pos, out var r_rot);
                RightHand = new(r_pos, r_rot);
                IK_Right.ProcessAnimation(stream);
            }

            if (IK_Left.End.IsValid(stream))
            {
                IK_Left.End.GetGlobalTR(stream, out var l_pos, out var l_rot);
                LeftHand = new(l_pos, l_rot); 
                IK_Left.ProcessAnimation(stream);
            }


            foreach (var finger in Fingers)
            {
                finger.ProcessAnimation(stream);
                //if (isAdditive)
                //    finger.SetHandleAdditive(stream);
                //else
                //    finger.SetHandle(stream);
            }
            

            //Debug.Log(IK_Right.Rotation.eulerAngles);
        }

        public void ProcessRootMotion(AnimationStream stream)
        {

        }
    }
}

#endif