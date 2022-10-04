using UnityEngine;

namespace Return.Humanoid
{
    public enum HumanoidLayer { Basic=0,Foot=1,Top=2,Hand_Prime=3,Hand_Second=4,Action_All=5}
    public class AnimatorHash_Humanoid
    {
        public int triggerPreMov;

        public int intPlayerState;
        public int boolMirror;
        public int boolGrounded;


        public int floatMovSpeed;
        public int floatMovDirX;
        public int floatMovDirZ;

        public int intHandState_prime;
        public int intHandState_second;
        public float floatHandSpeed;
        public float floatHandSeed;

        public int intFootState;
        public int floatFootSpeed;
        public int floatFootSeed;


        public int triggerTurn;
        public int floatTurn;


        public int floatIdleRate;
        public int floatBreathGasp;

        /*
        public int Centre;
        public int IdleState;
        public int Walk;
        public int Run;
        public int Jump;
        public int Fall;
        public int LookAt;
        public int Climb;
        */


        public int LayerBasic; // jump shake climb 
        public int LayerFoot; // walk run 
        public int LayerHand_prime; // pick
        public int LayerHand_second; // pick
        public int LayerTop; // fall
        public int LayerActionAll; // fall


        /*
        public float Weight;

        public int dyingState;
        public int locomotionState;
        public int shoutState;
        public int deadBool;
        public int speedFloat;
        public int sneakingBool;
        public int shoutingBool;
        public int playerInSightBool;
        public int shotFloat;
        public int aimWeightFloat;
        public int angularSpeedFloat;
        public int openBool;
        */

        #region State
        public int State_Jump;
        public int State_Land;
        public int State_Fall;
        public int State_Null;
        public int State_Idle;
        public int State_Locomotion;
        #endregion
        public AnimatorHash_Humanoid(Animator animator)
        {
            #region Parameter
            intPlayerState = Animator.StringToHash("intPlayerState");
            boolGrounded = Animator.StringToHash("boolGrounded");
            boolMirror = Animator.StringToHash("boolMirror");

            floatMovSpeed = Animator.StringToHash("floatMovSpeed");
            floatMovDirX = Animator.StringToHash("floatMovDirX");
            floatMovDirZ = Animator.StringToHash("floatMovDirZ");

            intHandState_prime = Animator.StringToHash("intHandState_prime");
            intHandState_second = Animator.StringToHash("intHandState_second");
            floatHandSpeed = Animator.StringToHash("floatHandSpeed");
            floatHandSeed = Animator.StringToHash("floatHandSeed");

            intFootState = Animator.StringToHash("intFootState");
            floatFootSpeed = Animator.StringToHash("floatFootSpeed");

            floatTurn = Animator.StringToHash("floatTurn");
            triggerTurn = Animator.StringToHash("triggerTurn");
            triggerPreMov = Animator.StringToHash("PreMov");

            floatIdleRate = Animator.StringToHash("floatIdleRate");
            floatBreathGasp = Animator.StringToHash("floatBreathGasp");

            #endregion

            #region Layer

            LayerBasic = animator.GetLayerIndex("Basic");
            LayerFoot = animator.GetLayerIndex("Foot");
            LayerHand_prime = animator.GetLayerIndex("Hand_Prime");
            LayerHand_second = animator.GetLayerIndex("Hand_Second");
            LayerTop = animator.GetLayerIndex("Up");
            LayerActionAll = animator.GetLayerIndex("Action_All");

            #endregion

            #region State
            State_Jump = Animator.StringToHash("Jump");
            State_Land = Animator.StringToHash("Land");
            State_Fall= Animator.StringToHash("Fall_Blance");
            State_Locomotion = Animator.StringToHash("Locomotion");
            State_Idle = Animator.StringToHash("IdleState");
            State_Null = Animator.StringToHash("Null");
            #endregion
        }

    }
}