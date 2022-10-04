using UnityEngine.Playables;
//using Return.Audios;
using Return;

namespace UnityEngine.Timeline
{
    public class MasterAudioIndependentBehaviour : IPlayableBehaviour
    {
        public double Duration;

        public MasterAudioIndependentClip Data;

        // get source when start or loop

        public void OnBehaviourPlay(Playable playable, FrameData info)
        {
# if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                Debug.Log(playable.GetTime());
                Debug.Log(playable.GetPlayState());

                if(Duration!=0)
                playable.SetDuration(Duration);
                playable.Play();
                Debug.Log(playable.GetDuration() == Duration);
            }
            else
#endif
            {
                //AudioManager.PlayOnceAt(Data.Clip,);
                
            }

        }

        public void OnBehaviourPause(Playable playable, FrameData info)
        {
            //Debug.Log(playable.GetTime());
            //playable.SetTime(0);

            if (playable.GetTime() < playable.GetInput(0).GetDuration())
                Debug.LogError(playable.GetTime() +" ** "+ playable.GetInput(0).GetDuration());
        }



        public void OnGraphStart(Playable playable)
        {
            
            playable.Play();
        }

        public void OnGraphStop(Playable playable)
        {

        }

        public void OnPlayableCreate(Playable playable)
        {
            Debug.Log(playable.GetDuration());
            if(Duration!=0)
            playable.SetDuration(Duration);
            //playable.DisconnectOutput();

            playable.SetTraversalMode(PlayableTraversalMode.Passthrough);
            playable.SetPropagateSetTime(false);
            // asign audio source
            //var output = ScriptPlayableOutput.Create(playable.GetGraph(),this.ToString());

            //output.SetSourcePlayable(playable,0);

        }

        public void OnPlayableDestroy(Playable playable)
        {

        }

        public void PrepareFrame(Playable playable, FrameData info)
        {
            //Debug.Log("Prepare : "/*+playable.GetTime()*/);
        }

        public void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            Debug.Log("Process : " /*+ playable.GetTime()*/);
        }
    }

}

