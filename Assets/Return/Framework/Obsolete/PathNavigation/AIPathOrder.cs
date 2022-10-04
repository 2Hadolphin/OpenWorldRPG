using UnityEngine;
using System;

public class AIPathOrder : MonoBehaviour
{

    [SerializeField]
    private LayerMask EnableMask;
    [SerializeField]
    private AIPathRouter router;
    [SerializeField]
    private AIPathTargetDisplay showGUI;

    private Vector3 GizmosV3;
    private float GizmosRadius;





    /*
    private void getPoint()
    {
        if (Inputs.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Inputs.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit,EnableMask))
            {

                GizmosV3 = hit.point;
                GizmosRadius = 1;

                showGUI.setTarget(GizmosV3);
                //showGUI.setTarget(hit.point);
            }
        }
    }
    */
    [Serializable]
    public struct gogo
    {
        public   int aka;
        public    string aba;
        GameObject apa;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GizmosV3, GizmosRadius);
    }
}
    

