using UnityEngine;
using Return.Agents;
using Sirenix.OdinInspector;
using System;
using Cysharp.Threading.Tasks;

namespace Return.Items.Weapons
{

    public partial class FireControlModule  // HostHandle
    {
        /// <summary>
        /// User control module
        /// </summary>
        public class HostHandler : ItemModuleHandle<FireControlModule>
        {
            protected override void ActivateHandle(IItem item, FireControlModule module)
            {
                base.ActivateHandle(item, module);

                if (item.Agent.IsLocalUser())
                {
                    if (Input == null)
                    {
                        Input = new InputHandle()
                        {
                            Control = this,
                        };

                        Input.RegisterInput(InputManager.Input);
                        item.resolver.RegisterModule(Input);
                        Input.SetHandler(module);
                        Input.enabled = true;
                    }
                 
                }

                item.resolver.TryGetModule(out trajectProvider);
            }

            #region Routine

            public override void Register()
            {
                base.Register();


            }

            public override void UnRegister()
            {
                base.UnRegister();

                Input.Dispose();
            }

            public override void Activate()
            {
                base.Activate();

                if (Input)
                    Input.enabled = true;
            }

            public override void Deactivate()
            {
                base.Deactivate();

                if (Input)
                    Input.enabled = false;
            }

            #endregion

            AmmunitionModule AmmoModule => Module.AmmoModule;

            #region Input

            protected InputHandle Input;

            #endregion

            #region Ballistic

            IMarkerProvider trajectProvider;

            #endregion

            #region Gun State

            /// <summary>
            /// Is trigger pulled
            /// </summary>
            public bool isTrigger;

            /// <summary>
            /// Is gun in stroke
            /// </summary>
            public bool Interval = false;

            #endregion


            #region Module Control Behaviour

            #region Pull Trigger

            /// <summary>
            /// Pull Trigger
            /// </summary>
            [Button]
            public virtual void Trigger()
            {
                //if (!MonoModule.performer.CanPlay())
                //    return;

                //isTrigger = true;

                // Logic
                Firing();

                // ask scope module for ballistic rows
                // enable attack
            }

            /// <summary>
            /// Fire the bullet
            /// </summary>
            protected virtual void Firing()
            {
                if (Interval)
                    return;

                if (Module.m_FireMode.FireMode.Equals(FireMode.Safty))
                    return;

                // _checkCache chamber
                if (AmmoModule.FiringPin(out var bulletData))
                {
                    // performer
                    Module.PlayFiringPerformerHandle();

                    // combine with performed?
                    Module.StartRecoil();

                    //prepare bulletData and pass to battle manager **trajectory **silence **damage 
                    // analyze trajectory
                    AnalyzeTrajectory(bulletData);
                }
                else
                {
                    Debug.LogError("No ammo, play empty trigger");
                    // play empty firing pin audio
                    Module.PlayEmptyTrigger();


                    //AmmoModule.Reload();
                    return;
                }


                BeginInterval();
            }

            /// <summary>
            /// Start bolt movement
            /// </summary>
            protected virtual async void BeginInterval()
            {
                if (Interval)
                    return;

                await UniTask.NextFrame();

                Interval = true;

                while (isTrigger || Interval)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(Module.m_FireMode.Rof));

                    // wait performer end
                    //await UniTask.NextFrame();

                    EndInterval();

                    //await UniTask.NextFrame();

                    // contiune burst fire
                    if (enabled && isTrigger)
                        Firing();
                    else
                        Interval = false;
                }

                return;
            }

            /// <summary>
            /// Load chamber.
            /// </summary>
            void EndInterval()
            {
                switch (Module.m_FireMode.FireMode)
                {
                    case FireMode.Manual:
                        isTrigger = false;
                        AmmoModule.ChargingBolt();

                        break;

                    case FireMode.SemiAutomatic:
                        isTrigger = false;
                        AmmoModule.Loaded();

                        break;

                    case FireMode.FullyAutomatic:
                        AmmoModule.Loaded();
                        break;
                }
            }

      
            //IEnumerator CaculateInterval()
            //{
            //    yield return null;

            //    var wait = ConstCache.WaitForSeconds(m_FireMode.Rof);
            //    Interval = true;

            //    while (isTrigger || Interval)
            //    {
            //        yield return wait;

            //        EndInterval();

            //        yield return null;

            //        if (enabled && isTrigger)
            //            Firing();
            //        else
            //            Interval = false;
            //    }
            //    yield break;
            //}

            /// <summary>
            /// Shoot ??
            /// </summary>

#if UNITY_EDITOR
            [Button]
            public void DebugShoot()
            {
                if (Interval)
                {
                    Debug.Log(Interval);
                    return;
                }

                Module.StartRecoil();

                AnalyzeTrajectory(null);

                Module.PlayFiringPerformerHandle();

                BeginInterval();
            }
#endif

            #endregion



            #endregion

            #region Apply Trajectory

            protected virtual void AnalyzeTrajectory(AmmunitionData ammo)
            {
                Ray ray;

                if (trajectProvider.IsNull())
                    ray = new(Module.transform.position, Module.transform.forward);
                else
                    ray = trajectProvider.GetRay();

                var mission =
                    FirearmsBallisticTrajectoryMission.Create(ray.origin, ray.direction);

                mission.MaxRaycastNumber = 5;
                mission.Speed = 700;
                mission.MaxDistance = 1131;
                mission.RaycastMask = Layers.AllLayers;

                RaycastSystem.AddMission(mission);

                //RaycastSystem.StartJobQueue(
                //    new FirearmsTrajectoryBatchMission(ray) // muzzle pos
                //    {
                //        MaxRaycastNumber=5,
                //        Speed = 700,
                //        Drag = 0.001f,
                //        MaxDistance = 1400,
                //        RaycastMask = Physics.AllLayers//Layers.Default,
                //    });
            }

            #endregion
        }
    }


}