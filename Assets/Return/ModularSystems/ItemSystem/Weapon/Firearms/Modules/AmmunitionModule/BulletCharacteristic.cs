using UnityEngine;

namespace Return.Items.Weapons
{
    [System.Serializable]
    public class BulletCharacteristic : PresetDatabase
    {
        public string Name;
        /// <summary>
        /// Execute this method to enable bullet effect
        /// </summary>
        public virtual void LoadEffect(GameObject @bullet)
        {
            // edit audio light texture
            return;
        }
        /// <summary>
        /// Execute this method to update bullet effect
        /// </summary>
        public virtual void mUpdate(GameObject @bullet)
        {
            // update light audio texture
            return;
        }

        /// <summary>
        /// Execute this method to finish bullet effect
        /// </summary>
        public virtual void Terminal(GameObject @bullet)
        {
            // do decal
            return;
        }
    }
}
