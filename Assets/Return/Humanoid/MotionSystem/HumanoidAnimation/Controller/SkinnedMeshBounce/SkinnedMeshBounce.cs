using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Sirenix.OdinInspector;

using System.Linq;
using System;

//#if UNITY_EDITOR
//using UnityEditor;

//using UnityEngine.ProBuilder;
//using UnityEditor.ProBuilder;

//public class SkinnedMeshBounce : MonoBehaviour
//{

//    public float Radius=0.07f;
//    public ReadOnlyTransform[] BonesCache;
//    public ReadOnlyTransform MeshRoot;

//    public int PointNums;
//    public Vector3[] Result;

//    public SkinnedMeshRenderer skinnedMesh;

//    [BoxGroup("Edtior")][HideInInspector]
//    [HideLabel]
//    public bool HasInitialize = false;
//    [HorizontalGroup("Edtior/aa")]
//    [LabelWidth(50)]
//    [HideLabel]
//    public Vector3 GUIDs;
//    [HorizontalGroup("Edtior/aa")]
//    [LabelWidth(50)]
//    [HideLabel]
//    public float Distance;
//    [HorizontalGroup("Edtior/Preset")][LabelWidth(100)]
//    public int SampleNumbers;

//    [HorizontalGroup("Edtior/Preset")][Button("AddGroup",ButtonSizes.Small)]
//    void AddGroup()
//    {
//        var group = new SampleGroup();
//        group.Center = GUIDs;
//        var points=mMathf.Circle(Vector2.zero, Distance, SampleNumbers, true);
//        group.Samples = points.Select(width => new Sample() { Normal = width.Y2Z() }).ToArray();

//        SampleGroups.Add(group);
//    }
//    [BoxGroup("Edtior")][PropertyOrder(1)]
//    public List<SampleGroup> SampleGroups = new List<SampleGroup>();

//    [Serializable]
//    public class SampleGroup 
//    {
//        public Vector3 Center;
//        public Sample[] Samples;

//        public Vector3[] AllPoints()
//        {
//            return Samples.Select(width => Center+width.Normal).ToArray();
//        }
//    }
//    [Serializable]
//    public struct Sample
//    {
//        public int[] BoneIndex;
//        public float[] Weights;
//        /// <summary>
//        /// space relative to weight bone
//        /// </summary>
//        public Vector3[] LocalPosition;
//        public Vector3 Normal;

//    }

  
//    public ProBuilderMesh Probuilder;
//    [Button("BuildMesh")]
//    void BuildMesh()
//    {
//        if (Probuilder)
//            DestroyImmediate(Probuilder.CharacterRoot);

//        //var vertexs = new Vector3[] { Vector3.zero, Vector3.right, Vector3.forward };
//        var vertexs = SampleGroups.SelectMany(width => width.AllPoints());
//        var length = vertexs.Count();
//        var func = new Func<int, int>(width => width + 1);

//        var faces = new List<Face>(length);

//        var butCount = SampleGroups.First().AllPoints().Length ;
//        var butVertices = new int[butCount].EditArray(0, func); 
//        faces.AddRange(mMathf.SubdividedTriangle(butVertices, true).Select(width => new Face(width)));


//        var capCount = SampleGroups.Last().AllPoints().Length;
//        var capVertices = new int[capCount].EditArray(vertexs.Count() - capCount, func);
//        faces.AddRange(mMathf.SubdividedTriangle(capVertices).Select(width => new Face(width)));

//        var groupNums = SampleGroups.Count;

        
//        var i = 0;

//        for (int g = 1; g < groupNums; g++)
//        {
//            var num0 = SampleGroups[g - 1].Samples.Length;
//            var num1 = SampleGroups[g].Samples.Length;

//            if (num0 != num1)
//            {
//                var diff = 0;
//                var top = butCount - 1;
//                var coneFaces = new Face[0];
//                if (num0 > num1)
//                {
//                    top += num0 + num1;
//                    diff = num0 - num1;
//                    coneFaces = mMathf.AutoFace_Cone(top, new int[diff].EditArray(i + num1, func)).Select(width => new Face(width)).ToArray();
//                }
//                else if (num0 < num1)
//                {
//                    top += num0;
//                    diff = num1 - num0;
//                    coneFaces = mMathf.AutoFace_Cone(top, new int[diff].EditArray(i + num0 + num0, func)).Select(width => new Face(width)).ToArray();
//                }

//                faces.AddRange(coneFaces);
//            }

//            var cylinderFaces = mMathf.AutoFace_Cylinder(new int[num0].EditArray(i, func), new int[num1].EditArray(i + num0, func),true);
//            faces.AddRange(cylinderFaces.Select(width => new Face(width)));

//            butCount += num0 + num1;
//        }

//        PointNums = butCount;
//        Result = new Vector3[PointNums];

//        Probuilder = ProBuilderMesh.Create(
//            vertexs,
//             faces
//            );

//        var mesh = Probuilder.GetComponent<MeshFilter>().sharedMesh;


//        var newMesh=EditorHelper.WriteAsset(mesh);
//        DestroyImmediate(Probuilder.CharacterRoot);

//        print(newMesh.Equals(mesh));
//        MeshCollider.sharedMesh = mesh;
//    }

//    [Button("BindData")]
//    void BindBounds()
//    {
//        BonesCache = skinnedMesh.bones;
 
//        var mesh = skinnedMesh.sharedMesh;
//        var vertices = mesh.vertices;
//        var weights = mesh.boneWeights;
//        var num = mesh.vertexCount;
//        var sqrDistance = Radius * Radius;

//        foreach (var group in SampleGroups)
//        {
//            var center = group.Center;
//            var samples = group.Samples;
//            var length = samples.Length;

//            for (int i = 0; i < length; i++)
//            {
//                var point=samples[i].Normal + center;
//                var queue = new Queue<int>();

//                for (int v = 0; v < num; v++)
//                {
//                    var mag = (point - vertices[v]).sqrMagnitude;

//                    if (mag > sqrDistance)
//                        continue;

//                     queue.Enqueue(i);
//                }

//                var score = new Dictionary<int, float>();

//                while (queue.Count > 0)
//                {
//                    var weight = weights[queue.Dequeue()];

//                    if (score.ContainsKey(weight.boneIndex0))
//                        score[weight.boneIndex0] += weight.weight0;
//                    else
//                        score.Add(weight.boneIndex0, weight.weight0);

//                    if (score.ContainsKey(weight.boneIndex1))
//                        score[weight.boneIndex1] += weight.weight1;
//                    else
//                        score.Add(weight.boneIndex1, weight.weight2);

//                    if (score.ContainsKey(weight.boneIndex2))
//                        score[weight.boneIndex2] += weight.weight2;
//                    else
//                        score.Add(weight.boneIndex2, weight.weight2);

//                    if (score.ContainsKey(weight.boneIndex3))
//                        score[weight.boneIndex3] += weight.weight3;
//                    else
//                        score.Add(weight.boneIndex3, weight.weight3);
//                }

//                var enableResult = new List<KeyValuePair<int, float>>(4);

//                for (int b = 0; b < 4; b++)
//                {
//                    if (score.Count <= 0)
//                        break;

//                    print(score.Count);
//                    var results = score.Aggregate((l, r) => l.m_Value > r.m_Value ? l : r);
//                    enableResult.Add(results);
//                    score.Remove(results.Key);
//                }


//                samples[i].BoneIndex = enableResult.Select(width => width.Key).ToArray();

//                var totalWeight = enableResult.Sum(width => width.m_Value);
//                samples[i].Weights = enableResult.Select(width => width.m_Value / totalWeight).ToArray();

//                var worldPoint = MeshRoot.TransformPoint(point);
//                samples[i].LocalPosition = enableResult.Select(width => BonesCache[width.Key].InverseTransformPoint(worldPoint)).ToArray();


//            }
//            group.Samples = samples;
//        }

//        HasInitialize = true;
//    }


//    public MeshCollider MeshCollider;

//    void FixedTick()
//    {

//        //var matrix = skinnedMesh.localToWorldMatrix;
//        var length = SampleGroups.Count;
//        var vertexSN=0;
//        for (int i = 0; i < length; i++)
//        {
//            var group = SampleGroups[i];
//            var center = group.Center;
//            foreach (var sample in group.Samples)
//            {
//                var pos = Vector3.zero;
//                var nums = sample.BoneIndex.Length;
//                for (int k = 0; k < nums; k++)
//                {
//                    if (sample.BoneIndex[k] == 0)
//                        continue;
//                    pos += BonesCache[sample.BoneIndex[k]].TransformPoint(sample.LocalPosition[k]).Multiply(sample.Weights[k]);
//                }
//                Result[vertexSN] = MeshRoot.InverseTransformPoint(pos);
//                vertexSN++;
//            }
//        }

 
//        var mesh=MeshCollider.sharedMesh;
//        mesh.vertices = Result;
//        MeshCollider.sharedMesh = mesh;
//    }


//    public struct WeightJob
//    {

//    }

//    private void OnDrawGizmos()
//    {
//        if (EditorApplication.isPlaying)
//        {
//            Gizmos.color = Color.red;
//            foreach (var position in Result)
//            {
//                Gizmos.DrawSphere(position, Radius);
//            }
//        }
//        else
//        {
//            Gizmos.color = Color.white;
//            var root = MeshRoot.position;
//            foreach (var sampleGroup in SampleGroups)
//            {
//                var pos = sampleGroup.Center;
//                foreach (var sample in sampleGroup.Samples)
//                {
//                    var point = MeshRoot.rotation * (MeshRoot.TransformVector(pos + sample.Normal) + root);
//                    Gizmos.DrawSphere(point, Radius);
//                }
//            }
//        }
//    }

//}
//#endif