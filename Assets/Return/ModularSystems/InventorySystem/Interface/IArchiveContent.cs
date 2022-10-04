using System;


namespace Return.Inventory
{
    /// <summary>
    /// Container to cache storage info and instance reference.
    /// </summary>
    public interface IArchiveContent : IEquatable<IArchiveContent>
    {
        public object content { get; }

        /// <summary>
        /// Content size.
        /// </summary>
        public abstract uint Volume { get; }



        public int GetHashCode()
        {
            if (content.IsNull())
                return 0;
            else
                return content.GetHashCode();
        }

        public new bool Equals(IArchiveContent other)
        {
            return content.Equals(other.content);
        }

    }


}