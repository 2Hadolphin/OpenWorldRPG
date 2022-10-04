using System;
namespace Return.Items
{
    //[Flags]
    public enum PostureAdjustMode
    {
        None=0,

        /// <summary>
        /// RootTransform of hands(UpChestBone).
        /// </summary>
        RootSpace=1,

        /// <summary>
        /// StreamTransformHandle space which calculate offsetPosition then offsetRotation, final solve ik if required
        /// </summary>
        AdditiveHandleSpace=2,

        /// <summary>
        /// Align to prime handle (right hand streamHandle), use for item handle
        /// </summary>
        PrimeHandleSpace=4,

        /// <summary>
        /// Align to prime handle (right hand streamHandle) and add offset
        /// </summary>
        AdditivePrimeHandleSpace=8,

        /// <summary>
        /// Align to item handle (item slot streamHandle) and add offset
        /// </summary>
        AdditiveItemHandleSpace=16,
    }
}