using UnityEngine;
using System;

using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;
using System.Linq;
using Return;
using UnityEngine.Audio;
using Return.Audios;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DynamicSoundConfig:NullCheck//,ISerializationCallbackReceiver
{
#if UNITY_EDITOR
    [PropertyOrder(-1)]
    [ShowInInspector]
    [OnValueChanged(nameof(LoadPreset))]
    [PropertySpace(5,5)]
    [HorizontalGroup("Config")]
    public DynamicSoundGroupCreator m_AudioPreset
    {
        get => AudioPreset.defaultValue as DynamicSoundGroupCreator;
        set => AudioPreset.defaultValue = value;
    }

    /// <summary>
    /// 
    /// </summary>
    [HorizontalGroup("Config")]
    [Button("Refresh",ButtonSizes.Small,ButtonStyle.Box)]
    public void LoadPreset()
    {
        if(m_AudioPreset)
            AudioPreset.defaultValue = m_AudioPreset;

        if (m_AudioPreset)
        {
            AllLists = m_AudioPreset.musicPlaylists.Select(x => x.playlistName).ToArray();

            if(string.IsNullOrEmpty(AudioList))
                AudioList = AllLists.FirstOrDefault();

            if (!string.IsNullOrEmpty(AudioList))
            {
                Clips = m_AudioPreset.musicPlaylists.Where(x => x.playlistName == AudioList).FirstOrDefault()?.MusicSettings.Select(x => x.songName).ToArray();
                if (string.IsNullOrEmpty(ClipID))
                {
                    ClipID = Clips.FirstOrDefault();
                    LoadClip();
                }
            }
        }
        else
        {
            // clean ?
        }
    }

    public void LoadClip()
    {
        var setting = m_AudioPreset.musicPlaylists.Where(x => x.playlistName == AudioList).First()?.MusicSettings.Where(x => x.songName == ClipID).First();
       
        if (setting != null)
        {
            switch (setting.audLocation)
            {
                case MasterAudio.AudioLocation.Clip:
                    AudioClip = setting.clip;
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    AudioClip = Resources.Load<AudioClip>(setting.resourceFileName);
                    break;
            }
        }
    }

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        LoadPreset();
    }

    [HideInInspector]
    [NonSerialized]
    public string[] AllLists;

    [HideInInspector]
    [NonSerialized]
    public string[] Clips;

#endif


    [SerializeField]
    [HideInInspector]
    public ExposedReference<DynamicSoundGroupCreator> AudioPreset;

    public ExposedReference<AudioTemplate> AudioTemplate;
    public ExposedReference<AudioMixerGroup> MixedGroup;

#if UNITY_EDITOR
    [ValueDropdown(nameof(AllLists))]
    [PropertySpace(5, 5)]
#endif
    public string AudioList;

#if UNITY_EDITOR
    [OnValueChanged(nameof(LoadClip))]
    [ValueDropdown(nameof(Clips))]
    [PropertySpace(5, 5)]
#endif
    public string ClipID;

    [Obsolete("Change to master audio play list")]
    [PropertyOrder(1)]
    [HideInInspector]
    [ReadOnly]
    public AudioClip AudioClip;

    [HideInInspector]
    [Obsolete("Change to master audio play list")]
    [Range(0,2f)]
    public float Volume=1f;
}


