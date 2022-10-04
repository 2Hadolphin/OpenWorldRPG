#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using Sirenix.OdinInspector;

namespace Return.Editors
{

    public class EditorRiggingPlayer : EditorAnimPlayer
    {

        [BoxGroup("IK", ShowLabel = false)]
        RigBuilder builder;
        [BoxGroup("IK")]
        public List<TwoBoneIKConstraint> Rigs;

        public override void Init()
        {
            base.Init();

            builder = GetComponent<RigBuilder>();

            //PlayAnimator();
            EnableRig();

            Rigs = new List<TwoBoneIKConstraint>(transform.GetComponentsInChildren<TwoBoneIKConstraint>());
            foreach (var rig in Rigs)
            {
                builder.AddEffector(rig.data.target, new RigEffectorData.Style() { color = Color.red, size = 0.3f });
                builder.AddEffector(rig.data.hint, new RigEffectorData.Style() { color = Color.blue, size = 0.3f });
            }
        }


        public override void Play()
        {
            if (builder.graph.IsValid() && builder.graph.IsPlaying())
                mGraph = builder.graph;

            base.Play();
        }


        [PropertyOrder(8f)]
        [Button(nameof(EnableRig))]
        void EnableRig()
        {
            builder.Clear();
            builder.Build();
            builder.StartPreview();
        }

        [PropertyOrder(8f)]
        [Button(nameof(DisableRig))]
        void DisableRig()
        {
            builder.StopPreview();
            builder.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DisableRig();
        }

    }


}

#endif