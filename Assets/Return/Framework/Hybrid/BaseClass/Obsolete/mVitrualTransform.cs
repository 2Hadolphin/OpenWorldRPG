using UnityEngine;
using System;

namespace Return
{
    [Obsolete]
    public enum DriveType { Data, Exhibit, Simulation }

    [Serializable]
    [Obsolete]
    public struct m_Transform
    {
        public string Name;
        public PR PR;

        public Transform Instance(Transform tf)
        {
            var instance=tf.Find(Name);
            if (!instance)
            {
                instance = new GameObject(Name).transform;
                instance.SetParent(tf);
            }

            return instance;
        }
    }
}
