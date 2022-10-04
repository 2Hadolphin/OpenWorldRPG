using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtension
{
	public static Quaternion Add(this Quaternion q1, Quaternion q2)
	{
		return new Quaternion(q1.x + q2.x, q1.y + q2.y, q1.z + q2.z, q1.w + q2.w);
	}

	public static Quaternion Normalize(this Quaternion q)
	{
		float len = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);

		q.x /= len;
		q.y /= len;
		q.z /= len;
		q.w /= len;

		return q;
	}

	public static Quaternion Multiply(this Quaternion q, float s)
	{
		q.x *= s;
		q.y *= s;
		q.z *= s;
		q.w *= s;
		return q;
	}

	public static Quaternion ScaleRotation(this Quaternion rot, float scale)
	{
        // Get an angle axis rotation for the quaternion
        rot.ToAngleAxis(out float angle, out Vector3 axis);

        // Scale the angle
        angle *= scale;

		// Return a new quaternion
		return Quaternion.AngleAxis(angle, axis);
	}
}
