using Return.Animations;
using System;
using UnityEngine;

namespace Return.Items
{
    [Serializable]
    public class PostureRotationData: HandPostureData
    {
        public Vector3 Rotation;

        public bool TryGetJob(Animator animator,out StreamAdditiveRotationHandle job)
        {
            var valid = TryGetHandle(animator, out var handle);
            
            if (valid)
            {
                job = new StreamAdditiveRotationHandle() { Handle = handle, Rotation = Rotation.ToEuler() };                
            }
            else
            {
                job = default;
            }

            return valid;
        }
    }
}

