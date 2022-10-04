using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Linq;
using System;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Return.Humanoid.Animation;

using Return;

#if UNITY_EDITOR
using UnityEditor;

using UnityEditor.Animations;
#endif
[ExecuteInEditMode]
public class FreeFormBlendNode_EditorTool_PolarGradient:FreeFormBlendNode
{
    public Material mat;
    public Animator Animator;
    public float[] GetMotionWeights(Vector3 velocity)
    {
        return Interpolate(new float[] { velocity.x, velocity.y, velocity.z },true);
    }

    public void Init()
    {
        var length = Nodes.Length;
        samples = new float[length][];
        for (int i = 0; i < length; i++)
        {
            samples[i] = new float[] { Nodes[i].x, 0, Nodes[i].z };
        }
        WeightResult = new float[length];
    }
    private void Update()
    {
        StartJob();
    }

    public override event Action<float[]> WeightPost;
    public override void StartJob()
    {
        var result = GetMotionWeights(InputParameter.Y2Z());
        WeightResult = result;
        WeightPost?.Invoke(WeightResult);
        return;
        var length = result.Length;
        for (int i = 0; i < length; i++)
        {
            WeightResult[i] = result[i];
        }
        WeightPost?.Invoke(WeightResult);
    }

    public Playable[] Init(AnimationLibrary_Vector3 library, Animator animator, PlayableGraph mGraph)
    {
        Animator = animator;

        #region LoadingAnimationLibrary
        _library = library;
        Nodes = new NativeArray<Vector3>(_library.GetParameters, Allocator.Persistent);
        Weight = new NativeArray<float>(Nodes.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        WeightResult = new float[Weight.Length];


        if (library.HasMirrorClip)
        {
            #region BoneMap
            var handles = HumanoidAnimationUtility.GetStreamBones(animator, HumanBodyBonesExtension.GetHumanoidBones(animator, HumanBodyBonesUtility.MirrorBones));
            MirrorBones = new NativeArray<TransformStreamHandle>(handles, Allocator.Persistent);

            var handlePairs = HumanoidAnimationUtility.GetStreamBones(animator, HumanBodyBonesExtension.GetHumanoidBones(animator, HumanBodyBonesUtility.MirrorBonePairs, true));
            MirrorBonePair = new NativeArray<Job_Humanoid_Mirror.HandlePair>(Job_Humanoid_Mirror.HandlePair.Pairs(handlePairs), Allocator.Persistent);
            #endregion
        }

        var clipsData = library.Collections.ToArray();
        var length = clipsData.Length;
        var playables = new List<Playable>(length);
        var clipLength = new List<double>(length);

        for (int i = 0; i < length; i++)
        {
            if (!clipsData[i].Clip)
                throw new KeyNotFoundException();

            var clip = clipsData[i].Clip;
            if (clipsData[i].Mirror)
            {
                /*
            var clipMirrorPlayable = Job_Humanoid_Mirror.Create(mGraph, clipsData[i].Clip);
            var mirrorJob = clipMirrorPlayable.GetJobData<Job_Humanoid_Mirror>();
            mirrorJob.MirrorBones = MirrorBones;
            mirrorJob.MirrorPairBones = MirrorBonePair;
            clipMirrorPlayable.SetJobData(mirrorJob);
            playables.Add(clipMirrorPlayable);

            var clipPlayable = AnimationClipPlayable.Create(mGraph, clipsData[i].Clip);
            playables.Add(clipPlayable);

            var clipMirrorPlayable = Job_Humanoid_Mirror.Create(mGraph, clipsData[i].Clip);
            var mirrorJob = clipMirrorPlayable.GetJobData<Job_Humanoid_Mirror>();
            mirrorJob.MirrorBones = MirrorBones;
            mirrorJob.MirrorPairBones = MirrorBonePair;
            clipMirrorPlayable.SetJobData(mirrorJob);
            playables.Add(clipMirrorPlayable);
                */
                var jobData = new MirrorPoseJob();
                jobData.AnimatorRoot = animator.BindStreamTransform(animator.transform);
                var mirrorJob = AnimationScriptPlayable.Create(mGraph, jobData);
                var mirrorClipPlayable= AnimationClipPlayable.Create(mGraph, clip);
                mirrorClipPlayable.SetTime(clip.length / 2f);
                mirrorJob.AddInput(mirrorClipPlayable, 0,1);
           
                playables.Add(mirrorJob);
            }
            else
            {
                var clipPlayable = AnimationClipPlayable.Create(mGraph, clip);
                playables.Add(clipPlayable);
            }

            clipLength.Add(clip.length);
        }


        var max = clipLength.Max();

        var parameter = 1 / max;
        var normalizeSpeed = clipLength.Select(x => max / x * parameter).ToArray();

        length = playables.Count;
        for (int i = 0; i < length; i++)
        {
            playables[i].SetSpeed(1 / normalizeSpeed[i]);
        }
        // speed*length**speedup=maxLength*1

        #endregion

        Init();

        return playables.ToArray();
    }

#if UNITY_EDITOR
    protected void OnRenderObject()
    {
        if (!Animator)
            return;
        mat.SetPass(0);

        var tf = Animator.transform;
        var hipPos = Animator.GetBoneTransform(HumanBodyBones.Hips).position;
        var matrix = MathfUtility.CreateMatrix(tf.right, tf.forward, tf.up, hipPos);

        Camera cam = UnityEditor.SceneView.lastActiveSceneView.camera;
        float size = (cam.transform.position - tf.TransformPoint(hipPos)).magnitude / 2;
        size = 1f;

        DrawArea graph = new DrawArea3D(new Vector3(-size, -size, 0), new Vector3(size, size, 0), matrix);

        GL.Begin(GL.QUADS);
        graph.DrawRect(new Vector3(0, 0, 0), new Vector3(1, 1, 0), new Color(0, 0, 0, 0.2f));
        GL.End();

        Color weakColor = new Color(0.7f, 0.7f, 0.7f, 1);

        float range = 0;
        var length = Nodes.Length;
        for (int i = 0; i < length; i++)
        {
            var node = Nodes[i];
            range = Mathf.Max(range, Mathf.Abs(node[0]));
            range = Mathf.Max(range, Mathf.Abs(node[2]));
        }
        if (range == 0) range = 1;
        else range *= 1.2f;

        GL.Begin(GL.LINES);
        graph.DrawLine(new Vector3(0.5f, 0, 0), new Vector3(0.5f, 1, 0), weakColor);
        graph.DrawLine(new Vector3(0, 0.5f, 0), new Vector3(1, 0.5f, 0), weakColor);
        graph.DrawLine(new Vector3(0, 0, 0), new Vector3(1, 0, 0), weakColor);
        graph.DrawLine(new Vector3(1, 0, 0), new Vector3(1, 1, 0), weakColor);
        graph.DrawLine(new Vector3(1, 1, 0), new Vector3(0, 1, 0), weakColor);
        graph.DrawLine(new Vector3(0, 1, 0), new Vector3(0, 0, 0), weakColor);
        GL.End();



        float mX, mY;
        for (int g = 0; g < length; g++)
        {
            Vector3 colorVect = Quaternion.AngleAxis((g + 0.5f) * 360.0f / length, Vector3.one) * Vector3.right;
            Color color = new Color(colorVect.x, colorVect.y, colorVect.z);

            // Draw weights
            GL.Begin(GL.QUADS);
            Color colorTemp = color * 0.4f;
            colorTemp.a = 0.8f;

            for (int i = 0; i < length; i++)
            {
                var m = Nodes[i];
                mX = (m[0]) / range / 2 + 0.5f;
                mY = (m[2]) / range / 2 + 0.5f;
                float s = 0.02f;
                graph.DrawDiamond(
                    new Vector3(mX - s, mY - s, 0),
                    new Vector3(mX + s, mY + s, 0),
                    colorTemp
                );
            }
            GL.End();


            if (WeightResult[g] == 0) continue;


            GL.Begin(GL.QUADS);
            color.a = WeightResult[g];

            for (int i = 0; i < length; i++)
            {
                var m = Nodes[i];
                mX = (m[0]) / range / 2 + 0.5f;
                mY = (m[2]) / range / 2 + 0.5f;
                float s = Mathf.Pow(WeightResult[i], 0.5f) * 0.05f;
                graph.DrawRect(
                    new Vector3(mX - s, mY - s, 0),
                    new Vector3(mX + s, mY + s, 0),
                    color
                );
            }
            GL.End();
        }


        GL.Begin(GL.QUADS);
        // Draw marker
        mX = (InputParameter.x) / range / 2 + 0.5f;
        mY = (InputParameter.y) / range / 2 + 0.5f;
        float t = 0.02f;
        graph.DrawRect(new Vector3(mX - t, mY - t, 0), new Vector3(mX + t, mY + t, 0), new Color(0, 0, 0, 1));
        t /= 2;
        graph.DrawRect(new Vector3(mX - t, mY - t, 0), new Vector3(mX + t, mY + t, 0), new Color(1, 1, 1, 1));
        GL.End();
    }

#endif
}