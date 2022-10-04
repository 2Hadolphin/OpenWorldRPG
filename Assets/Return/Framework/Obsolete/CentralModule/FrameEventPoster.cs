using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FrameEventPoster : MonoBehaviour
{
    private Dictionary<object, float> sub;
    public event EventHandler FramePost;

    public void Post()
    {
        FramePost.Invoke(this, EventArgs.Empty);
    }

    public void Regist(object o,float restT)
    {
        sub.Add(o, restT);
    }

}
