using UnityEngine;
using System;

namespace Return.Editors
{
    [ExecuteInEditMode]
    public class EditorTransoformPoster : TransformPoster
    {
        protected override void Awake()
        {
            base.Awake();

            hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
        }
    }
}