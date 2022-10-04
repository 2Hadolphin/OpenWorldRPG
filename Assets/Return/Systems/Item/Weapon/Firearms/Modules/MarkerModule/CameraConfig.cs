using UnityEngine;
using Sirenix.OdinInspector;

namespace Return.Items.Weapons
{
    /// <summary>
    /// Offset and Fov
    /// </summary>
    [HideLabel]
    [System.Serializable]
    public class CameraConfig
    {
        [SerializeField]
        public ValueSlider Fov;// = new() { Max = 77, Min = 40,Fov=55,FovRange=new(40,77) };


        [SerializeField, BoxGroup("Offset"), MinMaxSlider(-0.15f, 0.15f)]
        public Vector2 X = new(-0.15f, 0.15f);
        [SerializeField, BoxGroup("Offset"), MinMaxSlider(-0.15f, 0.15f)]
        public Vector2 Y = new(-0.15f, 0.15f);
        [SerializeField, BoxGroup("Offset"), MinMaxSlider(-0.15f, 0.15f)]
        public Vector2 Z = new(-0.15f, 0.15f);

        public Vector3 GetOffset(float t=0.5f)
        {
            return new Vector3(
                Mathf.Lerp(X.x,X.y,t),
                                Mathf.Lerp(Y.x, Y.y, t),
                                                Mathf.Lerp(Z.x, Z.y, t)
                //X.width.Lerp(X.height,t),
                //Y.width.Lerp(Y.height, t),
                //Z.width.Lerp(Z.height, t)
                );
        }


#if UNITY_EDITOR
        [BoxGroup("Debug")]
        [OnValueChanged(nameof(BuildGizmosHandle))]
        public GameObject GizmosDrawHandle;

        bool OnGizmos;

        void BuildGizmosHandle()
        {
            OnGizmos = GizmosDrawHandle;

            if (!OnGizmos)
                return;

            GizmosUtil.OnGizmos += DrawGizmos;
        }


        [BoxGroup("Debug")]
        [ShowIf(nameof(OnGizmos))]
        [OnValueChanged(nameof(EvaluateOffset))]
        [Range(0,1f)]
        public float GizmosSlider;

        [BoxGroup("Debug")]
        [ShowIf(nameof(OnGizmos))]
        public Vector3 Offset;

        [BoxGroup("Debug")]
        [Button]
        void SetToSceneCameraPosition()
        {
            var pos=UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
            Offset = GizmosDrawHandle.transform.InverseTransformPoint(pos);
        }


        void EvaluateOffset()
        {
            Offset = GetOffset(GizmosSlider);
        }

        void DrawGizmos()
        {
            if (!GizmosDrawHandle)
                return;

            var tf = GizmosDrawHandle.transform;

        

            Gizmos.color = Color.red;
            Gizmos.DrawLine(tf.position,tf.TransformPoint(Offset));
            
        }

#endif

    }
}
