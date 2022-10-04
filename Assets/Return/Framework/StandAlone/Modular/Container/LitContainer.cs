using Return.Framework.DI;
using Return.Modular;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Return
{
    /// <summary>
    /// Lightweight object repository.(nums < 10)
    /// </summary>
    [Serializable]
    public class LitContainer : NullCheck, IModularResolver
    {
        public LitContainer()
        {

        }

        /// <summary>
        /// Cache type and instance.
        /// </summary>
        protected Dictionary<Type, object> Modules=new();

        #region Iresolver

        IEnumerable<object> IModularResolver.Modules => GetModules();
        void IModularResolver.Inject(object obj) => Inject(obj);

        public virtual bool Inject(object obj)
        {
            try
            {
                Assert.IsFalse(obj.IsNull());

                obj.Inject(Modules);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }


        #endregion

        public virtual void RegisterModule<T>(object module)
        {
            Assert.IsFalse(module.IsNull());

            var type = typeof(T);

            var valid = !Modules.ContainsKey(type);

            if (valid)
            {
                Modules.Add(type, module);
                OnModuleRegister(module);
            }

            if (valid)
                Debug.Log($"RegisterHandler module {module} with type : {type}.");
            else
                Debug.LogError($"Repeat module binding - {type} already exist : {module} - {Modules.ContainsKey(type)}");

        }

        /// <summary>
        /// Register module to container.
        /// </summary>
        public virtual void RegisterModule(object module)
        {
            try
            {
                Assert.IsFalse(module.IsNull(), "Register null module to container.");

                var type = module.GetType();

                var valid = !Modules.ContainsKey(type);

                if (valid)
                {
                    Modules.Add(type, module);
                    OnModuleRegister(module);
                }

                if (valid)
                    Debug.Log($"RegisterHandler module {module} with type : {type}.");
                else
                    Debug.LogError($"Repeat module binding - {type} already exist : {module} - {Modules.ContainsKey(type)}");

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Unregister module from container.
        /// </summary>
        public virtual void UnregisterModule(object module)
        {
            try
            {
                Modules.RemoveValues(module);
                //throw new NotImplementedException($"{nameof(UnregisterModule)} has not finish.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public virtual bool TryGetModule<T>(out T module) where T : class
        {
            var exist = Modules.Values.TryGetValueOfType(out module);
            return exist;
        }

        /// <summary>
        /// Return all module instance.
        /// </summary>
        protected virtual IEnumerable<object> GetModules()
        {
            //var modules = Modules.Values.CacheLoop();
            var modules = Modules.Values;

            foreach (var module in modules)
                yield return module;
        }


        public IEnumerable<T> GetModules<T>()
        {
            //var values = Modules.Values.CacheLoop();
            var values = Modules.Values;

            foreach (var module in values)
                if (module is T value)
                    yield return value;
        }

        public virtual bool TryGetModule(Type type, out object module)
        {
            return Modules.TryGetValue(type, out module);
        }

        public IEnumerable<object> GetModules(Type type)
        {
            return Modules.Where(x => x.Key.IsSubclassOf(type)).Select(x => x.Value);
        }


        protected DictionaryList<Type, ActionWrapper> ModuleCallbacks = new();

        public virtual void SubscribeModule<T>(Action<T> callback, bool searchExisting = true) where T : class
        {
            Assert.IsNotNull(callback);

            Debug.Log($"{callback} subscribe {typeof(T)}.");

            var type = typeof(T);

            ModuleCallbacks.Add(type, new ActionWrapper<T>(callback));

            if (searchExisting)
            {
                var modules = GetModules<T>();

                foreach (var module in modules)
                    callback(module);
            }
        }

        /// <summary>
        /// Proccess module register event.
        /// </summary>
        protected virtual void OnModuleRegister(object module)
        {
            var type = module.GetType();

            var interfaces = type.GetInterfaces();

            foreach (var binding in interfaces)
                CheckBindingModuleRegister(module, binding);

            CheckBindingModuleRegister(module, type);
        }

        protected virtual void CheckBindingModuleRegister(object module, Type type)
        {
            try
            {
                if (ModuleCallbacks.TryGetValues(type, out var list))
                {
                    Debug.Log($"Valid {type.Name} callback nums : {list.Count}.");

                    foreach (ActionWrapper callback in list)
                    {
                        if (callback.NotNull())
                            callback.OnRecive(module);
                        else
                            Debug.LogError("Missing callback");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }


#if UNITY_EDITOR

        [OnInspectorGUI]
        [ShowInInspector]
        object draw_module;

        int index = -1;

        [OnInspectorGUI]
        void DrawModule()
        {
            GUILayout.BeginHorizontal();

            int newIndex = index;


            if (Modules != null)
            {
                if (GUILayout.Button("Last"))
                    index = Modules.Loop(--index);

                if (GUILayout.Button("Next"))
                    index = Modules.Loop(++index);

                if (index != newIndex)
                    draw_module = Modules.ToArray()[index].Value;

                if (draw_module == this)
                    index++;
            }

            GUILayout.EndHorizontal();
        }

    
#endif
    }
}