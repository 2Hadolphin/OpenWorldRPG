#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
using System;

namespace Return.Editors
{
    [RequireComponent(typeof(Animator))]
    public class WrapWindow : MonoBehaviour, IAnimationWindowPreview
    {
        public event Action Start;
        public event Action Stop;
        [ShowInInspector]
        [ReadOnly]
        public virtual bool Override { get; protected set; }
        private AnimationScriptPlayable m_Playable;

        public Job_Humanoid_MuscleRigging job;
        public PlayableGraph AnimationEditorGraph;

        public Func<PlayableGraph, AnimationScriptPlayable> InjectPlayable;
        public IAnimationJob JobData;
        public bool UpdateJob;
        [ReadOnly]
        public Animator mAnimator;
        protected virtual void Awake()
        {
            hideFlags = HideFlags.DontSave;
            mAnimator = GetComponent<Animator>();
        }

        public void SetJob<T>(T jobData) where T : struct, IAnimationJob
        {
            // behaviourPlayable.SetJobData(jobData);
            UpdateJob = true;

            AnimationEditorGraph.Play();
            AnimationEditorGraph.Evaluate();

            Debug.Log(AnimationEditorGraph.IsPlaying());
            //AnimationEditorGraph.Stop();
        }


        public virtual void StartPreview()
        {
            Override = true;
            Start?.Invoke();
        }

        public virtual void StopPreview()
        {
            Override = false;
            Stop?.Invoke();
        }

        public virtual void UpdatePreviewGraph(PlayableGraph graph)
        {
            m_Playable.SetJobData(job);

        }

        public virtual Playable BuildPreviewGraph(PlayableGraph graph, Playable input)
        {
            AnimationEditorGraph = graph;





            Debug.Log(graph.GetEditorName());
            m_Playable = InjectPlayable(graph);

            if (input.IsValid())
                graph.Connect(input, 0, m_Playable, 0);

            m_Playable.SetInputWeight(0, 1);

            return m_Playable;
        }
    }
}


#endif