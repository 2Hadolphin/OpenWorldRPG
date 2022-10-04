//using System.Collections;
//using UnityEngine;
//using Return.Definition;
//using Return.Humanoid;

//using Return.CentreModule;
//using Return;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Return.Humanoid.IK;

//public partial class Anim_Humanoid : HumanoidModularSystem
//{
//    public enum MotionRate { IdleState, Stroll, Walk, Jog, Sprint }
//    protected MotionRate AnimSpeedState;
//    protected MotionRate SetAnimSpeed
//    {
//        get { return AnimSpeedState; }
//        set
//        {
//            if (value > MotionRate.Sprint || value < 0)
//                return;


//            if (MathfUtility.Similar(AnimSpeedRate, (int)value, ConstCache.deltaTime))
//            {
//                AnimSpeedState = value;
//                AnimSpeedRate = (int)value;
//            }
//            else
//            {
//                var variable = (ConstCache.deltaTime * 5f).Lerp((int)AnimSpeedState, (int)value);
//                AnimSpeedRate += variable;
//            }
//        }
//    }
//    public class InpVariable
//    {
//        public Vector3 aimPos;
//        public ReadOnlyTransform mTransform;
//        public ReadOnlyTransform CamPoint;
//    }
//    public class CharacterState
//    {
//        public bool isHandIK;
//        public bool isAiming;
//        public float delta;
//        public float cooldownTime = 4;
//    }

//    //public mMotionSystem_Exhibit motor { get { return Agent.I.Motor; } }
//    public InpVariable inpv { get; protected set; }
//    public CharacterState chState { get; protected set; }

//    protected Animator _Animator;
//    public Animator anim { get { return _Animator; } }
//    protected IKManager_Humanoid IK;
//    public IKManager_Humanoid ikManager { get { return IK; } }
//    public AnimatorHash_Humanoid hash { get; protected set; }


//    public float lerpTime = 0.15f;



//    #region ActionDefinition
//    public enum ACList_Hand
//    {
//        defaultWait = 0,
//        PickUp = 1,
//        Pickup_Front = 2,
//        Pickup_Side = 3,
//        Pickup_Top = 4,

//        Equip = 5,
//        Release = 6,
//        Storage_Front = 7,
//        Storage_Side = 8,
//        Storage_Top = 9,

//        Use = 10,
//        Scrap = 11,


//        Aim = 20,
//    }




//    #endregion
//    public Dictionary<string, Injector.Void> AnimationEventCallback;
//    #region AnimateState

//    protected ACList_Hand RHandState;
//    protected ACList_Hand LHandState;
//    protected Motion_Foot FootState;

//    #endregion

//    [SerializeField]
//    protected bool Moveable = false; //Falling or UnStandup



//    protected WaitForSeconds waitLerp;
//    public float updateStateRate = 0.2f;



//    #region Main
//    public virtual void OnAnimatorMove()
//    {
//        ApplyMotionEnd();
//    }
//    protected Vector2 speed = Vector2.zero;
//    public virtual void ApplyMotion(Vector2 vector)
//    {
//        speed = Vector2.Lerp(speed, vector, ConstCache.deltaTime * 5f);
//        anim.SetFloat(hash.floatMovDirX, speed.width);
//        anim.SetFloat(hash.floatMovDirZ, speed.height);
//    }
//    protected virtual void ApplyMotionEnd()
//    {
//        //motor.MotionMov(anim.velocity);
//        //motor.MotionRot(anim.angularVelocity);
//    }

//    protected virtual void ApplyMotionSpeed(float velocity)
//    {
//        velocity /= anim.velocity.magnitude;

//        velocity.Normalize(1);
//        SetFloat(hash.floatFootSpeed, velocity.Clamp(0.2f, 2f));
//    }

//    protected virtual void ApplyMotion()
//    {

//    }
//    protected virtual void SetFloat(int id, float value)
//    {
//        anim.SetFloat(id, value);
//    }
//    #endregion

//    #region Initi

//    private void Reset()
//    {
//        //init in editor
//    }
//    public void Init(HumanoidAgent_Exhibit reference)
//    {

//        m_MonoUtility.InstanceComponent(ref _Animator, CharacterRoot);

//        hash = new AnimatorHash_Humanoid(anim);

//        FootState = Motion_Foot.Null;
//        inpv = new InpVariable();
//        chState = new CharacterState();

//        IK = reference.I.IK;

//        /*
//        if (!IK.Initialization(this))
//        {
//            Debug.LogError(IK + " Initialization Fail !");
//            return false;
//        }
//        */
//        //var rigstate = Agent.I.Motor as Interface_RigState;

//        //Agent.I.Motor.Event_Motion_Main += ApplyMotion_Main;
//        //Agent.I.Motor.Event_Motion_Foot += ApplyMotion_Foot;
//        //Agent.I.Motor.Event_Sprint += SetSprint;

//        //Agent.I.Motor.Event_Grounded += RigUpdate_Grounded;
//        //Agent.I.Motor.Event_RotateDegree += RigUpdate_RotateDegree;
//        //rigstate.Speed += ApplyMotionSpeed;
//        //rigstate.Velocity_Local += RigUpdate_LocalVelocity;
//        //waitLerp = new WaitForSeconds(updateStateRate);
//    }


//    #region Window

//    public void Standards(AnimationEvent @event)
//    {

//    }

//    public void ActionTargeted(int value)
//    {
//        Debug.LogWarning("Animate_Main " + value + " is Targeted !");

//        switch ((TargetedType)value)
//        {
//            case TargetedType.m_items:
//                break;
//            case TargetedType.Slot:
//                break;
//        }
//    }
//    public void MainActionTargeted(int value)
//    {
//        Debug.LogWarning("Animate_Main " + value + " is Targeted !");

//        switch ((TargetedType)value)
//        {
//            case TargetedType.m_items:
//                break;
//            case TargetedType.Slot:
//                break;
//        }
//    }
//    public void SecondActionTargeted(int value)
//    {
//        Debug.LogWarning("Animate_Second " + value + " is Targeted !");

//        switch ((TargetedType)value)
//        {
//            case TargetedType.m_items:
//                break;
//            case TargetedType.Slot:
//                break;
//        }
//    }



//    #endregion


//    #endregion

//    #region EventListener
//    public event Action<Vector3> BodyDirection;


//    protected bool Sprint;
//    protected virtual void SetSprint(bool value)
//    {
//        Sprint = value;

//    }



//    protected void ApplyAnimweight(object o, AnimConfigArg e)
//    {
//        switch (e.option)
//        {
//            case AnimConfigArg.packageOption.Null:
//                break;
//            case AnimConfigArg.packageOption.HandSecond:
//                anim.SetLayerWeight(hash.intHandState_second, e.weight);
//                break;
//            case AnimConfigArg.packageOption.ActionAll:
//                anim.SetLayerWeight(hash.LayerActionAll, e.weight);
//                break;
//        }

//    }

//    #region Catch
//    protected bool Moving;

//    #endregion

//    protected bool footActive = false;
//    protected bool grounded_Anim = true;
//    protected float movSpeed_Anim = 0;
//    protected float turnDegree_Anim = 0;
//    protected float AnimSpeedRate = 1f;
//    protected Vector2 movDir_Anim = Vector2.zero;


//    protected void RigUpdate_LocalVelocity(Vector3 loacaldir)
//    {
//        var v = new Vector2(loacaldir.width * AnimSpeedRate, loacaldir.z * AnimSpeedRate);
////        print(v);
//        if (v.Equals(movDir_Anim))
//            return;

//        if (LerpSpeed != null)
//        {
//            StopCoroutine(LerpSpeed);
//            UpdateAnimMovDir();
//        }

//        LerpSpeed = StartCoroutine(LerpSpeedPost(v));
//    }

//    public virtual void SpeedMultiply(float s)
//    {
//        if (float.IsNaN(s) || float.IsInfinity(s))
//            s = 1f;

//        var animS = anim.velocity.GetLength();

//        if (float.IsNaN(animS) || float.IsInfinity(animS) || animS.Equals(0f))
//            animS = 1f;

//        s /= animS;


//        if (s > 1f)
//            SetAnimSpeed++;
//        else if (s < 0.5f)
//            SetAnimSpeed--;

//        anim.SetFloat(hash.floatFootSpeed, s);
//    }



//    protected Coroutine LerpSpeed;
//    protected IEnumerator LerpSpeedPost(Vector2 localspeed)
//    {
//        if (!movDir_Anim.Equals(Vector2.zero))
//        {

//        }

//        float section = lerpTime;

//        while (section > float.Epsilon)
//        {
//            var speed = Vector2.Lerp(movDir_Anim, localspeed, 1 - (section / lerpTime));
//            anim.SetFloat(hash.floatMovDirX, speed.width);
//            anim.SetFloat(hash.floatMovDirZ, speed.height);
//            BodyDirection?.Invoke(speed);
//            yield return null;
//            section -= ConstCache.deltaTime;
//        }

//        anim.SetFloat(hash.floatMovDirX, localspeed.width);
//        anim.SetFloat(hash.floatMovDirZ, localspeed.height);
//        movDir_Anim = localspeed;
//        LerpSpeed = null;
//        yield break;
//    }
//    protected void UpdateAnimMovDir()
//    {
//        movDir_Anim.width = anim.GetFloat(hash.floatMovDirX);
//        movDir_Anim.height = anim.GetFloat(hash.floatMovDirZ);
//    }

//    protected void RigUpdate_Grounded(bool grounded)
//    {
//        if (!grounded_Anim.Equals(grounded))
//        {
//            grounded_Anim = grounded;
            
            
//            if (grounded)
//            {
//                if (ObsoleteMotionState.HasFlag(Motion_Main.Rebalance))
//                    return;
//                else
//                    ResetLayer(hash.LayerActionAll);
//                /*
//                if (ObsoleteMotionState.HasFlag(Motion_Main.Fall))
//                    FadeMainAction(hash.State_Land, 0.15f, 0);
//                else
//                    FadeMainAction(hash.State_Land, 0.3f, 0);
//                */
//            }
            
//        }

//        //anim.speed *= 1f / VC.Read(grounded_Anim.GetHashCode(),1f, true);

//    }

//    protected void RigUpdate_RotateDegree(float rot)
//    {
//        if (grounded_Anim)
//        {
//            if (!turnDegree_Anim.Equals(rot))
//            {
//                turnDegree_Anim = rot;
//                _Animator.SetFloat(hash.floatTurn, turnDegree_Anim);
//            }
//        }
//    }
//    public virtual void SetLayerWeight(int layer, float weight)
//    {
//        _Animator.SetLayerWeight(layer, weight);
//    }
//    public virtual void SetNormalizeTime(int layer, int state, float p)
//    {
//        _Animator.Play(state, layer, p);
//    }

//    protected virtual void ApplyMotion_Foot(Motion_Foot footState)
//    {
//        if (FootState.Equals(footState))
//            return;

//        IK.IK_Foot.LockFoot();
//        //Debug.LogError(ExcutionOderCatch.State);


//        switch (footState)
//        {
//            case Motion_Foot.Null:
//                //StartCoroutine(FadeFootMotion(hash.State_Idle, 0.35f, 0f));
//                IK.IK_Foot.EndPace(movDir_Anim);
//                ikManager.IK_Foot.Control_GetPaceLength = false;
//                break;
//            case Motion_Foot.Walk:
//                //anim.Play(hash.State_Locomotion, hash.LayerFoot);
//                //StartCoroutine(FadeFootMotion(hash.State_Locomotion, 0.35f, 0f));
//                anim.CrossFade(hash.State_Locomotion, 0.4f, hash.LayerFoot);
//                //.InvokeIK.Add(hash.LayerFoot, new Injector.Void(IK.IK_Foot.Stride));
//                IK.IK_Foot.Stride();
//                //setHeight
//                break;
//            case Motion_Foot.Squat:
//                break;
//            case Motion_Foot.Lie:
//                break;
//            case Motion_Foot.Swim:
//                break;
//            case Motion_Foot.Climb:
//                break;
//        }
//        // anim.SetInteger(hash.intFootState, (int)footState);
//        FootState = footState;
//    }


//    public MatchTargetWeightMask matchMask;

//    protected Motion_Main ObsoleteMotionState;
//    protected void ApplyMotion_Main(Motion_Main newstate)//main layer
//    {
//        //analyze
//        /*
//        var constant= ObsoleteMotionState & newstate;//get same
//        ObsoleteMotionState = newstate;// load new
//        constant = newstate^constant;//get different
//        */

//        if (ObsoleteMotionState.Equals(newstate))
//            return;


//        /*
//        switch (ObsoleteMotionState)
//        {
//            case Motion_Main.Null:

//                break;
//            case Motion_Main.Locomotion:

//                break;
//            case Motion_Main.Climb:
//                break;
//            case Motion_Main.Lie:
//                break;
//            case Motion_Main.Jump:

//                break;
//            case Motion_Main.Fall:

//                break;
//            case Motion_Main.Operate:
//                break;
//        }
//        */
  
//        switch (newstate)
//        {


//            case Motion_Main.Null:
//                ResetLayer(hash.LayerActionAll);

//                return;
//            case Motion_Main.Locomotion:
//                ResetLayer(hash.LayerActionAll);
//                return;

//            case Motion_Main.Climb:
//                break;
//            case Motion_Main.Lie:
//                break;
//            case Motion_Main.Jump:
//                MainAction_Jump();
//                break;
//            case Motion_Main.Fall: //to land? 
//                MainAction_Fall();
//                break;
//            case Motion_Main.Operate:
//                break;
//            case Motion_Main.Rebalance:
//                if (ObsoleteMotionState.Equals(Motion_Main.Rebalance))
//                    FadeMainAction(hash.State_Land, 0.25f, 0);

//                break;
//            default:
//                Debug.LogError("outofexception");
//                ResetLayer(hash.LayerActionAll);
//                return;
//        }

//        ObsoleteMotionState = newstate;
//    }
//    protected float LayerLerpTime=0.33f;

//    protected virtual void ResetLayer(int layerindex)
//    {
//        if (anim.GetLayerWeight(layerindex).Equals(0f))
//            return;

//        print(anim.GetLayerWeight(layerindex));
//        StartCoroutine(ApplyLayerWeight(layerindex, LayerLerpTime, 0f));
//    }


//    protected virtual void MainAction_Fall()
//    {
//        if (ObsoleteMotionState.Equals(Motion_Main.Jump))
//            FadeMainAction(hash.State_Fall, 0.25f, 0);
//    }

//    protected virtual void MainAction_Jump()
//    {

//        //if (motor.GetState(Motion_Main.Locomotion))
//        //{
//        //    FadeMainAction(hash.State_Jump, 0.15f, 0f);
//        //    if (motor.GetState(Motion_Main.Locomotion))
//        //    {
     
//        //        //anim.speed = VC.Reister(grounded_Anim.GetHashCode(),0.75f);
//        //        //slow motion &&  lost balance
//        //    }
//        //}
//        //else
//        //{

//        //    if (ObsoleteMotionState.Equals(Motion_Main.Jump))
//        //        FadeMainAction(hash.State_Jump, 0.15f, 0);
//        //    else
//        //        FadeMainAction(hash.State_Jump, 0.15f, 0f);
//        //}

//        //wait event to call back
//    }

//    IEnumerator FadeFootMotion(int statehash, float during, float offset)
//    {
//        yield return null;
//        Debug.LogError(ExcutionOderCatch.State);
//        anim.CrossFade(statehash, 0.15f, hash.LayerFoot, 0);
//    }
//    protected virtual void FadeMainAction(int statehash, float during, float offsset)
//    {
//        if (anim.GetCurrentAnimatorStateInfo(hash.LayerActionAll).shortNameHash.Equals(hash.State_Null))
//            anim.Play(statehash, hash.LayerActionAll);
//        else
//        anim.CrossFade(statehash, 0.15f, hash.LayerActionAll, 0);
//    }

//    #endregion

//    #region PublicCall

//    public void StartAnimate()
//    {
//        //?? Enable anim
//    }


//    #endregion

//    #region Rollback Unsubmit



//    #endregion

//    protected IEnumerator SpeedLerp(int HashTarget, float TargetSpeed)
//    {
//        float Start = anim.GetFloat(HashTarget);
//        float section = lerpTime;

//        for (float t = 0; t < section; t += ConstCache.deltaTime)
//        {
//            anim.SetFloat(HashTarget, Mathf.Lerp(Start, TargetSpeed, t / section));
//            yield return null;
//        }

//    }


//    protected Coroutine HandCoroutine = null;

//    protected void unLockLastAction()
//    {
//        switch (RHandState)
//        {
//            case ACList_Hand.defaultWait:
//              //  IK.StopIK(IKManager_Humanoid.IKState.Hand, 0.5f);
//                anim.SetLayerWeight(hash.LayerTop, 0);
//                break;
//            case ACList_Hand.Equip:
//                break;
//            case ACList_Hand.Release:
//                break;
//            case ACList_Hand.PickUp:
//                break;
//            case ACList_Hand.Aim:
//                break;
//        }
//    }


//    public void StartPrimeHandAction(ACList_Hand action)
//    {
//        if (action == RHandState)
//            return;
//        unLockLastAction();
//        anim.SetLayerWeight(hash.LayerHand_prime, 1);
//        anim.SetInteger(hash.intHandState_prime, (int)action);
//        print(action);
//        switch (action)
//        {
//            case ACList_Hand.defaultWait:
//                anim.SetLayerWeight(hash.LayerTop, 1);
//                break;
//            case ACList_Hand.PickUp:
//                /*
//                if (anim.GetBool(hash.boolMirror))
//                    ikManager.BuildIKMission(hash.LayerHand_prime, IKManager_Humanoid.IKPart.LeftHand, IKManager_Humanoid.IKFunction.CriticalBreak, 1, 1);
//                else
//                    ikManager.BuildIKMission(hash.LayerHand_prime, IKManager_Humanoid.IKPart.RightHand, IKManager_Humanoid.IKFunction.CriticalBreak, 1, 1);
//                break;
//                */
//            case ACList_Hand.Equip:
//                /*
//                if (anim.GetBool(hash.boolMirror))
//                    ikManager.BuildIKMission(hash.LayerHand_prime, IKManager_Humanoid.IKPart.LeftHand, IKManager_Humanoid.IKFunction.Null, 0, 1);
//                else
//                    ikManager.BuildIKMission(hash.LayerHand_prime, IKManager_Humanoid.IKPart.RightHand, IKManager_Humanoid.IKFunction.Null, 0, 1);
//                */
//                break;
   
//            case ACList_Hand.Aim:
//                break;
//            case ACList_Hand.Release:
//                break;
//            case ACList_Hand.Storage_Side:
//                break;
//            case ACList_Hand.Storage_Top:
//                break;
//            case ACList_Hand.Storage_Front:
//                break;
//            case ACList_Hand.Pickup_Front:
//                break;
//            case ACList_Hand.Pickup_Side:
//                break;
//            case ACList_Hand.Pickup_Top:
//                break;
//            case ACList_Hand.Use:

//                break;
//        }

//        RHandState = action;

//        if (HandCoroutine == null)
//        {

//            //HandCoroutine = StartCoroutine(EndClampAction(hash.LayerHand, hash.intHandAction));
//        }
//    }
//    public void StartSecondHandAction(ACList_Hand action)
//    {
//        if (action == LHandState)
//            return;
//        unLockLastAction();//?

//        print(action);
//        anim.SetLayerWeight(hash.LayerHand_second, 1);
//        anim.SetInteger(hash.intHandState_second, (int)action);
//        switch (action)
//        {
//            case ACList_Hand.defaultWait:
//                anim.SetLayerWeight(hash.LayerHand_second, 0);
//                break;
//                /*
//            case ACList_Hand.PickUp:
//                if (anim.GetBool(hash.boolMirror))
//                    ikManager.BuildIKMission(hash.LayerHand_second, IKManager_Humanoid.IKPart.RightHand, IKManager_Humanoid.IKFunction.CriticalBreak, 1, 1);
//                else
//                    ikManager.BuildIKMission(hash.LayerHand_second, IKManager_Humanoid.IKPart.LeftHand, IKManager_Humanoid.IKFunction.CriticalBreak, 1, 1);
//                break;
//            case ACList_Hand.Equip:

//                if (anim.GetBool(hash.boolMirror))
//                    ikManager.BuildIKMission(hash.LayerHand_second, IKManager_Humanoid.IKPart.RightHand, IKManager_Humanoid.IKFunction.Null, 0, 1);
//                else
//                    ikManager.BuildIKMission(hash.LayerHand_second, IKManager_Humanoid.IKPart.LeftHand, IKManager_Humanoid.IKFunction.Null, 0, 1);
//                break;
//                */
//            case ACList_Hand.Aim:
//                break;
//            case ACList_Hand.Release:
//                break;
//            case ACList_Hand.Storage_Side:
//                break;
//            case ACList_Hand.Storage_Top:
//                break;
//            case ACList_Hand.Storage_Front:
//                break;
//            case ACList_Hand.Pickup_Front:
//                break;
//            case ACList_Hand.Pickup_Side:
//                break;
//            case ACList_Hand.Pickup_Top:
//                break;
//            case ACList_Hand.Use:
//                break;
//        }

//        LHandState = action;

//        if (HandCoroutine == null)
//        {

//            //HandCoroutine = StartCoroutine(EndClampAction(hash.LayerHand, hash.intHandAction));
//        }
//    }

//    IEnumerator HandActionWaiter()
//    {
//        float t = 0.75f;
//        for (float i = 0; i < t; i += ConstCache.deltaTime)
//        {
//         //   IK.SetWeight(IKManager_Humanoid.IKPart.RightHand, 1 * i / t);
//            //Debug.LogWarning(i / t);
//            yield return null;
//        }

//        for (float i = 0; i < t; i += ConstCache.deltaTime)
//        {
//           // IK.SetWeight(IKManager_Humanoid.IKPart.RightHand, 1 - (1 * i / t));
//            yield return null;
//        }
//    }


//    IEnumerator EndClampAction(int layer, int action)
//    {
//        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(layer).tagHash == action);
//        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(layer).tagHash != action);
//        anim.SetInteger(action, 0);
//        anim.SetLayerWeight(layer, 0);
//    }
    
//    IEnumerator ApplyLayerWeight(int layerIndex,float time,float weight)
//    {
//        //print(System.Reflection.MethodBase.GetCurrentMethod().GetHashCode());
//        //print(System.Reflection.MethodBase.GetCurrentMethod().Name.GetHashCode());
//        using (var a = new CoroutineLock(System.Reflection.MethodBase.GetCurrentMethod().GetHashCode() + GetHashCode()))
//        {
//            yield return new WaitUntil(()=>a.MoveNext());

//            var lastWeight = anim.GetLayerWeight(layerIndex);

//            for (float i = 0; i < time && a.MoveNext(); i += ConstCache.deltaTime)
//            {
//                anim.SetLayerWeight(layerIndex, MathfUtility.Lerp(lastWeight, weight, i / time));
//                yield return null;
//            }

//            if(a.MoveNext())
//            {
//                anim.Play(hash.State_Null, layerIndex);
//                anim.SetLayerWeight(layerIndex, weight);
//            }
//        }
//    }
//}
