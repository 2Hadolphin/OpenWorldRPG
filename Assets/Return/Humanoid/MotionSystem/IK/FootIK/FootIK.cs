//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;
//using KinematicCharacterController;
//using UnityEngine.Assertions;
//using UnityEngine.Animations;
//using UnityEngine.Playables;
//using Return.Humanoid.Motion;

//namespace Return.Humanoid.IK
//{
//    public class FootIK : MonoBehaviour
//    {
//        #region Job

//        FootIKJob IKJob;
//        AnimationScriptPlayable IKScriptPlayable;

//        GetBodyPositionAnimPose Body;

//        GetGoalAnimPose GetRightFoot;
//        GetGoalAnimPose GetLeftFoot;

//        SetGoal SetRightFoot;
//        SetGoal SetLeftFoot;

//        public AnimationScriptPlayable CreateJob(PlayableGraph graph)
//        {
//            Body = new GetBodyPositionAnimPose();

//            // get
//            GetRightFoot = new GetGoalAnimPose() { Goal = AvatarIKGoal.RightLeg };
//            GetLeftFoot = new GetGoalAnimPose() { Goal = AvatarIKGoal.LeftLeg };

//            // set
//            SetRightFoot = new() { Goal = AvatarIKGoal.RightLeg };
//            SetLeftFoot = new() { Goal = AvatarIKGoal.LeftLeg };



//            IKJob = new FootIKJob()
//            {
//                CatchJobs = new[] { GetRightFoot, GetLeftFoot },
//                SetGoalJobs = new[] { SetRightFoot, SetLeftFoot },

//                Body = Body,

//            };

//            IKScriptPlayable = AnimationScriptPlayable.Create(graph, IKJob, 1);
//            return IKScriptPlayable;
//        }


//        #endregion


//        [System.Serializable]
//        public class AnimatorStateEntry { public int layer; public string animatorState; }

//        [Header("INFO: Please activate IK pass in the Animator (base layer recommended)")]

//        [Header("General")]
//        [SerializeField, Tooltip("Please activate the anim IK pass on a specific layer - typically it is most likely best to activate it for the base layer. This is an requirement for getting OnAnimatorIK called on this script (otherwise the script is not going to work). This variable defines the index of the specific layer where the IK pass is activated and will be used by the script.")]
//        protected int m_animatorIkPassLayerIndex = 0;
//        [SerializeField, Tooltip("The Collision-Layer for foot and body placement detection.")]
//        LayerMask m_collisionLayerMask = 1 << 0;
//        [SerializeField, Tooltip("The collision-setting for trigger-interactions for foot and body placement detection. Default setting is 'Ignore'.")]
//        QueryTriggerInteraction m_triggerCollisionInteraction = QueryTriggerInteraction.Ignore;
//        [SerializeField, Distance(0.05f, 10.0f), Tooltip("The speed at which the positions and rotations interpolate to the respective default values inbetween valid / grounded and invalid / not grounded state changes.")]
//        protected float m_speedForInterpolatingToRespectiveDefaultValues = 0.25f;
//        [SerializeField, Tooltip("A list of all anim states (in the anim controller), that will disable this effect when they are active. layer == anim layer. anim state == anim state name.")]
//        protected List<AnimatorStateEntry> m_forceInvalidOnAnimatorStates;

//        [Header("Foot IK placement")]
//        [SerializeField, Distance(0, 1), Tooltip("The strength that the effects will be applyed to.")]
//        protected float m_ikPlacementWeight = 1;
//        [SerializeField, Distance(1, 50), Tooltip("This value is used to smoothly lerp IK positions. The higher the value, the stiffer the transition. The smaller the value, the smoother the transition.")]
//        protected float m_ikPlacementStiffness = 10.0f;
//        [SerializeField, Distance(1, 50), Tooltip("This value is used to smoothly slerp IK rotations. The higher the value, the stiffer the transition. The smaller the value, the smoother the transition.")]
//        protected float m_ikRotationStiffness = 6.0f;
//        [SerializeField, Distance(0, 5), Tooltip("The IK extrapolation defines how much the feet overshoots when trying to achive the IK target position. A value > 0 will lead to a more robust and solid transition. A too high value will lead to stiff looking transitions.")]
//        protected float m_ikExtrapolation = 1.0f;
//        [SerializeField, Tooltip("The maximum IK correction for placing the feet.")]
//        protected float m_ikMaxCorrection = 0.5f;
//        [SerializeField, Tooltip("This value defines the distance between IK position and the foot ground-point. It's like the rows of the foot. This value is not allowed to be zero or smaller zero.")]
//        protected float m_ikFootHeight = 0.1f;
//        [SerializeField, Tooltip("The IK foot size defines the length of the foot used for the simulation model. This value is not allowed to be zero or smaller zero.")]
//        protected float m_ikFootLength = 0.24f;
//        [SerializeField, Tooltip("The IK foot lines defines the lines of the foot used for the simulation model. This value is not allowed to be zero or smaller zero.")]
//        protected float m_ikFootWidth = 0.085f;
//        [SerializeField, Tooltip("The IK foot forward bias defines the offset for the foots raycast origin in relation to the given IK position")]
//        protected float m_ikFootForwardBias = 0.065f;
//        [SerializeField, Distance(0, 250), Tooltip("This is an advanced setting with which intersections, when the feet move downwards, are stopped. A low value will let the feet bump inside the ground for a short period of time.")]
//        protected float m_antiDownwardsIntersectionStiffness = 50.0f;
//        [SerializeField, Distance(0, 90), Tooltip("This is an advanced setting where if this value is exceeded, the detected ground won't be handled as detected anymore.")]
//        protected float m_maxGroundAngleAtWhichTheGroundIsDetectedAsGround = 81.0f;
//        [SerializeField, Distance(0, 90), Tooltip("This is an advanced setting with which you can setup the maximum allowed angle the feet will adapt to.")]
//        protected float m_maxGroundAngleTheFeetsCanAdaptTo = 79.0f;
//        [SerializeField, Tooltip("If this value is true, rotations will be \"unlocked\" to their original rotation when not glued to the ground. If this value is false the foot rotation will try to adapt to the ground whenever grounded. The recommended value for having better transitions is typically true. [Default: true]")]
//        protected bool m_onlyAdaptRotationWhenGluedToGround = true;

//        [Header("Body placement")]
//        [SerializeField, Distance(0, 1), Tooltip("The strength that the effects will be applyed to.")]
//        protected float m_bodyPlacementWeight = 1;
//        [SerializeField, Distance(1, 50), Tooltip("This value is used to smoothly lerp body position transitions. The higher the value, the stiffer the transition. The smaller the value, the smoother the transition.")]
//        protected float m_bodyStiffness = 6.5f;
//        [SerializeField, Tooltip("The maximum possible body position correction.")]
//        protected float m_bodyPositionMaxCorrection = 0.7f;
//        [SerializeField, Tooltip("If enabled, 3D-Colliders will be used for foot and body placement. If disabled, 2D-Colliders will be used instead. [Default: true]")]
//        protected bool m_canBodyPositionMoveHigherThanGroundedPosition = true;
//        [SerializeField, Tooltip("The relative body position offset")]
//        protected float m_bodyPositionOffset = 0.0f;

//        [Header("Leaning")]
//        [SerializeField, Distance(0, 20), Tooltip("This parameter defines how much the characters body should lead towards the moving direction.")]
//        protected float m_leanStrength = 0.0f;
//        [SerializeField, Distance(1, 100), Tooltip("This value is used to smoothly lerp lean transitions. The higher the value, the stiffer the transition. The smaller the value, the smoother the transition.")]
//        protected float m_leanStiffness = 12.0f;
//        [SerializeField, Tooltip("Leans when the actor is grounded - like walking and running. [Default: true]")]
//        protected bool m_leanWhenGrounded = true;
//        [SerializeField, Tooltip("Leans when the actor is not grounded - like jumping or flying. [Default: true]")]
//        protected bool m_leanWhenNotGrounded = true;
//        [SerializeField, Tooltip("Flips the left/right leaning direction. [Default: false]")]
//        protected bool m_flipRightLean = false;
//        [SerializeField, Tooltip("Flips the forward/backward leaning direction. [Default: false]")]
//        protected bool m_flipForwardLean = false;

//        [Header("Grounded-CheckAdd")]
//        [SerializeField, Tooltip("The maximum raydistance to _checkCache if grounded.")]
//        protected float m_checkGroundedDistance = 0.7f;
//        [SerializeField, Tooltip("The radius used to _checkCache if grounded.")]
//        protected float m_checkGroundedRadius = 0.20f;

//        [Header("Footstep Events")]
//        [SerializeField, Tooltip("The distance used to reset the events.")]
//        protected float m_eventCheckResetDistance = 0.05f;
//        [SerializeField, Tooltip("The step length used to fire step events.")]
//        protected float m_stepLength = 0.4f;
//        [SerializeField, Tooltip("Whenever the left foot steps on the ground")]
//        protected IKResultEvent m_onFootstepLeftStart;
//        [SerializeField, Tooltip("Whenever the right foot steps on the ground")]
//        protected IKResultEvent m_onFootstepRightStart;
//        [SerializeField, Tooltip("Whenever the left foot stops stepping on the ground")]
//        protected UnityEvent m_onFootstepLeftStop;
//        [SerializeField, Tooltip("Whenever the right foot stops stepping on the ground")]
//        protected UnityEvent m_onFootstepRightStop;


//        protected bool m_isGrounded = false;
//        protected bool m_isWaitingForPlayingLeftFootEvent = true;
//        protected bool m_isWaitingForPlayingRightFootEvent = true;
//        protected Vector3 m_lastRightIKStepEventPos;
//        protected Vector3 m_lastLeftIKStepEventPos;

//        protected Animator m_animator;
//        protected Transform m_transform;

//        protected Vector3 m_prevTransformPosition;
//        protected Vector3 m_prevBodyPosition;
//        protected Vector3 m_bodyOffset;
//        protected Vector3 m_ikLeftOffset;
//        protected Vector3 m_ikRightOffset;
//        protected Quaternion m_ikLeftDeltaRotation;
//        protected Quaternion m_ikRightDeltaRotation;

//        protected float m_feetResetLerp = 0;
//        protected float m_bodyResetLerp = 0;

//        protected Vector3 m_velocityPlanar = Vector3.zero;

//        protected Vector3 m_extrapolationLeft = Vector3.zero;
//        protected Vector3 m_extrapolationRight = Vector3.zero;

//        protected float m_ikFootHeightToUseLeft;
//        protected float m_ikFootHeightToUseRight;
//        protected float m_ikFootLengthToUseLeft;
//        protected float m_ikFootLengthToUseRight;

//        IKResult m_resultLeft;
//        IKResult m_resultRight;

//        protected ValidationType m_validationType = ValidationType.CHECK_IS_GROUNDED;

//        List<KeyValuePair<int, int>> m_deactiveOnAnimatorState = new List<KeyValuePair<int, int>>();

//        public enum ValidationType
//        {
//            CHECK_IS_GROUNDED,
//            FORCE_VALID,
//            FORCE_INVALID
//        }

//        public virtual void setIsValidAndShouldCheckForGrounded(ValidationType validation)
//        {
//            m_validationType = validation;
//        }

//        public virtual void setLeaningStrength(float value)
//        {
//            m_leanStrength = value;
//        }

//        public virtual void setBodyPlacementWeight(float weight)
//        {
//            m_bodyPlacementWeight = weight;
//        }

//        public virtual void setFootPlacementWeight(float weight)
//        {
//            m_ikPlacementWeight = weight;
//        }

//        public virtual void setBodyPositionOffset(float offset)
//        {
//            m_bodyPositionOffset = offset;
//        }

//        public virtual void setCanBodyPositionMoveHigherThanGroundedPosition(bool value)
//        {
//            m_canBodyPositionMoveHigherThanGroundedPosition = value;
//        }

//        public virtual void setIKExtrapolation(float extrapolation)
//        {
//            m_ikExtrapolation = extrapolation;
//        }

//        protected virtual void Awake()
//        {
//            m_transform = this.transform;
//            m_animator = this.GetComponent<Animator>();
//            m_prevTransformPosition = m_transform.position;
//            m_prevBodyPosition = m_animator.GetBoneTransform(HumanBodyBones.Hips).position;
//            m_resultLeft = new IKResult(Vector3.zero, Vector3.zero, false, Vector3.zero, Vector3.up);
//            m_resultRight = new IKResult(Vector3.zero, Vector3.zero, false, Vector3.zero, Vector3.up);

//            foreach (var entry in m_forceInvalidOnAnimatorStates) m_deactiveOnAnimatorState.Add(new KeyValuePair<int, int>(entry.layer, Animator.StringToHash(entry.animatorState)));
//        }

//        protected virtual void Start()
//        {
//            m_prevTransformPosition = m_transform.position;
//            m_ikFootHeightToUseLeft = m_ikFootHeight;
//            m_ikFootHeightToUseRight = m_ikFootHeight;
//            m_ikFootLengthToUseLeft = m_ikFootLength;
//            m_ikFootLengthToUseRight = m_ikFootLength;

//            // clamping values that are not allowed to be zero or smaller zero
//            if (m_ikFootHeight < 0.001f) 
//                m_ikFootHeight = 0.001f;
//            if (m_ikFootLength < 0.001f)
//                m_ikFootLength = 0.001f;
//            if (m_ikFootWidth < 0.001f)
//                m_ikFootWidth = 0.001f;
//        }

//        //a callback for calculating IK

//        public virtual void OnIK(CharacterGroundingReport groundedResult, Vector3 _planarVelocity)
//        {
//            planarVelocity = _planarVelocity;
//            m_isGrounded = groundedResult.FoundAnyGround;
//        }

//        Vector3 planarVelocity;


//        private void LateUpdate()
//        {
//            float deltaTime=Time.deltaTime;

//            Vector3 feetZeroPos = m_transform.position;

//            if (m_isGrounded)
//            {
//                updateIK(feetZeroPos,deltaTime);

//                if (m_leanWhenGrounded)
//                {
//                    m_velocityPlanar = Vector3.Lerp(m_velocityPlanar, planarVelocity, deltaTime * m_leanStiffness);
//                }
//                else
//                {
//                    m_velocityPlanar = Vector3.Lerp(m_velocityPlanar, Vector3.zero, deltaTime * m_leanStiffness);
//                }
//            }
//            else
//            {
//                revertUpdateIK(deltaTime);

//                if (m_leanWhenNotGrounded)
//                {
//                    m_velocityPlanar = Vector3.Lerp(m_velocityPlanar, planarVelocity, deltaTime * m_leanStiffness);
//                }
//                else
//                {
//                    m_velocityPlanar = Vector3.Lerp(m_velocityPlanar, Vector3.zero, deltaTime * m_leanStiffness);
//                }
//            }

//            updateLeaning(feetZeroPos);

//            m_prevTransformPosition = feetZeroPos;
//        }

//        protected void updateLeaning(Vector3 feetZeroPos)
//        {
//            if (m_leanStrength < 0.001f) return;

//            Vector3 rotationVec = Body.BodyPosition - feetZeroPos;
//            Vector3 rightLeanVec = Vector3.Project(m_velocityPlanar, m_transform.right);
//            Vector3 forwardLeanVec = Vector3.Project(m_velocityPlanar, m_transform.forward);

//            float signRight = 1;
//            float signForward = 1;

//            if (Vector3.Dot(rightLeanVec, m_transform.right) > 0) signRight = -1;
//            if (Vector3.Dot(forwardLeanVec, m_transform.forward) < 0) signForward = -1;

//            if (m_flipForwardLean) signForward *= -1;
//            if (m_flipRightLean) signRight *= -1;

//            Quaternion rotRightAxis = Quaternion.AngleAxis(forwardLeanVec.magnitude * signForward * m_leanStrength, m_transform.right);
//            Quaternion rotForwardAxis = Quaternion.AngleAxis(rightLeanVec.magnitude * signRight * m_leanStrength, m_transform.forward);

//            Body.NewBodyPosition = feetZeroPos + rotForwardAxis * rotRightAxis * rotationVec;
//            //m_animator.bodyRotation = rotForwardAxis * rotRightAxis * m_animator.bodyRotation;
//        }

//        private void OnAnimatorIK(int layerIndex)
//        {
//            Debug.Log(layerIndex);
//        }
//        protected void updateIK(Vector3 feetZeroPos,float deltaTime)
//        {
//            m_feetResetLerp += deltaTime * m_ikPlacementStiffness * m_speedForInterpolatingToRespectiveDefaultValues;
//            m_feetResetLerp = Mathf.Min(m_feetResetLerp, 1);
//            m_bodyResetLerp += deltaTime * m_bodyStiffness * m_speedForInterpolatingToRespectiveDefaultValues;
//            m_bodyResetLerp = Mathf.Min(m_bodyResetLerp, 1);

//            Vector3 ikLeft = GetLeftFoot.GoalPosition;
//            Vector3 ikRight = GetRightFoot.GoalPosition;

//            var rot = m_transform.rotation;

//            Quaternion currRotLeft = GetLeftFoot.GoalRotation;
//            Quaternion currRotRight = GetRightFoot.GoalRotation;

//            float footForwardOffset = (m_ikFootForwardBias + m_ikFootLength * 0.5f) * 0.7f;
//            Vector3 lv = Vector3.Project(currRotLeft * Vector3.forward * footForwardOffset, m_transform.up);
//            Vector3 rv = Vector3.Project(currRotRight * Vector3.forward * footForwardOffset, m_transform.up);
//            float l = 1.0f;
//            float r = 1.0f;
//            if (Vector3.Dot(lv.normalized, m_transform.up) > 0) l = 0.0f;
//            if (Vector3.Dot(rv.normalized, m_transform.up) > 0) r = 0.0f;
//            m_ikFootHeightToUseLeft = m_ikFootHeight + l * lv.magnitude;
//            m_ikFootHeightToUseRight = m_ikFootHeight + r * rv.magnitude;

//            Vector3 lf = Vector3.Project(currRotLeft * Vector3.forward * m_ikFootLength, m_transform.forward);
//            Vector3 rf = Vector3.Project(currRotRight * Vector3.forward * m_ikFootLength, m_transform.forward);
//            m_ikFootLengthToUseLeft = Mathf.Max(lf.magnitude, m_ikFootHeight);
//            m_ikFootLengthToUseRight = Mathf.Max(rf.magnitude, m_ikFootHeight);

//            Vector3 ikPosFromOriginLeft = ikLeft - feetZeroPos;
//            Vector3 projOnRayDirVecLeft = Vector3.Project(ikPosFromOriginLeft, -m_transform.up);
//            Vector3 ikBottomPointLeft = ikLeft - projOnRayDirVecLeft;

//            Vector3 ikPosFromOriginRight = ikRight - feetZeroPos;
//            Vector3 projOnRayDirVecRight = Vector3.Project(ikPosFromOriginRight, -m_transform.up);
//            Vector3 ikBottomPointRight = ikRight - projOnRayDirVecRight;

//            var resultLeft = findNewIKPos(ikLeft, ikBottomPointLeft, m_ikFootHeightToUseLeft, m_ikFootForwardBias, m_transform.forward, m_transform.rotation, feetZeroPos, -m_transform.up, m_ikMaxCorrection, m_bodyPositionMaxCorrection, m_ikFootLengthToUseLeft, m_ikFootWidth, m_collisionLayerMask);
//            var resultRight = findNewIKPos(ikRight, ikBottomPointRight, m_ikFootHeightToUseRight, m_ikFootForwardBias, m_transform.forward, m_transform.rotation, feetZeroPos, -m_transform.up, m_ikMaxCorrection, m_bodyPositionMaxCorrection, m_ikFootLengthToUseRight, m_ikFootWidth, m_collisionLayerMask);

//            // calculate body offset vector
//            Vector3 leftRightBottomDiffVec = resultRight.bottomPoint - resultLeft.bottomPoint;
//            Vector3 leftRightBottomDiffProjVec = Vector3.Project(leftRightBottomDiffVec, m_transform.up);

//            Vector3 leftBottomVec = resultLeft.bottomPoint - feetZeroPos;
//            Vector3 leftBottomProjVec = Vector3.Project(leftBottomVec, m_transform.up);
//            Vector3 rightBottomVec = resultRight.bottomPoint - feetZeroPos;
//            Vector3 rightBottomProjVec = Vector3.Project(rightBottomVec, m_transform.up);

//            if (leftRightBottomDiffProjVec.magnitude > m_bodyPositionMaxCorrection)
//            {
//                if (Vector3.Dot(rightBottomProjVec.normalized, transform.up) < 0)
//                {
//                    resultRight = new IKResult(ikRight, ikRight, false, ikBottomPointRight, m_transform.up);
//                    rightBottomProjVec = Vector3.zero;
//                }
//                else
//                {
//                    resultLeft = new IKResult(ikLeft, ikLeft, false, ikBottomPointLeft, m_transform.up);
//                    leftBottomProjVec = Vector3.zero;
//                }
//            }

//            m_resultLeft.bottomPoint = resultLeft.bottomPoint;
//            m_resultLeft.gluedIKPos = resultLeft.gluedIKPos;
//            m_resultLeft.ikPos = resultLeft.ikPos;
//            m_resultLeft.isGlued = resultLeft.isGlued;
//            m_resultLeft.normal = resultLeft.normal;


//            m_resultRight.bottomPoint = resultRight.bottomPoint;
//            m_resultRight.gluedIKPos = resultRight.gluedIKPos;
//            m_resultRight.ikPos = resultRight.ikPos;
//            m_resultRight.isGlued = resultRight.isGlued;
//            m_resultRight.normal = resultRight.normal;

//            Vector3 bodyOffsetVec = Vector3.zero;
//            Vector3 ikLeftOffsetVec = Vector3.zero;
//            Vector3 ikRightOffsetVec = Vector3.zero;

//            if (Vector3.Dot(leftBottomProjVec.normalized, m_transform.up) < 0)
//            {
//                ikLeftOffsetVec = leftBottomProjVec;

//                if (Vector3.Dot(rightBottomProjVec.normalized, transform.up) < 0)
//                {
//                    ikRightOffsetVec = rightBottomProjVec;

//                    if (rightBottomProjVec.magnitude > leftBottomProjVec.magnitude)
//                    {
//                        bodyOffsetVec = rightBottomProjVec;
//                    }
//                    else
//                    {
//                        bodyOffsetVec = leftBottomProjVec;
//                    }
//                }
//                else
//                {
//                    bodyOffsetVec = leftBottomProjVec;
//                }
//            }
//            else if (Vector3.Dot(rightBottomProjVec.normalized, transform.up) < 0)
//            {
//                ikRightOffsetVec = rightBottomProjVec;
//                bodyOffsetVec = rightBottomProjVec;
//            }
//            else if (m_canBodyPositionMoveHigherThanGroundedPosition)
//            {
//                if (rightBottomProjVec.magnitude < leftBottomProjVec.magnitude)
//                {
//                    bodyOffsetVec = rightBottomProjVec;
//                }
//                else
//                {
//                    bodyOffsetVec = leftBottomProjVec;
//                }
//            }

//            m_bodyOffset = Vector3.Lerp(m_bodyOffset, bodyOffsetVec, deltaTime * m_bodyStiffness);
//            Body.NewBodyPosition = Body.BodyPosition + (m_bodyOffset + m_bodyPositionOffset * m_transform.up) * m_bodyPlacementWeight * m_bodyResetLerp;

//            // calculate ik positions
//            // since the findNewIKPos function clamps the positions to not be lower than it's original IK, in the case that the ik algorithm
//            // moves the bodyPosition downwards (can't happen on flat surfaces) we want to set the IK position downwards with the exact same value.
//            // this makes the animations not being glued to the ground when the feet basically moves up + moves the feet downwards when the body position
//            // is moved downwards as well. However, this is not moved by the same body offset vector, but by the respective feet downward vectors that may
//            // or may not affect the body position, depending on the above case handling.
//            Vector3 ikLeftTargetPos = resultLeft.ikPos + ikLeftOffsetVec * m_bodyPlacementWeight;
//            Vector3 ikRightTargetPos = resultRight.ikPos + ikRightOffsetVec * m_bodyPlacementWeight;

//            // fix so that the new ikTargetPos doesn't get lower than the lowest allowed (glued) position.
//            Vector3 distVecLeft = ikLeftTargetPos - resultLeft.gluedIKPos;
//            if (Vector3.Dot(distVecLeft.normalized, -m_transform.up) > 0)
//            {
//                ikLeftTargetPos = resultLeft.gluedIKPos;
//                resultLeft.isGlued = true;
//                resultLeft.ikPos = ikLeftTargetPos;
//            }
//            Vector3 distVecRight = ikRightTargetPos - resultRight.gluedIKPos;
//            if (Vector3.Dot(distVecRight.normalized, -m_transform.up) > 0)
//            {
//                ikRightTargetPos = resultRight.gluedIKPos;
//                resultRight.isGlued = true;
//                resultRight.ikPos = ikRightTargetPos;
//            }

//            debugDrawPoint(ikLeftTargetPos, Color.magenta);
//            debugDrawPoint(ikRightTargetPos, Color.magenta);

//            Vector3 ikLeftOffset = ikLeftTargetPos - ikLeft;
//            Vector3 ikRightOffset = ikRightTargetPos - ikRight;

//            // extrapolation
//            Vector3 leftOffsetDiff = ikLeftOffset - m_ikLeftOffset;
//            Vector3 rightOffsetDiff = ikRightOffset - m_ikRightOffset;

//            if (leftOffsetDiff.magnitude > 0.01f && Vector3.Dot(leftOffsetDiff.normalized, m_transform.up) > 0) m_extrapolationLeft = leftOffsetDiff * m_ikExtrapolation;
//            else m_extrapolationLeft = Vector3.zero;

//            if (rightOffsetDiff.magnitude > 0.01f && Vector3.Dot(rightOffsetDiff.normalized, m_transform.up) > 0) m_extrapolationRight = rightOffsetDiff * m_ikExtrapolation;
//            else m_extrapolationRight = Vector3.zero;

//            // apply positional offset
//            Vector3 targetOffsetLeft = ikLeftOffset + m_extrapolationLeft;
//            Vector3 targetOffsetRight = ikRightOffset + m_extrapolationRight;

//            float stiffnessLerpValue = deltaTime * m_ikPlacementStiffness;
//            m_ikLeftOffset = Vector3.Lerp(m_ikLeftOffset, targetOffsetLeft, stiffnessLerpValue);
//            m_ikRightOffset = Vector3.Lerp(m_ikRightOffset, targetOffsetRight, stiffnessLerpValue);

//            // this is a special case catch (which clamps the offsets), when the final offset length gets bigger than the downwards correction offset length (overshooting when moving feet downwards)
//            if (m_antiDownwardsIntersectionStiffness > 0.01f)
//            {
//                Vector3 changeInBodyPosition = Body.BodyPosition - m_prevBodyPosition;
//                Vector3 projChangeInBodyPos = Vector3.Project(changeInBodyPosition, -m_transform.up);
//                if (Vector3.Dot(projChangeInBodyPos.normalized, -m_transform.up) > 0)
//                {
//                    float clampLerpValue = Mathf.Min(deltaTime * m_antiDownwardsIntersectionStiffness, 1);
//                    if (ikLeftOffset.magnitude > 0 && m_ikLeftOffset.magnitude > ikLeftOffset.magnitude && Vector3.Dot(m_ikLeftOffset.normalized, -m_transform.up) > 0)
//                    {
//                        m_ikLeftOffset = Vector3.Lerp(m_ikLeftOffset, ikLeftOffset, clampLerpValue);
//                        m_extrapolationLeft = Vector3.Lerp(m_extrapolationLeft, Vector3.zero, clampLerpValue);
//                    }

//                    if (ikRightOffset.magnitude > 0 && m_ikRightOffset.magnitude > ikRightOffset.magnitude && Vector3.Dot(m_ikRightOffset.normalized, -m_transform.up) > 0)
//                    {
//                        m_ikRightOffset = Vector3.Lerp(m_ikRightOffset, ikRightOffset, clampLerpValue);
//                        m_extrapolationRight = Vector3.Lerp(m_extrapolationRight, Vector3.zero, clampLerpValue);
//                    }
//                }
//            }
//            m_prevBodyPosition = Body.BodyPosition;

//            Vector3 nextIkLeft = ikLeft + m_ikLeftOffset;
//            Vector3 nextIkRight = ikRight + m_ikRightOffset;

//            SetLeftFoot.GoalPosition = nextIkLeft;
//            SetLeftFoot.GoalPositionWeight = m_ikPlacementWeight * m_feetResetLerp;

//            SetRightFoot.GoalPosition = nextIkRight;
//            SetRightFoot.GoalPositionWeight = m_ikPlacementWeight * m_feetResetLerp;


//            // calculate ik rotations
//            float rotationStiffnessSlerpValue = deltaTime * m_ikRotationStiffness;
//            if (resultLeft.isGlued || m_onlyAdaptRotationWhenGluedToGround == false)
//            {
//                Vector3 normalLeftFoot = resultLeft.normal;
//                Quaternion deltaLeft = Quaternion.FromToRotation(m_transform.up, normalLeftFoot);
//                m_ikLeftDeltaRotation = Quaternion.Slerp(m_ikLeftDeltaRotation, deltaLeft, rotationStiffnessSlerpValue);
//            }
//            else
//            {
//                m_ikLeftDeltaRotation = Quaternion.Slerp(m_ikLeftDeltaRotation, Quaternion.identity, rotationStiffnessSlerpValue);
//            }

//            if (resultRight.isGlued || m_onlyAdaptRotationWhenGluedToGround == false)
//            {
//                Vector3 normalRightFoot = resultRight.normal;
//                Quaternion deltaRight = Quaternion.FromToRotation(m_transform.up, normalRightFoot);
//                m_ikRightDeltaRotation = Quaternion.Slerp(m_ikRightDeltaRotation, deltaRight, rotationStiffnessSlerpValue);
//            }
//            else
//            {
//                m_ikRightDeltaRotation = Quaternion.Slerp(m_ikRightDeltaRotation, Quaternion.identity, rotationStiffnessSlerpValue);
//            }

//            Quaternion nextRotLeft = m_ikLeftDeltaRotation * currRotLeft;
//            Quaternion nextRotRight = m_ikRightDeltaRotation * currRotRight;

//            SetLeftFoot.GoalRotation = nextRotLeft;
//            SetLeftFoot.GoalRotationWeight = m_ikPlacementWeight * m_feetResetLerp;

//            SetRightFoot.GoalRotation = nextRotRight;
//            SetRightFoot.GoalRotationWeight = m_ikPlacementWeight * m_feetResetLerp;

//            m_feetResetLerp = 1;

//            updateEvents(resultLeft, ikLeftTargetPos, resultRight, ikRightTargetPos, m_ikFootHeight, m_ikFootHeight);
//        }

//        protected void updateEvents(IKResult resultLeft, Vector3 ikLeftTargetPos, IKResult resultRight, Vector3 ikRightTargetPos, float footHeightLeft, float footHeightRight)
//        {
//            // left
//            float distToGroundLeft = Vector3.Project(resultLeft.bottomPoint - ikLeftTargetPos, m_transform.up).magnitude;
//            float distToLastLeftStepPos = (ikLeftTargetPos - m_lastLeftIKStepEventPos).magnitude;

//            if (m_isWaitingForPlayingLeftFootEvent)
//            {
//                if (distToGroundLeft < (footHeightLeft + m_eventCheckResetDistance) && distToLastLeftStepPos > m_stepLength)
//                {
//                    if (m_onFootstepLeftStart != null) m_onFootstepLeftStart.Invoke(m_resultLeft);
//                    m_isWaitingForPlayingLeftFootEvent = false;
//                    m_lastLeftIKStepEventPos = ikLeftTargetPos;
//                }
//            }
//            else
//            {
//                if (distToGroundLeft > (footHeightLeft + m_eventCheckResetDistance) && distToLastLeftStepPos > m_stepLength)
//                {
//                    if (m_onFootstepLeftStop != null) m_onFootstepLeftStop.Invoke();
//                    m_isWaitingForPlayingLeftFootEvent = true;
//                    m_lastLeftIKStepEventPos = ikLeftTargetPos;
//                }
//            }

//            // right
//            float distToGroundRight = Vector3.Project(resultRight.bottomPoint - ikRightTargetPos, m_transform.up).magnitude;
//            float distToLastRightStepPos = (ikRightTargetPos - m_lastRightIKStepEventPos).magnitude;

//            if (m_isWaitingForPlayingRightFootEvent)
//            {
//                if (distToGroundRight < (footHeightRight + m_eventCheckResetDistance) && distToLastRightStepPos > m_stepLength)
//                {
//                    if (m_onFootstepRightStart != null) m_onFootstepRightStart.Invoke(m_resultRight);
//                    m_isWaitingForPlayingRightFootEvent = false;
//                    m_lastRightIKStepEventPos = ikRightTargetPos;
//                }
//            }
//            else
//            {
//                if (distToGroundRight > (footHeightRight + m_eventCheckResetDistance) && distToLastRightStepPos > m_stepLength)
//                {
//                    if (m_onFootstepRightStop != null) m_onFootstepRightStop.Invoke();
//                    m_isWaitingForPlayingRightFootEvent = true;
//                    m_lastRightIKStepEventPos = ikRightTargetPos;
//                }
//            }
//        }

//        protected void revertUpdateIK(float deltaTime)
//        {
//            // reset to default body center of mass position
//            // reset to default IKs and weights
//            m_feetResetLerp -= deltaTime * m_ikPlacementStiffness * m_speedForInterpolatingToRespectiveDefaultValues;
//            m_feetResetLerp = Mathf.Max(m_feetResetLerp, 0);
//            m_bodyResetLerp -= deltaTime * m_bodyStiffness * m_speedForInterpolatingToRespectiveDefaultValues;
//            m_bodyResetLerp = Mathf.Max(m_bodyResetLerp, 0);

//            // body position
//            Body.NewBodyPosition = Body.BodyPosition + (m_bodyOffset + m_bodyPositionOffset * m_transform.up) * m_bodyPlacementWeight * m_bodyResetLerp;

//            // ik positions
//            Vector3 ikLeft = GetLeftFoot.GoalPosition;
//            Vector3 ikRight = GetRightFoot.GoalPosition;

//            m_ikLeftOffset = Vector3.Lerp(Vector3.zero, m_ikLeftOffset, m_feetResetLerp);
//            m_ikRightOffset = Vector3.Lerp(Vector3.zero, m_ikRightOffset, m_feetResetLerp);
//            m_extrapolationLeft = Vector3.Lerp(m_extrapolationLeft, Vector3.zero, m_feetResetLerp);
//            m_extrapolationRight = Vector3.Lerp(m_extrapolationRight, Vector3.zero, m_feetResetLerp);

//            Vector3 nextIkLeft = ikLeft + m_ikLeftOffset;
//            Vector3 nextIkRight = ikRight + m_ikRightOffset;

//            SetLeftFoot.GoalPosition = nextIkLeft;
//            SetLeftFoot.GoalPositionWeight = m_feetResetLerp * m_ikPlacementWeight;

//            SetRightFoot.GoalPosition = nextIkRight;
//            SetRightFoot.GoalPositionWeight = m_feetResetLerp * m_ikPlacementWeight;

//             // ik rotations
//            Quaternion currRotLeft = GetLeftFoot.GoalRotation;
//            Quaternion currRotRight = GetRightFoot.GoalRotation;

//            m_ikLeftDeltaRotation = Quaternion.Slerp(Quaternion.identity, m_ikLeftDeltaRotation, m_feetResetLerp);
//            m_ikRightDeltaRotation = Quaternion.Slerp(Quaternion.identity, m_ikRightDeltaRotation, m_feetResetLerp);

//            SetLeftFoot.GoalRotation = m_ikLeftDeltaRotation * currRotLeft;
//            SetLeftFoot.GoalRotationWeight = m_feetResetLerp * m_ikPlacementWeight;

//            SetRightFoot.GoalRotation = m_ikRightDeltaRotation * currRotRight;
//            SetRightFoot.GoalRotationWeight = m_feetResetLerp * m_ikPlacementWeight;

//            // events
//            if (m_isWaitingForPlayingLeftFootEvent == false && m_onFootstepLeftStop != null) m_onFootstepLeftStop.Invoke();
//            if (m_isWaitingForPlayingRightFootEvent == false && m_onFootstepRightStop != null) m_onFootstepRightStop.Invoke();
//            m_isWaitingForPlayingLeftFootEvent = true;
//            m_isWaitingForPlayingRightFootEvent = true;
//            m_lastLeftIKStepEventPos = nextIkLeft;
//            m_lastRightIKStepEventPos = nextIkRight;
//        }

//        protected IKResult findNewIKPos(Vector3 ikPos, Vector3 ikBottomPoint, float heightOffset, float forwardBias, Vector3 forwardDir, Quaternion rotation, Vector3 origin, Vector3 rayDirection, float maxIKCorrection, float maxCenterOfMassCorrection, float raycastLength, float raycastWidth, int raycastLayer)
//        {
//            float boxCastHeight = Mathf.Max(heightOffset * 0.01f, 0.01f);

//            Vector3 rayOriginPoint = ikBottomPoint - rayDirection * maxIKCorrection - rayDirection * boxCastHeight;
//            Vector3 rayOrigin = rayOriginPoint + forwardBias * forwardDir;

//            debugDrawPoint(rayOrigin, Color.blue);
//            debugDrawPoint(ikPos, Color.yellow);

//            Ray ray = new Ray(rayOrigin, rayDirection);
//            RaycastHit hit;
//            float raycastDistance = maxIKCorrection + maxCenterOfMassCorrection;

//            if (UnityEngine.Physics.BoxCast(ray.origin, new Vector3(raycastWidth * 0.5f, boxCastHeight, raycastLength * 0.5f), ray.direction, out hit, rotation, raycastDistance, raycastLayer, m_triggerCollisionInteraction))
//            {
//                Vector3 hitPoint = rayOrigin + hit.distance * rayDirection + boxCastHeight * rayDirection;
//                Vector3 foundBottomPoint = hitPoint - forwardDir * forwardBias;

//                // boxcast slope offset fix
//                Vector3 normalToUse = hit.normal;
//                float alpha = Vector3.Angle(-rayDirection, normalToUse);

//                RaycastHit fixHit;
//                if (UnityEngine.Physics.SphereCast(rayOriginPoint - raycastWidth * 0.485f * rayDirection, raycastWidth * 0.485f, rayDirection, out fixHit, raycastDistance, raycastLayer, m_triggerCollisionInteraction))
//                {
//                    if (Mathf.Abs(Vector3.Angle(-rayDirection, fixHit.normal) - alpha) > 1.0f)
//                    {
//                        Vector3 hitpointsVec = fixHit.point - hit.point;
//                        if (hitpointsVec.magnitude > 0.001f)
//                        {
//                            hitpointsVec.Normalize();
//                            Vector3 r = Vector3.Cross(hitpointsVec, -rayDirection).normalized;
//                            Vector3 fixNormal = Vector3.Cross(r, hitpointsVec).normalized;
//                            float fixAlpha = Vector3.Angle(-rayDirection, fixNormal);

//                            if (Mathf.Abs(fixAlpha) < Mathf.Abs(alpha))
//                            {
//                                normalToUse = fixNormal;
//                                alpha = fixAlpha;
//                            }
//                        }
//                    }
//                }

//                if (Mathf.Abs(alpha) > m_maxGroundAngleAtWhichTheGroundIsDetectedAsGround) return new IKResult(ikPos, ikPos, false, ikBottomPoint, -rayDirection);
//                alpha = Mathf.Clamp(alpha, -m_maxGroundAngleTheFeetsCanAdaptTo, m_maxGroundAngleTheFeetsCanAdaptTo);

//                Vector3 pointsDistVec = foundBottomPoint - hit.point;
//                float l = pointsDistVec.magnitude;
//                Vector3 c1 = Vector3.Cross(pointsDistVec.normalized, normalToUse).normalized;
//                Vector3 c2 = Vector3.Cross(normalToUse, c1).normalized;
//                float cosAlpha = Mathf.Cos(alpha * Mathf.Deg2Rad);

//                if (cosAlpha > 0 && cosAlpha < 0.01f) cosAlpha = 0.01f;
//                else if (cosAlpha < 0 && cosAlpha > -0.01f) cosAlpha = -0.01f;

//                float ll = l / cosAlpha;
//                Vector3 angleOffset = Vector3.Project(c2 * ll, -rayDirection);
//                foundBottomPoint = foundBottomPoint + angleOffset;

//                float realHeightOffset = heightOffset / cosAlpha;

//                Vector3 foundIKPos = foundBottomPoint - rayDirection * realHeightOffset;

//                // the ik point is not allowed to be lower than the original ik
//                bool isGlued = true;
//                Vector3 gluedIkPos = foundIKPos;
//                Vector3 distVec = foundIKPos - ikPos;
//                if (Vector3.Dot(distVec.normalized, rayDirection) > 0)
//                {
//                    foundIKPos = ikPos;
//                    isGlued = false;
//                }

//                debugDrawPoint(hitPoint, Color.blue);
//                debugDrawPoint(foundBottomPoint, Color.red);
//                debugDrawPoint(foundIKPos, Color.cyan);
//                debugDrawArrow(rayOrigin, hitPoint, Color.blue);

//                return new IKResult(foundIKPos, gluedIkPos, isGlued, foundBottomPoint, normalToUse);
//            }

//            return new IKResult(ikPos, ikPos, false, ikBottomPoint, -rayDirection);
//        }

//        private void debugDrawPoint(Vector3 point, Color color)
//        {
//#if UNITY_EDITOR
//            Debug.DrawLine(point - Vector3.right * 0.025f, point + Vector3.right * 0.025f, color);
//            Debug.DrawLine(point - Vector3.forward * 0.025f, point + Vector3.forward * 0.025f, color);
//            Debug.DrawLine(point - Vector3.up * 0.025f, point + Vector3.up * 0.025f, color);
//#endif
//        }

//        private void debugDrawArrow(Vector3 start, Vector3 end, Color color)
//        {
//#if UNITY_EDITOR
//            Debug.DrawLine(start, end, color);
//            Debug.DrawLine(end, end + Vector3.up * 0.025f + Vector3.right * 0.025f, color);
//            Debug.DrawLine(end, end + Vector3.up * 0.025f - Vector3.right * 0.025f, color);
//#endif
//        }
//    }

//}



//namespace Return.Humanoid.IK
//{


////    public class StepIK : BaseComponent
////    {

////        m_MotionSystem motionSystem;

////        public AnimationScriptPlayable CreateJob(PlayableGraph graph)
////        {
////            IKJob = new FootIKJob() { Pause = true };
////            IKScriptPlayable = AnimationScriptPlayable.Create(graph, IKJob, 1);
////            return IKScriptPlayable;
////            //motionSystem.AddPlayableIK(IKScriptPlayable);
////        }

////        public AnimationScriptPlayable IKScriptPlayable;

////        public TransformStreamHandle ControllerRoot;
////        public TransformStreamHandle RightLeg;
////        public TransformStreamHandle LeftLeg;

////        public FootIKJob LastJob;
////        public FootIKJob IKJob;




////        public FootIKJob.RayCastMission RightMission;
////        public FootIKJob.RayCastMission LeftMission;

////        public IKResult RightCastResult { get => m_resultRight; set => m_resultRight = value; }
////        public IKResult LeftCastResult { get => m_resultLeft; set => m_resultLeft = value; }

////        public Animator m_animator;
////        public ReadOnlyTransform m_transform;

////        ReadOnlyTransform rightFoot;
////        ReadOnlyTransform leftFoot;

////        IKResult m_resultLeft;
////        IKResult m_resultRight;

////        Vector3 BodyPos;

////        public Vector3 m_prevTransformPosition;

////        public Vector3 m_prevBodyPosition;



////        [Header("General")]

////        [SerializeField, Tooltip("The Collision-Layer for foot and body placement detection.")]
////        LayerMask m_collisionLayerMask = 1 << 0;
////        [SerializeField, Tooltip("The collision-setting for trigger-interactions for foot and body placement detection. Default setting is 'Ignore'.")]
////        QueryTriggerInteraction m_triggerCollisionInteraction = QueryTriggerInteraction.Ignore;
////        [SerializeField, Distance(0.05f, 10.0f), Tooltip("The speed at which the positions and rotations interpolate to the respective default values inbetween valid / grounded and invalid / not grounded state changes.")]
////        public float m_speedForInterpolatingToRespectiveDefaultValues  = 0.25f;


////        [Header("Foot IK placement")]
////        [SerializeField, Distance(0, 1), Tooltip("The strength that the effects will be applyed to.")]
////        public float m_ikPlacementWeight  = 1;
////        [SerializeField, Distance(1, 50), Tooltip("This value is used to smoothly lerp IK positions. The higher the value, the stiffer the transition. The smaller the value, the smoother the transition.")]
////        public float m_ikPlacementStiffness  = 10.0f;
////        [SerializeField, Distance(1, 50), Tooltip("This value is used to smoothly slerp IK rotations. The higher the value, the stiffer the transition. The smaller the value, the smoother the transition.")]
////        public float m_ikRotationStiffness  = 6.0f;
////        [SerializeField, Distance(0, 5), Tooltip("The IK extrapolation defines how much the feet overshoots when trying to achive the IK target position. A value > 0 will lead to a more robust and solid transition. A too high value will lead to stiff looking transitions.")]
////        public float m_ikExtrapolation = 1.0f;
////        [SerializeField, Tooltip("The maximum IK correction for placing the feet.")]
////        public float m_ikMaxCorrection  = 0.5f;
////        [SerializeField, Tooltip("This value defines the distance between IK position and the foot ground-point. It's like the rows of the foot. This value is not allowed to be zero or smaller zero.")]
////        public float m_ikFootHeight= 0.1f;
////        [SerializeField, Tooltip("The IK foot size defines the length of the foot used for the simulation model. This value is not allowed to be zero or smaller zero.")]
////        public float m_ikFootLength  = 0.24f;
////        [SerializeField, Tooltip("The IK foot lines defines the lines of the foot used for the simulation model. This value is not allowed to be zero or smaller zero.")]
////        public float m_ikFootWidth = 0.085f;
////        [SerializeField, Tooltip("The IK foot forward bias defines the offset for the foots raycast origin in relation to the given IK position")]
////        public float m_ikFootForwardBias  = 0.065f;
////        [SerializeField, Distance(0, 250), Tooltip("This is an advanced setting with which intersections, when the feet move downwards, are stopped. A low value will let the feet bump inside the ground for a short period of time.")]
////        public float m_antiDownwardsIntersectionStiffness = 50.0f;
////        [SerializeField, Distance(0, 90), Tooltip("This is an advanced setting where if this value is exceeded, the detected ground won't be handled as detected anymore.")]
////        public float m_maxGroundAngleAtWhichTheGroundIsDetectedAsGround  = 81.0f;
////        [SerializeField, Distance(0, 90), Tooltip("This is an advanced setting with which you can setup the maximum allowed angle the feet will adapt to.")]
////        public float m_maxGroundAngleTheFeetsCanAdaptTo = 79.0f;
////        [SerializeField, Tooltip("If this value is true, rotations will be \"unlocked\" to their original rotation when not glued to the ground. If this value is false the foot rotation will try to adapt to the ground whenever grounded. The recommended value for having better transitions is typically true. [Default: true]")]
////        public bool m_onlyAdaptRotationWhenGluedToGround  = true;

////        [Header("Body placement")]
////        [SerializeField, Distance(0, 1), Tooltip("The strength that the effects will be applyed to.")]
////        public float m_bodyPlacementWeight = 1;
////        [SerializeField, Distance(1, 50), Tooltip("This value is used to smoothly lerp body position transitions. The higher the value, the stiffer the transition. The smaller the value, the smoother the transition.")]
////        public float m_bodyStiffness = 6.5f;
////        [SerializeField, Tooltip("The maximum possible body position correction.")]
////        public float m_bodyPositionMaxCorrection = 0.7f;
////        [SerializeField, Tooltip("If enabled, 3D-Colliders will be used for foot and body placement. If disabled, 2D-Colliders will be used instead. [Default: true]")]
////        public bool m_canBodyPositionMoveHigherThanGroundedPosition = true;
////        [SerializeField, Tooltip("The relative body position offset")]
////        public float m_bodyPositionOffset = 0.0f;

////        [Header("Leaning")]
////        [SerializeField, Distance(0, 20), Tooltip("This parameter defines how much the characters body should lead towards the moving direction.")]
////        public float m_leanStrength = 0.0f;
////        [SerializeField, Distance(1, 100), Tooltip("This value is used to smoothly lerp lean transitions. The higher the value, the stiffer the transition. The smaller the value, the smoother the transition.")]
////        public float m_leanStiffness = 12.0f;
////        [SerializeField, Tooltip("Leans when the actor is grounded - like walking and running. [Default: true]")]
////        public bool m_leanWhenGrounded = true;
////        [SerializeField, Tooltip("Leans when the actor is not grounded - like jumping or flying. [Default: true]")]
////        public bool m_leanWhenNotGrounded = true;
////        [SerializeField, Tooltip("Flips the left/right leaning direction. [Default: false]")]
////        public bool m_flipRightLean = false;
////        [SerializeField, Tooltip("Flips the forward/backward leaning direction. [Default: false]")]
////        public bool m_flipForwardLean = false;

////        [Header("Grounded-CheckAdd")]
////        [SerializeField, Tooltip("The maximum raydistance to _checkCache if grounded.")]
////        public float m_checkGroundedDistance = 0.7f;
////        [SerializeField, Tooltip("The radius used to _checkCache if grounded.")]
////        public float m_checkGroundedRadius = 0.20f;

////        [Header("Footstep Events")]
////        [SerializeField, Tooltip("The distance used to reset the events.")]
////        public float m_eventCheckResetDistance = 0.05f;
////        [SerializeField, Tooltip("The step length used to fire step events.")]
////        public float m_stepLength = 0.4f;


////        [SerializeField, Tooltip("Whenever the left foot steps on the ground")]
////        public IKResultEvent m_onFootstepLeftStart;
////        [SerializeField, Tooltip("Whenever the right foot steps on the ground")]
////        public IKResultEvent m_onFootstepRightStart;
////        [SerializeField, Tooltip("Whenever the left foot stops stepping on the ground")]
////        public UnityEvent m_onFootstepLeftStop;
////        [SerializeField, Tooltip("Whenever the right foot stops stepping on the ground")]
////        public UnityEvent m_onFootstepRightStop;
  

////        public bool m_isGrounded = false;
////        public bool m_isWaitingForPlayingLeftFootEvent = true;
////        public bool m_isWaitingForPlayingRightFootEvent = true;
////        public Vector3 m_lastRightIKStepEventPos;
////        public Vector3 m_lastLeftIKStepEventPos;



////        public virtual void setLeaningStrength(float value)
////        {
////            m_leanStrength = value;
////        }

////        public virtual void setBodyPlacementWeight(float weight)
////        {
////            m_bodyPlacementWeight = weight;
////        }

////        public virtual void setFootPlacementWeight(float weight)
////        {
////            m_ikPlacementWeight = weight;
////        }

////        public virtual void setBodyPositionOffset(float offset)
////        {
////            m_bodyPositionOffset = offset;
////        }

////        public virtual void setCanBodyPositionMoveHigherThanGroundedPosition(bool value)
////        {
////            m_canBodyPositionMoveHigherThanGroundedPosition = value;
////        }

////        public virtual void setIKExtrapolation(float extrapolation)
////        {
////            m_ikExtrapolation = extrapolation;
////        }



////        private void OnAnimatorIK(int layerIndex)
////        {
////            BodyPos = m_animator.bodyPosition;
////            Debug.Log(GetLeftFoot.GoalPosition);

////        }

////        public virtual void Awake()
////        {
////            m_transform = this.transform;
////            m_animator = this.GetComponent<Animator>();
////            m_prevTransformPosition = m_transform.position;
////            m_prevBodyPosition = m_animator.GetBoneTransform(HumanBodyBones.Hips).position;
////            m_resultLeft = new IKResult(Vector3.zero, Vector3.zero, false, Vector3.zero, Vector3.up);
////            m_resultRight = new IKResult(Vector3.zero, Vector3.zero, false, Vector3.zero, Vector3.up);

////            InstanceIfNull(ref motionSystem);

////            rightFoot = m_animator.GetBoneTransform(HumanBodyBones.RightLeg);
////            leftFoot = m_animator.GetBoneTransform(HumanBodyBones.LeftLeg);

////            ControllerRoot= m_animator.BindStreamTransform(m_transform);
////            RightLeg = m_animator.BindStreamTransform(rightFoot);
////            LeftLeg = m_animator.BindStreamTransform(leftFoot);
////        }

////        public virtual void Start()
////        {
////            m_prevTransformPosition = m_transform.position;


////            IKJob.m_ikFootHeightToUseLeft = m_ikFootHeight;
////            IKJob.m_ikFootHeightToUseRight = m_ikFootHeight;
////            IKJob.m_ikFootLengthToUseLeft = m_ikFootLength;
////            IKJob.m_ikFootLengthToUseRight = m_ikFootLength;

////            // clamping values that are not allowed to be zero or smaller zero
////            m_ikFootHeight = m_ikFootHeight.Clamp(0.001f, m_ikFootHeight);

////            m_ikFootLength = m_ikFootLength.Clamp(0.001f, m_ikFootLength);

////            m_ikFootWidth = m_ikFootWidth.Clamp(0.001f, m_ikFootWidth);



////        }


////        //a callback for calculating IK
////        public virtual void OnIKUpdate(CharacterGroundingReport groundedResult,Vector3 planarVelocity,float deltaTime)
////        {
////            // sensor cast

////            Assert.IsTrue(enabled);

////            IKJob = IKScriptPlayable.GetJobData<FootIKJob>();

////            Vector3 feetZeroPos = m_transform.position;

////            if (groundedResult.FoundAnyGround)
////            {
////                if (m_isGrounded == false)
////                {
////                    m_isGrounded = true;
////                }

////                updateIK(feetZeroPos,deltaTime);

////                //if (m_leanWhenGrounded)
////                //{
////                //    //m_velocityPlanar = Vector3.Lerp(m_velocityPlanar, calculatePlanarVelocity(feetZeroPos), deltaTime * m_leanStiffness);
////                //    m_velocityPlanar = Vector3.Lerp(m_velocityPlanar, planarVelocity, deltaTime * m_leanStiffness);
////                //}
////                //else
////                //{
////                //    m_velocityPlanar = Vector3.Lerp(m_velocityPlanar, Vector3.zero, deltaTime * m_leanStiffness);
////                //}
////            }
////            else
////            {
////                if (m_isGrounded)
////                {
////                    m_isGrounded = false;
////                }

////                revertUpdateIK(deltaTime);

////                //if (m_leanWhenNotGrounded)
////                //{
////                //    //m_velocityPlanar = Vector3.Lerp(m_velocityPlanar, calculatePlanarVelocity(feetZeroPos), deltaTime * m_leanStiffness);
////                //    m_velocityPlanar = Vector3.Lerp(m_velocityPlanar, planarVelocity, deltaTime * m_leanStiffness);
////                //}
////                //else
////                //{
////                //    m_velocityPlanar = Vector3.Lerp(m_velocityPlanar, Vector3.zero, deltaTime * m_leanStiffness);
////                //}
////            }

////            updateLeaning(feetZeroPos);

////            m_prevTransformPosition = feetZeroPos;
////        }

////        //public Vector3 calculatePlanarVelocity(Vector3 feetZeroPos)
////        //{
////        //    if (m_leanStrength < 0.001f)
////        //        return Vector3.zero;

////        //    Vector3 velocity = (feetZeroPos - m_prevTransformPosition) / deltaTime;
////        //    Vector3 velocityUp = Vector3.Project(velocity, m_transform.up);
////        //    return velocity - velocityUp;
////        //}

////        public void updateLeaning(Vector3 feetZeroPos)
////        {
////            if (m_leanStrength < 0.001f) 
////                return;

////            //var rotationVec = BodyPos - feetZeroPos;
////            //var rightLeanVec = Vector3.Project(m_velocityPlanar, m_transform.right);
////            //var forwardLeanVec = Vector3.Project(m_velocityPlanar, m_transform.forward);

////            //float signRight = 1;
////            //float signForward = 1;

////            //if (Vector3.Dot(rightLeanVec, m_transform.right) > 0) 
////            //    signRight = -1;

////            //if (Vector3.Dot(forwardLeanVec, m_transform.forward) < 0) 
////            //    signForward = -1;

////            //if (m_flipForwardLean) signForward *= -1;
////            //if (m_flipRightLean) signRight *= -1;

////            //Quaternion rotRightAxis = Quaternion.AngleAxis(forwardLeanVec.magnitude * signForward * m_leanStrength, m_transform.right);
////            //Quaternion rotForwardAxis = Quaternion.AngleAxis(rightLeanVec.magnitude * signRight * m_leanStrength, m_transform.forward);

////            //m_animator.bodyPosition = feetZeroPos + rotForwardAxis * rotRightAxis * rotationVec;
////            //m_animator.bodyRotation = rotForwardAxis * rotRightAxis * m_animator.bodyRotation;
////        }

////        public void updateIK(Vector3 feetZeroPos,float deltaTime)
////        {
////            return;

////            //m_feetResetLerp += deltaTime * m_ikPlacementStiffness * m_speedForInterpolatingToRespectiveDefaultValues;
////            //m_feetResetLerp = Mathf.Min(m_feetResetLerp, 1);
////            //m_bodyResetLerp += deltaTime * m_bodyStiffness * m_speedForInterpolatingToRespectiveDefaultValues;
////            //m_bodyResetLerp = Mathf.Min(m_bodyResetLerp, 1);

////            //var ikLeft = leftFoot.position;
////            //var ikRight = rightFoot.position;
////            ////Debug.Log(ikLeft);
////            //var currRotLeft = leftFoot.rotation ;
////            //var currRotRight = rightFoot.rotation;

////            //var ikLeft = IKJob.LeftLeg.GUIDs;
////            //var ikRight = IKJob.RightLeg.GUIDs;
////            ////Debug.Log(ikLeft);
////            //var currRotLeft = IKJob.LeftLeg.Rotation;
////            //var currRotRight = IKJob.RightLeg.Rotation;

////            //float footForwardOffset = (m_ikFootForwardBias + m_ikFootLength * 0.5f) * 0.7f;
////            //Vector3 lv = Vector3.Project(currRotLeft * Vector3.forward * footForwardOffset, m_transform.up);
////            //Vector3 rv = Vector3.Project(currRotRight * Vector3.forward * footForwardOffset, m_transform.up);
////            //float l = 1.0f;
////            //float r = 1.0f;
////            //if (Vector3.Dot(lv.normalized, m_transform.up) > 0) 
////            //    l = 0.0f;

////            //if (Vector3.Dot(rv.normalized, m_transform.up) > 0) 
////            //    r = 0.0f;
////            //m_ikFootHeightToUseLeft = m_ikFootHeight + l * lv.magnitude;
////            //m_ikFootHeightToUseRight = m_ikFootHeight + r * rv.magnitude;

////            //Vector3 lf = Vector3.Project(currRotLeft * Vector3.forward * m_ikFootLength, m_transform.forward);
////            //Vector3 rf = Vector3.Project(currRotRight * Vector3.forward * m_ikFootLength, m_transform.forward);
////            //m_ikFootLengthToUseLeft = Mathf.Max(lf.magnitude, m_ikFootHeight);
////            //m_ikFootLengthToUseRight = Mathf.Max(rf.magnitude, m_ikFootHeight);

////            //Vector3 ikPosFromOriginLeft = ikLeft - feetZeroPos;
////            //Vector3 projOnRayDirVecLeft = Vector3.Project(ikPosFromOriginLeft, -m_transform.up);
////            //Vector3 ikBottomPointLeft = ikLeft - projOnRayDirVecLeft;

////            //Vector3 ikPosFromOriginRight = ikRight - feetZeroPos;
////            //Vector3 projOnRayDirVecRight = Vector3.Project(ikPosFromOriginRight, -m_transform.up);
////            //Vector3 ikBottomPointRight = ikRight - projOnRayDirVecRight;
////            var fwd=m_transform.forward;
////            var rot = m_transform.rotation;

////            var up = m_transform.up;
////            var down = -up;

////            var resultLeft = findNewIKPos(LeftMission.Pos, LeftMission.ButtomPoint, LeftMission.FootHeightToUse, m_ikFootForwardBias, fwd, rot, feetZeroPos, down, m_ikMaxCorrection, m_bodyPositionMaxCorrection, LeftMission.FootLengthToUse, m_ikFootWidth, m_collisionLayerMask);
////            var resultRight = findNewIKPos(RightMission.Pos, RightMission.ButtomPoint, RightMission.FootHeightToUse, m_ikFootForwardBias, fwd, rot, feetZeroPos, down, m_ikMaxCorrection, m_bodyPositionMaxCorrection, RightMission.FootLengthToUse, m_ikFootWidth, m_collisionLayerMask);


////            //// calculate body offset vector
////            //Vector3 leftRightBottomDiffVec = resultRight.bottomPoint - resultLeft.bottomPoint;
////            //Vector3 leftRightBottomDiffProjVec = Vector3.Project(leftRightBottomDiffVec, m_transform.up);

////            //Vector3 leftBottomVec = resultLeft.bottomPoint - feetZeroPos;
////            //Vector3 leftBottomProjVec = Vector3.Project(leftBottomVec, m_transform.up);
////            //Vector3 rightBottomVec = resultRight.bottomPoint - feetZeroPos;
////            //Vector3 rightBottomProjVec = Vector3.Project(rightBottomVec, m_transform.up);

////            //if (leftRightBottomDiffProjVec.magnitude > m_bodyPositionMaxCorrection)
////            //{
////            //    if (Vector3.Dot(rightBottomProjVec.normalized, transform.up) < 0)
////            //    {
////            //        resultRight = new IKResult(RightMission.Pos, RightMission.Pos, false, RightMission.ButtomPoint, up);
////            //        rightBottomProjVec = Vector3.zero;
////            //    }
////            //    else
////            //    {
////            //        resultLeft = new IKResult(LeftMission.Pos, LeftMission.Pos, false, LeftMission.ButtomPoint, up);
////            //        leftBottomProjVec = Vector3.zero;
////            //    }
////            //}

////            //m_resultLeft.bottomPoint = resultLeft.bottomPoint;
////            //m_resultLeft.gluedIKPos = resultLeft.gluedIKPos;
////            //m_resultLeft.ikPos = resultLeft.ikPos;
////            //m_resultLeft.isGlued = resultLeft.isGlued;
////            //m_resultLeft.normal = resultLeft.normal;


////            //m_resultRight.bottomPoint = resultRight.bottomPoint;
////            //m_resultRight.gluedIKPos = resultRight.gluedIKPos;
////            //m_resultRight.ikPos = resultRight.ikPos;
////            //m_resultRight.isGlued = resultRight.isGlued;
////            //m_resultRight.normal = resultRight.normal;


////            //Vector3 bodyOffsetVec = Vector3.zero;
////            //Vector3 ikLeftOffsetVec = Vector3.zero;
////            //Vector3 ikRightOffsetVec = Vector3.zero;

////            //if (Vector3.Dot(leftBottomProjVec.normalized, m_transform.up) < 0)
////            //{
////            //    ikLeftOffsetVec = leftBottomProjVec;

////            //    if (Vector3.Dot(rightBottomProjVec.normalized, transform.up) < 0)
////            //    {
////            //        ikRightOffsetVec = rightBottomProjVec;

////            //        if (rightBottomProjVec.magnitude > leftBottomProjVec.magnitude)
////            //        {
////            //            bodyOffsetVec = rightBottomProjVec;
////            //        }
////            //        else
////            //        {
////            //            bodyOffsetVec = leftBottomProjVec;
////            //        }
////            //    }
////            //    else
////            //    {
////            //        bodyOffsetVec = leftBottomProjVec;
////            //    }
////            //}
////            //else if (Vector3.Dot(rightBottomProjVec.normalized, transform.up) < 0)
////            //{
////            //    ikRightOffsetVec = rightBottomProjVec;
////            //    bodyOffsetVec = rightBottomProjVec;
////            //}
////            //else if (m_canBodyPositionMoveHigherThanGroundedPosition)
////            //{
////            //    if (rightBottomProjVec.magnitude < leftBottomProjVec.magnitude)
////            //    {
////            //        bodyOffsetVec = rightBottomProjVec;
////            //    }
////            //    else
////            //    {
////            //        bodyOffsetVec = leftBottomProjVec;
////            //    }
////            //}

////            //m_bodyOffset = Vector3.Lerp(m_bodyOffset, bodyOffsetVec, deltaTime * m_bodyStiffness);
////            ////m_animator.bodyPosition = m_animator.bodyPosition + (m_bodyOffset + m_bodyPositionOffset * m_transform.up) * m_bodyPlacementWeight * m_bodyResetLerp;

////            ////calculate ik positions
////            ////since the findNewIKPos function clamps the positions to not be lower than it's original IK, in the case that the ik algorithm
////            //// moves the bodyPosition downwards(can't happen on flat surfaces) we want to set the IK position downwards with the exact same value.
////            //// this makes the animations not being glued to the ground when the feet basically moves up + moves the feet downwards when the body position
////            //// is moved downwards as well.However, this is not moved by the same body offset vector, but by the respective feet downward vectors that may
////            ////or may not affect the body position, depending on the above case handling.

////            //Vector3 ikLeftTargetPos = resultLeft.ikPos + ikLeftOffsetVec * m_bodyPlacementWeight;
////            //Vector3 ikRightTargetPos = resultRight.ikPos + ikRightOffsetVec * m_bodyPlacementWeight;

////            //// fix so that the new ikTargetPos doesn't get lower than the lowest allowed (glued) position.
////            //Vector3 distVecLeft = ikLeftTargetPos - resultLeft.gluedIKPos;
////            //if (Vector3.Dot(distVecLeft.normalized, -m_transform.up) > 0)
////            //{
////            //    ikLeftTargetPos = resultLeft.gluedIKPos;
////            //    resultLeft.isGlued = true;
////            //    resultLeft.ikPos = ikLeftTargetPos;
////            //}
////            //Vector3 distVecRight = ikRightTargetPos - resultRight.gluedIKPos;
////            //if (Vector3.Dot(distVecRight.normalized, -m_transform.up) > 0)
////            //{
////            //    ikRightTargetPos = resultRight.gluedIKPos;
////            //    resultRight.isGlued = true;
////            //    resultRight.ikPos = ikRightTargetPos;
////            //}

////            //debugDrawPoint(ikLeftTargetPos, Color.magenta);
////            //debugDrawPoint(ikRightTargetPos, Color.magenta);


////            //Vector3 ikLeftOffset = ikLeftTargetPos - LeftMission.Pos;
////            //Vector3 ikRightOffset = ikRightTargetPos - RightMission.Pos;

////            //// extrapolation
////            //Vector3 leftOffsetDiff = ikLeftOffset - m_ikLeftOffset;
////            //Vector3 rightOffsetDiff = ikRightOffset - m_ikRightOffset;

////            //if (leftOffsetDiff.magnitude > 0.01f && Vector3.Dot(leftOffsetDiff.normalized, m_transform.up) > 0) m_extrapolationLeft = leftOffsetDiff * m_ikExtrapolation;
////            //else m_extrapolationLeft = Vector3.zero;

////            //if (rightOffsetDiff.magnitude > 0.01f && Vector3.Dot(rightOffsetDiff.normalized, m_transform.up) > 0) m_extrapolationRight = rightOffsetDiff * m_ikExtrapolation;
////            //else m_extrapolationRight = Vector3.zero;

////            //// apply positional offset
////            //Vector3 targetOffsetLeft = ikLeftOffset + m_extrapolationLeft;
////            //Vector3 targetOffsetRight = ikRightOffset + m_extrapolationRight;

////            //float stiffnessLerpValue = deltaTime * m_ikPlacementStiffness;
////            //m_ikLeftOffset = Vector3.Lerp(m_ikLeftOffset, targetOffsetLeft, stiffnessLerpValue);
////            //m_ikRightOffset = Vector3.Lerp(m_ikRightOffset, targetOffsetRight, stiffnessLerpValue);

////            //// this is a special case catch (which clamps the offsets), when the final offset length gets bigger than the downwards correction offset length (overshooting when moving feet downwards)
////            //if (m_antiDownwardsIntersectionStiffness > 0.01f)
////            //{
////            //    Vector3 changeInBodyPosition = BodyPos- m_prevBodyPosition;
////            //    Vector3 projChangeInBodyPos = Vector3.Project(changeInBodyPosition, -m_transform.up);
////            //    if (Vector3.Dot(projChangeInBodyPos.normalized, -m_transform.up) > 0)
////            //    {
////            //        float clampLerpValue = Mathf.Min(deltaTime * m_antiDownwardsIntersectionStiffness, 1);
////            //        if (ikLeftOffset.magnitude > 0 && m_ikLeftOffset.magnitude > ikLeftOffset.magnitude && Vector3.Dot(m_ikLeftOffset.normalized, -m_transform.up) > 0)
////            //        {
////            //            m_ikLeftOffset = Vector3.Lerp(m_ikLeftOffset, ikLeftOffset, clampLerpValue);
////            //            m_extrapolationLeft = Vector3.Lerp(m_extrapolationLeft, Vector3.zero, clampLerpValue);
////            //        }

////            //        if (ikRightOffset.magnitude > 0 && m_ikRightOffset.magnitude > ikRightOffset.magnitude && Vector3.Dot(m_ikRightOffset.normalized, -m_transform.up) > 0)
////            //        {
////            //            m_ikRightOffset = Vector3.Lerp(m_ikRightOffset, ikRightOffset, clampLerpValue);
////            //            m_extrapolationRight = Vector3.Lerp(m_extrapolationRight, Vector3.zero, clampLerpValue);
////            //        }
////            //    }
////            //}
////            //m_prevBodyPosition = BodyPos;

////            //Vector3 nextIkLeft = ikLeft + m_ikLeftOffset;
////            //Vector3 nextIkRight = ikRight + m_ikRightOffset;
////            ////m_animator.SetIKPosition(AvatarIKGoal.LeftLeg, nextIkLeft);
////            ////m_animator.SetIKPosition(AvatarIKGoal.RightLeg, nextIkRight);

////            ////m_animator.SetIKPositionWeight(AvatarIKGoal.LeftLeg, m_ikPlacementWeight * m_feetResetLerp);
////            ////m_animator.SetIKPositionWeight(AvatarIKGoal.RightLeg, m_ikPlacementWeight * m_feetResetLerp);

////            //// calculate ik rotations
////            //float rotationStiffnessSlerpValue = deltaTime * m_ikRotationStiffness;
////            //if (resultLeft.isGlued || m_onlyAdaptRotationWhenGluedToGround == false)
////            //{
////            //    Vector3 normalLeftFoot = resultLeft.normal;
////            //    Quaternion deltaLeft = Quaternion.FromToRotation(m_transform.up, normalLeftFoot);
////            //    m_ikLeftDeltaRotation = Quaternion.Slerp(m_ikLeftDeltaRotation, deltaLeft, rotationStiffnessSlerpValue);
////            //}
////            //else
////            //{
////            //    m_ikLeftDeltaRotation = Quaternion.Slerp(m_ikLeftDeltaRotation, Quaternion.identity, rotationStiffnessSlerpValue);
////            //}

////            //if (resultRight.isGlued || m_onlyAdaptRotationWhenGluedToGround == false)
////            //{
////            //    Vector3 normalRightFoot = resultRight.normal;
////            //    Quaternion deltaRight = Quaternion.FromToRotation(m_transform.up, normalRightFoot);
////            //    m_ikRightDeltaRotation = Quaternion.Slerp(m_ikRightDeltaRotation, deltaRight, rotationStiffnessSlerpValue);
////            //}
////            //else
////            //{
////            //    m_ikRightDeltaRotation = Quaternion.Slerp(m_ikRightDeltaRotation, Quaternion.identity, rotationStiffnessSlerpValue);
////            //}

////            //Quaternion nextRotLeft = m_ikLeftDeltaRotation * currRotLeft;
////            //Quaternion nextRotRight = m_ikRightDeltaRotation * currRotRight;
////            ////m_animator.SetIKRotation(AvatarIKGoal.LeftLeg, nextRotLeft);
////            ////m_animator.SetIKRotation(AvatarIKGoal.RightLeg, nextRotRight);

////            ////m_animator.SetIKRotationWeight(AvatarIKGoal.LeftLeg, m_ikPlacementWeight * m_feetResetLerp);
////            ////m_animator.SetIKRotationWeight(AvatarIKGoal.RightLeg, m_ikPlacementWeight * m_feetResetLerp);

////            //m_feetResetLerp = 1;


////            //updateEvents(resultLeft, ikLeftTargetPos, resultRight, ikRightTargetPos, m_ikFootHeight, m_ikFootHeight);
////        }

////        public void updateEvents(IKResult resultLeft, Vector3 ikLeftTargetPos, IKResult resultRight, Vector3 ikRightTargetPos, float footHeightLeft, float footHeightRight)
////        {
////            // left
////            float distToGroundLeft = Vector3.Project(resultLeft.bottomPoint - ikLeftTargetPos, m_transform.up).magnitude;
////            float distToLastLeftStepPos = (ikLeftTargetPos - m_lastLeftIKStepEventPos).magnitude;

////            if (m_isWaitingForPlayingLeftFootEvent)
////            {
////                if (distToGroundLeft < (footHeightLeft + m_eventCheckResetDistance) && distToLastLeftStepPos > m_stepLength)
////                {
////                    if (m_onFootstepLeftStart != null) 
////                        m_onFootstepLeftStart.Invoke(m_resultLeft);

////                    m_isWaitingForPlayingLeftFootEvent = false;
////                    m_lastLeftIKStepEventPos = ikLeftTargetPos;
////                }
////            }
////            else
////            {
////                if (distToGroundLeft > (footHeightLeft + m_eventCheckResetDistance) && distToLastLeftStepPos > m_stepLength)
////                {
////                    if (m_onFootstepLeftStop != null) 
////                        m_onFootstepLeftStop.Invoke();

////                    m_isWaitingForPlayingLeftFootEvent = true;
////                    m_lastLeftIKStepEventPos = ikLeftTargetPos;
////                }
////            }

////            // right
////            float distToGroundRight = Vector3.Project(resultRight.bottomPoint - ikRightTargetPos, m_transform.up).magnitude;
////            float distToLastRightStepPos = (ikRightTargetPos - m_lastRightIKStepEventPos).magnitude;

////            if (m_isWaitingForPlayingRightFootEvent)
////            {
////                if (distToGroundRight < (footHeightRight + m_eventCheckResetDistance) && distToLastRightStepPos > m_stepLength)
////                {
////                    if (m_onFootstepRightStart != null) m_onFootstepRightStart.Invoke(m_resultRight);
////                    m_isWaitingForPlayingRightFootEvent = false;
////                    m_lastRightIKStepEventPos = ikRightTargetPos;
////                }
////            }
////            else
////            {
////                if (distToGroundRight > (footHeightRight + m_eventCheckResetDistance) && distToLastRightStepPos > m_stepLength)
////                {
////                    if (m_onFootstepRightStop != null) m_onFootstepRightStop.Invoke();
////                    m_isWaitingForPlayingRightFootEvent = true;
////                    m_lastRightIKStepEventPos = ikRightTargetPos;
////                }
////            }
////        }

////        public void revertUpdateIK(float deltaTime)
////        {
////            // reset to default body center of mass position
////            // reset to default IKs and weights
////            //m_feetResetLerp -= deltaTime * m_ikPlacementStiffness * m_speedForInterpolatingToRespectiveDefaultValues;
////            //m_feetResetLerp = Mathf.Max(m_feetResetLerp, 0);
////            //m_bodyResetLerp -= deltaTime * m_bodyStiffness * m_speedForInterpolatingToRespectiveDefaultValues;
////            //m_bodyResetLerp = Mathf.Max(m_bodyResetLerp, 0);

////            //// body position
////            ////m_animator.bodyPosition = m_animator.bodyPosition + (m_bodyOffset + m_bodyPositionOffset * m_transform.up) * m_bodyPlacementWeight * m_bodyResetLerp;


////            //// ik positions
////            ////var ikLeft = leftFoot.position;
////            ////var ikRight = rightFoot.position;

////            //var ikLeft = IKJob.LeftLeg.GUIDs;
////            //var ikRight = IKJob.RightLeg.GUIDs;
 
////            //m_ikLeftOffset = Vector3.Lerp(Vector3.zero, m_ikLeftOffset, m_feetResetLerp);
////            //m_ikRightOffset = Vector3.Lerp(Vector3.zero, m_ikRightOffset, m_feetResetLerp);
////            //m_extrapolationLeft = Vector3.Lerp(m_extrapolationLeft, Vector3.zero, m_feetResetLerp);
////            //m_extrapolationRight = Vector3.Lerp(m_extrapolationRight, Vector3.zero, m_feetResetLerp);

////            //Vector3 nextIkLeft = ikLeft + m_ikLeftOffset;
////            //Vector3 nextIkRight = ikRight + m_ikRightOffset;

////            ////m_animator.SetIKPosition(AvatarIKGoal.LeftLeg, nextIkLeft);
////            ////m_animator.SetIKPosition(AvatarIKGoal.RightLeg, nextIkRight);

////            ////m_animator.SetIKPositionWeight(AvatarIKGoal.LeftLeg, m_feetResetLerp * m_ikPlacementWeight);
////            ////m_animator.SetIKPositionWeight(AvatarIKGoal.RightLeg, m_feetResetLerp * m_ikPlacementWeight);

////            //// ik rotations
////            ////var currRotLeft = leftFoot.rotation;
////            ////var currRotRight = rightFoot.rotation;

////            //var currRotLeft = IKJob.LeftLeg.Rotation;
////            //var currRotRight = IKJob.RightLeg.Rotation;

////            //m_ikLeftDeltaRotation = Quaternion.Slerp(Quaternion.identity, m_ikLeftDeltaRotation, m_feetResetLerp);
////            //m_ikRightDeltaRotation = Quaternion.Slerp(Quaternion.identity, m_ikRightDeltaRotation, m_feetResetLerp);

////            ////m_animator.SetIKRotation(AvatarIKGoal.LeftLeg, m_ikLeftDeltaRotation * currRotLeft);
////            ////m_animator.SetIKRotation(AvatarIKGoal.RightLeg, m_ikRightDeltaRotation * currRotRight);

////            ////m_animator.SetIKRotationWeight(AvatarIKGoal.LeftLeg, m_feetResetLerp * m_ikPlacementWeight);
////            ////m_animator.SetIKRotationWeight(AvatarIKGoal.RightLeg, m_feetResetLerp * m_ikPlacementWeight);


////            //IKJob = new FootIKJob()
////            //{
////            //    BodyOffset= m_bodyOffset + m_bodyPlacementWeight * m_bodyResetLerp * (m_bodyOffset + m_bodyPositionOffset * m_transform.up),

////            //    LeftLeg = new(nextIkLeft, m_ikLeftDeltaRotation * currRotLeft),
////            //    RightLeg = new(nextIkRight, m_ikRightDeltaRotation * currRotRight),

////            //    LeftFootPositionWeight = m_feetResetLerp * m_ikPlacementWeight,
////            //    RightFootPositionWeight = m_feetResetLerp * m_ikPlacementWeight,

////            //    LeftFootRotationWeight= m_feetResetLerp * m_ikPlacementWeight,
////            //    RightFootRotationWeight = m_feetResetLerp * m_ikPlacementWeight
////            //};

////            //IKScriptPlayable.SetJobData(IKJob);

////            //// events
////            //if (m_isWaitingForPlayingLeftFootEvent == false && m_onFootstepLeftStop != null) 
////            //    m_onFootstepLeftStop.Invoke();

////            //if (m_isWaitingForPlayingRightFootEvent == false && m_onFootstepRightStop != null) 
////            //    m_onFootstepRightStop.Invoke();

////            //m_isWaitingForPlayingLeftFootEvent = true;
////            //m_isWaitingForPlayingRightFootEvent = true;
////            //m_lastLeftIKStepEventPos = nextIkLeft;
////            //m_lastRightIKStepEventPos = nextIkRight;
////        }

////        public IKResult findNewIKPos(Vector3 ikPos, Vector3 ikBottomPoint, float heightOffset, float forwardBias, Vector3 forwardDir, Quaternion rotation, Vector3 origin, Vector3 rayDirection, float maxIKCorrection, float maxCenterOfMassCorrection, float raycastLength, float raycastWidth, int raycastLayer)
////        {
////            float boxCastHeight = Mathf.Max(heightOffset * 0.01f, 0.01f);

////            Vector3 rayOriginPoint = ikBottomPoint - rayDirection * maxIKCorrection - rayDirection * boxCastHeight;
////            Vector3 rayOrigin = rayOriginPoint + forwardBias * forwardDir;

////            debugDrawPoint(rayOrigin, Color.blue);
////            debugDrawPoint(ikPos, Color.yellow);

////            Ray ray = new Ray(rayOrigin, rayDirection);
////            RaycastHit hit;
////            float raycastDistance = maxIKCorrection + maxCenterOfMassCorrection;

////            if (Physics.BoxCast(ray.origin, new Vector3(raycastWidth * 0.5f, boxCastHeight, raycastLength * 0.5f), ray.direction, out hit, rotation, raycastDistance, raycastLayer, m_triggerCollisionInteraction))
////            {
////                Vector3 hitPoint = rayOrigin + hit.distance * rayDirection + boxCastHeight * rayDirection;
////                Vector3 foundBottomPoint = hitPoint - forwardDir * forwardBias;

////                // boxcast slope offset fix
////                Vector3 normalToUse = hit.normal;
////                float alpha = Vector3.Angle(-rayDirection, normalToUse);

////                RaycastHit fixHit;
////                if (Physics.SphereCast(rayOriginPoint - raycastWidth * 0.485f * rayDirection, raycastWidth * 0.485f, rayDirection, out fixHit, raycastDistance, raycastLayer, m_triggerCollisionInteraction))
////                {
////                    if (Mathf.Abs(Vector3.Angle(-rayDirection, fixHit.normal) - alpha) > 1.0f)
////                    {
////                        Vector3 hitpointsVec = fixHit.point - hit.point;
////                        if (hitpointsVec.magnitude > 0.001f)
////                        {
////                            hitpointsVec.Normalize();
////                            Vector3 r = Vector3.Cross(hitpointsVec, -rayDirection).normalized;
////                            Vector3 fixNormal = Vector3.Cross(r, hitpointsVec).normalized;
////                            float fixAlpha = Vector3.Angle(-rayDirection, fixNormal);

////                            if (Mathf.Abs(fixAlpha) < Mathf.Abs(alpha))
////                            {
////                                normalToUse = fixNormal;
////                                alpha = fixAlpha;
////                            }
////                        }
////                    }
////                }

////                if (Mathf.Abs(alpha) > m_maxGroundAngleAtWhichTheGroundIsDetectedAsGround) return new IKResult(ikPos, ikPos, false, ikBottomPoint, -rayDirection);
////                alpha = Mathf.Clamp(alpha, -m_maxGroundAngleTheFeetsCanAdaptTo, m_maxGroundAngleTheFeetsCanAdaptTo);

////                Vector3 pointsDistVec = foundBottomPoint - hit.point;
////                float l = pointsDistVec.magnitude;
////                Vector3 c1 = Vector3.Cross(pointsDistVec.normalized, normalToUse).normalized;
////                Vector3 c2 = Vector3.Cross(normalToUse, c1).normalized;
////                float cosAlpha = Mathf.Cos(alpha * Mathf.Deg2Rad);

////                if (cosAlpha > 0 && cosAlpha < 0.01f) cosAlpha = 0.01f;
////                else if (cosAlpha < 0 && cosAlpha > -0.01f) cosAlpha = -0.01f;

////                float ll = l / cosAlpha;
////                Vector3 angleOffset = Vector3.Project(c2 * ll, -rayDirection);
////                foundBottomPoint = foundBottomPoint + angleOffset;

////                float realHeightOffset = heightOffset / cosAlpha;

////                Vector3 foundIKPos = foundBottomPoint - rayDirection * realHeightOffset;

////                // the ik point is not allowed to be lower than the original ik
////                bool isGlued = true;
////                Vector3 gluedIkPos = foundIKPos;
////                Vector3 distVec = foundIKPos - ikPos;
////                if (Vector3.Dot(distVec.normalized, rayDirection) > 0)
////                {
////                    foundIKPos = ikPos;
////                    isGlued = false;
////                }

////                debugDrawPoint(hitPoint, Color.blue);
////                debugDrawPoint(foundBottomPoint, Color.red);
////                debugDrawPoint(foundIKPos, Color.cyan);
////                debugDrawArrow(rayOrigin, hitPoint, Color.blue);

////                return new IKResult(foundIKPos, gluedIkPos, isGlued, foundBottomPoint, normalToUse);
////            }

////            return new IKResult(ikPos, ikPos, false, ikBottomPoint, -rayDirection);
////        }

////        private void debugDrawPoint(Vector3 point, Color color)
////        {
////#if UNITY_EDITOR
////            Debug.DrawLine(point - Vector3.right * 0.025f, point + Vector3.right * 0.025f, color);
////            Debug.DrawLine(point - Vector3.forward * 0.025f, point + Vector3.forward * 0.025f, color);
////            Debug.DrawLine(point - Vector3.up * 0.025f, point + Vector3.up * 0.025f, color);
////#endif
////        }

////        private void debugDrawArrow(Vector3 start, Vector3 end, Color color)
////        {
////#if UNITY_EDITOR
////            Debug.DrawLine(start, end, color);
////            Debug.DrawLine(end, end + Vector3.up * 0.025f + Vector3.right * 0.025f, color);
////            Debug.DrawLine(end, end + Vector3.up * 0.025f - Vector3.right * 0.025f, color);
////#endif
////        }

      
////    }


//    public class IKResult
//    {
//        public IKResult(Vector3 _ikPos, Vector3 _gluedIkPos, bool _isGlued, Vector3 _bottomPoint, Vector3 _normal)
//        {
//            ikPos = _ikPos;
//            gluedIKPos = _gluedIkPos;
//            isGlued = _isGlued;
//            bottomPoint = _bottomPoint;
//            normal = _normal;
//        }

//        public Vector3 ikPos;
//        public Vector3 gluedIKPos;
//        public bool isGlued;
//        public Vector3 bottomPoint;
//        public Vector3 normal;
//    }

//    [System.Serializable]
//    public class IKResultEvent : UnityEngine.Events.UnityEvent<IKResult>
//    {
//    }

//}

