using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
using UnityEngine.Animations;


namespace TestTransform
{
    public class PositionTrace : MonoBehaviour
    {
        [SerializeField]
        Animator Animator;

        PlayableGraph graph;

        AnimationPlayableOutput output;

        Playable Playable;

        [SerializeField]
        AnimationClip clip;

        [Button]
        void Play()
        {
            transform.SetParent(Animator.transform);

            if (!graph.IsValid())
                graph = PlayableGraph.Create("Test Graph");

            if(output.IsOutputNull() || !output.IsOutputValid())
            {
                output = AnimationPlayableOutput.Create(graph, "AnimPort", Animator);
            }

            if (!Playable.IsQualify())
                Playable = AnimationClipPlayable.Create(graph, clip);


            output.SetSourcePlayable(Playable, 0);

            graph.Play();
            Playable.Play();
        }

        [Button]
        void Stop()
        {
            if (graph.IsValid())
                graph.Destroy();

            Debug.Log(nameof(Stop));



        }

        [Button]
        void NullParent()
        {
            //Stop();

            Playable.SafeDestory();

            transform.SetParent(null, true);
        }

        void Log(object sender, Vector3 pos)
        {
            Debug.Log($"Frame Count {sender} : {pos}");
        }

        private void Start()
        {
            //transform.BindSetPositionExecuting(new EventHandler<Vector3>(Log));
        }

        private void Update()
        {
            if (transform.hasChanged)
            {
                Debug.Log($"{Time.frameCount} Tracer {transform.parent} {transform.position}.");

                transform.hasChanged = false;
            }
        }

        private void OnDestroy()
        {
            if (graph.IsValid())
                graph.Destroy();

        }
    }
}
