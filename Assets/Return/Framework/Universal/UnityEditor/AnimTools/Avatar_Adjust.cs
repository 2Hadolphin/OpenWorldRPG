using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using System.Linq;
using Return;

public class Avatar_Adjust_WD : EditorWindow
{
    public Avatar_Adjust_WD()
    {
        this.titleContent = new GUIContent("Avatar Editor");
    }
    Editor window;
    [MenuItem("Tools/Animation/Avatar/Edit")]
    public static void OpenAvatarPanel()
    {
        EditorWindow.CreateWindow<Avatar_Adjust_WD>();
    }

    private void OnEnable()
    {
        window = Editor.CreateEditor(Avatar_Adjust_SO.Instance);
    }

    private void OnGUI()
    {
        window.OnInspectorGUI();
    }

}
[Serializable]
public class Avatar_Adjust_SO : ScriptableObject
{
    public static Avatar_Adjust_SO _Instance;

    public static Avatar_Adjust_SO Instance
    {
        get
        {
            if (!_Instance)
                _Instance = ScriptableObject.CreateInstance<Avatar_Adjust_SO>();
            _Instance.hideFlags = HideFlags.HideAndDontSave;
            return _Instance;
        }
    }

    protected Dictionary<string, int> SkeletonSN;
    protected Dictionary<HumanBodyBones, int> HumanBoneMap;
    protected HumanDescription HumanDescription;

    public void Init(ref Avatar avatar)
    {
        HumanDescription = avatar.humanDescription;

        HumanBoneMap = new Dictionary<HumanBodyBones, int>(HumanDescription.human.Length);
        SkeletonSN = new Dictionary<string, int>(HumanDescription.skeleton.Length);
        var enumerator = HumanDescription.human.GetEnumerator();

        int index = 0;
        while (enumerator.MoveNext())
        {
            var humanBone = (HumanBone)enumerator.Current;
            if (Enum.TryParse(humanBone.humanName.Replace(" ",""), out HumanBodyBones humanBodyBone))
                HumanBoneMap.SafeAdd(humanBodyBone,index);
            else
                Debug.LogError(string.Format("Parse human bone {0} failed.", humanBone.humanName));

            index++;
        }


        enumerator = HumanDescription.skeleton.GetEnumerator();
        index = 0;
        while (enumerator.MoveNext())
        {
            var skeleton = (SkeletonBone)enumerator.Current;
            SkeletonSN.SafeAdd(skeleton.name, index);
            index++;
        }


    }

    public bool SearchHumanBone(HumanBodyBones target, out int sn)
    {
        if (HumanBoneMap.TryGetValue(target, out sn))
            return true;

        Debug.LogError(string.Format("Can't find HumanBone reference with HumanBodyBone : {0}", target.ToString()));
        return false;
    }

    public bool SearchSkeleton(HumanBodyBones target, out int sn)
    {
        if(SearchHumanBone(target,out sn))
            if (SkeletonSN.TryGetValue(HumanDescription.human[sn].boneName, out sn))
                return true;

        Debug.LogError(string.Format("Can't find bone reference with HumanBodyBone : {0} which name as {1}.", target.ToString(), HumanDescription.human[sn].boneName));
        sn = -1;
        return false;
    }

}

[CustomEditor(typeof(Avatar_Adjust_SO))]
public class Avatar_Adjust_ED : Editor
{
    static Avatar avatar;
    static GameObject ob;
    static HumanBodyBones humanBodyBone;
    static SkeletonBone SkeletonBone;
    static HumanBone HumanBone;
    static int sn;
    static bool editing;
    static bool SearchResult=false;
    static string path;
    static Vector2 scroll_human;
    static Vector2 scroll_skeleton;
    public override void OnInspectorGUI()
    {
        Compare();
        var dataBase = target as Avatar_Adjust_SO;

        {
            GUILayout.BeginHorizontal();

            var newAvatar = EditorGUILayout.ObjectField("Avatar", avatar, typeof(Avatar), false) as Avatar;

            if (GUILayout.Button("Select Avatar", EditorHelper.MiddleLittleButton(5)))
            {
                EditorHelper.OpenFile(ref path, ref newAvatar);
            }
            if (avatar != newAvatar)
            {
                avatar = newAvatar;
                dataBase.Init(ref avatar);
            }
            GUILayout.EndHorizontal();
        }

        {
           
            GUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            ob = EditorGUILayout.ObjectField("Skeleton", ob, typeof(GameObject), false) as GameObject;
            EditorGUI.EndDisabledGroup();

            string _path = null;
            if (GUILayout.Button("Select Skeleton" ,EditorHelper.MiddleLittleButton(5)))
            {
                EditorHelper.OpenFile(ref _path, ref ob);
            }


            GUILayout.EndHorizontal();
        }

 

        humanBodyBone = (HumanBodyBones)EditorGUILayout.EnumPopup("EditBone", humanBodyBone);

        GUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Search", EditorHelper.MiddleLittleButton(3)))
        {
            {
                if (dataBase.SearchHumanBone(humanBodyBone, out var sn))
                    HumanBone = avatar.humanDescription.human[sn];
            }

            {
                if (dataBase.SearchSkeleton(humanBodyBone, out sn))
                    SkeletonBone = avatar.humanDescription.skeleton[sn];
            }

            editing = true;
            SearchResult = true;
        }




        if (GUILayout.Button("Archive", EditorHelper.MiddleLittleButton(3)))
        {
            if (editing)
            {
                var value = avatar.humanDescription;
                value.skeleton[sn] = SkeletonBone;

                Avatar newavatar = AvatarBuilder.BuildHumanAvatar(ob, value);
                var catchpath = AssetDatabase.GetAssetPath(avatar);
                AssetDatabase.DeleteAsset(catchpath);
                AssetDatabase.CreateAsset(newavatar, catchpath);
                AssetDatabase.SaveAssets();

                avatar = newavatar;
                EditorUtility.SetDirty(avatar);
                EditorGUILayout.HelpBox("Storage " + humanBodyBone.ToString() + "successfully !", MessageType.Info, false);
                SkeletonBone = new SkeletonBone();
                editing = false;
                SearchResult = false;
            }
            else
            {

                EditorGUILayout.HelpBox("Archive " + humanBodyBone.ToString() + "failed !", MessageType.Error, false);
            }

        }
        GUILayout.EndHorizontal();

        if (!avatar && !ob)
        {
            SearchResult = false;
            return;
        }

        if (editing && SearchResult)
            EditorGUILayout.HelpBox("Successed load " + humanBodyBone.ToString() + " !", MessageType.Info, false);
        else if (SearchResult)
        {
            EditorGUILayout.HelpBox("Didn't find any bone call as " + humanBodyBone.ToString() + " !", MessageType.Warning, false);
        }

        {
            var humanBones = avatar.humanDescription.human;
            scroll_human = GUILayout.BeginScrollView(scroll_human);
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Label(avatar.name+"_Skeleton count : "+humanBones.Length);

            foreach (var skeleton in humanBones)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField(skeleton.humanName, EditorHelper.MiddleLittleButton(4));
                EditorGUILayout.TextField(skeleton.boneName, EditorHelper.MiddleLittleButton(4));
                EditorGUILayout.FloatField("AxisLength",skeleton.limit.axisLength, EditorHelper.MiddleLittleButton(4));
                GUILayout.EndHorizontal();
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.EndScrollView();
        }

        {
            var skeletons = avatar.humanDescription.skeleton;
            scroll_skeleton = GUILayout.BeginScrollView(scroll_skeleton);

            if(GUILayout.Button("CopyData"))
            {
                var sb=new System.Text.StringBuilder();
                var enumator=skeletons.Select(x => x.name).GetEnumerator();
                while (enumator.MoveNext())
                {
                    sb.AppendLine(enumator.Current);
                }
         
                EditorGUIUtility.systemCopyBuffer = sb.ToString();
            }

            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Label(avatar.name + "_Skeleton count : " + skeletons.Length);

            foreach (var skeleton in skeletons)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField(skeleton.name, EditorHelper.MiddleLittleButton(4));
                EditorGUILayout.Vector3Field("Position",skeleton.position, EditorHelper.MiddleLittleButton(4));
                EditorGUILayout.Vector3Field("Rotation", skeleton.rotation.eulerAngles, EditorHelper.MiddleLittleButton(4));
                EditorGUILayout.Vector3Field("Scale", skeleton.scale, EditorHelper.MiddleLittleButton(4));

                GUILayout.EndHorizontal();
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.EndScrollView();
        }



        if (editing)
        {

            EditorGUILayout.BeginVertical(new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white } });
            SkeletonBone.name = EditorGUILayout.TextField(SkeletonBone.name);
            SkeletonBone.position = EditorGUILayout.Vector3Field("Position", SkeletonBone.position);
            SkeletonBone.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", SkeletonBone.rotation.eulerAngles));
            SkeletonBone.scale = EditorGUILayout.Vector3Field("Scale", SkeletonBone.scale);
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(true);
            HumanBone.boneName = EditorGUILayout.TextField("BoneName",HumanBone.boneName);
            HumanBone.humanName = EditorGUILayout.TextField("HumanName",HumanBone.humanName);
            HumanBone.limit.axisLength = EditorGUILayout.FloatField("AxisLength", HumanBone.limit.axisLength);
            HumanBone.limit.center= EditorGUILayout.Vector3Field("Center", HumanBone.limit.center);
            HumanBone.limit.max = EditorGUILayout.Vector3Field("Max", HumanBone.limit.max);
            HumanBone.limit.min = EditorGUILayout.Vector3Field("Min", HumanBone.limit.min);
            EditorGUI.EndDisabledGroup();
            if (Enum.TryParse<HumanBodyBones>(HumanBone.humanName, out var humanBone))
                EditorGUILayout.EnumPopup(humanBone);
            EditorGUILayout.EndVertical();
        }



    }

    static Avatar avatar1;
    static Avatar avatar2;
    void Compare()
    {
        avatar1 = EditorGUILayout.ObjectField("AvatarA", avatar1, typeof(Avatar), false) as Avatar;
        avatar2 = EditorGUILayout.ObjectField("AvatarB", avatar2, typeof(Avatar), false) as Avatar;

        if (!avatar1 || !avatar2)
            return;

        if (GUILayout.Button("CompareAll"))
        {
            var skeleton1 = avatar1.humanDescription.skeleton;
            var dictionary1 = new Dictionary<string, int>(skeleton1.Length);

            var length = skeleton1.Length;
            for (int i = 0; i < length; i++)
            {
                dictionary1.Add(skeleton1[i].name, i);
            }

            var skeleton2 = avatar2.humanDescription.skeleton;
            var dictionary2 = new Dictionary<string, int>(skeleton2.Length);
            length = skeleton2.Length;
            for (int i = 0; i < length; i++)
            {
                dictionary2.Add(skeleton2[i].name, i);
            }

            var enumator = dictionary1.GetEnumerator();
            while (enumator.MoveNext())
            {
                var key=enumator.Current.Key;

                if(!dictionary2.TryGetValue(key,out var sn))
                {
                    Debug.LogError("Can't find key : " + key);
                    return;
                }

                var skeletonA = skeleton1[enumator.Current.Value];
                var skeletonB = skeleton2[sn];

                var sb =new System.Text.StringBuilder();


                if (skeletonA.Equals(skeletonB))
                    Debug.Log(string.Format("Name : {0}---{1} \n Position : {2}---{3}", skeletonA.name, skeletonB.name, skeletonA.position, skeletonB.position));
                else
                    Debug.LogError(string.Format("Name : {0}---{1} \n Position : {2}---{3}",skeletonA.name,skeletonB.name,skeletonA.position,skeletonB.position));

            }


            var human1 = avatar1.humanDescription.human;
            var human2 = avatar2.humanDescription.human;

            length = human1.Length;
            var dictionaryH1 = new Dictionary<string, int>(human1.Length);
            for (int i = 0; i < length; i++)
            {
                dictionaryH1.Add(human1[i].humanName, i);
            }

            length = human2.Length;
            var dictionaryH2 = new Dictionary<string, int>(human2.Length);
            for (int i = 0; i < length; i++)
            {
                dictionaryH2.Add(human2[i].humanName, i);
            }

            foreach (var keyPair in dictionaryH1)
            {
                if (!dictionaryH2.TryGetValue(keyPair.Key, out var sn))
                    Debug.LogError("Avatar2 missing " + keyPair.Key);

                var a = human1[keyPair.Value];
                var b = human2[sn];

                if (!a.Equals(b))
                    Debug.Log(a.limit.axisLength + "-" + b.limit.axisLength);
                else
                Debug.Log("Human"+a.limit.Equals(b.limit));
            }

        }
    }

    private static void OpenFile()
    {
        var path = EditorUtility.OpenFilePanel("Avatar", "Assets", "");
        if (path.Contains(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            avatar = AssetDatabase.LoadAssetAtPath(path, typeof(Avatar)) as Avatar;
        }
    }
}

