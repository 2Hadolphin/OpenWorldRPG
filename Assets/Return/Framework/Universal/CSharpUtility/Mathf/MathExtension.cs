using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

namespace Return
{
    public static partial class MathExtension
    {
        #region Const
        const int Vector3Length = 3;

        #endregion

        #region Float

        public static float Abs(this float f)
        {
            return f > 0 ? f : -f;
        }

        /// <summary>
        /// return sign of value
        /// </summary>
        public static float Sign(this float f)
        {
            if (f == 0)
                return f;

            return f > 0 ? 1 : -1;
        }
        /// <summary>
        /// return reverse sign of value
        /// </summary>
        public static float ASign(this float f)
        {
            if (f == 0)
                return f;

            return f > 0 ? -1 : 1;
        }



        public static float Lerp(this float p, float from, float to)
        {
            return (to - from) * p + from;
        }


        /// <summary>
        /// reture difference between this float and value (always positive)
        /// </summary>
        public static float Difference(this float f, float value)
        {
            if (f > value)
                return f - value;
            else
                return value - f;
        }


        public static byte Random(this byte f, bool canRepeat = false)
        {
            byte random;

            do
            {
                random = (byte)UnityEngine.Random.Range(byte.MinValue, byte.MaxValue);
            }
            while (!canRepeat && random == f);

            return random;
        }


        public static byte Difference(this byte f, byte value)
        {
            if (f > value)
                return (byte)(f - value);
            else
                return (byte)(value - f);
        }

        public static bool IsInteger(this float f)
        {
            return f % 1 == 0;
        }




        public static float Weight(this float p, float min, float max)
        {
            p = p.Clamp(min, max);
            return (p - min) / (max - min);
        }

        public static float Clamp(this float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        /// <summary>
        /// Clamp with positive and negative parameter.
        /// </summary>
        public static float Clamp(this float value, float clamp = 1f)
        {
            if (value < -clamp)
                return -clamp;

            if (value > clamp)
                return clamp;

            return value;
        }

        public static float Clamp01(this float value)
        {
            if (value < 0f)
                return 0f;

            if (value > 1f)
                return 1f;

            return value;
        }


        public static float normalize(this ref float value, float @default = 0)
        {
            if (float.IsInfinity(value))
                return @default;
            else if (float.IsNaN(value))
                return @default;
            else
                return value > 0 ? 1 : -1;
        }

        public static void Normalize(this ref float value, float @default = 0)
        {
            if (float.IsInfinity(value))
                value = @default;
            else if (float.IsNaN(value))
                value = @default;
        }

        public static float Remap_Clamp(this float value, float in_from, float in_to, float out_from, float out_to)
        {
            value = value.Clamp(in_from, in_to);
            return (value - in_from) / (in_to - in_from) * (out_to - out_from) + out_from;
        }

        /// <summary>
        /// CheckAdd value and flow extand value.
        /// </summary>
        public static bool Flow(this float value, float min, float max, out float overflow)
        {
            if (value < min)
            {
                overflow = value - min;
            }
            else if (value > max)
            {
                overflow = value - max;
            }
            else
                overflow = 0;

            return overflow != 0;
        }

        public static float Clamp_flow(this float value, float min, float max, out float Overflow)
        {
            if (value < min)
            {
                Overflow = value - min;
                return min;
            }
            else if (value > max)
            {
                Overflow = value - max;
                return max;
            }
            /*
            else if(min.Equals(0) && max.Equals(0))
            {
                Overflow = value;
                return 0;
            }
            */

            Overflow = 0;
            return value;
        }



        public static float m_Sqrt(this float value, int Times)
        {
            if (0.Equals(value))
                return 0;

            float last = 0f;
            float rest = 1f;

            while (last != rest && Times > 0)
            {
                last = rest;
                Times--;
                rest = (rest + value / rest) / 2;
            }

            return rest;
        }

        public static float d_Sqrt(this float value)
        {
            return (1 + value / 1) / 2;
        }

        /// <summary>
        /// Convert angle inside -180 ~180 degree
        /// </summary>
        public static float WrapAngle(this float f)
        {

            return f > 180f ? f - 360f : f < -180 ? f + 360 : f;
        }

        #endregion

        #region Interge

        public static int Abs(this int f)
        {
            return f > 0 ? f : -f;
        }


        #endregion



        #region Logic

        public static float Max(this float value, float other)
        {
            return value > other ? value : other;
        }

        public static float Min(this float value, float other)
        {
            return value > other ? other : value;
        }

        public static int Max(this int value, int other)
        {
            return value > other ? value : other;
        }

        public static int Min(this int value, int other)
        {
            return value > other ? other : value;
        }

        public static bool IsOdd(this int value)
        {
            return (value & 1) == 1;
        }

        public static int Deg1Pi(this int f)
        {
            return f > 180 ? f - 360 : f < -179 ? f + 360 : f;
        }

        public static float Deg1Pi(this float f)
        {
            return f > 180 ? f - 360 : f <= -180 ? f + 360 : f;
        }

        public static int Deg2Pi(this int f)
        {
            return f < 0 ? f + 360 : f > 359 ? f - 360 : f;
        }

        public static float Deg2Pi(this float f)
        {
            return f < 0 ? f + 360 : f >= 360 ? f - 360 : f;
        }


        #endregion

        public static Vector2 Switch(this Vector2 v)
        {
            return new Vector2(v.y, v.x);
        }



        #region Math


        #region Vector2

        public static Vector2 Repack(this Vector3 vector, SnapAxis axis_First, SnapAxis axis_Second)
        {
            var newVector = new Vector2();

            switch (axis_First)
            {
                case SnapAxis.X:
                    newVector.x = vector.x;
                    break;
                case SnapAxis.Y:
                    newVector.x = vector.y;
                    break;
                case SnapAxis.Z:
                    newVector.x = vector.z;
                    break;
            }

            switch (axis_First)
            {
                case SnapAxis.X:
                    newVector.y = vector.x;
                    break;
                case SnapAxis.Y:
                    newVector.y = vector.y;
                    break;
                case SnapAxis.Z:
                    newVector.y = vector.z;
                    break;
            }

            return newVector;
        }

        /// <summary>
        /// return magnitude ratio 
        /// </summary>
        public static float DistanceRatio(this Vector2 vector, Vector2 vector1)
        {
            return Mathf.Sqrt(vector.sqrMagnitude / vector1.sqrMagnitude);
        }

        public static Vector2 XY(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }


        public static Vector2 Z2Y(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        public static Vector2 Abs(this Vector2 vector)
        {
            if (vector.x < 0)
                vector.x = -vector.x;

            if (vector.y < 0)
                vector.y = -vector.y;

            return vector;
        }

        /// <summary>
        /// (x+y)*0.5f
        /// </summary>
        public static float Avg(this Vector2 vector)
        {
            return (vector.x + vector.y) * 0.5f;
        }

        public static Vector2 Multiply(this Vector2 vector, float parameter)
        {
            vector.x *= parameter;
            vector.y *= parameter;
            return vector;
        }

        public static Vector2 Multiply(this Vector2 vector, float x,float y)
        {
            vector.x *= x;
            vector.y *= y;
            return vector;
        }

        public static Vector2 Multiply(this Vector2 vector, Vector2 parameter)
        {
            vector.x *= parameter.x;
            vector.y *= parameter.y;
            return vector;
        }


        #endregion

        #region Vector3



        public static Vector3 WrapAngle(this Vector3 vector)
        {
            vector.x = MathfUtility.WrapAngle(vector.x);
            vector.y = MathfUtility.WrapAngle(vector.y);
            vector.z = MathfUtility.WrapAngle(vector.z);
            return vector;
        }


        public static Vector3 Clamp_flow(this Vector3 value, Vector3 Min, Vector3 Max, out Vector3 Overflow)
        {
            value.x = value.x.WrapAngle();
            value.y = value.y.WrapAngle();
            value.z = value.z.WrapAngle();
            Vector3 clamp = new Vector3(
                value.x.Clamp(Min.x, Max.x),
                value.y.Clamp(Min.y, Max.y),
                value.z.Clamp(Min.z, Max.z)
                );
            Overflow = value - clamp;
            return clamp;
        }

        /// <summary>
        /// Set vector multiply -1.
        /// </summary>
        public static void ASign_ref(this ref Vector3 vector)
        {
            vector.x *= -1;
            vector.y *= -1;
            vector.z *= -1;
        }

        /// <summary>
        /// Return vector multiply -1.
        /// </summary>
        public static Vector3 ASign(this Vector3 vector)
        {
            vector.x *= -1;
            vector.y *= -1;
            vector.z *= -1;
            return vector;
        }
        public static Vector3 MirrorSign(this Vector3 vector)
        {
            return vector - vector - vector;
        }

        public static Vector3 Y2Z(this Vector2 vector)
        {
            return new Vector3(vector.x, 0, vector.y);
        }


        public static Vector3 Reciprocal(this Vector3 vector)
        {
            return new Vector3(
                1f / vector.x,
                1f / vector.y,
                1f / vector.z
                );
        }

        /// <summary>
        /// Multiply the vector.
        /// </summary>
        public static Vector3 Multiply(this Vector3 vector, float parameter)
        {
            vector.x *= parameter;
            vector.y *= parameter;
            vector.z *= parameter;
            return vector;
        }

        /// <summary>
        /// Multiply the vector.
        /// </summary>
        public static Vector3 Multiply(this Vector3 vector, float x,float y,float z)
        {
            vector.x *= x;
            vector.y *= y;
            vector.z *= z;
            return vector;
        }

        /// <summary>
        /// Multiply the vector.
        /// </summary>
        public static void MultiplyWith(this ref Vector3 vector, float parameter)
        {
            vector.x *= parameter;
            vector.y *= parameter;
            vector.z *= parameter;
        }

        public static Vector3 Multiply(this Vector3 vector, Vector3 parameter)
        {
            vector.x *= parameter.x;
            vector.y *= parameter.y;
            vector.z *= parameter.z;
            return vector;
        }

        /// <summary>
        /// Lerp this vector to target
        /// </summary>
        public static void LerpTo(this ref Vector3 vector, Vector3 target, float p)
        {
            vector = Vector3.Lerp(vector, target, p);
        }

        /// <summary>
        /// Return max magnitude value foreach axis(Abs).
        /// </summary>
        public static Vector3 GetEachMaxWith(this Vector3 a,Vector3 b)
        {
            var length = Vector3Length;
            for (int i = 0; i < length; i++)
            {
                if ((a[i] < 0 ? 0 - a[i] : a[i]) < (b[i] < 0 ? 0 - b[i] : b[i]))
                    a[i] = b[i];
            }
            
            return a;
        }
        /// <summary>
        /// From a to b with RelayPoint by t
        /// </summary>
        public static Vector3 Vector3_Bezier(this Vector3 controll, Vector3 a, Vector3 b, float t)
        {
            var alpha = Vector3.Lerp(a, controll, t);
            var beta = Vector3.Lerp(controll, b, t);
            return Vector3.Lerp(alpha, beta, t);
        }
        /// <summary>
        /// Get vetical point at vector A_B
        /// </summary>
        public static Vector3 GetProjectPoint(this Vector3 value, Vector3 a, Vector3 b)
        {
            return b + Vector3.Project(value - b, a - b);
        }

        public static float GetLength(this Vector3 vector)
        {
            return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }
        public static float GetLength_Pow(this Vector3 vector)
        {
            return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
        }

        public static Vector3 Clamp(this Vector3 value, Vector3 Min, Vector3 Max)
        {
            value.x = value.x.WrapAngle();
            value.y = value.y.WrapAngle();
            value.z = value.z.WrapAngle();

            return new Vector3(
            value.x < Min.x ? Min.x : value.x > Max.x ? Max.x : value.x,
            value.y < Min.y ? Min.y : value.y > Max.y ? Max.y : value.y,
            value.z < Min.z ? Min.z : value.z > Max.z ? Max.z : value.z
            );

            /*
            return new Vector3(
                value.x.Clamp(Min.x, Max.x),
                value.y.Clamp(Min.y, Max.y),
                value.z.Clamp(Min.z, Max.z)
                );
                */
        }

        public static Vector3 ClampRotation(this Vector3 direction,Quaternion coordinate,Vector3 min,Vector3 max)
        {
            var local =  Quaternion.Inverse(coordinate)* direction;

            return coordinate*local.Clamp(min, max);
        }

        #endregion
        #endregion


        #region Quaternion

        public static Quaternion ToEuler(this Vector3 value)
        {
            return Quaternion.Euler(value);
        }

        public static Quaternion Clamp(this Quaternion q, Vector3 Min, Vector3 Max)
        {
            Vector3 value = (q.eulerAngles);

            return Quaternion.Euler(
                value.x < Min.x ? Min.x : value.x > Max.x ? Max.x : value.x,
                value.y < Min.y ? Min.y : value.y > Max.y ? Max.y : value.y,
                value.z < Min.z ? Min.z : value.z > Max.z ? Max.z : value.z
            );
        }

        public static bool IsNaN(this Quaternion q)
        {
            if (float.IsNaN(q.x))
                return true;
            else if (float.IsNaN(q.y))
                return true;
            else if (float.IsNaN(q.z))
                return true;
            else if (float.IsNaN(q.w))
                return true;

            return false;
        }



        /*
        public static void Deg1Pi(this ref int f)
        {
            f = f > 180 ? f - 360 : f < -180 ? f + 360 : f;
        }

        public static void Deg1Pi(this ref float f)
        {
            f = f > 180 ? f - 360 : f < -180 ? f + 360 : f;
        }

        public static void Deg2Pi(this ref int f)
        {
            f = f < 0 ? f + 360 : f > 360 ? f - 360 : f;
        }

        public static void Deg2Pi(this ref float f)
        {
            f = f < 0 ? f + 360 : f > 360 ? f - 360 : f;
        }
        */
       
        /*
        public static Quaternion Clamp(this Quaternion value, Vector3 Min, Vector3 Max)
        {
            Vector3 rot= value.eulerAngles;

            rot.x = rot.x > 180 ? rot.x - 360 : rot.x;
            rot.y = rot.y > 180 ? rot.y - 360 : rot.y;
            rot.z = rot.z > 180 ? rot.z - 360 : rot.z;

            rot = new Vector3(
                rot.x.Clamp(Min.x, Max.x),
                rot.y.Clamp(Min.y, Max.y),
                rot.z.Clamp(Min.z, Max.z)
                );

            return Quaternion.Euler(rot);
        }

        public static Quaternion Clamp_none_gift(this Quaternion value, Vector3 Min, Vector3 Max)
        {
            Vector3 rot = value.eulerAngles;

            if (rot.x > 180)
                rot.x -= 360;
            if (rot.y > 180)
                rot.y -= 360;
            if (rot.z > 180)
                rot.z -= 360;

            rot = new Vector3(
                rot.x.Clamp(Min.x, Max.x),
                rot.y.Clamp(Min.y, Max.y),
                rot.z.Clamp(Min.z, Max.z)
                );

            return Quaternion.Euler(rot);
        }
        */
        /*
        public static Quaternion Clamp(this Quaternion value, Vector3 Min, Vector3 Max)
        {
            Vector3 rot = value.eulerAngles;

            if (rot.x > 180)
                rot.x -= 360;
            if (rot.y > 180)
                rot.y -= 360;
            if (rot.z > 180)
                rot.z -= 360;


            rot = new Vector3(
        rot.x < Min.x ? Min.x : rot.x > Max.x ? Max.x : rot.x,
        rot.y < Min.y ? Min.y : rot.y > Max.y ? Max.y : rot.y,
        rot.z < Min.z ? Min.z : rot.z > Max.z ? Max.z : rot.z
        );

            return Quaternion.Euler(rot);
        }

        public static Quaternion Clamp_flow(this Quaternion value, Vector3 Min, Vector3 Max)
        {
            Vector3 rot = value.eulerAngles;
            rot = new Vector3(
                rot.x.Clamp(Min.x, Max.x),
                rot.y.Clamp(Min.y, Max.y),
                rot.z.Clamp(Min.z, Max.z)
                );

            return Quaternion.Euler(rot);
        }
        */


        public static bool IsValid(this Quaternion q)
        {
            if (q.w.Equals(0))
                if ((q.x + q.y + q.z).Equals(0))
                    return false;

            return true;
        }

        /// <summary>
        /// Push back rotation of parent
        /// </summary>
        public static Quaternion SolveParent(this Quaternion globalRotation, Quaternion localRotation)
        {
            return globalRotation * Quaternion.Inverse(localRotation);
        }

        public static void ReflectRotation(this ref Quaternion source, Vector3 normal)
        {
            source = Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
        }

        public static Quaternion Reflect(this Quaternion source, Vector3 normal)
        {
            return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
        }
         #endregion
    }
}