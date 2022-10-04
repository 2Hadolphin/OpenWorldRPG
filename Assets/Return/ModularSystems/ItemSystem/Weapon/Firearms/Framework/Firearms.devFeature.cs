using System;

namespace Return.Items.Weapons.Firearms
{
    public partial class Firearms
    {
        class devFeature
        {

            [Obsolete]
            /// <summary>
            /// Rate of fire <RPS>
            /// </summary>
            public virtual BlackBoard_Summary<object> Rof { get; protected set; } = new BlackBoard_Summary<object>();

            /// <summary>
            /// Returns the current accuracy of the gun.
            /// </summary>
            public virtual float Accuracy { get; set; }

            /// <summary>
            /// Returns the weight with all modules of the gun.
            /// </summary>
            public virtual float Weight { get; set; }


            /// <summary>
            /// Effective Distance
            /// </summary>
            public float Range { get; set; }

            /// <summary>
            /// Defines how fast this gun will be inaccurate due the character movement. (Read Only)
            /// </summary>
            public float DecreaseRateByWalking;

            /// <summary>
            /// Defines how fast this gun will be inaccurate due constant shooting. (Read Only)
            /// </summary>
            public float DecreaseRateByShooting;

        }
    }
}