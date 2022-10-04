using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JobManager : MonoBehaviour
{
    private static JobManager _Instance;
    public static JobManager Instance
    {
        get
        {
            if (!_Instance)
            {

                var go = new GameObject("JobManager");
                go.hideFlags = HideFlags.NotEditable;
                _Instance = go.AddComponent<JobManager>();
            }
            return _Instance;
        }
    }

    public Action Update_Frame;

    private void Update()
    {
        Update_Frame?.Invoke();
    }
}
