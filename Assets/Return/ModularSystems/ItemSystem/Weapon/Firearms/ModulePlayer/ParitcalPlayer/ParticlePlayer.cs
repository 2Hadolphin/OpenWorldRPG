using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using static UnityEngine.PlayerLoop.PreLateUpdate;
using Return.Cameras;
using Return.Agents;

namespace Return.Items.Weapons.Firearms
{
    [DefaultExecutionOrder(11000)]
    public class ParticlePlayer : FirearmsModule<ParticlePlayerPreset>
    {
        protected Transform sl_Muzzle;
        [SerializeField]
        protected ParticleSystem ps_Muzzle;

        protected Transform sl_Eject;

        [SerializeField]
        protected ParticleSystem ps_Ejecte;



        //public override ControlMode InitMode => ControlMode.RegisterHandler;

        protected override void Register()
        {
            base.Register();

            var targets = Item.gameObject.GetAllTags(Tags.Config);

            foreach (var go in targets)
            {
                if (sl_Muzzle.IsNull() && go.name == FirearmsConfig.str_Muzzle)
                    sl_Muzzle = go.transform;
                else if (sl_Eject.IsNull() && go.name == FirearmsConfig.str_Eject)
                    sl_Eject = go.transform;
            }


            if (pref.Muzzle.NotNull() && sl_Muzzle.NotNull())
                ps_Muzzle = Instantiate(pref.Muzzle).ParentOffset(sl_Muzzle);


            if (pref.Eject.NotNull() && sl_Eject.NotNull())
            {
                ps_Ejecte = Instantiate(pref.Eject).ParentOffset(sl_Eject);
            }

            if (Item.Agent.IsLocalUser())
            {
                CameraManager.OnCamerasChange += CameraManager_OnCamerasChange;
                var cam = CameraManager.GetCamera<FirstPersonCamera>();
                CameraManager_OnCamerasChange(cam.cameraType, cam.isActiveAndEnabled);
            }
        }

        private void CameraManager_OnCamerasChange(UTag type, bool active)
        {
            if (type.HasFlag(CameraManager.FirstPersonCamera))
            {
                SetFirstPersonView(ps_Ejecte,active);
                SetFirstPersonView(ps_Muzzle, active);
            }
        }

        void SetFirstPersonView(ParticleSystem particle,bool active)
        {
            var layer = active ? Layers.FirstPerson_Handle : Layers.Partical;
            particle.gameObject.layer = layer;
        }

        protected override void Activate()
        {
            base.Activate();

            if (ps_Muzzle.NotNull())
                if (Item.resolver.TryGetModule<IMuzzleParticleProvider>(out var muzzle))
                    muzzle.OnMuzzlePlay += PlayMuzzle;

            if (ps_Ejecte.NotNull())
            {
                if (Item.resolver.TryGetModule<IEjectParticleProvider>(out var eject))
                    eject.OnEjectPlay += PlayEject;

                ps_Ejecte.Pause(true);
            }
        }


        [Button]
        public virtual void PlayMuzzle()
        {
            if (ps_Muzzle.IsNull())
                return;

            ps_Muzzle.Play(true);
        }

        [Button]
        public virtual async void PlayEject()
        {
            if (ps_Ejecte.IsNull())
                return;

            await UniTask.WaitForEndOfFrame(this);

            //ps_Ejecte.transform.Copy(sl_Eject);

            //ps_Ejecte.transform.Copy(sl_Eject);
            ps_Ejecte.Emit(1);
            //ps_Ejecte.Pause(true);
        }

        //ParticleSystem.EmitParams EmitParams;
        //ParticleSystem.Particle[] eject_paras;

        //[SerializeField]
        //bool adjust = true;

        //private void LateUpdate()
        //{
        //    if (adjust)
        //    {
        //        ps_Ejecte.Simulate(ConstCache.deltaTime, true);

        //    }
        //}


        private void LateUpdate()
        {

            //ps_Ejecte.transform.Copy(sl_Eject);

            //return;

            if (!ps_Ejecte.IsAlive(true))
                return;


            //ps_Ejecte.Simulate(ConstCache.deltaTime, true, false, false);
            //Debug.LogError("Simulate eject");
        }

        public bool defaultLoop = true;

        // Start is called before the first frame update
        void SetParticleOrder()
        {
            if (defaultLoop)
            {
                PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
                return;
            }

            var loop = PlayerLoop.GetCurrentPlayerLoop();

            PlayerLoopSystem? particleUpdate = null;

            // Find the particle system update
            for (int i = 0; i < loop.subSystemList.Length; ++i)
            {
                if (loop.subSystemList[i].type == typeof(PreLateUpdate))
                {
                    var preLateUpdate = loop.subSystemList[i];
                    for (int j = 0; j < preLateUpdate.subSystemList.Length; ++j)
                    {
                        if (preLateUpdate.subSystemList[j].type == typeof(ParticleSystemBeginUpdateAll))
                        {
                            // Remove particle system update
                            particleUpdate = preLateUpdate.subSystemList[j];
                            var list = preLateUpdate.subSystemList.ToList();
                            list.RemoveAt(j);
                            preLateUpdate.subSystemList = list.ToArray();
                        }
                    }
                }
            }

            if (!particleUpdate.HasValue)
                return;

            // Move it so it is updated after LateUpdate
            for (int i = 0; i < loop.subSystemList.Length; ++i)
            {
                if (loop.subSystemList[i].type == typeof(PostLateUpdate))
                {
                    var system = loop.subSystemList[i];
                    var list = system.subSystemList.ToList();
                    list.Add(particleUpdate.Value);
                    system.subSystemList = list.ToArray();
                }
            }

            PlayerLoop.SetPlayerLoop(loop);
        }
    }

}