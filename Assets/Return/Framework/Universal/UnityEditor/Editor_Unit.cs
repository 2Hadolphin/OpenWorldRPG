using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Window_Unit : EditorWindow
{
    public enum Tool { Ruler, Protractor }
    public Tool Operate;
    public float Value;


    Editor window;
    [MenuItem("Tools/Ruler")]
    public static void OpenAvatarPanel()
    {
        EditorWindow.CreateWindow<Window_Unit>();
    }

    private void OnEnable()
    {
        window = Editor.CreateEditor(this);
    }

    private void OnGUI()
    {
        window.OnInspectorGUI();
    }

}
[CustomEditor(typeof(Window_Unit))]
public class Editor_Unit : Editor
{
    public static GameObject A;
    public static GameObject B;
    public static float Distance;
    public static GameObject Direction;

    public static Vector3 Position_Start;
    public static Vector3 Position_Relay;
    public static Vector3 Position_Stop;

    public Vector3 v_a { get { return Position_Start - Position_Relay; } }
    public Vector3 v_b { get { return Position_Stop - Position_Relay; } }
    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();

        var go = target as Window_Unit;

        switch (go.Operate)
        {
            case Window_Unit.Tool.Ruler:
                A = EditorGUILayout.ObjectField("A",A, typeof(GameObject), true) as GameObject;
                B = EditorGUILayout.ObjectField("B",B, typeof(GameObject), true) as GameObject;
                Direction = EditorGUILayout.ObjectField("Direction", Direction, typeof(GameObject), true) as GameObject;
                if (!A || !B)
                    break;

                if (GUILayout.Button("CaculateDistance"))
                {
                    Distance = Vector3.Distance(A.transform.position, B.transform.position);
                }
                if (GUILayout.Button("CaculateProjectDistance"))
                {
                    var anim = Direction.GetComponent<Animator>();

                    var rf = anim.GetIKPosition(AvatarIKGoal.RightFoot);
                    var lf = anim.GetIKPosition(AvatarIKGoal.LeftFoot);
                    var dir = Direction ? Direction.transform.forward : Vector3.forward;
                    Distance = Vector3.Project(rf - lf, dir).magnitude;

                    // Distance = Vector3.Project(A.transform.position-B.transform.position,dir).magnitude;
                }

                EditorGUILayout.FloatField("Distance",Distance);

                break;
            case Window_Unit.Tool.Protractor:

                EditorGUILayout.BeginVertical();
                GUILayout.Label("Position");
                Position_Start = EditorGUILayout.Vector3Field("Point A", Position_Start);
                Position_Stop = EditorGUILayout.Vector3Field("Point B", Position_Stop);
                Position_Relay = EditorGUILayout.Vector3Field("RelayPoint", Position_Relay);
                EditorGUILayout.EndVertical();
                if (GUILayout.Button("CaculateAngle"))
                {
                    go.Value = Vector3.SignedAngle(v_a, v_b, Vector3.Cross(v_a, v_b));
                }
                if (GUILayout.Button("CaculatePosition"))
                {
                    go.Value = Vector3.Angle(Position_Start - Position_Relay, Position_Stop - Position_Relay);
                }

                break;
        }



    }

    public void OnSceneGUI()
    {
        base.OnInspectorGUI();

        var go = target as Window_Unit;

        switch (go.Operate)
        {
            case Window_Unit.Tool.Ruler:
                break;
            case Window_Unit.Tool.Protractor:
                Handles.color = Color.green;
                Position_Start = Handles.PositionHandle(Position_Start, Quaternion.identity);
                Position_Start = Handles.DoPositionHandle(Position_Start, Quaternion.identity);
                Position_Stop = Handles.DoPositionHandle(Position_Stop, Quaternion.identity);
                Position_Relay = Handles.DoPositionHandle(Position_Relay, Quaternion.identity);
                break;
        }

    }


}
