using System;

namespace NeoSaveGames.Serialization
{
    public enum PropertyType : byte
    {
        EndOfData,
        NextBuffer,
        PushContext,
        PopContext,
        Bool,
        Byte,
        SignedByte,
        Char,
        Short,
        UnsignedShort,
        Int,
        UnsignedInt,
        Long,
        UnsignedLong,
        Float,
        Double,
        String,
        Vector2,
        Vector3,
        Vector4,
        Vector2Int,
        Vector3Int,
        Quaternion,
        Color,
        Color32,
        Guid,
        DateTime,
        Serializable
    }
}
