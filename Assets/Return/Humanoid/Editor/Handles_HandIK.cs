using Return;
using Return.Humanoid.IK;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

//[CustomEditor(typeof(IK_Hand)),CanEditMultipleObjects]

public class Handles_HandIK : Editor
{
    /*
    protected static IK_Hand _target;
    protected static ReadOnlyTransform tf;
    public Side side=Side.Right;
    public bool FinishClean;
    protected Vector2 scroll;
    protected string ID;
    static BezierData _m=new BezierData();


    protected Dictionary<string, TagT<BezierData>> dictionary=new Dictionary<string, TagT<BezierData>>();
    public override void OnInspectorGUI()
    {
        _target = target as IK_Hand;
        tf = _target.transform;

        _m.StartRotation = Quaternion.identity;
        _m.RelayRotation = Quaternion.identity;
        _m.StopRotation = Quaternion.identity;

        Debug.Log(tf);
        ActiveEditorTracker.sharedTracker.isLocked = true;

        if (Tools.current.HasFlag(Tool.Move))
        {
            PositionHandle_Axis();
        }
        else if (Tools.current.HasFlag(Tool.Rotate))
        {
            RotationHandle();
        }
        PositionHandle();
        DrawBezier();

        if (GUI.changed)
            EditorUtilityTools.SetDirty(target);

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        IK_Hand go = target as IK_Hand;

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        side = (Side)EditorGUILayout.EnumPopup("EditTarget:", side);
        if (side.HasFlag((Side)0))
            go.transform.root.GetComponent<RigBuilder>().StopPreview();
        else
            go.transform.root.GetComponent<RigBuilder>().StartPreview();

        FinishClean = EditorGUILayout.Toggle("ClearData", FinishClean);
        GUILayout.EndHorizontal();

        List<TagT<BezierData>> list;
        IEnumerator<TagT<BezierData>> enumerator;

        if (side.HasFlag(Side.Right))
        {
            list = go.HandData_R;
        }
        else if (side.HasFlag(Side.Left))
        {
            list = go.HandData_L;
        }
        else
        {
            GUILayout.EndVertical();
            return;
        }
        //dictionary = new Dictionary<string, TagT<BezierData>>(list.Count);


        GUILayout.FlexibleSpace();
        ID = EditorGUILayout.TextField("ID",ID);

        _m.StartPoint = EditorGUILayout.Vector3Field("StartPoint", _m.StartPoint);
        _m.StartRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("StartRotation", _m.StartRotation.eulerAngles));

        _m.RelayPoint = EditorGUILayout.Vector3Field("RelayPoint", _m.RelayPoint);
        _m.RelayRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("StartRotation", _m.RelayRotation.eulerAngles));

        _m.StopPoint = EditorGUILayout.Vector3Field("StopPoint", _m.StopPoint);
        _m.StopRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("StartRotation", _m.StopRotation.eulerAngles));



        GUILayout.BeginHorizontal();

        enumerator = list.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var id = enumerator.Current.ID;
            if (dictionary.ContainsKey(id))
                dictionary[id] = enumerator.Current;
            else
                dictionary.Add(id, enumerator.Current);
        }

        if (GUILayout.Button("StorageHand"))
        {
            if (string.IsNullOrEmpty(ID))
                Debug.LogError("Enter Name");
            else
            {
                var newdata = new TagT<BezierData>(ID, _m);

                if (dictionary.TryGetValue(ID, out var editdata))
                {
                    if (list.Contains(editdata))
                        list.Remove(editdata);
                    dictionary[ID] = newdata;
                }
                else
                {
                    dictionary.Add(ID, newdata);
                    if (list.Contains(editdata))
                        list.Remove(editdata);
                }

                list.Add(newdata);
                EditorUtilityTools.SetDirty(go);
                if (FinishClean)
                {
                    Clean();
                }
            }
        }

        if (GUILayout.Button("ReflactData"))
            ReflectData(go);

        if (GUILayout.Button("ClearData"))
        {
            Clean();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginScrollView(scroll);

        enumerator = list.GetEnumerator();
        var delete=new List<TagT<BezierData>>();
        while (enumerator.MoveNext())
        {
            var id = enumerator.Current.ID;
            GUILayout.BeginHorizontal();
            GUILayout.Label(id);
            if (GUILayout.Button("Edit"))
            {
                if (dictionary.TryGetValue(id, out var editData))
                {
                    if (list.Contains(editData))
                    {
                        ID = id;
                        _m.StartPoint = editData.m_Value.StartPoint;
                        _m.RelayPoint = editData.m_Value.RelayPoint;
                        _m.StopPoint = editData.m_Value.StopPoint;

                        _m.StartRotation = editData.m_Value.StartRotation;
                        _m.RelayRotation = editData.m_Value.RelayRotation;
                        _m.StopRotation = editData.m_Value.StopRotation;

                        EditorUtilityTools.SetDirty(this);
                    }

                }
            }
            if (GUILayout.Button("Delete"))
            {
                if (dictionary.TryGetValue(id,out var tagT))
                {
                    delete.Add(tagT);
                    dictionary.Remove(id);
                    EditorUtilityTools.SetDirty(go);
                }
            }
            GUILayout.EndHorizontal();
        }

        enumerator = delete.GetEnumerator();
        while (enumerator.MoveNext())
        {
            list.Remove(enumerator.Current);
        }

        GUILayout.EndScrollView();


        GUILayout.EndVertical();
    


    }
    private void Clean()
    {
        _m = new BezierData();
    }
 
    private void DrawBezier()
    {

        var pos= tf.position;
        UnityHandles.DrawBezier(tf.TransformVector(_m.StartPoint)+pos, tf.TransformVector(_m.StopPoint) +pos, tf.TransformVector(_m.RelayPoint) + pos, tf.TransformVector(_m.RelayPoint) + pos, Color.white, null, 1f);
    }

    
    private void PositionHandle()
    {
        UnityHandles.color = Color.green;
        var 
        v = UnityHandles.FreeMoveHandle(tf.position + tf.TransformVector(_m.StartPoint),
                                  Quaternion.identity,0.07f,Vector3.one,UnityHandles.SphereHandleCap);
        _m.StartPoint = tf.InverseTransformVector(v - tf.position);
   

        UnityHandles.color = Color.red;
        v = UnityHandles.FreeMoveHandle(tf.position + tf.TransformVector(_m.StopPoint),
                                  Quaternion.identity, 0.07f, Vector3.one, UnityHandles.SphereHandleCap);
        _m.StopPoint = tf.InverseTransformVector(v - tf.position);

        UnityHandles.color = Color.yellow;
        v = UnityHandles.FreeMoveHandle(tf.position + tf.TransformVector(_m.RelayPoint),
                                  Quaternion.identity, 0.07f, Vector3.one, UnityHandles.SphereHandleCap);
        _m.RelayPoint = tf.InverseTransformVector(v - tf.position);
    }
    private static void PositionHandle_Axis()
    {
        var rot = tf.rotation;
        Quaternion q;

        q = _m.StartRotation;
        Debug.Log(_m+""+tf);
        var 
        v = UnityHandles.DoPositionHandle(tf.position + tf.TransformVector(_m.StartPoint),rot* q);
        _m.StartPoint = tf.InverseTransformVector(v - tf.position);

        v = UnityHandles.DoPositionHandle(tf.position + tf.TransformVector(_m.StopPoint),
                                  rot * q);
        _m.StopPoint = tf.InverseTransformVector(v - tf.position);

        v = UnityHandles.DoPositionHandle(tf.position + tf.TransformVector(_m.RelayPoint),
                                  rot *q);
        _m.RelayPoint = tf.InverseTransformVector(v - tf.position);
    }

    private void RotationHandle()
    {
        UnityHandles.color = Color.green;
        var r = UnityHandles.DoRotationHandle(tf.rotation*_m.StartRotation, tf.position+tf.TransformVector(_m.StartPoint));
        _m.StartRotation =Quaternion.Inverse(tf.rotation)*r;
    }
    private void ReflectData(IK_Hand go)
    {
        var tf = go.transform;
        var 

        v= -_m.StartPoint;
        v = Vector3.Reflect(v, Vector3.forward);
        _m.StartPoint =v;

        v = -_m.RelayPoint;
        v = Vector3.Reflect(v, Vector3.forward);
        _m.RelayPoint = v;

        v = -_m.StopPoint ;
        v = Vector3.Reflect(v, Vector3.forward);
        _m.StopPoint =v;
    }
    */
}
