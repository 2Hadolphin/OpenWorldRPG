using UnityEngine;
using Sirenix.OdinInspector;

namespace Return.Items.Weapons
{
    [HideLabel]
    [System.Serializable]
    public struct ValueSlider
    {

#if UNITY_EDITOR
        [MinMaxSlider(nameof(Min), nameof(Max))]
#endif
        public Vector2 FovRange;

#if UNITY_EDITOR
        [HideInInspector]
        public float Max; 
        [HideInInspector]
        public float Min;

        float GetMax => FovRange.y;
        float GetMin => FovRange.x;

        [PropertyRange(nameof(GetMin), nameof(GetMax))]
#endif
        public float Fov;

    }
}
