using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;

[CustomPropertyDrawer(typeof(IndexAttribute))]
public class IndexDrawer : OdinAttributeDrawer<IndexAttribute,int>
{
    private ValueResolver<int> MaxResolver;

    protected override void Initialize()
    {
        this.MaxResolver = ValueResolver.Get<int>(this.Property, this.Attribute.MaxGetter);
    }


    protected override void DrawPropertyLayout(GUIContent label)
    {
        var rect = EditorGUILayout.GetControlRect();

        //if(null!=label)
        //    rect = EditorGUI.PrefixLabel(rect, label);
        var max = MaxResolver.GetValue();
        var value = this.ValueEntry.SmartValue;

        GUILayout.BeginHorizontal();

        GUILayout.Label(label);

        //GUIHelper.PushLabelWidth(20);
        //value = EditorGUI.IntSlider(rect.AlignLeft(rect.lines * 0.5f), value, 0, max);
        //GUIHelper.PopLabelWidth();
        var layout = GUILayout.Width(80);

        int min = 0;
        if (max <= 0)
        {
            max = 0;
            value = EditorGUILayout.IntField(value,layout);
            //value = EditorGUILayout.IntSlider(value, min, max);
        }
        else
            value = EditorGUILayout.IntSlider(value+1 , 1, max)-1;

        GUILayout.Label(string.Format(" / {0}",max), layout);

        if (GUILayout.Button("Last",layout))
            value--;
        if (GUILayout.Button("Next", layout))
            value++;

        GUILayout.EndHorizontal();

        value = Math.Clamp(value, min, max);


        this.ValueEntry.SmartValue = value;
    }
}


