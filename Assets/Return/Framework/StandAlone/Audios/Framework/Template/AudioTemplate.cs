using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Database;
using Sirenix.OdinInspector;
using System;

namespace Return.Audios
{
    /// <summary>
    /// Audio source wrapper.    **Go abract??
    /// </summary>
    public class AudioTemplate : PresetDatabase
    {
        [Obsolete]
        [ShowInInspector]
        public const string AudioTemplateFolder = "Resources/Audios/m_Template";

        [Tooltip("Number of permanent audio source (can be remap by AudioManager).")]
        public int MaxPoolNumber=1;

#if UNITY_EDITOR
        void LoadNewConfig()
        {
            if(AudioSource)
                Config = new AudioSourceConfig(AudioSource);
        }

        [OnValueChanged(nameof(LoadNewConfig))]
#endif
        public AudioSource AudioSource;

#if UNITY_EDITOR

        void LoadConfig()
        {
            if (Config && AudioSource)
                AudioSource.LoadConfig(Config);
        }

        [OnValueChanged(nameof(LoadConfig),IncludeChildren =true,InvokeOnInitialize =false,InvokeOnUndoRedo =true)]
        [BoxGroup]
#endif
        [SerializeField]
        public AudioSourceConfig Config;


#if UNITY_EDITOR

        [Button]
        void CreateTemplate()
        {
            if (AudioSource)
            {
                AudioSource = Return.Editors.EditorAssetsUtility.CreatePrefab(AudioSource, new Editors.AssetPathCatch(this));
            }
            else
            {
                var source = new GameObject(Title).AddComponent<AudioSource>();
                AudioSource = Return.Editors.EditorAssetsUtility.CreatePrefab(source, new Editors.AssetPathCatch(this));
                DestroyImmediate(source.gameObject);
            }


            AudioSource.playOnAwake = Config.PlayOnAwake;
            AudioSource.loop = Config.Loop;
        }


#endif
    }
}