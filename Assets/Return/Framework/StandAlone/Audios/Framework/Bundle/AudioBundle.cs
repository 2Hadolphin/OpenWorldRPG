using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Database;
using Sirenix.OdinInspector;

namespace Return.Audios
{
    public partial class AudioBundle : PresetDatabase
    {

#if UNITY_EDITOR
        [SerializeField]
        [OnValueChanged(nameof(ResetTemplate),IncludeChildren =true)]
        UTagPicker m_Tag;

        public UTagPicker Tag => m_Tag;

        /// <summary>
        /// Editor template to quick push 
        /// </summary>
        [SerializeField]
        AudioTemplate m_Template;

        void ResetTemplate()
        {
            if (m_Template.IsNull())
                return;

            if (m_Tag.Definition.IsNull())
                return;


            foreach (var preset in Presets)
            {
                var newPicker = m_Tag;
                newPicker.Tag = preset.Tag.Tag;
                preset.Tag = newPicker;
            }
        }

        void AddPreset(Sirenix.OdinInspector.Editor.InspectorProperty obj)
        {
            Debug.Log(obj);
            var tag = m_Tag;
            tag.HideDefinition = true;
            var preset = new AudioAssetList<AudioPreset>() { Tag =tag, Template=m_Template };

            Presets.Add(preset);
        }

        [PropertySpace(5, 5)]
        [ListDrawerSettings(CustomAddFunction =nameof(AddPreset),Expanded =true,ShowPaging =true,NumberOfItemsPerPage =1)]
#endif
        public List<AudioAssetList<AudioPreset>> Presets=new ();

        public virtual AudioClip Random()
        {
            return Presets.Random().Assets.Random().Asset;
        }

        public void GetPreset(UTag tag,out AudioPreset preset,out AudioTemplate template)
        {
            foreach (var asset in Presets)
            {
                if (asset.Tag.Tag.HasFlag(tag))
                {
                    preset = asset.Assets.Random();
                    template = asset.Template;
                    return;
                }
            }
            preset = null;
            template = null;
            Debug.LogError(new KeyNotFoundException(tag.ToString()));
        }
    }


    [System.Serializable]
    public struct BundleSelecter
    {
        public static implicit operator bool (BundleSelecter selecter)
        {
            return selecter.Bundle;
        }

#if UNITY_EDITOR

        bool HasBundle => Bundle;

        void LoadBundle()
        {
            if (HasBundle)
            {
                TagPicker = Bundle.Tag;
            }
        }
        [OnValueChanged(nameof(LoadBundle))]
#endif
        [SerializeField]
        public AudioBundle Bundle;


#if UNITY_EDITOR
        [ShowIf(nameof(HasBundle))]
#endif
        [SerializeField]
        public UTagPicker TagPicker;

        /// <summary>
        /// Return audio source and template via selected tag.
        /// </summary>
        /// <param name="preset">Audio clip wrapper.</param>
        /// <param name="template">Audio template.</param>
        public void GetPreset(out AudioPreset preset, out AudioTemplate template)
        {
            Bundle.GetPreset(TagPicker, out preset, out template);
        }



    }
}