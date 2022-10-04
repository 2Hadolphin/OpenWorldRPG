//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.Animations;
//using UnityEngine.Playables;
//using UnityEngine;
//using Return.Humanoid;
//using Unity.Collections;
//using System;
//using System.Linq;

//using UnityEngine.Experimental;
//using Sirenix.OdinInspector;

//namespace Return.Humanoid.Animation
//{

//    [RequireComponent(typeof(Animator))]
//    public partial class Animator_Humanoid : HumanoidModularSystem, IAnimator
//    {
//        ReadOnlyTransform RootTransform;

//        protected Animator _Animator;
//        protected PlayableGraph m_Graph;
//        protected PlayableOutput m_Output;
//        protected StreamBoneHandleMap StreamBoneHandleMap;
        

//        [SerializeField]
//        protected HashSet<AnimationBehaviourBundle> BhvBundles=new HashSet<AnimationBehaviourBundle>();

//        protected DoubleSidedDictionaryInPairs<AnimationBehaviourBundle, IAnimationBehaviour> BundleCatch;

//        public NativeArray<TransformStreamHandle> StreamBoneHandles;
//        public NativeArray<Job_Humanoid_Mirror.HandlePair> MirrorPairBones;
//        public NativeArray<TransformStreamHandle> MirrorBones;

//        protected event Action<Vector2> Locomotion_Level;
//        public event Action<float> JumpSchedule;
//        public event Action<Vector3> RootScale;
//        protected event Action<float> MovementVelocity;


//        [SerializeField][Distance(0,1f)]
//        protected float _Jump;
//        [SerializeField]
//        protected Vector2 _Move;

//        public float Jump 
//        { 
//            get
//            {
//                return _Jump;
//            }
//            set
//            {
//                _Jump = value;
//                JumpSchedule?.Invoke(value);
//            }
//        }

//        public Vector2 Move
//        {
//            get
//            {
//                return _Move;
//            }
//            set
//            {
//                print(value);
//                _Move = value;
//                Locomotion_Level?.Invoke(value);
//                MovementVelocity?.Invoke(value.magnitude);
//            }
//        }


//        #region IAnimator
//        ReadOnlyTransform IAnimator.RootBone => /*Agent.root*/default;
//        public Animator GetAnimator => _Animator;
//        PlayableGraph IAnimator.GetGraph => m_Graph;
//        NativeArray<TransformStreamHandle> IAnimator.MirrorBones => MirrorBones;

//        event Action<Vector2> IAnimator.Locomotion_Level { add { Locomotion_Level += value; } remove { Locomotion_Level -= value; } }
//        event Action<float > IAnimator.MovementVelocity { add { MovementVelocity += value; }remove { MovementVelocity -= value; } } 

//        FreeFormBlendNode IAnimator.CreateBlendNode()
//        {
//            return CharacterRoot.AddComponent<FreeFormBlendNode>();
//        }


//        HumanBone IAnimator.GetHumanBone(HumanBodyBones humanBodyBone)
//        {
//            if (HumanBoneMap.TryGetValue((int)humanBodyBone, out var sn))
//                return _Animator.avatar.humanDescription.human[sn];
//            else
//                return new HumanBone();
//        }

//        public SkeletonBone GetSkeletonBone(HumanBodyBones humanBodyBone)
//        {
//            if (HumanBoneMap.TryGetValue((int)humanBodyBone, out var sn))
//                if (SkeletonBoneMap.TryGetValue(_Animator.avatar.humanDescription.human[sn].boneName, out sn))
//                    return _Animator.avatar.humanDescription.skeleton[sn];

//            return new SkeletonBone();
//        }

        

//        #endregion


//        /// <summary>
//        /// Behaviour Map with AvatarMaskBodyPart
//        /// </summary>
//        public class BehaviourMap
//        {
//            /// <summary>
//            /// Source port
//            /// </summary>
//            public AnimationLayerMixerPlayable Layermixer;

//            public AnimationLayerMixerPlayable FinalMixer;
//            public BehaviourMap(PlayableGraph playableGraph)
//            {
//                Layermixer = AnimationLayerMixerPlayable.Create(playableGraph);
//                FinalMixer = AnimationLayerMixerPlayable.Create(playableGraph);
//                var sn=FinalMixer.AddInput(Layermixer, 0,1);
//                FinalMixer.SetLayerAdditive((uint)sn,true);

//                var length = (int)AnimationBehaviourBundle.StreamType.LastType;
//                Map = new HashSet<IAnimationBehaviour>[length];
//                for (int i = 0; i < length; i++)
//                {
//                    Map[i] = new HashSet<IAnimationBehaviour>();
//                }
//            }
//            public readonly HashSet<IAnimationBehaviour>[] Map;
//            public Playable Output => FinalMixer;
//            /// <summary>
//            /// schedule playable in behaviour map
//            /// </summary>
//            public void ActivateBehaviour(IAnimationBehaviour behaviour)
//            {
//                var bundle = behaviour.Bundle;
//                var dependence = bundle.Dependence;
//                int length = 0;
//                var newSource=behaviour.GetOutputPort;
//                HashSet<IAnimationBehaviour> map = null;
//                switch (bundle._StreamType)
//                {
//                    case AnimationBehaviourBundle.StreamType.Source:
//                        {
//                            map = Map[(int)bundle._StreamType];
//                            var enumerator = map.GetEnumerator();
//                            while (enumerator.MoveNext())
//                            {
//                                // if repeat
//                                if (enumerator.Current.Bundle.Equals(bundle))
//                                    throw new AccessViolationException();
//                            }

//                            behaviour.Port = (uint)Layermixer.AddInput(behaviour.GetOutputPort, 0, bundle.Priority);
//                            Layermixer.SetLayerMaskFromAvatarMask(behaviour.Port, bundle.HumanoidMask);
//                            map.Add(behaviour);
//                        }
//                        break;


//                    case AnimationBehaviourBundle.StreamType.Inherit:

//                        {
//                            IAnimationBehaviour targetBhv = null;
//                            if (dependence)
//                            {
//                                length = Map.Length;
//                                for (int i = 0; i < length; i++)
//                                {
//                                    targetBhv = Map[i].Where(width => width.Bundle.Equals(dependence)).First();
//                                    if (targetBhv != null)
//                                        break;
//                                }
//                                throw new InvalidProgramException("Script unfinish");
//                            }
//                            else
//                            {
//                                map = Map[(int)AnimationBehaviourBundle.StreamType.Inherit];
//                                var enumerator = map.GetEnumerator();
//                                var priority = bundle.Priority;
//                                byte difference = byte.MaxValue;
//                                while (enumerator.MoveNext())
//                                {
//                                    var mapPriority = enumerator.Current.Bundle.Priority;
//                                    if (priority > mapPriority && priority - mapPriority < difference)
//                                    {
//                                        difference = (byte)(priority - mapPriority);
//                                        targetBhv = enumerator.Current;
//                                    }
//                                }
//                            }

//                            if (!Map[(int)bundle._StreamType].Add(behaviour))
//                                Debug.LogError(new KeyNotFoundException());
//                            // stanard inherit --single rows
//                            {
//                                var inputTarget = targetBhv == null ? Layermixer : targetBhv.GetOutputPort;
//                                var outputTarget = inputTarget.GetOutput(0);
//                                //disconnect
//                                outputTarget.DisconnectInput(inputTarget);
//                                behaviour.Port=(uint)newSource.AddInput(inputTarget, 0, 1f);
//                                outputTarget.AddInput(newSource, 0, 1f);
//                            }
//                            map.Add(behaviour);
//                        }
//                        break;


//                    case AnimationBehaviourBundle.StreamType.Blend:
//                        var sn=(uint)FinalMixer.AddInput(newSource, 0, 10);
//                        FinalMixer.SetLayerMaskFromAvatarMask((uint)sn, bundle.HumanoidMask);
//                        FinalMixer.SetLayerAdditive(sn, false);
//                        break;
//                }
 
//            }
//            public void DiscardBehaviour(IAnimationBehaviour behaviour)
//            {
//                switch (behaviour.Bundle._StreamType)
//                {
//                    // Clean Layer?
//                    case AnimationBehaviourBundle.StreamType.Source:
//                        behaviour.GetOutputPort.DisconnectOutput();



//                        break;
//                    // Destory
//                    case AnimationBehaviourBundle.StreamType.Inherit:
//                        {
//                            var length = (int)AnimationBehaviourBundle.StreamType.LastType;
//                            for (int i = 0; i < length; i++)
//                            {
//                                if (Map[i].Remove(behaviour))
//                                {
//                                    var Input = behaviour.GetInputPort.GetInput(0);
//                                    behaviour.GetInputPort.DisconnectInput(0);
//                                    var output = behaviour.GetOutputPort.GetOutput(0);
//                                    output.DisconnectInput(behaviour.GetOutputPort);
//                                    output.AddInput(Input,0,1f);
//                                }
//                            }
//                        }
//                        break;
//                    // Destory
//                    case AnimationBehaviourBundle.StreamType.Blend:
//                        behaviour.GetOutputPort.DisconnectOutput();
//                        break;
//                }
//            }
//        }
//        /// <summary>
//        /// Actived behaviour map
//        /// </summary>
//        protected BehaviourMap BhvMap;

//        /// <summary>
//        /// RegisterHandler animation bundle
//        /// </summary>
//        public void AddBehaviourBundle(AnimationBehaviourBundle behaviourBundle,out IAnimationBehaviour humanoidBehaviour)
//        {
//            if (!BhvBundles.Add(behaviourBundle))
//                throw new KeyNotFoundException(behaviourBundle + " has exist!");

//            //  Instance behaviour

//            behaviourBundle.LoadBehaviour(this, out humanoidBehaviour);
//            BundleCatch.Add(behaviourBundle, humanoidBehaviour);

//            //BhvMap.ActivateBehaviour(humanoidBehaviour);

//            #region Enable/Disable Setting

//            if(behaviourBundle._ParameterType.HasFlag(AnimationBehaviourBundle.ParameterType.Script))
//                Behaviours.MonoBinding.Add(humanoidBehaviour);

//            if (behaviourBundle._ParameterType.HasFlag(AnimationBehaviourBundle.ParameterType.MotionChannel))
//                Behaviours.MotionChannel.Add(humanoidBehaviour);

//            /*
//            switch (behaviourBundle._ParameterType)
//            {
//                case AnimationBehaviourBundle.ParameterType.Script:
//                    // already out behaviour

//                    break;

//                case AnimationBehaviourBundle.ParameterType.MotionChannel:
//                    // add linstener

//                    break;
//            }
//            */
//            #endregion

//            return;

//            foreach (var type in Enum.GetValues(typeof(AnimationBehaviourBundle.BehaviourType)))
//            {
//                var ValueType = (AnimationBehaviourBundle.BehaviourType)type;
//                var behaviours = new Dictionary<byte, AnimationBehaviourBundle>();

//                #region GetBundleType
//                foreach (var behaviour in BhvBundles)
//                {
//                    if (ValueType.Equals(behaviour._BehaviourType))
//                    {
//                        if (behaviours.ContainsKey(behaviour.Priority))
//                            behaviours.Add(behaviour.Priority++, behaviour);
//                        else
//                            behaviours.Add(behaviour.Priority, behaviour);
//                    }

//                }

//                if (behaviours.Count == 0)
//                    continue;
//                #endregion

//                var behaviourEnumator = behaviours.Values.GetEnumerator();
//                var parentPlayable = new Playable();
//                var parentMask = new AvatarMask();
//                while (behaviourEnumator.MoveNext())
//                {
//                    var bundle = behaviourEnumator.Current;
//                    bundle.LoadBehaviour(this, out var playableBehaviour);

//                    Playable playable = playableBehaviour.GetOutputPort;
//                    BundleCatch.Add(bundle, playableBehaviour);

//                    if (parentPlayable.IsNull())
//                    {
//                        parentPlayable = playable;
//                        parentMask = playableBehaviour.Mask;
//                    }
//                    else
//                    {

//                        switch (bundle._StreamType)
//                        {
//                            case AnimationBehaviourBundle.StreamType.Source:
//                                Debug.LogError("Bundle behaviours contain two root stream " + bundle.name);
//                                break;
//                            case AnimationBehaviourBundle.StreamType.Inherit:
//                                playableBehaviour.AddSource(parentPlayable);
//                                if (playableBehaviour.Mask)
//                                    parentMask = mAvatarMaskUtility.Combine(parentMask, playableBehaviour.Mask);
//                                break;
//                            case AnimationBehaviourBundle.StreamType.Blend:
//                                Debug.LogError(bundle.name + " require blend animateStream");
//                                break;
//                        }
//                        parentPlayable = playable;
//                    }
//                 }

//                /*
//                if (parentPlayable.IsValid())
//                {
//                    print(parentPlayable.GetOutputCount().ToString());
//                    var sn = maskMixer.AddInput(parentPlayable, 0, 1);
//                    maskMixer.SetLayerMaskFromAvatarMask((uint)sn, parentMask);
//                }
//                else
//                    print(parentPlayable.IsValid());
//                */
//            }
//        }



//        public void SetBehaviour(IAnimationBehaviour behaviour,bool enable)
//        {
           
//            if (enable)
//            {
//                BhvMap.ActivateBehaviour(behaviour);
//                //behaviour.Init(this);
//            }

//            else
//            {
//                BhvMap.DiscardBehaviour(behaviour);
//                behaviour.DisposePlayingPerformer();
//            }
        

           
//        }

//        protected virtual void RemoveBehaviour(object sender, EventArgs e = null)
//        {
//            var timer = sender as Playable_Timer;
//            if (timer.IsNull())
//                return;

//            RemoveBehaviour(timer.ClockTarget);
//        }
//        protected virtual void RemoveBehaviour(IAnimationBehaviour behaviour)
//        {
//            if (!BundleCatch.Reverse.TryGetValue(behaviour, out var bundle))
//                return;

//        }
//        /// <summary>
//        /// in= humanBodyBone, out= avatar human sn
//        /// </summary>
//        protected Dictionary<int, int> HumanBoneMap;
//        /// <summary>
//        /// in= skeletonName, out= avatar skeleton sn
//        /// </summary>
//        protected Dictionary<string, int> SkeletonBoneMap;
//        void LoadBoneData()
//        {
//            var description = _Animator.avatar.humanDescription;
//            var humanMap = mHumanoidUtility.HumanNameMap;

//            var boneChain = description.human;
//            var length = boneChain.Length;
//            HumanBoneMap = new Dictionary<int, int>(length);
//            for (int i = 0; i < length; i++)
//            {
//                var name = boneChain[i].humanName;
//                if (!humanMap.TryGetValue(name.Replace(" ", ""), out var humanBodyBone))
//                    continue;

//                HumanBoneMap.SafeAdd((int)humanBodyBone, i);
//            }
            
//            var skeletons = description.skeleton;
//            length = skeletons.Length;
//            SkeletonBoneMap = new Dictionary<string, int>(length);
//            for (int i = 0; i < length; i++)
//            {
//                SkeletonBoneMap.SafeAdd(skeletons[i].name, i);
//            }
//        }
//        public IMotionChannel MotionChannel { get; protected set; }

//        public ReadOnlyTransform IKGoalRootBone { get; protected set; }
//        public TransformStreamHandle GetBindingBone(HumanBodyBones humanBodyBone)
//        {
//            return StreamBoneHandleMap.GetHandle(humanBodyBone);
//        }

//        protected bool IsChannelUpdate;
//        protected class Behaviours_
//        {

//            public HashSet<IAnimationBehaviour> MotionChannel=new HashSet<IAnimationBehaviour>();
//            /// <summary>
//            /// hand IK system, handle like, 
//            /// </summary>
//            public HashSet<IAnimationBehaviour> MonoBinding=new HashSet<IAnimationBehaviour>();
//        }
//        protected Behaviours_ Behaviours = new Behaviours_();

        

//        public virtual void Init(IMotionChannel motionChannel)
//        {
//            _Animator = CharacterRoot.GetComponent<Animator>();
//            RootTransform = _Animator.transform;
//            BundleCatch = new DoubleSidedDictionaryInPairs<AnimationBehaviourBundle, IAnimationBehaviour>();
//            LoadBoneData();
//            //_Animator.keepAnimatorControllerStateOnDisable = true;
//            //_Animator.stabilizeFeet = false;
//            IKGoalRootBone = RootTransform.GetComponentInChildren<SkinnedMeshRenderer>().rootBone; 
//            {
//                StreamBoneHandleMap = new StreamBoneHandleMap(_Animator);
//                StreamBoneHandles = new NativeArray<TransformStreamHandle>(mHumanoidUtility.GetStreamBones(_Animator), Allocator.Persistent);
//                //reamake mirror handle
//            }

//            // Load Anim parameter System ()=>controller>State>Blend tree(SO) || analyze motion channel
//            {
//                //build behaviour<locomotion><seat><operate><crouch>/Inertia/IK
//                MotionChannel = motionChannel; 
//                //MotionChannel.Update += () => IsChannelUpdate = true;
//            }

//            m_Graph = PlayableGraph.Create(CharacterRoot.GetInstanceID()+"_HumanoidController");
//            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
//            BhvMap = new BehaviourMap(m_Graph);
//            m_Output = AnimationPlayableOutput.Create(m_Graph, "humanoidOutput", _Animator);
//            m_Output.SetSourcePlayable(BhvMap.Output);





//            //m_Graph.Evaluate();
//            m_Graph.Play();


//            #region Link Motor

//            //Agent.I.Motor.Velocity_Local +=(width)=> Move=width;
//            //(_Agent as HumanoidAgent_Simulation).
//            #endregion

//            #region MotionChannel
//            if (!EnableBehaviour)
//                StartCoroutine(BehaviourManager());

//            #endregion
//        }

//        public class Preset
//        {
//            public float EnableLocomotionDistance=0.2f;
//        }

//        protected bool EnableBehaviour = false;

//        IEnumerator BehaviourManager()
//        {
//            EnableBehaviour = true;
//            while (EnableBehaviour)
//            {
//                yield return ConstCache.WaitForSeconds(0.2f);
//                yield return StartCoroutine(AnalyzeChannelAnimation());


//            }
//        }

//        /// <summary>
//        /// Analyze MotionChannel and manager animation behaviour data
//        /// </summary>
//        IEnumerator AnalyzeChannelAnimation()
//        {

//            var points=MotionChannel.ChannelPoints;
//            var index = MotionChannel.Key_Current;
//            var length = points.Length;

//            var channelBhv=new HashSet<string>();
            
//            // _checkCache motionChannel-bhv whether activated in channel
//            #region VerifyBehaviour



//            #endregion

//            #region Unload Behaviour
//            var unloadBhv = new HashSet<string>();
//            for (int i = 0; i < index; i++)
//            {
//                yield return null;
//            }

//            #endregion


//            #region Prepare Behaviour
//            var prepareBhv = new HashSet<string>();
//            for (int i = index; i < length; i++)
//            {
//                yield return null;
//            }
//            #endregion


       

//        }

//        public virtual void Transit(AnimationBehaviourBundle behaviour)
//        {
//            // m_Graph.Connect(behaviour.LoadBehaviour(), 0, m_MixedGraph,m_MixedGraph.GetInputCount());
//        }

//        private void OnDestroy()
//        {
//            if (null != BundleCatch)
//            {
//                var enumerator = BundleCatch.GetEnumerator();

//                while (enumerator.MoveNext())
//                {
//                    enumerator.Current.m_Value.DisposePlayingPerformer();
//                }
//            }

//            if (m_Graph.IsValid())
//                m_Graph.Destroy();
//            StreamBoneHandles.CleanMemory();
//            MirrorBones.CleanMemory();

//            if (MirrorPairBones.IsCreated)
//                MirrorPairBones.DisposePlayingPerformer();
//        }


//        public Vector3 ToLoacl(Vector3 vector)
//        {
//            return RootTransform.InverseTransformVector(vector);
//        }

//        public float GetSkeketonPathDistance(HumanBodyBones parent, HumanBodyBones target,Dictionary<ReadOnlyTransform,SkeletonBone>map=null)
//        {
//            if (map == null)
//                map = SkeletonMap;



//            var p=_Animator.GetBoneTransform(parent);
//            var t= _Animator.GetBoneTransform(target);
//            print(p);
//            if (!p || !t)
//                return -1;

//            if (!t.IsChildOf(p))
//                return -1;

//            var distance = 0f;

//            var middle = p;


//            foreach (ReadOnlyTransform next in middle)
//            {
//                if (t.Equals(next))
//                {
//                    break;
//                }
//                else
//                {
//                    if (!t.IsChildOf(next))
//                        continue;

//                    middle = next;
//                    if (map.TryGetValue(middle, out var skeleton))
//                        Debug.LogError(new KeyNotFoundException(middle.name));

//                    distance += skeleton.position.magnitude;
//                    continue;
//                }
//            }

//            return distance;
//        }

//        public Dictionary<ReadOnlyTransform, HumanBodyBones> TransformMap
//        {
//            get
//            {
//                var bones = mHumanoidUtility.AllHumanBodyBones;
//                var d = new Dictionary<ReadOnlyTransform, HumanBodyBones>(bones.Length);

//                foreach (var bodyBone in bones)
//                {
//                    var bone = _Animator.GetBoneTransform(bodyBone);
//                    if (bone != null)
//                        d.Add(bone, bodyBone);
//                }
//                return d;
//            }
//        }

//        public Dictionary<ReadOnlyTransform, SkeletonBone> SkeletonMap
//        {
//            get
//            {
//                var root = transform;// Agent.root;
//                var skeletons = _Animator.avatar.humanDescription.skeleton;
//                var d = new Dictionary<ReadOnlyTransform, SkeletonBone>(skeletons.Length);

//                foreach (var skeleton in skeletons)
//                {
//                    var bone = root.Find(skeleton.name);
//                    if (bone != null)
//                        d.Add(bone, skeleton);
//                }
//                return d;
//            }
//        }
//    }

//}
