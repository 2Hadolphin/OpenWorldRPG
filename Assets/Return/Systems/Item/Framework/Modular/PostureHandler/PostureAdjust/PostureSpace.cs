using UnityEngine;

namespace Return.Items
{
    [Tooltip("Prime Handle using parent space, ")]
    public enum PostureSpace
    {
        /// <summary>
        /// Upchest Space for not additive, selfSpace(rotation*offsetVector) for additive.
        /// </summary>
        HandleRoot,

        /// <summary>
        /// m_items Space
        /// </summary>
        AssistHandle,

        #region Additive



        #endregion
    }
}