#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(ControllerPhysicCaculator))]
public class ControllerSetting : Editor
{
    private SerializedObject Caculator;
    private SerializedProperty controller, CollisionType, SphereRadius, CapsuleSize, Custome, GroundedTF, TargetLayer;
    private void OnEnable()
    {
        Caculator = new SerializedObject(target);
        controller = Caculator.FindProperty("controller");
        CollisionType = Caculator.FindProperty("PhysicColliderType");
        SphereRadius = Caculator.FindProperty("Radius");
        CapsuleSize = Caculator.FindProperty("Size");
        Custome = Caculator.FindProperty("TargetCollider");
        GroundedTF = Caculator.FindProperty("GroundedTF");

        TargetLayer = Caculator.FindProperty("TargetLayer");
    }

    public Texture Icon;

    public override void OnInspectorGUI()
    {
        Caculator.Update();

        EditorGUILayout.PropertyField(controller);
        EditorGUILayout.PropertyField(CollisionType);
        EditorGUILayout.PropertyField(GroundedTF);

        EditorGUILayout.PropertyField(TargetLayer);

        switch (CollisionType.enumValueIndex)
        {
            case 0:
                EditorGUILayout.PropertyField(SphereRadius);
                break;
            case 1:
                EditorGUILayout.PropertyField(CapsuleSize);
                break;
            case 2:
                EditorGUILayout.PropertyField(Custome);
                break;
            default:
                break;
        }



        Caculator.ApplyModifiedProperties();
    }
}


#endif