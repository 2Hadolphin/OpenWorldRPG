using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Sirenix.Utilities;
using System;
using Return.UI;

public class PerformanceHandler : MonoBehaviour
{
    PerformanceCounter cpuCounter;
    PerformanceCounter ramCounter;

    [NonSerialized]
    GUIStyle label;

    private void Awake()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available MBytes");
    }

    void OnGUI()
    {
        if (label == null)
        {
            label = new GUIStyle(GUI.skin.label)
            {
                fontSize=20,
                alignment=TextAnchor.MiddleRight,
                
            };
        }

        var width = 250;
        var height = 25;

        GUILayout.BeginArea(GUIUtil.RightTop(width,height,10,10));
        GUILayout.Label(getCurrentCpuUsage(), label);
        GUILayout.EndArea();

        GUILayout.BeginArea(GUIUtil.RightBottom(width, height, 10, 10));
        GUILayout.Label(getAvailableRAM(), label);
        GUILayout.EndArea();
    }

    public string getCurrentCpuUsage()
    {
        return cpuCounter.NextValue() + "%";
    }

    public string getAvailableRAM()
    {
        return ramCounter.NextValue() + "MB";
    }
}
