using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;
using System;

namespace Return.Modular
{
    /// <summary>
    /// Global Resolver handle with type bindings container, will unregister modules when destroy.
    /// </summary>
    public class SceneInstaller : BindingResolver
    {
        [PropertyOrder(-1)]
        [SerializeField]
        bool m_crossScene = false;
        /// <summary>
        /// Set don't destroy if true.
        /// </summary>
        public bool CrossScene { get => m_crossScene; set => m_crossScene = value; }

        protected override void Awake()
        {
            try
            {
                if (CrossScene)
                {
                    if(transform.parent!=null)
                    {
                        Debug.LogWarning($"Persistent installer should be as root transform.");
                        transform.parent = null;
                    }

                    DontDestroyOnLoad(gameObject);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            base.Awake();
        }

        protected override void SetupContainer()
        {
            if (Container == null)
            {
                var framework = ModularContainer.Framework;

                if (framework == null)
                {
                    framework = new ModularContainer();
                    ModularContainer.Framework = framework;
                }

                Container = framework as ModularContainer;
            }
        }

        protected override void RegisterPreset(UnityEngine.Object preset)
        {
            base.RegisterPreset(preset);
            
            // set initialize
            if (preset is IStart start)
                Routine.AddStartable(start);
        }

        protected virtual void OnDestroy()
        {
            if (Routine.quitting)
                return;

            foreach (var preset in PresetModules)
            {
                try
                {
                    Assert.IsFalse(preset == null);

                    Resolver.UnregisterModule(preset);

                    // install module with Resolve handle
                    //if (preset is IResolveHandle Resolver)
                    //    Resolver.InstallResolver(Resolver);
                    //else
                    //    Resolver.RegisterModule(preset);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    Debug.Log($"Resolver load preset module {preset}.");
                }
            }
        }
    }
}