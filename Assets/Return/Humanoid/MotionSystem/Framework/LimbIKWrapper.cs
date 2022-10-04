using UnityEngine;
using UnityEngine.Animations.Rigging;
using Return.Creature;


namespace Return.Humanoid
{
    /// <summary>
    /// Two bone IK wrapper
    /// </summary>
    public class LimbIKWrapper
    {

        /// <summary>
        /// Container to cache constraint and logic control
        /// </summary>
        /// <param name="hash">owner</param>
        /// <param name="limb">body part could IK</param>
        /// <param name="ik">constraint</param>
        public LimbIKWrapper(int hash, Limb limb, TwoBoneIKConstraint ik)
        {
            Hash = hash;
            Limb = limb;
            Qualified = false;

            Root = ik.data.root; // WrapTransform(ik.data.root);
            Tip = ik.data.tip; //new WrapTransform(ik.data.tip);
            Target = ik.data.target; // new WrapTransform(ik.data.target);
            Hint = ik.data.hint; // new WrapTransform(ik.data.hint);

            TwoBoneIK = ik;
        }

        #region Verify

        private readonly int Hash;
        public readonly Limb Limb;
        public bool Qualified { get; private set; }

        public virtual void VerifyIK(int moduleHash, bool enable, Limb limb)
        {
            if (!Limb.Equals(limb) || !Hash.Equals(moduleHash))
                return;

            Qualified = enable;
        }

        #endregion

        #region IK
        protected readonly TwoBoneIKConstraint TwoBoneIK;

        //public ReadOnlyTransform tip => TwoBoneIK.data.tip;
        //public ReadOnlyTransform mid => TwoBoneIK.data.mid;
        //public ReadOnlyTransform root => TwoBoneIK.data.root;
        //public ReadOnlyTransform target => TwoBoneIK.data.target;
        //public ReadOnlyTransform hint => TwoBoneIK.data.hint;

        /// <summary>
        /// Set offset value between IK target and skeleton
        /// </summary>
        //public virtual void SetGoalOffset(PR pr)
        //{
        //    if (!Qualified)
        //        return;
        //    TwoBoneIK.enabled = false;
        //    TwoBoneIK.data.target.SetLocalPR(pr);
        //    TwoBoneIK.enabled = true;
        //}

        public Vector3 GoalPosition
        {
            get => TwoBoneIK.data.target.position;
            set
            {
                if (Qualified)
                    TwoBoneIK.data.target.position = value;
            }
        }

        public Vector3 GoalLocalPosition
        {
            get => TwoBoneIK.data.root.InverseTransformPoint(TwoBoneIK.data.target.position);
            set
            {
                if (Qualified)
                    TwoBoneIK.data.target.position = TwoBoneIK.data.root.TransformPoint(value);
            }
        }

        public Quaternion GoalRotation
        {
            get => TwoBoneIK.data.target.rotation;
            set => TwoBoneIK.data.target.rotation = value;
        }

        public Quaternion GoalLocalRotation
        {
            get => TwoBoneIK.data.target.localRotation;//rotation * Quaternion.Inverse(TwoBoneIK.data.root.rotation);
            set => TwoBoneIK.data.target.rotation = value;
        }

        public Vector3 HintPosition
        {
            get => TwoBoneIK.data.hint.position;
            set => TwoBoneIK.data.hint.position = value;
        }
        public Vector3 HintLocalPosition
        {
            get => TwoBoneIK.data.root.InverseTransformPoint(TwoBoneIK.data.hint.position);
            set
            {
                if (Qualified)
                    TwoBoneIK.data.hint.position = TwoBoneIK.data.root.TransformPoint(value);
            }
        }

        public float weight
        {
            get => TwoBoneIK.weight;
            set
            {
                if (Qualified)
                    TwoBoneIK.weight = value;
            }
        }

        public float hintWeight
        {
            get => TwoBoneIK.data.hintWeight;
            set
            {
                if (Qualified)
                    TwoBoneIK.data.hintWeight = value;
            }
        }

        public bool maintainTargetPositionOffset
        {
            get => TwoBoneIK.data.maintainTargetPositionOffset;
            set
            {
                if (Qualified)
                    TwoBoneIK.data.maintainTargetPositionOffset = value;
            }
        }

        public float targetPositionWeight
        {
            get => TwoBoneIK.data.targetPositionWeight;
            set
            {
                if (Qualified)
                    TwoBoneIK.data.targetPositionWeight = value;
            }
        }

        public bool maintainTargetRotationOffset
        {
            get => TwoBoneIK.data.maintainTargetRotationOffset;
            set
            {
                if (Qualified)
                    TwoBoneIK.data.maintainTargetRotationOffset = value;
            }
        }

        public float targetRotationWeight
        {
            get => TwoBoneIK.data.targetRotationWeight;
            set
            {
                if (Qualified)
                    TwoBoneIK.data.targetRotationWeight = value;
            }
        }

        public Transform Root { get; protected set; }
        public Transform Tip { get; protected set; }
        public Transform Target { get; protected set; }
        public Transform Hint { get; protected set; }

        public void Reset()
        {
            if (Qualified)
                TwoBoneIK.Reset();
        }
        #endregion

        public static implicit operator bool(LimbIKWrapper wrapper)
        {
            return wrapper.Qualified;
        }
    }

}