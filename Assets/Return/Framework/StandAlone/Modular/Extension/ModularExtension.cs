using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Assertions;

namespace Return.Modular
{
    public static class ModularExtension
    {
        public static T[] LoadModules<T>(this GameObject @object)
        {
            T[] modules;

            if (@object.TryGetComponent<IModularResolver>(out var handle))
                modules = handle.GetModules<T>().ToArray();
            else
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
                modules = @object.GetComponentsInChildren<T>();
#pragma warning restore UNT0014 // Invalid type for call to GetComponent

            return modules;
        }

        public static void LoadModules<T>(this GameObject @object,Action<T> action)
        {
            if (@object.TryGetComponent<IModularResolver>(out var handle))
            {
                var ports = handle.GetModules<T>();

                foreach (var port in ports)
                    action(port);
            }
            else
            {
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
                var ports = @object.GetComponentsInChildren<T>();
#pragma warning restore UNT0014 // Invalid type for call to GetComponent

                foreach (var port in ports)
                    action(port);
            }
        }
    }
}