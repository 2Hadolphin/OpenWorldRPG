using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using System.Collections;
using Return.Humanoid.IK;
using Return.Modular;

namespace Return.Humanoid
{
    public class PlayerWeaponIK : ModuleHandler
    {
        #region Resolver

        public override void InstallResolver(IModularResolver resolver)
        {

        }


        #endregion


        [SerializeField]
        private Transform IK_Right = null;
        [SerializeField]
        private Transform IK_Left = null;
        [SerializeField]
        private Transform TargetPoint = null;

        private bool EnableRight;
        private bool EnableLeft;

        private void ClampWeapon()
        {
            if (EnableRight)
                IK_Right.LookAt(TargetPoint, Vector3.up);
            if (EnableLeft)
                IK_Left.LookAt(TargetPoint, Vector3.up);
        }

        //public void SetOrigin(Vector3 pos,IKManager_Humanoid.IKPart side)
        //{
        //    switch (side)
        //    {
        //        case IKManager_Humanoid.IKPart.LeftHand:
        //            EnableLeft = true;
        //            IK_Left.position = pos;
        //            break;
        //        case IKManager_Humanoid.IKPart.RightHand:
        //            EnableRight = true;
        //            IK_Right.position = pos;
        //            break;
        //    }
        //}
        private void OnEnable()
        {
            if (IK_Right==null || IK_Left == null)
            {
                this.enabled = false;
                return;
            }

            if (TargetPoint == null)
            {
                this.enabled = false;
                return;
            }
        }

        private void OnDisable()
        {
            EnableRight = false;
            EnableLeft = false;
        }

        private void FixedUpdate()
        {
            ClampWeapon();
        }

        public void UnClampWeapon()
        {

        }


    }
}