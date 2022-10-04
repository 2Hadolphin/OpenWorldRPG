//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Return.Humanoid.Animation;
//namespace Return.Humanoid.IK
//{
//    public class IK_Body : HumanoidModularSystem
//    {
//        Behaviour_BodyIK IK;

//        public void Init(HumanoidAgent_Simulation agent)
//        {





//            var bundle = Resources.Load<BehaviourBundle_Humanoid_BodyIK>(BehaviourBundle_Humanoid_BodyIK.m_Path);
//            //Agent.Animator.AddBehaviourBundle(bundle, out var behaviour);

//            //IK = behaviour as Behaviour_BodyIK;
//            //IK.Init(Agent.IKManager.IKPartment);
//            IK.Enable(true);
//            IK.UpdateJobData();
//        }

//        private void Start()
//        {
//            IK.Weight = 1;
//            IK.UpdateJobData();
//        }

//    }
//}