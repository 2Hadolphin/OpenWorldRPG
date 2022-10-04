using UnityEngine;
using Sirenix.OdinInspector;

namespace Return.Cameras
{
    public partial class FreeCamera : CustomCamera
    {
        InputHandle Input;

        [SerializeField]
        [Required]
        AudioListener Listener;

        #region Config

        [SerializeField]
        private bool LockWhileStart;


        #endregion

        Transform Transform;


        public bool Tracing = false;

        public Transform TracingTarget;

        public Vector3 TracingDistance;

        public bool InvertAxisX = false;
        public bool InvertAxisY = false;

        public string LeftRight = "Mouse Y";
        public string UpDown = "Mouse X";

        public KeyCode Forward = KeyCode.W;
        public KeyCode Right = KeyCode.D;
        public KeyCode Left = KeyCode.A;
        public KeyCode Backward = KeyCode.S;

        public KeyCode Sprint = KeyCode.LeftShift;

        [Range(0, 100)]
        [SerializeField]
        private float MovSensivitity = 17f;

        [Range(0, 60)]
        [SerializeField]
        private float RotSensivitity = 30f;

        [Range(1,5f)]
        [SerializeField]
        private float SpeedMultiply=2f;

        bool SpeedUp=false;

        Vector2 Mouse;

        Vector2 Movement;

        public override Transform HostObject => TracingTarget;


        public override void SetTarget(Transform tf)
        {
            TracingTarget = tf;
        }

        protected override void Awake()
        {
            base.Awake();

            Transform = transform;

            if (CreateIfNull(ref Input))
            {
                Input.RegisterInput();
                Input.SetHandler(this);
                
            }
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            //Input.enabled = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            //Input.enabled = false;
        }

        protected override void SetHandles(bool enable)
        {
            base.SetHandles(enable);

            if (Input != null)
                Input.enabled = enable;

            LockMouse(enable);

            if (Listener != null)
                Listener.enabled = enable;
        }

        private void LateUpdate()
        {
            if (Movement != Vector2.zero)
            {
                CamMove();
            }

            if (Mouse != Vector2.zero)
            {
                float xRot = Transform.eulerAngles.x;
                if (xRot > 180)
                    xRot -= 360;

                xRot -= (InvertAxisX ? -1 : 1) * Mouse.y * RotSensivitity * ConstCache.deltaTime;
                xRot = xRot < -89 ? -89 : xRot;

                Transform.eulerAngles = new(xRot, Transform.eulerAngles.y + (InvertAxisX ? -1 : 1) * Mouse.x * RotSensivitity * ConstCache.deltaTime);
            }

        }

        private void LockMouse(bool lockMouse=true)
        {
            if (lockMouse)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
        }

        private void Update()
        {
            if (Tracing)
            {
                Transform.position = TracingTarget.position + TracingTarget.TransformDirection(TracingDistance);
                Transform.LookAt(TracingTarget, Vector3.up);
            }


        }




        private void CamMove()
        {
            var speed = MovSensivitity * ConstCache.deltaTime;

            if (SpeedUp)
                speed*= SpeedMultiply;

            Transform.position += (ReadOnlyTransform.right * Movement.x + ReadOnlyTransform.forward * Movement.y).Multiply(speed);
        }
    }
}