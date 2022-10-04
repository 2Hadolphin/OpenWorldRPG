using UnityEngine;


namespace Return.Items.Weapons
{
    public class FirearmsEvent : PresetDatabase
    {
        [SerializeField]
        public UTags OnCameraAnimationStart;

        [SerializeField]
        public UTags OnCameraAnimationStop;

        [SerializeField]
        public UTags OnEject;

        [SerializeField]
        public UTags OnLoaded;

        [SerializeField]
        protected UTags m_OnMuzzlePlay;
        public UTags OnMuzzlePlay => m_OnMuzzlePlay;



    }
}