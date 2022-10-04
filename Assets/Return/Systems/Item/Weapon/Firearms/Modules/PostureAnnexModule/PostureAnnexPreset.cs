using Return.Humanoid.Motion;
using Return.Items.Weapons.Firearms;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Items.Weapons
{
    [PropertyTooltip("**Anex ** Pose adjust data **Base firearms performance parameters")]
    public class PostureAnnexPreset : FirearmsModulePreset<PostureAnnexModule>
    {
        //public m_Location[] ForeGrips;
        //public m_Location[] Mount;
        //public m_Location[] Grips;
        //public m_Location[] Stocks;

        public MotionModule_FirearmsHandlePreset HandlerModule;
    }
}