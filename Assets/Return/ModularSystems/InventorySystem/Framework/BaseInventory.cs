using System;
using TNet;
using Return.Modular;

namespace Return.Inventory
{
    /// <summary>
    /// Base class to archive game content.
    /// </summary>
    public abstract class BaseInventory : MonoModule, IBaseInventory,IDataNodeSerializable
    {
        #region IDataNodeSerializable

        public virtual void Deserialize(DataNode node) { }

        public virtual void Serialize(DataNode node) { }

        #endregion

        #region IBaseInventory

        public abstract bool Search(Func<IArchiveContent, bool> method, out object obj);

        public abstract bool CanStorage(object obj);

        public abstract void Store(object obj);

        public abstract bool CanExtract(object obj);

        public abstract void Extract(object obj);


        #endregion



    }


}