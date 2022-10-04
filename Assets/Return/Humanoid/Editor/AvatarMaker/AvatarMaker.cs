using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Return.Humanoid;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.Animations.Rigging;
using System;
using UnityEngine.Events;
using Return;

public class AvatarMaker : OdinEditorWindow
{
    public AvatarMaker()
    {
        this.titleContent = new GUIContent("AvatarMaker");

        SetBoneNameFunc = (x) => { return x.ToString(); };
    }

    [MenuItem("Tools/Animation/Avatar/AvatarMaker")]
    public static void OpenWindow()
    {
        var window = CreateWindow<AvatarMaker>();
        window.Show();

        if(EditorUtilityTools.GetSelectedComponent(out Animator animator))
        {
            window.CharacterTransform = animator.transform;
            window.ScanAvatar();
        }

    }

    const string Key_DataPath = "AvatarMakerDataPath";


    [ShowInInspector]
    [FolderPath]
    public string DataPath
    {
        get => EditorPrefs.GetString(Key_DataPath, "Assets/Return");
        set => EditorPrefs.SetString(Key_DataPath, value);
    }

    public const string Layout_Tab = "BuildFunction";



    [SerializeField]
    [HideInInspector]
    UnityEvent<GameObject> OnCullingObjects;

    Vector2 Scroll;
    string KeyWord;

    m_HumanoidDescription mDescription;



    [TabGroup(Layout_Tab, "DrawAvatarCerator")]
    [OnInspectorGUI]
    void DrawAvatarCerator()
    {
        DrawRootPort();

        if (!CharacterTransform)
            return;

        var tfs = CharacterTransform.GetComponentsInChildren<Transform>();

        // Draw custom binding option
        DrawManualBindingBone(tfs);

        if (GUILayout.Button("Log SkinnedMeshRendere Bone", EditorHelper.MiddleLittleButton(1)))
            ScanSkinnedMeshRenderer(CharacterTransform.gameObject);

        if (GUILayout.Button("Scan bone via avatar", EditorHelper.MiddleLittleButton(1)))
            ScanAvatar();

        if (GUILayout.Button("Scan bone via name of transforms", EditorHelper.MiddleLittleButton(1)))
            ScanBone();

        bool ready2Build = CharacterTransform != null & SkeletonMap.IsReady;


        if (ready2Build)
            DrawNormalized();



        EditorGUI.BeginDisabledGroup(!ready2Build);

        if (GUILayout.Button("GenerateAvatar"))
        {
            {
                mDescription = m_HumanoidDescription.Create(SkeletonMap.BoneTransform);

                var avatar = mDescription.CreateAvatar(CharacterTransform, SkeletonMap);
                EditorHelper.WriteFile(avatar, avatar.GetType(), EditorHelper.InvalidFolderOpenPenel(DataPath) + '/' + CharacterTransform.name + "_Create via SkeletonMap");
            }

            {
                mDescription = m_HumanoidDescription.Create(SkeletonMap.BoneTransform);
                var avatar = mDescription.CreateAvatar(CharacterTransform);
                EditorHelper.WriteFile(avatar, avatar.GetType(), EditorHelper.InvalidFolderOpenPenel(DataPath)  + '/' + CharacterTransform.name + "_Create via all transforms");
            }

        }

        EditorGUI.EndDisabledGroup();

        // draw bone list
        DrawBoneList();
    }

    static void ScanSkinnedMeshRenderer(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();

        var sb = new StringBuilder();

        foreach (var renderer in renderers)
            sb.AppendLine(renderer.name + " -- " + renderer.bones.Length);

        if (renderers.FirstOrDefault() is SkinnedMeshRenderer skinned)
            foreach (var bone in skinned.bones)
                if (bone == null)
                    Debug.LogError("Missing renderer bone.");
                else
                    sb.AppendLine(bone.name);

        var ren = renderers[0];

        var pose = ren.sharedMesh.bindposes;


        Debug.Log(sb.ToString());
    }

    void DrawManualBindingBone(Transform[] tfs)
    {
        #region Binding Custom Keywords

        GUILayout.BeginHorizontal();
        KeyWord = EditorGUILayout.TextField(KeyWord);
        GUILayout.Label("EnterKeyWord");
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();

        if (!string.IsNullOrEmpty(KeyWord))
        {
            var matchTransforms = tfs.Where(x => x.name.ToUpper().Contains(KeyWord.ToUpper()));

            var label = new GUIContent("Key In") { tooltip = "Bind this transform into skeleton map." };


            using (var cache = new EditorUIColorCache(() => EditorStyles.objectField.normal.textColor, (x) => EditorStyles.objectField.normal.textColor = x))
            {
                foreach (var tf in matchTransforms)
                {
                    GUILayout.BeginHorizontal();

                    if (!SkeletonMap.BoneTransform.Reverse.Contains(tf))
                        cache.SetColor(Color.red);
                    else
                        cache.SetColor();

                    if (!SkeletonMap.BoneTransform.Reverse.Contains(tf))
                        EditorGUILayout.ObjectField(tf, typeof(Transform), true);
                    else
                        EditorGUILayout.ObjectField(tf, typeof(Transform), true);

                    if (GUILayout.Button(label))
                    {
                        if (TryBindBone(tf))
                            EditorGUILayout.HelpBox("Success bind " + tf + ".", MessageType.Info);
                        else
                            EditorGUILayout.HelpBox("Fail to bind " + tf + ", please drag it manual !", MessageType.Error);
                    }

                    GUILayout.EndHorizontal();
                }
            }

            //EditorStyles.objectField.normal.textColor = catchColor;
        }
        else
        {
            EditorGUILayout.HelpBox("Enter bone name to quick search !", MessageType.Info);
        }

        GUILayout.EndVertical();

        #endregion
    }

    void DrawBoneList()
    {
        #region Show Bones
        var map = SkeletonMap.BoneTransform.Forward.keyValuePairs;
        var dictionary = SkeletonMap.BoneTransform;

        EditorGUILayout.BeginVertical(GUILayout.Height(400));
        Scroll = EditorGUILayout.BeginScrollView(Scroll, GUILayout.Height(300));
        var length = map.Length;
        var catchColor = EditorStyles.objectField.normal.textColor;
        var errorColor = Color.red;
        for (int i = 0; i < length; i++)
        {
            var value = map[i].Value;


            if (map[i].Value == null)
                EditorStyles.objectField.normal.textColor = errorColor;
            else
                EditorStyles.objectField.normal.textColor = catchColor;

            var newValue = EditorGUILayout.ObjectField(map[i].Key.ToString(), value, typeof(Transform), true) as Transform;


            if (value == newValue)
                continue;

            Debug.Log(value + "-" + newValue);

            dictionary.Edit(map[i].Key, value, newValue);
        }

        EditorStyles.objectField.normal.textColor = catchColor;
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        #endregion
    }

    void DrawRootPort()
    {
        CharacterTransform = EditorGUILayout.ObjectField("CharacterTransform", CharacterTransform, typeof(Transform), true) as Transform;

        if (!CharacterTransform)
            EditorGUILayout.HelpBox("This function require the transfrom of target !", MessageType.Error);
    }

    /// <summary>
    /// Normalize skeleton => humanoid & rotation & name
    /// </summary>
    void DrawNormalized()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();

        ReplaceSkeleton = EditorGUILayout.ToggleLeft(nameof(ReplaceSkeleton), ReplaceSkeleton);
        RemoveFinalBone = EditorGUILayout.ToggleLeft(nameof(RemoveFinalBone), RemoveFinalBone);
        RemoveNonHumanBone = EditorGUILayout.ToggleLeft(nameof(RemoveNonHumanBone), RemoveNonHumanBone);
        KeepSkinnedBone = EditorGUILayout.ToggleLeft(nameof(KeepSkinnedBone), KeepSkinnedBone);

        var serializedObject = new SerializedObject(this);
        SerializedProperty sprop = serializedObject.FindProperty(nameof(OnCullingObjects));
        EditorGUILayout.PropertyField(sprop);

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("NormalizeBones"))
            NormalizeBones();

        EditorGUILayout.EndHorizontal();
    }

    #region Normalize Skeleton

    [HideInInspector]
    public bool ReplaceSkeleton = true;
    [HideInInspector]
    public bool RemoveNonHumanBone = true;
    [HideInInspector]
    public bool RemoveFinalBone = false;

    [HideInInspector]
    public bool KeepSkinnedBone = true;

    /// <summary>
    /// Custom function to set GameObject name of human bone.
    /// </summary>
    [HideInInspector]
    public Func<HumanBodyBones, string> SetBoneNameFunc;

    public void NormalizeBones()
    {
        try
        {
            UnityEditor.EditorUtility.DisplayProgressBar("Scanning", "Searching for skeleton bones", 0);
            DoNormalizeBones();
        }
        finally 
        {

            UnityEditor.EditorUtility.ClearProgressBar();
        }
    }

    void DoNormalizeBones()
    {
        if(PrefabUtility.GetPrefabInstanceStatus(CharacterTransform.gameObject).HasFlag(PrefabInstanceStatus.Connected))
            PrefabUtility.UnpackPrefabInstance(CharacterTransform.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        Undo.RegisterFullObjectHierarchyUndo(CharacterTransform.gameObject, nameof(NormalizeBones));

        var originSkeletonMap = SkeletonMap.BoneTransform.Forward;// new Dictionary<HumanBodyBones, Transform>(SkeletonMap.BoneTransform.Forward.keyValuePairs);
        var names = new Dictionary<Transform, HumanBodyBones>(SkeletonMap.BoneTransform.Reverse.keyValuePairs);
        var originBones = originSkeletonMap.Select(x => x.Value).ToList();
        var root = CharacterTransform;
        var fwd = root.forward;
        var up = root.up;
        var right = root.right;

        // check root
        if (!originSkeletonMap.TryGetValue(HumanBodyBones.Hips, out var hipsBone))
        {
            Debug.LogError("Missing hips bone.");
            return;
        }

        var skeletonRoot = hipsBone.parent;

        skeletonRoot.gameObject.SetActive(true);

        var newSkeletonRoot = Instantiate(skeletonRoot.gameObject, root, false);
        newSkeletonRoot.name = skeletonRoot.name;

        skeletonRoot.name += "_old";

        var skeletonBones = newSkeletonRoot.transform.Traverse().ToList();//.Where(width=>width.CharacterRoot.activeInHierarchy);

        skeletonBones.Remove(newSkeletonRoot.transform);

        // debug log all bones under skeleton
        //{
        //    var sb_check = new StringBuilder();
        //    int a = 0;

        //    foreach (var bone in skeletonBones)
        //        sb_check.AppendLine(a++ + " : " + bone.name);

        //    Debug.Log(sb_check.ToString());
        //}

        // cache origin bone as key and new skeleton bone as value, prepare for setting skinned mesh renderer bones.
        var skeletonPairs = new Dictionary<Transform, Transform>((int)HumanBodyBones.LastBone);
        skeletonPairs.Add(skeletonRoot, newSkeletonRoot.transform);

        var skinnedRenderers = CharacterTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        var cacheSkinnedBoneName = skinnedRenderers.SelectMany(x => x.bones.Select(y => y)).ToHashSet().ToDictionary(tf => tf.name);

        var bones = new Dictionary<HumanBodyBones, Transform>(SkeletonMap.BoneTransform.Forward.keyValuePairs);

        // delete not human bone
        var deleteList = new List<GameObject>(Mathf.Max(skeletonBones.Count() - (int)HumanBodyBones.LastBone, 0));

        // remove redundancy transforms **equipment
        if (CharacterTransform.TryGetComponent<Animator>(out var animator))
        {
            var skeletonMap = animator.avatar.humanDescription.skeleton.ToDictionary(x => x.name);

            // cache origin human bone name
            var dic_human = animator.avatar.humanDescription.human.ToDictionary(x => x.boneName);



            bones.Clear();

            var changeLog = new StringBuilder();
            var errorLog = new StringBuilder();

            foreach (var bone in skeletonBones)
            {
                var name = bone.name;

                var isEndBone =/*!bone.name.ToUpper().Contains("END") ||*/ bone.childCount == 0;

                if (cacheSkinnedBoneName.TryGetValue(name, out var originBone))
                {
                    if (dic_human.TryGetValue(name, out var human))
                    {
                        var humanName = human.humanName.Replace(" ", "");

                        if (Enum.TryParse<HumanBodyBones>(humanName, true, out var bodybone))
                        {
                            bones.Add(bodybone, bone);

                            var newName = SetBoneNameFunc(bodybone);

                            if (bone.name != newName)
                            {
                                changeLog.AppendLine("Rename bone from " + name + " to " + newName);

                                // set transform name
                                if (SetBoneNameFunc.NotNull())
                                    bone.gameObject.name = SetBoneNameFunc(bodybone);
                            }
                        }
                        else
                            errorLog.AppendLine("Failure to parse human bone : " + human.humanName);
                    }
                    else if (isEndBone && !RemoveFinalBone)
                    {
                        string newName;

                        if (IsFinger(bone)||IsChild(bone,"Toe")||bone.name.Contains("end", StringComparison.CurrentCultureIgnoreCase))
                            newName = bone.parent.name + "_End";
                        else
                            newName = originBone.name;

                        changeLog.AppendLine("Rename bone from " + name + " to " + newName);

                        bone.gameObject.name = newName;
                    }
                    else if (!KeepSkinnedBone) // remove
                    {
                        deleteList.CheckAdd(bone.gameObject);
                        changeLog.AppendLine("Remove skinned bone : " + name);
                        continue;
                        //Debug.LogError("Failure to find human name of skinned bone : " + human.humanName);
                    }

                    skeletonPairs.Add(originBone, bone);
                }
                else
                {
                    // set removeable transform from skeleton map
                    if (!skeletonMap.ContainsKey(name))
                    {
                        deleteList.CheckAdd(bone.gameObject);
                    }
                    else if (RemoveNonHumanBone && !dic_human.ContainsKey(name))
                    {
                        if (isEndBone && !RemoveFinalBone)
                            deleteList.CheckAdd(bone.gameObject);
                    }
                    else
                    {
                        deleteList.CheckAdd(bone.gameObject);
                        errorLog.AppendLine("Culling bone function has ignore : " + name);
                    }
                }
            }

            if (errorLog.Length > 0)
                Debug.LogError(errorLog.ToString());

            if (changeLog.Length > 0)
                Debug.Log(changeLog.ToString());

        }
        else
        {
            Debug.LogError("Couldn't found animator on root transform. \nThis will cause gameobject culling failure.");
            return;
        }

        // cache attach item
        var attachBoneCache = new Dictionary<GameObject, PR>();
        {
            deleteList.Remove(newSkeletonRoot);

            if (deleteList.Count > 0)
            {
                var removed = RemoveNonHumanBone && OnCullingObjects.GetPersistentEventCount() == 0;

                var sb = new StringBuilder();

                if (removed)
                    sb.AppendLine("Following transform has been remove cause not found in avatar skeleton data : ");

                var length = deleteList.Count;

                for (int i = 0; i < length; i++)
                {

                    if(!deleteList[i])
                    {
                        //Debug.LogError(i);
                        continue;
                    }

                    //if (deleteList[i].transform.childCount > 0)
                    //    Debug.LogError(deleteList[i].name);

                    if (RemoveNonHumanBone)
                    {


                        if (removed)
                        {
                            sb.AppendLine(deleteList[i].name);
                            DestroyImmediate(deleteList[i]);
                        }
                        else
                            OnCullingObjects.Invoke(deleteList[i]);
                    }
                    else
                    {
                        var itme = deleteList[i].transform;
                        attachBoneCache.Add(deleteList[i], new(CharacterTransform.InverseTransformPoint(itme.position), CharacterTransform.InverseTransformRotation(itme.rotation)));
                    }

                }

                if (removed)
                    Debug.Log(sb.ToString());
            }
        }


        var targetBones = newSkeletonRoot.transform.Traverse().Where(x => x.gameObject.activeInHierarchy).ToList();
        targetBones.Remove(newSkeletonRoot.transform);

        // Set Editor Bone Renderer
        {
            var devRenderer = new GameObject("BoneRenderer")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            var renderer = devRenderer.InstanceIfNull<BoneRenderer>();
            renderer.transforms = targetBones.ToArray();


            var originRenderer = devRenderer.AddComponent<BoneRenderer>();
            originRenderer.transforms = originBones.ToArray();
            originRenderer.boneColor = Color.red;
        }

        #region Body

        AlignSpineChild(
            bones,
            HumanBodyBones.Hips,
            HumanBodyBones.Spine,
            fwd);


        AlignSpineChild(
            bones,
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            fwd);


        AlignSpineChild(
            bones,
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            fwd);


        AlignSpineChild(
            bones,
            HumanBodyBones.UpperChest,
            HumanBodyBones.Neck,
            fwd);


        AlignSpineChild(
            bones,
            HumanBodyBones.Neck,
            HumanBodyBones.Head,
            fwd);


        if (bones.TryGetValue(HumanBodyBones.Head, out var tf))
        {
            EditRotationButKeepChild(tf, Quaternion.LookRotation(fwd, up));
            //tf.name = HumanBodyBones.View.ToString();
            AlignEndBone(tf);
        }

        #endregion

        #region Legs

        if (bones.TryGetValue(HumanBodyBones.RightUpperLeg, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightLowerLeg, out var child))
                AlignLegChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.RightLowerLeg, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightFoot, out var child))
                AlignLegChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.RightFoot, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightToes, out var child))
                AlignLegChild(tf, child, up);

        if (bones.TryGetValue(HumanBodyBones.RightToes, out tf))
        {
            EditRotationButKeepChild(tf, Quaternion.LookRotation(fwd, up));
            AlignEndBone(tf);
        }

        if (bones.TryGetValue(HumanBodyBones.LeftUpperLeg, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftLowerLeg, out var child))
                AlignLegChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.LeftLowerLeg, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftFoot, out var child))
                AlignLegChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.LeftFoot, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftToes, out var child))
                AlignLegChild(tf, child, up);

        if (bones.TryGetValue(HumanBodyBones.LeftToes, out tf))
        {
            EditRotationButKeepChild(tf, Quaternion.LookRotation(fwd, up));
            AlignEndBone(tf);
        }

        #endregion

        #region Arms

        if (bones.TryGetValue(HumanBodyBones.RightShoulder, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightUpperArm, out var child))
                AlignArmChild(tf, child, up);

        if (bones.TryGetValue(HumanBodyBones.RightUpperArm, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightLowerArm, out var child))
                AlignArmChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.RightLowerArm, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightHand, out var child))
                AlignArmChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.RightHand, out tf))
            EditRotationButKeepChild(tf, Quaternion.LookRotation(right, fwd));

        if (bones.TryGetValue(HumanBodyBones.LeftShoulder, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftUpperArm, out var child))
                AlignArmChild(tf, child, up);

        if (bones.TryGetValue(HumanBodyBones.LeftUpperArm, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftLowerArm, out var child))
                AlignArmChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.LeftLowerArm, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftHand, out var child))
                AlignArmChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.LeftHand, out tf))
            EditRotationButKeepChild(tf, Quaternion.LookRotation(-right, fwd));

        #endregion

        #region Finger

        var bondsID = bones.Select(x => x.Value.name.ToUpper()).ToHashSet();

        if (bones.TryGetValue(HumanBodyBones.RightHand, out var hand))
            AlignFingerChild(hand, bondsID);

        if (bones.TryGetValue(HumanBodyBones.LeftHand, out hand))
            AlignFingerChild(hand, bondsID);


        #endregion

        #region Remap Skeleton

        if (ReplaceSkeleton)
        {
            skeletonRoot.gameObject.SetActive(false);

            foreach (var renderer in skinnedRenderers)
            {
                var rendererTransform = renderer.transform;

                var skinnedBones = renderer.bones;

                var length = skinnedBones.Length;

                var bindposes = new Matrix4x4[length];

                for (int i = 0; i < length; i++)
                {
                    if (skeletonPairs.TryGetValue(skinnedBones[i], out var newBone))
                    {
                        skinnedBones[i] = newBone;
                        bindposes.RebindBone(i, rendererTransform, newBone);
                        //bindposes[i] = newBone.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                    }
                    else
                    {
                        Debug.LogError("Can't found new bone at " + i + " this will fuck up everything if missing skinned bone." + skinnedBones[i]);
                    }
                }

                renderer.bones = skinnedBones;

                var mesh = renderer.sharedMesh.Copy();
                mesh.bindposes = bindposes;

                renderer.sharedMesh = mesh;
                renderer.rootBone = newSkeletonRoot.transform;
            }

            DestroyImmediate(skeletonRoot.gameObject);

            SkeletonMap = SkeletonMap.BuildSkeketonBase;

            foreach (var bone in bones)
                SkeletonMap.BindBone(bone.Key,bone.Value);

            SkeletonMap.SkeletonRoot = newSkeletonRoot.transform;
        }

        if (!RemoveNonHumanBone)
        {
            foreach (var cache in attachBoneCache)
            {
                var item = cache.Key.transform;
                var pr = cache.Value;

                item.SetPositionAndRotation(CharacterTransform.TransformPoint(pr), CharacterTransform.TransformRotation(pr));
                OnCullingObjects?.Invoke(cache.Key);
            }
        }

        #endregion
    }

    void AlignEndBone(Transform tf)
    {
        if (tf.childCount == 0)
            return;

        foreach (Transform child in tf)
        {
            if (child.childCount == 0)
                if (child.name.Contains("end", StringComparison.CurrentCultureIgnoreCase))
                    child.rotation = Quaternion.LookRotation(child.position-tf.position,tf.up);
        }
    }

    void AlignSpineChild(Dictionary<HumanBodyBones,Transform> dic,HumanBodyBones bone,HumanBodyBones childBone,Vector3 forward)
    {
        if (dic.TryGetValue(bone, out var tf))
            if (dic.TryGetValue(childBone, out var child))
            {
                AlignSpineChild(tf, child, forward);
                tf.name = bone.ToString();
            }
    }

    static void AlignSpineChild(Transform tf, Transform child, Vector3 fwd)
    {
        EditRotationButKeepChild(tf, Quaternion.LookRotation(fwd, child.position - tf.position));
    }

    static void AlignLegChild(Transform tf, Transform child, Vector3 up)
    {
        EditRotationButKeepChild(tf, Quaternion.LookRotation(child.position - tf.position, up));
    }

    static void AlignArmChild(Transform tf, Transform child, Vector3 up)
    {
        EditRotationButKeepChild(tf, Quaternion.LookRotation(child.position - tf.position, up));
    }

    static void AlignFingerChild(Transform hand,HashSet<string> humanBoneNames)
    {

        foreach (Transform finger in hand)
        {
            var knuckle = finger;
            var valid = knuckle.childCount > 0;

            while (valid)
            {
                var child = knuckle.GetChild(0);

                if (humanBoneNames.Contains(knuckle.name.ToUpper()))
                {
                    var fwd = child.position - knuckle.position;

                    EditRotationButKeepChild(knuckle, Quaternion.LookRotation(fwd, knuckle.up));
                }
                else
                {
                    EditRotationButKeepChild(knuckle, knuckle.parent.rotation);
                }

                valid = child.childCount > 0;

                if(valid) 
                {
                    knuckle = child;
                }
                else if(child.name.Contains("end",StringComparison.CurrentCultureIgnoreCase))
                {
                    child.rotation = knuckle.rotation;
                    break;
                }
                else // finger end
                {
                    child.rotation = knuckle.rotation;
                    break;
                }
            }


        }

    }

    static void EditRotationButKeepChild(Transform tf, Quaternion rotation)
    {
        var tfs = tf.GetComponentsInChildren<Transform>();
        var length = tfs.Length;

        // cache
        var positions = tfs.Select(x => x.position).ToArray();
        var rotations = tfs.Select(x => x.rotation).ToArray();

        tf.rotation = rotation;

        for (int i = 0; i < length; i++)
        {
            if (tfs[i] == tf)
                continue;

            tfs[i].SetPositionAndRotation(positions[i], rotations[i]);
        }
    }

    #endregion


    public const string PrefKeyWord = "AvatarMakerKeywords";


    static Transform m_Root;

    public Transform CharacterTransform
    {
        get { return m_Root; }
        set
        {
            if (m_Root != value)
            {
                BindingData.Clear();
                SkeletonMap = SkeletonMap.BuildSkeketonBase;
            }
            m_Root = value;
        }
    }



    static string[] m_allBoneNames;

    public string[] AllBoneName
    {
        get
        {
            if (m_allBoneNames is null)
                m_allBoneNames = Return.HumanBodyBonesUtility.AllHumanBodyBoneNamesInTitlecaseLetter;

            return m_allBoneNames;
        }
    }



    static Transform[] m_allBoneTransform;

    public Transform[] AllBoneTransform
    {
        get
        {
            if (m_allBoneTransform is null)
                m_allBoneTransform = m_Root.GetComponentsInChildren<Transform>();

            return m_allBoneTransform;
        }
    }

    Dictionary<HumanBodyBones, MapData> BindingData = new Dictionary<HumanBodyBones, MapData>();

    struct MapData
    {
        public int Score;
        public Transform Bone;
    }


    [LabelText("KeyWordPair")]
    [Tooltip("Match ReadOnlyTransform name")]
    [HorizontalGroup("KeyPair", Width = 100)]
    [ShowInInspector]
    public string Newkey;
    [HideLabel]
    [HorizontalGroup("KeyPair", Width = 100)]
    [ShowInInspector]
    [Tooltip("Match Humanbodybone")]
    public string Newvalue;

    static Dictionary<string, string> KeywordPair;

    [HorizontalGroup("KeyPair", Width = 110)]
    [Button(nameof(AddKeyword))]
    public void AddKeyword()
    {
        if (string.IsNullOrEmpty(Newkey) || string.IsNullOrEmpty(Newvalue))
            return;
        KeywordPair.SafeAdd(Newkey.ToUpper(), Newvalue.ToUpper());
    }

    [HorizontalGroup("KeyPair", Width = 110)]
    [Button(nameof(RemoveKeyword))]
    public void RemoveKeyword()
    {
        if (string.IsNullOrEmpty(Newkey) || string.IsNullOrEmpty(Newvalue))
            return;
        KeywordPair.Remove(Newkey.ToUpper());
    }

    public SkeletonMap SkeletonMap;

    #region Scan via name

    public void ScanBone()
    {
        var sb = new StringBuilder();

        var tfs = AllBoneTransform;
        var sn = 0;

        foreach (var tf in tfs)
        {
            UnityEditor.EditorUtility.DisplayProgressBar("Scanning", "Searching for skeleton bones", (float)sn / tfs.Length);
            if (!TryBindBone(tf))
            {
                SkeletonMap.ExtraBoneMap.SafeAdd(tf, -1);
                sb.AppendLine(tf.name);
            }
            sn++;
        }


        UnityEditor.EditorUtility.ClearProgressBar();

        if (!string.IsNullOrEmpty(sb.ToString()))
            Debug.LogError("The following transforms can't been reference to skeleton data via human name. \n" + sb.ToString());
    }

    static string[] LoadKeyWord(string name)
    {
        var tags = mTextUtility.Depart(name, mTextUtility.TitlecaseLetter).Select(x => x.ToUpper()).ToList();

        var length = tags.Count;

        for (int i = 0; i < length; i++)
        {
            if (KeywordPair.TryGetValue(tags[i], out var bindTag))
                tags.Add(bindTag);

            switch (tags[i])
            {
                case "L":
                    tags[i] = "LEFT";
                    break;
                case "R":
                    tags[i] = "RIGHT";
                    break;
                case "U":
                    tags[i] = "UPPER";
                    break;
                case "D":
                    tags[i] = "DOWN";
                    break;
                case "LOW":
                    tags[i] = "LOWER";
                    break;
                default:
                    if (tags[i].Length == 1)
                        tags[i] = string.Empty;
                    break;
            }
        }



        return tags.Where(x => !string.Empty.Equals(x)).ToArray();
    }

    bool IsChild(Transform tf,string parentName,int iterator=4)
    {
        var parent = tf.parent;
        
        while (parent != null)
        {
            if (parent.name.Contains(parentName, StringComparison.CurrentCultureIgnoreCase))
                return true;

            parent = parent.parent;

            if (iterator > 0)
                iterator--;
            else
                break;
        }

        return false;
    }


    bool IsFinger(Transform tf)
    {
        return IsChild(tf, "Hand");

        //var parent = hand.parent;
        //while (parent != null)
        //{
        //    if (parent.name.Contains("HAND",StringComparison.CurrentCultureIgnoreCase))
        //        return true;

        //    parent = parent.parent;
        //}
        //return false;
    }

    bool SetFinger(Transform tf)
    {
        int HumanSN = 0;
        var parent = tf.parent;
        var index = 0;
        while (parent != null)
        {
            if (parent.name.ToUpper().Contains("HAND"))
            {
                var name = parent.name.ToUpper();

                var dot = Vector3.Dot(CharacterTransform.right, parent.position - CharacterTransform.position);
                if (dot > 0)
                    HumanSN = (int)HumanBodyBones.RightThumbProximal;
                else
                    HumanSN = (int)HumanBodyBones.LeftThumbProximal;

                break;
            }
            index++;
            if (index > 3)
            {
                Debug.Log(tf.name);
                return false;
            }

            parent = parent.parent;
        }



        if (HumanSN == 0)
            return false;


        var fingerName = tf.name.ToLower();

        if (fingerName.Contains("thumb"))
            HumanSN += 0;
        else if (fingerName.Contains("index"))
            HumanSN += 3;
        else if (fingerName.Contains("middle"))
            HumanSN += 6;
        else if (fingerName.Contains("ring"))
            HumanSN += 9;
        else if (fingerName.Contains("little"))
            HumanSN += 12;

        HumanSN += index;
        Debug.Log(tf.name + " : " + (HumanBodyBones)HumanSN);
        SkeletonMap.BoneTransform.Add((HumanBodyBones)HumanSN, tf);

        return true;
    }

    public bool TryBindBone(Transform tf)
    {
        //CheckAdd is finger

        if (tf.name.ToUpper().Contains("END"))
            return false;

        if (IsFinger(tf))
            return SetFinger(tf);

        //departstring
        var tags = LoadKeyWord(tf.name);
        var bonesName = AllBoneName;
        var count = tags.Length;

        var score = 0;
        var index = 0;
        var length = bonesName.Length;
        var results = new List<possibleResult>();

        for (int i = 0; i < length; i++)
        {
            var mScore = 0;
            var bone = bonesName[i];
            for (int w = 0; w < count; w++)
            {
                if (tags[w].Length < 3)
                    continue;
                if (bone.Contains(tags[w]))
                    mScore++;

            }

            if (mScore > score)
            {
                index = i;
                score = mScore;
                results.Add(new possibleResult() { HumanBodyBone = (HumanBodyBones)i, Score = score });
            }
        }


        var sb = new StringBuilder();

        foreach (var result in results)
        {
            sb.AppendLine("Match Bone : " + result.HumanBodyBone + "=>" + result.Score);
        }

        sb.AppendLine("Tags : ");

        foreach (var text in tags)
        {
            sb.Append(text);
            sb.Append('-');
        }

        Debug.Log(sb.ToString());



        if (score == 0)
            return false;

        results.Reverse();
        BindBone(tf, results.ToArray());
        return true;
    }

    public void BindBone(Transform tf, possibleResult[] results)
    {

        foreach (var result in results)
        {
            if (!BindingData.TryGetValue(result.HumanBodyBone, out var targetData))
            {
                SkeletonMap.BoneTransform.Add(result.HumanBodyBone, tf);
                BindingData.SafeAdd(result.HumanBodyBone, new MapData() { Bone = tf, Score = result.Score });
                return;
            }
            else
            {
                if (targetData.Bone.Equals(tf))
                    return;

                if (targetData.Score > result.Score)
                    continue;
                else
                {
                    Debug.LogError(string.Format(" Overwrite  {0} -- {1} with {2}", result.HumanBodyBone, targetData.Bone.name, tf.name));
                    SkeletonMap.ExtraBoneMap.SafeAdd(targetData.Bone, -1);
                    SkeletonMap.BoneTransform.Add(result.HumanBodyBone, tf);
                    BindingData.SafeAdd(result.HumanBodyBone, new MapData() { Bone = tf, Score = result.Score });

                    return;
                }
            }
        }
        var sb = new System.Text.StringBuilder();
        foreach (var result in results)
        {
            sb.Append(result.HumanBodyBone.ToString() + " : " + result.Score + " lower than " + BindingData[result.HumanBodyBone].Score + '\n');
        }
        Debug.LogError("Reference bone fail : " + tf + sb.ToString());
        //Bind To Extra

        SkeletonMap.ExtraBoneMap.SafeAdd(tf, -1);

    }

    public struct possibleResult
    {
        public HumanBodyBones HumanBodyBone;
        public int Score;
    }

    #endregion


    [Tooltip("Scan avatar data from another")]
    public void ScanAvatar()
    {
        // check animator
        if (CharacterTransform.IsNull() || !CharacterTransform.TryGetComponent<Animator>(out var animator))
        {
            Debug.LogError(string.Format("Couldn't found animator of {0}.", CharacterTransform));
            return;
        }

        // check avatar
        if (!animator.isHuman || !animator.avatar)
        {
            Debug.LogError(string.Format("Avatar of {0} is not humanoid.", animator));
            return;
        }

        // cache all humanBodyBones
        var bones = Return.HumanBodyBonesUtility.AllHumanBodyBones;

        // clean skeleton map
        SkeletonMap = SkeletonMap.BuildSkeketonBase;


        foreach (var bone in bones)
        {
            var tf = animator.GetBoneTransform(bone);
            if (!tf)
                continue;

            SkeletonMap.BoneTransform.Add(bone, tf);
        }

        var tfs = CharacterTransform.Traverse();

        foreach (var tf in tfs)
        {
            if (!SkeletonMap.BoneTransform.Reverse.Contains(tf))
                SkeletonMap.ExtraBoneMap.Add(tf, -1);
        }

    }


    /// <summary>
    /// Add selected object as new avatar. 
    /// </summary>
    [Obsolete]
    public static void MakeAvatarMask()
    {
        GameObject activeGameObject = Selection.activeGameObject;

        if (activeGameObject != null)
        {
            AvatarMask avatarMask = new ();

            avatarMask.AddTransformPath(activeGameObject.transform);

            var path = string.Format("Assets/{0}.mask", activeGameObject.name.Replace(':', '_'));
            AssetDatabase.CreateAsset(avatarMask, path);
        }
    }

    [Obsolete]
    public static void MakeAvatar()
    {
        GameObject activeGameObject = Selection.activeGameObject;

        if (activeGameObject != null)
        {
            Avatar avatar = AvatarBuilder.BuildHumanAvatar(activeGameObject, new HumanDescription() { });
            avatar.name = activeGameObject.name;
            Debug.Log(avatar.isHuman ? "is human" : "is generic");

            var path = string.Format("Assets/{0}.ht", avatar.name.Replace(':', '_'));
            AssetDatabase.CreateAsset(avatar, path);
        }
    }

    protected override void OnEnable()
    {
        var json = PlayerPrefs.GetString(PrefKeyWord);
        if (string.IsNullOrEmpty(json))
            KeywordPair = new Dictionary<string, string>();

        KeywordPair = new Dictionary<string, string>(JsonConvert.DeserializeObject<KeyValuePair<string, string>[]>(json));

        SkeletonMap = SkeletonMap.BuildSkeketonBase;
    }

    protected void OnDisable()
    {
        PlayerPrefs.SetString(PrefKeyWord, JsonConvert.SerializeObject(KeywordPair.Select(x => x).ToArray()));
    }


    protected override void OnGUI()
    {
        base.OnGUI();
    }

}

[Obsolete]
public class Editor_Avatar
{



    //void AutoDetect()
    //{
    //    var maker = target as AvatarMaker;

    //    maker.CharacterTransform = EditorGUILayout.ObjectField("root", maker.CharacterTransform, typeof(ReadOnlyTransform), true) as ReadOnlyTransform;

    //    EditorGUILayout.BeginHorizontal();

    //    EditorGUI.BeginDisabledGroup(false);
    //    mDescription = EditorGUILayout.ObjectField("m_HumanoidDescription", mDescription, typeof(m_HumanoidDescription), true) as m_HumanoidDescription;
    //    path = EditorGUILayout.TextField(path);
    //    EditorGUI.EndDisabledGroup();

    //    if (GUILayout.Button("Select"))
    //    {
    //        EditorHelper.OpenFile(ref path, ref mDescription);
    //    }
    //    if (GUILayout.Button("Create"))
    //    {
    //        //var skeleton=Skeleton.
    //        mDescription = m_HumanoidDescription.Create();
    //    }

    //    EditorGUILayout.EndHorizontal();

    //    /*
    //    if (null != maker.CharacterTransform && null != mDescription)
    //    {
    //        if (GUILayout.Button("GenerateAvatar"))
    //        {
    //            var avatar = mDescription.CreateAvatar(maker.CharacterTransform);
    //            if (avatar.isValid)
    //                EditorHelper.WriteAsset(avatar, avatar.GetType(), EditorHelper.ChoseFolder() + '/' + maker.CharacterTransform.name + ".avatar.asset");
    //            else
    //                Debug.LogError(avatar + " is not valid");
    //        }
    //    }
    //    else
    //    {
    //        EditorGUI.BeginDisabledGroup(true);
    //        GUILayout.Button("GenerateAvatar");
    //        EditorGUI.EndDisabledGroup();
    //    }
    //    */
    //}





}

/// <summary>
/// Temp cache for using editor style color
/// </summary>
public class EditorUIColorCache : IDisposable
{
    public EditorUIColorCache(Func<Color> getter, Action<Color> setter)
    {
        originColor = getter();
        this.getter = getter;
        this.setter = setter;
    }

    Color originColor;

    Func<Color> getter;
    Action<Color> setter;

    public void SetColor(Color color = default)
    {
        if (color == default)
            color = originColor;

        setter(color);
    }

    public void Dispose()
    {
        setter(originColor);

        getter = null;
        setter = null;
    }
}


