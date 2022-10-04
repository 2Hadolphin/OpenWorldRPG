using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Text;
using System;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

public class IKHandle : SerializedScriptableObject
{
    public void Init(Animator animator)
    {
        
    }

    public HashSet<GameObject> IKHandles;


}
