using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum Anchor
{
    Middle=0,
    Top=1,
    RightTop=2,
    Right=3,
    RightBottom=4,
    Bottom=5,
    LeftBottom=6,
    Left=7,
    LeftTop=8,

}
public class m_ScreenUtility
{
    public static Vector3 GetPositionInRatio(float xAxis,float yAxis)
    {
        var width = Screen.width * xAxis;
        var height = Screen.height * yAxis;


        return new Vector3(width, height, 0.05f);
    }
}
