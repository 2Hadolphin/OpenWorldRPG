using Return.Motions;

namespace Return.Humanoid
{
    /// <summary>
    /// Humanoid limb sequence
    /// </summary>
    public interface ISequence
    {
        /// <summary>
        /// Remove all motion and stop accept request.
        /// </summary>
        void Dispose();

        /// <summary>
        /// Can this motion execute immediately.
        /// </summary>
        bool CanMotion(IMotionModule_Huamnoid module, bool autoAdd = true);

        /// <summary>
        /// Remove current or in queue motion.
        /// </summary>
        void RemoveMotion(IMotionModule_Huamnoid module);

        /// <summary>
        /// Re-schedule module motions.
        /// </summary>
        /// <param name="ignoreModule">module to ignore queue in this call</param>
        void SubstituteMotion(IMotionModule_Huamnoid ignoreModule = null);
    }

}