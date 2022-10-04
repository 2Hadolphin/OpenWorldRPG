using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Return;

public static class mTagExtension
{
    public static Transform[] GetChildWithTag(this Transform tf,string tag)
    {
        return tf.Traverse().Where(x => x.CompareTag(tag)).ToArray();
    }

}
