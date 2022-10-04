using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheraBytes.BetterUi;
using System;
using Michsky.UI.Zone;
using Sirenix.OdinInspector;
using System.Linq;

[CreateAssetMenu(fileName = "CreateUIPrefabCollection", menuName = "Return/UI", order = 1)]
public class mUIPrefabs : ScriptableObject
{
    const string UIElement = "UIElement";
    const string UIAudio = "UIAudio";

    [BoxGroup(UIElement)]
    [SerializeField]
    public List<UIElemant> Controls = new();

    private Dictionary<Type, UIElemant> CatchControls;
    public Dictionary<Type, UIElemant> GetControlPrefabs
    {
        get
        {
            if (null == CatchControls)//||CatchControls.Count!=Controls.Count)
                CatchControls = Controls.ToDictionary(x => x.GetType());

            return CatchControls;
        }
    }

    [BoxGroup(UIElement)]
    [SerializeField]
    public TitleField TitleField;

    [BoxGroup(UIElement)]
    [SerializeField]
    public TitleField NormalField;

    [BoxGroup(UIElement)]
    [SerializeField]
    public ButtonField ButtonField;




    [BoxGroup(UIAudio)]
    [SerializeField]
    public AudioClip UIElementPassby;

    [BoxGroup(UIAudio)]
    [SerializeField]
    public AudioClip UIElementClick;
    //[BoxGroup(UIElement)]
    //[SerializeField]
    //public SwitchManager SwitchToggle;

    //[BoxGroup(UIElement)]
    //[SerializeField]
    //public HorizontalSelector HorizontalSelector;

    //[BoxGroup(UIElement)]
    //[SerializeField]
    //public SliderGamepadManager Slider;



}
