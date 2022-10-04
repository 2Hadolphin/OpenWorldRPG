using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
public class m_BlurManager : MonoBehaviour
{
    public Camera UICamera;
    public Camera TopUICamera;

    public Canvas BaseCanvas;
    public Canvas TopCanvas;

    public RenderTexture RenderTexture;

    public Image BlurImage;
    public Image BlurBaseImage;

    [Button("SetRenderTexture")]
    protected virtual void SetRenderTexture()
    {
        //RenderTexture=RenderTexture.GetTemporary(UICamera.pixelWidth, UICamera.pixelHeight);
        if (RenderTexture.IsCreated())
            RenderTexture.Release();

        RenderTexture.width = UICamera.pixelWidth;
        RenderTexture.height = UICamera.pixelHeight;

        RenderTexture.Create();

        UICamera.targetTexture = RenderTexture;
        gameObject.SetActive(true);
        TopUICamera.enabled = true;
    }

    [Button("RemoveRenderTexture")]
    protected virtual void RemoveRenderTexture()
    {
        UICamera.targetTexture = null;
        gameObject.SetActive(false);
        TopUICamera.enabled = false;
        RenderTexture.Release();
        //DestroyImmediate(RenderTexture);
    }

    [Range(0,1f),OnValueChanged(nameof(EvaluteBlur))]
    public float Blur;

    void EvaluteBlur()
    {
        if (Blur > 0 && !isActiveAndEnabled)
            SetRenderTexture();
        else if (Blur == 0 && isActiveAndEnabled)
            RemoveRenderTexture();

        BlurImage.material.SetFloat("_Blur", Blur);
        //var c=BlurBaseImage.color;
        //c.a = 1 - Blur;
        //BlurBaseImage.color = c;
    }
}
