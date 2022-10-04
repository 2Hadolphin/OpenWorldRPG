using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using TNet;
using System.IO;

namespace Return
{
    public class MonoPreset : PresetDatabase
    {
        public Type Type;

        public bool ShowData;

        [ShowIf(nameof(ShowData))]
        public DataNode Node;

        [ShowInInspector]
        [OnValueChanged(nameof(SaveData))]
        public Component SavePreset;

        public virtual void SaveData(Component component)
        {
            //using (var bw = new BinaryWriter())
            //{
            //    Serialization.Write(bw,component);

            //}
#if UNITY_EDITOR
            if (null == Node)
                Node = new DataNode();

            if (!component && SavePreset)
            {
                component = SavePreset;
                SavePreset = null;
            }

            if (!component)
                return;

            UnityEditor.EditorUtility.DisplayCancelableProgressBar("Working", "Creating a DataNode...", 0f);
            ComponentSerialization.ClearReferences();
            component.Serialize(Node);
            ComponentSerialization.ClearReferences();
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }


    }
}