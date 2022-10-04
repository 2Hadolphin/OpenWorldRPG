using System;

namespace Return
{
    /// <summary>
    /// C# space null check class.
    /// </summary>
    public abstract class NullCheck : /*UnityEngine.Object,*/ICloneable
    {
        public static implicit operator bool(NullCheck exists) => exists != null;

        public virtual object Clone()
        {
            var clone = this.MemberwiseClone();
            return clone;
        }
    }



}