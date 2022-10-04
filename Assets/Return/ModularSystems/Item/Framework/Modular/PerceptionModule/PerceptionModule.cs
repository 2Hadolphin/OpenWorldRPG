using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Cameras;
using Return.Modular;
using UnityEngine.Assertions;
using Return.Agents;
using System;

namespace Return.Items
{
    /// <summary>
    /// Control first person view layer and collision layer.
    /// </summary>
    public class PerceptionModule : ItemModule
    {
        public override ControlMode CycleOption => ControlMode.Register;//|ControlMode.Deactivate;


        public override void Register()
        {
            // valid module user    **camera control
            Assert.IsTrue(Item.Agent.IsLocalUser());

            Activate();
        }

        protected override void Activate()
        {
            base.Activate();

            CameraManager.SubscribeCamera<FirstPersonCamera>(OnCameraStateChange);
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            SetRenderers(enabled);
            SetPartical(enabled);
        }


        protected virtual void OnCameraStateChange(CustomCamera cam, UTag type, bool enabled)
        {
            Debug.Log("OnCameraStateChanged");

            // is local user
            if (Item.isMine)
            {
                if (cam is FirstPersonCamera fpCamera)
                {
                    SetRenderers(enabled);
                    SetCamera(fpCamera, enabled);
                }
                else if (cam is Camera_ThirdPerson tpCamera)
                {
                    Debug.LogException(new NotImplementedException("Third person cam not finish."));
                }
            }
            else
                Debug.LogError("Not local user.");


            SetColliders(enabled);
            SetPartical(enabled);
        }

        protected override void OnDisable()
        {
            CameraManager.OnCameraStateChange -= OnCameraStateChange;
            var cam = CameraManager.mainCameraHandler;
            OnCameraStateChange(cam,cam.cameraType,false);
        }

        [Button]
        protected virtual void SetCamera(FirstPersonCamera fpCamera, bool enabled)
        {
            fpCamera.HandleCamera.enabled = enabled;
        }

        [Button]
        protected virtual void SetRenderers(bool enabled)
        {
            if (Item == null)
                return;

            var layer = enabled ? Layers.FirstPerson_Handle : Layers.Default;

            var go = Item?.gameObject;

            if (go)
            {
                var renderers = go.GetComponentsInChildren<Renderer>(true);

                foreach (var renderer in renderers)
                {
                    renderer.gameObject.layer = layer;
                }
            }
         
        }


        [Button]
        protected virtual void SetColliders(bool enabled)
        {
            var colliders = Item.gameObject.GetComponentsInChildren<Collider>(true);

            foreach (var col in colliders)
            {
                if (col.CompareTag(Tags.Interactable))
                    continue;

                col.isTrigger = enabled;
            }
        }

        [Button]
        protected virtual void SetPartical(bool enabled)
        {
            var layer = enabled ? Layers.FirstPerson_Handle : Layers.Default;

            var particleSystems = Item.gameObject.GetComponentsInChildren<ParticleSystem>(true);

            foreach (var particle in particleSystems)
            {
                particle.gameObject.layer = layer;
            }
        }
    }
}
