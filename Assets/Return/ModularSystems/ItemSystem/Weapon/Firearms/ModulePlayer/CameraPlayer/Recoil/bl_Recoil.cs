using UnityEngine;
using Return;


/// <summary>
/// Recoil FPHandleCam and FPMainCam
/// </summary>
public class bl_Recoil : bl_RecoilBase
{
    /// <summary>
    /// ReadOnlyTransform of first person main camera slot
    /// </summary>
    public Transform FPMainCamSlotTF;

    /// <summary>
    /// ReadOnlyTransform of first person main camera slot
    /// </summary>
    public Transform FPMainCamTF;

    /// <summary>
    /// m_items handle camera
    /// </summary>
    public Transform FPHandleCamTF;

    public bool OnRecoil = false;



    /// <summary>
    /// ????
    /// </summary>
    [Range(1, 25)] 
    public float MaxRecoil = 5;

    /// <summary>
    /// ????? Push recoil to temperary effect slot or player camera directly
    /// </summary>
    public bool AutomaticallyComeBack = true;

    #region Private members
    private Vector3 RecoilRot;
    private float Recoil = 0;
    private float RecoilSpeed = 2;

    private float lerpRecoil = 0;
    #endregion

    protected override void Start()
    {
        base.Start();
        OnReloadConfig();
    }

    public virtual void OnReloadConfig()
    {
        RecoilRot = FPHandleCamTF.localEulerAngles;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Update()
    {
        RecoilControl();
    }

    /// <summary>
    /// 
    /// </summary>
    void RecoilControl()
    {
        if (OnRecoil)
        {
            if (AutomaticallyComeBack)
            {
                Quaternion q = Quaternion.Euler(new Vector3(-Recoil, 0, 0));
                FPMainCamSlotTF.localRotation = Quaternion.Slerp(FPMainCamSlotTF.localRotation, q, ConstCache.deltaTime * RecoilSpeed);
            }
            else
            {
                lerpRecoil = Mathf.Lerp(lerpRecoil, Recoil, ConstCache.deltaTime * RecoilSpeed);
                FPMainCamTF.Rotate(-lerpRecoil, 0, 0, Space.Self);
            }
        }
        else
        {
            if (AutomaticallyComeBack)
            {
                BackToOrigin(FPMainCamSlotTF);
            }
            else
            {
                Recoil = 0;
                lerpRecoil = 0;
                //fpController.GetMouseLook().CombineVerticalOffset();
            }
        }

        if (false)//GunManager.CurrentGun == null)
        {
            BackToOrigin(FPMainCamSlotTF);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void BackToOrigin(Transform tf)
    {
        if (tf == null) 
            return;

        Quaternion q = Quaternion.Euler(RecoilRot);
        tf.localRotation = Quaternion.Slerp(tf.localRotation, q, ConstCache.deltaTime * RecoilSpeed);
        Recoil = tf.localEulerAngles.x;
    }

    /// <summary>
    /// Add recoil
    /// </summary>
    public override void SetRecoil(RecoilData data)
    {
        Recoil += data.Amount;
        Recoil = Mathf.Clamp(Recoil, 0, MaxRecoil);
        RecoilSpeed = data.Speed;
    }


}