using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Return
{
    public class SceneInstaller : MonoInstaller
    {
        [SerializeField]
        List<Component> m_Components;

        [SerializeField]
        List<ScriptableObject> m_ScriptableObjects;


        #region Prop

        public List<Component> Components { get => m_Components; set => m_Components = value; }
        public List<ScriptableObject> ScriptableObjects { get => m_ScriptableObjects; set => m_ScriptableObjects = value; }

        #endregion

        public override void Start()
        {
            base.Start();
        }

        public override void InstallBindings()
        {

            Debug.Log(nameof(InstallBindings));


            foreach (var comp in Components)
                Register(comp);

            foreach (var so in ScriptableObjects)
                Register(so);
        }

        public void Register(object obj)
        {
            if (obj is IRegister register)
                register.Register(Container);
            else
                Container.
                    BindInterfacesTo(obj.GetType()).
                    FromInstance(obj).
                    AsSingle().
                    NonLazy();
        }
    }
}