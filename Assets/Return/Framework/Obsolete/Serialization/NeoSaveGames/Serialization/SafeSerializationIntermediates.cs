using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    [Serializable]
    public struct IntermediateVector2
    {
        public float x;
        public float y;

        public IntermediateVector2(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }

        public static implicit operator Vector2(IntermediateVector2 v)
        {
            return new Vector2(v.x, v.y);
        }
    }

    [Serializable]
    public struct IntermediateVector3
    {
        public float x;
        public float y;
        public float z;

        public IntermediateVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static implicit operator Vector3(IntermediateVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
    }

    [Serializable]
    public struct IntermediateVector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public IntermediateVector4(Vector4 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            w = v.w;
        }

        public IntermediateVector4(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public IntermediateVector4(Color c)
        {
            x = c.r;
            y = c.g;
            z = c.b;
            w = c.a;
        }

        public static implicit operator Vector4(IntermediateVector4 v)
        {
            return new Vector4(v.x, v.y, v.z, v.w);
        }

        public static implicit operator Quaternion(IntermediateVector4 v)
        {
            return new Quaternion(v.x, v.y, v.z, v.w);
        }

        public static implicit operator Color(IntermediateVector4 v)
        {
            return new Color(v.x, v.y, v.z, v.w);
        }
    }

    [Serializable]
    public struct IntermediateVector2Int
    {
        public int x;
        public int y;

        public IntermediateVector2Int(Vector2Int v)
        {
            x = v.x;
            y = v.y;
        }

        public static implicit operator Vector2Int(IntermediateVector2Int v)
        {
            return new Vector2Int(v.x, v.y);
        }
    }

    [Serializable]
    public struct IntermediateVector3Int
    {
        public int x;
        public int y;
        public int z;

        public IntermediateVector3Int(Vector3Int v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static implicit operator Vector3Int(IntermediateVector3Int v)
        {
            return new Vector3Int(v.x, v.y, v.z);
        }
    }

    [Serializable]
    public struct IntermediateColor32
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public IntermediateColor32(Color32 c)
        {
            r = c.r;
            g = c.g;
            b = c.b;
            a = c.a;
        }

        public static implicit operator Color32(IntermediateColor32 c)
        {
            return new Color32(c.r, c.g, c.b, c.a);
        }
    }
}