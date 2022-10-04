using Return;
using UnityEngine;

namespace Return.Cameras
{
    public interface IAdditiveTransformHandler
    {
        float springPositionMultiplier
        {
            get;
            set;
        }

        float springRotationMultiplier
        {
            get;
            set;
        }

        void ApplyAdditiveEffect(IAdditiveTransform add);
        void RemoveAdditiveEffect(IAdditiveTransform add);

        T GetAdditiveTransform<T>() where T : class, IAdditiveTransform;
        T[] GetAdditiveTransforms<T>() where T : class, IAdditiveTransform;

        GameObject gameObject { get; }
    }
}