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

public class FreeFormBlendNode : MonoBehaviour
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> Nodes;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> Weight;

    protected NativeArray<TransformStreamHandle> MirrorBones;
    protected NativeArray<Job_Humanoid_Mirror.HandlePair> MirrorBonePair;
    protected Vector3[] NodesCatch;
    public virtual event Action<float[]> WeightPost;


    public Vector2 InputParameter;
    public AnimationLibrary_Vector3 _library;
    public JobHandle handle;
    public float[] WeightResult = new float[0];


    public Playable[] Init(AnimationLibrary_Vector3 library, Return.Humanoid.Animation.IAdvanceAnimator IAnimator)
    {

        if (!library)
        {
            throw new KeyNotFoundException("animation liabrary missing");
        }




        var animator = IAnimator.GetAnimator;
        var mGraph = IAnimator.GetGraph;

        #region LoadingAnimationLibrary
        _library = library;
        NodesCatch = library.GetParameters;
        Nodes = new NativeArray<Vector3>(NodesCatch, Allocator.Persistent);
        Weight = new NativeArray<float>(Nodes.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        WeightResult = new float[Weight.Length];


        #region PolarGradientInterpolator

        #endregion

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
        var animatorTrensformHandle= animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.Hips).parent);
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
                */
                var jobData = new MirrorPoseJob();
                jobData.AnimatorRoot = animatorTrensformHandle;
                var mirrorJob = AnimationScriptPlayable.Create(mGraph, jobData);
                var mirrorClipPlayable = AnimationClipPlayable.Create(mGraph, clip);
                mirrorClipPlayable.SetTime(clip.length / 2f);
                mirrorJob.AddInput(mirrorClipPlayable, 0, 1);

                playables.Add(mirrorJob);
            }
            else
            {
                var clipPlayable = AnimationClipPlayable.Create(mGraph, clip);

                playables.Add(clipPlayable);
            }

            clipLength.Add(clipsData[i].Clip.length);
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



        return playables.ToArray();
    }

    bool DuringJob = false;



    private void Update()
    {
        if (!DuringJob)
            StartJob();
    }

    private void LateUpdate()
    {
        if (DuringJob)
            EndJob();
    }
    public float staticClamp = 0.01f;

    public virtual void StartJob()
    {
        DuringJob = true;
        var x = InputParameter.x;
        var z = InputParameter.y;

        var input = new Vector3(Mathf.Abs(x)<staticClamp?0:x,0, Mathf.Abs(z) < staticClamp?0: z);

        var check = BasicCheck(input);

        if (check == null)
        {
            var job = new InterpolateJob()
            {
                Input = input,
                Nodes = Nodes,
                Weight = Weight,
            };
            var length = Nodes.Length;
            handle = job.Schedule(length, Math.Min(length, 4));
        }
        else
        {
            var length = Weight.Length;
            for (int i = 0; i < length; i++)
            {
                var value = Mathf.Lerp(Weight[i], check[i], ConstCache.deltaTime);
                Weight[i] = value;
            }
        }
    }
    public virtual void EndJob()
    {
        handle.Complete();

        var weights = Weight.ToArray();
        var length = weights.Length;

        float sumWeight = 0;
        for (int i = 0; i < length; i++)
            sumWeight += weights[i];

        if (sumWeight > 0)
            for (int i = 0; i < length; i++)
                weights[i] /= sumWeight;

        WeightResult = weights;
        WeightPost?.Invoke(WeightResult);
        DuringJob = false;
    }

    public Vector3 GetNodeVector()
    {
        handle.Complete();
        var length = NodesCatch.Length;

        var vector = Vector3.zero;
        for (int i = 0; i < length; i++)
        {
            if (WeightResult[i].Equals(0))
                continue;

            vector += NodesCatch[i] * WeightResult[i];
        }

        return vector;
    }

    public virtual void OnEnable()
    {

    }
    public virtual void OnDestroy()
    {
        handle.Complete();

        if (Nodes.IsCreated)
            Nodes.Dispose();
        if (Weight.IsCreated)
            Weight.Dispose();
        if (MirrorBones.IsCreated)
            MirrorBones.Dispose();
        if (MirrorBonePair.IsCreated)
            MirrorBonePair.Dispose();

    }



    #region PolarGradientBandInterpolator

    public float[][] samples;

    /// <summary>
    /// _checkCache condition whether simple result
    /// </summary>
    public float[] BasicCheck(float[] input)
    {
        if (samples.Length == 1)// only one motion
        {
            return new float[1] { 1 };
        }

        for (int i = 0; i < samples.Length; i++)//match motion
        {
            print(input);
            if (Equals(input, samples[i]))
            {
                float[] weights = new float[samples.Length];
                weights[i] = 1;
                return weights;
            }
        }
        return null;
    }

    public float[] BasicCheck(Vector3 input)
    {
        var length = Nodes.Length;
        if (length == 1)// only one motion
        {
            return new float[1] { 1 };
        }

        for (int i = 0; i < length; i++)//match motion
        {
            if (Equals(input, Nodes[i]))
            {
                float[] weights = new float[length];
                weights[i] = 1;
                return weights;
            }
        }
        return null;
    }

    public struct InterpolateJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction][ReadOnly]
        public NativeArray<Vector3> Nodes;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> Weight;

        public Vector3 Input;
        public void Execute(int index)
        {
            var length = Nodes.Length;
            //? out side range?
            bool outsideHull = false;
            float value = 1;


            for (int t = 0; t < length; t++) // compare with each point 
            {
                if (index == t) continue;// if point-myself point => pass

                Vector3 nodeA = Nodes[index];
                Vector3 nodeB = Nodes[t];


                // in radian
                float angle_Node, angle_Input;

                // project Input on nodes plane
                Vector3 input_project;

                float angleMultiplier = 2;//why
                if (nodeA == Vector3.zero)
                {
                    angle_Node = Vector3.Angle(Input, nodeB) * Mathf.Deg2Rad;
                    angle_Input = 0;
                    input_project = Input;
                    angleMultiplier = 1;
                }
                else if (nodeB == Vector3.zero)
                {
                    angle_Node = Vector3.Angle(Input, nodeA) * Mathf.Deg2Rad;
                    angle_Input = angle_Node;
                    input_project = Input;
                    angleMultiplier = 1;
                }
                else
                {
                    angle_Node = Vector3.Angle(nodeA, nodeB) * Mathf.Deg2Rad;

                    if (angle_Node > 0) // clock wise?
                    {
                        if (Input == Vector3.zero)
                        {
                            angle_Input = angle_Node;
                            input_project = Input;
                        }
                        else
                        {
                            #region flat to 2D
                            Vector3 axis = Vector3.Cross(nodeA, nodeB);
                            input_project = Vector3.ProjectOnPlane(Input, axis); // Input as node space 
                            #endregion

                            angle_Input = Vector3.Angle(nodeA, input_project) * Mathf.Deg2Rad; // get map angle dir 

                            if (angle_Node < Mathf.PI * 0.99f) // node same half dir one PI==180 deg
                            {
                                if (Vector3.Dot(Vector3.Cross(nodeA, input_project), axis) < 0) // Input whether between node
                                {
                                    angle_Input *= -1; // why
                                }
                            }
                        }
                    }
                    else
                    {
                        input_project = Input;
                        angle_Input = 0;
                    }
                }

                float magA = nodeA.magnitude;
                float magB = nodeB.magnitude;
                float magInput = input_project.magnitude;
                float avgMag = (magA + magB) / 2;
                magA /= avgMag;
                magB /= avgMag;
                magInput /= avgMag;

                // unknow
                Vector3 vecIJ = new Vector3(angle_Node * angleMultiplier, magB - magA, 0);
                Vector3 vecIO = new Vector3(angle_Input * angleMultiplier, magInput - magA, 0);
    
                float newValue = 1f - Vector3.Dot(vecIJ, vecIO) / vecIJ.sqrMagnitude;

                if (newValue < 0)   // ??
                {
                    outsideHull = true;
                    break;
                }
                value = Mathf.Min(value, newValue);
            }
            if (!outsideHull) Weight[index] = value;
        }
    }

    public float[] Interpolate(float[] input, bool normalize)
    {
        float[] weights = BasicCheck(input);
        if (weights != null) return weights;

        var length = Nodes.Length;

        StartJob();

        EndJob();

        return WeightResult;

        Vector3 inputV;
        Vector3[] samp = new Vector3[length];
        #region Not need?
        // samp motion data as vector2 or vector3 
        if (input.Length == 2)
        {
            inputV = new Vector3(input[0], input[1], 0);
            for (int i = 0; i < length; i++)
            {
                samp[i] = new Vector3(Nodes[i][0], Nodes[i][1], 0);
            }
        }
        else if (input.Length == 3)
        {
            inputV = new Vector3(input[0], input[1], input[2]);
            for (int i = 0; i < length; i++)
            {
                samp[i] = new Vector3(Nodes[i][0], Nodes[i][1], Nodes[i][2]);
            }
        }
        else return null;
        #endregion

        // start m_duringTransit
        for (int i = 0; i < length; i++)
        {
            //? out side range?
            bool outsideHull = false;
            float value = 1;

            for (int t = 0; t < length; t++) // compare with each point 
            {
                if (i == t) continue;// if point-myself point => pass

                Vector3 nodeA = samp[i];
                Vector3 nodeB = samp[t];


                // in radian
                float angle_Node, angle_Input;

                // project Input on nodes plane
                Vector3 input_project;

                float angleMultiplier = 2;//why
                if (nodeA == Vector3.zero)
                {
                    angle_Node = Vector3.Angle(inputV, nodeB) * Mathf.Deg2Rad;
                    angle_Input = 0;
                    input_project = inputV;
                    angleMultiplier = 1;
                    print(nodeA);
                }
                else if (nodeB == Vector3.zero)
                {
                    angle_Node = Vector3.Angle(inputV, nodeA) * Mathf.Deg2Rad;
                    angle_Input = angle_Node;
                    input_project = inputV;
                    angleMultiplier = 1;
                    print(nodeB);
                }
                else
                {
                    angle_Node = Vector3.Angle(nodeA, nodeB) * Mathf.Deg2Rad;

                    if (angle_Node > 0) // clock wise?
                    {
                        if (inputV == Vector3.zero)
                        {
                            angle_Input = angle_Node;
                            input_project = inputV;
                        }
                        else
                        {
                            #region flat to 2D
                            Vector3 axis = Vector3.Cross(nodeA, nodeB);
                            input_project = Vector3.ProjectOnPlane(inputV, axis); // Input as node space 
                            #endregion

                            angle_Input = Vector3.Angle(nodeA, input_project) * Mathf.Deg2Rad; // get map angle dir 

                            if (angle_Node < Mathf.PI * 0.99f) // node same half dir one PI==180 deg
                            {
                                if (Vector3.Dot(Vector3.Cross(nodeA, input_project), axis) < 0) // Input whether between node
                                {
                                    angle_Input *= -1; // why
                                }
                            }
                        }
                    }
                    else
                    {
                        input_project = inputV;
                        angle_Input = 0;
                    }
                }

                float magA = nodeA.magnitude;
                float magB = nodeB.magnitude;
                float magInput = input_project.magnitude;
                float avgMag = (magA + magB) / 2;
                magA /= avgMag;
                magB /= avgMag;
                magInput /= avgMag;

                // unknow
                Vector3 vecIJ = new Vector3(angle_Node * angleMultiplier, magB - magA, 0);
                Vector3 vecIO = new Vector3(angle_Input * angleMultiplier, magInput - magA, 0);
                var tf = transform;
                var pos = transform.position;
                //Debug.DrawRay(pos, tf.TransformVector(((Vector2)vecIJ).Y2Z()), Color.red) ;
                //Debug.DrawRay(pos, tf.TransformVector(((Vector2)vecIO).Y2Z()), Color.blue);

                float newValue = 1f - Vector3.Dot(vecIJ, vecIO) / vecIJ.sqrMagnitude;

                if (newValue < 0)   // ??
                {
                    outsideHull = true;
                    break;
                }
                value = Mathf.Min(value, newValue);
            }
            if (!outsideHull) weights[i] = value;
        }

        // Normalize weights
        if (normalize)
        {
            float summedWeight = 0;
            for (int i = 0; i < samples.Length; i++)
                summedWeight += weights[i];

            if (summedWeight > 0)
                for (int i = 0; i < samples.Length; i++)
                    weights[i] /= summedWeight;
        }


        return weights;
    }

    #endregion


#if UNITY_EDITOR
    #region Debug

    protected void OnRenderObject()
    {
        var mat = Resources.Load<Material>("Materials/URP_Unlit_Transparent");
        mat.SetColor("Color", new Color(255, 255, 255, 25));
        mat.SetPass(0);

        var Animator = gameObject.GetComponent<Animator>();


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

    #endregion

#endif 
}