using System;

namespace NeoSaveGames.Serialization
{
    [Flags]
    public enum PropertyFlags : byte
    {
        None,
        IsArray = 1,
        Unnammed = 2,
        NullOrEmpty = 4
    }
}
