using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Humanoid.Motion;
using Return.Cameras;

namespace Return.Items
{

    /// <summary>
    /// Apply **motion inertia 
    /// </summary>
    public class AdditivePlayer : AdditiveTransformHandler
    {
        [Tooltip("Transform to apply additive.")]
        public Transform OriginHandle;

        /// <summary>
        /// Non repeat or child must be root effectived target
        /// </summary>
        [SerializeField]
        protected List<Transform> EffectiveHandles;


        protected override void ApplyAdditive(PR pr)
        {
            base.ApplyAdditive(pr);
            ProccessAdditiveEffect(pr);
        }

     
        public virtual void ProccessAdditiveEffect(PR pr)
        {
            //Debug.Log(pr.GUIDs +" ** "+ pr.eulerAngles);

            if (pr.Equals(PR.Default))
                return;

            var length = EffectiveHandles.Count;

            for (int i = 0; i < length; i++)
            {
                var handle = EffectiveHandles[i];

                if (handle == OriginHandle)
                    continue;

                // turn handle pos to target space and apply additive pos
                var pos = OriginHandle.InverseTransformPoint(handle.position);
                pos += pr;
                pos = pr.Rotation * pos;
                pos = OriginHandle.TransformPoint(pos);

                var rot = OriginHandle.InverseTransformRotation(handle.rotation);
                rot = pr.Rotation*rot;
                rot = OriginHandle.TransformRotation(rot);

                handle.SetPositionAndRotation(pos, rot);
            }
            //Debug.Log(pr.ToString());
            OriginHandle.SetAdditiveLocalPR(pr);
        }
    }
}