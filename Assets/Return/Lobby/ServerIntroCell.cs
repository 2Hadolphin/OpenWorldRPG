using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;
using TheraBytes.BetterUi;
using System;

public class ServerIntroCell : EnhancedScrollerCellView
{

    public BetterTextMeshProUGUI ServerName;
    public BetterImage ServerIcon;

    public BetterImage NetHealthIcon;

    public event Action<float> OnInspect;
    public event Action<float> OnJoinServer;

    public virtual void Inspect()
    {

    }

    public virtual void JoinServer()
    {

    }
}
