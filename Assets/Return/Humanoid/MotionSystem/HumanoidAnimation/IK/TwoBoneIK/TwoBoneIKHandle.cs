using UnityEngine;

namespace Return.Animations.IK
{
    public class TwoBoneIKHandle : NullCheck
    {
        public TwoBoneIKHandle(Transform handBone)
        {
            if (handBone)
                End = handBone;

            if (End)
                Middle = End.parent;

            if (Middle)
                Top = Middle.parent;
        }

        public Transform Top;
        public Transform Middle;
        public Transform End;

        public void Solve(Vector3 ePosition, Quaternion rotation, float weight)
        {
            Quaternion aRotation = Top.rotation;
            Quaternion bRotation = Middle.rotation;
            Quaternion cRotation = End.rotation;
            rotation = Quaternion.Lerp(cRotation, rotation, weight);

            Vector3 aPosition = Top.position;
            Vector3 bPosition = Middle.position;
            Vector3 cPosition = End.position;
            ePosition = Vector3.Lerp(cPosition, ePosition, weight);

            Vector3 ab = bPosition - aPosition;
            Vector3 bc = cPosition - bPosition;
            Vector3 ac = cPosition - aPosition;
            Vector3 ae = ePosition - aPosition;

            float abcAngle = TriangleAngle(ac.magnitude, ab, bc);
            float abeAngle = TriangleAngle(ae.magnitude, ab, bc);
            float angle = (abcAngle - abeAngle) * Mathf.Rad2Deg;
            Vector3 axis = Vector3.Cross(ab, bc).normalized;

            Quaternion fromToRotation = Quaternion.AngleAxis(angle, axis);

            Quaternion worldQ = fromToRotation * bRotation;
            Middle.rotation = worldQ;

            cPosition = End.position;
            ac = cPosition - aPosition;
            Quaternion fromTo = Quaternion.FromToRotation(ac, ae);
            Top.rotation = fromTo * aRotation;
            End.rotation = rotation;
        }



        /// <summary>
        /// Returns the angle needed between v1 and v2 so that their extremities are
        /// spaced with a specific length.
        /// </summary>
        /// <returns>The angle between v1 and v2.</returns>
        /// <param name="aLen">The desired length between the extremities of v1 and v2.</param>
        /// <param name="v1">First triangle edge.</param>
        /// <param name="v2">Second triangle edge.</param>
        private static float TriangleAngle(float aLen, Vector3 v1, Vector3 v2)
        {
            float aLen1 = v1.magnitude;
            float aLen2 = v2.magnitude;
            float c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
            return Mathf.Acos(c);
        }
    }
}