using UnityEngine;

namespace Return
{
    public static class LayerExtension
    {
        public static LayerMask Exclude(this LayerMask mask,params int[] exclude)
        {
            var length = exclude.Length;

            for (int i = 0; i < length; i++)
                mask |= ~(1 << exclude[i]);

            return mask;
        }

        public static void Add(this ref LayerMask mask, params int[] include)
        {
            var length = include.Length;

            for (int i = 0; i < length; i++)
                mask |= (1 << include[i]);
        }

        public static void SetChildLayer(this GameObject go,int layer,params string[] tags)
        {
            bool compare = tags != null && tags.Length > 0;


            var tfs = go.transform.Traverse();


            foreach (var child in tfs)
            {
                if (compare)
                {
                    foreach (var tag in tags)
                        if (child.CompareTag(tag))
                        {
                            child.gameObject.layer = layer;
                            break;
                        }
                }
                else
                {
                    child.gameObject.layer = layer;
                }

            }
        }

    } 
}
