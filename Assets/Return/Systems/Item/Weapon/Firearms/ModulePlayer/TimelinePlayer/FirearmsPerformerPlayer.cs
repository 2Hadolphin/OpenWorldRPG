using Return;
using Return.Humanoid;
using Return.Humanoid.IK;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;
using Cysharp.Threading.Tasks;
using Return.Audios;
using Return.Modular;
using Return.Motions;

namespace Return.Items.Weapons
{
    /// <summary>
    /// performer Player **Apply asset **Apply timelinecallback(event&audio) **apply aiming target
    /// </summary>
    public partial class FirearmsPerformerPlayer : BaseFirearmsPerformerPlayer, IItemModule, IFireamrsPerformerHandler, INotificationReceiver
    {
  
        #region Routine

        public override bool DisableAtRegister => false;

        /// <summary>
        /// Prepare posture mixer playable and inject to motion graph.
        /// </summary>
        protected override void Register()
        {
            base.Register();
            // link firearms posture handler

            // Develop phase => no motion system
            if (base.Item.Agent == null || !base.Item.Agent.Resolver.TryGetModule<IMotionSystem>(out var motionSystem))
            {
                Debug.LogError("Missing motion system to build graph, loading custom graph player.");
#if UNITY_EDITOR
                EditorLoadPerformerHandles(base.Item);
                //ReloadAnimationPorts(item);
#endif
                return;
            }


            if (!TimelineMixer.IsNull() && TimelineMixer.IsValid())
            {
                //CleanGraph();
                TimelineMixer.DestoryUpward(false);
                Debug.LogError("Reload performed handle");
            }

            var motionGraph = motionSystem.GetGraph;

            InitMixer(motionGraph);

            bhv.OnTransitEnd -= OnTransitFinish;
            bhv.OnTransitEnd += OnTransitFinish;

            bhv.OnStateEnd -= OnStateFinish;
            bhv.OnStateEnd += OnStateFinish;

            bhv.DevLog -= Bhv_DevLog;
            bhv.DevLog += Bhv_DevLog;

            BindOutputs(motionSystem);

            LoadPerformerHandles(motionGraph);

            // **rebind animator bug

            //Debug.Log("Rebind");

            //motionSystem.Animator.Rebind();
            //motionSystem.Rebind();
            //motionSystem.Animator.avatar=AvatarBuilder.BuildHumanAvatar(gameObject,default);
        }

        private void Bhv_DevLog(Playable obj)
        {
            if (cachePerformers.TryGetKey(obj, out var asset))
                Debug.LogError(asset);
            else
                Debug.LogError("Dev log faliure to find performer asser.");
        }

        protected override void Unregister()
        {
            Deactivate();

            Debug.Log($"{Time.frameCount} {nameof(Deactivate)} performer");

            base.Deactivate();

            Assert.IsFalse(Item.Agent.IsNull());

            Item.Agent.Resolver.TryGetModule<IHumanoidMotionSystem>(out var motionSystem);

            if (TimelineMixer.IsQualify())
            {
                //  remove layer from motion graph
                {
                    var animPort = bhv.OutputAnimationNode;

                    if (animPort.IsQualify())
                    {
                        if (motionSystem != null)
                        {
                            motionSystem.RemoveAnimationLayer(animPort, 0);
                        }
                    }
                }

                //  destroy timeline mixer
                Debug.Log($"{Time.frameCount} mixer destroy.");

                if (TimelineMixer.IsQualify() && TimelineMixer.CanDestroy())
                    TimelineMixer.Destroy();

                //TimelineMixer.Pause();
            }

            Debug.Log($"{nameof(Deactivate)} {this.GetType().Name}");
        }

        /// <summary>
        /// SetHandler performed as first module at register phase.
        /// </summary>
        /// <param name="item"></param>
        protected override void Activate()
        {
            base.Activate();

            // do not subscribe deactive bus
            //base.SetHandler(item);


            Debug.LogError(nameof(FirearmsPerformerPlayer));
            // don't deactivate when deactivate phase, manual invoke by item unequip phase 
            //base.SetHandler(item);

            //Debug.LogError("Avtivate : "+this);

            var graph = GetGraph;

            if (graph.IsValid())
            {
                //IAnimationStreamJob
                //if(EditorForceCreateGraph)
                //    EditorLoadPerformerHandles(item);

                var handles = Item.resolver.GetModules<IFirearmsPersistentPerformerHandle>().ToArray();
                LoadPerformerHandles(graph, handles);
            }
            else if (base.Item.Agent == null ||
                !base.Item.Agent.Resolver.TryGetModule<IMotionSystem>(out var motionSystem))// ||
                                                                                            //!moduleHandle.Agent.Resolver.TryGetModule<IAnimator>(out var IAnimator))
            {
                base.Deactivate();
                return;
            }
            else
            {
                Debug.LogError(new NotImplementedException());
            }

            // subscribe aduio performer
            {
                base.Item.resolver.SubscribeModule<IFirearmsAudioHandle>(this.AddPerformer);
                //var audios = item.Resolver.GetModules<IFirearmsAudioHandle>();

                //foreach (var source in audios)
                //    AddPerformer(source);
            }



            bhv.OnIdle -= Bhv_OnIdle;
            bhv.OnIdle += Bhv_OnIdle;

            if (base.Item.Agent.Resolver.TryGetModule(out IAnimator animator) && base.Item.resolver.TryGetModule<TNet.TNObject>(out var tno))
                InitIK(animator, tno);
            else
                Debug.LogError($"Failure to setup ik {animator}");

            if (BodyIK)
                BodyIK.enabled = true;
        }

        protected override void OnEnable()
        {
            //base.OnEnable();

            if (BodyIK)
                BodyIK.enabled = true;


            CreateIfNull(ref cachePerformers);
        }

        protected override void OnDisable()
        {
            //base.OnDisable();

            if (BodyIK)
                BodyIK.enabled = false;

            if (cachePerformers.NotNull())
                cachePerformers.Clear();

            OnMarkerPost = null;
        }

        private void OnDestroy()
        {
            if (JobCache != null)
            {
                foreach (var job in JobCache)
                {
                    job.Value.Dispose();
                    if (job.Key.IsQualify() && job.Key.CanDestroy())
                        job.Key.Destroy();
                }

                JobCache.Clear();
            }

            if (TimelineMixer.IsQualify() && TimelineMixer.CanDestroy())
                TimelineMixer.Destroy();

            //bhv.OnPlayableDestroy(TimelineMixer);

            cachePerformers.Clear();
        }


        #endregion



        #region Callback

        public event Action<Marker> OnMarkerPost;

        /// <summary>
        /// Timeline event reciver.
        /// </summary>
        void INotificationReceiver.OnNotify(Playable origin, INotification notification, object context)
        {
#if UNITY_EDITOR
            if (notification is UniversalMarker uMarker)
            {
                Debug.Log(notification + " On marker notify : " + uMarker.EventID.Tag + origin.GetPlayableType());
            }
            else if (notification is DefinitionMarker dMarker)
            {
                Debug.Log(notification + " On marker notify : " + dMarker.EventID.GetTag());
            }
            else
                Debug.Log(notification + " ** " + notification.id);
#endif

            if (notification is Marker marker)
            {
                OnMarkerPost?.Invoke(marker);
            }
        }

        private void Bhv_OnIdle()
        {
            if (CurrentPerformer.NotNull())
                CurrentPerformer = null;

            if (queue.TryDequeue(out var handle))
                PlayState(handle);
        }

        protected virtual void OnTransitFinish(Playable playable)
        {
            var asset=cachePerformers.FirstOrDefault(x => x.Value.Equals(playable)).Key;
            Debug.LogError($"Tansit blending finish {asset}.");


            if (queue.TryDequeue(out var handle))
                PlayState(handle);
        }

        protected virtual void OnStateFinish(Playable playable)
        {
            var asset = cachePerformers.FirstOrDefault(x => x.Value.Equals(playable)).Key;
            Debug.LogError($"Performer state finish {asset}.");
        }

        #endregion


        #region Audio

        Dictionary<AudioTemplate, AudioSource> cacheSource = new();

        /// <summary>
        /// Listen to handler container, cache new reigister module. 
        /// </summary>
        /// <param name="handle"></param>
        public virtual void AddPerformer(IFirearmsAudioHandle handle)
        {
            handle.OnAudioPlay -= Source_OnAudioPlay;
            handle.OnAudioPlay += Source_OnAudioPlay;
        }




        /// <summary>
        /// Cache audio event and calculate audio effect(level?).
        /// </summary>
        protected virtual void Source_OnAudioPlay(BundleSelecter audio)
        {
            audio.GetPreset(out var preset, out var template);

            var source = AudioManager.GetTemplateSource(template);
            //template.Config.LoadConfig();

            source.
                AttachTo(this).
                PlayOneShot(preset.Asset);

            source.PlayOneShot(preset);

            AudioManager.ReturnTemplateSource(source);
        }

        #endregion


        #region TimelineMixer

        protected ScriptPlayable<TimelineMixerBehaviour> TimelineMixer;

        TimelineMixerBehaviour bhv => TimelineMixer.GetBehaviour();

        PlayableGraph GetGraph => TimelineMixer.GetGraph();

        /// <summary>
        /// performer in graph.
        /// </summary>
        [ShowInInspector, ReadOnly]
        protected Dictionary<TimelineAsset, Playable> cachePerformers;

        /// <summary>
        /// native playable m_duringTransit to dispose while end of performer. 
        /// </summary>
        [ShowInInspector]
        public Dictionary<Playable, IPlayableDisposable> JobCache = new();

        /// <summary>
        /// Performer queue, waitting to play.
        /// </summary>
        protected Queue<TimelinePerformerHandle> queue=new();


        [Obsolete]
        public TimelinePreset CurrentPerformer;

        [Obsolete]
        public Token CurrentToken;

        #endregion


        #region Custom Bindings

        /// <summary>
        /// Custom port for asset bindings.
        /// </summary>
        Dictionary<Type, object> customPorts = new();

        /// <summary>
        /// Set custom port for asset bindings.
        /// </summary>
        public void BindPort(object obj, Type type)
        {
            if (!customPorts.TryAdd(type, obj))
                Debug.LogError(type + " failure to add port : " + obj);
        }

        /// <summary>
        /// Set output source and bind item avatar **animator **audioSource **marker
        /// </summary>
        protected virtual void BindOutputs(IMotionSystem motionSystem)
        {
            if (motionSystem.NotNull() && motionSystem is IHumanoidMotionSystem system)
            //if (!AutoOutput)
            {
                //var t = TimelineMixer.GetInput(0).GetInput(0).GetInput(0);
                //var count = t.GetInputCount() - 1;
                //count = count < 0 ? 0 : count;
                //t.SetInputWeight(count, 1);
                //Debug.Log(t.GetPlayableType());
                //var output = AnimationPlayableOutput.Create(graph, name, anim);
                //var bhv = TimelineMixer.GetBehaviour();
                //bhv.EditorBindOutput(output);

                // insert to system
                #region Inject Animation Layer
                var animMixer = bhv.OutputAnimationNode;

                var mask = new AvatarMask();

                mask.CleanHumanoidPart();

                mask.SetAvatarBodyParts(true,
                    AvatarMaskBodyPart.RightArm,
                    AvatarMaskBodyPart.LeftArm,
                    AvatarMaskBodyPart.Body,
                    AvatarMaskBodyPart.RightFingers,
                    AvatarMaskBodyPart.LeftFingers,
                    AvatarMaskBodyPart.Head);

                var bone = system.Animator.GetBoneTransform(HumanBodyBones.Spine);
                mask.AddTransformPath(bone, true);

#if UNITY_EDITOR
                var length = mask.transformCount;
                for (int i = 0; i < length; i++)
                    if (!mask.GetTransformActive(i))
                        Debug.LogError(mask.GetTransformPath(i) + " not activate.");
#endif
                mask.AddTransformPath(transform, true);

                system.AddAnimationLayer(animMixer, mask);

                Destroy(mask);

                #endregion

                LoadCustomOutput(GetGraph);

                //bhv.BlendToState();
            }
        }


        #endregion

        #region Preload Peroformer

        /// <summary>
        /// get all anim port of modules 
        /// </summary>
        [Button, PropertyOrder(10)]
        public virtual void ReloadAnimationPorts(IItem item)
        {
            var animPorts = item.resolver.GetModules<IFirearmsPerformerHandle>();

            Debug.Log("LoadPerformerPorts : " + animPorts.Count());

            //foreach (var animPort in animPorts)
            //{
            //    animPort.OnPerformerPlay -= PlayState;
            //    animPort.OnPerformerPlay += PlayState;
            //}
        }


        protected virtual Playable PlayDynamicPreset(TimelinePerformerHandle handle)
        {
            var preset = handle.Preset;

            var additiveJobs = new List<(Playable, List<PlayableBinding>, Playable)>();
            var performer = LoadPerformerHandles(GetGraph, gameObject, true, additiveJobs, preset).FirstOrDefault();

            bhv.BindingPlayables(false, (performer.Item1, performer.Item2, preset.GetAsset()));

            if (additiveJobs.Count < 1)
                Debug.LogError(preset.name+" none adjust loaded.");

            foreach (var job in additiveJobs)
                bhv.LoadAdditiveJob(job.Item1, job.Item2, job.Item3);

            bhv.BlendToState(performer.Item1, handle.blendTime, handle.Speed);

            return performer.Item1;
        }

        /// <summary>
        /// Bind marker and source ports.
        /// </summary>
        protected virtual void LoadCustomOutput(PlayableGraph graph)
        {
            var output = bhv.ScriptableOutput;

            output.AddNotificationReceiver(this);
            output.SetReferenceObject(this);

            //var scriptableOutput = ScriptPlayableOutput.Create(graph, "FirearmsCustomOutput");
            //scriptableOutput.AddNotificationReceiver(this);
            //scriptableOutput.SetReferenceObject(this);

 
            //{
            //    var audioOutput = AudioPlayableOutput.Create(graph, "firearmAudioOutput", AudioSource);
            //    bhv.EditorBindOutput(audioOutput);
            //}
        }

        #endregion


        #region Control Graph

        #region Preload

        public override void AddState(TimelinePerformerHandle preset)
        {
            if (cachePerformers.ContainsKey(preset.Preset.GetAsset()))
            {
                Debug.LogError("Repeat add state");
                return;
            }
            else
            {
                var additiveJobs = new List<(Playable, List<PlayableBinding>, Playable)>();

                var valid = LoadPerformerHandle(
                    preset.Preset,
                    Item,
                    GetGraph,
                    gameObject,
                    additiveJobs,
                    out var handle);

                if (valid)
                {
                    //if (cachePerformers.TryAdd(preset.Preset.GetAsset(), handle.Item1))
                    bhv.AddPlayable(handle.Item1,preset.Preset.GetAsset(), handle.Item2);
                }
                else
                {
                    Debug.LogError("Loading performed failure : "+preset.Preset);
                }
            }


        }

        // force break??
        public override async void RemoveState(TimelinePreset preset)
        {
            var asset = preset.GetAsset();

            Debug.Log(nameof(DisposePlayingPerformer) + " : " + preset.GetAsset().name);

            if (cachePerformers.TryGetValue(asset, out var playable))
            {
                if (JobCache.TryGetValue(playable, out IPlayableDisposable disposable))
                    JobCache.Remove(playable);

                cachePerformers.Remove(asset);


                if (bhv.IsStatePlaying(playable))
                    await DisposePlayingPerformer(playable, disposable);
                else
                    Debug.LogError(preset.name + " not playing " + bhv.CurrentPerformer.Equals(playable));


                if (!bhv.RemovePlayable(playable))
                    bhv.DisposeDynamicTimeline(playable);

                if (disposable.NotNull())
                    disposable.Dispose();
            }
            else
                Debug.LogError("cachePerformers not found : " + preset.name);
        }

        // wait state finish
        protected async UniTask DisposePlayingPerformer(Playable playable, IPlayableDisposable disposable = null, float blendTime = 0.23f)
        {
            if (!bhv.DuringTransit)
            {
                Debug.LogError("Not transit blend to idle");
                bhv.BlendToDefault(blendTime);
            }
            else
            {
                if (!bhv.CurrentPerformer.Equals(playable))
                    throw new NotImplementedException("dispose performer with targetperformer");

                if (!playable.IsDone())
                    blendTime = playable.GetTimeLeft();
                else
                    blendTime = bhv.BlendTimeLeft;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(blendTime));

            bool wait() => bhv.IsStatePlaying(playable) == false || playable.IsDone();


            if (!wait())
            {
                Debug.LogError("State not finish " + playable.GetTimeLeft());
                await UniTask.WaitUntil(wait, PlayerLoopTiming.FixedUpdate).
                                       TimeoutWithoutException(TimeSpan.FromSeconds(blendTime));
            }

            
            await UniTask.NextFrame();

            bhv.DisposeDynamicTimeline(playable);

            if (disposable.NotNull())
                disposable.Dispose();
        }


        #endregion

        #region Set Base State


        public virtual void SetIdleState(TimelinePerformerHandle handle)
        {
            if (cachePerformers.TryGetValue(handle.Preset.GetAsset(), out var playable))
            {
                if (handle.cancel) 
                {
                    bhv.RemoveIdleState(playable);
                }
                else
                {
                    Debug.Log("Set IdleMotion"+ handle.Preset);
                    bhv.SetIdleState(playable);
                }
            }
            else
                Debug.LogError("State performer not found");
        }

        #endregion

        #region Play


        public override void PlayState(TimelinePerformerHandle handle, IPerformerHandle endPerformerCallback = null)
        {
            if (CurrentPerformer == handle.Preset)
                return;

            // if exist queue
            if (queue.Contains(handle))
                return;

            Debug.Log("Play temp anim : " + handle.Preset);

            Playable playable;

            if (handle.cancel)
            {
                if (!cachePerformers.TryGetValue(handle.Preset.GetAsset(), out playable))
                    return;

                bhv.BlendToDefault(handle.blendTime);
            }
            else
            {
                playable = Play(handle);

                if (!playable.IsQualify())
                {
                    Debug.LogError("Invalid performer! " + handle.Preset);
                    return;
                }
            }


            // wait end turn idle and callback
            WaitPerformer(playable, handle, endPerformerCallback);
        }


        public override void CancelState(TimelinePreset animationType)
        {
            CurrentPerformer = null;
        }


        Playable Play(TimelinePerformerHandle handle)
        {
            if (handle.cancel || handle.Preset.IsNull())
            {
                Debug.Log("Blend to idle ");

                bhv.BlendToDefault();

                return Playable.Null;
            }
            else
            {
                if (cachePerformers.TryGetValue(handle.Preset.GetAsset(), out var playable))
                {
                    Debug.Log("performer play : " + handle.Preset);

                    bhv.BlendToState(playable, handle.blendTime, handle.Speed);
                    CurrentPerformer = handle.Preset;
                    //CurrentToken = ;
                }
                else
                {
                    Debug.LogError("Loading dynamic asset handle : " + handle.Preset);
                    playable = PlayDynamicPreset(handle);
                }

                return playable;
            }
        }
        

        #region Routine

        async void WaitPerformer(Playable playable, TimelinePerformerHandle handle, IPerformerHandle callback)
        {
            if (!playable.IsQualify())
            {
                Debug.LogError("Await invalid");
                return;
            }

            //Debug.Log("Wait performer "+Time.time);

            var duration =(float)handle.Preset.GetAsset().duration;
           
            Debug.Log(handle.Preset.GetAsset().name+" "+ duration);
          
            var valid = await this.WaitDone(playable, duration);

            // performer break
            if (!valid)
            {
                callback?.Cancel(handle.Preset);
                return;
            }

            if (bhv == null)
                return;

            Debug.Log(Time.time + " playing :" + bhv.IsStatePlaying(playable));

            await UniTask.NextFrame();

            if(CurrentPerformer==handle.Preset)
                CurrentPerformer = null;

            callback?.Finish(handle.Preset);

            Debug.Log(handle.Preset.name + " end performed : " + handle.WrapMode);


            // auto IdleState
            if (handle.WrapMode.HasFlag(WrapMode.Once))
            {

                // next motion ?
                if (queue.TryDequeue(out var newjob))
                    PlayState(newjob);
                else
                    bhv.BlendToDefault();


                // remove dynamic performer
                if (!bhv.DynamicPerformers.Contains(playable))
                    return;

                //await bhv.WaitPlayableEnd(playable);
                RemoveState(handle.Preset);
                //bhv.RemovePlayable(playable);
            }
            else
            {
                //PlayState(default);
                Debug.LogError("Unexcept play mode : "+handle.WrapMode);
            }


   
        }



        // init scope bolt 
        public virtual void QueuePerformer(TimelinePerformerHandle handle)
        {
            if (queue.Contains(handle) || queue.Contains(CurrentPerformer))
            {
                Debug.LogError("Repeat push queue performer.");
                return;
            }

            queue.Enqueue(handle);
        }

        #endregion

        /// <summary>
        /// Return false if invalid
        /// </summary>
        public async UniTask<bool> WaitDone<T>(T playable, float duration = 0) where T : struct, IPlayable
        {

            if (duration > 0)
            {

            }
            else
            {
                var time = (float)playable.GetDuration();

                var wait = time - (float)playable.GetTime();

                if (wait <= 0 || float.IsInfinity(wait))
                    wait = duration;

                duration = wait;
            }

            //Debug.Log(duration);
            //Debug.Log("Wait done"+Time.time);

            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            //Debug.Log(Time.time);

            await UniTask.WaitUntil(() => playable.IsDone()).TimeoutWithoutException(TimeSpan.FromSeconds(0.1f));

            if (this == null || this.enabled == false)
                return false;

            //Debug.Log(Time.time);

            //time -= 0.01f;

            //while (Mathf.Approximately((float)playable.GetTime(), time))
            //{
            //    await UniTask.NextFrame();
            //    Debug.Log(playable.GetTime() + " ** " + time);

            //    if (!playable.IsQualify() || !playable.IsPlaying())
            //        return false;
            //}

            return playable.IsQualify();
        }


        #endregion


        #region Additive

        public async void PlayAdditive(TimelinePreset preset, double duration, WrapMode wrapMode = WrapMode.Once)
        {
            var timeline = preset.GetAsset();
            var bindings = LoadTimelineHandle(timeline, Item, GetGraph, gameObject, autoRebalanceTree: true);

            var playable = bindings.Item1;
            var clipLength = timeline.duration;

            double speed;

            if (clipLength > duration)
            {
                speed = clipLength / duration;
            }
            else
            {
                speed = 1;
                duration = clipLength;
            }

            //speed = 0.05f;

            Debug.Log(speed + " : " + playable.GetDuration() + " : " + duration);

            //speed = 1f.Max((float)speed);

            playable.SetSpeed(speed);


            var layer = bhv.AdditiveMixer;

            var index = layer.AddInput(playable, 0, 1);
            layer.SetLayerAdditive(((uint)index), true);

            var mask = new AvatarMask();
            mask.CleanHumanoidPart();
            //mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body,true);
            mask.SetAvatarBodyParts(true, AvatarMaskBodyPart.RightArm, AvatarMaskBodyPart.LeftArm);
            layer.SetLayerMaskFromAvatarMask((uint)index, mask);

            playable.Play();

            Debug.LogError(duration + " : " + playable.GetDuration());

            if (wrapMode.HasFlag(WrapMode.Loop))
                return;


            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            Debug.Log(playable.IsQualify());

            layer.DisconnectInput(index);

            playable.Destroy();

            layer.Strip();

        }

        public async void PlayAdditive(TimelinePerformerHandle handle)
        {
            var asset = handle.Preset.GetAsset();
            var clipLength = asset.duration;

            Playable playable;

            var layer = bhv.AdditiveMixer;

            //if (cachePerformers.TryGetValue(asset, out playable))
            //{

            //}
            //else
            {
                var bindings = LoadTimelineHandle(asset, Item, GetGraph, gameObject, autoRebalanceTree: true);
                playable = bindings.Item1;

                foreach (var binding in bindings.Item2)
                    bhv.BindPorts(playable, binding, layer);

            }

            var index = layer.IndexOfInput(playable);

            layer.SetLayerAdditive(((uint)index), true);
            layer.SetInputWeight(index, 1f);

            {
                var mask = new AvatarMask();
                mask.CleanHumanoidPart();

                mask.SetAvatarBodyParts(true, AvatarMaskBodyPart.RightArm, AvatarMaskBodyPart.LeftArm);
                layer.SetLayerMaskFromAvatarMask((uint)index, mask);
                
            }


            playable.Play();


            if (!handle.WrapMode.HasFlag(WrapMode.ClampForever))
            {
                await UniTask.Delay(TimeSpan.FromSeconds(clipLength));

                Debug.Log(playable.IsQualify());

                layer.DisconnectInput(index);

                playable.Destroy();

                layer.Strip();
            }
        }

        public void StopAdditive(TimelinePreset preset)
        {
            var playable=bhv.AdditiveMixer;

            var count = playable.GetInputCount();

            for (int i = count - 1; i > 1; i--)
            {
                var input = playable.GetInput(i);

                if (!input.IsNull() && input.IsValid())
                {
                    playable.DisconnectInput(i);
                    input.Destroy();
                }
            }

            playable.SetInputCount(1);
        }

        //public async void PlayAdditive(AnimationClip clip, double duration)
        //{
        //    var playable = AnimationClipPlayable.Create(GetGraph, clip);

        //    double speed;

        //    if (clip.length > duration)
        //    {
        //        speed = clip.length / duration;
        //    }
        //    else
        //    {
        //        speed = 1;
        //        duration = clip.length;
        //    }


        //    Debug.Log(speed + " : " + playable.GetDuration() + " : " + duration);

        //    //speed = 1f.Max((float)speed);

        //    playable.SetSpeed(speed);


        //    var layer = bhv.AdditiveMixer;

        //    var index = layer.AddInput(playable, 0, 1);
        //    layer.SetLayerAdditive(((uint)index), true);

        //    var mask = new AvatarMask();
        //    mask.CleanHumanoidPart();
        //    //mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body,true);
        //    mask.SetAvatarBodyParts(true, AvatarMaskBodyPart.RightArm, AvatarMaskBodyPart.LeftArm);
        //    layer.SetLayerMaskFromAvatarMask((uint)index, mask);

        //    playable.Play();

        //    Debug.LogError(duration + " : " + playable.GetDuration());

        //    await UniTask.Delay(TimeSpan.FromSeconds(0.2f));

        //    Debug.Log(playable.IsQualify());

        //    layer.DisconnectInput(index);

        //    playable.Destroy();

        //    layer.Strip();





        //}

        #endregion

        #region Logic => Performer Order

        protected mStack<Token> PerformerQueue=new(2);

        /// <summary>
        /// Can item module execute item motion(filter token and sequence). 
        /// </summary>
        public bool CanPlay(Token token = null)
        {
            if (token)
            {
                if(PerformerQueue.TryPeek(out var current))
                {
                    var order = token.CompareTo(current);
                    if (order <= 0)
                        return true;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return !bhv.DuringTransit && bhv.StateFinish;
            }

            return true;

            if (token && CurrentToken && token != CurrentToken)
            {
                var priority = token.CompareTo(CurrentToken);

                if (priority > 0) //break and blend to
                {
                    //CancelState(CurrentPerformer);
                    return true;
                }
                else if (priority < 0)
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        #endregion


        #endregion



        #region IK => Child Handle

        public ProceduralSpineAimMatcher BodyIK;

        /// <summary>
        /// Load Body aiming ik.
        /// </summary>
        protected virtual void InitIK(IAnimator system, TNet.TNObject tno)
        {
            InstanceIfNull(ref BodyIK);
            BodyIK.Init(system, tno);

            if (tno.isMine)
            {
                BodyIK.AlignFrom = new Coordinate(transform);
                var cam = Return.Cameras.CameraManager.GetCamera<Cameras.FirstPersonCamera>();
                BodyIK.AlignTo = new Coordinate(cam.PitchTransform);
            }

            // marker module => align handle
        }

        #endregion



        #region Timeline Func

        /// <summary>
        /// Create timeline mixer playable.
        /// </summary>
        /// <param name="graph">Playable graph from motion system.</param>
        protected virtual void InitMixer(PlayableGraph graph)
        {
            if(!graph.IsValid())
                graph = PlayableGraph.Create(nameof(FirearmsPerformerPlayer));

            if (TimelineMixer.IsNull())
                TimelineMixer = ScriptPlayable<TimelineMixerBehaviour>.Create(graph);
            else
                Debug.LogError($"{nameof(TimelineMixer)} is created.");
        }

        /// <summary>
        /// Load performers data to inject playable.
        /// </summary>
        public virtual void LoadPerformerHandles(PlayableGraph graph, params IFirearmsPerformerHandle[] handles)
        {
            var timeLineObj = gameObject;

            var autoRebalanceTree = false;
#if UNITY_EDITOR
            autoRebalanceTree = true;
#endif

            // playable behaviour **track Input ports ->child mixer link port

            var timelines = new List<(Playable, List<PlayableBinding>,TimelineAsset)>(10);

            var additiveJobs = new List<(Playable, List<PlayableBinding>, Playable)>();

            Debug.Log("Loading performed count : " + handles.Length);


            foreach (var timeLineHandle in handles)
            {
                var timelineAssets = timeLineHandle.LoadPerformers();

                if (timelineAssets.IsNull() || timelineAssets.Length==0)
                    continue;

                var performers = LoadPerformerHandles(graph, timeLineObj, autoRebalanceTree, additiveJobs, timelineAssets);

                var i = 0;

                foreach (var performer in performers)
                {
                    if (!performer.Item1.IsValid())
                    {
                        Debug.LogError("PerformerHandle has invalid build : " + timeLineHandle);
                        continue;//performed.Item1 = ScriptPlayable<TimelinePlayable>.Null;
                    }
                    else
                        timelines.Add(new (performer.Item1,performer.Item2, timelineAssets[i++].GetAsset()));
                }
            }

            //Debug.Log(timelines.Count);
            var timelineBhv = TimelineMixer.GetBehaviour();
            timelineBhv.BindingPlayables(true, timelines.ToArray());

            foreach (var job in additiveJobs)
            {
                Debug.LogError(job);
                timelineBhv.LoadAdditiveJob(job.Item1, job.Item2, job.Item3);
            }
            //TimelineMixer.SetInputWeight(0, 1);
        }

        List<(Playable, List<PlayableBinding>)> LoadPerformerHandles(PlayableGraph graph, GameObject timeLineObj, bool autoRebalanceTree, List<(Playable, List<PlayableBinding>, Playable)> additiveJobs, params TimelinePreset[] presets)
        {
            var item = timeLineObj.GetComponent<AbstractItem>();

            List<(Playable, List<PlayableBinding>)> timelines = new();

            foreach (var preset in presets)
            {
                if (LoadPerformerHandle(preset, item, graph, timeLineObj, additiveJobs, out var handle, autoRebalanceTree))
                {
                    timelines.Add(handle);
                }
                else
                    Debug.LogError("Failure load performer handle : " + preset);
            }

            return timelines;
        }

        /// <summary>
        /// Return true if handle is valid and add the performer cache.
        /// </summary>
        bool LoadPerformerHandle(TimelinePreset preset, IItem item, PlayableGraph graph, GameObject timeLineObj, List<(Playable, List<PlayableBinding>, Playable)> additiveJobs, out (Playable, List<PlayableBinding>) handle, bool autoRebalanceTree = false)
        {
            try
            {
                handle = default;

                if (!preset || preset.GetAsset() is not TimelineAsset asset)
                {
                    Debug.LogError("Empty performed asset : " + preset);
                    return false;
                }

                if (cachePerformers.TryGetValue(asset,out var playable))
                {
                    Debug.LogError(playable.IsNull()+" : null "+nameof(cachePerformers) + " already contain this performed handle : " + asset.name);

                    return false;
                }


                handle = LoadTimelineHandle(asset, item, graph, timeLineObj, autoRebalanceTree);

                var valid = !handle.Item1.IsNull();

                if (valid)
                {
                    cachePerformers.Add(asset, handle.Item1);

                    //LoadAdjustJobs(preset, item, handle.Item1,handle.Item2, graph, additiveJobs);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                handle = default;
                return false;
            }

            return !handle.Item1.IsNull();
        }

        /// <summary>
        /// Load asset playable from asset asset.
        /// </summary>
        (Playable,List<PlayableBinding>) LoadTimelineHandle(TimelineAsset asset, IItem item, PlayableGraph graph, GameObject timeLineObj, bool autoRebalanceTree = false)
        {

            var tracks = asset.GetOutputTracks();


            // remove manual generate track
            var validTracks = tracks.ToList();

            var bindingCaches = tracks.ToDictionary(x => x, x => x.outputs.ToList());

            var customPlayables = new List<Playable>();

            //marker
            if(false)
            {
                var markerTrack = asset.markerTrack;

                if (markerTrack.CanCreateTrackMixer())
                {
                    validTracks.Remove(markerTrack);

                    if (markerTrack && !markerTrack.isEmpty)
                    {
                        var markerCount = markerTrack.GetMarkerCount();

                        if (markerCount > 0)
                        {
                            for (int i = 0; i < markerCount; i++)
                            {
                                var marker = asset.markerTrack.GetMarker(i);

                                if (marker is DefinitionMarker defMarker)
                                    Debug.Log(defMarker.EventID.GetTag() + " loading marker : " + defMarker.time);
                                else
                                    Debug.Log(" loading marker : " + marker.time);
                                //if (marker is UniversalMarker uniMarker)
                                //    Debug.Log(uniMarker.EventID.Tags);
                            }
                        }

                        markerTrack.CreateTrackMixer(graph, gameObject, 0);
                    }
                }
                

            }



            //List<PlayableBinding> bindings = new();

            List<Playable> animClipCache = new(1);

            #region Check Track
            foreach (var track in tracks)
            {
                if (track.muted)
                    continue;

                if (!bindingCaches.TryGetValue(track, out var curBindings))
                {
                    Debug.LogError(track + " missing binding");
                }

                ////Debug.Log(track.GetType());
                //if (track is AnimationTrack animationTrack)
                //{
                //    Debug.Log(animationTrack);
                //}
                else if (track is AnimationTrack animTrack && track is not MyAnimationTrack)
                {
                    animClipCache.Clear();

                    Debug.Log("Loading native anim track : " + asset.name);

                    validTracks.Remove(track);
                    //bindings.AddRange(track.outputs);

                    var clips = animTrack.GetClips();


                    foreach (var clip in clips)
                    {
                        Assert.IsNotNull(clip);

                        var clipcaps = clip.clipCaps;

                        if (clip.asset is AnimationPlayableAsset animClip)
                        {
                            var playable = AnimationClipPlayable.Create(graph, animClip.clip);
                            playable.SetDuration(animClip.duration);
                            playable.SetApplyPlayableIK(true);
                            //playable.SetPropagateSetTime(false);
                            //playable.SetLeadTime(clip.clipIn);

                            playable.SetApplyFootIK(animClip.applyFootIK);

                            //playable.SetOverrideLoopTime(loop != LoopMode.UseSourceAsset);
                            //playable.SetLoopTime(loop == LoopMode.On);

                            animClipCache.Add(playable);
                        }
                    }

                    if (animClipCache.Count == 1)
                        customPlayables.Add(animClipCache[0]);
                    else if (animClipCache.Count > 0)
                    {
                        var mixer = AnimationMixerPlayable.Create(graph);
                        mixer.SetPropagateSetTime(true);
                        customPlayables.Add(mixer);
                        foreach (var playable in animClipCache)
                            mixer.AddInput(playable, 0, 1);
                    }
                    else
                        Debug.LogError("None anim clip found in " + asset.name + track);
                }
                //else if (track is MasterAudioTrack masterTrack)
                //{
                //    var clips = masterTrack.GetClips();

                //    foreach (var clip in clips)
                //    {
                //        Assert.IsNotNull(clip);

                //        if (clip.asset is MasterAudioPlayableClip audiio)
                //        {
                //            var source = GameObject.Instantiate(audiio.AudioConfig.AudioTemplate.Resolve(graph.GetResolver()).AudioSource);

                //            var exposeRef = new ExposedReference<AudioSource>() { defaultValue = source };
                //            audiio.AudioSource = exposeRef;
                //            audiio.AudioSource.Resolve(graph.GetResolver());
                //        }

                //        //var exposeRef = new ExposedReference<MasterAudio>() { defaultValue = MasterAudio };
                //        //info.Audios= exposeRef;
                //        //info.Audios.Resolve(Graph.GetResolver());

                //        //var exposeRef_Controller = new ExposedReference<PlaylistController>() { defaultValue = PlaylistController };
                //        //info.controller = exposeRef_Controller;
                //        //info.controller.Resolve(Graph.GetResolver());

                //        if (clip.asset is SignalTrack)
                //            continue;
                //    }
                //}
                else if (track is MasterAudioTrack audioTrack)
                {
                    var clips = audioTrack.GetClips();

                    //validTracks.Remove(track);

                    {
                        var bindingOutputs = curBindings;//track.outputs.ToArray();

                        if (audioTrack.Template.IsNull())
                            continue;

                        if(!cacheSource.TryGetValue(audioTrack.Template,out var source))
                        {
                            source= audioTrack.CreateTemplateSource(timeLineObj);
                            cacheSource.Add(audioTrack.Template, source);
                        }

                        for (int i = 0; i < bindingOutputs.Count; i++)
                        {
                            var binding = bindingOutputs[i];
                            binding.sourceObject = source;
                            bindingOutputs[i] = binding;

                            Debug.Log(i + " / " + clips.Count() + binding.streamName + " masterClip " + binding.outputTargetType + bindingOutputs[i].sourceObject);
                        }

                        //bindings.AddRange(bindingOutputs);
                    }


                    //foreach (var clip in clips)
                    //{
                    //    Assert.IsNotNull(clip);

                    //    if (clip.asset is MasterAudioPlayableClip masterClip)
                    //    {
                    //        var audio=masterClip.CreatePlayable(graph, timeLineObj);
                    //        customPlayables.Add(audio);
                    //    }

                    //    //var exposeRef = new ExposedReference<MasterAudio>() { defaultValue = MasterAudio };
                    //    //info.Audios= exposeRef;
                    //    //info.Audios.Resolve(Graph.GetResolver());

                    //    //var exposeRef_Controller = new ExposedReference<PlaylistController>() { defaultValue = PlaylistController };
                    //    //info.controller = exposeRef_Controller;
                    //    //info.controller.Resolve(Graph.GetResolver());
                    //    else
                    //    if (clip.asset is SignalTrack bb)
                    //    {
                    //        Debug.LogError(asset+" error marker "+bb);
                    //        continue;
                    //    }
                    //}
                }

            }

            #endregion

            //???
            //TimeNotificationBehaviour aa;

            //var asset = TimelinePlayable.Create(graph, validTracks, timeLineObj, autoRebalanceTree, AutoOutput);

            var timeline = ScriptPlayable<TimelineBhv>.Create(graph);

            foreach (var cp in customPlayables)
                timeline.AddInput(cp, 0, 1);

            //foreach (var track in validTracks)
            //    bindings.AddRange(track.outputs);

            timeline.SetTraversalMode(PlayableTraversalMode.Passthrough);

            var sequence = timeline.GetBehaviour();
#if UNITY_EDITOR
            sequence.Compile(graph, timeline, validTracks, timeLineObj, autoRebalanceTree, AutoOutput);
#else
            sequence.Compile(graph, timeline, validTracks, timeLineObj, autoRebalanceTree, false);
#endif

            timeline.SetDuration(asset.duration);
            timeline.SetPropagateSetTime(true);

            Assert.IsFalse(timeline.GetDuration() == 0);

            //Debug.Log("Port : " + asset.GetInput(0).GetPlayableType() + " ** " + asset.GetDuration());


            //handle.Item1 = asset;
            //handle.Item2 = bindings;
            //return new(asset, bindings);
            var bindings = bindingCaches.SelectMany(x => x.Value).ToList();
            Debug.Log($"Create timeline {asset.name} with {bindings.Count} bindings");
            return new(timeline,bindings);

        }

        void LoadAdjustJobs(TimelinePreset preset,AbstractItem item,Playable timeline,List<PlayableBinding> bindings,PlayableGraph graph,List<(Playable, List<PlayableBinding>, Playable)> adjustJobs)
        {
            if (preset is ItemPerformerPreset itemPerformer && itemPerformer.AdjustData)
            {
                var adjust = itemPerformer.AdjustData;

                if (adjust.CreateStreamAdjustJob(item, graph, out var jobCache))
                {
                    JobCache.Add(timeline, jobCache);
                    adjustJobs.Add(new(timeline, new(bindings), jobCache.GetPlayable));
                }
            }
        }
        #endregion



    }
}

