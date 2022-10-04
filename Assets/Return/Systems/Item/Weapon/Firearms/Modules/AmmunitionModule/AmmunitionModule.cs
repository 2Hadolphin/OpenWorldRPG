using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using TNet;
using System;
using UnityEngine.Playables;
using Return.Items.Weapons.Firearms;
using Return;
using Return.Agents;

namespace Return.Items.Weapons
{
    //[RequireComponent(typeof(FireControlModule))]
    public partial class AmmunitionModule : FirearmsModule<AmmunitionPreset>, IFirearmsPersistentPerformerHandle, IEjectParticleProvider
    {
        #region Setup

        public override void LoadModuleData(ConfigurableItemModulePreset preset)
        {
            base.LoadModuleData(preset);
        }

        #endregion


        [Inject]
        protected ITNO tno;

        #region Routine

        protected override void Activate()
        {
            base.Activate();

            if (Item.resolver.TryGetModule(out performer))
            {
                performer.OnMarkerPost -= Performer_OnMarkerPost;
                performer.OnMarkerPost += Performer_OnMarkerPost;
            }

            var tf = transform;

            if (Magazine.IsNull())
                Magazine = tf.GetChildWithTag(Tags.Config).FirstOrDefault(x => x.name == FirearmsConfig.str_Magazine).GetComponentInChildren<Magazine>();


            if (Item.Agent.IsLocalUser())
            {
                //this.ValidMonoHandle<InputHandle>(item).
                //    BindLocalUser().
                //    Instance(gameObject).
                //    SetHandler().
                //    Register().
                //    DebugLog();

                if (Item.Agent.IsLocalUser())
                {
                    var input = new InputHandle();

                    input.RegisterInput();
                    Item.resolver.RegisterModule(input);
                    input.SetHandler(this);
                    input.enabled = true;
                }

                this.ValidHandle<UGUIHandle>(Item).
                     BindLocalUser().
                     Instance().
                     SetModule().
                     Register().
                     DebugLog();

                if (Chamber.Count == 0)
                    CheckState();
            }
        }


        protected override void Deactivate()
        {
            base.Deactivate();

            // cancel invertory habbit 
            // disconnect battle system

        }

        #endregion

        /// <summary>
        /// Separate ammo compartment **high explosive**incendiary**rubber**armor piercing**flare
        /// </summary>
        public Magazine[] Magazines;

        /// <summary>
        /// Bullet container.
        /// </summary>
        [SerializeField]
        protected Magazine Magazine;


        /// <summary>
        /// Loaded bullets.
        /// </summary>
        [NonSerialized]
        [ShowInInspector]
        protected Queue<AmmunitionData> Chamber = new(1);

        /// <summary>
        /// Ejected bullets.
        /// </summary>
        [NonSerialized]
        protected Queue<AmmunitionData> Ejects = new(1);
        //bool boltActionOpen = false;

        [NonSerialized,ShowInInspector]
        bool duringLoading;

        /// <summary>
        /// Process ammunition supply.
        /// </summary>
        protected Magazine Supplement()
        {
            var newMag= Magazines.FirstOrDefault(x => x.Amount > 0);

            if (newMag.IsNull())
                newMag = Magazine;

            return newMag; 
        }

        /// <summary>
        /// Extract ammo to chamber of firearms. (None animation**auto-fill)
        /// </summary>
        public virtual void Loaded()
        {
            //Eject();

            Debug.Log("Loaded bullet.");

            // check mag
            if (Magazine.IsNull())
                Magazine = Supplement();

            if (Magazine)
            {
                if (pref.HasChamber && pref.ChamberCapacity <= Chamber.Count)
                {
                    Debug.LogError("Out of chamber capacity");
                    return;
                }

                var ammo = Magazine.Withdraw();

                if (ammo.NotNull())
                    Chamber.Enqueue(ammo);
                //else
                //    Chamber.Enqueue(ammo);
            }
        }


        /// <summary>
        /// CheckAdd chamber has bullet or not.
        /// </summary>
        /// <param name="ammunition">ammo data to run ballistic.</param>
        /// <returns>Return true if firing a bullet</returns>
        public virtual bool FiringPin(out AmmunitionData ammunition)
        {
            Debug.Log(Chamber.Count);

            var checkChamber= Chamber.TryDequeue(out ammunition);

            // has ammo
            if (checkChamber)
                Ejects.Enqueue(ammunition);
            else // go loaded or charging bolt 
            {
                CheckState();
            }

            return checkChamber;
        }


        public virtual void CheckState()
        {
            if (duringLoading)
                return;

            if (Magazine.NotNull() && Magazine.Amount > 0)
            {
                ChargingBolt();
                //if (boltActionOpen)
                //Loaded();
                //else
                //    ChargingBolt();
            }
            else
                Reload();
        }

        public virtual void Reload()
        {
            if (!performer.CanPlay())
                return;

            // set ui ammo zero

            // 
            // require inventory item
            // 

            //Magazine.Content.AddRange(new AmmunitionData[30]);

            PlayReload();
        }

        [RFC]
        protected virtual void PlayReload()
        {
            if (Item.isMine)
                tno?.Send(nameof(PlayReload), Target.Others);

            // sound and animation

            performer.PlayState(pref.Reload);
        }

        /// <summary>
        /// Manually load bullet to chamber Anim&Audio
        /// </summary>
        public virtual void ChargingBolt()
        {
            //boltActionOpen = true;
            // _checkCache state can use 

            //if (pref.LoadedToken)
            //    if (!Firearms.CanSetStatus(pref.LoadedToken))
            //        return;

            // start anim and wait for Loaded event

            Debug.LogError(nameof(ChargingBolt));

            if (pref.Loaded)
            {
                if (performer.CanPlay())
                    performer.PlayState(pref.Loaded);
                else
                    performer.QueuePerformer(pref.Loaded);

                // wait event
                duringLoading = true;
            }
            else
            {
                Debug.LogError("missing loaded anim");
                Loaded();
            }

        }

        #region Performer Handle

        IFireamrsPerformerHandler performer;
        
        public TimelinePreset[] LoadPerformers()
        {
            #region Load Playables

            return new[]
            {
                pref.Reload,
                pref.Loaded,
            };

            #endregion
        }

        public void Cancel(TimelinePreset preset)
        {
            if (pref.Loaded == preset)
            {
                if (pref.LoadedToken.NotNull())
                    Firearms.RemoveStatus(pref.LoadedToken);

                duringLoading = false;
            }
        }

        public void Finish(TimelinePreset preset)
        {
            switch (preset)
            {
                case var _ when preset == pref.Loaded:
                    // idle 
                    duringLoading = false;
                    break;
            }
        }

        private void Performer_OnMarkerPost(UnityEngine.Timeline.Marker marker)
        {
            if (marker is DefinitionMarker umarker)
            {
                if (umarker.EventID.Equals(pref.Events.OnEject))
                    Eject();
                else if (umarker.EventID.Equals(pref.Events.OnLoaded))
                    Loaded();
                //else if (umarker.EventID.Equals(pref.Events.OnLoaded))

            }
        }

        #endregion


        #region Eject

        protected virtual void Eject()
        {
            // particle
            if (Ejects.TryDequeue(out var ammunition))
                OnEjectPlay?.Invoke();
        }

        #region Partical

        public event Action OnEjectPlay;

        #endregion
        #endregion

 
    }
}