using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Assertions;
using System.Linq;

namespace Return
{
    public static partial class MeshExtension
    {
        /// <summary>
        /// Duplicate mesh
        /// </summary>
        public static Mesh Copy(this Mesh mesh)
        {
            var newmesh = new Mesh
            {
                name = mesh.name,

                vertices = mesh.vertices,
                triangles = mesh.triangles,

                uv = mesh.uv,
                normals = mesh.normals,
                tangents = mesh.tangents,

                colors = mesh.colors,
                colors32 = mesh.colors32,

                bounds = mesh.bounds,

                bindposes = mesh.bindposes,
                boneWeights = mesh.boneWeights,
            };

            return newmesh;
        }

        /// <summary>
        /// Set bindposes of mesh.
        /// </summary>
        public static void RebindBones(this Mesh mesh,Transform rendererTransform,params Transform[] bones)
        {
            var bindposes = mesh.bindposes;

            var length = bones.Length;

            if (bindposes.Length!=length)
                throw new InvalidOperationException(string.Format("Bone number {0} doesn't match mesh bindposes count {1}", length, bindposes.Length));


            for (int i = 0; i < length; i++)
                bindposes[i] = bones[i].worldToLocalMatrix * rendererTransform.localToWorldMatrix;

            mesh.bindposes = bindposes;
        }

        /// <summary>
        /// Set bindpose of mesh.
        /// </summary>
        public static void RebindBone(this Mesh mesh,int index,Transform rendererTransform, Transform bone)
        {
            var bindposes = mesh.bindposes;
            bindposes[index] = bone.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
            mesh.bindposes = bindposes;
        }
        
        public static void RebindBone(this Matrix4x4[] bindposes, int index,Transform rendererTransform,Transform bone)
        {
            bindposes[index] = bone.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
        }


        public static void SplitMesh(this Mesh mesh)
        {

        }

        /// <summary>
        /// Split Mesh via bones.
        /// </summary>
        public static void SplitMesh(this SkinnedMeshRenderer renderer,params Transform[] bones)
        {
            var mesh = renderer.sharedMesh;

            var boneMap = bones.ToHashSet();
            var boneCount = renderer.bones.Length;
            var boneIndexs = new Dictionary<int, int>(bones.Length);


            Debug.Log($"Remove {boneCount - bones.Length} bones");

            int length;



            #region bindpose
            var bindposes = mesh.bindposes;
            {
                var originBones = renderer.bones;
                length = originBones.Length;

                Assert.IsTrue(bindposes.Length == length, $"Bonebinding length of skinnedMeshRenderer {bindposes.Length} doesn't match bone length{length}");

                int newIndex = 0;
                var newBindposes = new Queue<Matrix4x4>(length);

                for (int i = 0; i < length; i++)
                {
                    if (boneMap.Contains(originBones[i]))
                    {
                        boneIndexs.Add(i, newIndex);
                        bones[newIndex] = originBones[i];

                        newIndex++;

                        newBindposes.Enqueue(bindposes[i]);
                    }
                }

                //bindposes = newBindposes.ToArray();
                Debug.Log($"bindposes : {newBindposes.Count} bones : {bones.Length}");

                //renderer.bones = bones;
            }
            #endregion




            length = mesh.vertexCount;
            HashSet<int> removeVertices = new(length);
            var invalid = new bool[length];

            #region boneWeight
            var boneweights = mesh.boneWeights;
            {
                Assert.IsTrue(boneweights.Length == length);

                var newBoneWeights = new Queue<BoneWeight>(length);

                for (int i = 0; i < length; i++)
                {
                    var weight = boneweights[i];

                    if (
                        boneIndexs.ContainsKey(weight.boneIndex0) ||
                        boneIndexs.ContainsKey(weight.boneIndex1) ||
                        boneIndexs.ContainsKey(weight.boneIndex2) ||
                        boneIndexs.ContainsKey(weight.boneIndex3))
                    {
                        //if (boneIndexs.TryGetValue(weight.boneIndex0,out var newIndex))
                        //    weight.boneIndex0 = newIndex;

                        //if (boneIndexs.TryGetValue(weight.boneIndex1, out newIndex))
                        //    weight.boneIndex1 = newIndex;

                        //if (boneIndexs.TryGetValue(weight.boneIndex2, out newIndex))
                        //    weight.boneIndex2 = newIndex;

                        //if (boneIndexs.TryGetValue(weight.boneIndex3, out newIndex))
                        //    weight.boneIndex3 = newIndex;


                        newBoneWeights.Enqueue(weight);
                        continue;
                    }

                    removeVertices.Add(i);
                    invalid[i] = true;
                }

                boneweights = newBoneWeights.ToArray();
            }
            #endregion
            // remap old vertice index to new one.
            Dictionary<int, int> verticeMap;

            #region vertice
            var vertices = mesh.vertices;
            {
                Assert.IsTrue(vertices.Length == length);

                length = vertices.Length;

                var nums = length - removeVertices.Count;
                verticeMap = new(nums);
                int index = 0;

                Debug.Log($"Remove {removeVertices.Count}/{length}");

                if (nums > 0)
                {
                    var newVertices = new Queue<Vector3>(nums);

                    for (int i = 0; i < length; i++)
                    {
                        if (invalid[i])
                            continue;

                        verticeMap.Add(i, index++);
                        newVertices.Enqueue(vertices[i]);
                    }

                    vertices = newVertices.ToArray();
                }
            }

            Debug.Log($"cache vertices : {verticeMap.Count}");
            #endregion


            #region triangle
            // filter triangles with removed vertices
            var triangles = mesh.triangles;
            {
                length = triangles.Length;

                var newtriangles = new Queue<int>(length * (boneIndexs.Count / boneCount));

                for (int i = 0; i < length; i += 3)
                {
                    if (invalid[triangles[i]] || invalid[triangles[i+1]] || invalid[triangles[i+2]])
                        continue;

                    Assert.IsTrue(verticeMap.ContainsKey(triangles[i]), $"invalid :{invalid[triangles[i]]} {nameof(verticeMap)} doesn't exist {triangles[i]}/{verticeMap.Count}");
                    Assert.IsTrue(verticeMap.ContainsKey(triangles[i+1]), $"invalid :{invalid[triangles[i+1]]} {nameof(verticeMap)} doesn't exist {triangles[i+1]}/{verticeMap.Count}");
                    Assert.IsTrue(verticeMap.ContainsKey(triangles[i+2]), $"invalid :{invalid[triangles[i+2]]} {nameof(verticeMap)} doesn't exist {triangles[i+2]}/{verticeMap.Count}");

                    newtriangles.Enqueue(verticeMap[triangles[i]]);
                    newtriangles.Enqueue(verticeMap[triangles[i + 1]]);
                    newtriangles.Enqueue(verticeMap[triangles[i + 2]]);
                }

                triangles = newtriangles.ToArray();
            }
            #endregion


            #region normal
            var normals = mesh.normals;
            {
                //Assert.IsTrue(normals.Length == length||normals.Length==0,$"normal count : {normals.Length}");

                length = normals.Length;

                var nums = length - removeVertices.Count;
                if (nums > 0)
                {
                    var newNormals = new Queue<Vector3>(nums);

                    for (int i = 0; i < length; i++)
                    {
                        if (invalid[i])
                            continue;

                        newNormals.Enqueue(normals[i]);
                    }

                    normals = newNormals.ToArray();
                }
                //else
                //    mesh.RecalculateNormals();
            }
            #endregion

            #region tangent
            var tangents = mesh.tangents;
            {
                //Assert.IsTrue(tangents.Length == length || tangents.Length == 0, $"tangent count : {tangents.Length}");

                length = tangents.Length;

                var nums = length - removeVertices.Count;
                if (nums > 0)
                {
                    var newTangents = new Queue<Vector4>(nums);

                    for (int i = 0; i < length; i++)
                    {
                        if (invalid[i])
                            continue;

                        newTangents.Enqueue(tangents[i]);
                    }

                    tangents = newTangents.ToArray();
                }
            }
            #endregion



        


            #region uvs
            var uvs = mesh.uv;
            {
                //Assert.IsTrue(bindposes.Length == length, $"bindpose count : {bindposes.Length}");

                length = uvs.Length;

                var nums = length - removeVertices.Count;
                if (nums > 0)
                {
                    var newUVs = new Queue<Vector2>(nums);

                    for (int i = 0; i < length; i++)
                    {
                        if (invalid[i])
                            continue;

                        newUVs.Enqueue(uvs[i]);
                    }

                    uvs = newUVs.ToArray();
                }
            }
            #endregion


            mesh = new Mesh()
            {
                vertices=vertices,
                triangles=triangles,
                bindposes=bindposes,
                boneWeights=boneweights,
                normals=normals,
                tangents=tangents,         
                uv=uvs,
            };

            renderer.sharedMesh = mesh;

            Debug.Log(
                $"Bones : {bones.Length}\n" +
                $"vertice : {vertices.Length}\n"+
                $"triangles : {triangles.Length}\n"+
                $"bindposes : {bindposes.Length}\n"+
                $"boneWeights : {boneweights.Length}\n"+
                $"normals : {normals.Length}\n"+
                $"tangents : {tangents.Length}\n" +
                $"uvs : {uvs.Length}\n"
                );


            renderer.sharedMesh.RecalculateBounds(UnityEngine.Rendering.MeshUpdateFlags.Default);
            renderer.ResetLocalBounds();
        }


        public static SkinnedMeshRenderer SkinnedMeshRenderer;


        static Dictionary<int, Transform> TargetedTransforms;

        public static Dictionary<HumanBodyBones, Transform> SelectedBones;


        public static void SelectBonesTransform(Mesh mesh, HumanBodyBones[] bones)
        {
            var animator = SkinnedMeshRenderer.GetComponentInParent<Animator>();

            Assert.IsNotNull(animator);

            SelectedBones = HumanBodyBonesExtension.HumanBoneMap(animator, bones);

            //var tfs = SkinnedMeshRenderer.bones;
            //var length = tfs.Length;

            //var indexMap = new Dictionary<int, ReadOnlyTransform>(length);

            //for (int i = 0; i < length; i++)
            //{
            //    var tf=tfs[i];
            //    if (!bonesDic.ContainsValue(tf))
            //        continue;

            //    indexMap.Add(i, tf);
            //}



            //var colors = Mesh.colors;
            //Debug.Log(colors.Length);

            //var weights = Mesh.boneWeights;
            //var indexs = new int[4];

            //length = weights.Length;

            //TargetedVertices = new((int)(length*(indexMap.Count / (float)HumanBodyBones.LastBone)));
            //collapseVertices = new bool[length];

            //for (int i = 0; i < length; i++)
            //{
            //    var weight = weights[i];

            //    var sn = weight.BoneWeightIndexArray(ref indexs);

            //    if (sn == 0)
            //        continue;

            //    for (int q = 0; q < sn; q++)
            //    {
            //        if (indexMap.TryGetValue(indexs[q], out var bone))
            //        {
            //            Debug.Log(bone.name+i);
            //            TargetedVertices.Add(i);
            //            collapseVertices[i] = true;
            //        }
            //    }
            //}

            //Debug.Log(Mesh.uv.Length);

        }


        public static void PushBonesTransform()
        {
            var list = SkinnedMeshRenderer.bones;
            foreach (var selected in SelectedBones)
            {
                if (TargetedTransforms.ContainsValue(selected.Value))
                    continue;

                //var index = list.GetChildOfIndex(selected.Value);
                var index = list.IndexOf(selected.Value);
                TargetedTransforms.Add(index, selected.Value);
            }
        }

        //void DrawLimbMesh()
        //{
        //    Mesh = SkinnedMeshRenderer.sharedMesh;

        //    var limbs = Limbs.List.Select(x => x.ParseRootBone()).ToArray();

        //    var animator = SkinnedMeshRenderer.GetComponentInParent<Animator>();

        //    Assert.IsNotNull(animator);

        //    var bones = HumanBodyBonesExtension.GetAllHumanoidBones(animator, limbs);

        //    var tfs = SkinnedMeshRenderer.bones;
        //    var length = tfs.Length;

        //    for (int i = 0; i < length; i++)
        //    {
        //        var tf = tfs[i];

        //        foreach (var bone in bones)
        //        {
        //            if (tf.IsChildOf(bone))
        //            {
        //                TargetedTransforms.SafeAdd(i, tf);
        //                break;
        //            }
        //        }
        //    }
        //}

        //public void DrawBoneVertices()
        //{
        //    var colors = Mesh.colors;
        //    Debug.Log(colors.Length);

        //    var weights = Mesh.boneWeights;
        //    var indexs = new int[4];

        //    var length = weights.Length;

        //    TargetedVertices = new((int)(length * (TargetedTransforms.Count / (float)HumanBodyBones.LastBone)));
        //    collapseVertices = new bool[length];

        //    for (int i = 0; i < length; i++)
        //    {
        //        var weight = weights[i];

        //        var sn = weight.BoneWeightIndexArray(ref indexs);

        //        if (sn == 0)
        //            continue;

        //        for (int q = 0; q < sn; q++)
        //        {
        //            if (TargetedTransforms.TryGetValue(indexs[q], out var bone))
        //            {
        //                //Debug.Log(bone.name + i);
        //                TargetedVertices.Add(i);
        //                collapseVertices[i] = true;
        //            }
        //        }
        //    }


        //    Debug.Log(Mesh.uv.Length);
        //}


        /// <summary>
        /// Collapse mesh vertices
        /// </summary>
        /// <param name="collapseVertices">Vertice to delete.</param>
        public static void StripMesh(this Mesh mesh,bool[] collapseVertices)
        {
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var weights = mesh.boneWeights;
            var uvs = mesh.uv;

            var length = vertices.Length;

            //var rebindVertice = new Dictionary<int, int>(length);
            //var newVertices = new List<Vector3>(length);
            //var newWeights = new List<BoneWeight>(length);
            //var newVerticeNum = 0;

            //for (int i = 0; i < length; i++)
            //{
            //    if (!collapseVertices[i])
            //        continue;

            //    rebindVertice.Add(i, newVerticeNum);
            //    newVertices.Add(vertices[i]);

            //    if (weights.Length < i)
            //        Debug.Log(i + "**" + weights.Length);
            //    else
            //        newWeights.Add(weights[i]);

            //    newVerticeNum++;
            //}


            var rebindVertice = new Dictionary<int, int>(length);

            length = triangles.Length;
            var newTriangles = new List<int>(length);

            var newVerticeNum = 0;

            for (int i = 0; i < length; i += 3)
            {
                var a = triangles[i];
                var b = triangles[i + 1];
                var c = triangles[i + 2];

                if (collapseVertices[a] || collapseVertices[b] || collapseVertices[c])
                {
                    if (!rebindVertice.TryGetValue(a, out var newSn))
                    {
                        rebindVertice.Add(a, newVerticeNum);
                        newSn = newVerticeNum;
                        newVerticeNum++;
                    }

                    newTriangles.Add(newSn);

                    if (!rebindVertice.TryGetValue(b, out newSn))
                    {
                        rebindVertice.Add(b, newVerticeNum);
                        newSn = newVerticeNum;
                        newVerticeNum++;
                    }

                    newTriangles.Add(newSn);

                    if (!rebindVertice.TryGetValue(c, out newSn))
                    {
                        rebindVertice.Add(c, newVerticeNum);
                        newSn = newVerticeNum;
                        newVerticeNum++;
                    }

                    newTriangles.Add(newSn);
                }
            }

            Debug.Log(newTriangles.Count);

            length = rebindVertice.Count;
            var newVertices = new Vector3[length];
            var newWeights = new BoneWeight[length];
            var newUVs = new Vector2[length];

            foreach (var rebind in rebindVertice)
            {
                var old = rebind.Key;
                var @new = rebind.Value;

                newVertices[@new] = vertices[old];
                newWeights[@new] = weights[old];
                newUVs[@new] = uvs[old];
            }

            mesh.vertices = newVertices;
            mesh.triangles = newTriangles.ToArray();
            mesh.uv = newUVs;
            mesh.bindposes = mesh.bindposes;
            mesh.boneWeights = newWeights;

            //mesh = new Mesh()
            //{
            //    vertices = newVertices,
            //    triangles = newTriangles.ToArray(),
            //    uv = newUVs,
            //    bindposes = mesh.bindposes,
            //    boneWeights = newWeights,
            //};

            mesh.RecalculateNormals();
            // mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            //mesh.RecalculateUVDistributionMetric();
        }
       
    }
}
