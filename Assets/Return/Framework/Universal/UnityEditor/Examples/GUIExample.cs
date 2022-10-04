using UnityEngine;
using UnityEditor;

public class GUIExample : EditorWindow
{
    private Vector2 scrollPosition = Vector2.zero;
    private string search = string.Empty;

    [MenuItem("Tools/GUIExample")]
    public static void Init()
    {
        EditorWindow.GetWindow(typeof(GUIExample));
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        GUILayout.Label("Click to copy style", "label");
        GUILayout.FlexibleSpace();
        GUILayout.Label("Search:");
        search = EditorGUILayout.TextField(search);
        GUILayout.EndHorizontal();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (GUIStyle style in GUI.skin)
        {
            if (style.name.ToLower().Contains(search.ToLower()))
            {
                GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
                GUILayout.Space(7);
                if (GUILayout.Button(style.name, style))
                {
                    EditorGUIUtility.systemCopyBuffer = "\"" + style.name + "\"";
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.SelectableLabel("\"" + style.name + "\"");
                GUILayout.EndHorizontal();
                GUILayout.Space(11);
            }
        }

        GUILayout.EndScrollView();
    }
}