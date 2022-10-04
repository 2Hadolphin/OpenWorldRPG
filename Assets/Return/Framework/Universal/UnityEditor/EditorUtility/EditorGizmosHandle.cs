#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ??????
/// </summary>
[ExecuteInEditMode]
public class EditorGizmosHandle : MonoBehaviour
{
    public event Action OnGizmos;

    public void Awake()
    {
        this.hideFlags = HideFlags.DontSave;
    }

    private void OnDrawGizmos()
    {
        OnGizmos?.Invoke();
    }
}

#endif