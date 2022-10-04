//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Sirenix.OdinInspector.Editor.Drawers;
//using Sirenix.OdinInspector.Editor.Internal;
//using Sirenix.OdinInspector.Editor;
//using Sirenix.OdinInspector;
//using TNet;
//using Return;
//using UnityEditor;
//using Sirenix.Utilities.Editor;
//using Sirenix.Utilities;
//using System.IO;

//public class DataNodeHandlerDrawer : OdinValueDrawer<DataNodeHandler>
//{
//    protected override void DrawPropertyLayout(GUIContent label)
//    {
//        var value = ValueEntry.SmartValue;


//        SirenixEditorGUI.BeginBox();

//        EditorGUI.BeginChangeCheck();

//        var txt = "DataNodeHandler";

//        if (label == null)
//            label = new(txt);
//        else
//            label.text = txt;

//        EditorGUILayout.LabelField(label);

//        value.ArchiveOption = (ArchiveOption)EditorGUILayout.EnumPopup(nameof(ArchiveOption), value.ArchiveOption);

        
//        switch (value.ArchiveOption)
//        {
//            case ArchiveOption.Persistent:
//                ValueEntry.SmartValue = DrawPersistent(value);
//                break;
//            case ArchiveOption.Custom:
//                ValueEntry.SmartValue = DrawCustom(value);
//                break;
//        }

//        if(EditorGUI.EndChangeCheck())
//            ValueEntry.ApplyChanges();


//        GUILayout.BeginHorizontal();

//        if (GUILayout.Button(nameof(ShowData), SirenixGUIStyles.Button))
//            ShowData(value);

//        if (GUILayout.Button(nameof(DataNodeHandler.CleanData), SirenixGUIStyles.Button))
//            value.CleanData();

//        GUILayout.EndHorizontal();

//        SirenixEditorGUI.EndBox();
//    }

//    protected DataNodeHandler DrawPersistent(DataNodeHandler handler)
//    {
//        DrawDataPath(ref handler);
//        handler.FileName = EditorGUILayout.TextField(nameof(handler.FileName), handler.FileName);
//        return handler;
//    }

//    protected DataNodeHandler DrawCustom(DataNodeHandler handler)
//    {
//        handler.DataPath = EditorGUILayout.TextField(nameof(handler.DataPath), handler.DataPath);
//        handler.FileName = EditorGUILayout.TextField(nameof(handler.FileName), handler.FileName);
//        return handler;
//    }

//    protected void DrawDataPath(ref DataNodeHandler handler)
//    {
//        GUILayout.BeginHorizontal();
//        handler.DataPath = EditorGUILayout.TextField(nameof(handler.DataPath), handler.DataPath);
//        if (GUILayout.Button("Folder", SirenixGUIStyles.ButtonRight,GUILayout.Width(15)))
//            handler.DataPath=EditorUtility.OpenFolderPanel("Archive DataPath",handler.DataPath,null);

//        GUILayout.EndHorizontal();
//    }

//    void ShowData(DataNodeHandler handler)
//    {
//        var folderPath = Path.Combine(Application.persistentDataPath, handler.DataPath);

//        if (Directory.Exists(folderPath))
//            System.Diagnostics.Process.Start(folderPath);
//        else
//            Debug.LogWarning($"None exist file at {handler.GetFilePath()}.");
//    }
//}
