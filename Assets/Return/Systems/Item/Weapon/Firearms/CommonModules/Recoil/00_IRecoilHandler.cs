using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
    public interface IRecoilHandler
    {
        //void Enable();
        //void Disable();

        void Recoil();

        float accuracyKickMultiplier { get; set; }

        float hipAccuracyKick { get; }
        float hipAccuracyRecover { get; }
        float sightedAccuracyKick { get; }
        float sightedAccuracyRecover { get; }

        void SetRecoilMultiplier(float move, float rotation);

        event UnityAction onRecoil;
    }
}