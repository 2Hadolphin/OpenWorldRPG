namespace Return.Framework.DI
{
    /// <summary>
    /// Generic cast parameters.    **Instance pool **Injection cast
    /// </summary>
    public interface IResolvable
    {
        void Resolve(IResolver resolver);
    }
}