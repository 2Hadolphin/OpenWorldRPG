using System;
using System.Collections.Generic;
using Return.Framework.DI;

namespace Return
{
    public static class BindingExtension
    {
        static readonly BindingHandle handle = new();



        /// <summary>
        /// Binding type of interfaces to handle.
        /// </summary>
        public static BindingHandle BindInterface(this Container container,IEnumerable<Type>types)
        {
            handle.Container = container;
            handle.Bindings.AddRange(types);

            return handle;
        }

        /// <summary>
        /// Binding type of interfaces to handle.
        /// </summary>
        public static BindingHandle BindInterface<T>(this Container container)
        {
            handle.Container = container;
            handle.Bindings.Add(typeof(T));

            return handle;
        }


        /// <summary>
        /// Binding type of interfaces to handle.
        /// </summary>
        public static BindingHandle BindInterface(this Container container,params Type[] types)
        {
            handle.Container = container;
            handle.Bindings.AddRange(types);

            return handle;
        }


        /// <summary>
        /// Push binding handle to container.
        /// </summary>
        public static void Finish(this BindingHandle handle)
        {
            // push binding
            if (handle.Source.IsNull())
            {
                // factory

            }
            else
            {

            }

            // clean data
            handle.Clean();
        }

        #region Instance


        public static void AsSingleton(this BindingHandle handle)
        {
            handle.singleton = true;

            handle.Build();
        }

        public static void Build(this BindingHandle handle)
        {
            if (handle.singleton)
            {
                if (handle.Type != null)
                    handle.Container.RegisterSingleton(handle.Type, handle.Source);

                foreach (var binding in handle.Bindings)
                    handle.Container.RegisterSingleton(binding, handle.Source);
            }
            else
            {
                if (handle.Type != null)
                    handle.Container.Register(handle.Type, handle.Source);

                foreach (var binding in handle.Bindings)
                    handle.Container.Register(binding, handle.Source);
            }  
        }

        /// <summary>
        /// Create 
        /// </summary>
        public static void NonLazy(this BindingHandle handle)
        {
            

        }


        #endregion

        public static BindingHandle FromInstance(this BindingHandle handle,object obj,bool pushInstanceType=false)
        {
            handle.Source = obj;

            var type = obj.GetType();

            if (handle.Type == null)
                handle.Type = type;

            if (pushInstanceType)
                handle.Bindings.Add(type);

            return handle;
        }


    }
}