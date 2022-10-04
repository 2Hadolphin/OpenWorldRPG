using System;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Return
{
    public partial class MathfUtility
    {
        #region CSharp

        #region Logic

        /// <summary>
        /// Return angle in 180 ~ -180 degree
        /// </summary>
        public static float WrapAngle(float f)
        {
            return f > 180f ? f - 360f : f;
        }


        public static void WrapAngle(ref float f)
        {
            if (f > 180f)
                f -= 360f;
        }


        /// <summary>
        /// return reverse sign of value
        /// </summary>
        public static float ASign(float f)
        {
            return f > 0 ? -1 : 1;
        }


        public static bool SameSign(float a, float b)
        {
            return a > 0 ? b > 0 : b < 0;
        }

        #endregion

        public static float LerpAngle(float a, float b, float t)
        {
            float max = math.PI * 2f;
            float da = (b - a) % max;

            return a + (2f * da % max - da) * t;
        }

        #region Unit Convert


        /// <summary>
        /// Is A similar to B
        /// </summary>
        /// <param name="value">allowable different value between a and b</param>
        public static bool Similar(float a, float b, float value)
        {
            return b > a ? b - a > value : a - b > value;
        }

        /// <summary>
        /// clamp value in range as a seal loop,"Warning,value can't be mulity than range(1-10<<1000)"
        /// </summary>
        public static void Loop_Clockwise(ref float value, float start, float end)
        {
            if (value > end)
                value = value - end + start;
            else if (value < start)
                value = end - (start - value);
        }


        public static int Loop_Distance_Clockwise(int valueA, int valueB, int start, int end)
        {
            if (valueA > valueB)
                return end - valueA + valueB - start;
            else
                return valueB - valueA;
        }
        public static void Loop_Clockwise(ref int value, int start, int end)
        {
            if (value > end)
                value = value - end + start - 1;
            else if (value < start)
                value = end - (start - value) + 1;
        }

        public static int Loop_Clockwise(int value, int start, int end)
        {
            if (value > end)
                return value - end + start - 1;
            else if (value < start)
                return end - (start - value) + 1;
            else
                return value;
        }

        public static int Loop(int length, int sn)
        {
            if (length == 0)
                return -1;

            var remainder = sn % length;
            if (remainder < 0)
                return remainder + length;
            else
                return remainder;
        }


        #endregion

        #endregion

        #region Vector3
        /// <summary>
        /// Returns a random point within the space defined by the lower and upper bounds.
        /// </summary>
        /// <param name="min">Lower bounds.</param>
        /// <param name="max">Upper bounds.</param>
        public static Vector3 RandomInsideBounds(Vector3 min, Vector3 max)
        {
            return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
        }

        /// <summary>
        /// 	Returns the midpoint between two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector3 Midpoint(Vector3 a, Vector3 b)
        {
            return new Vector3((a.x + b.x) / 2, (a.y + b.y) / 2, (a.z + b.z) / 2);
        }

        public static Vector3 ConvertWorldVectorToVirtualCoordnate(Vector3 virtualCoordnate, Vector3 vector)// z aixs forward
        {
            return Quaternion.FromToRotation(Vector3.forward, virtualCoordnate) * vector;
        }

        #endregion

        #region Quaternion

        /// <summary>
        /// Clamps the given Quaternion's X-axis between the given minimum float and maximum float values.
        /// Returns the given value if it is within the min and max range.
        /// </summary>
        /// <param name="q">Target Quaternion.</param>
        /// <param name="minimum">Minimum float value.</param>
        /// <param name="maximum">Maximum float value.</param>
        /// <returns></returns>
        public static Quaternion ClampRotationAroundXAxis(Quaternion q, float minimum, float maximum)
        {
            q.x /= q.w;
            q.y = 0;
            q.z = 0;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, minimum, maximum);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

        #endregion

        #region Matrix

        public static Matrix4x4 CreateMatrix(Vector3 right, Vector3 up, Vector3 forward, Vector3 position)
        {
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, right);
            m.SetColumn(1, up);
            m.SetColumn(2, forward);
            m.SetColumn(3, position);
            m[3, 3] = 1;
            return m;
        }

        #endregion


        #region Logic


        public static void SetAckerMan(float wheelBase, float turnRadius, float rearTrack, out float farSide, out float closeSide)
        {
            farSide = -Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack / 2));
            closeSide = -Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack / 2));
        }


        #endregion



        #region Math

        public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
        {
            Vector3 c = current.eulerAngles;
            Vector3 t = target.eulerAngles;
            return Quaternion.Euler(
              Mathf.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime),
              Mathf.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime),
              Mathf.SmoothDampAngle(c.z, t.z, ref currentVelocity.z, smoothTime)
            );
        }


        public static Quaternion ClampRotationAroundX_Axis(Quaternion q, float minimum, float maximum)
        {
            q.x /= q.w;
            q.y = 0;
            q.z = 0;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, minimum, maximum);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }


        #endregion


    }
}