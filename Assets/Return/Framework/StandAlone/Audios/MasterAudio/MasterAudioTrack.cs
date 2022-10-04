using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using DarkTonic.MasterAudio;
using UnityEngine.Audio;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Return.Audios;
using Sirenix.OdinInspector;

namespace UnityEngine.Timeline
{
    [TrackColor(9, 8, 7)]
    [TrackBindingType(typeof(AudioSource),TrackBindingFlags.AllowCreateComponent)]
    //[TrackBindingType(typeof(MasterAudio))]
    [TrackClipType(typeof(MasterAudioPlayableClip),true)]
    [TrackClipType(typeof(MasterAudioIndependentClip))]
    //[TrackClipType(typeof(AudioPlayableAsset))]
    public class MasterAudioTrack : AudioTrack
    {
        public AudioTemplate Template;

        [Tooltip("Do not assign.")]
        [System.Obsolete]
        public AudioSource TrackSourceOutput;

        [OnValueChanged(nameof(LoadClipTimelineData))]
        public bool CreateBehaviour;

        // bind template to new audio source (from pool)
        public virtual AudioSource CreateTemplateSource(GameObject go)
        {
            AudioSource source;

            if (TrackSourceOutput != null)
            {
                source = TrackSourceOutput;
    
            }
            else
            {
                source = Instantiate(Template.AudioSource, go.transform);
                source.gameObject.name = Template.name;
            }

            source.LoadConfig(Template.Config);

            return source;
        }

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            LoadClipTimelineData();


            var clipPlayable= base.CreatePlayable(graph, gameObject, clip);
            Debug.Log("Create Track Playable "+( clipPlayable.IsNull()? clipPlayable:clipPlayable.GetPlayableType()));

            //if(!CreateBehaviour)
            return clipPlayable;

            var output = (AudioPlayableOutput)graph.GetOutputByType<AudioPlayableOutput>(0);

#if UNITY_EDITOR

            if (output.IsOutputNull())
            {
                AudioSource AudioSource;
                if (TrackSourceOutput)
                {
                    AudioSource=GameObject.Instantiate(TrackSourceOutput);
                }
                else if(!gameObject.TryGetComponent(out AudioSource))
                {
                    AudioSource = new GameObject("MasterAudioTrackPlayer").AddComponent<AudioSource>();
                }
                AudioSource.hideFlags = HideFlags.DontSave;

                output = AudioPlayableOutput.Create(graph, "m_AudioOutput", AudioSource);
            }

#endif
            var playable = base.CreatePlayable(graph, gameObject, clip);
            output.SetSourcePlayable(playable);
            Debug.Log(playable);
            return playable;
        }

        [Button]
        public void LoadClipTimelineData()
        {
            // Load clip config
            foreach (var clip in GetClips())
            {
                if (clip.asset is MasterAudioPlayableClip masterClip)
                {
                    //if (masterClip.clip)
                    //    CreateClip(masterClip.clip);
                }
                else if (clip.asset is MasterAudioIndependentClip independentClip)
                {
                    //independentClip.Start = clip.start;
                    //independentClip.End = clip.end;
                    //independentClip.loop = clip.postExtrapolationMode.HasFlag(TimelineClip.ClipExtrapolation.Loop);
                }
                Debug.Log(this+clip.displayName);
            }
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {

            LoadClipTimelineData();
            return base.CreateTrackMixer(graph, go, inputCount);


            var director = graph.GetResolver() as PlayableDirector;
            var binding = director.GetGenericBinding(this) as AudioSource;
            foreach (var clip in GetClips())
            {
                var playableAsset = clip.asset as MasterAudioPlayableClip;
                if (playableAsset != null)
                    playableAsset.AudioSource.defaultValue = binding;
            }

            Debug.Log("Create Track Playable " + binding);

            return ScriptPlayable<MasterAudioMixer>.Create(graph, inputCount);

            //return base.CreateTrackMixer(graph, go, inputCount);

            Debug.Log("Create Track Count " + inputCount);
            var clipPlayable= base.CreateTrackMixer(graph, go, inputCount);
            Debug.Log("Create Track Playable " + clipPlayable.GetPlayableType());

            return clipPlayable;


        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);
            Debug.Log(clip);
            LoadClipTimelineData();
        }

        public override bool CanCreateTrackMixer()
        {
            //Debug.Log(base.CanCreateTrackMixer());
            return base.CanCreateTrackMixer();
        }

        public override IEnumerable<PlayableBinding> outputs
        {
            get { yield return AudioPlayableBinding.Create(name, this); }
        }

        //protected override void OnAfterTrackDeserialize()
        //{
        //    LoadClipTimelineData();
        //}
    }

    public class MasterAudioMixer : IPlayableBehaviour, ITimelineClipAsset
    {
        double Duration;
        double time;

        Playable Parent;
        AudioSource source;
        AudioPlayableOutput Output;
        public ClipCaps clipCaps => ClipCaps.All;

        public void OnBehaviourPause(Playable playable, FrameData info)
        {
            playable.Play();
        }

        public void OnBehaviourPlay(Playable playable, FrameData info)
        {
            playable.Play();
            time = playable.GetTime();
        }

        public void OnGraphStart(Playable playable)
        {
            Assertions.Assert.IsTrue(playable.IsValid());
            Duration = playable.GetDuration();
        }

        public void OnGraphStop(Playable playable)
        {
            //playable.SetDone(true);
        }

        public void OnPlayableCreate(Playable playable)
        {
            Assertions.Assert.IsFalse(playable.IsNull());
            playable.SetTraversalMode(PlayableTraversalMode.Mix);
            Parent = playable.GetInput(0);
            //source = new GameObject().AddComponent<AudioSource>();
            //Output = AudioPlayableOutput.Create(playable.GetGraph(),"MasterAudioMixerOutput",source);
            //Output.SetSourcePlayable(playable.GetOutput(0));
        }

        public void OnPlayableDestroy(Playable playable)
        {
            if (source)
                GameObject.DestroyImmediate(source.gameObject);
            //if(playable.CanDestroy())
            //playable.Destroy();
        }

        public void PrepareFrame(Playable playable, FrameData info)
        {
            Assert.IsTrue(playable.IsValid());
            time += info.deltaTime;
            if(time>Duration)
            {
                playable.SetTime(0);
                time = 0;
                if(Parent.IsNull())
                Parent = playable.GetInput(0);

                Parent.SetDone(false);
            }

          
        }

        public void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            throw new System.NotImplementedException();
        }
    }

}

