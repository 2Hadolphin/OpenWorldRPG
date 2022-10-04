using System.Collections.Generic;
using UnityEngine;

public static partial class mHumanoidExtension
{
    public static HumanBodyBones GetParent(this HumanBodyBones bone)
    {
        return (HumanBodyBones)HumanTrait.GetParentBone((int)bone);
    }

    public static int Parse(this HumanBodyBones bone)
    {
        return (int)bone;
    }

    public static HumanPartDof ToHumanDof(this AvatarMaskBodyPart part)
    {
        switch (part)
        {
            case AvatarMaskBodyPart.Body:
                return HumanPartDof.Body;
            case AvatarMaskBodyPart.Head:
                return HumanPartDof.Head;
            case AvatarMaskBodyPart.LeftLeg:
                return HumanPartDof.LeftLeg;
            case AvatarMaskBodyPart.RightLeg:
                return HumanPartDof.RightLeg;
            case AvatarMaskBodyPart.LeftArm:
                return HumanPartDof.LeftArm;
            case AvatarMaskBodyPart.RightArm:
                return HumanPartDof.RightArm;
            case AvatarMaskBodyPart.LeftFingers:
                return HumanPartDof.LeftThumb;
            case AvatarMaskBodyPart.RightFingers:
                return HumanPartDof.RightThumb;
        }

        throw new KeyNotFoundException(part.ToString());
    }
}
