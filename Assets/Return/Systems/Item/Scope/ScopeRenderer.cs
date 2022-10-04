using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class ScopeRenderer : MonoBehaviour
{
    // to data
    [Required][SerializeField]
    public MeshRenderer Scope;

    protected Camera ScopeCamera;
    protected RenderTexture ScopeTexture;
    protected Material OriginMat;
    protected Material RenderingMat;
    public bool ForceRender;
    public bool IsRenderering { get; protected set; } = false;

    public virtual void Init(ScopeData data)
    {
        // save data

        // get cam distance and view ratio
        ScopeTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
        if (!ScopeTexture.IsCreated())
            ScopeTexture.Create();

        ScopeCamera = new GameObject("ScopeCam").AddComponent<Camera>();
        ScopeCamera.targetTexture = ScopeTexture;

        OriginMat = Scope.sharedMaterial;
        RenderingMat = Scope.material;
        RenderingMat.mainTexture = ScopeTexture;

        var camtf = ScopeCamera.transform;
        camtf.parent = transform;
        camtf.localPosition = default;
        camtf.localRotation = default;
    }

    // set form scope to scope renderer
    private void OnBecameVisible()
    {
        if (ForceRender|| IsRenderering)
            return;


    }

    private void OnBecameInvisible()
    {
        if (!ForceRender || IsRenderering)
            return;

        StopAllCoroutines();
    }

    

    private void OnDestroy()
    {
        if (RenderingMat)
            Destroy(RenderingMat);

        if (ScopeCamera)
            Destroy(ScopeCamera.gameObject);

        if (ScopeTexture)
        {
            ScopeTexture.Release();
            Destroy(ScopeTexture);
        }

        if (OriginMat)
            Scope.sharedMaterial = OriginMat;
    }


}

public class ScopeData
{

}