using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Items.Weapons
{
    public class BulletCharacteristic_Bypass : BulletCharacteristic
    {
        public AudioClip BypassEffect;
        public float Distance=1f;
        public override void LoadEffect(GameObject bullet)
        {
            var audio = bullet.InstanceIfNull<AudioSource>();
            audio.loop = true;
            audio.clip = BypassEffect;
            audio.maxDistance = Distance;
            audio.minDistance = 0;

            audio.PlayDelayed(0.1f);
        }
        public override void mUpdate(GameObject bullet)
        {
            return;
        }

        public override void Terminal(GameObject bullet)
        {
            return;
        }
    }
}