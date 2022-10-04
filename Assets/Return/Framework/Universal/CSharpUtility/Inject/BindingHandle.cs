using System;
using System.Collections.Generic;

namespace Return.Framework.DI
{
    public class BindingHandle
    {
        public Container Container;

        public Type Type;

        public object Source;

        public IFactory Factory;

        public InjectOption Option;

        public HashSet<Type> Bindings=new(5);

        public bool singleton;

        public void Clean()
        {
            Container = null;
            Source = null;
            Option = 0;
            Bindings.Clear();
            singleton = false;
            Factory = null;
        }
    }
}