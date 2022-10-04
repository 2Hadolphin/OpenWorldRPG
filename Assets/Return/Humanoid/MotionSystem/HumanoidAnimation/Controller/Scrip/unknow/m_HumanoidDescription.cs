#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using Sirenix.OdinInspector;

namespace Return.Humanoid
{
    [Serializable]
    public class m_HumanoidDescription : ScriptableObject
    {
        public float armStretch = 0.05f;
        public float legStretch = 0.05f;
        public float upperArmTwist = 0.5f;
        public float lowerArmTwist = 0.5f;
        public float upperLegTwist = 0.5f;
        public float lowerLegTwist = 0.5f;
        public float feetSpacing = 0;
        public bool hasTranslationDoF;
        public HumanBoneLimit[] human;

        public Avatar CreateAvatar(Transform root,SkeletonMap skeleton)
        {
            //return AvatarBuilder.BuildHumanAvatar(root.CharacterRoot, ToHumanDescription(root));
            return AvatarBuilder.BuildHumanAvatar(root.gameObject, ToHumanDescription(skeleton));
        }

        public HumanDescription ToHumanDescription(Transform root)
        {
            var transforms = root.GetComponentsInChildren<Transform>();
            var skeletonBones = new SkeletonBone[transforms.Length];

            var index = 0;

            foreach (var t in transforms)
            {
                skeletonBones[index] = t.ToSkeletonBone();
                index++;
            }

            var length = human.Length;

            var humanBones = new HumanBone[length];


            for (int i = 0; i < length; i++)
            {
                humanBones[i] = human[i].ToHumanBone();
            }

            //length = skeletonBones.Length;

            // ???????
            //for (int i = 0; i < length; i++)
            //{
            //    var a = UnityEngine.Random.Distance(0, length);
            //    var b = UnityEngine.Random.Distance(0, length);

            //    if (a == b)
            //    {
            //        Debug.LogError("********");
            //        continue;
            //    }


            //    var skeletonA = skeletonBones[a];
            //    var skeletonB = skeletonBones[b];

            //    skeletonBones[a] = skeletonB;
            //    skeletonBones[b] = skeletonA;
            //    Debug.Log("Switch " + a + " and " + b);
            //}



            return new HumanDescription
            {
                skeleton = skeletonBones,
                human = humanBones,
                armStretch = armStretch,
                legStretch = legStretch,
                upperArmTwist = upperArmTwist,
                lowerArmTwist = lowerArmTwist,
                upperLegTwist = upperLegTwist,
                lowerLegTwist = lowerLegTwist,
                feetSpacing = feetSpacing,
                hasTranslationDoF = hasTranslationDoF,
            };
        }


        public HumanDescription ToHumanDescription(SkeletonMap skeletonMap)
        {
            var humanBones = human.Select(x => x.ToHumanBone()).ToArray();

            //var length = boneChain.Length;

            var skeletonBones = skeletonMap.ToSkeletonBones();

            Debug.Log("SkeletonMap only : "+skeletonBones.Length);

            return new HumanDescription
            {
                skeleton = skeletonBones,
                human = humanBones,
                armStretch = armStretch,
                legStretch = legStretch,
                upperArmTwist = upperArmTwist,
                lowerArmTwist = lowerArmTwist,
                upperLegTwist = upperLegTwist,
                lowerLegTwist = lowerLegTwist,
                feetSpacing = feetSpacing,
                hasTranslationDoF = hasTranslationDoF,
            };

        }

        [Obsolete]
        public HumanDescription ToHumanDescription(Transform root,SkeletonMap skeletonMap)
        {
            var length = human.Length;
            var humanBones = human.Select(x => x.ToHumanBone()).ToArray();


            var transforms = root.GetComponentsInChildren<Transform>();
            length = transforms.Length;

            var skeletonBones = new SkeletonBone[length];


            var index = 0;

            foreach (var t in transforms)
            {
                skeletonBones[index] = t.ToSkeletonBone();
                index++;
            }

            Debug.Log("root +  skeletonMap : " + skeletonBones.Length);


            /*
            for (int i = 0; i < length; i++)
            {
                var a = UnityEngine.Random.Distance(0, length);
                var b = UnityEngine.Random.Distance(0, length);
                if (a == b)
                {
                    Debug.LogError("********");
                    continue;
                }


                var skeletonA = skeletonBones[a];
                var skeletonB = skeletonBones[b];

                skeletonBones[a] = skeletonB;
                skeletonBones[b] = skeletonA;
            }
            
            */

            return new HumanDescription
            {
                skeleton = skeletonBones,
                human = humanBones,
                armStretch = armStretch,
                legStretch = legStretch,
                upperArmTwist = upperArmTwist,
                lowerArmTwist = lowerArmTwist,
                upperLegTwist = upperLegTwist,
                lowerLegTwist = lowerLegTwist,
                feetSpacing = feetSpacing,
                hasTranslationDoF = hasTranslationDoF,
            };

        }


        public Avatar CreateAvatar(Transform root)
        {
            return AvatarBuilder.BuildHumanAvatar(root.gameObject, ToHumanDescription(root));
        }

        public Avatar CreateAvatarAndSetup(Transform root)
        {
            var avatar = CreateAvatar(root);
            avatar.name = name;

            var animator = root.GetComponent<Animator>();

            /*
            if (anim != null)
            {
                var positionMap = root.Traverse().ToDictionary(width => width, width => width.position);
                anim.avatar = avatar;
                foreach (var width in root.Traverse())
                {
                    width.position = positionMap[width];
                }
            }

            var transfer = root.GetComponent<HumanPoseTransfer>();
            if (transfer != null)
            {
                transfer.Avatar = avatar;
            }

            */
            return avatar;
        }

#if UNITY_EDITOR
        public static m_HumanoidDescription CreateFrom(Avatar avatar)
        {
            var description = default(HumanDescription);
            if (!GetHumanDescription(avatar, ref description))
            {
                return null;
            }

            return CreateFrom(description);
        }
#endif

        public static m_HumanoidDescription CreateFrom(HumanDescription description)
        {
            var avatarDescription = CreateInstance<m_HumanoidDescription>();
            avatarDescription.name = "AvatarDescription";
            avatarDescription.armStretch = description.armStretch;
            avatarDescription.legStretch = description.legStretch;
            avatarDescription.feetSpacing = description.feetSpacing;
            avatarDescription.hasTranslationDoF = description.hasTranslationDoF;
            avatarDescription.lowerArmTwist = description.lowerArmTwist;
            avatarDescription.lowerLegTwist = description.lowerLegTwist;
            avatarDescription.upperArmTwist = description.upperArmTwist;
            avatarDescription.upperLegTwist = description.upperLegTwist;
            avatarDescription.human = description.human.Select(x=>HumanBoneLimit.From(x)).ToArray();
            return avatarDescription;
        }
        public static m_HumanoidDescription Create(Transform[] boneTransforms, SkeletonMap skeleton)
        {
            throw new InvalidOperationException();

            /*
            return Create(skeleton.BoneIndex.Select(
                width => new KeyValuePair<HumanBodyBones, ReadOnlyTransform>(width.Key, boneTransforms[width.m_Value])));
            */
        }

        public static m_HumanoidDescription Create(IEnumerable<KeyValuePair<HumanBodyBones, Transform>> skeleton)
        {
            var description = Create();
            description.SetHumanBones(skeleton);
            return description;
        }
        public static m_HumanoidDescription Create(m_HumanoidDescription src = null)
        {
            var humanDescription = CreateInstance<m_HumanoidDescription>();
            humanDescription.name = "AvatarDescription";
            if (src != null)
            {
                humanDescription.armStretch = src.armStretch;
                humanDescription.legStretch = src.legStretch;
                humanDescription.feetSpacing = src.feetSpacing;
                humanDescription.upperArmTwist = src.upperArmTwist;
                humanDescription.lowerArmTwist = src.lowerArmTwist;
                humanDescription.upperLegTwist = src.upperLegTwist;
                humanDescription.lowerLegTwist = src.lowerLegTwist;
                //humanDescription.human = src.human;
            }
            else
            {
                humanDescription.armStretch = 0.05f;
                humanDescription.legStretch = 0.05f;
                humanDescription.feetSpacing = 0.0f;
                humanDescription.lowerArmTwist = 0.5f;
                humanDescription.upperArmTwist = 0.5f;
                humanDescription.upperLegTwist = 0.5f;
                humanDescription.lowerLegTwist = 0.5f;
                //humanDescription.human = new BoneLimit[0];
            }

            return humanDescription;
        }




        public void SetHumanBones(IEnumerable<KeyValuePair<HumanBodyBones, Transform>> skeleton)
        {
            human = skeleton.Where(x=>x.Value!=null).Select(x =>
            {
                return new HumanBoneLimit
                {
                    humanBone = x.Key,
                    boneName = x.Value.name,
                    useDefaultValues = true,
                    axisLength = x.Value.childCount>0? x.Value.GetChild(0).localPosition.magnitude:0,
                };
            }).ToArray();
        }

#if UNITY_EDITOR
        public static bool GetHumanDescription(UnityEngine.Object target, ref HumanDescription des)
        {
            if (target != null)
            {
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(target));
                if (importer != null)
                {
                    Debug.Log("AssetImporter Type: " + importer.GetType());
                    ModelImporter modelImporter = importer as ModelImporter;
                    if (modelImporter != null)
                    {
                        des = modelImporter.humanDescription;
                        Debug.Log("## Cool stuff data by ModelImporter ##");
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning("## Please Select Imported Model in Project View not prefab or other things ##");
                    }
                }
            }

            return false;
        }
#endif
    }
}

#endif