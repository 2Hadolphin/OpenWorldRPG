using UnityEngine;
using System;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

namespace Return
{
    public abstract class BaseComponent : MonoBehaviour
    {
#if UNITY_EDITOR
        protected virtual string EditorLabel => null;
        private bool hideLabel => string.IsNullOrEmpty(EditorLabel);

        [PropertyOrder(-999)]
        [HideIf(nameof(hideLabel))]
        [OnInspectorGUI]
        void DrawEditorLabel()
        {
            GUILayout.Label(EditorLabel);
        }
#endif

        public TResult InstanceIfNull<TResult,TCreation>() where TCreation : Component, TResult
        {
            return gameObject.InstanceIfNull<TResult, TCreation>();
        }

        public T InstanceIfNull<T>() where T : Component
        {
            return gameObject.InstanceIfNull<T>();
        }

        public void InstanceIfNull<T>(ref T component) where T : Component
        {
            gameObject.InstanceIfNull(ref component);
        }


        public static T CreateIfNull<T>(T obj) where T : class, new()
        {
            var isNull = obj.IsNull();

            if (isNull)
                obj = new();

            return obj;
        }

        /// <summary>
        /// Create instance if reference is null. => new()
        /// </summary>
        /// <returns>return true if create new obj.</returns>
        public static bool CreateIfNull<T>(ref T @object ) where T : class,new()
        {
            var isNull = @object.IsNull();

            if (isNull)
                @object = new();

            return isNull;
        }

        public Component InstanceIfNull(Type type)// where T:UnityEngine.Component
        {
            return gameObject.InstanceIfNull(type);
        }

        /// <summary>
        /// Overwrite destory with extension.
        /// </summary>
        protected new void Destroy(Object obj)
        {
            obj.Destroy();
        }

        protected void Destroy(object obj)
        {
            if (obj is Component c)
                Destroy(c);
            else if (obj is IDisposable disposable)
                disposable.Dispose();
            else
                Debug.LogWarning($"Destroy with invalid type {obj}");
        }
    }
}