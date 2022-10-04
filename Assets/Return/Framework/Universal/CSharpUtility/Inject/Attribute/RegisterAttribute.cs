using System;

namespace Return
{
    /// <summary>
    /// Attribute to sign source register.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class RegisterAttribute : CastAttribute
    {

    }
}