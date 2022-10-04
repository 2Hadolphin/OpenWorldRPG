using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheraBytes.BetterUi;
[RequireComponent(typeof(BetterButton))]
public class ButtonField : TitleField
{
    [SerializeField]
    public BetterButton Button;

    public override void Release()
    {
        base.Release();
        Button.onClick.RemoveAllListeners();
    }
    //public override void ResetField(string title=default)
    //{
    //    base.ResetField(title);
    //    Debug.LogError("RemoveListeners");
    //    Button.onClick.RemoveAllListeners();
    //}

}

