using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public interface INeoSerializedGameObjectLimiter
    {
        bool restrictChildObjects { get; }
        bool restrictNeoComponents { get; }
        bool restrictOtherComponents { get; }
    }
}