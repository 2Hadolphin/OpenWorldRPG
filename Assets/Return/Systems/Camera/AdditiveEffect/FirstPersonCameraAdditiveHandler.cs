using System;
using System.Collections.Generic;
using UnityEngine;
using Return;
using Sirenix.OdinInspector;
using Return.Items;
using Return.Agents;
using DG.Tweening;
using Cysharp.Threading.Tasks;


namespace Return.Cameras
{
    [DefaultExecutionOrder(ExecuteOrderList.CameraEffect)]
    public class FirstPersonCameraAdditiveHandler : BaseAdditiveTransformHandler
    {
        public Transform ItemCameraTransform;

        #region Cache Effect

        HashSet<IAdditiveTransform> cacheEffects = new();
        
        public override void ApplyAdditiveEffect(IAdditiveTransform add)
        {
            base.ApplyAdditiveEffect(add);

            cacheEffects.Add(add);
        }

        #endregion


        protected override void ApplyAdditive(PR pr)
        {
            ItemCameraTransform.SetAdditiveLocalPR(pr);
        }

    }
}