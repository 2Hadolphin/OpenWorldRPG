#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[ExecuteInEditMode]
public class EditorTransformUpdate : MonoBehaviour
{
    public event Action<Transform> TransformUpdate;
    private void Awake()
    {
        tf = transform;
        this.hideFlags = HideFlags.DontSave;
    }
    Transform tf;
    private void Update()
    {
        if (tf.hasChanged)
        {
            tf.hasChanged = false;
            TransformUpdate?.Invoke(tf);
        }
    }
}


#endif