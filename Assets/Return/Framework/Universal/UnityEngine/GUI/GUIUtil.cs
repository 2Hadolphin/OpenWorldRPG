using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.UI
{
    public class GUIUtil
    {
        #region GUI

        public static Rect LeftTop(float width, float height, float offsetTop, float offsetLeft)
        {
            return new Rect(offsetLeft, offsetTop, width, height);
        }

        public static Rect LeftBottom(float width, float height, float offsetBottom, float offsetRight)
        {
            return new Rect(offsetRight, Screen.height - offsetBottom - height, width, height);
        }

        public static Rect RightTop(float width,float height,float offsetTop,float offsetRight)
        {
           return new Rect(Screen.width - width- offsetRight, offsetTop, width, height);
        }

        public static Rect RightBottom(float width, float height, float offsetBottom, float offsetRight)
        {
            return new Rect(Screen.width - width - offsetRight, Screen.height- offsetBottom-height, width, height);
        }

        #endregion

    }
}