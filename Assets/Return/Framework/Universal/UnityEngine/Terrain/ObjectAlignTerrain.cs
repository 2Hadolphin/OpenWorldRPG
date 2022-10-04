using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace Return
{
    /// <summary>
    /// Place object on the terrain.
    /// </summary>
    public class ObjectAlignTerrain : MonoBehaviour
    {
        [ShowInInspector]
        public LayerMask TargetLayer;


        public float Offset;

        public bool OverrideRotationViaNormal;


        [Button(nameof(Align))]
        public virtual void Align()
        {
            var root = transform;
            var list = new List<Transform>(root.childCount);

            foreach (Transform tf in root)
                list.Add(tf);

            AlignObject2Terrain(TargetLayer, new Vector3(0, Offset), OverrideRotationViaNormal, list.ToArray());
        }


        public static void AlignObject2Terrain(LayerMask layer, Vector3 offset, bool alignNormal, params Transform[] objects)
        {
            var down = Vector3.down;
            var hits = new RaycastHit[10];

            foreach (var tf in objects)
            {
                var count = Physics.RaycastNonAlloc(tf.position, down, hits, 10000, layer);

                for (int i = 0; i < count; i++)
                {
                    if (hits[i].collider is TerrainCollider)
                    {
                        tf.position = hits[i].point + offset;
                        tf.up = hits[i].normal;
                        break;
                    }
                    Debug.Log(count + "**" + hits[i].collider);
                }

            }
        }
    }
}