using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using VContainer.Unity;


namespace VContainer
{
    public abstract class BaseHandlerContainer : LifetimeScope
    {
        [SerializeField]
        List<HandlerRegister> m_UnityHandles;
        public List<HandlerRegister> UnityHandles { get => m_UnityHandles; set => m_UnityHandles = value; }

        HashSet<IRegister> m_dynamicHandles = null;
        public HashSet<IRegister> dynamicHandles
        {
            get
            {
                if (m_dynamicHandles == null)
                    m_dynamicHandles = new();

                return m_dynamicHandles;
            }

            set => m_dynamicHandles = value;
        }

        public virtual void Push(DynamicRegister register)
        {
            Assert.IsFalse(dynamicHandles.Contains(register));
            dynamicHandles.Add(register);
        }

        public virtual void Push(IRegister register)
        {
            Assert.IsFalse(dynamicHandles.Contains(register));
            dynamicHandles.Add(register);
        }

        public virtual void Push(object obj, params Type[] types)
        {
            var register = new DynamicRegister()
            {
                Object = obj,
                BindingTypes = types
            };

            Assert.IsFalse(dynamicHandles.Contains(register));
            var valid=dynamicHandles.Add(register);
            Assert.IsTrue(valid);
        }

        public virtual void Push(Type createType, Lifetime lifetime, params Type[] types)
        {
            var register = new TypeRegister()
            {
                CreateType = createType,
                LifeTime = lifetime,
                BindingTypes = types
            };

            Assert.IsFalse(dynamicHandles.Contains(register));
            dynamicHandles.Add(register);
        }

        protected override void Awake()
        {
            // overwrite to ignore auto build
        }

        /// <summary>
        /// Bind targets with container builder.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {

            if (UnityHandles != null)
            {
                //Debug.Log(UnityHandles.Count);
                foreach (var handle in UnityHandles)
                {
                    try
                    {
                        handle.Binding(builder);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, this);
                    }
                }
            }


            if (dynamicHandles != null)
            {
                //Debug.Log(dynamicHandles.Count);
                foreach (var handle in dynamicHandles)
                {
                    try
                    {
                        handle.Binding(builder);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, this);
                    }
                }

                dynamicHandles.Clear();
                dynamicHandles = null;
            } 
        }

    }

    public static class InjectExtension
    {
        /// <summary>
        /// Function to bind instance
        /// </summary>
        public static void Binding(this IContainerBuilder builder, object obj, params Type[] interfaces)
        {
            RegistrationBuilder rb;

            //Debug.LogError(interfaces.Length);

            //foreach (var i in interfaces)
            //{
            //    Debug.LogErrorFormat("Binding {0} with {1}.", obj, i);
            //}

            if (obj is Component c)
            {
                rb = builder.RegisterComponent(c);
            }
            else// if (obj is ScriptableObject s)
            {
                rb = builder.RegisterInstance(obj);
            }

            if (interfaces == null)
                rb.AsImplementedInterfaces();
            else
                rb.As(interfaces);
        }

    }
}