using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(m_MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var minMaxAttribute = (m_MinMaxSliderAttribute)attribute;
        var propertyType = property.propertyType;

        if (minMaxAttribute.Tooltip == "")
            label.tooltip = "The minimum and maximum " + label.text + " values.";
        else
            label.tooltip = minMaxAttribute.Tooltip;

        Rect controlRect = EditorGUI.PrefixLabel(position, label);

        Rect[] splittedRect = SplitRect(controlRect, 3);

        switch (propertyType)
        {
            case SerializedPropertyType.Vector2:
                {
                    EditorGUI.BeginChangeCheck();

                    Vector2 vector = property.vector2Value;
                    float minVal = vector.x;
                    float maxVal = vector.y;

                    minVal = EditorGUI.FloatField(splittedRect[0], float.Parse(minVal.ToString(minMaxAttribute.Format)));
                    maxVal = EditorGUI.FloatField(splittedRect[2], float.Parse(maxVal.ToString(minMaxAttribute.Format)));

                    EditorGUI.MinMaxSlider(splittedRect[1], ref minVal, ref maxVal, minMaxAttribute.MinLimit, minMaxAttribute.MaxLimit);

                    minVal = Mathf.Clamp(minVal, minMaxAttribute.MinLimit, minMaxAttribute.MaxLimit);
                    maxVal = Mathf.Clamp(maxVal, minMaxAttribute.MinLimit, minMaxAttribute.MaxLimit);
                    vector = new Vector2(minVal > maxVal ? maxVal : minVal, maxVal);

                    if (EditorGUI.EndChangeCheck())
                    {
                        property.vector2Value = vector;
                    }
                    break;
                }
            case SerializedPropertyType.Vector2Int:
                {
                    EditorGUI.BeginChangeCheck();

                    Vector2Int vector = property.vector2IntValue;
                    float minVal = vector.x;
                    float maxVal = vector.y;

                    minVal = EditorGUI.FloatField(splittedRect[0], minVal);
                    maxVal = EditorGUI.FloatField(splittedRect[2], maxVal);

                    EditorGUI.MinMaxSlider(splittedRect[1], ref minVal, ref maxVal, minMaxAttribute.MinLimit, minMaxAttribute.MaxLimit);

                    minVal = Mathf.Clamp(minVal, minMaxAttribute.MinLimit, minMaxAttribute.MaxLimit);
                    maxVal = Mathf.Clamp(maxVal, minMaxAttribute.MinLimit, minMaxAttribute.MaxLimit);
                    vector = new Vector2Int(Mathf.FloorToInt(minVal > maxVal ? maxVal : minVal), Mathf.FloorToInt(maxVal));

                    if (EditorGUI.EndChangeCheck())
                    {
                        property.vector2IntValue = vector;
                    }
                    break;
                }
            default:
                EditorGUI.LabelField(position, label.text, "MinMaxSlider is only compatible with Vector2 types.");
                break;
        }
    }

    private static Rect[] SplitRect(Rect rectToSplit, int n)
    {
        Rect[] rects = new Rect[n];

        for (int i = 0; i < n; i++)
        {
            rects[i] = new Rect(rectToSplit.position.x + (i * rectToSplit.width / n), rectToSplit.position.y, rectToSplit.width / n, rectToSplit.height);
        }

        if (EditorGUI.indentLevel > 0)
        {
            int space = 5 * EditorGUI.indentLevel;
            int padding = (int)rects[0].width - 60;

            rects[0].x -= space * 3;
            rects[0].width -= padding - space;

            rects[1].x -= padding + space * 3.5f;
            rects[1].width += padding * 2 + space * 4;

            rects[2].x += padding - space;
            rects[2].width -= padding - space;
        }
        else
        {
            int space = 5;
            int padding = (int)rects[0].width - 60;

            //rects[0].x -= space; 
            rects[0].width -= padding + space * 2;

            rects[1].x -= padding * 0.75f;
            rects[1].width += padding * 1.5f;

            rects[2].x += padding + space * 2;
            rects[2].width -= padding + space * 2;
        }

        return rects;
    }
}

