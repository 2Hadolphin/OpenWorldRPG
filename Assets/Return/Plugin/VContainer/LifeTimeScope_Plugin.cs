using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Diagnostics;
using VContainer.Internal;

namespace VContainer.Unity
{
    public interface IInjectHandler
    {
        #region Handler Cache

        /// <summary>
        /// Use this method to get module.
        /// </summary>
        bool TryGetModule(Type type, out object module);

        /// <summary>
        /// Use this method to get all modules of type.
        /// </summary>
        IEnumerable<object> GetModules(Type type);

        #endregion

        /// <summary>
        /// Container to resolver parameter.
        /// </summary>
        IObjectResolver Resolver { get; }
    }

    #region ScopeExtension

    public class InjectException : Exception
    {
        public InjectException(string log):base (log)
        {
            
        }
    }

    public static class InjectHandler
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Inject<T>(this IInjectHandler handler, T obj) where T : class
        {
            var cacheInfos=TypeAnalyzer.AnalyzeWithCache(obj.GetType());



            InjectFields(cacheInfos, obj,handler);

            InjectProperties(cacheInfos, obj, handler);

            InjectMethods(cacheInfos, obj, handler, null);

            // inject from exist handler cache

            // inject new instance from container binding **factory

        }


        //public static void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
        //{
        //    InjectFields(instance, resolver);
        //    InjectProperties(instance, resolver);
        //    InjectMethods(instance, resolver, parameters);
        //}


        static void InjectFields(InjectTypeInfo injectTypeInfo, object obj, IInjectHandler resolver)
        {
            if (injectTypeInfo.InjectFields == null)
                return;

            foreach (var x in injectTypeInfo.InjectFields)
            {
                try
                {
                    if (resolver.Resolver is not null and IObjectResolver re)
                        x.SetValue(obj, re.Resolve(x.FieldType));
                }
                catch(VContainerException)
                {
                    if (resolver.TryGetModule(x.FieldType, out var module))
                        x.SetValue(obj, module);
                    else
                        Debug.LogException(
                            new KeyNotFoundException(
                            String.Format(
                                "{0} missing inject module of {1} : {2} and unable to get resolver",
                                obj,
                                x.Name,
                                x.FieldType
                                ))
                            , resolver as UnityEngine.Object);
 
                }
            }
        }

        static void InjectProperties(InjectTypeInfo injectTypeInfo, object obj, IInjectHandler resolver)
        {
            if (injectTypeInfo.InjectProperties == null)
                return;

            foreach (var x in injectTypeInfo.InjectProperties)
            {
                try
                {
                    if (resolver.Resolver is not null and IObjectResolver re)
                        x.SetValue(obj, re.Resolve(x.PropertyType));
                }
                catch (VContainerException)
                {
                    if (resolver.TryGetModule(x.PropertyType, out var module))
                        x.SetValue(obj, module);
                    else
                        throw new KeyNotFoundException(
                            String.Format(
                                "{0} missing inject module of {1} : {2} and unable to get resolver",
                                obj,
                                x.Name,
                                x.PropertyType
                                ));
                }
                catch (KeyNotFoundException e)
                {
                    Debug.LogException(e, resolver as UnityEngine.Object);
                }

                //if (resolver.TryGetModule(x.PropertyType, out var module))
                //    x.SetValue(obj, module);
                //else if (resolver.Resolver is not null and IObjectResolver re)
                //    x.SetValue(obj, re.Resolve(x.PropertyType));
                //else
                //    Debug.LogException(new KeyNotFoundException("Missing inject module and unable to get resolver"));
            }
        }

        static void InjectMethods(InjectTypeInfo injectTypeInfo, object obj, IInjectHandler resolver, IReadOnlyList<IInjectParameter> parameters)
        {
            if (injectTypeInfo.InjectMethods == null)
                return;

            foreach (var method in injectTypeInfo.InjectMethods)
            {
                var parameterInfos = method.ParameterInfos;
                var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(parameterInfos.Length);
                try
                {
                    for (var i = 0; i < parameterInfos.Length; i++)
                    {
                        var parameterInfo = parameterInfos[i];
                        parameterValues[i] = resolver.ResolveParameters(
                            parameterInfo.ParameterType,
                            parameterInfo.Name,
                            parameters);
                    }
                    method.MethodInfo.Invoke(obj, parameterValues);
                }
                catch (VContainerException ex)
                {
                    throw new VContainerException(ex.InvalidType, $"Failed to resolve {injectTypeInfo.Type} : {ex.Message}");
                }
                finally
                {
                    CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
                }
            }
        }


        public static object ResolveParameters(
                this IInjectHandler resolver,
                Type parameterType,
                string parameterName,
                IReadOnlyList<IInjectParameter> parameters)
        {
            if (parameters != null)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.Match(parameterType, parameterName))
                    {
                        return parameter.Value;
                    }
                }
            }



            try
            {
                if (resolver.Resolver is null)
                {
                    if (resolver.TryGetModule(parameterType, out var module))
                        return module;
                }
                else
                    return resolver.Resolver.Resolve(parameterType);
            }
            catch (VContainerException)
            {
                if (resolver.TryGetModule(parameterType, out var module))
                    return module;
            }
 
            throw new InjectException("Failure to inject function parameter.");
        }
    }

    partial class LifetimeScope
    {
        public virtual bool AutoRun { get => autoRun; set => autoRun = value; }




    }

    public partial class BaseHandlerContainer : LifetimeScope
    {

        protected override void Awake()
        {
            // auto bind not build
            if(AutoRun)
            {

            }
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);


        }

    }


    #endregion









    #region Obsolete

//    [DefaultExecutionOrder(-5000)]
//    public partial class obBaseHandlerContainer : MonoBehaviour, IDisposable
//    {
//        public readonly struct ParentOverrideScope : IDisposable
//        {
//            public ParentOverrideScope(obBaseHandlerContainer nextParent) => overrideParent = nextParent;
//            public void Dispose() => overrideParent = null;
//        }

//        public readonly struct ExtraInstallationScope : IDisposable
//        {
//            public ExtraInstallationScope(IInstaller installer) => EnqueueExtra(installer);
//            void IDisposable.Dispose() => RemoveExtra();
//        }

//        [SerializeField]
//        public ParentReference parentReference;



//        [SerializeField]
//        protected List<GameObject> autoInjectGameObjects;

//        static obBaseHandlerContainer overrideParent;
//        static ExtraInstaller extraInstaller;
//        static readonly object SyncRoot = new object();

//        static obBaseHandlerContainer Create(GameObject go = null,IInstaller installer = null)
//        {
//            if(go == null)
//                go = new GameObject("obBaseHandlerContainer");

//            //go.SetActive(false);

//            var newScope = go.AddComponent<obBaseHandlerContainer>();

//            if (installer != null)
//            {
//                newScope.extraInstallers.Add(installer);
//            }

//            //go.SetActive(true);

//            return newScope;
//        }

//        public static obBaseHandlerContainer Create(GameObject go=null,Action<IContainerBuilder> configuration)
//            => Create(go,new ActionInstaller(configuration));

//        public static ParentOverrideScope EnqueueParent(obBaseHandlerContainer parent)
//            => new ParentOverrideScope(parent);

//        public static ExtraInstallationScope Enqueue(Action<IContainerBuilder> installing)
//            => new ExtraInstallationScope(new ActionInstaller(installing));

//        public static ExtraInstallationScope Enqueue(IInstaller installer)
//            => new ExtraInstallationScope(installer);

//        [Obsolete("obBaseHandlerContainer.PushParent is obsolete. Use obBaseHandlerContainer.EnqueueParent instead.", false)]
//        public static ParentOverrideScope PushParent(obBaseHandlerContainer parent) => new ParentOverrideScope(parent);

//        [Obsolete("obBaseHandlerContainer.Push is obsolete. Use obBaseHandlerContainer.Enqueue instead.", false)]
//        public static ExtraInstallationScope Push(Action<IContainerBuilder> installing) => Enqueue(installing);

//        [Obsolete("obBaseHandlerContainer.Push is obsolete. Use obBaseHandlerContainer.Enqueue instead.", false)]
//        public static ExtraInstallationScope Push(IInstaller installer) => Enqueue(installer);

//        public static obBaseHandlerContainer Find<T>(Scene scene) where T : obBaseHandlerContainer => Find(typeof(T), scene);
//        public static obBaseHandlerContainer Find<T>() where T : obBaseHandlerContainer => Find(typeof(T));

//        static obBaseHandlerContainer Find(Type type, Scene scene)
//        {
//            var buffer = UnityEngineObjectListBuffer<GameObject>.Get();
//            scene.GetRootGameObjects(buffer);
//            foreach (var gameObject in buffer)
//            {
//                var found = gameObject.GetComponentInChildren(type) as obBaseHandlerContainer;
//                if (found != null)
//                    return found;
//            }
//            return null;
//        }

//        static obBaseHandlerContainer Find(Type type)
//        {
//            return (obBaseHandlerContainer)FindObjectOfType(type);
//        }

//        static void EnqueueExtra(IInstaller installer)
//        {
//            lock (SyncRoot)
//            {
//                if (extraInstaller != null)
//                    extraInstaller.Add(installer);
//                else
//                    extraInstaller = new ExtraInstaller { installer };
//            }
//        }

//        static void RemoveExtra()
//        {
//            lock (SyncRoot) extraInstaller = null;
//        }

//        public IObjectResolver Container { get; private set; }
//        public obBaseHandlerContainer Parent { get; private set; }
//        public bool IsRoot { get; set; }

//        readonly List<IInstaller> extraInstallers = new List<IInstaller>();

//        protected virtual void Awake() { }

//        public virtual void Load()
//        {
//            try
//            {
//                Parent = GetRuntimeParent();
//                Build();
//            }
//            catch (VContainerParentTypeReferenceNotFound) when (!IsRoot)
//            {
//                if (WaitingList.Contains(this))
//                {
//                    throw;
//                }
//                EnqueueAwake(this);
//            }
//        }

//        protected virtual void OnDestroy()
//        {
//            DisposeCore();
//        }

//        protected virtual void Configure(IContainerBuilder builder) { }

//        public void Dispose()
//        {
//            DisposeCore();
//            if (this != null) Destroy(gameObject);
//        }

//        public void DisposeCore()
//        {
//            Container?.Dispose();
//            Container = null;
//            CancelAwake(this);
//        }

//        public void Build()
//        {
//            if (Parent == null)
//                Parent = GetRuntimeParent();

//            if (Parent != null)
//            {
//                Container = Parent.Container.CreateScope(builder =>
//                {
//                    builder.ApplicationOrigin = this;
//                    builder.Diagnostics = VContainerSettings.DiagnosticsEnabled ? DiagnositcsContext.GetCollector(name) : null;
//                    InstallTo(builder);
//                });
//            }
//            else
//            {
//                var builder = new ContainerBuilder
//                {
//                    ApplicationOrigin = this,
//                    Diagnostics = VContainerSettings.DiagnosticsEnabled ? DiagnositcsContext.GetCollector(name) : null,
//                };
//                InstallTo(builder);
//                Container = builder.Build();
//            }

//            extraInstallers.Clear();

//            AutoInjectAll();
//            AwakeWaitingChildren(this);
//        }

//        public obBaseHandlerContainer CreateChild(IInstaller installer = null)
//        {
//            var childGameObject = new GameObject("obBaseHandlerContainer (Child)");
//            childGameObject.SetActive(false);
//            childGameObject.transform.SetParent(transform, false);
//            var child = childGameObject.AddComponent<obBaseHandlerContainer>();
//            if (installer != null)
//            {
//                child.extraInstallers.Add(installer);
//            }
//            child.parentReference.Object = this;
//            childGameObject.SetActive(true);
//            return child;
//        }

//        public obBaseHandlerContainer CreateChild(Action<IContainerBuilder> installation)
//            => CreateChild(new ActionInstaller(installation));

//        public obBaseHandlerContainer CreateChildFromPrefab(obBaseHandlerContainer prefab, IInstaller installer = null)
//        {
//            var wasActive = prefab.gameObject.activeSelf;
//            if (wasActive)
//            {
//                prefab.gameObject.SetActive(false);
//            }
//            var child = Instantiate(prefab, transform, false);
//            if (installer != null)
//            {
//                child.extraInstallers.Add(installer);
//            }
//            child.parentReference.Object = this;
//            if (wasActive)
//            {
//                prefab.gameObject.SetActive(true);
//                child.gameObject.SetActive(true);
//            }
//            return child;
//        }

//        public obBaseHandlerContainer CreateChildFromPrefab(obBaseHandlerContainer prefab, Action<IContainerBuilder> installation)
//            => CreateChildFromPrefab(prefab, new ActionInstaller(installation));

//        void InstallTo(IContainerBuilder builder)
//        {
//            Configure(builder);

//            foreach (var installer in extraInstallers)
//            {
//                installer.Install(builder);
//            }

//            ExtraInstaller extraInstallerStatic;
//            lock (SyncRoot)
//            {
//                extraInstallerStatic = obBaseHandlerContainer.extraInstaller;
//            }
//            extraInstallerStatic?.Install(builder);

//            builder.RegisterInstance<obBaseHandlerContainer>(this).AsSelf();
//        }

//        obBaseHandlerContainer GetRuntimeParent()
//        {
//            if (IsRoot) return null;

//            if (parentReference.Object != null)
//                return parentReference.Object;

//            // Find in scene via type
//            if (parentReference.Type != null && parentReference.Type != GetType())
//            {
//                var found = Find(parentReference.Type);
//                if (found != null && found.Container != null)
//                {
//                    return found;
//                }
//                throw new VContainerParentTypeReferenceNotFound(
//                    parentReference.Type,
//                    $"{name} could not found parent reference of type : {parentReference.Type}");
//            }

//            var nextParent = overrideParent;
//            if (nextParent != null)
//                return nextParent;

//            // Find root from settings
//            if (VContainerSettings.Instance != null)
//            {
//                var rootBaseHandlerContainer = VContainerSettings.Instance.RootLifetimeScope;
//                if (rootBaseHandlerContainer != null)
//                {
//                    if (rootBaseHandlerContainer.Container == null)
//                    {
//                        rootBaseHandlerContainer.Build();
//                    }
//                    return rootBaseHandlerContainer;
//                }
//            }
//            return null;
//        }

//        void AutoInjectAll()
//        {
//            if (autoInjectGameObjects == null)
//                return;

//            foreach (var target in autoInjectGameObjects)
//            {
//                if (target != null) // Check missing reference
//                {
//                    Container.InjectGameObject(target);
//                }
//            }
//        }
//    }

//    partial class obBaseHandlerContainer
//    {
//        static readonly List<obBaseHandlerContainer> WaitingList = new List<obBaseHandlerContainer>();

//#if UNITY_2019_3_OR_NEWER
//        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//#else
//        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//#endif
//        static void SubscribeSceneEvents()
//        {
//            SceneManager.sceneLoaded -= OnSceneLoaded;
//            SceneManager.sceneLoaded += OnSceneLoaded;
//        }

//        static void EnqueueAwake(obBaseHandlerContainer BaseHandlerContainer)
//        {
//            WaitingList.Add(BaseHandlerContainer);
//        }

//        static void CancelAwake(obBaseHandlerContainer BaseHandlerContainer)
//        {
//            WaitingList.Remove(BaseHandlerContainer);
//        }

//        static void AwakeWaitingChildren(obBaseHandlerContainer awakenParent)
//        {
//            if (WaitingList.Count <= 0) return;

//            var buf = new List<obBaseHandlerContainer>();

//            for (var i = WaitingList.Count - 1; i >= 0; i--)
//            {
//                var waitingScope = WaitingList[i];
//                if (waitingScope.parentReference.Type == awakenParent.GetType())
//                {
//                    waitingScope.parentReference.Object = awakenParent;
//                    WaitingList.RemoveAt(i);
//                    buf.Add(waitingScope);
//                }
//            }

//            foreach (var waitingScope in buf)
//            {
//                waitingScope.Awake();
//            }
//        }

//        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//        {
//            if (WaitingList.Count <= 0)
//                return;

//            var buf = new List<obBaseHandlerContainer>();

//            for (var i = WaitingList.Count - 1; i >= 0; i--)
//            {
//                var waitingScope = WaitingList[i];
//                if (waitingScope.gameObject.scene == scene)
//                {
//                    WaitingList.RemoveAt(i);
//                    buf.Add(waitingScope);
//                }
//            }

//            foreach (var waitingScope in buf)
//            {
//                waitingScope.Awake(); // Re-throw if parent not found
//            }
//        }
//    }

    #endregion

}