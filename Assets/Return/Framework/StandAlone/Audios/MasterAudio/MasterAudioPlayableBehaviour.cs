using UnityEngine.Playables;
using DarkTonic.MasterAudio;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.Timeline;
using Return.Audios;

/// <summary>
/// Create audio list instance and bind to audio controller
/// </summary>
public class MasterAudioPlayableBehaviour : PlayableBehaviour,IPlayable
{
    public MasterAudioPlayableClip AudioTrackAsset;

    public DynamicSoundGroupCreator AudioPreset;

    public Playable m_Playable;

    public PlayableHandle GetHandle()
    {
        return m_Playable.GetHandle();
    }

//#if UNITY_EDITOR
    public AudioSource AudioSource;
//#endif

    AudioPlayableOutput Output;


    /// <summary>
    /// Link audio playable to AudioManager port 
    /// </summary>
    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);

        m_Playable = playable;

        playable.SetTime(0);
        Debug.Log(" Behaviour "+playable.GetPlayState());

        playable.SetTraversalMode(PlayableTraversalMode.Passthrough);
        Debug.Log(playable.GetOutputCount());
        if (!playable.GetOutput(0).IsNull())
            playable.GetOutput(0).SetTraversalMode(PlayableTraversalMode.Passthrough);
        else
            Debug.Log("Null output handle :"+ playable.GetOutput(0).GetHandle());

        Debug.Log("OnPlayableCreate " + playable.GetPlayableType());
        var graph = playable.GetGraph();

#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            AudioSource = new GameObject("TimelineAudio").AddComponent<AudioSource>();

            var output = (AudioPlayableOutput)playable.GetGraph().GetOutputByType<AudioPlayableOutput>(0);



            if (output.IsOutputNull())
                output = AudioPlayableOutput.Create(graph, nameof(MasterAudioPlayableBehaviour),AudioSource);
            else
                output.SetTarget(AudioSource);

            return;
        }
#endif
  

        if (AudioManager.UseTemplate(AudioTrackAsset.AudioConfig.AudioTemplate.Resolve(graph.GetResolver()),out var audiosource))
        {
            AudioSource = audiosource;
            if (Output.IsOutputNull())
            {
                Output = AudioPlayableOutput.Create(
                    graph,
                    AudioTrackAsset.AudioConfig.AudioTemplate.Resolve(graph.GetResolver()).Title,
                    audiosource
                    );
            }

            Output.SetSourcePlayable(playable);
        }

        return;
        AudioPreset = GameObject.Instantiate(AudioPreset);

#if UNITY_EDITOR

#endif
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);
        Debug.Log(playable.GetTime());
    }

    double time;

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        if (time > playable.GetTime())
        {
            AudioSource.PlayScheduled(time);
            AudioSource=AudioManager.RentSource();
        }


    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        playable.SetSpeed(info.effectiveParentSpeed);

        Debug.Log(playable.GetTraversalMode());

        Debug.Log(info.output);
        Assert.IsTrue(!info.output.IsOutputNull() && info.output.IsOutputValid());

        return;
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
        {
            //Assert.IsTrue(MasterAudio.PlaySoundAndForget(AudioTrackAsset.AudioConfig.AudioList));
            AudioSource = AudioManager.PlayOnceAt(AudioTrackAsset.AudioConfig.AudioClip,Vector3.zero,true);
            AudioSource.volume = AudioTrackAsset.AudioConfig.Volume;
        }
        else
        {
            Assert.IsNotNull(AudioTrackAsset);
            Assert.IsNotNull(AudioTrackAsset.AudioConfig);

            Assert.IsNotNull(AudioPreset);
            Assert.IsNotNull(AudioPreset.musicPlaylists);

            var clip = AudioTrackAsset.AudioConfig.AudioClip;
            AudioSource.PlayOneShot(clip,AudioTrackAsset.AudioConfig.Volume);

            //var clip = AudioPreset.musicPlaylists.Where(x => x.playlistName == AudioTrackAsset.AudioConfig.AudioList).FirstOrDefault().
            //    MusicSettings.Where(x => x.songName == AudioTrackAsset.AudioConfig.ClipID).FirstOrDefault().clip;
            //AudioSource?.PlayOneShot(clip);
        }
#else
        MasterAudio.PlaySoundAndForget(AudioTrackAsset.AudioConfig.AudioList);
#endif
        //playable.GetSpeed();
    }

    public override void OnGraphStop(Playable playable)
    {

        if (AudioSource)
            AudioSource.Stop();

        base.OnGraphStop(playable);
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        Debug.Log(nameof(OnPlayableDestroy) + " ** " + AudioTrackAsset.name);

        if (!AudioSource)
            return;

#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            if(AudioSource.isActiveAndEnabled)
            GameObject.DestroyImmediate(AudioSource.gameObject);
        }
        else
#endif
        {
            AudioManager.ReturnTemplateSource(AudioSource);
        }

        base.OnPlayableDestroy(playable);
    }

    public override void OnGraphStart(Playable playable)
    {
        base.OnGraphStart(playable);
        Debug.Log(nameof(OnGraphStart));
    }


}
