using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEngine.Assertions;

namespace Return.Framework.DI
{
    /// <summary>
    /// Cache types and Resolve instance with custom attributes.
    /// </summary>
    public class Container : NullCheck
    {
        public Container()
        {
            Creation = new();
            Singleton = new();
        }

        public static Container Framework;
        

        public Container Parent;

        public delegate Func<string, Container> Feature();

        /// <summary>
        /// Use this to delay posts during batch binding.
        /// </summary>
        public bool OnFreeze;

        #region ID


        public DictionaryList<Type, object> InstancePools=new();

        #endregion

        #region DI

        public Dictionary<Type, InstanceCache> Creation;

        public Dictionary<Type, object> Singleton;


        public virtual void Inject(object obj)
        {
            Assert.IsFalse(obj.IsNull());
            var type = obj.GetType();

            var cache=ReflectionCache.GetCache(type);

            #region Fields

            foreach (var field in cache.FieldInfos)
            {
                try
                {
                    var option = field.GetCustomAttribute<InjectAttribute>(true);

                    if(TryResolve(option,field.FieldType,out var source))
                        field.SetValue(obj, source);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Inject field failure.\n{obj}\nfield : {field.Name} .");
                    Debug.LogException(e);
                    continue;
                }
            }

            #endregion

            #region Property

            foreach (var prop in cache.PropertyInfos)
            {
                try
                {
                    var option = prop.GetCustomAttribute<InjectAttribute>(true);

                    if (TryResolve(option, prop.PropertyType, out var source))
                        prop.SetValue(obj, source);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Inject property failure.\n{obj}\nproperty : {prop.Name} .");
                    Debug.LogException(e);
                    continue;
                }
            }

            #endregion

            #region Method

            foreach (var method in cache.MethodInfos)
            {
                try
                {
                    var option = method.GetCustomAttribute<InjectAttribute>(true);

                    var parameters = method.GetParameters();

                    var length = parameters.Length;
                    var paras = new object[length];

                    for (int i = 0; i < length; i++)
                    {
                        if (IsParams(parameters[i]))
                        {
                            throw new NotImplementedException("Reflection cast params function not finish.");
                        }
                        
                        if (TryResolve(option, parameters[i].ParameterType, out var source))
                            paras[i]=source;
                        else
                            paras[i] = default;
                    }

                    method.Invoke(obj, paras);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Inject function failure.\n{obj}\nfunction : {method.Name}");
                    Debug.LogException(e);
                    continue;
                }
            }

            #endregion

        }

        static bool IsParams(ParameterInfo param)
        {
            return param.IsDefined(typeof(ParamArrayAttribute), false);
        }

        public virtual bool TryResolve(InjectAttribute option,Type injectType,out object source)
        {
            Container container = option.InjectOption switch
            {
                InjectOption.Handler => this,
                InjectOption.Agent => Parent,
                InjectOption.Zone => throw new NotImplementedException(option.InjectOption.ToString()),
                InjectOption.Framework => Framework,
                InjectOption.Feature => throw new NotImplementedException(option.InjectOption.ToString()),
                _ => throw new NotImplementedException(option.InjectOption.ToString()),
            };

            if (!container.TryResolve(injectType, out source))
            {
                if (option.Optional)
                    return false;

                throw new KeyNotFoundException(
                      $"Require inject type has not register : {injectType}"
                    );
            }

            return true;
        }

        public virtual bool TryResolve(Type type,out object obj)
        {
            // singleton
            if (Singleton.TryGetValue(type, out obj))
                return true;

            // instance pools
            if(InstancePools.TryGetValues(type,out var list))
            {
                Debug.LogWarning($"Resolve value from instance pool, this operation might be danger.");

                if (list.Count == 0)
                    return false;


                obj = list.FirstOrDefault();
                return true;
            }    

            // factory
            if (Creation.TryGetValue(type, out var creation))
            {
                obj = creation.GetValue();
                return true;
            }




            return false;
        }

        #endregion

        #region Register

        public virtual void RegisterSingleton(Type type, object obj)
        {
            if (Singleton.ContainsKey(type))
            {
                var sigleton = Singleton[type];

                if (sigleton.IsNull())
                    Singleton[type] = obj;
                else
                    throw new InvalidOperationException($"Singleton of {type} has been register as {sigleton}");
            }
            else
            {
                Singleton.Add(type, obj);
            }
        }

        
        /// <summary>
        /// Register object with type.
        /// </summary>
        public virtual void Register(Type type, object obj)
        {
            //if(!Singleton.ContainsKey(type))
            //{

            //}
            //else 

            InstancePools.TryGetValues(type, out var list, true);

            {
                var valid = list.CheckAdd(obj);
                Debug.Log($"Container bindig {type} to {obj} success : {valid}.");
            }

        }

        


        #endregion


        public virtual void Unregister(object obj)
        {
            var types = obj.GetType().GetInterfaces();

            foreach (var type in types)
                if (InstancePools.TryGetValues(type, out var list))
                    list.Remove(obj);

            Singleton.RemoveByValue(obj);
        }

    }

    [Obsolete]
    public abstract class InstanceCache
    {
        public abstract object GetValue();
    }
}