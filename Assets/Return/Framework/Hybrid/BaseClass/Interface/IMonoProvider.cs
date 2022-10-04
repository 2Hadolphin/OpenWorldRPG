using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    /// <summary>
    /// Provide MonoBehaviour property.
    /// </summary>
    public interface IMonoProvider
    {
        #region Mono

        GameObject gameObject { get; }
        Transform transform { get; }

        #endregion
    }
}