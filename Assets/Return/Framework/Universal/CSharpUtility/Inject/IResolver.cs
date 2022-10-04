using Return.Framework.DI;
using System;
using System.Collections.Generic;

namespace Return
{
    /// <summary>
    /// Interface to binding modules and inject parameters.
    /// </summary>
    public interface IResolver
    {
        BindingHandle BindInterface<T>();

        BindingHandle BindInterface(IEnumerable<Type> types);
        BindingHandle BindInterface(params Type[] types);



        void Inject(object obj);
    }
}