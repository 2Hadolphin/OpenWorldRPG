#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
using System;
using UnityEditor;
using Return.Editors;

namespace Return.Editors
{
    [ExecuteInEditMode]
    public class mPreview : WrapWindow
    {
        protected override void Awake()
        {
            base.Awake();
            Clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Return/Humanoid/MotionMatching/ClipsSources/Movement/Walking/Loop/RootTransform/CLazy@Mvm_Walk_Root.asset");
        }
        public AnimationClip Clip;

        PlayableOutput output;
        Playable playable;
        float min = 0;
        float max = 1;
        [OnValueChanged(nameof(updateSpeed))]
        [PropertyRange(nameof(min), nameof(max))]
        public float Speed = 1f;

        bool Playing => AnimationEditorGraph.IsValid() && AnimationEditorGraph.IsPlaying();

        [HideIf(nameof(Playing))]
        [Button(nameof(StartPreview))]
        public override void StartPreview()
        {
            if (!Clip) return;

            playable = AnimationClipPlayable.Create(AnimationEditorGraph, Clip);
            if (!output.IsOutputValid())
                output = AnimationPlayableOutput.Create(AnimationEditorGraph, nameof(mPreview) + "-Output", mAnimator);

            output.SetSourcePlayable(playable);


            AnimationEditorGraph.Play();
            AnimationEditorGraph.Evaluate();
        }

        [ShowIf(nameof(Playing))]
        [Button(nameof(StopPreview))]
        public override void StopPreview()
        {
            if (AnimationEditorGraph.IsValid() && AnimationEditorGraph.IsPlaying())
                AnimationEditorGraph.Stop();
        }


        bool mPause = false;

        [EnableIf(nameof(Playing))]
        [ShowIf(nameof(mPause))]
        [Button(nameof(Resume))]
        public virtual void Resume()
        {
            if (playable.IsValid())
            {
                playable.Play();
                mPause = false;
            }


        }

        [EnableIf(nameof(Playing))]
        [HideIf(nameof(mPause))]
        [Button(nameof(Pause))]
        public virtual void Pause()
        {
            if (playable.IsValid())
            {
                playable.Pause();
                mPause = true;
            }

        }


        private void OnEnable()
        {
            AnimationEditorGraph = PlayableGraph.Create(gameObject.name + "-" + nameof(mPreview));
        }

        private void Update()
        {
            if (AnimationEditorGraph.IsValid() && AnimationEditorGraph.IsPlaying())
            {
                AnimationEditorGraph.Evaluate();
            }

        }

        void updateSpeed()
        {
            if (playable.IsValid())
                playable.SetSpeed(Speed);
        }

    }
}

#endif