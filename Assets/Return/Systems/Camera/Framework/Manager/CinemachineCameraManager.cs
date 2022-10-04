using UnityEngine;

namespace Return.Cameras
{
    public class CinemachineCameraManager : CameraManager
    {
        //VirtualCamera m_virtualCamera;

        //public VirtualCamera virtualCamera
        //{
        //    get
        //    {
        //        if (m_virtualCamera.IsNull())
        //        {
        //            m_virtualCamera = new GameObject(nameof(VirtualCamera)).AddComponent<VirtualCamera>();
        //            m_virtualCamera.gameObject.hideFlags = HideFlags.DontSave;
        //        }

        //        return m_virtualCamera;
        //    }
        //}


        //protected override void SetCamera(CustomCamera camera, bool enable)
        //{
        //    base.SetCamera(camera, enable);

        //    if(enable)
        //    {
        //        if (camera is IVirtualCamera virtualCam)
        //        {
        //            virtualCam.SetCamera(virtualCamera.Camera);
        //            virtualCam.SetBrain(virtualCamera.Brain);
        //        }
        //    }
        //}
    }
}

