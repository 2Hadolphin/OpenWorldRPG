using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

namespace Return.Framework.DI
{
    public static class ReflectionCache
    {
        public static Dictionary<Type, Cache> InjectionCache = new(100);

        public readonly static BindingFlags InjectOption =
            BindingFlags.Instance | // non static
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.SetField |
            BindingFlags.SetProperty |
            BindingFlags.InvokeMethod |
            BindingFlags.CreateInstance
            ;

        public static Cache GetCache(this Type type)
        {
            if(!InjectionCache.TryGetValue(type,out var cache))
            {
                cache = new Cache()
                {
                    FieldInfos = type.GetFields<InjectAttribute>(InjectOption),
                    PropertyInfos = type.GetProps<InjectAttribute>(InjectOption),
                    MethodInfos = type.GetMethods<InjectAttribute>(InjectOption),
                    //ConstructorInfos = type.GetConstructors()
                };

                InjectionCache.Add(type, cache);
            }

            return cache;
        } 

        /// <summary>
        /// Inject parameters to target by reflection.
        /// </summary>
        public static void Inject(this object obj, IDictionary<Type,object> parameters)
        {
            Assert.IsFalse(obj.IsNull());

            var type= obj.GetType();

            var cache = ReflectionCache.GetCache(type);

            #region Fields

            foreach (var field in cache.FieldInfos)
            {
                try
                {
                    var option = field.GetCustomAttribute<InjectAttribute>(true);

                    if (TryResolve(field.FieldType,parameters, out var source))
                        field.SetValue(obj, source);
                    else if (!option.Optional)
                        throw new KeyNotFoundException();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Inject failure : {obj} field {field.Name} type : {field.FieldType}.");
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

                    if (TryResolve(prop.PropertyType, parameters, out var source))
                        prop.SetValue(obj, source);
                    else if (!option.Optional)
                        throw new KeyNotFoundException();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{obj} field {prop.Name} inject failure.");
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

                    var methodParas = method.GetParameters();

                    var length = methodParas.Length;
                    var paras = new object[length];

                    for (int i = 0; i < length; i++)
                    {
                        if (TryResolve(methodParas[i].ParameterType,parameters, out var source))
                            paras[i] = source;
                        else if(option.Optional)
                            paras[i] = default;
                        else 
                            throw new KeyNotFoundException(
                                $"{obj} - func : {method.Name} unable to get inject parameter {methodParas[i].ParameterType}."
                                );
                    }

                    method.Invoke(obj, paras);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{obj} inject failure.");
                    Debug.LogException(e);
                    continue;
                }
            }

            #endregion
        }

        public static bool TryResolve(Type type, IDictionary<Type,object> parameters, out object obj)
        {
            if (parameters.TryGetValue(type, out obj))
                return true;

            if (type.IsInterface)
            {
                foreach (var para in parameters)
                {
                    //Debug.Log($"Trying cast {para.Key} to {type}.");

                    if (type.IsAssignableFrom(para.Key))
                    {
                        obj = para.Value;
                        parameters.Add(type, obj);
                        return true;
                    }
                    else if (type.IsInstanceOfType(obj))
                    {
                        obj = para.Value;
                        parameters.Add(type, obj);
                        return true;
                    }
                    else if (para.Key.GetInterfaces().Any(x => x == type))
                    {
                        obj = para.Value;
                        parameters.Add(type, obj);
                        return true;
                    }
                }
            }
            else
            {
                foreach (var para in parameters)
                {
                    Debug.Log($"Trying to cast {para.Key} as {type}.");

                    if (para.Key.IsSubclassOf(type))
                    {
                        obj = para.Value;
                        parameters.Add(type, obj);
                        return true;
                    }
                    else if (type.IsAssignableFrom(para.Key))
                    {
                        obj = para.Value;
                        parameters.Add(type, obj);
                        return true;
                    }
                    else if (type.IsInstanceOfType(obj))
                    {
                        obj = para.Value;
                        parameters.Add(type, obj);
                        return true;
                    }
                }
            }


            obj = default;
            return false;
        }


        public class Cache
        {
            public FieldInfo[] FieldInfos;
            public PropertyInfo[] PropertyInfos;
            public MethodInfo[] MethodInfos;
            public ConstructorInfo[] ConstructorInfos;
        }
    }

}