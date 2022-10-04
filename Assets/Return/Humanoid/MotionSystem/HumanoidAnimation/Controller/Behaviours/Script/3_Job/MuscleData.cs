using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Animations;
using System;

using Sirenix.OdinInspector;
using System.Linq;

namespace Return.Humanoid.Animation
{
    [Serializable]
    public struct MuscleData
    {
        public MuscleData(MuscleHandle handle)
        {
            m_Handle = handle;
            m_Value = default;
            m_Clamp = true;
            InheritIKOverwrite = false;
        }
        

        [HideInInspector][SerializeField]
        public MuscleHandle m_Handle;

#if UNITY_EDITOR
        [ReadOnly]
        [ShowInInspector]
        [HorizontalGroup]
        [HideLabel]
        [PropertyOrder(-1)]
        public string Name => m_Handle.name;

#endif
        [PropertyOrder(0)]
        [Range(-1f, 1f)]
        [HorizontalGroup]
        [HideLabel]
        public float m_Value;

        public bool InheritIKOverwrite;

        public bool m_Clamp;

        public static MuscleData[] CreateAllMuscleDatas
        {
            get
            {
                var handles = new MuscleHandle[MuscleHandle.muscleHandleCount];
                MuscleHandle.GetMuscleHandles(handles);

                var length = handles.Length;
                var datas = new MuscleData[length];
                for (int i = 0; i < length; i++)
                {
                    datas[i] = new MuscleData(handles[i]);
                }

                return datas;
            }
        }

        public static MuscleData[] BindDatas(params MuscleHandle[] handles)
            => handles.Select(x => new MuscleData(x)).ToArray();

        //public bool IsMatch(string searchString)
        //{
        //    return Name.Contains(searchString);
        //}

        public static MuscleData[] RightArm => BindDatas(HumanoidAnimationUtility.GetArmMuscleHandles(HumanPartDof.RightArm));
        public static MuscleData[] RightFingers => BindDatas(HumanoidAnimationUtility.GetFingerMuscleHandles(HumanPartDofUtility.RightHand));

        public static MuscleData[] LeftArm => BindDatas(HumanoidAnimationUtility.GetArmMuscleHandles(HumanPartDof.LeftArm));
        public static MuscleData[] LeftFingers => BindDatas(HumanoidAnimationUtility.GetFingerMuscleHandles(HumanPartDofUtility.LeftHand));

    }
}