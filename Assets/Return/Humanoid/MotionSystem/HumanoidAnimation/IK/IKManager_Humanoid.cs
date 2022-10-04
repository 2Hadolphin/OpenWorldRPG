//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Animations.Rigging;

//using System;
//using Return.Humanoid.Animation;
//using Sirenix.OdinInspector;

//namespace Return.Humanoid.IK
//{
//    public class IKManager_Humanoid : HumanoidModularSystem
//    {


//        public IAnimator Animator { get; protected set; }
//        public Animator GetAnimator { get => Animator.GetAnimator; }
//        [ShowInInspector]
//        public IK_Hand Hand { get; protected set; }
//        public IK_Body Body { get; protected set; }
//        public IK_Foot Foot { get; protected set; }

//        public List<IK.ILimbIKGoalData> IKPartment;

//        #region outDate
//        public enum IKState { none = 0, View = 1, Hand = 2, Foot = 3, Body = 4 }
//        public enum IKPart { LeftHand = 0, RightHand = 1 }
//        public IKState ikState { get; private set; }

//        protected Animator Anim = null;

//        protected ReadOnlyTransform body_tip;
//        [SerializeField]
//        protected ReadOnlyTransform AnimationRoot = null;
//        protected ReadOnlyTransform Hip;

//        public IK_Head View { get; protected set; }
//        public IK_Head Head_IK { get; protected set; }

//        public IK_Foot IK_Foot { get; protected set; }

//        public enum IKFunction { Null = 0, CriticalBreak = 1 }



//        #endregion

//        public void Init(HumanoidAgent_Simulation agent)
//        {
  
//            var go = CharacterRoot;
//            //Animator = Agent.Animator;

//            go.InstanceIfNull(() => Hand, (width) => Hand=width);
//            go.InstanceIfNull(() => Body, (width) => Body = width);
//            go.InstanceIfNull(() => Foot, (width) => Foot = width);


//            Hip = Animator.GetAnimator.GetBoneTransform(HumanBodyBones.Hips);

//            Hand.Init(agent);
//            Foot.Init(agent);

//            IKPartment = new List<ILimbIKGoalData>(4);
//            IKPartment.Add(Hand.RightHandGoal);
//            IKPartment.Add(Hand.LeftHandGoal);
//            IKPartment.Add(Foot.RightFootGoal);
//            IKPartment.Add(Foot.LeftFootGoal);

//            Body.Init(agent);


//            #region SetHandler


//            #endregion

//        }




//    }
//}