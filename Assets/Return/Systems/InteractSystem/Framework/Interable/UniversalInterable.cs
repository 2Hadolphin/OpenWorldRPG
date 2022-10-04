using Return.Agents;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Return.InteractSystem
{
    /// <summary>
    /// Interact vfx option.
    /// </summary>
    //[Flags]
    public enum HighLightMode
    {
        None = 0,

        /// <summary>
        /// Volume outline.
        /// </summary>
        Outline = 1,

        /// <summary>
        /// Wrap collider bounds.
        /// </summary>
        ColliderBounds = 2,

        /// <summary>
        /// Wrap renderer bounds.
        /// </summary>
        RendererBounds = 3,

        /// <summary>
        /// Custom effect.
        /// </summary>
        Custom = 4
    }

    /// <summary>
    /// Interactable object with UnityEvent to post agent interact behaviour.
    /// </summary>
    public class UniversalInterable : BaseInterable
    {
        public override void Interact(IAgent agent, object sender = null)
        {
            OnInteract?.Invoke(agent, sender);
        }

        [SerializeField]
        HighLightMode m_HighLightMode = HighLightMode.Outline;
        public override HighLightMode HighLightMode { get => m_HighLightMode; set => m_HighLightMode = value; }



        [SerializeField]
        UnityEvent<IAgent, object> m_OnInteract;

        public virtual UnityEvent<IAgent, object> OnInteract=> m_OnInteract;


    }

    /// <summary>
    /// Dynamic interactable wrapper.
    /// </summary>
    public class DynamicInterable : UniversalInterable
    {
        public static UniversalInterable Create<T>(T obj, bool collider = false, bool dynamicObj = false) where T : IInteractHandle
        {
            var go = new GameObject($"{obj}_{nameof(UniversalInterable)}")
            {
                hideFlags = HideFlags.DontSave,
                layer = Layers.Physics_Interactable,
                tag = Tags.Interactable,
            };

            if(obj is Component c)
            {
                go.transform.Copy(c.transform);
                c.transform.SetParent(go.transform);
            }

            var handle = go.AddComponent<UniversalInterable>();

            if (collider)
            {
                var col = go.AddComponent<BoxCollider>();
                col.isTrigger = true;
            }

            if (dynamicObj)
            {
                if (!go.TryGetComponentInChildren<Rigidbody>(out var rig))
                    rig = go.AddComponent<Rigidbody>();

                rig.isKinematic = false;
                rig.useGravity = true;
            }

            //void action(IAgent agent,object sender)
            //{
            //    if (interact == null)
            //    {
            //        handle.OnInteract.
            //    }
            //    else
            //        interact.Invoke(agent, sender);
            //}

#if UNITY_EDITOR
            handle.OnInteract.AddListener(obj.Interact);
#endif
            return handle;
        }

        public override void Interact(IAgent agent, object sender = null)
        {   

            base.Interact(agent, sender);
        }

    }
}