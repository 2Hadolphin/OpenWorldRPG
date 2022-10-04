using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    [Serializable]
    [TrackClipType(typeof(AnimationPlayableAsset), false)]
    [TrackBindingType(typeof(Animator))]
    [ExcludeFromPreset]
    public class MyAnimationTrack:AnimationTrack
    {
        private int GetDefaultBlendCount()
        {
            return 0;
        }

        Playable CompileTrackPlayable(PlayableGraph graph, AnimationTrack track, GameObject go, IntervalTree<RuntimeElement> tree)
        {
            var markers=track.GetMarkers();

            Debug.Log(go+" ** "+track.GetMarkerCount());

            

            var mixer = AnimationMixerPlayable.Create(graph, track.clips.Length);
            for (int i = 0; i < track.clips.Length; i++)
            {
                var c = track.clips[i];
                var asset = c.asset as PlayableAsset;
                if (asset == null)
                    continue;

                var animationAsset = asset as AnimationPlayableAsset;

                var source = asset.CreatePlayable(graph, go);
                if (source.IsValid())
                {
                    var clip = new RuntimeClip(c, source, mixer);
                    tree.Add(clip);
                    graph.Connect(source, 0, mixer, i);
                    mixer.SetInputWeight(i, 1f);
                }
            }

            if (!track.AnimatesRootTransform())
                return mixer;

            return mixer;
        }

        Playable CreateInfiniteTrackPlayable(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
        {
            if (infiniteClip == null)
                return Playable.Null;

            var mixer = AnimationMixerPlayable.Create(graph, 1);

            // In infinite mode, we always force the loop mode of the clip off because the clip keys are offset in infinite mode
            //  which causes loop to behave different.
            // The inline curve editor never shows loops in infinite mode.
            var playable = AnimationPlayableAsset.CreatePlayable(graph, infiniteClip, infiniteClipOffsetPosition, infiniteClipOffsetEulerAngles, false,AppliedOffsetMode.NoRootTransform, infiniteClipApplyFootIK, AnimationPlayableAsset.LoopMode.Off);
            if (playable.IsValid())
            {
                tree.Add(new InfiniteRuntimeClip(playable));
                graph.Connect(playable, 0, mixer, 0);
                mixer.SetInputWeight(0, 1.0f);
            }

            return mixer;
        }


        internal override Playable CreateMixerPlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
        {
            if (isSubTrack)
                throw new InvalidOperationException("Nested animation tracks should never be asked to create a graph directly");

            List<MyAnimationTrack> flattenTracks = new List<MyAnimationTrack>();
            if (CanCompileClips())
                flattenTracks.Add(this);


            int defaultBlendCount = GetDefaultBlendCount();
            var layerMixer = AnimationLayerMixerPlayable.Create(graph, flattenTracks.Count + defaultBlendCount, false);  
            for (int c = 0; c < flattenTracks.Count; c++)
            {
                int blendIndex = c + defaultBlendCount;
                // if the child is masking the root transform, compile it as if we are non-root mode
  

                var compiledTrackPlayable = flattenTracks[c].inClipMode ?
                    CompileTrackPlayable(graph, flattenTracks[c], go, tree) :
                    flattenTracks[c].CreateInfiniteTrackPlayable(graph, go, tree);

                graph.Connect(compiledTrackPlayable, 0, layerMixer, blendIndex);

                Debug.Log(flattenTracks[c].inClipMode);

                layerMixer.SetInputWeight(blendIndex, flattenTracks[c].inClipMode ? 1 : 0);

                if (flattenTracks[c].applyAvatarMask && flattenTracks[c].avatarMask != null)
                {
                    layerMixer.SetLayerMaskFromAvatarMask((uint)blendIndex, flattenTracks[c].avatarMask);
                }
            }


            // motionX playable not required in scene offset mode, or root transform mode
            Playable mixer = layerMixer;


#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var animator = GetBinding(go != null ? go.GetComponent<PlayableDirector>() : null);
                if (animator != null)
                {
                    GameObject targetGO = animator.gameObject;
                    IAnimationWindowPreview[] previewComponents = targetGO.GetComponents<IAnimationWindowPreview>();

                    //m_HasPreviewComponents = previewComponents.Length > 0;
                    if (hasPreviewComponents)
                    {
                        foreach (var component in previewComponents)
                        {
                            mixer = component.BuildPreviewGraph(graph, mixer);
                        }
                    }
                }
            }
#endif

            return mixer;
        }
    }
}