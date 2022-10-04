using System;

namespace Return
{
    /// <summary>
    /// Attribute to sign source injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class InjectAttribute : CastAttribute
    {
        public InjectAttribute() { }

        public InjectAttribute(InjectOption injectOption)
        {
            InjectOption = injectOption;
        }

        public InjectAttribute(string id)
        {
            ID = id;
        }
    }

    /// <summary>
    /// Interface for resolver to identify and inject parameters.
    /// </summary>
    public interface IInjectable { }
}