using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using System;
using UnityEngine.Timeline;
using Return;
using Cysharp.Threading.Tasks;


namespace Return.Items.Weapons
{
    public class TimelineMixerBehaviour : IPlayableBehaviour
    {
        #region Bhv

        /// <summary>
        /// Timeline mixer newIdlePlayable
        /// </summary>
        protected Playable behaviourPlayable;

        protected PlayableGraph Graph => behaviourPlayable.GetGraph();

        /// <summary>
        /// List of all root playable node, easy to destroy or pause. 
        /// </summary>
        protected HashSet<Playable> cachePlayables = new(5);


        #region Bhv Routine

        public void OnPlayableCreate(Playable playable)
        {
            var graph = playable.GetGraph();

            // bhv
            {
                playable.SetTraversalMode(PlayableTraversalMode.Mix);
                playable.SetPropagateSetTime(true);
                playable.SetOutputCount(1);

                behaviourPlayable = playable;
                ScriptableOutput = ScriptPlayableOutput.Create(graph, "ScriptableOutput");
                ScriptableOutput.SetSourcePlayable(behaviourPlayable, 1);
            }

            // marker
            {
                //ScriptableOutput.SetSourcePlayable(MarkerMixer, 0);

                MarkerMixer = Playable.Create(graph);
                MarkerMixer.SetTraversalMode(PlayableTraversalMode.Mix);
                MarkerMixer.SetPropagateSetTime(true);

                behaviourPlayable.AddInput(MarkerMixer,0,1);
            }

            // animation
            {
                StateMixer = AnimationMixerPlayable.Create(graph);
                StateMixer.SetTraversalMode(PlayableTraversalMode.Mix);
                StateMixer.SetPropagateSetTime(true);

                // additive layer
                AdditiveMixer = AnimationLayerMixerPlayable.Create(graph, 0, true);
                AdditiveMixer.SetTraversalMode(PlayableTraversalMode.Mix);
                AdditiveMixer.SetPropagateSetTime(true);

                AdditiveMixer.AddInput(StateMixer, 0, 1);
            }

            // audio
            {
                //AudioMixer = AudioMixerPlayable.Create(graph);
                //AudioMixer.SetTraversalMode(PlayableTraversalMode.Mix);
                //AudioMixer.SetPropagateSetTime(true);

                //AudioMixer.AddInput(Playable.Null, 0, 1);
            }

            InitStates();
        }

        public void OnBehaviourPause(Playable playable, FrameData info)
        {

        }

        public void OnBehaviourPlay(Playable playable, FrameData info)
        {

        }

        public void OnGraphStart(Playable playable)
        {

        }

        public void OnGraphStop(Playable playable)
        {

        }

        public void OnPlayableDestroy(Playable playable)
        {
            var graph = Graph;

            //  scriptable
            {
                graph.DestroyOutput(ScriptableOutput);
                graph.DestroySubgraph(MarkerMixer);
                Debug.LogError($"mixer valid : {MarkerMixer.IsValid()}");
                //MarkerMixer.SafeDestory();
            }

            //  audio
            {
                foreach (var pair in audioCache)
                {
                    PlayableOutput output = pair.Value;

                    if (!output.IsOutputNull())
                    {
                        var mixer = output.GetSourcePlayable();
                        graph.DestroySubgraph(mixer);
                        graph.DestroyOutput(output);
                        //mixer.SafeDestory();
                    }
                    else
                    {
                        Debug.LogError("audio output null " + output);

                        if (pair.Key != null)
                            Debug.LogError($"Dispose audio source {pair.Key.gameObject}");
                    }
            
                }
            
                     
            }

            //  animation-inject motion graph
            {
                graph.DestroySubgraph(OutputAnimationNode);
                //OutputAnimationNode.SafeDestory();

                //foreach (var pb in Timelines)
                //    pb.SafeDestory();


                //foreach (Playable pb in DynamicPerformers)
                //    pb.SafeDestory();
            }

            Debug.Log($"{Time.frameCount} Destroy mixer.");
        }


        public void PrepareFrame(Playable playable, FrameData info)
        {
            // wait handler push new performer
            if (await)
            {
                await = false;
                return;
            }

            // valid state transit
            if (!DuringTransit)
            {
                // check playing statues finish
                if (!CurrentPerformer.IsQualify())
                    return;

                // valid state change
                if(CurrentPerformer.ValidDone())
                    // valid done
                    if(CurrentPerformer.IsDone())
                        // invoke event
                        OnStateEnd?.Invoke(CurrentPerformer);

                return;
            }

            try
            {
                // valid transit target
                Assert.IsTrue(TargetState.NotNull());
                if (!TargetState.Playable.IsQualify())
                    throw new InvalidProgramException("Transit target is invalid.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                DuringTransit = false;
                return;
            }


            var finish = currentBlendTime >= targetBlendTime;

            if (finish)
                DuringTransit = !finish;

            var t = currentBlendTime / targetBlendTime;

            #region Multi Transit
            //if (MultiTransit)
            //{
            //    var count = Timelines.Length;

            //    for (int i = 0; i < count; i++)
            //    {
            //        var p = Timelines[i];

            //        if (p.IsNull() || !p.IsValid())
            //        {
            //            Debug.LogError("LoadweightFailuare : " + i);
            //            continue;
            //        }

            //        float weight;

            //        if (finish)
            //        {
            //            if (Timelines[i].Equals(TargetPlayable))
            //            {
            //                weight = 1;
            //            }
            //            else
            //            {
            //                weight = 0;
            //                p.Pause();
            //            }
            //            LastWeights[i] = weight;
            //        }
            //        else
            //        {
            //            weight = LastWeights[i];//StateMixer.GetInputWeight(i);

            //            if (Timelines[i].Equals(TargetPlayable))
            //                weight = Mathf.Lerp(weight, 1, t);
            //            else if (weight == 0)
            //                continue;
            //            else
            //                weight = Mathf.Lerp(weight, 0, t);

            //        }

            //        //Debug.Log(IdleState.Equals(p) ? "IdleState" : DynamicPerformers.FirstOrDefault(width => width.Equals(p)) != null ? "Dynamic" : !Timelines.First(width => width.Equals(p)).Equals(Playable.Null) ? "Preset" : "Unknow");
            //        p.SetOutputsWeight(weight);
            //        //StateMixer.SetInputWeight(i, weight);
            //    }

            //    float weightToMute = Mathf.Lerp(1, 0, t);

            //    foreach (var dynamic in DynamicPerformers)
            //    {
            //        Playable performer = dynamic;

            //        if (!performer.IsQualify())
            //            continue;

            //        float weight = dynamic.Weight;

            //        if (performer.Equals(TargetPlayable))
            //        {
            //            if (finish)
            //                weight = 1;
            //            else
            //                weight = Mathf.Lerp(0, 1, t);
            //        }
            //        else if (weight > 0)
            //        {
            //            if (finish)
            //                weight = 0;
            //            else
            //                weight = weightToMute;
            //        }
            //        else
            //            return;

            //        dynamic.Weight = weight;

            //        if (performer.IsQualify())
            //            performer.SetOutputsWeight(weight);
            //    }


            //    //{
            //    //    Playable idle = IdleState;

            //    //    if (idle.IsQualify() && idle.IsPlaying())
            //    //    {
            //    //        var weight = 0f;

            //    //        if (TargetPlayable.Equals(idle))
            //    //        {
            //    //            weight = 1f - weightToMute;
            //    //        }
            //    //        else if (IdleState.Weight > 0)
            //    //        {
            //    //            weight = weightToMute;
            //    //        }

            //    //        if (finish)
            //    //        {
            //    //            if (TargetPlayable.Equals(idle))
            //    //            {
            //    //                weight = 1f;
            //    //                OnIdle?.Invoke();
            //    //            }
            //    //            else
            //    //                weight = 0f;
            //    //        }

            //    //        if (weight > 0 || finish)
            //    //            idle.SetOutputsWeight(weight);

            //    //        IdleState.Weight = weight;
            //    //    }
            //    //}
            //}
            #endregion
            //else
            {
                if (finish)
                {
                    if (CurrentState.NotNull())
                        CurrentState.Weight = 0;

                    if (TargetState.NotNull())
                        TargetState.Weight = 1;

                    CurrentState = TargetState;
                    TargetState = null;


                    if (ConstStates.Contains(CurrentState))
                        OnIdle?.Invoke();
                }
                else
                {
                    if (TargetState.NotNull())
                        TargetState.Weight = Mathf.Lerp(0, 1, t);

                    if (CurrentState.NotNull())
                        CurrentState.Weight = Mathf.Lerp(1, 0, t);
                }
            }

            currentBlendTime += info.deltaTime;
            //Debug.Log(currentBlendTime);

            if (finish)
            {
                await = true;
                OnTransitEnd?.Invoke(CurrentState);
            }
        }


        public void ProcessFrame(Playable playable, FrameData info, object playerData)
        {

        }

        #endregion

        #endregion



        #region Marker
        /// <summary>
        /// Scriptable playable output  **MixerBehaviour    **MarkerMixer
        /// </summary>
        public ScriptPlayableOutput ScriptableOutput;

        /// <summary>
        /// Definition marker root node.
        /// </summary>
        public Playable MarkerMixer { get; protected set; }

        public HashSet<Playable> Markers=new();

        #endregion


        #region Audio

        /// <summary>
        /// Audio source and output binding for audio playable. 
        /// </summary>
        Dictionary<AudioSource, AudioPlayableOutput> audioCache = new();

        #endregion

        #region Animation

        /// <summary>
        /// RootTransform node to connect animation output.
        /// </summary>
        public Playable OutputAnimationNode => AdditiveMixer;

        /// <summary>
        /// Animation mixer contains all animation playable.
        /// </summary>
        public AnimationMixerPlayable StateMixer { get; protected set; }

        /// <summary>
        /// Layer mixer, use to blend additive m_duringTransit.
        /// </summary>
        public AnimationLayerMixerPlayable AdditiveMixer { get; protected set; }

        #endregion


        #region State

        /// <summary>
        /// Wait performer handler push event, current states will freeze.
        /// </summary>
        public bool await { get; protected set; }

        #region Event

        public event Action<Playable> DevLog;

        /// <summary>
        /// Invoke while the transition of current timeline finish.
        /// </summary>
        public event Action<Playable> OnTransitEnd;

        /// <summary>
        /// Invoke while the duration of current timeline finish.
        /// </summary>
        public event Action<Playable> OnStateEnd;

        #endregion

        #region IdleState

        /// <summary>
        /// Base performer state. **IdleMotion **aiming
        /// </summary>
        public mStack<Playable> ConstStates = new(2);

        /// <summary>
        /// Default state.
        /// </summary>
        //public PlayableWrapper IdleState;

        public event Action OnIdle;

        public virtual void SetIdleState(Playable playable)
        {
            // set state order
            if (ConstStates.Peek().Equals(playable))
            {
                if (CurrentState == playable)
                    return;
            }
            else
            {
                if (ConstStates.Contains(playable))
                    ConstStates.SetAsNew(playable);
                else
                    ConstStates.Push(playable);

                Assert.IsTrue(ConstStates.IndexOf(playable) == ConstStates.Count - 1);
            }


            if(CurrentState.NotNull())
                BlendToDefault();

            //if (IdleState.IsNull())
            //    IdleState = new PlayableWrapper(playable);
            //else if(DuringTransit)
            //{
            //    //if(IdleState==TargetPlayable)


            //}


            //var curState = IdleState;

            //IdleState = new PlayableWrapper(playable, 1f);

            //// not during transit && (not idle && )


            //// blend between idle
            //if (!DuringTransit)
            //    if (curState.NotNull() && curState != playable && TargetPlayable == null)
            //    {
            //        if (CurrentPlayable.NotNull() && ((!CurrentPlayable.Playable.IsDone())))
            //            return;

            //        BlendToIdle();
            //    }

        }


        /// <summary>
        /// 
        /// </summary>
        public virtual async void RemoveIdleState(Playable playable)
        {

            var waitBlending = ConstStates.Peek().Equals(playable);

            ConstStates.Remove(playable);

            // as first
            if (waitBlending)
            {
                waitBlending =
                 CurrentState == playable || TargetState == playable;

                waitBlending |= DuringTransit;

                waitBlending |= TargetState.NotNull() && TargetState.Playable.IsDone();

                if (waitBlending)
                    await UniTask.WaitUntil(() => DuringTransit == false || TargetState.IsNull());

                // if state add back
                if (ConstStates.Contains(playable))
                {
                    Debug.LogError("IdleMotion state add back");
                    return;
                }

                BlendToDefault();
            }
   
        }

        //[Obsolete]
        //public virtual void SetIdle(Playable newIdlePlayable, List<PlayableBinding> bindings)
        //{
        //    if (IdleState.NotNull())
        //    {
        //        if (newIdlePlayable.Equals(IdleState))
        //            return;

        //        IdleState.Playable.SafeDestory();
        //    }

        //    //childCache.TryGetValue(newIdlePlayable, out var childs);
        //    IdleState = new PlayableWrapper(newIdlePlayable);

        //    var length = bindings.Count;

        //    Debug.Log("SetIdle");

        //    for (int i = 0; i < length; i++)
        //    {
        //        var input = newIdlePlayable.GetInput(i);
        //        Debug.Log(input.IsNull() ? "null" : input.GetPlayableType());
        //        Debug.Log(string.Format("Binding {0} {1} {2}", i, bindings[i].streamName, bindings[i].outputTargetType));
        //    }

        //    for (int i = 0; i < length; i++)
        //    {
        //        BindPorts(newIdlePlayable, bindings[i], i);
        //    }



        //    newIdlePlayable.Init();

        //    Assert.IsTrue(newIdlePlayable.IsValid());

        //    newIdlePlayable.SetOutputsWeight();
        //}

        #endregion


        #region Persistent

        PlayableWrapper CurrentState;
        public Playable CurrentPerformer => CurrentState.NotNull() ? CurrentState.Playable : Playable.Null;

        PlayableWrapper TargetState;
        public Playable TargetPerformer => TargetState.NotNull() ? TargetState.Playable : Playable.Null;

        public List<Playable> Timelines;

        bool m_duringTransit;

        /// <summary>
        /// Is transit between states.
        /// </summary>
        public bool DuringTransit
        {
            get => m_duringTransit;
            set
            {
                //Debug.Log(value);
                m_duringTransit = value;
            }
        }

        public bool StateFinish
        {
            get
            {
                bool finish = true;

                if (DuringTransit)
                    finish = false;
                else if (CurrentPerformer.IsQualify())
                {
                    // pass idle state
                    if(!ConstStates.Contains(CurrentPerformer))
                    {
                        //if (!CurrentPerformer.IsDone())
                        //    finish = false;

                        if (!CurrentPerformer.HasFinish())
                            finish = false;
                    }
                }

                Debug.LogError($"Check finish {finish}.");

                return finish;
            }
        }

        //public bool MultiTransit = false;

        float targetBlendTime;
        float currentBlendTime;

        public float BlendTimeLeft => targetBlendTime - currentBlendTime;

        float BlendTime
        {
            set
            {
                targetBlendTime = value;
                currentBlendTime = float.Epsilon;
            }
        }

        /// <summary>
        /// RegisterHandler states
        /// </summary>
        void InitStates(int count = 0)
        {

            Debug.LogError("SetPorts : " + count);

            var zero = count == 0;

            //if (zero || LastWeights == null)
            //    LastWeights = new float[count];
            //else
            //    Array.Resize(ref LastWeights, LastWeights.Length + count);

            cachePlayables = new(count + 2);

            if (zero || Timelines == null)
                Timelines = new(count);
            //Timelines = new Playable[count];
            else
                Timelines.Capacity = Timelines.Count+ count;
                //Array.Resize(ref Timelines, Timelines.Length + count);
        }


        #endregion

        #region Dynamic

        public List<PlayableWrapper> DynamicPerformers = new();

        public virtual bool DisposeDynamicTimeline(Playable timeline)
        {
            var remove = DynamicPerformers.Remove(timeline);

            if (remove) // remove from persistent performers
            {
                timeline.DisconnectOutputs();

                if (timeline.IsPlaying())
                    timeline.Pause();

                // disconnect and strip

                Assert.IsTrue(timeline.CanDestroy());
                timeline.Destroy();
                //performer.DestoryUpward();
            }
            else
                Debug.LogError(new KeyNotFoundException("Not found dynamicPerformers."));

            return remove;
        }

        #endregion

        #region Setup

        /// <summary>
        /// Bind out put ports
        /// </summary>
        public virtual void BindPorts(Playable timeline, PlayableBinding binding,Playable mixer=default)
        {
            #region Marker
  

            if (binding.streamName == "Markers")
            {
                BindMarker(timeline, binding);
                return;
            }

            #endregion

            #region Animation

            if (binding.outputTargetType == typeof(Animator))
            {
                if (mixer.IsQualify()) 
                    BindAnimation(mixer, timeline, binding);
                else
                    BindAnimation(StateMixer,timeline, binding);


                return;
            }

            #endregion

            #region Audio

            if (binding.outputTargetType == typeof(AudioSource))
            {
                BindAudio(timeline, binding);
                return;
            }
            #endregion

            Debug.LogError(binding.outputTargetType + " exception port " + binding.sourceObject);
        }

        public virtual void BindMarker(Playable timeline, PlayableBinding binding)
        {
            if (MarkerMixer.HasInput(timeline))
            {
                Debug.LogError(binding.streamName + " loading playable repeat : " + binding.sourceObject);
                return;
            }

            Debug.Log("Marker target : " + binding.sourceObject);
            var bindPort = timeline.IndexOfBehaviour<TimeNotificationBehaviour>();

            if (bindPort < 0)
            {
                //foreach (var playable in performer.GetEnumerator())
                //{
                //    Debug.LogError(playable.IsNull() ? "null" : playable.GetPlayableType());
                //}
                Debug.LogError("Can't not find type of marker playable");
                return;
            }

            //MarkerMixer.AddInput(performer, index, 1);

            {
                //var input = performer.GetInput(index);
                //performer.DisconnectInput(input);

                //Debug.Log(input.GetPlayableType());

                if (MarkerMixer.HasInput(timeline))
                {
                    Debug.LogError(bindPort + " loading playable repeat : " + timeline);
                    return;
                }



                //var length= timeline.GetOutputCount();
                //for (int i = 0; i < length; i++)
                //{
                //    var output=timeline.GetOutput(i);
                //    var type = output.IsNull() ? null :output.GetPlayableType();
                //    Debug.LogError("marker type : " + type);
                //    if (output.GetPlayableType() == typeof(PlayableOutput))
                //        continue;
                //}
                Markers.Add(timeline);

                timeline.SetPortOutput(bindPort);

                MarkerMixer.AddInput(timeline, bindPort, 1);
            }
        }


        public virtual int BindAnimation(Playable mixer,Playable timeline, PlayableBinding binding)
        {
            var bindPort = timeline.IndexOfType<AnimationClipPlayable>();

            if (bindPort < 0)
                bindPort = timeline.IndexOfType<AnimationLayerMixerPlayable>();

            if (bindPort < 0)
                bindPort = timeline.IndexOfType<AnimationMixerPlayable>();

            if (bindPort < 0)
            {
                Debug.LogError("Can't found animation playable.");
                return -1;
            }

            if (mixer.HasInput(timeline))
            {
                Debug.LogError(bindPort + " loading playable repeat : " + timeline);
                return -1;
            }

            timeline.SetPortOutput(bindPort);

            return mixer.AddInput(timeline, bindPort, 0);
        }

        public virtual void BindAudio(Playable timeline,PlayableBinding binding)
        {
            AudioMixerPlayable audioMixer;

            // custom output
            if (binding.sourceObject is AudioSource source)
            {
                if (!audioCache.TryGetValue(source, out var output))
                {
                    output = AudioPlayableOutput.Create(Graph, source.name, source);
                    audioMixer = AudioMixerPlayable.Create(Graph, 0);
                    output.SetSourcePlayable(audioMixer);
                    audioCache.Add(source, output);
                }
                else
                    audioMixer = (AudioMixerPlayable)output.GetSourcePlayable();

                Debug.LogError("Binding custom audio source " + source);
            }
            else
            {
                Debug.LogError("None audio template binding.");
                //audioMixer = AudioMixer;
                return;
            }

            if (audioMixer.HasInput(timeline))
            {
                Debug.LogError(binding.streamName + " loading playable repeat : " + timeline.GetPlayableType());
                return;
            }

            var bindPort = timeline.IndexOfType<AudioMixerPlayable>();

            if (bindPort < 0)
                bindPort = timeline.IndexOfType<AudioClipPlayable>();

            if (bindPort < 0)
            {
                foreach (var bb in timeline.GetEnumerator())
                {
                    Debug.LogError(bb.IsNull() ? "null" : bb.GetPlayableType());
                }
                Debug.LogError("Can't found input of audio playable.");
                return;
            }
            else
                Debug.Log(timeline.GetInput(bindPort).GetPlayableType());

            timeline.SetPortOutput(bindPort);
            Debug.Log(timeline.GetOutput(bindPort).IsNull() ? "null" : timeline.GetOutput(bindPort).GetPlayableType());
            audioMixer.AddInput(timeline, bindPort, 1);
        }

        #endregion

        #endregion


        #region Playable Control

        /// <summary>
        /// Play IdleMotion state
        /// </summary>
        public virtual void BlendToDefault(float blendTime = 0.2f)
        {
            if (CurrentState.IsNull())
            {
                Debug.LogError(Time.frameCount + " : " + new NotImplementedException("Blend without pickup"));
                //return;
            }
            // blend between state
            else if (DuringTransit)
            {
                Debug.LogError("During transit will not switch idle");
                return;
            }
            else if (!CurrentPerformer.IsDone())
            {
                Debug.LogError("Current playable not done" + CurrentPerformer.GetRatio());

                // ????
                if (Timelines.Contains(CurrentPerformer))
                {
                    Debug.Log("Couldn't break persistent and not idle performer.");
                    return;
                }
                else
                {
                    DevLog?.Invoke(CurrentPerformer);
                    Debug.LogException(new NotImplementedException("IdleMotion state not set as const state."));
                    return;
                }

                //if (Timelines.Contains(CurrentState))
                //    return;
            }


            #region Blend to Idle

            Playable targetTimelinePlayable = ConstStates.Peek();

            // check idle valid
            Assert.IsTrue(targetTimelinePlayable.IsQualify());

            // immediately set idle
            if (CurrentState.IsNull())
            {
                Debug.Log("Set IdleMotion immediately");
                CurrentState = targetTimelinePlayable;
                CurrentState.Weight = 1f;
                return;
            }
            else if (CurrentState.Equals(targetTimelinePlayable)) // check playback
            //if (targetTimelinePlayable.Equals(CurrentPlayable))
            {
                // playback
                if (TargetState.NotNull())
                {
                    if (TargetState.Equals(targetTimelinePlayable))
                    {
                        Debug.LogError("Unknow match");
                        return;
                    }

                    Debug.LogError("Switch state and playback.");
                    currentBlendTime = targetBlendTime - currentBlendTime;
                    CurrentState = TargetState;
                    TargetState = targetTimelinePlayable;
                    DuringTransit = true;
                    return;
                }
                else // target null
                {
                    Debug.LogError("Unknow null");
                    return;
                }
            }
            else if (TargetState.NotNull() && TargetState.Equals(targetTimelinePlayable))
            {
                Debug.LogError("Equal Target State");
                return;
            }
            else
            {

            }

            // set idle as target state 

            BlendToState(targetTimelinePlayable, blendTime);
            #endregion
        }


        /// <summary>
        /// Play performer to state.
        /// </summary>
        public virtual void BlendToState(Playable targetTimelinePlayable, float blendTime = 0.25f, double speed = 1)
        {
            Debug.LogError("Blending State");

            if (DuringTransit)
                return;

            //if(CurrentPerformer.IsQualify())


            if (!targetTimelinePlayable.IsQualify())
            {
                Debug.LogError("Blending to empty performed, change target as IdleState.");
                BlendToDefault(blendTime);
                return;
            }
            else if (!ConstStates.Contains(targetTimelinePlayable))
            {

                //if (Timelines.Contains(targetTimelinePlayable))
                {
                    targetTimelinePlayable.SetTime(0);
                    targetTimelinePlayable.SetDone(false);
                }


                // playback
                if (targetTimelinePlayable.Equals(CurrentState) || targetTimelinePlayable.Equals(TargetState))
                {
                    Debug.LogError("Port loading repeat : " + targetTimelinePlayable);

                    targetTimelinePlayable.Play();
                    return;
                }
            }
            //else if(DynamicPerformers.Contains(targetTimelinePlayable))
            //{

            //}

            //blendTime = Mathf.Min(blendTime, targetTimelinePlayable.GetDurationF() * 0.25f);


            blendTime = Mathf.Max(0.1f, blendTime);

            bool immediately = blendTime <= 0;

            immediately |= CurrentState.IsNull();// || (TargetPlayable.IsNull()/* && !TargetPlayable.Playable.IsQualify()*/);

            DuringTransit = !immediately;

            if (immediately)
            {
                if (CurrentState.NotNull())
                    CurrentState.Weight = 0;

                CurrentState = targetTimelinePlayable;
                CurrentState.Weight = 1f;

                targetTimelinePlayable.Play();

                Debug.LogError("Play immediately");
                return;
            }
            else
            {
                BlendTime = blendTime;
            }


            if (ConstStates.Contains(targetTimelinePlayable))
            {
                //targetTimelinePlayable.Init();
            }
            // check persistent or dynamic
            else if (!Timelines.Contains(targetTimelinePlayable))
            {
                foreach (var dynamic in DynamicPerformers)
                {
                    if (dynamic.Equals(targetTimelinePlayable))
                    {
                        DynamicPerformers.Add(new(targetTimelinePlayable));
                        targetTimelinePlayable.Init();
                        break;
                    }
                }
            }

            {
                targetTimelinePlayable.CheckPlay();
                TargetState = new PlayableWrapper(targetTimelinePlayable);
            }

            behaviourPlayable.CheckPlay();
        }

        public virtual void SetTime(double time = 0)
        {
            foreach (Playable performer in DynamicPerformers)
            {
                performer.SetTime(time);
            }

            foreach (var timeline in Timelines)
            {
                timeline.SetTime(time);

                //if (childCache.TryGetValue(performer, out var childs))
                //    foreach (var child in childs)
                //        child.SetTime(time);

            }
        }

        public async UniTask WaitPlayableEnd(Playable playable)
        {
            if (TargetState != playable && CurrentState != playable)
                return;

            await UniTask.WaitUntil(() => TargetState != playable || CurrentState != playable, PlayerLoopTiming.LastFixedUpdate);
            return;
        }


        #endregion

        /// <summary>
        /// Return true if playable is primary playing performer.
        /// </summary>
        public virtual bool IsStatePlaying(Playable playable)
        {
            return playable == CurrentState || playable == TargetState;
        }

        /// <summary>
        /// Add persistent performer.
        /// </summary>
        public virtual void AddPlayable(Playable playable,TimelineAsset asset,List<PlayableBinding> bindings)
        {
            var index = Timelines.Count;//Timelines.Length;

            InitStates(index + 1);

            playable.SetPropagateSetTime(true);

            Timelines.Add(playable);

            foreach (var binding in bindings)
                BindPorts(playable, binding,Playable.Null);

            playable.Pause();
        }

        public virtual bool RemovePlayable(Playable playable)
        {
#if UNITY_EDITOR
            if (CurrentState == playable)
                Debug.LogError("Remove playable from " + nameof(CurrentState));

            if (TargetState == playable)
                Debug.LogError("Remove playable from " + nameof(TargetState));
#endif

            var valid= Timelines.Remove(playable);

            if (!valid)
                valid = DynamicPerformers.Remove(playable);

            if (playable.IsQualify())
                playable.Destroy();

            return valid;
        }

        /// <summary>
        /// Inject playable to mixers. **animation **audio **marker
        /// </summary>
        public virtual void BindingPlayables(bool persistent = true, params (Playable, List<PlayableBinding>, TimelineAsset)[] timelines)
        {
            var length = timelines.Length;

            Debug.Log("BindingPlayables : " + length);

            var start = 0;

            if (persistent)
            {
                if (!Timelines.IsNull())
                    start = Timelines.Count;//Timelines.Length;

                InitStates(start+length);
            }


            for (int i = 0; i < length; i++)
            {
                var performer = timelines[i].Item1;
                performer.SetPropagateSetTime(true);

                if (persistent)
                {
                    Timelines.Add(performer);
                    //TimelinePorts.Add(performer, i);
                }


                var bindings = timelines[i].Item2;
                var count = bindings.Count;

                Debug.LogError("Binding performer : "+timelines[i].Item3.name);

                for (int k = 0; k < count; k++)
                {

                    Debug.Log("Playable type "+(performer.GetInput(k).IsNull() ? "null" : performer.GetInput(k).GetPlayableType()));
                    Debug.Log(string.Format("Binding sn : {0} {1} {2}", k, bindings[k].streamName, bindings[k].outputTargetType));
                }

                for (int k = 0; k < count; k++)
                {
                    var binding = bindings[k];
                    //Debug.Log(binding.streamName);
                    BindPorts(performer, binding,Playable.Null);
                }

                performer.Pause();
            }
        }


        /// <summary>
        /// Load adjust playable **animation adjust job
        /// </summary>
        public virtual void LoadAdditiveJob(Playable timeline, List<PlayableBinding> bindings, Playable additiveJob)
        {
            Playable sourcePlayable;

            var length = bindings.Count;

            for (int i = 0; i < length; i++)
            {
                var binding = bindings[i];

                if (binding.streamName == "Markers")
                {
                    continue;
                }

                switch (binding.outputTargetType)
                {
                    case Type type when type == typeof(Animator):

                        sourcePlayable = timeline.GetInput(i);
                        break;

                    default:
                        Debug.LogError(new NotImplementedException(binding.outputTargetType.ToString()));
                        continue;
                }

                if (sourcePlayable.IsNull())
                {
                    Debug.LogError(binding.streamName + " failure to load adjust playable because of missing performer adjust playable. " + timeline.GetInput(i));
                    return;
                }
                else
                    Debug.Log(binding.streamName + additiveJob.IsNull());

                timeline.InsertPlayable(sourcePlayable, additiveJob);
            }
        }


   
        [Obsolete] // dev phase
        public void EditorBindOutput(PlayableOutput output)
        {
            Assert.IsFalse(output.IsOutputNull());

            if (output.IsPlayableOutputOfType<AnimationPlayableOutput>())
            {
                ((AnimationPlayableOutput)output).SetSourcePlayable(StateMixer, 0);
            }
            //else
            //if (output.IsPlayableOutputOfType<AudioPlayableOutput>())
            //{
            //    ((AudioPlayableOutput)output).SetSourcePlayable(AudioMixer, 0);
            //}
            //else if (output.IsPlayableOutputOfType<ScriptPlayableOutput>())
            //{
            //    output.SetSourcePlayable(MarkerMixer, 0);
            //}
        }
    }

    public class PlayableWrapper : NullCheck, IEquatable<Playable>, IEquatable<PlayableWrapper>
    {
        public PlayableWrapper(Playable playable, float weight = 0)
        {
            this.Playable = playable;
            //Weight = weight;
        }

        public readonly Playable Playable;

        protected float m_Weight;



        /// <summary>
        /// Set playable weight
        /// </summary>
        public virtual float Weight
        {
            get => m_Weight;
            set
            {
                m_Weight = value;

                if (Playable.IsQualify())
                {
                    Playable.SetOutputsWeight(value);
                }

            }
        }

        public static implicit operator Playable(PlayableWrapper wrapper)
        {
            return wrapper.NotNull() && wrapper.Playable.IsQualify() ? wrapper.Playable : Playable.Null;
        }

        public static implicit operator PlayableWrapper(Playable playable)
        {
            return new PlayableWrapper(playable);
        }

        public bool Equals(Playable other)
        {
            return this.Playable.Equals(other);
        }

        public bool Equals(PlayableWrapper other)
        {
            return this.Playable.Equals(other.Playable);
        }

        public static bool operator ==(PlayableWrapper wrapper,Playable playable)
        {
            return wrapper.NotNull() && wrapper.Playable.Equals(playable);
        }

        public static bool operator !=(PlayableWrapper wrapper, Playable playable)
        {
            return wrapper.IsNull() || !wrapper.Playable.Equals(playable);
        }

        public override int GetHashCode()
        {
            return Playable.GetHashCode();
        }
    }
}

