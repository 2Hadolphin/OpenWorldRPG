using Return.Motions;

namespace Return.Humanoid.Motion
{
    /// <summary>
    /// Humanoid motion module (**inspect **avoid collosion 
    /// </summary>
    public class MotionModule_ItemHandle : MonoMotionModule_Humanoid
    {
        public MotionModule_ItemHandlePreset Data { get; protected set; }

        public override MotionModulePreset_Humanoid GetData => Data;


        protected override void LoadData(Humanoid.MotionModulePreset data)
        {
            Data = data as MotionModule_ItemHandlePreset;
        }

     
        public virtual void LoadHandPose()
        {

        }



        #region Behaviour ** call by item module


        public virtual void AvoidCollision()
        {

        }



        public virtual void Unequip()//??????
        {
            
        }


        public virtual void Inspect()
        {

        }

        /// <summary>
        /// Dequip
        /// </summary>
        public virtual void Discard()//??????
        {

        }

        #endregion


        public override int CompareTo(IHumanoidMotionModule other)
        {
            return -1;
        }
    }
}