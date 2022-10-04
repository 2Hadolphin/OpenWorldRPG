using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Assertions;

namespace Return.Editors
{
    [RequireComponent(typeof(Animator))]
    [ExecuteInEditMode]
    public class EditorAnimPlayer : MonoBehaviour
    {
        public Animator animator;

        public AvatarMask AvatarMask;

        protected PlayableGraph mGraph;

        public PlayableGraph GetGraph
        {
            get
            {
                if (!mGraph.IsValid())
                    mGraph = PlayableGraph.Create(nameof(EditorAnimPlayer));

                return mGraph;
            }
        }

        public AnimationPlayableOutput Output;

        #region PlayConfig



        public bool ResetPoseAtEnd;

        bool _PlayControllerGraph;

        [ShowInInspector]
        public virtual bool PlayControllerGraph
        {
            get => _PlayControllerGraph;
            set
            {
                if (!_PlayControllerGraph.SetAs(value))
                    return;

                CleanGraph();

                if (value)
                    mGraph = animator.playableGraph;
            }
        }

        [PropertyOrder(8f)]
        [Button(nameof(TPose))]
        void TPose()
        {
            animator.ForceTPose();
        }

        [Button(nameof(LastClip))]
        [PropertyOrder(5f)]
        [HorizontalGroup("Player/packageOption")]
        void LastClip()
        {
            mClip = Clips.Last(ref sn);
            Play();
        }
        [PropertyOrder(5f)]
        [HorizontalGroup("Player/packageOption", MinWidth = 0.2f)]
        [HideLabel]
        public int sn = -1;

        [PropertyOrder(5f)]
        [HorizontalGroup("Player/packageOption", MinWidth = 0.5f)]
        [HideLabel]
        [ReadOnly]
        public AnimationClip mClip;


        [PropertyOrder(5f)]
        [Button(nameof(NextClip))]
        [HorizontalGroup("Player/packageOption")]
        void NextClip()
        {
            mClip = Clips.Next(ref sn);
            Play();
        }

        [HideIf(nameof(_PlayControllerGraph))]
        [ListDrawerSettings(Expanded = true)]
        public List<AnimationClip> Clips = new();

        #endregion

        public Playable Playable;

        #region State

        public bool isPlaying => mGraph.IsValid() && mGraph.IsPlaying();
        public void SetTime(float t)
        {
            var during = Playable.GetDuration();
            Playable.SetTime(during * t);
        }

        public bool isPause
        {
            get => Playable.IsValid() ? Playable.GetPlayState() == PlayState.Paused:false;
            set
            {
                if (Playable.IsValid())
                {
                    if (value)
                        Playable.Pause();
                    else
                        Playable.Play();
                }
            }
        }

        public void Insert(Playable playable)
        {
            if(!Output.IsOutputNull()&&Output.IsOutputValid())
                Output.Insert(playable);
        }

        #endregion

        private void Awake()
        {
            Init();
        }


        public virtual void Init()
        {
            animator = GetComponent<Animator>();
            hideFlags = HideFlags.DontSave;
            Assert.IsTrue(GetGraph.IsValid());
        }

        [PropertyOrder(4f)]
        [VerticalGroup("Player")]
        [Button(nameof(Play))]
        public virtual void Play()
        {
            CleanGraph();
            PlayClip();
        }

        protected virtual void PlayClip()
        {
            if (PlayControllerGraph || sn < 0)
            {
                if (animator.playableGraph.IsValid())
                    animator.playableGraph.Play();
                else
                {

                    Debug.LogError(animator.playableGraph + " is not valid.");
                }
            }
            else
            {
                mClip = Clips[sn];
                if (!mClip)
                    return;

                Playable = PlayClip(mClip);
                Playable.SetDuration(mClip.length);
                SetOutput(Playable);

                mGraph.Play();
            }
        }

        public virtual void Play(AnimationClip clip)
        {

            Playable = PlayClip(clip);
            Playable.SetDuration(clip.length);
            SetOutput(Playable);

            mGraph.Play();
        }

        protected virtual void SetOutput(Playable playable)
        {
            if (Output.IsOutputNull())
            {
                Output = (AnimationPlayableOutput)mGraph.GetOutputByType<AnimationPlayableOutput>(0);
                if (Output.IsOutputNull())
                    Output = AnimationPlayableOutput.Create(mGraph, nameof(EditorAnimPlayer), animator);
            }

            Output.SetSourcePlayable(playable);
        }


        protected virtual Playable PlayClip(AnimationClip clip)
        {
            AnimationClipPlayable playable;

            if (AvatarMask)
            {
                AnimationLayerMixerPlayable mixer;
                if (mGraph.IsValid())
                    mixer = AnimationLayerMixerPlayable.Create(mGraph, 1);
                else
                    mixer = AnimationPlayableUtilities.PlayLayerMixer(animator, 1, out mGraph);

                playable = AnimationClipPlayable.Create(mGraph, clip);
                mixer.ConnectInput(0, playable, 0);
                mixer.SetLayerMaskFromAvatarMask(0, AvatarMask);

                return mixer;
            }
            else
            {
                if (mGraph.IsValid())
                {
                    return AnimationClipPlayable.Create(mGraph, clip);
                }
                else
                    return AnimationPlayableUtilities.PlayClip(animator, clip, out mGraph);
            }
        }

        [PropertyOrder(8f)]
        [VerticalGroup("Player")]
        [Button(nameof(Stop))]
        void Stop()
        {
            if (mGraph.IsValid() && mGraph.IsPlaying())
                mGraph.Stop();

            CleanGraph();
        }

        protected virtual void CleanGraph()
        {
            if (animator.playableGraph.Equals(mGraph) && mGraph.IsValid())
                mGraph.Stop();
            else if (mGraph.IsValid())
                mGraph.Destroy();
        }

        [PropertyOrder(8f)]
        [Button("Destory")]
        void Destroy()
        {
            DestroyImmediate(this);
        }
        protected virtual void OnDestroy()
        {
            CleanGraph();
        }
    }


}