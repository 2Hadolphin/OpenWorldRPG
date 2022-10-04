#if UNITY_EDITOR
using Return;
using Return.Humanoid;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEditor;


namespace Return.Items.Weapons
{
    partial class FirearmsPerformerPlayer   // EditorDev
    {
        public bool EditorForceCreateGraph;

        [Button]
        void Pause()
        {
            if (TimelineMixer.IsQualify())
            {
                var bhv = TimelineMixer.GetBehaviour();

                var graph = TimelineMixer.GetGraph();

                if (graph.IsPlaying())
                    graph.Stop();
                else
                    graph.Play();


                //if (TimelineMixer.GetPlayState().HasFlag(PlayState.Playing))
                //    TimelineMixer.Pause();
                //else
                //    TimelineMixer.Play();
            }
        }

        //[Button]
        void TestHandle(int level)
        {
            Playable p = TimelineMixer;

            for (int i = 0; i < level; i++)
            {
                p = p.GetInput(0);
                Debug.Log(p.GetHandle() + " ** " + p.GetPlayableType() + p.IsPlayableOfType<AnimationScriptPlayable>());
            }
        }

        /// <summary>
        /// Develop phase. create custom graph and ik
        /// </summary>
        [Button, PropertyOrder(10)]
        public void EditorLoadPerformerHandles(IItem item)
        {
            var IAnimator = GetComponentInParent<IAnimator>();

            if (IAnimator.NotNull())
                InitIK(IAnimator, GetComponent<TNet.TNObject>());

            if (!TimelineMixer.IsNull() && TimelineMixer.IsValid())
            {
                //CleanGraph();
                TimelineMixer.DestoryUpward(false);
                Debug.LogError("Reload performed handle");
            }

            if (EditorForceCreateGraph || !UnityEditor.EditorApplication.isPlaying)
            {
                var player = GetComponentInParent<Animator>().gameObject.InstanceIfNull<Return.Editors.EditorAnimPlayer>();
                var graph = player.GetGraph;

                InitMixer(graph);

                Debug.Log(GetGraph);
                //if (TryGetComponent<IModularItemHandler>(out var moduleHandle))
                //{
                //    var handles = GetPlayableHandles<IFirearmsPersistentPerformerHandle>(moduleHandle).ToArray();
                //    LoadPerformerHandles(graph,handles);
                //}
                //else
                //    Debug.LogError("Missing " + moduleHandle);

                if (!AutoOutput)
                {
                    //var t = TimelineMixer.GetInput(0).GetInput(0).GetInput(0);
                    //var count = t.GetInputCount() - 1;
                    //count = count < 0 ? 0 : count;
                    //t.SetInputWeight(count, 1);
                    //Debug.Log(t.GetPlayableType());
                    var output = AnimationPlayableOutput.Create(graph, name, IAnimator.Animator);
                    var bhv = TimelineMixer.GetBehaviour();
                    bhv.EditorBindOutput(output);

                    LoadCustomOutput(graph);

                    //bhv.BlendToState();
                }

                //Debug.Log(graph.GetRootPlayable(0));
                graph.Evaluate();
                graph.Play();

            }
        }
        [Tooltip("Auto create playable output.")]
        public bool AutoOutput = true;

        [Button]
        void ResetTime()
        {
            //var player = transform.GetRoot().CharacterRoot.InstanceIfNull<Return.Editors.EditorAnimPlayer>();
            //var graph = player.GetGraph;
            //var root = graph.GetRootPlayable(0);
            //Debug.Log(root.GetPlayableType());

            //root.SetTime(0);

            bhv.SetTime();

            //TimelineMixer.SetTime(0);
        }

        [Button]
        void CleanGraph()
        {
            var player = transform.GetRoot().gameObject.InstanceIfNull<Return.Editors.EditorAnimPlayer>();
            var graph = player.GetGraph;
            var root = graph.GetRootPlayable(0);
            Debug.Log(root.GetPlayableType());
            graph.DestroyPlayable(root);
            graph.Stop();
            graph.Destroy();
            cachePerformers.Clear();
        }

        [ShowInInspector]
        [OnInspectorGUI]
        void DrawAnim()
        {
            if (!TimelineMixer.IsQualify())
                return;

            var states = cachePerformers.GetEnumerator();

            EditorGUILayout.BeginVertical();

            var disable = bhv.DuringTransit;

            EditorGUI.BeginDisabledGroup(disable);

            while (states.MoveNext())
            {
                if (GUILayout.Button(states.Current.Key.name))
                    bhv.BlendToState(states.Current.Value);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }
    }
}

#endif

