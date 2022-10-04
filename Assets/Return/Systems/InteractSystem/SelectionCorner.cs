using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System;
using UnityEngine.Assertions;

namespace Return.InteractSystem
{
    public class SelectionCorner : MonoBehaviour
    {       

        [SerializeField]
        Transform[] m_corners;
        public Transform[] Corners { get => m_corners; set => m_corners = value; }

#if UNITY_EDITOR
        void ChangeWidth()
        {
            foreach (var corner in Corners)
            {
                if (!corner)
                    continue;

                corner.localScale = WidthRatio * Vector3.one;
            }
        }
        [SerializeField]
        [Range(0.2f,10f)]
#endif
        float m_widthRatio=1.2f;
        public float WidthRatio { get => m_widthRatio; set => m_widthRatio = value; }



        [Button]
        public virtual void LoadCorners()
        {
            var corners = 
                transform.
                GetChilds().
                Where(x => x.name.StartsWith("Corner", StringComparison.CurrentCultureIgnoreCase)).
                OrderBy(x=>x.name).
                ToArray();

            Assert.IsTrue(corners.Length == 8);

            Corners = corners;
        }

     



        [Button]
        public virtual void SetTarget(Renderer ren)
        {
            SetTarget(ren.bounds);
        }

        [Button]
        public virtual void SetTarget(Collider col)
        {
            SetTarget(col.bounds);
        }

        public virtual void SetTarget(Bounds bounds)
        {
            GizmosUtil.DrawWireCube(bounds.center, bounds.size, Color.red).Wait(10);

            transform.position = bounds.center;

            var parentScale = 
                transform.parent==null?
                Vector3.one:
                transform.lossyScale.Reciprocal();

            var scale = bounds.size.Multiply(WidthRatio).Multiply(parentScale);
            var size = bounds.size.Multiply(0.5f).Multiply(parentScale);

            for (int i = 0; i < 8; i++)
            {
                var pos =
                    size.Multiply(
                        i%4<2?1:-1,
                        i < 4 ? -1 : 1,
                        i%2>0?-1:1
                        );

                Corners[i].localScale = scale;
                Corners[i].localPosition = pos;
            }
        }


    }
}