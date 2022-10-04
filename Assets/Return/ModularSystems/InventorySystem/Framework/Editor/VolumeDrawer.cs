using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Return.Framework.Grids;
using UnityEditor;

namespace Return.Editors
{
    public class VolumeDrawer : OdinValueDrawer<Volume>
    {
        const byte max = 16;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var volume = ValueEntry.SmartValue;

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();
            //volume.width = (byte)EditorGUILayout.IntSlider(volume.width, 1, max);
            //volume.height = (byte)EditorGUILayout.IntSlider(volume.height, 1, max);
            volume = EditorGUILayout.Vector2IntField("Volume Size",volume,GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                if (volume.width < 1)
                    volume.width = 1;

                if (volume.height < 1)
                    volume.height = 1;

                ValueEntry.SmartValue = volume;
                ValueEntry.ApplyChanges();
            }

        }

    }
}