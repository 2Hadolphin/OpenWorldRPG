using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Audio;
using Return.Audios;
using Sirenix.OdinInspector;

namespace UnityEngine.Timeline
{
    [TrackColor(50, 50, 50)]
    [TrackBindingType(typeof(AudioSource), TrackBindingFlags.AllowCreateComponent)]
    [TrackClipType(typeof(MasterAudioIndependentClip))]
    //[TrackClipType(typeof(AudioPlayableAsset))]
    public class MasterAudioIndependentTrack : PlayableTrack
    {
        [Title("This class will create independent audio source while runtime which not effected by timelineclip or speed even graph stop.")]
        public AudioTemplate Template;

        public AudioSource TrackSourceOutput;

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            Debug.Log(clip.start);

            var clipPlayable = Playable.Null;

            if(clip.asset is IPlayableAsset playable)
            {
                if(playable is MasterAudioIndependentClip masterIndependent)
                {
                    var master= masterIndependent.CreateNode(graph, gameObject);

                    var bhv=master.GetBehaviour();

                    //bhv.Duration = masterIndependent.Clip.length;
                    clipPlayable = master;
                    Debug.Log(master.GetDuration());
                }
                else
                {

                    clipPlayable = playable.CreatePlayable(graph, gameObject);
                    Debug.LogError(clipPlayable.GetPlayableType());
                }
            }

            return clipPlayable;
 

            //return ScriptPlayable<MasterAudioIndependentBehaviour>.Create(graph, 0);
            return base.CreatePlayable(graph, gameObject);

            Debug.Log("Create Track Playable " + (clipPlayable.IsNull() ? clipPlayable : clipPlayable.GetPlayableType()));

            //if(!CreateBehaviour)
            return clipPlayable;

            var output = (AudioPlayableOutput)graph.GetOutputByType<AudioPlayableOutput>(0);

#if UNITY_EDITOR

            if (output.IsOutputNull())
            {
                AudioSource AudioSource;
                if (TrackSourceOutput)
                {
                    AudioSource = GameObject.Instantiate(TrackSourceOutput);
                }
                else if (!gameObject.TryGetComponent(out AudioSource))
                {
                    AudioSource = new GameObject("MasterAudioTrackPlayer").AddComponent<AudioSource>();
                }
                AudioSource.hideFlags = HideFlags.DontSave;

                output = AudioPlayableOutput.Create(graph, "m_AudioOutput", AudioSource);
            }

#endif
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
                Debug.Log(clip.displayName);
            }
        }

        

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            Debug.Log("Create Track Mixer "+inputCount);
            var mixer= base.CreateTrackMixer(graph, go, inputCount);
            mixer.SetTraversalMode(PlayableTraversalMode.Mix);
            mixer.SetPropagateSetTime(false);
            return mixer;

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
            var clipPlayable = base.CreateTrackMixer(graph, go, inputCount);
            Debug.Log("Create Track Playable " + clipPlayable.GetPlayableType());

            return clipPlayable;


        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            
            base.OnCreateClip(clip);
            Debug.Log(clip+" ** "+clip.start);
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
}

