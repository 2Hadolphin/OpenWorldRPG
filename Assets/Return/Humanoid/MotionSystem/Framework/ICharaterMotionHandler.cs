using Return.Framework.PhysicController;

namespace Return.Motions
{
    public interface ICharaterMotionHandler : IMotionHandler
    {
        IControllerMotor IMotionHandler.Motor => Motor;

        /// <summary>
        /// Controller motor to get status.
        /// </summary>
        new ICharacterControllerMotor Motor { get; }


        /// <summary>
        /// Blend velocity between root motion and module.
        /// </summary>
        public float VelocityBlendRatio { get; set; }

        /// <summary>
        /// Blend angular velocity between root motion and module.
        /// </summary>
        public float RotationBlendRatio { get; set; }



    }
}
