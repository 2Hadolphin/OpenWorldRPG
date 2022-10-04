using System.Collections.Generic;
using Return.Modular;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Return.InteractSystem
{
    public abstract class BaseSelectionHandle<T> : BaseComponent, IModularHandle, ISelectionHandle<InteractWrapper>//, ISelectable
    {
        #region Setup

        //public virtual void OnResolve(IModularResolver Resolver) { }

        #endregion

        protected Dictionary<InteractWrapper, T> effects = new(1);

        protected ISelectionHandler<InteractWrapper> m_handler;

        public void SetHandler(ISelectionHandler<InteractWrapper> handler)
        {
            handler.OnSelected -= OnSelected;
            handler.OnSelected += OnSelected;

            handler.OnDeselected -= OnDeselect;
            handler.OnDeselected += OnDeselect;

            m_handler = handler;
        }

        protected virtual void OnSelected(InteractWrapper wrapper)
        {
            if (effects.ContainsKey(wrapper))
                return;

            if (AddTarget(wrapper, out var interable))
                effects.Add(wrapper,interable);

            Debug.LogFormat("{0} selected {1}.", this,wrapper.Interactable);
        }


        protected virtual async void OnDeselect(InteractWrapper wrapper)
        {
            if (!effects.TryGetValue(wrapper, out var effect))
            {
                if (wrapper.Interactable is ICustomSelectionVFX vfx)
                    vfx.OnDeselect(wrapper.Agent, this);

                return;
            }

            await RemoveTarget(effect);

            effects.Remove(wrapper);


            //Debug.LogFormat("{0} deselect.", wrapper.Interactable);
        }

        /// <summary>
        /// Add effect target to cache data.
        /// </summary>
        protected abstract bool AddTarget(InteractWrapper wrapper,out T value);

        protected abstract UniTask RemoveTarget(T value);
    }
}