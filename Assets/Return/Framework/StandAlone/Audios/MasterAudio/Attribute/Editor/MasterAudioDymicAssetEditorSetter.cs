#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor.Validation;
using DarkTonic.MasterAudio;
using System.Linq;


[assembly: RegisterValidator(typeof(MasterAudioDymicAssetEditorSetter))]
[CustomPropertyDrawer(typeof(MasterAudioDymicAssetAttribute))]
public class MasterAudioDymicAssetEditorSetter : AttributeValidator<MasterAudioDymicAssetAttribute, ExposedReference<DynamicSoundGroupCreator>>
{
    protected override void Validate(ValidationResult result)
    {
        if (this.ValueEntry.SmartValue.defaultValue == null)
            return;

        var targetSource = this.ValueEntry.SmartValue.defaultValue as DynamicSoundGroupCreator;

        if (!targetSource)
            return;

        var list = targetSource.musicPlaylists.Select(x => x.playlistName).ToArray();

        var clipId= targetSource.musicPlaylists.Select(x => x.MusicSettings).Select(x=>x.First().clip).ToArray();

        var pathProperty = Property.Parent.FindChild(prop => prop.Name == this.Attribute.AudioListSetter, false);

        if (pathProperty != null)
        {
            pathProperty.ValueEntry.WeakSmartValue = list;
            pathProperty.ValueEntry.ApplyChanges();
        }
        else 
            Debug.Log(pathProperty);

    }

    public override RevalidationCriteria RevalidationCriteria => RevalidationCriteria.OnValueChange;
}

#endif


