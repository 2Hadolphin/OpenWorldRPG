

using UnityEngine;

namespace Return.Humanoid.Animation
{
    public class mAvatarMaskUtility
    {

        public static AvatarMask Combine(AvatarMask a, AvatarMask b)
        {
            var length = (int)AvatarMaskBodyPart.LastBodyPart;
            var avatarMask = new AvatarMask();
            for (int i = 0; i < length; i++)
            {
                var maskPart = (AvatarMaskBodyPart)i;
                avatarMask.SetHumanoidBodyPartActive(maskPart,
                            a.GetHumanoidBodyPartActive(maskPart) |
                            b.GetHumanoidBodyPartActive(maskPart)
                    );
            }

            return avatarMask;
        }
    }
}