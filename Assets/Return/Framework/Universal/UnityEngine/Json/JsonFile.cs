//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Sirenix.OdinInspector;
//#if UNITY_EDITOR
//using Sirenix.OdinInspector.Editor;
//using Sirenix.Utilities.Editor;
//using UnityEditor;
//#endif

//namespace Return
//{

//    [HideMonoScript]
//    //[NativeHeader("Runtime/Scripting/TextAsset.h")]
//    public class JsonFile : TextAsset
//    {
//        public JsonFile() : base() { }
//        public JsonFile(string txt) : base(txt) { }

//#if UNITY_EDITOR

//        //[ReadOnly]
//        //[ShowInInspector]
//        //string m_Text { get => text; }

//        [OnInspectorGUI]
//        void DrawText()
//        {
//            EditorGUI.BeginDisabledGroup(true);
//            EditorGUILayout.TextArea(text, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
//            EditorGUI.EndDisabledGroup();
//        }

//#endif


//    }
//}