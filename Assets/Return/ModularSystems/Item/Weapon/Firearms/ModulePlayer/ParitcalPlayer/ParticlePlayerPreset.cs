using UnityEngine;

namespace Return.Items.Weapons.Firearms
{
    public class ParticlePlayerPreset : FirearmsModulePreset<ParticlePlayer> 
    {
        [SerializeField]
        public ParticleSystem Muzzle;
        [SerializeField]
        public ParticleSystem Eject;


    }

}