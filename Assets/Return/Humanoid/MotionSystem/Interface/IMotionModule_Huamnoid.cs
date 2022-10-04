using Return.Creature;
using Return.Humanoid;
using Return.Humanoid.Motion;
using System;

namespace Return.Motions
{


    public interface IMotionModule_Huamnoid : IHumanoidMotionModule
    {
        [Obsolete]
        MotionModulePreset_Humanoid GetData { get; }
    }

}

