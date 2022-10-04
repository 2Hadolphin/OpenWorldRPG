using System;
using UnityEngine.Assertions;
using Return.Humanoid.IK;
using Return.Creature;

namespace Return
{

    #region Interact

    /// <summary>
    /// Class to post two bone IK data event 
    /// </summary>
    public class InteractAdapter : IDisposable
    {
        public InteractAdapter(object sender, Symmetry enableHand)
        {
            Assert.IsNotNull(sender);
            Owner = sender;
            PermissionInteractor = enableHand;
        }

        /// <summary>
        /// Use to valid ik sender qualify. (**Motion handle)
        /// </summary>
        public object Owner { get; protected set; }

        /// <summary>
        /// Dispose adapter.
        /// </summary>
        public event Action<object, Limb> OnDispose;

        /// <summary>
        /// Event data to apply.
        /// </summary>
        public event EventHandler<TwoBoneIKDataArg> IKHandle;


        #region Verify
        /// <summary>
        /// Handle has been apply to use
        /// </summary>
        public readonly Symmetry PermissionInteractor;

        /// <summary>
        /// Handle to use
        /// </summary>
        protected Symmetry ChosenInteractor;

        protected bool _Init = false;

        /// <summary>
        /// RegisterHandler handles and activate, also register the dispose event 
        /// </summary>
        /// <param name="enableHandle"> handle want to use</param>
        public virtual void Init(Symmetry enableHandle)
        {
            if ((PermissionInteractor | enableHandle).HasFlag(Symmetry.Deny))
                Dispose();

            if (PermissionInteractor.HasFlag(Symmetry.Both))
                switch (PermissionInteractor ^ enableHandle)
                {
                    case Symmetry.Both:
                        Assert.AreEqual(PermissionInteractor, enableHandle);
                        Dispose();
                        break;

                    case Symmetry.Deny:
                        Assert.AreEqual(PermissionInteractor, enableHandle);
                        Dispose();
                        break;

                    case Symmetry.Right:
                        DisposeRightHand();
                        break;

                    case Symmetry.Left:
                        DisposeLeftHand();
                        break;
                }
            else
                switch (enableHandle)
                {
                    case Symmetry.Both:
                        if (!PermissionInteractor.HasFlag(enableHandle))
                            Dispose();
                        break;

                    case Symmetry.Deny:
                        throw new NotImplementedException();

                    case Symmetry.Right:
                        if (PermissionInteractor.HasFlag(enableHandle))
                            DisposeLeftHand();
                        else
                            Dispose();
                        break;
                    case Symmetry.Left:
                        if (PermissionInteractor.HasFlag(enableHandle))
                            DisposeRightHand();
                        else
                            Dispose();
                        break;
                }
            _Init = true;
        }

        void DisposeHandle(Limb limb)
        {
            OnDispose.Invoke(Owner, limb);
            _Init = false;
        }

        void DisposeRightHand()
        {
            DisposeHandle(Limb.RightHand);
        }
        void DisposeLeftHand()
        {
            DisposeHandle(Limb.LeftHand);
        }


        #endregion

        public virtual void UpdateIK(TwoBoneIKDataArg IKarg)
        {
            if (_Init)
                IKHandle.Invoke(Owner, IKarg);
        }

        public void Dispose()
        {
            DisposeRightHand();
            DisposeLeftHand();
        }
    }

    #endregion



}
