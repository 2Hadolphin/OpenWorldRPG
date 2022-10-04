using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DarkTonic.MasterAudio;

namespace Return.Items.Weapons
{
    [Obsolete]
    public interface IFirearmsAudioPort
    {
        public event Action OnPlayAudio;
    }
}
