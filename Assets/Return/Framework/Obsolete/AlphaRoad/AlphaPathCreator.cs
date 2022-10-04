using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[ExecuteInEditMode]
public class AlphaPathCreator : MonoBehaviour
{
    [HideInInspector]
    public AlphaPath path;

    public enum PathLayer {BuildPath,VehiclePath }
    public PathLayer PathMode;

    public LayerMask CurLayer;
    [Header("SnapSetting")]

    [Space]

    
    [Range(0,360)]
    public float Move;
    [Range(0, 360)]
    public float  Roate;
    [Range(0, 360)]
    public float Scale;

    public void CreatePath()
    {
        path = new AlphaPath(transform.position + Vector3.zero);

        print("CreatePath");
    }

    public GameObject[] GameObjects;
}
