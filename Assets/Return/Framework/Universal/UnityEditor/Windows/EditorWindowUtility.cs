#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorWindowUtility
{
    public static void CollapseInHierarchyWindow(GameObject go, bool collapse)
    {
        if (go.transform.childCount == 0) return;

        FocusUnityEditorWindow(SelectWindowType.Hierarchy);

        Selection.SetActiveObjectWithContext(go, null);

        //if (collapse == false)
        //{
        //    SimulatePCControl.RightArrow();
        //}
        //else
        //{
        //    SimulatePCControl.LeftArrow();
        //}
    }


    private enum SelectWindowType { Inspector, ProjectBrowser, Game, Console, Hierarchy, Scene };

    private static void FocusUnityEditorWindow(SelectWindowType swt)
    {
        System.Type unityEditorWindowType = null;
        EditorWindow editorWindow = null;

        switch (swt)
        {
            case SelectWindowType.Inspector:
                unityEditorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
                break;
            case SelectWindowType.ProjectBrowser:
                unityEditorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
                break;
            case SelectWindowType.Game:
                unityEditorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
                break;
            case SelectWindowType.Console:
                unityEditorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ConsoleView");
                break;
            case SelectWindowType.Hierarchy:
                unityEditorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
                break;
            case SelectWindowType.Scene:
                unityEditorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneView");
                break;
        }

        editorWindow = EditorWindow.GetWindow(unityEditorWindowType);
    }

    private static void SelectObject(GameObject obj)
    {
        Selection.activeObject = obj;
    }
    private static EditorWindow GetFocusedWindow(string window)
    {
        FocusOnWindow(window);
        return EditorWindow.focusedWindow;
    }
    private static void FocusOnWindow(string window)
    {
        EditorApplication.ExecuteMenuItem("Window/" + window);

    }
}
#endif
