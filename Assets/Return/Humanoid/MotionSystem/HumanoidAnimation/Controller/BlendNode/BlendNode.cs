using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendNode : MonoBehaviour
{

    public float Weight=1f;

    // Used to draw a circle in SceneView to indicate the weight of this node's settlement.
    void OnDrawGizmos()
    {

        Gizmos.color = Weight==0?Color.red: Color.cyan;

        var radiusFromWeight = Mathf.Sqrt(Weight);//From Unity Editor Source Code
        Gizmos.DrawWireSphere(transform.position, Mathf.Clamp(radiusFromWeight * 0.2f,0.05f,1f));
    }
}
