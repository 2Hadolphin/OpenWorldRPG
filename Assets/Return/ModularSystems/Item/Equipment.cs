using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Equipment : MonoBehaviour
{

    public abstract void Init();

    public abstract void Use();

    #region PROPERTIES

    public abstract float UsageDuration
    {
        get;
    }

    #endregion

    /// <summary>
    /// Deactivates the shadows created by the equipment.
    /// </summary>
    public virtual void DisableShadowCasting()
    {
        // For each object that has a renderer inside the equipment CharacterRoot.
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0, l = renderers.Length; i < l; i++)
        {
            renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderers[i].sharedMaterial.EnableKeyword("_VIEWMODEL");
        }
    }

    public abstract void Refill();
}
