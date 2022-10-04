using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Database;
using Return.Audios;

namespace Return.Inventory
{
    public class InventoryEffects : PresetDatabase
    {
        // audio
        [SerializeField]
        AudioBundle m_onDrag_audio;

        [SerializeField]
        AudioBundle m_onDrop_audio;

        public AudioBundle OnDrag_audio { get => m_onDrag_audio; set => m_onDrag_audio = value; }
        public AudioBundle OnDrop_audio { get => m_onDrop_audio; set => m_onDrop_audio = value; }

        // vfx
    }
}