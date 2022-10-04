using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Return
{
    public static class ComponentExtension
    {
        public static bool IsComponent(this Type type)
        {
            return type.IsSubclassOf(typeof(Component));
        }


        /// <summary>
        /// Four times as long as Addcomponent.
        /// </summary>
        public static T AddComponent<U, T>(this U holder, T duplicate) where U : Component where T : Component
        {
            return holder.gameObject.AddComponent(duplicate);
        }

        /// <summary>
        /// Reflection add go.
        /// </summary>
        public static T AddComponent<T>(this GameObject holder, T duplicate) where T : Component
        {
            T target = holder.AddComponent(duplicate.GetType()) as T;

            target.ReflectFrom(duplicate);

            return target;
        }

        public static void ReflectFrom<T, U>(this T newComponent, U duplicate)// where T : Behaviour where U:Behaviour
        {
            foreach (PropertyInfo x in typeof(T).GetProperties())
            {
                if (!x.CanWrite)
                    continue;

                //Debug.Log("write " + x.Name);

                if (x.Name == "tag")
                    continue;

                if (x.Name == "name")
                    continue;

                if (x.Name == "hideFlags")
                    continue;

                x.SetValue(newComponent, x.GetValue(duplicate));
            }
        }


        #region Instance

        public static void InstanceComponent<T>(this GameObject where, ref T value) where T : Component
        {
            if (null == value)
            {
                if (!where.TryGetComponent(out value))
                    value = where.AddComponent<T>();
            }
        }

        /// <summary>
        /// ensure go if doesn't exist on gameobject
        /// </summary>
        public static Component InstanceIfNull(this GameObject @object, Type type)
        {
            Component component;

            try
            {
                if (!@object.TryGetComponent(type, out component))
                    return @object.AddComponent(type);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                component = null;
            }

            return component;
        }


        /// <summary>
        /// Try get instance with required type or create new one.
        /// </summary>
        public static TResult InstanceIfNull<TResult, TCreation>(this GameObject go) where TCreation : Component, TResult
        {
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
            if (!go.TryGetComponent<TResult>(out var result))
                result = go.AddComponent<TCreation>();
#pragma warning restore UNT0014 // Invalid type for call to GetComponent

            return result;
        }

        /// <summary>
        /// ensure go if doesn't exist on gameobject
        /// </summary>
        public static T InstanceIfNull<T>(this Component component, T defaultField = null) where T : Component
        {
            var go = component.gameObject;
            return go.InstanceIfNull<T>(defaultField);
        }

        /// <summary>
        /// ensure go if doesn't exist on gameobject
        /// </summary>
        public static T InstanceIfNull<T>(this GameObject @object, T defaultField = null) where T : Component
        {
            if (defaultField is null)
                if (!@object.TryGetComponent(out defaultField))
                    return @object.AddComponent<T>();

            return defaultField;
        }

        /// <summary>
        /// ensure go if doesn't exist on gameobject
        /// </summary>
        public static void InstanceIfNull<T>(this GameObject @object, T defaultField, out T outField) where T : Component
        {
            if (defaultField is null)
            {
                if (!@object.TryGetComponent(out outField))
                {
                    outField = @object.AddComponent<T>();
                    return;
                }
            }
            else
                outField = defaultField;
        }

        /// <summary>
        /// Instance go if reference is null
        /// </summary>
        public static void InstanceIfNull<T>(this GameObject @object, ref T value) where T : Component
        {
            //Debug.Log(value);
            if (value is null)
                if (!@object.TryGetComponent(out value))
                    value = @object.AddComponent<T>();

            //Debug.Log(value);
        }

        public static void InstanceIfNull<T>(this GameObject @object, ref T value, Func<T> get) where T : Component
        {
            if (value is null)
                if (!@object.TryGetComponent(out value))
                    value = get();
        }

        public static void InstanceIfNull<T>(this GameObject @object, Func<T> Get, Action<T> Set) where T : MonoBehaviour
        {
            if (Get() is null)
            {
                if (@object.TryGetComponent(out T value))
                    Set(value);
                else
                    Set(@object.AddComponent<T>());
            }
        }


        public static bool TryGetTargetComponent<T>(this GameObject @object, Func<T, bool> check, out T result)
        {
            var components = @object.GetComponents<T>();
            foreach (var component in components)
            {
                if (check(component))
                {
                    result = component;
                    return true;
                }
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Get target go with type or interface
        /// </summary>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Type Safety", "UNT0014:Invalid type for call to GetComponent", Justification = "<¼È¤î>")]
        public static bool TryGetTarget<T>(this GameObject @object, out T target, bool includeChild = true)
        {
            if (includeChild)
            {
                if (typeof(T).IsSubclassOf(typeof(Component)))
                {
                    target = @object.GetComponentInChildren<T>();
                    return true;
                }
                else if (typeof(T).IsInterface)
                {
                    var components = @object.GetComponentsInChildren<Component>();
                    foreach (var c in components)
                    {
                        if (c is T value)
                        {
                            target = value;
                            return true;
                        }
                    }
                }
                else
                    throw new NotImplementedException(typeof(T).ToString());
            }
            else
            {
                if (typeof(T).IsSubclassOf(typeof(Component)))
                {
                    target = @object.GetComponent<T>();
                    return true;
                }
                else if (typeof(T).IsInterface)
                {
                    var components = @object.GetComponents<Component>();

                    foreach (var c in components)
                    {
                        if (c is T value)
                        {
                            target = value;
                            return true;
                        }
                    }
                }
                else
                    throw new NotImplementedException(typeof(T).ToString());
            }

            target = default;
            return false;
        }




        #endregion
    }
}