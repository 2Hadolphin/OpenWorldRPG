using UnityEngine;
using UnityEngine.UI;

namespace Return.mGUI
{
    public class WeaponGUI : UI_Base
    {

        public Text ammoCountText;

        public string ammo_;

        private void Start()
        {
        }


        public void AmmoController(int ammo)
        {
            int Ammo_ = ammo;
            //print(Ammo_);
            ammo_ = Ammo_.ToString();

            ammoCountText.text = ammo_;

        }


    }
}