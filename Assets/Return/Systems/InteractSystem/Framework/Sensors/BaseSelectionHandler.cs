using UnityEngine;
using System.Collections.Generic;
using Return.Modular;
using System;

namespace Return.InteractSystem
{
    public abstract class BaseSelectionHandler : BaseComponent, IModularHandle,ISelectionHandler<InteractWrapper>
    {
        #region Setup

        //public virtual void OnResolve(IModularResolver Resolver) { }

        #endregion

        /// <summary>
        /// Cache id for new selected hash none allo.
        /// </summary>
        protected static HashSet<int> ps_newIDs = new(20);

        #region ISelectionHandler

        public virtual event Action OnSelectedChanged;

        public virtual event Action<InteractWrapper> OnSelected;
        public virtual event Action<InteractWrapper> OnDeselected;

        #endregion

        // Senser Config
        public List<Type> TargetTypes = new(1);

        // Layer
        [SerializeField]
        private LayerMask m_SelectableMask = -1;
        public LayerMask selectableMask { get => m_SelectableMask; set => m_SelectableMask = value; }

        [SerializeField]
        private QueryTriggerInteraction m_triggerInteraction = QueryTriggerInteraction.Collide;
        public QueryTriggerInteraction triggerInteraction { get => m_triggerInteraction; set => m_triggerInteraction = value; }


        // Tag
        [SerializeField]
        private string m_Tag;// = Tags.Detectable;
        public string Tag { get => m_Tag; set => m_Tag = value; }

        // Max amount
        [SerializeField]
        [Range(0, 100)]
        private int m_maxRaycastNumber = 1;
        public int maxRaycastNumber { get => m_maxRaycastNumber; set => m_maxRaycastNumber = value; }

        // OnSensorUpdate distance
        [SerializeField]
        private float m_senserRange = 1.71f;
        public float senserRange { get => m_senserRange; set => m_senserRange = value; }

        // Update ratio
        [SerializeField, Range(0.1f, 5)]
        private float m_updateRate = 0.25f;
        public float updateRate { get => m_updateRate; set => m_updateRate = value; }


        // Cache        
        protected DictionaryList<int, InteractWrapper> cacheColliders;
        public IEnumerable<InteractWrapper> Targets()
        {
            foreach (var wrappers in cacheColliders.elements.Values)
            {
                //if (wrappers.IsNull())
                //    continue;

                foreach (var wrapper in wrappers)
                    yield return wrapper;
            }

            yield break;
        }


    }
}