using UnityEngine;
using System;

namespace Return.Humanoid
{
    /// <summary>
    /// Develop handle providing ik phase instead of motion system.
    /// </summary>
    public class AnimatorHandle : BaseComponent, IHumanoidAnimator
    {
        private void Awake()
        {
            if(!m_Animator)
                m_Animator = GetComponentInParent<Animator>();
        }

        [SerializeField]
        private Animator m_Animator;
        public Animator Animator { get => m_Animator; }


        public event Action<int> OnAnimatorIKPhase;

        protected virtual void OnAnimatorIK(int layer)
        {
            OnAnimatorIKPhase?.Invoke(layer);
        }
    
    }
}
