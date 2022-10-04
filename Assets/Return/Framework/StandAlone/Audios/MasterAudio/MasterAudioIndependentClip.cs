using System.Collections.Generic;
using UnityEngine.Playables;
using Return.Audios;
using Sirenix.OdinInspector;
using UnityEngine.Audio;

namespace UnityEngine.Timeline
{
    [Tooltip("This class will create independent audio source while runtime which not effected by timelineclip or speed even graph stop.")]
    [System.Serializable]
    public class MasterAudioIndependentClip: PlayableAsset
    {
        //[Title("This class will create independent audio source while runtime which not effected by timelineclip or speed even graph stop.")]

        [PropertyTooltip("This class will create independent audio source while runtime which not effected by timelineclip or speed even graph stop.")]
        public AudioSource Source;
        public AudioTemplate AudioTemplate;
        public AudioClip Clip;

#if UNITY_EDITOR
        public bool ForceBuildMasterTemplate;
#endif

        

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            return CreateNode(graph, go);
            return AudioClipPlayable.Create(graph, Clip, true);


#if UNITY_EDITOR
            if (!ForceBuildMasterTemplate && !UnityEditor.EditorApplication.isPlaying)
            {
                if (Clip == null)
                    return Playable.Null;

                var audioClipPlayable = AudioClipPlayable.Create(graph, Clip, AudioTemplate?AudioTemplate.Config.Loop:true);
                Debug.Log(audioClipPlayable);
                return audioClipPlayable;
            }
#endif

            return ScriptPlayable<MasterAudioIndependentBehaviour>.Create(graph);
        }

        public ScriptPlayable<MasterAudioIndependentBehaviour> CreateNode(PlayableGraph graph,GameObject go)
        {
            var master = ScriptPlayable<MasterAudioIndependentBehaviour>.Create(graph);
            var clip = AudioClipPlayable.Create(graph, Clip, false);
            master.AddInput(clip, 0, 1);
            Debug.Log(clip.GetOutput(0));
            return master;
        }

        public override IEnumerable<PlayableBinding> outputs
        {
            get
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    foreach (var output in base.outputs)
                        yield return output;

                    yield break;
                }
                
#endif
                yield return ScriptPlayableBinding.Create(nameof(MasterAudioIndependentClip), AudioManager.Instance, typeof(AudioManager));
            }
        }
    }
}

