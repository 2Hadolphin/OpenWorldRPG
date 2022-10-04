using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Zenject;

namespace Return
{
    /// <summary>
    /// Provide delta time.
    /// </summary>
    [DefaultExecutionOrder(ExecuteOrderList.Framework)]
    public class ConstCache :BaseComponent //IInitializable, Zenject.ITickable, IFixedTickable
    {
        #region TimeCatch

        protected static int _Frame;
        public static int Frame { get { return _Frame; } }

        protected static float _deltaTime;
        public static float deltaTime { get { return _deltaTime; } }
        protected static float _fixedDeltaTime;
        public static float fixeddeltaTime { get { return _fixedDeltaTime; } }
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
        public static readonly WaitForSeconds WaitForOneSecond = new WaitForSeconds(1f);
        protected static Dictionary<float, WaitForSeconds> mWaitForSeconds = new Dictionary<float, WaitForSeconds>();
        public static WaitForSeconds WaitForSeconds(float t)
        {
            if (!mWaitForSeconds.TryGetValue(t, out var timer))
            {
                timer = new WaitForSeconds(t);

                if (t < 60 && Mathf.RoundToInt(t) == t)
                    mWaitForSeconds.Add(t, timer);
            }

            return timer;
        }


        #endregion

        //#region Layer
        //public static PhysicConfig PhysicSetting= PhysicConfig.Instance;
        //#endregion


        #region CURSOR CAPTURE

        private static bool m_CaptureMouseCursor;

        /// <summary>
        /// valid cursor enable for ui or game play.
        /// </summary>
        public static bool captureMouseCursor
        {
            get { return m_CaptureMouseCursor; }
            set
            {
                m_CaptureMouseCursor = value;
                if (m_CaptureMouseCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        #endregion



        //public void Initialize()
        //{
        //    Debug.Log(this);
        //}

        //public void FixedTick()
        //{
        //    FixedUpdate();
        //}

        //void Zenject.ITickable.Tick()
        //{
        //    Update();
        //}



        //public override void LoadInitilization_BeforeSceneLoad()
        //{
        //    LogSystem(this);
        //}


        private void FixedUpdate()
        {
            _fixedDeltaTime = Time.fixedDeltaTime;
        }

        private void Update()
        {
            _deltaTime = Time.deltaTime;
            _Frame = Time.frameCount;
        }

    }


}

