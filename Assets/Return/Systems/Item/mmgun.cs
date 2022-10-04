using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Return.Items.Weapons
{

    public class mmgun : MonoBehaviour
    {
        //ammo ui
        

        //----------------------------------------------------------------------------------
        bool isTrigger;
        public WeaponData data;
        public ParticleSystem Effect_muzzle;
        public ParticleSystem Effect_Shell;
        public int currentAmount
        {
            get => amount;
            set
            {
                amount = value;
                //if (ui)
                //    ui.Text.text = string.Format("{0}/{1}", amount, data.Amount);
            }
        }

        public int amount;
        Transform tf;
        AmmoInstruction ui;
        FrontSight frontSight;
        public ICoordinate target;
        Transform muzzle;
        bool IsPrime;
        List<GameObject> UIs = new List<GameObject>();
        public void LoadHostHUD()
        {
            IsPrime = true;
            //foreach (var gui in data.UI)
            //{
            //    if (!gui)
            //        continue;

            //    var newUI = Instantiate(gui);
            //    UIs.Add(newUI);

            //    if (!ui)
            //        newUI.TryGetComponent(out ui);

            //    if (!frontSight)
            //        newUI.TryGetComponent(out frontSight);

            //}
        }

        private void Start()
        {
            tf = transform;

            //if (data.sl_Muzzle)
            //{
            //    muzzle = tf.Find("sl_Muzzle");
            //    Effect_muzzle = Instantiate(data.sl_Muzzle, muzzle);
            //    Effect_muzzle.transform.Zero();
            //}


            //if (data.Shell)
            //{
            //    var shell = tf.Find("sl_Eject");
            //    Effect_Shell = Instantiate(data.Shell, shell);
            //    Effect_Shell.transform.Zero();
            //}

            //if (!muzzle)
            //    muzzle = tf;
            //currentAmount = data.Amount;
        }

        Quaternion lastQuat;
        private void Update()
        {
            var p = target.position;
            var dir = p - muzzle.position;

            var quat = Quaternion.LookRotation(dir, target.up);
            lastQuat = tf.rotation;
            tf.rotation = quat;

            var ang = Quaternion.Angle(lastQuat, quat) / ConstCache.deltaTime;


            if (IsPrime)
                if (frontSight)
                {
                    if (Room)
                        frontSight.Size = 0.7f;
                    else
                        frontSight.Size = Mathf.Max(1f, ang / 20f);
                }


        }

        private void OnEnable()
        {
            foreach (var ui in UIs)
            {
                if (ui)
                    ui.SetActive(true);
            }

        }
        private void OnDisable()
        {
            isTrigger = false;
            reloading = false;
            Unsubscribe();
            foreach (var ui in UIs)
            {
                if (ui)
                    ui.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            foreach (var ui in UIs)
            {
                if (ui)
                    Destroy(ui.gameObject);
            }
        }

        public void Subscribe()
        {
            var input = InputManager.Input;
            input.Battle.MainUse.performed += Fire;
            input.Battle.MainUse.canceled += Release;
            input.Battle.Reload.performed += Reload;
            input.Battle.Assist.performed += ZoomIn;
            input.Battle.Assist.canceled += ZoomUp;
        }

        public void Unsubscribe()
        {
            var input = InputManager.Input;
            input.Battle.MainUse.performed -= Fire;
            input.Battle.MainUse.canceled -= Release;
            input.Battle.Reload.performed -= Reload;
            input.Battle.Assist.performed -= ZoomIn;
            input.Battle.Assist.canceled -= ZoomUp;
        }

        public void Fire(InputAction.CallbackContext ctx)
        {
            //switch (data.Type)
            //{
            //    case ValueType.Slot:
            //        Shoot();
            //        break;
            //    case ValueType.Auto:
            //        StartCoroutine(AutoShoot());
            //        break;
            //}

        }
        public void Release(InputAction.CallbackContext ctx)
        {
            isTrigger = false;
        }

        bool Room;
        void ZoomIn(InputAction.CallbackContext ctx)
        {
            //var cam = ConstCache.PrimeCamDirector;

            //cam.Zoom(cam.Fov / data.Scope, 0.35f, 4);
            //Room = true;
        }
        void ZoomUp(InputAction.CallbackContext ctx)
        {
            // move cam 
            //var cam = ConstCache.PrimeCamDirector;
            //cam.Zoom(-1, 0.35f, 3);
            Room = false;
        }
        bool reloading;
        IEnumerator AutoShoot()
        {
            if (isTrigger)
                yield break;

            isTrigger = true;
            if (currentAmount <= 0)
                yield return StartCoroutine(Reloading());



            //var wait = new WaitForSeconds(data.FireRate);
            //while (isTrigger)
            //{
            //    Shoot();
            //    yield return wait;
            //}

            yield break;
        }

        void Reload(InputAction.CallbackContext ctx)
        {
            StartCoroutine(Reloading());
        }

        IEnumerator Reloading()
        {
            if (reloading)
                yield break;

            reloading = true;
            yield return null;
            //var clip = data.Effect_Reload;
            //var wait = ConstCache.WaitForSeconds(clip.length);
            //mAudioManager.PlayOnceAt(clip, tf.position);

            //yield return wait;

            //currentAmount = data.Amount;
            reloading = false;
        }
        public byte Team;
        void Shoot()
        {
            if (currentAmount <= 0)
                return;
            else
                currentAmount--;

            //mAudioManager.PlayOnceAt(data.Effect_Muzzle, tf.position, true);
            //AgentBlackboard.m_Instance.PostCombatNoise(tf.position, Team);

            if (Effect_muzzle)
                Effect_muzzle.Play(true);

            if (Effect_Shell)
                Effect_Shell.Emit(1);

            var value = UnityEngine.Random.value;
            //if (value > 0.8f)
            //    mAudioManager.PlayOnceAt(DemoPreset.Instance.NormalHit.Random(), target.position, true);
            //else if (value < 0.2f)
            //    mAudioManager.PlayOnceAt(DemoPreset.Instance.TargetFlyby.Random(), target.position, true);


            //if (Physics.OnSensorUpdate(muzzle.position, muzzle.forward, out var hit, data.DamageMask))
            //{
            //    hit.collider.SendMessageUpwards(nameof(ICombat.ReciveDamage), data, SendMessageOptions.DontRequireReceiver);
            //    AgentBlackboard.Instance.PostCombatNoise(hit.point, Team);
            //    if (hit.collider.CompareTag(TagRefer.Vehicle))
            //        mAudioManager.PlayOnceAt(DemoPreset.Instance.MetalHit.Random(), target.GUIDs, true);

            //}
        }

    }

}