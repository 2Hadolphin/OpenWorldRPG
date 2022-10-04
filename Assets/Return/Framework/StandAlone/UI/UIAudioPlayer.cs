using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class UIAudioPlayer : MonoBehaviour
{
    [SerializeField]
    public mUIPrefabs Preset;

    public float Gap=0.07f;

    protected static mUIPrefabs CurrentPreset;
    protected static AudioSource m_UIAudioSource;
    protected static float During;
    protected static float TargetGap;
    protected static bool CanPlay;
    public static AudioSource AudioSource
    {
        get
        {
            if (!m_UIAudioSource)
            {
                m_UIAudioSource = new GameObject("UIAudioSource").AddComponent<AudioSource>();
                m_UIAudioSource.hideFlags = HideFlags.DontSaveInEditor;
            }

            return m_UIAudioSource;
        }
    }

    public static void PlayUIPassBy()
    {
        if (CanPlay)
            Play(CurrentPreset.UIElementPassby);
    }

    public static void PlayUIClick()
    {
        if(CanPlay)
            Play(CurrentPreset.UIElementClick);
    }

    public static void Play(AudioClip clip)
    {
        AudioSource.PlayOneShot(clip);
        During = 0;
    }


    private void Awake()
    {
        if (m_UIAudioSource)
            DestroyImmediate(this);

        if (!gameObject.TryGetComponent<AudioSource>(out var source))
            AudioSource.playOnAwake = false;
        else
            m_UIAudioSource = source;

        hideFlags = HideFlags.NotEditable;

        CurrentPreset = Preset;

        TargetGap = Gap;
        During = TargetGap;
    }

    private void Update()
    {
        CanPlay = During >= TargetGap;

        if (!CanPlay)
            During += Time.deltaTime;

    }

}
