using UnityEngine;

namespace Return.InteractSystem
{
    public interface ICameraConvert
    {
        /// <summary>
        /// Point to draw anchor of UI.
        /// </summary>
        Vector3 position { get; }

        /// <summary>
        /// Rect to 
        /// </summary>
        Rect rect { set; }
    }
}