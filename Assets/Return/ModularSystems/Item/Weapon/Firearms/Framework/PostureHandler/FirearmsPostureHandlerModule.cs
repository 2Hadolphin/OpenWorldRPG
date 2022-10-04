using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Return.Animations;
using UnityEngine.Animations;
using Unity.Collections;
using System;
using UnityEngine.Assertions;
using Return.Items;
using Sirenix.OdinInspector;
using UnityEngine.Timeline;
using System.Linq;
using VContainer;
using Cysharp.Threading.Tasks;
using Return.Modular;
using Return.Motions;

namespace Return.Items.Weapons.Firearms
{
    /// <summary>
    /// 
    /// </summary>
    //[DefaultExecutionOrder(5000)]
    public class FirearmsPostureHandlerModule : ItemPostureHandlerModule<FirearmsPostureHandlerPreset>,IFirearmsPerformerHandle, IModularHandle
    {
        protected override string ItemName { get => pref.HierarchyName; set => pref.HierarchyName = value; }

        //[Inject]
        //void TestInject(Firearms firearms)
        //{
        //    //Debug.Log("Inject " + firearms);
        //    Debug.LogErrorFormat("Inject : {0} {1}.", nameof(Firearms), Firearms == null ? "Failure" : "Successed");
        //}

        #region Setup




        protected override void Activate()
        {
            base.Activate();

            if (Item.Agent != null && Item.Agent.Resolver.TryGetModule(out IMotionSystem msys))
            {
                // bind motion sequence
                MotionHandle = pref.MotionModuleItemHandler.Create(gameObject);
                //MotionHandle.SetHandler(msys);

                Item.resolver.RegisterModule(MotionHandle);
            }
            else
            {
                Debug.LogError(Item.Agent + "=> None motion system found.");
            }

            performer.AddState(pref.Idle);
            performer.SetIdleState(pref.Idle);
        }

       

        #endregion

        private void LateUpdate()
        {
            if (FixedItemPos)
                SetHandleSlotPos();
        }

        #region Behaviour

        /// <summary>
        /// invoke by IUS
        /// </summary>
        public override async UniTask Equip()
        {
            Assert.IsNotNull(performer);

            SubscribeMarker(true);

            FixedItemPos = true;

            {
                var preset = pref.Equip;

                try
                {
                    // dynamic
                    performer.PlayState(preset, this);
                }
                finally
                {
                    var speed = 1f;
                    var time = 0f;

                    if (preset.GetAsset().TryGetUTagMarker(pref.EquipID, out var equip))
                    {
                        // hide item until timeline awake
                        SetRenderer(false);

                        await UniTask.Delay(TimeSpan.FromSeconds(equip.time * speed));
                        time += (float)equip.time;

                        // hide item until timeline awake
                        SetRenderer(true);
                    }
                    else
                        Debug.LogError("Missing equip marker.");

                    if (preset.GetAsset().TryGetUTagMarker((x) => x.GetTag() == nameof(AbstractItem.Activate), out var activate))
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds((activate.time - time) * speed));
                    }
                    else
                        Debug.LogError("Missing Activate marker.");
                }
            }
        }


        public override async UniTask Dequip()
        {
            var dequip = pref.Unequip;

            try
            {
                FixedItemPos = true;

                //Time.timeScale = 0.05f;

                // dynamic
                performer.PlayState(dequip, this);

                var asset = dequip.GetAsset();

                // get speedup
                var speed = 1f;

                // get performer duration
                var time = asset.duration*speed;

                // get deactivate marker time or timeline duration
                if (asset.TryGetUTagMarker((x) => x.GetTag() == nameof(Dequip), out var marker))
                {
                    var markerTime = marker.time * speed - 0.05f;

                    Debug.LogError($"Timeline dequio tag at {markerTime}/{time}.");

                    time -= markerTime;

                    await UniTask.Delay(TimeSpan.FromSeconds(markerTime));
                }
                else
                {
                    Debug.LogError($"Missing timeline tag {nameof(Dequip)}.");
                    await UniTask.Delay(TimeSpan.FromSeconds(time));
                    time = 0;
                }

                // move item to hand slot
                {
                    var local = transform.GetLocalPR();
                    transform.parent = ArmHandleSlot;
                    transform.SetLocalPR(local);
                    Debug.Log($"{transform.parent} {transform.localPosition==local}");
                    //gameObject.SetActive(false);
                }

                // true??
                FixedItemPos = false;

                // wait remaining time
                if (time>0)
                    await UniTask.Delay(TimeSpan.FromSeconds(time));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
            }
            finally
            {

                Debug.Log("posture end dequip" + transform.position);

                //SubscribeMarker();
                SetRenderer(false);

                // performer module need manually shutdown until holster.
                if (performer is IItemModule module)
                    module.Deactivate();


                Debug.Log($"{Time.frameCount} dequip end.");
                Debug.Log("posture "+transform.position);

                Deactivate();

                //Time.timeScale = 1;
            }

            Debug.Log($"{Time.frameCount} dequip finish.");
            await UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);
            Debug.Log($"{Time.frameCount} dequip finish.");
        }

        public override async UniTask Inspect()
        {
            Debug.LogError("Inspect item");
        }

        #endregion


        #region Hierarchy

        [SerializeField]
        Transform m_ItemHandleSlot;
        public Transform ItemHandleSlot { get => m_ItemHandleSlot; set => m_ItemHandleSlot = value; }

        [SerializeField]
        Transform m_ArmHandleSlot;

        /// <summary>
        /// Transform of hand slot(purlicue). 
        /// </summary>
        public Transform ArmHandleSlot { get => m_ArmHandleSlot; set => m_ArmHandleSlot = value; }

        protected override void SetPostureHierarchy(IItem item)
        {
            base.SetPostureHierarchy(item);

            if (item.Agent.transform.FindChild(HandleOption.HandleSlot_RightHand, out m_ArmHandleSlot))
                m_ItemHandleSlot = transform.parent; // item.transform.parent;
        }


        [SerializeField]
        bool FixedItemPos = false;

        /// <summary>
        /// Use this function to align item handle slot(anim) to hand slot(interactor).
        /// </summary>
        [Button]
        protected virtual void SetHandleSlotPos()
        {
            if (m_ArmHandleSlot && m_ItemHandleSlot)
            {
                //Debug.Log($"Set Pos time {Time.frameCount}");

                m_ItemHandleSlot.Copy(m_ArmHandleSlot);
                pos = m_ItemHandleSlot.position;
                //Debug.Log(pos);
            }
            //else
            //    Debug.LogError(m_ArmHandleSlot.IsNull() ? nameof(m_ArmHandleSlot) : m_ItemHandleSlot.IsNull());
        }

        Vector3 pos;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pos, 0.05f);
        }

        #endregion



        /// <summary>
        /// Show item
        /// </summary>
        void SetRenderer(bool enable)
        {
            var rens = GetComponentsInChildren<Renderer>(true);

            foreach (var ren in rens)
                ren.enabled = enable;
        }




        #region LayerMixer
        ///// <summary>
        ///// ?? layer mixer move to motion system
        ///// </summary>
        //protected AnimationScriptPlayable LayerMixer;
        //protected AnimationMixerJob MixerJob;

        //protected virtual void CreateMixer(PlayableGraph graph)
        //{
        //    MixerJob = new AnimationMixerJob()
        //    {

        //    };
        //}
        #endregion


        #region Posture Adjust
        //[Obsolete]
        //protected NativeArray<TransformStreamHandle> AdditiveHandles; 
        //[Obsolete]
        //protected NativeArray<Quaternion> AdditiveRotations; 
        //[Obsolete]
        //protected NativeArray<float> AdjustWeights;

        //protected virtual void Init_AdjustPosture(IHumanoidAnimator anim)
        //{
        //    var itemTransforms = transform.GetChilds();


        //    var postureRootBone = anim.GetAnimator.GetCloestRootBone(HumanBodyBones.Hips,HumanBodyBones.Neck);

        //    var enableTransforms = postureRootBone.GetChilds().RemoveDuplicates(itemTransforms);

        //    //var handles=anim.GetAnimator.GetHandles();
        //}

        #endregion


        #region Performer


        //public override event Action<TimelinePerformerHandle> OnPerformerPlay;


        public TimelinePreset[] LoadPerformers()
        {
            return null;//new[] { pref.Equip, pref.Unequip,pref.IdleMotion  };
        }

        public override void Cancel(TimelinePreset preset)
        {
            Assert.IsNotNull(preset);

            //if(!TryGetComponent<m_items>(out var item))
            //    throw new KeyNotFoundException("Failure to get handler of item.");

            //switch (preset)
            //{
            //    case var value when value == pref.Equip:
            //        item.SetHandler();
            //        break;

            //    case var value when value == pref.Unequip:
            //        item.Deactivate();
            //        break;
            //}
            Debug.LogError($"Unknow break Performer {performer}");
            DisposePerformer(preset);
        }

        public override void Finish(TimelinePreset preset)
        {
            //DisposePlayingPerformer(preset);
        }

        void DisposePerformer(TimelinePreset preset)
        {
            //Debug.Log("DisposePlayingPerformer : " + preset);
            //performer.RemoveState(preset);
            if(this&&transform)
                Debug.Log("Dispose performer callback : "+transform.position);
        }

        /// <summary>
        /// Default unscribe performed equip.
        /// </summary>
        /// <param name="enable"></param>
        void SubscribeMarker(bool enable = false)
        {
            performer.OnMarkerPost -= OnMarkerPost;

            if (enable)
                performer.OnMarkerPost += OnMarkerPost;
        }

        /// <summary>
        ///  SetHandler(-- Equip--)  Deactivate(--Unequip--)
        /// </summary>
        protected virtual void OnMarkerPost(Marker obj)
        {
            if (obj is not DefinitionMarker marker)
                return;

            //Debug.Log("On performed trigger : "+equip+transform.parent);

            if (marker.EventID.Equals(pref.EquipID))
            {
                //SetRenderer(true);
            }
            else if (marker.EventID.Equals(pref.UnequipID))
            {
                Debug.Log($"{Time.frameCount} Unequip Event");
            }
            else
            {
                if (!TryGetComponent<AbstractItem>(out var item))
                    throw new KeyNotFoundException("Failure to get handler of item.");

                var id = marker.EventID.GetTag();

                switch (id)
                {
                    case nameof(item.Activate):
                        //SubscribeMarker();
                        //item.SetHandler();
                        FixedItemPos = false;
                        break;

                    case nameof(item.Deactivate):
                        //item.Deactivate();
                        break;

                    default:
                        //Debug.LogError("Missing maker target - "+obj+" : "+id);
                        return;
                }
            }
        }


        #endregion

        #region Motion Handle
        protected IHumanoidMotionModule MotionHandle;

        #endregion
    }
}