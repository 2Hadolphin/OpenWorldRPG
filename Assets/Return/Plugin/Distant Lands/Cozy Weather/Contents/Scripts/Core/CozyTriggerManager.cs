// Distant Lands 2021.



using System.Collections.Generic;
using UnityEngine;

namespace DistantLands.Cozy
{
    public class CozyTriggerManager : CozyModule
    {


        [Tooltip("The layer mask that contains all triggers that stop weather FX from playing.")]
        public string cozyTriggerTag = "FX Block Zone";

        [HideInInspector]
        public List<Collider> cozyTriggers;


        // Start is called before the first frame update
        void Awake()
        {

            if (!enabled)
                return;

            base.SetupModule();

            foreach (Collider i in FindObjectsOfType<Collider>())
            {

                if (i.gameObject.tag == cozyTriggerTag)
                    cozyTriggers.Add(i);

            }
        }

    }
}