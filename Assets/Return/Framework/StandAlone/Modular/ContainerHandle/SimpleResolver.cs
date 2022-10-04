using Return.Framework.DI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;

namespace Return.Modular
{



    /// <summary>
    /// Binding module instance to container.
    /// </summary>
    public class SimpleResolver : BaseResolver
    {
        protected override void SetupContainer()
        {
            // set container
            Container = new();

            Container.RegisterModule(Container);
        }


        #region IResolver Wrapper

        [ShowInInspector]
        LitContainer Container;

        protected override IModularResolver Resolver => Container;

        #endregion



        #region obsolete

        //IEnumerable<object> IModularResolver.Modules => GetModules();
        //void IModularResolver.Inject(object obj) => Inject(obj);

        //public virtual bool Inject(object obj)
        //{
        //    try
        //    {
        //        Assert.IsFalse(obj.IsNull());

        //        obj.Inject(Modules);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogException(e);
        //        return false;
        //    }

        //    return true;
        //}


        ///// <summary>
        ///// Register module to container.
        ///// </summary>
        //public virtual void RegisterModule(object module)
        //{
        //    Assert.IsFalse(module.IsNull());

        //    var type = module.GetType();

        //    var valid = Modules.ContainsKey(type);

        //    if (valid)
        //    {
        //        Modules.Add(type, module);
        //        OnModuleRegister(module);
        //    }

        //    if (valid)
        //        Debug.Log("RegisterHandler itemModule module : " + module.GetType().Name);
        //    else
        //        Debug.LogError("RegisterHandler itemModule module already exist : " + module);
        //}

        ///// <summary>
        ///// Unregister module from container.
        ///// </summary>
        //public virtual void UnregisterModule(object module)
        //{
        //    try
        //    {
        //        throw new NotImplementedException($"{nameof(UnregisterModule)} has not finish.");


        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogException(e);
        //    }
        //}

        //public virtual bool TryGetModule<T>(out T module, Type defaultTypeToInstance = null) where T : class//, IBaseModuleState
        //{
        //    var exist = Modules.TryGetValueOfType(out module);

        //    if (!exist && defaultTypeToInstance != null)
        //    {
        //        //var type = typeof(TSerializable);
        //        //if (type.IsSubclassOf(typeof(Component)))
        //        {
        //            module = InstanceIfNull(defaultTypeToInstance) as T;
        //            exist = true;
        //        }
        //    }

        //    return exist;
        //}

        ///// <summary>
        ///// Return all module instance.
        ///// </summary>
        //protected virtual IEnumerable<object> GetModules()
        //{
        //    foreach (var pair in Modules)
        //        yield return pair.Value;
        //}


        //public IEnumerable<T> GetModules<T>()
        //{
        //    foreach (var module in Modules)
        //        if (module.Value is T value)
        //            yield return value;
        //}

        //public virtual bool TryGetModule(Type type, out object module)
        //{
        //    return Modules.TryGetValue(type, out module);
        //}

        //public IEnumerable<object> GetModules(Type type)
        //{
        //    return Modules.Where(x => x.Key.IsSubclassOf(type)).Select(x=>x.Value);
        //}
        #endregion
    }


}