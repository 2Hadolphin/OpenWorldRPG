



namespace Return
{
    /// <summary>
    /// ??? set parameter?
    /// </summary>
    public interface IModifier : IEntity<string>
    {
    #region Public Variables

        int          Amount { get; }
        ModifierType Type   { get; }

    #endregion
    }
}