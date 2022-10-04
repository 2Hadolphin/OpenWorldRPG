using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public struct NeoSerializationKey
    {
        private int value;

        public bool isValid
        {
            get { return value != 0; }
        }

        public NeoSerializationKey(string key)
        {
            value = Animator.StringToHash(key);
        }

        public static implicit operator int(NeoSerializationKey key)
        {
            return key.value;
        }
    }
}