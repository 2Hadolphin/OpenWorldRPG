using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class m_MinMaxSliderAttribute : PropertyAttribute
{
    public readonly float MinLimit;
    public readonly float MaxLimit;
    public readonly string Tooltip;
    public readonly string Format;

    public m_MinMaxSliderAttribute(float min = 0, float max = 1, string tooltip = "", string format = "F1")
    {
        MinLimit = min;
        MaxLimit = max;
        Tooltip = tooltip;
        Format = format;
    }
}
