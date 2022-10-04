//using System.Collections;
//using UnityEngine;
//using UnityEngine.Animations.Rigging;
//using System;
//using Return.Humanoid.IK;
//using Return.Humanoid.Animation;

//namespace Return.Humanoid.IK
//{
//    [RequireComponent(typeof(IKManager_Humanoid))]
//    public class IK_Foot : HumanoidModularSystem
//    {

//        public LimbIKGoal RightFootGoal { get; protected set; }
//        public LimbIKGoal LeftFootGoal { get; protected set; }

//        public void Init(HumanoidAgent_Simulation agent)
//        {
//            var anim = agent.Animator;
//            var IAnimator = anim as IAnimator;
//            RightFootGoal = new LimbIKGoal(IAnimator,anim.IKGoalRootBone, AvatarIKGoal.RightLeg)
//            {
//                MaximumExtension =
//                 IAnimator.GetSkeletonBone(HumanBodyBones.RightLowerLeg).position.magnitude
//                +
//                 IAnimator.GetSkeletonBone(HumanBodyBones.RightLeg).position.magnitude
//            };

//            LeftFootGoal = new LimbIKGoal(IAnimator,anim.IKGoalRootBone, AvatarIKGoal.LeftLeg)
//            {
//                MaximumExtension =
//                 IAnimator.GetSkeletonBone(HumanBodyBones.LeftLowerLeg).position.magnitude
//                +
//                 IAnimator.GetSkeletonBone(HumanBodyBones.LeftLeg).position.magnitude
//            };

            

//        }

//        public float PaceLength_Walk = 0.289f;
//        public float PaceLength_Sprint = 0.45f;


//        #region outDate

//        #region Reference

//        protected IKManager_Humanoid _manager;

//        [SerializeField]
//        protected RigTransform rigTransform = null;
//        [SerializeField]
//        protected ReadOnlyTransform AnimRoot = null;
//        [SerializeField]
//        protected Constructure_Foot _R;
//        [SerializeField]
//        protected Constructure_Foot _L;

//        #endregion

//        #region AbstractValue
//        [SerializeField]
//        protected Quadrant StepLocation;

//        #region AvatarIK
//        public bool EnableFeetIK;

//        #endregion

//        #region Rigging
//        [SerializeField]
//        protected AnimationCurve RelayCurve;
//        public float CastSpacing;
//        public float CastSize = 0.2f;
//        public int SensorNumbers = 15;
//        protected RaycastHit[] Obstacles;
//        #endregion


//        public float StepDistance = 0.45f;//?load by rows*const
//        public float CastHeight;

//        public float StepHeight = 0.33f;
//        public float StepTime = 0.2f;

//        [SerializeField]
//        protected float FootLength = 0.5f;

//        #endregion

//        #region State
//        protected Side Pace_Current;

//        #endregion

//        #region Catch
//        /// <summary>
//        /// Which Foot is Front of Body
//        /// </summary>
//        protected Side Pace_Next;
//        protected bool _Grounded_Right;
//        protected bool _Grounded_Left;


//        protected Coroutine Step;
//        ReadOnlyTransform Mov;
//        ReadOnlyTransform Stay;
//        Vector3 Origin;
//        Vector3 RelayPoint;
//        Vector3 DropPoint;
//        Vector3 DropOffset;
//        Vector3 StayPoint;

//        Vector3 old_right;
//        Vector3 old_left;
//        #endregion

//        #region PublicWindows

//        public bool Grounded_Right { get { return _Grounded_Right; } }
//        public bool Grounded_Left { get { return _Grounded_Left; } }

//        #endregion


//        #region Init
//        public bool Init(IKManager_Humanoid manager)
//        {
//            _manager = manager;
//            var Anim = manager.Animator.GetAnimator;
//            EnableFeetIK = true;

//            Hips = Anim.GetBoneTransform(HumanBodyBones.Hips);
//            AnimRoot = rigTransform.transform;


//            _R.Feet = Anim.GetBoneTransform(HumanBodyBones.RightLeg);
//            _L.Feet = Anim.GetBoneTransform(HumanBodyBones.LeftLeg);

//            _R.Toes = Anim.GetBoneTransform(HumanBodyBones.RightToes);
//            _L.Toes = Anim.GetBoneTransform(HumanBodyBones.LeftToes);

//            _R.Target = _R.Rig.data.target;
//            _L.Target = _L.Rig.data.target;


//            _R.Offset_pos = _R.Target.position - _R.Feet.position;
//            _R.Offset_rot = _R.Feet.rotation * Quaternion.Inverse(_R.Target.rotation);
//            _L.Offset_pos = _L.Target.position - _L.Feet.position;
//            _L.Offset_rot = _L.Feet.rotation * Quaternion.Inverse(_L.Target.rotation);
//            //  Game-PhysicSetting
//            IKPhysicMask = GDR.AnimationIKMask;


//            //  Caculate AbstractValue Initi


//            Obstacles = new RaycastHit[SensorNumbers];

//            return true;

//        }

//        #endregion

//        protected IEnumerator ApplyWeight(Side side, float weight)
//        {
//            yield return null;


//            if (side.HasFlag(Side.Right))
//            {
//                _R.Rig.weight = weight;
//            }

//            if (side.HasFlag(Side.Left))
//            {
//                _L.Rig.weight = weight;
//            }

//        }
//        public virtual void LockFoot()
//        {
//            _R.Catch_Pos = _R.Feet.position + _R.Offset_pos;
//            _R.Catch_Rot = _R.Offset_rot * _R.Feet.rotation;
//            //_R.Rig.weight = 1;

//            _L.Catch_Pos = _L.Feet.position + _L.Offset_pos;
//            _L.Catch_Rot = _L.Offset_rot * _L.Feet.rotation;
//            //_L.Rig.weight = 1;
//        }



//        public IEnumerator LockStep()
//        {
//            Vector3 PosR = GetCastPoint(Side.Right);
//            Vector3 PosL = GetCastPoint(Side.Left);
//            Vector3 pos = AnimRoot.position;

//            float maxDistance = StepDistance * 0.5f;
//            maxDistance *= maxDistance;
//            ApplyWeight(Side.Left | Side.Right, 1);
//            while (Vector3.SqrMagnitude(AnimRoot.position - pos) < maxDistance)
//            {
//                print(Vector3.SqrMagnitude(AnimRoot.position - pos));
//                _R.Target.position = PosR;
//                _L.Target.position = PosL;
//                yield return null;
//            }
//            yield break;
//        }

//        protected virtual Vector3 GetCastPoint(Side side)
//        {
//            Vector3 pos;

//            if (side.HasFlag(Side.Right))
//                pos = _R.Feet.position;
//            else
//                pos = _L.Feet.position;

//            if (Physics.OnSensorUpdate(pos, -AnimRoot.up, out var hit, float.MaxValue, IKPhysicMask))
//            {
//                return hit.point;
//            }
//            else
//            {
//                return pos;
//            }
//        }
//        /// <summary>
//        /// 1.motor invoke foot motion
//        /// 2.animCTC call ik_foot record pos  
//        /// 3.animCTC play anim clip
//        /// 4.animCTC register anim event callback while OnAnimatorIK
//        /// 5.IK_Foot caculate locked foot step as side and length
//        /// 6.(if RightSide |LeftSide )locked length>PoseLength*0.85 ()=> go another()left>>switch offset data and set time 0.5
//        /// 7.
//        /// 8.
//        /// </summary>



//        public virtual void Stride()
//        {
//            if (bool_Stride)
//                return;

//            var dir_Start = _R.Catch_Pos - _L.Catch_Pos;
//            var dir_Stop = _R.Feet.position - _L.Feet.position;

//            var SqrPaceLength = dir_Stop.GetLength_Pow();

//            dir_Start = Vector3.ProjectOnPlane(dir_Start, AnimRoot.up).normalized;
//            dir_Stop = Vector3.ProjectOnPlane(dir_Stop, AnimRoot.up).normalized;

//            var movdir = Vector3.zero;//_manager.RigState.VelocityDirection_Global;
//            Pace_Next = CalMoveStep(movdir, dir_Start, out var stumble);

//            //AvatarIK.Enable_Rigging = EnableFeetIK;
//            //take clip catch or offset turn
//            /*
//            Debug.DrawLine(_R.Feet.position, _L.Feet.position, Color.red);
//            Debug.DrawLine(_R.Target.position+AnimRoot.up*0.01f, _L.Target.position + AnimRoot.up * 0.01f, Color.green);
//            Debug.Break();
//            */
//            Vector3 dropVector;
//            float MaxStepLength = 0;
//            if (stumble)
//            {
//                Debug.LogError(Pace_Next);
//                Pace_Next ^= Side.Right | Side.Left;
//                Debug.LogError(Pace_Next);
//                if (Pace_Next.HasFlag(Side.Left))
//                {//get clip time 0.5 pose?
//                    dropVector = AnimRoot.position + Vector3.Reflect(AnimRoot.position - _L.Feet.position, -AnimRoot.forward);
//                }
//                else
//                {
//                    dropVector = _R.Feet.position;
//                }
//                MaxStepLength = Mathf.Pow(PaceLength_Walk, 2);
//            }
//            else
//            {
//                if (Pace_Next.HasFlag(Side.Left))
//                {
//                    // dropVector = _R.Catch_Pos + _L.Feet.position-_R.Feet.position;
//                    dropVector = _L.Feet.position;
//                }
//                else
//                {
//                    //dropVector = _L.Catch_Pos + _R.Feet.position - _L.Feet.position;
//                    dropVector = _R.Feet.position;
//                }
//                MaxStepLength = SqrPaceLength;
//            }

//            if (Pace_Next.HasFlag(Side.Left))
//            {
//                Origin = _L.Catch_Pos;
//                StayPoint = _R.Catch_Pos;
//                Mov = _L.Target;
//                Stay = _R.Target;

//            }
//            else
//            {
//                Origin = _R.Catch_Pos;
//                StayPoint = _L.Catch_Pos;
//                Mov = _R.Target;
//                Stay = _L.Target;
//            }


//            StartCoroutine(ApplyWeight(Side.Left | Side.Right, 1));

//            dropVector -= AnimRoot.position;
//            dropVector = AnimRoot.InverseTransformVector(dropVector);
//            StartCoroutine(Cal_Stride(dropVector, MaxStepLength));
//        }

//        /*
//        public virtual void FootDirectionSlope(out float right,out float left)
//        {
//            Vector2 coordnate;
//            var dir = Vector3.ProjectOnPlane(_R.Feet.position - AnimRoot.position,AnimRoot.up);
//            var deg= Vector3.SignedAngle(AnimRoot.forward, dir, AnimRoot.up)/180;
//            right= deg;

//            dir= Vector3.ProjectOnPlane(_L.Feet.position - AnimRoot.position, AnimRoot.up);
//            deg = Vector3.SignedAngle(AnimRoot.forward, dir, AnimRoot.up) / 180;
//          left= deg;
//        }
//        */
//        private void OnDisable()
//        {
//            bool_Stride = false;
//        }
//        protected virtual void ClearStep()
//        {
//            StopCoroutine(Step);
//        }



//        protected Vector3 GetDropPoint(Side foot)
//        {
//            Vector3 go = Vector3.zero;
//            if (Side.Right.Equals(foot))
//            {
//                switch (Pace_Next)
//                {
//                    case Side.Right:
//                        go = StepLocation.First;
//                        break;
//                    case Side.Left:
//                        go = StepLocation.Second;
//                        break;
//                }

//            }
//            else if (Side.Left.Equals(foot))
//            {
//                switch (Pace_Next)
//                {
//                    case Side.Right:
//                        go = StepLocation.Fourth;
//                        break;

//                    case Side.Left:
//                        go = StepLocation.Third;
//                        break;
//                }
//            }
//            else
//            {
//                Debug.LogError("WorngBone");
//            }

//            return AnimRoot.TransformVector(go) + AnimRoot.position;

//        }
//        protected Vector3 GetDropPoint(ReadOnlyTransform foot, Side FinalFootState)
//        {
//            if (_R.Target.Equals(foot))
//            {
//                switch (FinalFootState)
//                {
//                    case Side.Right:
//                        return StepLocation.First;

//                    case Side.Left:
//                        return StepLocation.Second;
//                }

//            }
//            else if (_L.Target.Equals(foot))
//            {
//                switch (FinalFootState)
//                {
//                    case Side.Right:
//                        return StepLocation.Fourth;

//                    case Side.Left:
//                        return StepLocation.Third;
//                }
//            }
//            else
//            {
//                Debug.LogError("WorngBone");
//            }

//            return AnimRoot.position;

//        }


//        /// <summary>
//        /// keep distance
//        /// </summary>
//        public float StepSafe = 0.1f;

//        protected virtual Vector3 CalRelayPoint(Vector3 StartPoint, Vector3 EndPoint, float rows)
//        {
//            return Vector3.Lerp(StartPoint, EndPoint, 0.7f) + AnimRoot.up * rows;

//            //var cols=Physics.OverlapSphere(AnimRoot.position, StepDistance * 1.5f,ObstacleMask);



//            var count = Physics.SphereCastNonAlloc(StartPoint, CastSize, (EndPoint - StartPoint).normalized, Obstacles, Vector3.Distance(StartPoint, EndPoint), IKPhysicMask);
//            Vector3 relayPoint = Vector3.Lerp(StartPoint, EndPoint, 0.7f) + AnimRoot.up * StepHeight;


//            return relayPoint;

//            float ob_RestHeight = StepHeight;
//            Vector3 castDir = AnimRoot.up * -StepHeight;

//            for (int i = 0; i < count; i++)
//            {
//                if (Physics.SphereCast(Obstacles[i].point - castDir, CastSize, castDir, out Obstacles[i], StepHeight, IKPhysicMask))
//                    if (ob_RestHeight > Obstacles[i].distance)
//                    {
//                        ob_RestHeight = Obstacles[i].distance;
//                        relayPoint = Obstacles[i].point;
//                    }
//            }

//            if (ob_RestHeight < StepHeight)
//                relayPoint += AnimRoot.up * (StepHeight - ob_RestHeight);

//            return relayPoint;
//        }
//        public bool Control_GetPaceLength = false;
//        public int SamplingTimes = 4;
//        //public IEnumerator GetPaceLength(Injector.m_Value<float> callback)
//        //{
//        //    if (Control_GetPaceLength)
//        //        yield break;
//        //    else
//        //        Control_GetPaceLength = true;

//        //    using (var time = AvatarIK.GetProgress())
//        //    {
//        //        float t;
//        //        int length = SamplingTimes;

//        //        for (int i = 0; i < length && Control_GetPaceLength; i++)
//        //        {
//        //            t = float.Epsilon;
//        //            while (time.value > t)
//        //            {
//        //                t = time.value;
//        //                yield return null;
//        //            }

//        //            float distance = 0;//Vector3.Project(_R.Feet.position - _L.Feet.position,_manager.RigState.VelocityDirection_Global).magnitude;
//        //            callback?.Invoke(distance);

//        //            yield return null;
//        //        }

//        //    }

//        //    Control_GetPaceLength = false;
//        //}

//        Vector3 stride_start;
//        Vector3 stride_relay;
//        Vector3 stride_stop;

//        public bool bool_Stride;


//        public virtual IEnumerator Cal_Stride(Vector3 MovOffset, float maxDistance)
//        {


//            //bool_Stride = true;
//            stride_start = Origin;
//            float stayLimit;

//            //using (var time = AvatarIK.GetProgress())
//            //{
//            //    //Time.timeScale = 0.1f;
//            //    Vector3 lastRootPos = AnimRoot.position;
//            //    Vector3 final;

//            //    for (float p = float.Epsilon, verify = float.Epsilon; bool_Stride;)
//            //    {
//            //        try
//            //        {

//            //            final = AnimRoot.position + AnimRoot.TransformVector(MovOffset);
//            //            if (Physics.OnSensorUpdate(final, -AnimRoot.up, out var hit, float.MaxValue, IKPhysicMask))
//            //                final = hit.point;

//            //            if (true)//(Vector3.Dot(_manager.RigState.VelocityDirection_Global, Stay.position - AnimRoot.position) < 0)
//            //            {
//            //                stayLimit = Hips.localPosition.height;
//            //                var normal = Vector3.Cross(Hips.position - AnimRoot.position, StayPoint - AnimRoot.position);
//            //                verify = Vector3.Angle(Vector3.ProjectOnPlane(AnimRoot.position - Hips.position, normal), Vector3.ProjectOnPlane(StayPoint - Hips.position, normal));
//            //                verify /= Mathf.Acos(stayLimit / FootLength) * Mathf.Rad2Deg;
//            //                // print(verify+"***"+ Mathf.Acos(stayLimit / FootLength) * Mathf.Rad2Deg);
//            //            }

//            //            if (p < verify)
//            //                p = verify;
//            //            // print(p);
//            //            if (p > 1)
//            //                break;
//            //            else
//            //                final = Cal_RelayPoint(Origin, final, p);

//            //            //p = (final - StayPoint).GetLength_Pow() / maxDistance;
//            //            //if (p < time.value)

//            //            stride_stop = final;

//            //            Mov.position = final;
//            //            Stay.position = StayPoint;

//            //            stride_relay = final;
//            //            //RelayPoint.Vector3_Bezier(Origin, DropPoint + AnimRoot.TransformVector(DropOffset), t);
//            //            p = time.value * 2;
//            //        }
//            //        catch (Exception e)
//            //        {
//            //            print(e);
//            //            throw;

//            //        }
//            //        yield return null;


//            //    }
//            //    //Time.timeScale = 1f;

//            //    AvatarIK.Enable_Rigging = false;

//                if (bool_Stride)//stride second
//                {
//                    bool_Stride = false;
//                    //  var clipoffset = CalFadeStep(_manager.RigState.VelocityDirection_Global);
//                    // _manager.ctc.SetNormalizeTime(_manager.ctc.hash.LayerFoot, _manager.ctc.hash.State_Locomotion, clipoffset);

//                    StartCoroutine(ApplyWeight(Side.Right | Side.Left, 0.1f, 0));

//                }
//                else//endpace
//                {

//                }
//                /*
//                var applyLayer = new Injector.m_Value<int, float>(_manager.ctc.SetLayerWeight);


//                for (float i = 0; i < 0.25; i += ConstCache.deltaTime)
//                {
//                    applyLayer.Invoke(_manager.ctc.hash.LayerFoot, 1 * i / 0.25f);
//                    yield return null;
//                }
//                */
//                yield break;
            


//        }

//        protected Vector3 Cal_RelayPoint(Vector3 start, Vector3 stop, float p)
//        {
//            return AnimRoot.up * (StepHeight * StepHeightCurve.Evaluate(p)) + Vector3.Lerp(start, stop, p);
//        }

//        protected IEnumerator ApplyWeight(Side side, float duringTime, float endWeight)
//        {
//            float weightR = _R.Rig.weight;
//            float weightL = _L.Rig.weight;

//            for (float t = 0; t < duringTime && !bool_Stride; t += ConstCache.deltaTime)
//            {
//                var k = t / duringTime;

//                if (side.HasFlag(Side.Right))
//                    _R.Rig.weight = MathfUtility.Lerp(weightR, endWeight, k);
//                if (side.HasFlag(Side.Left))
//                    _L.Rig.weight = MathfUtility.Lerp(weightL, endWeight, k);
//                yield return null;
//            }
//            _R.Rig.weight = endWeight;
//            _L.Rig.weight = endWeight;

//            yield break;
//        }

//        /*
//        protected IEnumerator MovStep(ReadOnlyTransform mov, ReadOnlyTransform stay)
//        {
//            ApplyWeight(Side.Left | Side.Right, 1);
//            Vector3 Origin = mov.position;
//            Vector3 stayPoint = stay.position;
//            Vector3 DropPoint;
//            Vector3 DropOffset = GetDropPoint(mov, Pace_Next);
//            Vector3 relayPoint = CalRelayPoint(Origin, AnimRoot.position + AnimRoot.TransformVector(DropOffset));


//            for (float t = 0; t < StepTime; t += ConstCache.deltaTime)
//            {
//                DropPoint = AnimRoot.position + AnimRoot.TransformVector(DropOffset);
//                mov.position = relayPoint.Vector3_Bezier(Origin, DropPoint, t / StepTime);
//                stay.position = stayPoint;
//                yield return null;
//            }

//            // yield return StartCoroutine(FinishStep(stay, root.position + root.TransformVector(GetDropPoint(stay, PlaceState))));

//            ApplyWeight(Side.Right | Side.Left, 0);

//        }
//        protected IEnumerator FinishStep(ReadOnlyTransform mov, Vector3 DropPoint)
//        {
//            Vector3 origin = mov.position;
//            Vector3 relayPoint = CalRelayPoint(origin, DropPoint);
//            for (float t = 0; t < 0.2f; t += ConstCache.deltaTime)
//            {
//                mov.position = relayPoint.Vector3_Bezier(origin, DropPoint, t / 0.2f);
//                yield return null;
//            }
//            mov.position = DropPoint;
//            yield break;
//        }
//        */

//        #region Cal
//        /// <summary>
//        /// Enter mov direction to get next moving foot
//        /// </summary>
//        protected Side CalMoveStep(Vector3 movDir, Vector3 Dir_Start, out bool stumble)
//        {
//            var dodo = Vector3.Dot(movDir, Dir_Start);
//            Side side;
//            Vector3 origin;
//            Vector3 target;
//            Constructure_Foot mov;
//            //mov the one is behind
//            if (dodo > 0)
//            {
//                side = Side.Left;
//                mov = _L;
//            }
//            else
//            {
//                side = Side.Right;
//                mov = _R;
//            }

//            if (dodo == 0)
//                Debug.LogError("Step Default (Right Feet)!");



//            //  bool verify;     //cal cross middle rows
//            var dirR = _R.Feet.position - _R.Catch_Pos;
//            var dirL = _L.Feet.position - _L.Catch_Pos;
//            Debug.DrawLine(_R.Feet.position, _R.Catch_Pos, Color.red);
//            Debug.DrawLine(_L.Feet.position, _L.Catch_Pos, Color.red);
//            Debug.DrawRay(_R.Feet.position, Vector3.up, Color.red);
//            Debug.DrawRay(_L.Feet.position, Vector3.up, Color.red);
//            //Debug.Break();
//            stumble = Vector3.Angle(dirR, dirL) > 40f;
//            /*
//            origin = Vector3.ProjectOnPlane(mov.Catch_Pos - AnimRoot.position, AnimRoot.up).normalized;
//            target = Vector3.ProjectOnPlane(mov.Feet.position - AnimRoot.position, AnimRoot.up).normalized;

//            verify = Vector3.Dot(AnimRoot.up, Vector3.Cross(movDir, origin)) > 0;

//            if (Vector3.Dot(AnimRoot.up, Vector3.Cross(movDir, target)) > 0)
//            {
//                if (!verify&&Vector3.Angle(origin, target) > 45f)
//                {
//                    side ^= Side.Right | Side.Left;
//                    stumble = true;
//                }
//            }
//            else if (verify && Vector3.Angle(origin, target) > 45f)
//            {
//                side ^= Side.Right | Side.Left;
//                stumble = true;
//            }
//            */
//            return side;

//        }
//        protected float CalFadeStep(Vector3 direction)
//        {
//            var dir = Vector3.ProjectOnPlane((_R.Feet.position - _L.Feet.position).normalized, Vector3.up);
//            var dodo = Vector3.Dot(direction, dir);

//            if (dodo < 0)
//            {
//                return 0.5f;
//            }
//            else if (dodo > 0)
//            {
//                return 0f;
//            }
//            else
//            {
//                Debug.LogError(0);
//                return 0f;
//            }
//        }

//        #endregion

//        #region EndingPace

//        public virtual void EndPace(Vector2 dir)
//        {
//            if (bool_Stride)
//                bool_Stride = false;

//            return;

//            dir.Normalize();

//            Vector3 dir_right = _R.Feet.position - AnimRoot.position;
//            Vector3 dir_left = _L.Feet.position - AnimRoot.position;
//            Vector3 pos;

//            ///<summary >
//            ///
//            /// </summary>

//            if (Vector3.Dot(dir, dir_right.normalized) > 0)
//            {
//                pos = Vector3.Reflect(dir_left, -AnimRoot.up);
//                pos += AnimRoot.position;
//                if (Physics.OnSensorUpdate(pos, -AnimRoot.up, out var hit, IKPhysicMask))
//                    pos = hit.point;
//                _R.Rig.data.target.position = pos;
//                _R.Rig.weight = 1f;
//                print("Right");
//            }

//            if (Vector3.Dot(dir, dir_left.normalized) > 0)
//            {
//                pos = Vector3.Reflect(dir_right, -AnimRoot.up);
//                pos += AnimRoot.position;
//                if (Physics.OnSensorUpdate(pos, -AnimRoot.up, out var hit, IKPhysicMask))
//                    pos = hit.point;
//                _L.Rig.data.target.position = pos;
//                _L.Rig.weight = 1f;
//                print("Left");
//            }

//            //        Debug.LogError("None valid Foot IK");
//        }

//        #endregion

//        private Return.Definition.FootStepArg arg;
//        bool bool_coroutine_ApplyWeight_R;
//        bool bool_coroutine_ApplyWeight_L;
//        private Vector3 worldPoint;
//        IEnumerator LockIKPos()
//        {
//            worldPoint = Vector3.zero;
//            yield return ConstCache.WaitForFixedUpdate;
//            switch (arg.GroundedSide)
//            {
//                case Side.Left:
//                    if (Physics.OnSensorUpdate(_L.Feet.position, Vector3.down, out _hit, IKPhysicMask))
//                    {
//                        var Lpos = _hit.point;
//                        _L.Target.position = Lpos;
//                        _L.Rig.weight = 1;
//                        _R.Rig.weight = 0;

//                        worldPoint = Lpos;
//                    }
//                    else
//                    {
//                        _L.Rig.weight = 0;
//                        Debug.LogError("Null Cast Left !");
//                    }
//                    if (!worldPoint.Equals(Vector3.zero))
//                    {
//                        if (bool_coroutine_ApplyWeight_L)
//                            StopCoroutine(LockLeft());

//                        StartCoroutine(LockLeft());
//                    }


//                    break;
//                case Side.Right:
//                    if (Physics.OnSensorUpdate(_R.Feet.position, Vector3.down, out _hit, IKPhysicMask))
//                    {
//                        var Rpos = _hit.point;
//                        _R.Target.position = Rpos;
//                        _R.Rig.weight = 1;
//                        _L.Rig.weight = 0;

//                        worldPoint = Rpos;
//                    }
//                    else
//                    {
//                        _R.Rig.weight = 0;
//                        Debug.LogError("Null Cast Right !");
//                    }

//                    if (!worldPoint.Equals(Vector3.zero))
//                    {
//                        if (bool_coroutine_ApplyWeight_R)
//                            StopCoroutine(LockRight());

//                        StartCoroutine(LockRight());
//                    }

//                    break;
//            }
//        }
//        IEnumerator LockLeft()
//        {
//            bool_coroutine_ApplyWeight_L = true;
//            float rot = AnimRoot.localEulerAngles.height;
//            rot = rot > 180 ? rot - 360 : rot;
//            float newRot = 0;
//            for (; bool_coroutine_ApplyWeight_L;)
//            {
//                yield return ConstCache.WaitForFixedUpdate;
//                _L.Target.position = worldPoint;

//                newRot = AnimRoot.localEulerAngles.height;
//                newRot = newRot > 180 ? rot - (newRot - 360) : rot - newRot;

//                if (newRot > 30 || newRot < -30)
//                    break;
//            }

//            if (bool_coroutine_ApplyWeight_L)
//                yield return StartCoroutine(ReleaseL());

//            bool_coroutine_ApplyWeight_L = false;
//        }
//        IEnumerator ReleaseL()
//        {
//            if (_L.Rig.weight == 0)
//                yield break;
//            for (float t = 0; t < 0.1f && bool_coroutine_ApplyWeight_L; t += ConstCache.fixeddeltaTime)
//            {
//                yield return ConstCache.WaitForFixedUpdate;
//                _L.Rig.weight -= t / 0.1f;
//            }
//            _L.Rig.weight = 0;
//        }
//        IEnumerator LockRight()
//        {
//            bool_coroutine_ApplyWeight_L = true;
//            float rot = AnimRoot.localEulerAngles.height;
//            rot = rot > 180 ? rot - 360 : rot;
//            float newRot = 0;
//            for (; bool_coroutine_ApplyWeight_R;)
//            {
//                yield return ConstCache.WaitForFixedUpdate;
//                _R.Target.position = worldPoint;

//                newRot = AnimRoot.localEulerAngles.height;
//                newRot = newRot > 180 ? rot - (newRot - 360) : rot - newRot;

//                if (newRot > 30 || newRot < -30)
//                    break;
//            }

//            if (bool_coroutine_ApplyWeight_R)
//                yield return StartCoroutine(ReleaseR());

//            bool_coroutine_ApplyWeight_R = false;
//        }
//        IEnumerator ReleaseR()
//        {
//            if (_R.Rig.weight == 0)
//                yield break;
//            for (float t = 0; t < 0.1f && bool_coroutine_ApplyWeight_R; t += ConstCache.fixeddeltaTime)
//            {
//                yield return ConstCache.WaitForFixedUpdate;
//                _R.Rig.weight -= t / 0.1f;
//            }
//            _R.Rig.weight = 0;
//        }


//        public bool IKState { get { if (_R.Rig.weight == 1 || _L.Rig.weight == 1) return true; else return false; } }


//        private RaycastHit _hit;




//        #region Debug
//        public bool _Debug = false;
//        private void OnDrawGizmosSelected()
//        {
//            if (!_Debug)
//                return;

//            ReadOnlyTransform tf = rigTransform.transform;
//            Vector3 pos = tf.position;
//            Gizmos.color = Color.green;
//            Gizmos.DrawSphere(tf.TransformVector(StepLocation.Second) + pos, 0.01f);
//            Gizmos.DrawSphere(tf.TransformVector(StepLocation.First) + pos, 0.01f);
//            Gizmos.DrawSphere(tf.TransformVector(StepLocation.Third) + pos, 0.01f);
//            Gizmos.DrawSphere(tf.TransformVector(StepLocation.Fourth) + pos, 0.01f);

//            if (worldPoint != Vector3.zero)
//            {
//                Gizmos.color = Color.red;
//                Gizmos.DrawWireSphere(worldPoint, 0.1f);
//                Gizmos.color = Color.blue;
//            }

//            Gizmos.color = Color.green;
//            Gizmos.DrawRay(_R.Feet.position, (Hips.position - _R.Feet.position) * FootLength);

//            Gizmos.color = Color.blue;
//        }
//        private void OnDrawGizmos()
//        {
//            if (!_Debug)
//                return;
//            //Draw Feet IK Capsule
//            var feet_r = _L.Feet;
//            var toes_R = _L.Toes;


//            Gizmos.color = Color.yellow;
//            Gizmos.DrawWireSphere(feet_r.position + feet_r.TransformVector(CastOffset_Feet), CapsuleRadius);
//            Gizmos.DrawWireSphere(toes_R.position + toes_R.TransformVector(CastOffset_Toes), CapsuleRadius);


//            Gizmos.color = Color.red;
//            Gizmos.DrawSphere(hitPoint, 0.01f);

//            Gizmos.color = Color.green;
//            Gizmos.DrawSphere(feetOri, 0.01f);
//            Gizmos.DrawSphere(toesOri, 0.01f);

//            Gizmos.color = Color.blue;
//            Gizmos.DrawSphere(feetfin, 0.01f);
//            Gizmos.DrawSphere(toesfin, 0.01f);

//            Gizmos.color = Color.white;
//            Gizmos.DrawLine(feetfin, feetOri);
//            Gizmos.DrawLine(toesfin, toesOri);

//            Gizmos.color = Color.blue;
//            Gizmos.DrawSphere(stride_start, 0.03f);
//            Gizmos.color = Color.red;
//            Gizmos.DrawSphere(stride_stop, 0.03f);
//            Gizmos.color = Color.white;
//            Gizmos.DrawSphere(stride_relay, 0.03f);
//            Gizmos.color = Color.yellow;
//            Gizmos.DrawLine(stride_start, stride_relay);
//            Gizmos.DrawLine(stride_relay, stride_stop);
//        }


//        Vector3 hitPoint = Vector3.zero;
//        Vector3 feetOri = Vector3.zero;
//        Vector3 toesOri = Vector3.zero;
//        Vector3 feetfin = Vector3.zero;
//        Vector3 toesfin = Vector3.zero;

//        public Vector3 GizmosSize = new Vector3(0.1f, 0.1f, 0.1f);
//        protected Vector3[] RecordPoints_R = new Vector3[60];
//        #endregion

//        #region Catch
//        [SerializeField]
//        protected ReadOnlyTransform Hips;

//        #endregion


//        #region Adapt Setting
//        /// <summary>
//        /// Make this gizmos collider middle of ankle and align sole
//        /// </summary>
//        public Vector3 CastOffset_Feet;
//        /// <summary>
//        /// Make this gizmos collider middle of toes and align sole
//        /// </summary>
//        public Vector3 CastOffset_Toes;
//        /// <summary>
//        /// Size of sensor's capsule radius
//        /// </summary>
//        public float CapsuleRadius;

//        #endregion
//        public float MaxStepHeight;
//        public LayerMask IKPhysicMask;
//        public LayerMask ObstacleMask;

//        #region Flat IK
//        [SerializeField]
//        protected AnimationCurve StepHeightCurve;
//        /// <summary>
//        /// Caculate feet position every frame, make them above ground and obstacle
//        /// </summary>

//        public void Cal_Normal(float normalTime, out Vector3 rfp, out Quaternion rfr, out Vector3 lfp, out Quaternion lfr)
//        {
//            Vector3 dir = -AnimRoot.up;

//            Vector3 pos;
//            Vector3 normal;


//            if (Cast(_R.Feet.position + _R.Feet.TransformVector(CastOffset_Feet), _R.Toes.position + _R.Toes.TransformVector(CastOffset_Toes), dir, out pos, out normal))
//            {
//                rfp = pos - _R.Feet.TransformVector(CastOffset_Feet);
//                rfr = Quaternion.LookRotation(Vector3.Cross(normal, _R.Feet.right), normal);
//            }
//            else
//            {
//                Debug.LogError("RightNullGrounded");
//                rfp = _R.Feet.position;
//                rfr = _R.Feet.rotation;
//            }


//            if (Cast(_L.Feet.position + _L.Feet.TransformVector(CastOffset_Feet), _L.Toes.position + _L.Toes.TransformVector(CastOffset_Toes), dir, out pos, out normal))
//            {
//                lfp = pos - _L.Feet.TransformVector(CastOffset_Feet);
//                lfr = Quaternion.LookRotation(Vector3.Cross(normal, _L.Feet.right), normal);
//            }
//            else
//            {
//                Debug.LogError("LeftNullGrounded");
//                lfp = _L.Feet.position;
//                lfr = _L.Feet.rotation;
//            }
//        }
//        protected virtual bool Cast(Vector3 foot, Vector3 toes, Vector3 dir, out Vector3 FootPoint, out Vector3 hitNormal)
//        {
//            feetOri = foot;
//            toesOri = toes;

//            var footlength = Vector3.Dot(AnimRoot.up, Hips.position - foot);
//            var offset = dir * -footlength;
//            if (Physics.CapsuleCast(foot + offset, toes + offset, CapsuleRadius, dir, out var hit_foot, float.MaxValue, IKPhysicMask))
//            {
//                if (hit_foot.distance > footlength + 0.05f)//None IK above obstacle
//                {
//                    FootPoint = foot;
//                    hitNormal = hit_foot.normal;
//                    return true;
//                }
//                else if (hit_foot.distance < Hips.localPosition.height + MaxStepHeight)//Grounded
//                {

//                    feetfin = feetOri + dir * hit_foot.distance + offset;
//                    toesfin = toesOri + dir * hit_foot.distance + offset;
//                    FootPoint = foot + dir * hit_foot.distance + offset;
//                    hitNormal = hit_foot.normal;
//                    hitPoint = hit_foot.point;
//                    Debug.DrawRay(toes + dir * hit_foot.distance + offset, -hit_foot.distance * dir);
//                    Debug.DrawRay(FootPoint, -hit_foot.distance * dir);
//                    return true;
//                }
//                else
//                {

//                }

//                if (Vector3.Dot(dir, (hit_foot.point - foot)) > 0)
//                //reset()=>obs top on foot 
//                {
//                    //hitPoint = ray.GetPoint(hit.distance - SafeSpace);
//                }
//                //none()=>foot top on obs
//                else
//                {

//                }




//            }

//            FootPoint = Vector3.zero;
//            hitNormal = Vector3.zero;
//            return false;
//        }

//        #endregion



//    }

//    #endregion
//}

//[Serializable]
//public class Constructure_Foot : Constructure_Humanoid
//{
//    public ReadOnlyTransform Feet;
//    public ReadOnlyTransform Toes;
//}

//public abstract class Constructure_Humanoid
//{
//    public TwoBoneIKConstraint Rig;
//    [HideInInspector]
//    public ReadOnlyTransform Target;
//    public Vector3 Offset_pos;
//    public Quaternion Offset_rot;
//    public Vector3 Catch_Pos;
//    public Quaternion Catch_Rot;
//}
