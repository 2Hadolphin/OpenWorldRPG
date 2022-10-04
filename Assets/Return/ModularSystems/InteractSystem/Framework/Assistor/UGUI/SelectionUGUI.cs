using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Return.Items;
using System;
using Cysharp.Threading.Tasks;
using Return.Cameras;
using EPOOutline;
using Return.mGUI;

namespace Return.InteractSystem
{

    /// <summary>
    /// Listen to selection module post and display gaming user interface.
    /// </summary>
    public class SelectionUGUI : BaseSelectionHandle<BaseSelectionIndicatorData>
    {
        protected override bool AddTarget(InteractWrapper wrapper, out BaseSelectionIndicatorData value)
        {
            value = null;

            if (wrapper.Interactable is not IItem item)
                return false;

            var coordinate = item.Grab();
            value = new SelectionIndicatorData() 
            {
                Context = item.Preset.Name,
                DrawPosition= coordinate 
            };

            return true;
        }

        protected override async UniTask RemoveTarget(BaseSelectionIndicatorData value)
        {
            await UniTask.NextFrame();
        }

        // change to VFX
        private void Start()
        {
            // load selection outline material
            // Resources.Load();
        }

        protected Dictionary<int, BaseSelectionIndicatorData> assistTarget=new(1);


        [Obsolete]
        protected virtual void OnGUI()
        {
            return;

            var cam = CameraManager.mainCameraHandler.mainCamera;

            float width = Screen.width;
            width = Math.Max(250, width * 0.01f);

            float height = Screen.height;
            height = Math.Max(130, height * 0.01f);


            foreach (var target in assistTarget.Values)
            {
                var pos = cam.WorldToScreenPoint(target.DrawPosition.position, Camera.MonoOrStereoscopicEye.Mono);

                var rect = new Rect(pos.x, pos.y, width, height);

                target.DrawGUI(rect);
            }
        }


        private Dictionary<IItem, bool> NearbyObjects;

        private bool isMonitor;
        private IItem monitorTarget;


        private ItemAroundList ItemListUI;

        public void ShowList()
        {
            ItemListUI.gameObject.SetActive(true);
        }

        public void CloseList()
        {
            ItemListUI.gameObject.SetActive(false);
        }

        public void UpdateUI()
        {
            if (NearbyObjects.Count == 0)
            {
                CloseList();
            }
            else
            {
                ShowList();
            }
        }

        public void AddObj(IItem item)
        {
            if (!NearbyObjects.ContainsKey(item))
                NearbyObjects.Add(item, false);
        }

        public void RemoveObj(IItem item)
        {
            if (NearbyObjects.ContainsKey(item))
            {
                /*
                if(NearbyObjects[item])    // if been Monitor
                    Agent.I.IUS.CancelTargetValid();
                */
                NearbyObjects.Remove(item);
            }
        }


        public void RemoveAllObjs()
        {
            NearbyObjects = new Dictionary<IItem, bool>();
        }



        public virtual void MonitorTargetValid(IItem item)
        {
            if (NearbyObjects.ContainsKey(item))
            {
                NearbyObjects[item] = true;
            }
        }

        public virtual void StopMonitor(IItem item)
        {
            if (NearbyObjects.ContainsKey(item))
            {
                NearbyObjects[item] = false;
            }
        }


        [Obsolete]
        public void LookAt(ItemListCage link)
        {
            var target = link.Item.GetComponent<IItem>().Grab();
            StopCoroutine(goLookTarget(target.position));
            StartCoroutine(goLookTarget(target.position));
        }

        [Obsolete]
        IEnumerator goLookTarget(Vector3 target)
        {

            for (float i = 0; i < 4f; i += Time.deltaTime)
            {
                //ReadOnlyTransform CurCam = Scripts.refs.motor.CurrentCam.transform;
                //ReadOnlyTransform CurBody = Agent.I.Motor.CharacterRoot.transform;
                //var Yrot = CurBody.InverseTransformPoint(target).width;
                //var Xrot = Agent.I.Motor.Cam.transform.InverseTransformPoint(target).height;
                //if (Mathf.Round(Xrot * 100) != 0 || Mathf.Round(Yrot * 100) != 0)
                //{
                //    Agent.I.Motor.AutoRotate(Xrot, Yrot);
                //    yield return null;
                //}
                //else
                //{
                //    yield break;
                //}

            }
            yield break;
        }



        public void ClearZoneObjects()
        {
            NearbyObjects.Clear();
            UpdateUI();
        }


        private ItemLayout UILayout;
        private HeaderGUI HDLayout;



        [Obsolete]
        public void OpenGUI(int go, Vector3? hitPoint, Collider lasthit)
        {
            if (false)
            {
                switch (go)
                {
                    case 0:
                        UILayout.gameObject.SetActive(true);
                        //HDLayout.LoadingAssets(hitPoint, lasthit);
                        UILayout.LoadingAssets();
                        break;

                    case 1:
                        UILayout.gameObject.SetActive(false);
                        break;


                    case 2:
                        UILayout.gameObject.SetActive(true);
                        //HDLayout.UpdateAssets(hitPoint.m_Value);
                        break;

                }
            }
            else
            {
                switch (go)
                {
                    case 0:
                        //HDLayout.LoadingAssets(hitPoint, lasthit);
                        //HDLayout.Canvas[0].enabled = true;
                        //print(lasthit);
                        break;

                    case 1:
                        //HDLayout.Canvas[0].enabled = false;
                        break;


                    case 2:
                        //HDLayout.UpdateAssets(hitPoint.m_Value);
                        //HDLayout.Canvas[0].enabled = true;
                        break;

                }
            }

        }

    }
}