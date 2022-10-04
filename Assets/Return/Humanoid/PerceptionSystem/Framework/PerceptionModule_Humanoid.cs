using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Inputs;
using UnityEngine.InputSystem;
using Return.Cameras;
using System.Linq;
using System;
using UnityEngine.Assertions;
using Return.Modular;

namespace Return.Perception
{
    /// <summary>
    /// Local user module, set view port config.   **characterRenderer layer
    /// </summary>
    public class PerceptionModule_Humanoid : PerceptionModule, IPerceptionModule
    {

        const string str_FirstPerson = "FirstPerson";
        const string str_HumanoidCharacterModel = "Model_0"; 
        const string str_HumanoidHand = "Model_Arms";
        const string str_HumanoidBody = "Model_Body";

        PerceptionModule_UserInputHandle InputHandle;

        protected override void Register()
        {
            //base.Register();
            //Debug.LogException(new Exception("Set character perception."));

            // input
            {
                InputHandle = new();
                InputHandle.RegisterInput(InputManager.Input);
                InputHandle.SetHandler(this);
                InputHandle.enabled = true;
            }

            // camera
            {
                // get activate camera handle
                var cam = CameraManager.GetCamera<FirstPersonCamera>();

                // dev
                {
                    cam.enabled = true;

                    // hide hand camera when without item **dev
                    cam.HandleCamera.enabled = false;
                }


                //if (CameraManager.ActivateCameraTypes.HasFlag(CameraManager.FirstPersonCamera))
                if (true)
                    SwitchFirstPersonViewport();
                else
                    SwitchThirdPersonViewport();
            }

            SplitMeshs();
        }




        #region IPerception
        protected bool enableOverlapCamera;

        public SkinnedMeshRenderer HandRenderer { get => m_handRenderer; set => m_handRenderer = value; }
        public SkinnedMeshRenderer BodyRenderer { get => m_bodyRenderer; set => m_bodyRenderer = value; }
        public SkinnedMeshRenderer CharacterRenderer { get => m_characterRenderer; set => m_characterRenderer = value; }

        public virtual void SetOverlapCamera(bool enable)
        {
            enableOverlapCamera = enable;
            SetFirstPersonOverlapCamera();
        }

        protected virtual void SetFirstPersonOverlapCamera()
        {
            var cam = CameraManager.GetCamera<FirstPersonCamera>();
            Assert.IsFalse(cam == null);

            //cam.HandleCamera.enabled = enableOverlapCamera;
        }

        #endregion




        [Button]
        protected virtual void SwitchFirstPersonViewport()
        {
            var cam=CameraManager.GetCamera<FirstPersonCamera>();
            CameraManager.SwitchCamera(cam);

            if (transform.FindChild("FPHandleCamSlot", out var handle))
                cam.FirstPersonHandleTransform = handle;
            else
                Debug.LogError("Can't found transform named FPHandleCamSlot");

            cam.HandleCamera.enabled = false;

            {
                if(Resolver.TryGetModule<Animator>(out var animator))
                {
                    cam.FirstPersonBodyCamera.enabled = true;
                    cam.SetBodyCamera(animator);
                }
                
            }

            var tag = Tags.Renderer;

            foreach (var child in transform.GetChilds())
            {
                if (child.gameObject.layer == Layers.FirstPerson_Handle || child.gameObject.layer == Layers.FirstPerson_Body)
                    continue;

                if (!child.CompareTag(tag) || !child.TryGetComponent<Renderer>(out var renderer))
                    continue;

                renderer.enabled = false;
            }
        }

        [Button]
        protected virtual void SwitchThirdPersonViewport()
        {
            var cam = CameraManager.GetCamera<Camera_ThirdPerson>();
            CameraManager.SwitchCamera(cam);

            var tag = Tags.Renderer;

            foreach (var child in transform.GetChilds())
            {
                if (child.gameObject.layer == Layers.FirstPerson_Handle || child.gameObject.layer == Layers.FirstPerson_Body)
                    continue;

                if (!child.CompareTag(tag) || !child.TryGetComponent<Renderer>(out var renderer))
                    continue;

                renderer.enabled = true;
            }
        }



        #region Renderer

        [SerializeField]
        Renderer[] m_Models;

        [SerializeField]
        SkinnedMeshRenderer m_characterRenderer;

        [SerializeField]
        SkinnedMeshRenderer m_handRenderer;

        [SerializeField]
        SkinnedMeshRenderer m_bodyRenderer;

        /// <summary>
        /// split mesh   **HandRenderer  **body
        /// </summary>
        [Button]
        protected virtual void SplitMeshs()
        {
            var renderers = transform.
            GetChilds().
            Where(x => x.CompareTag(Tags.Renderer));

            if (CharacterRenderer == null)
                CharacterRenderer =
                    renderers.
                    FirstOrDefault(x => x.name.StartsWith(str_HumanoidCharacterModel))?.
                    GetComponent<SkinnedMeshRenderer>();

            Assert.IsFalse(CharacterRenderer == null);

            if (Resolver == null || !Resolver.TryGetModule(out Animator animator))
                animator = transform.GetComponentInParent<Animator>();

            if (HandRenderer == null)
                HandRenderer =
                renderers.
                FirstOrDefault(x => x.name.StartsWith(str_HumanoidHand))?.
                GetComponent<SkinnedMeshRenderer>();

            bool rebind = false;

            // hand
            {
                if (HandRenderer == null)
                {
                    HandRenderer = Instantiate(CharacterRenderer, transform);
                    //new GameObject(str_HumanoidHand).AddComponent<SkinnedMeshRenderer>();

                    HandRenderer.gameObject.name = str_HumanoidHand;
                    HandRenderer.gameObject.layer = Layers.FirstPerson_Handle;


                    var rightArm = HumanBodyBonesUtility.RightArmBones;
                    var leftArm = HumanBodyBonesUtility.LeftArmBones;

                    var bones = animator.GetHumanoidBones(rightArm.Concat(leftArm).ToArray());

                    SplitMesh(HandRenderer, bones);
                    rebind = true;
                }

                HandRenderer.gameObject.SetActive(true);
                HandRenderer.enabled = true;
                HandRenderer.updateWhenOffscreen = true;
            }


            // body
            if (false)
            {
                if (BodyRenderer == null)
                {
                    BodyRenderer = Instantiate(CharacterRenderer, transform);
                    //new GameObject(str_HumanoidHand).AddComponent<SkinnedMeshRenderer>();

                    BodyRenderer.gameObject.name = str_HumanoidBody;
                    BodyRenderer.gameObject.layer = Layers.FirstPerson_Body;

                    var bodybones = new List<HumanBodyBones>(10)
                    {
                        HumanBodyBones.Hips,
                        HumanBodyBones.Spine
                    };

                    bodybones.AddRange(HumanBodyBonesUtility.LeftLegBones);
                    bodybones.AddRange(HumanBodyBonesUtility.RightLegBones);

                    var bones = animator.GetHumanoidBones(bodybones.ToArray());

                    SplitMesh(BodyRenderer, bones);
                    rebind = true;
                }


                BodyRenderer.gameObject.SetActive(true);
                BodyRenderer.enabled = true;
                //HandRenderer.updateWhenOffscreen = true;
            }



            if (rebind && animator != null)
                animator.Rebind();
        }

        public static void SplitMesh(SkinnedMeshRenderer handRenderer, params Transform[] bones)
        {
            //var mesh = HandRenderer.sharedMesh.Copy();
            //HandRenderer.sharedMesh = mesh;

            handRenderer.SplitMesh(bones);
        }


        #endregion

        #region UserInput

        public class PerceptionModule_UserInputHandle: UserInputExtendHandle<PerceptionModule_Humanoid>
        {
            #region Binding


            protected override void SubscribeInput()
            {
                Input.Cameras.FirstPersonCamera.Subscribe(FirstPersonCamera_performed);
                Input.Cameras.ThirdPersonCamera.Subscribe(ThirdPersonCamera_performed);
            }

            protected override void UnsubscribeInput()
            {
                Input.Cameras.FirstPersonCamera.Unsubscribe(FirstPersonCamera_performed);
                Input.Cameras.ThirdPersonCamera.Unsubscribe(ThirdPersonCamera_performed);
            }

            #endregion

            
            private void FirstPersonCamera_performed(InputAction.CallbackContext ctx)
            {
                module.SwitchFirstPersonViewport();
            }

            private void ThirdPersonCamera_performed(InputAction.CallbackContext ctx)
            {
                module.SwitchThirdPersonViewport();
            }


        }

        #endregion
    }
}
