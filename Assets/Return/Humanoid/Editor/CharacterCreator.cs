//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//using Return.Agent;



//#if UNITY_EDITOR
//using Sirenix.OdinInspector;
//using Sirenix.Utilities.Editor;
//using Sirenix.OdinInspector.Editor;
//using UnityEditor;

//public class CharacterCreator
//{
//    public CharacterCreator()
//    {
//        CharacterArchives = CharacterArchives.Instance;
//    }

    

//    [HorizontalGroup("Resource")]
//    public CharacterArchives CharacterArchives;
//    [HorizontalGroup("Resource")]
//    [Button("LoadSource")]
//    public void LoadSource()
//    {
//        CharacterArchives = CharacterArchives.Instance;
//    }

//    bool Targeted => CharacterArchives != null;

//    [SerializeField]
//    [BoxGroup("Preset")][ShowIf("Targeted")]
//    [HideLabel][OnValueChanged("Edit",InvokeOnInitialize =false,IncludeChildren =true) ]
//    public Return.Agent.CharacterInfo Current;

//    [ShowIf("Targeted")]
//    [HorizontalGroup("Selector")]
//    [Button("Last")]
//    public void Last()
//    {
//        DataIndex=mMathf.Loop( CharacterArchives.Count,DataIndex - 1);
//        LoadData();
//    }

//    [ShowIf("Targeted")]
//    [HorizontalGroup("Selector")]
//    [PropertyRange("min", "max")][OnValueChanged("LoadData")]
//    public int DataIndex;

//    [ShowIf("Targeted")]
//    [HorizontalGroup("Selector")]
//    [Button("Next")]
//    public void Next()
//    {
//        DataIndex = mMathf.Loop(CharacterArchives.Count, DataIndex + 1);
//        LoadData();
//    }

//    public int max
//    {
//        get
//        {
//            if (Targeted)
//                return Mathf.Max(default, CharacterArchives.Count - 1);
//            else
//                return default;
//        }
//    }

//    [ShowIf("Targeted")]
//    [Button("Create new character")]
//    public void CreateCharacter()
//    {
//        var info = new Return.Agent.CharacterInfo();
//        Current = info;
//        CharacterArchives.Add(info);
//        DataIndex = (CharacterArchives.Count - 1);
//    }
//    int min=-1;
//    public void LoadData()
//    {
//        DataIndex = Mathf.Clamp(DataIndex,min, max);
//        if (DataIndex >= 0)
//            Current = CharacterArchives[DataIndex];

//        Current.OnAfterDeserialize();

//        if(Current.Skeleton)
//        Selection.activeObject = Current.Skeleton;
//    }


//    public void Edit()
//    {
//        if (null == Current)
//            return;
//        EditorUtilityTools.SetDirty(CharacterArchives);
//    }

//    [ShowIf("Targeted")]
//    [Button("Ping")]
//    public void Ping()
//    {
//        EditorGUIUtility.PingObject(CharacterArchives);
//    }
//}
//#endif