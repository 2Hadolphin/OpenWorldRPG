//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Animations;

//using Return.CentreModule;
//using Return;
//using Return.Humanoid;
//using Return.Humanoid.Animation;
//using Sirenix.OdinInspector;
//using System;

//namespace Return.Humanoid.IK
//{
 
//    public class IK_Hand :  HumanoidModularSystem
//    {
//        public ReadOnlyTransform GoalRoot { get; protected set; }

//        public Behaviour_HandIK IK;
//        public void Init(HumanoidAgent_Simulation agent)
//        {

//            Init(agent.Animator);

//        }

//        public virtual void Init(IAnimator anim)
//        {

//            var bundle = Resources.Load<BehaviourBundle_Humanoid_HandIK>(BehaviourBundle_Humanoid_HandIK.m_Path);
  
//            anim.AddBehaviourBundle(bundle, out var behaviour);
//            IK = behaviour as Behaviour_HandIK;

//            GoalRoot = anim.GetAnimator.GetBoneTransform(HumanBodyBones.UpperChest);

//            RightHandGoal = new LimbIKGoal(anim, GoalRoot, AvatarIKGoal.RightHand)
//            {
//                MaximumExtension =
//                 anim.GetSkeletonBone(HumanBodyBones.RightLowerArm).position.magnitude
//                +
//                 anim.GetSkeletonBone(HumanBodyBones.RightHand).position.magnitude
//            };

//            LeftHandGoal = new LimbIKGoal(anim, GoalRoot, AvatarIKGoal.LeftHand)
//            {
//                MaximumExtension =
//                 anim.GetSkeletonBone(HumanBodyBones.LeftLowerArm).position.magnitude
//                +
//                 anim.GetSkeletonBone(HumanBodyBones.LeftHand).position.magnitude
//            };


//            IK.Init(RightHandGoal, LeftHandGoal);

//            IK.Enable(true);
//            SetIKPositionSpace(GetBodyIKGoal(Side.Right), true);
//            SetIKPositionSpace(GetBodyIKGoal(Side.Left), true);
//            IK.UpdateJobData();
//        }

//        public ICoordinateDelegate RightHandDelegate;
//        public ICoordinateDelegate LeftHandDelegate;

//        public ICoordinateDelegate RightElbowDelegate;
//        public ICoordinateDelegate LeftElbowDelegate;

//        public LimbIKGoal RightHandGoal { get; protected set; }
//        public LimbIKGoal LeftHandGoal { get; protected set; }


//        public class ActionType : ScriptableObject
//        {
//            public AnimationCurve CurveA;
//            public AnimationCurve CurveB;
//            public AnimationCurve CurveC;

//        }

//        public void Delegate(Side side, out Coordinate_Delegate hand, out Coordinate_Delegate elbow)
//        {
//            hand = new Coordinate_Delegate(GoalRoot);
//            elbow=new Coordinate_Delegate(GoalRoot);
//            switch (side)
//            {
//                case Side.Right:
//                    RightHandDelegate = hand;
//                    RightElbowDelegate = elbow;
//                    break;
//                case Side.Left:
//                    LeftHandDelegate = hand;
//                    LeftElbowDelegate = elbow;
//                    break;
//            }
//        }

//        public void Release()
//        {
//            RightHandDelegate = null;
//            RightElbowDelegate = null;
//            LeftHandDelegate = null;
//            LeftElbowDelegate = null;
//        }

//        mQueue<ISchedule> RightHandSchedules=new mQueue<ISchedule>(3);
//        mQueue<ISchedule> LeftHandSchedules=new mQueue<ISchedule>(3);

//        private void Start()
//        {
//            IK.Weight = 1;
//        }

//        /*DebugTest
//        private void Start()
//        {
//            RightHandGoal.Weight = 1;
//            RightHandGoal.PullWeight = 1;
//            RightHandGoal.LocalPositionSpace = true;
//            RightHandGoal.LocalRotationSpace = true;

//            if(IK.Poses.TryGetValue("IdleState",out var pr))
//            {
//                Pos = pr.GUIDs;
//                Rot = pr.Rotation.eulerAngles;
//                RightHandGoal.GoalPosition = Pos;
//                RightHandGoal.GoalRotation = Quaternion.Euler(Rot);
//            }

//            IK.Weight = 1;
//            IK.UpdateJobData();
//        }

//        public Vector3 Pos;
//        public Vector3 Rot;
//        */
//        private void LateUpdate()
//        { 
//            var deltaTime = ConstCache.deltaTime;


//            var goal = GetBodyIKGoal(Side.Right);
//            if (RightHandDelegate != null)
//            {
//                DelegateHand(goal, RightHandDelegate);
//                DelegateElbow(goal, RightElbowDelegate);
//            }
//            else
//            {
//                Solve(RightHandSchedules, goal, deltaTime);
//            }


//            goal = GetBodyIKGoal(Side.Left);
//            if (LeftHandDelegate != null)
//            {

//                DelegateHand(goal, LeftHandDelegate);
//                DelegateElbow(goal, LeftElbowDelegate);
//            }
//            else
//            {
//                Solve(LeftHandSchedules, goal, deltaTime);
//            }

//            IK.UpdateJobData();
//        }

//        protected void DelegateHand(LimbIKGoal goal, ICoordinateDelegate @delegate)
//        {
//            SetIKPositionSpace(goal, @delegate.OutputPositionSpace == Space.Self);
//            SetIKRotationSpace(goal, @delegate.OutputRotationSpace == Space.Self);
//            SetIK(goal, @delegate.position, @delegate.rotation);
//            SetIKWeight(goal, @delegate.PositionWeight * @delegate.Weight);
//        }

//        protected void DelegateElbow(LimbIKGoal goal, ICoordinateDelegate @delegate)
//        {
//            SetIKHintSpace(goal, @delegate.OutputPositionSpace == Space.Self);
//            SetHint(goal, @delegate.position);
//            SetHintWeight(goal, @delegate.PositionWeight * @delegate.Weight);
//        }


//        protected void Solve(mQueue<ISchedule> queue, LimbIKGoal goal,float deltaTime)
//        {

//            if (queue.Count > 0)
//            {
//                var schedule = queue.Peek();

                

//                if (!schedule.Processing)
//                {
//                    //goal.ZeroGoal(Agent.Animator.GetAnimator);
//                    schedule.Init(goal);
//                    goal.GoalWeight = 1;
//                    goal.InputViaLocalGoal = false;
//                    goal.InputViaLocalRotation = false;
//                    goal.HasTarget = true;
//                }


//                schedule.TimeLog += deltaTime;

//                if (schedule.DuringTime > schedule.TimeLog)
//                {
//                    var ratio = schedule.TimeLog / schedule.DuringTime;

//                    var startP = schedule.Start.position;
//                    var startR = schedule.Start.rotation;

//                    var endP = schedule.Stop.position;
//                    var endR = schedule.Stop.rotation;
           
                        

//                    var relayPoint = Vector3.Project(startP, GoalRoot.up)+GoalRoot.position;

//                    startP -= relayPoint;
//                    endP -= relayPoint;

//                    var pos = Vector3.Slerp(startP, endP, ratio)+relayPoint;


//                    var rot = Quaternion.Lerp(startR, endR, ratio);
//                    DebugDrawUtil.DrawCoordinate(relayPoint, Quaternion.identity);
//                    DebugDrawUtil.DrawCoordinate(startP+relayPoint, startR);
//                    DebugDrawUtil.DrawCoordinate(endP+relayPoint, endR);

//                    if (schedule.ApplyInterlocking)
//                        goal.PullWeight = Mathf.Lerp(0, 1, ratio);

//                    SetIK(goal, pos, rot);
                        
//                }
//                else
//                {
//                    SetIK(goal, schedule.Stop.position, schedule.Stop.rotation);
//                    schedule.DisposePlayingPerformer();
//                    queue.Dequeue();
//                    if (queue.Count == 0)
//                        goal.HasTarget = false;
//                }


//                IK.UpdateJobData();
//            }
//            else
//            {
//                var weight = goal.GoalWeight;
//                if(weight>float.Epsilon)
//                    goal.GoalWeight = Mathf.Lerp(weight, 0, deltaTime * 5);

//                var pullWeight = goal.PullWeight;
//                if (pullWeight > float.Epsilon)
//                    goal.PullWeight = Mathf.Lerp(weight, 0, deltaTime * 5);
//            }
//        }

//        protected LimbIKGoal GetBodyIKGoal(Side side)
//        {
//            switch (side)
//            {
//                case Side.Right : return RightHandGoal;
//                case Side.Left: return LeftHandGoal;
//                default: throw new InvalidProgramException();
//            }
//        }

//        private void SetIKPositionSpace(LimbIKGoal goal, bool localSpace)
//        {
//            goal.InputViaLocalGoal = localSpace;
//        }

//        private void SetIKHintSpace(LimbIKGoal goal, bool localSpace)
//        {
//            goal.InputViaLocalHint = localSpace;
//        }

//        private void SetIKRotationSpace(LimbIKGoal goal, bool localSpace)
//        {
//            goal.InputViaLocalRotation = localSpace;
//        }
//        private void SetIKWeight(LimbIKGoal goal, float weight)
//        {
//            goal.GoalWeight = weight;
//        }
//        private void SetHintWeight(LimbIKGoal goal, float weight)
//        {
//            goal.HintWeight = weight;
//        }
//        private void SetIK(LimbIKGoal goal, Vector3 position, Quaternion rotation)
//        {
//            goal.GoalPosition = position;
//            goal.GoalRotation = rotation;
//        }

//        private void SetHint(LimbIKGoal goal, Vector3 position)
//        {
//            goal.HintPosition = position;
//        }

//        public ISchedule SealMission(Side side)
//        {
//            mQueue<ISchedule> queue = TakeSide(side);
//            return queue.Last;
//        }

//        public mQueue<ISchedule> TakeSide(Side side)
//        {
//            switch (side)
//            {
//                case Side.Right:
//                    return RightHandSchedules;

//                case Side.Left:
//                    return LeftHandSchedules;

//                default:
//                    throw new InvalidProgramException();
//            }
//        }

//        public void SetMission(Side side, ISchedule schedule)
//        {
//            mQueue<ISchedule> queue = TakeSide(side);
//            queue.Enqueue(schedule);
//        }

//        private void OnDrawGizmos()
//        {
//            return;
//            Gizmos.DrawWireSphere(RightHandGoal.GoalPosition, 0.1f);
//            Gizmos.DrawWireSphere(LeftHandGoal.GoalPosition, 0.1f);
//            Gizmos.DrawWireSphere(RightHandGoal.HintPosition, 0.1f);
//            Gizmos.DrawWireSphere(LeftHandGoal.HintPosition, 0.1f);
//        }
//    }
//}