using Return.Framework.PhysicController;

namespace Return.Motions
{
    public interface IMotionHandler
    {
        IControllerMotor Motor { get; }

        void SetMotionModule(IKCCVelocityModule module);
        void RemoveMotionModule(IKCCVelocityModule module);

        void SetMotionModule(IKCCAngularVelocityModule module);
        void RemoveMotionModule(IKCCAngularVelocityModule module);


        /// <summary>
        /// Velocity response speed.
        /// </summary>
        public float PositionBias { get; set; }

        /// <summary>
        /// Angular velocity response speed.
        /// </summary>
        public float RotationBias { get; set; }
    }
}
