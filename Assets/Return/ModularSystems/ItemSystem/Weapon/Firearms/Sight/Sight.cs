using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;

namespace Return.Items
{
    public class Sight : MonoItemModule
    {
        public float aimingTime = 0.15f;

        // un complete
        public FrontSight FrontSight;

        [SerializeField]
        Transform m_AimPoint;

        public Transform AimPoint { get => m_AimPoint; set => m_AimPoint = value; }

        public Vector3 Offset;
        // Lens ratio data 
        public Vector3 GetAimPoint()
        {
            if (AimPoint.IsNull())
            {
                AimPoint = transform;
                Offset = new Vector3(0, 0.03f);
            }

            return AimPoint.TransformPoint(Offset);
        }
 

    
    }
}