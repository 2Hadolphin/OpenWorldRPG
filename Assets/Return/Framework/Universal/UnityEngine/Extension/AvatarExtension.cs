using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public static class AvatarExtension
    {
        public static void CleanHumanoidPart(this AvatarMask mask)
        {
            var length = (int)AvatarMaskBodyPart.LastBodyPart;

            for (int i = 0; i < length; i++)
            {
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            }
        }

        /// <summary>
        /// Apply all human body part in avatar mask
        /// </summary>
        public static void SetAllAvatarBodyPart(this AvatarMask mask, bool enable = false)
        {
            var length = (int)AvatarMaskBodyPart.LastBodyPart;

            for (int i = 0; i < length; i++)
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, enable);
        }

        /// <summary>
        /// Remove all human body part in avatar mask
        /// </summary>
        public static void SetAvatarBodyParts(this AvatarMask mask, bool enable, params AvatarMaskBodyPart[] parts)
        {
            mask.SetAllAvatarBodyPart(!enable);

            var length = parts.Length;

            for (int i = 0; i < length; i++)
                mask.SetHumanoidBodyPartActive(parts[i], enable);
        }

    }
}