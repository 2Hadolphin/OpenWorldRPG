using UnityEngine;
using UnityEngine.Animations;

namespace Return.Items
{
    public abstract class BasePoseAdjust : NullCheck
    {
        public TransformStreamHandle StreamHandle;

        public abstract void ProcessAnimationStream(AnimationStream stream);

        public virtual bool Valid(AnimationStream stream)
        {
            var valid= (StreamHandle.IsValid(stream));

            if (!valid)
                Debug.LogError("Invalid handle : "+StreamHandle);

            return valid;
        }
           
    }
}

