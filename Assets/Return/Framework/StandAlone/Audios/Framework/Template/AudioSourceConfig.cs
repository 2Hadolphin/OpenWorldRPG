using UnityEngine;
using UnityEngine.Audio;
using System;

namespace Return.Audios
{

    /// <summary>
    /// Audio source config data.
    /// </summary>
    [Serializable]
    public class AudioSourceConfig:NullCheck
    {
        public AudioSourceConfig(AudioSource audioSource)
        {
            PlayOnAwake = audioSource.playOnAwake;
            Loop = audioSource.loop;
            MixerGroup = audioSource.outputAudioMixerGroup;
        }



        public bool PlayOnAwake;
        public bool Loop;

        public AudioMixerGroup MixerGroup;

#if UNITY_EDITOR
        public bool HideInHierarchy=true;
#endif
    }
}