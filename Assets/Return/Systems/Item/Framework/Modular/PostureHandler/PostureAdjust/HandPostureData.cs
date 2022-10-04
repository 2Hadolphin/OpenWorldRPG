using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Return.Items
{
    public class HandPostureData
    {
        [SerializeField]
        [HideInInspector]
        protected int m_Bone;
        [ShowInInspector]
        //[Sirenix.OdinInspector.ReadOnly]
        public HumanBodyBones Bone
        {
            get => (HumanBodyBones)m_Bone;
            set => m_Bone=(int)value;
        }

        protected bool TryGetHandle(Animator animator,out TransformStreamHandle handle)
        {
            return animator.GetStreamHandle(Bone, out handle);
        }
    }
}

