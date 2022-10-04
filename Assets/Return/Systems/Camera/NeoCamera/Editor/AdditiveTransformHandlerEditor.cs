#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Return.Cameras;

namespace NeoFPSEditor
{
    //[CustomEditor(typeof(AdditiveTransformHandler))]
    public class AdditiveTransformHandlerEditor : Editor
    {
        private void OnSceneGUI()
        {
            Color c = Handles.color;
            var cast = target as AdditiveTransformHandler;

            var scale = cast.transform.lossyScale;
            var pivotOffset = serializedObject.FindProperty("m_PivotOffset").vector3Value;
            pivotOffset.x *= scale.x;
            pivotOffset.y *= scale.y;
            pivotOffset.z *= scale.z;

            Vector3 center = cast.transform.position + (cast.transform.rotation * pivotOffset);

            Handles.color = new Color(1f, 0f, 0f, 0.25f);
            Handles.DrawSolidArc(center, cast.transform.right, cast.transform.forward, 22.5f, 0.05f);
            Handles.DrawSolidArc(center, cast.transform.right, cast.transform.forward, -22.5f, 0.05f);

            Handles.color = new Color(0, 1f, 0f, 0.25f);
            Handles.DrawSolidArc(center, cast.transform.up, cast.transform.forward, 22.5f, 0.05f);
            Handles.DrawSolidArc(center, cast.transform.up, cast.transform.forward, -22.5f, 0.05f);

            Handles.color = new Color(0f, 0f, 1f, 0.25f);
            Handles.DrawSolidDisc(center, cast.transform.forward, 0.01f);

            Handles.color = Color.red;
            Handles.DrawLine(center, center + cast.transform.right * 0.05f);
            Handles.color = Color.green;
            Handles.DrawLine(center, center + cast.transform.up * 0.05f);
            Handles.color = Color.blue;
            Handles.DrawLine(center, center + cast.transform.forward * 0.05f);

            Gizmos.color = c;
        }
    }
}

#endif