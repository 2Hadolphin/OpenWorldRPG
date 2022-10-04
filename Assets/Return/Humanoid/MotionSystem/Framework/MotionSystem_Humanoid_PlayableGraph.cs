using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Return.Humanoid.Motion;
using UnityEngine.Animations.Rigging;
using Return.Creature;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Return.Humanoid.IK;
using Return.Animations;
using Unity.Collections;
using Cysharp.Threading.Tasks;
using HoaxGames;
using Return.Humanoid;
using Return.Humanoid.Character;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;

namespace Return.Motions
{

    public partial class MotionSystem_Humanoid // **Animation Graph   **IK
    {
        public event Action<AnimationEvent> OnAnimEvent;

        public virtual void MotionEvent(AnimationEvent e)
        {
            OnAnimEvent?.Invoke(e);
        }


        public override void Rebind()
        {
            var valid = RigBuilder != null && RigBuilder.layers.Count > 0;
            var enable = RigBuilder.enabled;

            if(RigBuilder)
            {
                Animator.UnbindAllStreamHandles();
            }

            if (valid)
                RigBuilder.Clear();

            if (valid)
                RigBuilder.Build();

            //if (enable)
            //    RigBuilder.enabled = false;

            base.Rebind();

            //if (enable)
            //    RigBuilder.enabled = true;
        }

#if UNITY_EDITOR
        Color GraphValidGUI => MotionGraph.IsValid() ? Color.green:Color.red;
        [GUIColor(nameof(GraphValidGUI))]
#endif
        protected PlayableGraph MotionGraph;

        public override PlayableGraph GetGraph
        {
            get
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                    return  InstanceIfNull<Return.Editors.EditorAnimPlayer>().GetGraph;
#endif
                return MotionGraph;
            }
        }

        /// <summary>
        /// RootTransform motion node. MotionStream<-  IK<-  LayerMixer<-    MxM/Item Performer. 
        /// </summary>
        public Playable MotionStream { get; protected set; }

        protected PlayableOutput Output;

        public PlayableOutput GetOutput
        {
            get
            {
                if (Output.IsOutputNull() || !Output.IsOutputValid())
                    Output = AnimationPlayableOutput.Create(GetGraph, "MotionSystemOutput", Animator);

                return Output;
            }
        }

        //protected AnimationScriptPlayable AnimLayerMixer;
        public AnimationLayerMixerPlayable AnimMixer { get; protected set; }

        //protected Dictionary<Playable, AnimationScriptPlayable> Mixers;
        //protected Dictionary<Playable, NativeArray<TransformStreamHandle>> MixerHandles;
        //protected Dictionary<Playable, NativeArray<float>> MixerWeights;

        /// <summary>
        /// Inputs playable and its index of mixer.
        /// </summary>
        protected Dictionary<Playable, int> MixerCache=new();


        #region Routine

        protected virtual void Register_PlayableGraph()
        {
            //Agent.RegisterSystem(Animator);

            MotionGraph = PlayableGraph.Create("MotionSystem");
            MotionGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            MotionStream = Playable.Create(GetGraph);
            MotionStream.SetTraversalMode(PlayableTraversalMode.Passthrough);
   

            // create animation mixer
            Init_SetAnimationMixer();

            // Create job schedule
            Sequence = LimbSequence.GetDictionary;

            //use playable IK or rigging

            switch (m_IKType)
            {
                case IKType.None:

                    break;
                case IKType.Playable:
                    CreatePlayableIK();
                    break;
                case IKType.Rigging:
                    BuildRiggingIK();
                    break;
                case IKType.LateUpdate:

                    break;
            }

            SetAnimSource();
        }

        /// <summary>
        /// Create animation layaer mixer and inject mxm stream.
        /// </summary>
        protected virtual void Init_SetAnimationMixer()
        {
            if (AnimMixer.IsQualify())
                return;

            AnimMixer = AnimationLayerMixerPlayable.Create(GetGraph, 1);
            //AnimMixer.AddInput(MxMPlayable,0,1);
            MotionStream.AddInput(AnimMixer,0,1);
        }

        protected virtual void Unregister_Grpah()
        {
            if (MotionGraph.IsValid())
                MotionGraph.Destroy();
        }

        #endregion

        public void AddAnimationLayer(Playable animPlayable,AvatarMask mask,float blendTime=0.2f)
        {
            Init_SetAnimationMixer();

            if(MixerCache.TryGetValue(animPlayable,out var index))
            {
                Debug.Log("Overwrite mask : "+animPlayable.GetPlayableType());

            }
            else
            {
                var weight = blendTime > 0 ? 0 : 1;
                index = AnimMixer.AddInput(animPlayable, 0, weight);
                MixerCache.Add(animPlayable, index);

                if (weight == 0)
                    BlendLayerWeight(animPlayable, 0, 1, blendTime);
            }

            AnimMixer.SetLayerMaskFromAvatarMask((uint)index, mask);
            
            //var m_duringTransit = AnimationMixerJob.Create();
            //m_duringTransit.handles = handles;

            //AnimLayerMixer =AnimationScriptPlayable.Create(GetGraph,)
        }

        async UniTask BlendLayerWeight(Playable playable,float from,float to,float blendTime)
        {
            var time = 0f;

            while (blendTime > time && playable.IsQualify())
            {   
                time += ConstCache.deltaTime;
                var ratio = time / blendTime;

                var weight = ratio.Lerp(from, to);
                AnimMixer.SetInputWeight(playable,weight);

                await UniTask.NextFrame();
            }
        }


        public async void RemoveAnimationLayer(Playable playable,float blendTime=0.2f)
        {
            if (MixerCache.TryGetValue(playable, out var index))
            {
                MixerCache.Remove(playable);

                if (blendTime>0)
                {
                    Debug.LogWarning($"Removing mixer layer input after {blendTime} seconds");
                    await BlendLayerWeight(playable, AnimMixer.GetInputWeight(index), 0, blendTime);
                }

                if(!MixerCache.ContainsKey(playable))
                {

                    AnimMixer.SetLayerMaskFromAvatarMask((uint)index, new AvatarMask());
                    AnimMixer.DisconnectInput(playable);
                }
            }
            else
                Debug.LogError("Mixer layer not found");
        }

        /// <summary>
        /// Bind animation stream to animator output
        /// </summary>
        protected virtual void SetAnimSource()
        {
            var ik = IKBehaviour?.GetPlayable;

            if(ik.HasValue && !ik.Value.IsNull())
                GetOutput.SetSourcePlayable(ik.Value);
            else
                GetOutput.SetSourcePlayable(AnimMixer);
        }


        #region LimbSequence
        [TabGroup(MainTab)]
        [ShowInInspector][ReadOnly]
        protected Dictionary<int, LimbSequence> Sequence;

        public LimbSequence this[Limb key]
        {
            get
            {
                if (Sequence.TryGetValue((int)key, out var seq))
                    return seq;
                return null;
            }
        }
        public LimbSequence this[int hash] => Sequence[hash];

        #endregion

        #region IK

        public override event Action<int> OnAnimatorIKPhase;


        protected virtual void OnAnimatorIK(int layer)
        {
            OnAnimatorIKPhase?.Invoke(layer);
        }

        #region Step
        [Obsolete] // goto motion module
        public FootIK StepIK;
        #endregion


        [SerializeField]
        protected IKType m_IKType = IKType.Rigging;

        MotionSystemIKGraphBehaviour IKBehaviour;

        /// <summary>
        /// Create Playable IK and add mixer as source
        /// </summary>
        protected virtual void CreatePlayableIK()
        {
            if (IKBehaviour.IsNull())
            {
                IKBehaviour = MotionSystemIKGraphBehaviour.Create(this);
                IKBehaviour.ConnecntSource();
                IKBehaviour.GetPlayable.AddInput(AnimMixer, 0, 1);
            }
        }

        #region Playable IK

        public List<Playable> IKPlayables { get; protected set; } =new ();

        /// <summary>
        /// Inject IK playable to IK playable behaviour.
        /// </summary>
        public virtual void AddPlayableIK(Playable playable)
        {
            if(IKPlayables.CheckAdd(playable))
                IKBehaviour.AddIK(playable);
        }

        #endregion


        #region Rigging IK

        protected event Action<int,bool, Limb> VerifyIK;

        protected RigBuilder RigBuilder;
        protected Rig RightHandRig;
        protected Rig LeftHandRig;
        protected Rig RightLegRig;
        protected Rig LeftLegRig;

        protected TwoBoneIKConstraint RightHandIK;
        protected TwoBoneIKConstraint LeftHandIK;
        protected TwoBoneIKConstraint RightLegIK;
        protected TwoBoneIKConstraint LeftLegIK;

        protected virtual void BuildRiggingIK()
        {
            foreach (var pair in Sequence)
            {
                pair.Value.Verify += VerifyQualify;
            }

            gameObject.InstanceIfNull(ref RigBuilder);

            if(Resolver.TryGetModule<CharacterDataWrapper>(out var wrapper))
            {
                {
                    if (wrapper.SearchSlotBinding((x) => x.TargetBone.Equals(HumanBodyBones.RightHand), out var binding))
                        RightHandRig = HumanoidAnimationUtility.BuildRightHandRig(base.Animator, binding.Coordinate, out RightHandIK);
                    else
                        RightHandRig = HumanoidAnimationUtility.BuildRightHandRig(base.Animator, PR.Default, out RightHandIK);

                    RightHandRig.weight = 0;
                }

                {
                    if (wrapper.SearchSlotBinding((x) => x.TargetBone.Equals(HumanBodyBones.LeftHand), out var binding))
                    {
                        LeftHandRig = HumanoidAnimationUtility.BuildLeftHandRig(base.Animator, binding.Coordinate, out LeftHandIK);
                        LeftHandRig.weight = 0;
                    }
                }

                {
                    if (wrapper.SearchSlotBinding((x) => x.TargetBone.Equals(HumanBodyBones.RightFoot), out var binding))
                    {
                        RightLegRig = HumanoidAnimationUtility.BuildRightLegRig(base.Animator, binding.Coordinate, out RightLegIK);
                        RightLegRig.weight = 0;
                    }
                }

                {
                    if (wrapper.SearchSlotBinding((x) => x.TargetBone.Equals(HumanBodyBones.LeftFoot), out var binding))
                    {
                        LeftLegRig = HumanoidAnimationUtility.BuildLeftLegRig(base.Animator, binding.Coordinate, out LeftLegIK);
                        LeftLegRig.weight = 0;
                    }
                }
            }



            RigBuilder.Build();
        }


        /// <summary>
        /// Set IK wrapper enable.
        /// </summary>
        protected virtual void VerifyQualify(int hash, bool enable, Limb limb)
        {
            Rig rig;

            switch (limb)
            {
                case Limb.RightHand:
                    rig = RightHandRig;
                    break;
                case Limb.LeftHand:
                    rig = LeftHandRig;
                    break;
                case Limb.RightLeg:
                    rig = RightLegRig;
                    break;
                case Limb.LeftLeg:
                    rig = LeftLegRig;
                    break;

                default:
                    return;
            }

            //Debug.Log("Verify IK qualify : "+rig);

            if (!rig)
                return;
            var weight = enable ? 1f : 0f;

            rig.weight = weight;
            rig.enabled = enable;

            VerifyIK?.Invoke(hash, enable, limb);
        }

      
        /// <summary>
        /// Get IK wrapper
        /// </summary>
        /// <param name="hash">key to operate IK E.g hash code</param>
        public virtual LimbIKWrapper GetIK(int hash,Limb limb)
        {
            TwoBoneIKConstraint constraint;

            switch (limb)
            {
                case Limb.RightHand:
                    constraint = RightHandIK;
                    break;
                case Limb.LeftHand:
                    constraint = LeftHandIK;
                    break;
                case Limb.RightLeg:
                    constraint = RightLegIK;
                    break;
                case Limb.LeftLeg:
                    constraint = LeftLegIK;
                    break;
                default:
                    throw new KeyNotFoundException(limb.ToString());
            }

            var wrapper= new LimbIKWrapper(hash, limb, constraint);
            VerifyIK+= wrapper.VerifyIK;
            return wrapper;

        }

        #endregion


        #endregion



        #region customized Channel

        /*
        #region MotionChannel
        [ShowInInspector]
        public Vector3 Inertia_local { get; protected set; }
        [ShowInInspector]
        public Vector3 Inertia_local_acceleration { get; protected set; }
        [ShowInInspector]
        public Vector3 Inertia_world { get; protected set; }
        [ShowInInspector]
        public Vector3 Inertia_world_delta { get; protected set; }
        [ShowInInspector]
        public Vector3 Inertia_world_predict { get; protected set; }

        [ShowInInspector]
        public Quaternion Angular_local { get; protected set; }
        [ShowInInspector]
        public Quaternion Angular_local_acceleration { get; protected set; }
        [ShowInInspector]
        public Quaternion Angular_world { get; protected set; }
        [ShowInInspector]
        public Quaternion Angular_world_delta { get; protected set; }
        [ShowInInspector]
        public Quaternion Angular_world_predict { get; protected set; }

        [SerializeField]
        protected MotionChannel Channel;
        public IMotionChannel MotionChannel{ get => Channel; }

        protected JobHandle SensorJobHandle;

        [SerializeField]
        public LocomotionPoint[] DebugCatch;
        protected void KeyIn()
        {
         
            Channel.Extract();
            DebugCatch = Channel.GetChannelData;

            Channel.KeyIn.Enqueue(new Rig() { PR = ReadOnlyTransform.GetWorldPR(), ModuleVelocity = Inertia_world, Acceleration = Inertia_world_predict,AngularVelocity= Angular_world_predict}) ;
           // Channel.KeyIn(tf.position, tf.rotation, Inertia_world, Inertia_world_predict);

        }

#endregion

#region Routine
  
        private void _Update()
        {
            PR delta=new PR(), predict = new PR();

            var enumerator = Modules.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var module = enumerator.Current;
                if (!module.enabled||module.Weight==0)
                    continue;

                enumerator.Current.Slove(out var _delta, out var _predict);
                delta += _delta;
                predict += _predict;
            }

#region Inertia
            Inertia_local_acceleration = delta.GUIDs - Inertia_local;
            Inertia_local = delta.GUIDs;

            Inertia_world = ReadOnlyTransform.TransformVector(Inertia_local);
            Inertia_world_delta = Inertia_world.Multiply(ConstCache.deltaTime);
            Inertia_world_predict = ReadOnlyTransform.TransformVector(predict.GUIDs);
#endregion

#region Angular

            Angular_local_acceleration = Angular_local * Quaternion.Inverse(delta.Rotation);
            Angular_local = delta.Rotation;

            Angular_world = ReadOnlyTransform.rotation * Angular_local;
            Angular_world_delta = Quaternion.Lerp(Quaternion.identity, Angular_world, ConstCache.deltaTime);
            Angular_world_predict = ReadOnlyTransform.rotation * predict.Rotation;
#endregion

            KeyIn();
            var pos = ReadOnlyTransform.position;
            var rot = ReadOnlyTransform.rotation;
            Debug.DrawRay(pos, rot*Angular_local_acceleration*Vector3.forward, Color.yellow);
            Debug.DrawRay(pos, rot * Angular_world * Vector3.forward, Color.blue);
            Debug.DrawRay(pos, Angular_world*Vector3.forward, Color.green);

            //ReadOnlyTransform.position += Inertia_world_delta;
            if(!Angular_local.Equals(Quaternion.identity))
                ReadOnlyTransform.localRotation = Quaternion.Lerp(ReadOnlyTransform.localRotation, ReadOnlyTransform.localRotation * Angular_local,ConstCache.deltaTime*10);

            //Controller.Move(Inertia_world_delta);
        }
        */
        #endregion


    }


}

