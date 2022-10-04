using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return;

namespace Return.Audios
{
    [System.Serializable]
    public class AudioAssetList<T>
    {
        [SerializeField]
        public UTagPicker Tag;
        public AudioTemplate Template;
        public List<T> Assets = new();

#if UNITY_EDITOR
        [DropZone(typeof(UnityEngine.Object), nameof(LoadClip))]
        [ShowInInspector]
        [OnValueChanged(nameof(LoadClip))]
        UnityEngine.Object @object;

        protected void LoadClip()
        {
            var a = @object;
            @object = null;

            Debug.Log(a);

            if (a is T preset)
            {
                if (!Assets.Contains(preset))
                    Assets.Add(preset);
            }
        }
#endif
    }

}