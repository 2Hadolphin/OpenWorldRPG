using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;


public class MasterAudioDymicAssetAttribute : PropertyAttribute
{
    public string AudioListSetter;
    
    public MasterAudioDymicAssetAttribute(string audioListSetter)
    {
        AudioListSetter = audioListSetter;
    }
}

