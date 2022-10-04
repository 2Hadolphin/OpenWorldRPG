using UnityEngine;

namespace Return
{
    public interface IAdditiveTransform
    {
        bool enabled { get; set; }

        bool bypassPositionMultiplier { get; }
        bool bypassRotationMultiplier { get; }

        Vector3 position { get; }
        Quaternion rotation { get; }

        void UpdateTransform();
    }
}
