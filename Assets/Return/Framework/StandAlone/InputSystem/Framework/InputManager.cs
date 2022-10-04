using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;

namespace Return
{
    /// <summary>
    /// Input manager.
    /// </summary>
    public class InputManager : BaseComponent//, IInitializable//, IRegister
    {
        [ShowInInspector,ReadOnly]
        public static mPlayerInput Input { get; protected set; }

        #region DI

        protected virtual void Awake()
        {
            Initialize();
        }

        //public void Register(DiContainer container)
        //{
        //    container.
        //}

        public void Initialize()
        {
            Input = new();
            Input.Enable();
        }

        #endregion


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            IsAppShuttingDown = false;
        }

        public static bool IsAppShuttingDown;

        private void OnApplicationQuit()
        {
            IsAppShuttingDown = true;
        }


    }
}