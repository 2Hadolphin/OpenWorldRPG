using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;
using UnityEngine.Audio;
using UnityEngine.Timeline;

namespace UnityEngine.Timeline
{

    /// <summary>
    /// Playable asset in timeline track
    /// </summary>
    [System.Serializable]
    public class MasterAudioPlayableClip : m_AudioPlayableAsset
    {
        [PropertyOrder(-1)]
        [BoxGroup("Preset Config")]
        [HideLabel][OnValueChanged(nameof(LoadClip),IncludeChildren =true)]
        public DynamicSoundConfig AudioConfig;

    

        [BoxGroup("Play Config")]
        [ReadOnly]
        [Tooltip("Binding at runtime.")]
        public ExposedReference<AudioSource> AudioSource;

        //public override IEnumerable<PlayableBinding> outputs => new PlayableBinding[]
        //{
        //ScriptPlayableBinding.Create(AudioConfig.AudioPreset.exposedName.ToString(),AudioConfig.AudioPreset.defaultValue,typeof(DynamicSoundGroupCreator)),
        //};

        void LoadClip()
        {
            if(AudioConfig.AudioClip)
            clip = AudioConfig.AudioClip;
            
        }

        public bool CreateBehaviour;


        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {


            if (clip == null)
            {
                Debug.LogError(Playable.Null);
                return Playable.Null;
            }
         

            var audioPlayable = base.CreatePlayable(graph, go);

            //var audioPlayable = AudioClipPlayable.Create(graph, clip, AudioConfig.AudioTemplate.Resolve(graph.GetResolver()).Config.Loop);
            //Debug.Log(audioPlayable.GetPlayableType());
#if UNITY_EDITOR

            if (/*!UnityEditor.EditorApplication.isPlaying&&*/!CreateBehaviour)
                return base.CreatePlayable(graph,go);
#endif

            if (audioPlayable.GetOutputCount() > 0)
            {
                var length = audioPlayable.GetOutputCount();
                for (int i = 0; i < length; i++)
                {
                    var output = audioPlayable.GetOutput(i);
                    if (!output.IsNull())
                        Debug.Log(output.GetPlayableType());
                    else
                        Debug.Log("Is null handle : "+output.Equals(Playable.Null));
                }
            }
            else
                Debug.Log(audioPlayable.GetPlayState());
            //return audioPlayable;

            //if(!CreateBehaviour)
            //return audioPlayable;

            var apb = new MasterAudioPlayableBehaviour
            {
                AudioPreset = AudioConfig.AudioPreset.Resolve(graph.GetResolver()),
                AudioTrackAsset = this,
                //AudioSource=AudioSource.Resolve(graph.GetResolver())
            };

            var masterNode = ScriptPlayable<MasterAudioPlayableBehaviour>.Create(graph, apb);


            var outputcount = masterNode.GetOutputCount();

            for (int i = 0; i < outputcount; i++)
            {
                var output=masterNode.GetOutput(i);
                if (output.IsNull())
                    Debug.Log(i+" null output " + output);
                else
                    Debug.Log(output.GetPlayableType());
            }

            //return masterNode;
            //masterNode.AddInput(audioPlayable, 0, 1);

            masterNode.SetTraversalMode(PlayableTraversalMode.Passthrough);
            masterNode.SetOutputCount(1);
            masterNode.SetPropagateSetTime(true);
            Debug.Log(masterNode.GetPlayState());

            //return masterNode;

            Playable emptyNode = ScriptPlayable<MasterAudioMixer>.Create(graph,1);

            //emptyNode = Playable.Create(graph, 1);



            emptyNode.Play();
            

            emptyNode.ConnectInput(0, audioPlayable, 0,1);
            //emptyNode.ConnectInput(1, masterNode, 0,1);

            emptyNode.SetTraversalMode(PlayableTraversalMode.Passthrough);
            emptyNode.SetPropagateSetTime(true);

            //emptyNode.Play();

            return emptyNode;

            //audioPlayable.AddInput(masterNode, 0, 1);
            //return audioPlayable;
        }
        /// <summary>
        /// Returns a description of the PlayableOutputs that may be created for this asset.
        /// </summary>
        public override IEnumerable<PlayableBinding> outputs
        {
            get 
            {
                yield return AudioPlayableBinding.Create(name, this);
                yield return ScriptPlayableBinding.Create(AudioSource.exposedName.ToString(), AudioSource.defaultValue, typeof(AudioSource));

            }
        }


    }

    [System.Serializable]
    public class m_AudioPlayableAsset : AudioPlayableAsset
    {

    }

}

