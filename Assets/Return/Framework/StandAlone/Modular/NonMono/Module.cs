namespace Return.Modular
{
    /// <summary>
    /// Non-Mono base class for generic handler to control life cycle.
    /// </summary>
    public abstract class Module<T> : AbstractModule, IModule<T>
    {
        //void IModule.SetHandler(object obj)
        //{
        //    if (obj is T handler)
        //        SetHandler(handler);
        //    else
        //        Debug.LogException(new InvalidCastException($"Set [{typeof(T)}] handler failure with {obj}."));
        //}

        //public override void SetHandler(object obj)
        //{
        //    if (obj is T handler)
        //        SetHandler(handler);
        //    else
        //        Debug.LogException(new InvalidCastException($"Set [{typeof(T)}] handler failure with {obj}."));
        //}

        public abstract void SetHandler(T module);
    }
}