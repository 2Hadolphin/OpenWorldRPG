using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Assertions;

namespace Return
{
    public static class GizmosUtilityExtension
    {
        /// <summary>
        /// Subscribe gizmos event phase and wait for end.
        /// </summary>
        /// <param name="draw">Function to draw gizmos.</param>
        /// <param name="time">Time to keep this gizmos</param>
        public static async UniTask Wait(this GizmosUtil.GizmosDelegate draw, float time)
        {
            Assert.IsFalse(draw == null);

            GizmosUtil.OnGizmos += draw.DrawGizmos;

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(time));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                GizmosUtil.OnGizmos -= draw.DrawGizmos;
            }
        }
    }
}