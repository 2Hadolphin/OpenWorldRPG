using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;
using System;
using Return.Modular;
using Return.Framework.Parameter;

namespace Return.Agents
{
    /// <summary>
    /// Autonomous unit (single or group) assembly state and system log
    /// </summary>
    [DefaultExecutionOrder(ExecuteOrderList.AgentInitialize)]
    public abstract class Agent : ModuleHandler, IAgent
    {
        #region Dev

        [SerializeField]
        bool m_devStart = true;

        // dev start
        private IEnumerator Start()
        {
            if (!m_devStart)
                yield break;

            yield return new WaitForSeconds(1);

            Register();

            // execute Resolver inject
            //if(Resolver is IModularresolverHandle resolverHandle)
            //{
            //    resolverHandle.SetHandler();
            //}
            //else

            Activate();
        }

        #endregion

        public const string Tag = nameof(Agent);

        #region Mono
        /// <summary>
        /// Return agent transform.
        /// </summary>
        public virtual Transform GetTransform => base.transform;

        /// <summary>
        /// Agent id.
        /// </summary>
        [Obsolete]
        public virtual string Name { get; set; }

        #endregion

        #region IAgent

        GameObject IAgent.GameObject => gameObject;
        Transform IAgent.transform => GetTransform;

        bool IAgent.isMine => isMine;

        #endregion

        #region resolver

        public override void InstallResolver(IModularResolver resolver)
        {
            if (resolver is IResolver bindingresolver)
            {
                bindingresolver.
                BindInterface<IAgent>().
                FromInstance(this).
                AsSingleton();
            }
            else
            {
                resolver.RegisterModule<IAgent>(this);
            }
        }

        #endregion


        #region TNet

        [SerializeField]
        TNet.TNObject m_tno;
        public TNet.TNObject tno { get => m_tno; protected set => m_tno = value; }


        [ShowInInspector,ReadOnly]
        Parameter<bool> m_isMine;
        public Parameters<bool> isMine => m_isMine;

        /// <summary>
        /// Set network handle.
        /// </summary>
        /// <param name="TNObject"></param>
        public void LoadTNO(TNet.TNObject TNObject = null)
        {
            if (TNObject.IsNull())
            {
                if (!TryGetComponent(out TNObject))
                    TNObject = gameObject.AddComponent<TNet.TNObject>(); // fake tno
            }

            Debug.Log("Log TNet id from : " + TNObject);
            tno = TNObject;

            LoadTNO();
        }

        /// <summary>
        /// Inject network handle.
        /// </summary>
        public virtual void LoadTNO()
        {
            bool func() => tno.isMine;

            if (m_isMine.IsNull())
                m_isMine = new Parameter<bool>(func);
            else // update setter ?
                m_isMine = new Parameter<bool>(func);
        }

        #endregion



        #region Routine

        public override bool DisableAtRegister => false;

        protected override void Register()
        {
            base.Register();

            LoadTNO(null);

            // inject parameters
            var modules = Resolver.Modules.CacheLoop();
            foreach (var module in modules)
            {
                Resolver.Inject(module);
            }
        }

        protected override void Activate()
        {
            try
            {
                LoadModules();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                enabled = false;
            }
        }

        #endregion

        /// <summary>
        /// SetHandler modules.   **Motions  **Inventory  **Interact  **Damage
        /// </summary>
        protected abstract void LoadModules();

        /// <summary>
        /// ????
        /// </summary>
        protected abstract void LoadIdentity();
    }
}