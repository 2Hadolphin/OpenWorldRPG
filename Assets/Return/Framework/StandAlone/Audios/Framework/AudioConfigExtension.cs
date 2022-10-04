#define UniTask

using UnityEngine;

namespace Return.Audios
{ 
    /// <summary>
    /// Extension works with audio module.
    /// </summary>
    public static class AudioConfigExtension
    {
        public static AudioSource LoadConfig(this AudioSource audioSource,AudioSourceConfig config)
        {
            audioSource.playOnAwake = config.PlayOnAwake;
            audioSource.loop = config.Loop;
            audioSource.outputAudioMixerGroup = config.MixerGroup;
            return audioSource;
        }



    }
}