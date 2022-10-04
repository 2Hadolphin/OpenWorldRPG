#if UNITY_EDITOR


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;

namespace Return.Humanoid
{
    /// <summary>
    /// Mapping HumanBodyBones to BoneIndex
    /// </summary>
    public struct SkeletonMap
    {
        public static SkeletonMap BuildSkeketonBase
        {
            get
            {
                var skeleton = new SkeletonMap();

                var final = (int)HumanBodyBones.LastBone;
                var bones = new HumanBodyBones[final];

                skeleton._ExtraBoneMap = new Dictionary<Transform, int>();
                skeleton.HumanBoneMapTransform = new DoubleSidedDictionary<HumanBodyBones, Transform>(final);



                for (int i = 0; i < final; i++)
                {
                    var bone= (HumanBodyBones)i;
                    bones[i] = bone;
                    skeleton.HumanBoneMapTransform.Add(bone, null);
                }


                /*
                var bones = (HumanBodyBones[])Enum.GetValues(typeof(HumanBodyBones));
                skeleton.BoneMapIndex = bones.ToDictionary(width => width, width => (int)width);
   
                foreach (var value in bones)
                {
                    skeleton.BoneMapTransform.Add(value, null);
                }

                print(skeleton.BoneMapTransform);
                */
                return skeleton;

            }
        }

        public bool IsReady => HumanBoneMapTransform[HumanBodyBones.Hips] != null;

 

        private DoubleSidedDictionary<HumanBodyBones, Transform> HumanBoneMapTransform;
        public DoubleSidedDictionary<HumanBodyBones, Transform> BoneTransform { get => HumanBoneMapTransform; }
        private Dictionary<Transform, int> _ExtraBoneMap; 
        public Dictionary<Transform, int> ExtraBoneMap { get => _ExtraBoneMap; }

        public Transform SkeletonRoot;

        public bool HasBone(HumanBodyBones bone)
        {
            return HumanBoneMapTransform.InPairs(bone);
        }

        public void BindBone(HumanBodyBones humanBodyBone,Transform transform)
        {
            Debug.Log("Binding " +humanBodyBone + "-" + transform);

            if (transform == null)
                return;

            HumanBoneMapTransform.Add(humanBodyBone, transform);
        }


        public SkeletonBone[] ToSkeletonBones()
        {
            var length = (int)HumanBodyBones.LastBone;
            var skeletonBones = new List<SkeletonBone>(HumanBoneMapTransform.Forward.Length);

            var sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                if (!HumanBoneMapTransform.Forward.TryGetValue((HumanBodyBones)i, out var tf) || !tf)
                {
                    sb.AppendLine(string.Format("Missing {0} bone", (HumanBodyBones)i));
                    continue;
                }


                skeletonBones.Add(
                    new SkeletonBone()
                    {
                        name = tf.name,
                        position = tf.localPosition,
                        rotation = tf.localRotation,
                        scale = tf.localScale,
                    });
            }

            skeletonBones.Add(
                                    new SkeletonBone()
                                    {
                                        name = SkeletonRoot.name,
                                        position = SkeletonRoot.localPosition,
                                        rotation = SkeletonRoot.localRotation,
                                        scale = SkeletonRoot.localScale,
                                    });
            

            if (sb.Length > 0)
                Debug.LogError("Missing human body bones : \n"+sb.ToString());

            Debug.Log("ListCapacity : " + skeletonBones.Capacity);
            Debug.Log("HumanBoneMapCount : " + skeletonBones.Count);

            foreach (var extraBone in ExtraBoneMap)
            {
                var tf = extraBone.Key;

                skeletonBones.Add(
                    new SkeletonBone()
                    {
                         name = tf.name,
                         position = tf.localPosition,
                         rotation = tf.localRotation,
                         scale = tf.localScale,
                    });
            }

            Debug.Log("AllBoneMapCount : " + skeletonBones.Count);

            

            return skeletonBones.OrderBy(x => x.name).ToArray();

            skeletonBones.AddRange(
                ExtraBoneMap.Select( x =>
                {
                    var tf = x.Key;
   
                    return new SkeletonBone()
                    {
                        name = tf.name,
                        position = tf.localPosition,
                        rotation = tf.localRotation,
                        scale = tf.localScale,
                    };
                }).ToArray());


        }

        public static SkeletonMap Create(Transform root)
        {
            var skeleton = new SkeletonMap();
            var tfs = root.GetComponentsInChildren<Transform>();
            Transform hip=null;

            foreach (var tf in tfs)
            {
                if (tf.name.Contains("hip"))
                {
                    hip = tf;
                    break;
                }
            }

            if (hip == null)
                UnityEditor.EditorGUILayout.HelpBox("Can't find hip bone", UnityEditor.MessageType.Error, false);

            return skeleton;
        }



  

    }

    [Serializable]
    public struct HumanBoneLimit
    {
        public HumanBodyBones humanBone;
        public string boneName;
        public bool useDefaultValues;
        public Vector3 min;
        public Vector3 max;
        public Vector3 center;
        public float axisLength;
        private static string[] cashedHumanTraitBoneName = null;



        public static HumanBoneLimit From(HumanBone bone)
        {
            return new HumanBoneLimit
            {
                humanBone = (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), bone.humanName.Replace(" ", ""), true),
                boneName = bone.boneName,
                useDefaultValues = bone.limit.useDefaultValues,
                min = bone.limit.min,
                max = bone.limit.max,
                center = bone.limit.center,
                axisLength = bone.limit.axisLength,
            };
        }

        public static string ToHumanBoneName(HumanBodyBones b)
        {
            if (cashedHumanTraitBoneName == null)
            {
                cashedHumanTraitBoneName = HumanTrait.BoneName;
            }

            foreach (var x in cashedHumanTraitBoneName)
            {
                if (x.Replace(" ", "") == b.ToString())
                {
                    return x;
                }
            }

            throw new KeyNotFoundException();
        }

        public HumanBone ToHumanBone()
        {
            return new HumanBone
            {
                boneName = boneName,
                humanName = ToHumanBoneName(humanBone),
                limit = new HumanLimit
                {
                    useDefaultValues = useDefaultValues,
                    axisLength = axisLength,
                    center = center,
                    max = max,
                    min = min
                },
            };
        }
    }

}

#endif