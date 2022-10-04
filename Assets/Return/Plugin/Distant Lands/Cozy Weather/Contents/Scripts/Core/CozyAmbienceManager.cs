// Distant Lands 2021.



using DistantLands.Cozy.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DistantLands.Cozy
{

    public class CozyAmbienceManager : CozyModule
    {

        [Header("Profile Reference")]
        public AmbienceForecastProfile m_Ambiences;


        public AmbienceProfile currentAmbienceProfile;
        private float m_AmbienceTimer;


        public AmbienceProfile[] ambienceProfiles { get { return m_Ambiences.ambienceProfiles.ToArray(); } }

        void Awake()
        {
            if (!enabled)
                return;

            base.SetupModule();

            SetNextAmbience();

            foreach (AmbienceProfile i in ambienceProfiles)
            {
                if (i.particleFX && i.useVFX)
                {
                    ParticleSystem system = Instantiate(i.particleFX, transform.GetChild(2)).GetComponent<ParticleSystem>();

                    if (system.GetComponent<CozyAmbientParticles>())
                        system.GetComponent<CozyAmbientParticles>().ambienceProfile = i;
                    else
                        system.gameObject.AddComponent<CozyAmbientParticles>().ambienceProfile = i;

                    if (triggerManagerModule)
                    {
                        ParticleSystem.TriggerModule triggers = system.trigger;

                        triggers.enter = ParticleSystemOverlapAction.Kill;
                        triggers.inside = ParticleSystemOverlapAction.Kill;
                        for (int j = 0; j < triggerManagerModule.cozyTriggers.Count; j++)
                            triggers.SetCollider(j, triggerManagerModule.cozyTriggers[j]);
                    }

                }
            }

        }

        // Update is called once per frame
        void Update()
        {
            m_AmbienceTimer -= Time.deltaTime * weatherSphere.perennialProfile.ModifiedTickSpeed();

            if (m_AmbienceTimer <= 0)
            {
                SetNextAmbience();
            }
        }

        public void SetNextAmbience()
        {

            currentAmbienceProfile = WeightedRandom(m_Ambiences.ambienceProfiles.ToArray());
            if (audioModule)
                audioModule.ChangeSound(currentAmbienceProfile);
            m_AmbienceTimer += Random.Range(currentAmbienceProfile.playTime.x, currentAmbienceProfile.playTime.y);


        }

        public void SkipTicks(float ticksToSkip)
        {

            m_AmbienceTimer -= ticksToSkip;

        }

        public AmbienceProfile WeightedRandom(AmbienceProfile[] profiles)
        {

            AmbienceProfile i = null;
            List<float> floats = new List<float>();
            float totalChance = 0;


            foreach (AmbienceProfile k in profiles)
            {
                float chance;

                if (k.dontPlayDuring.Contains(weatherSphere.weatherProfile))
                    chance = 0;
                else
                    chance = !climateModule ? 1 : k.GetChance(climateModule.GetTemprature(false), climateModule.GetHumidity(), weatherSphere.perennialProfile.YearPercentage(), weatherSphere.perennialProfile.DayPercentage());

                floats.Add(chance);
                totalChance += chance;
            }

            float selection = Random.Range(0, totalChance);

            int m = 0;
            float l = 0;

            while (l <= selection)
            {
                if (selection >= l && selection < l + floats[m])
                {
                    i = profiles[m];
                    break;
                }
                l += floats[m];
                m++;

            }

            if (!i)
            {
                i = profiles[0];
            }

            return i;
        }

        public float TimeTillNextAmbience()
        {
            return m_AmbienceTimer;
        }
    }
}