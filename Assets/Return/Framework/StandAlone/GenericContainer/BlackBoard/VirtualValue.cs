namespace Return
{
    /// <summary>
    /// Value type of content.
    /// </summary>
    public enum VirtualValue : byte
    {
        /// <summary>
        /// Boolean value
        /// </summary>
        Bool = 1,

        /// <summary>
        /// Float type parameter.
        /// </summary>
        Float = 2,

        /// <summary>
        /// Int type parameter
        /// </summary>
        Int = 3,

        /// <summary>
        /// Vector2 type parameter
        /// </summary>
        Vector2=4,

        /// <summary>
        /// Vector3 type parameter
        /// </summary>
        Vector3=5,

        /// <summary>
        /// Vector4 type parameter
        /// </summary>
        Vector4=6,

        /// <summary>
        /// Quaternion type parameter
        /// </summary>
        Quaternion=7,

        /// <summary>
        /// Action Interface
        /// </summary>
        Trigger = 9,

        /// <summary>
        /// Custom context.
        /// </summary>
        Generic =10,
    }
}