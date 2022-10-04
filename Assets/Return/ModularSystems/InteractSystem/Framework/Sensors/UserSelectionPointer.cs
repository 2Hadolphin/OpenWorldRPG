using System;
using Return.Cameras;

namespace Return.InteractSystem
{
    /// <summary>
    /// Select obj via and bind with player camera.
    /// </summary>
    public class UserSelectionPointer : SelectionPointer
    {
        //public override event Action OnSelectedChanged;
        //public override event Action<InteractWrapper> OnSelected;
        //public override event Action<InteractWrapper> OnDeselected;


        protected override void OnEnable()
        {
            CameraManager.OnMainCameraHandlerChanged += OnMainCameraHandlerChanged;
            var cam = CameraManager.mainCameraHandler;
            ViewTransform = cam.mainCameraTransform;
            base.OnEnable();
        }

        protected virtual void OnMainCameraHandlerChanged(CustomCamera cam)
        {
            if (cam == null)
                return;

            ViewTransform = cam.mainCameraTransform;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CameraManager.OnMainCameraHandlerChanged -= OnMainCameraHandlerChanged;
        }
    }
}