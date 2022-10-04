// Distant Lands 2021.



using DistantLands.Cozy.Data;
using System.Collections.Generic;
using UnityEngine;


namespace DistantLands.Cozy
{
    public class CozyAmbientParticles : MonoBehaviour
    {
        public AmbienceProfile ambienceProfile;
        [SerializeField]
        private float m_TransitionSpeed = 1;

        private CozyAmbienceManager m_weatherManager;

        [System.Serializable]
        public class ParticleType
        {

            public ParticleSystem particleSystem;
            public float emissionAmount;



        }

        public List<ParticleType> m_ParticleTypes;


        // Start is called before the first frame update
        void Awake()
        {

            m_weatherManager = FindObjectOfType<CozyAmbienceManager>();

            foreach (ParticleSystem i in GetComponentsInChildren<ParticleSystem>())
            {
                ParticleType j = new ParticleType();
                j.particleSystem = i;
                j.emissionAmount = i.emission.rateOverTime.constant;
                m_ParticleTypes.Add(j);
            }


            foreach (ParticleType i in m_ParticleTypes)
            {

                ParticleSystem.EmissionModule k = i.particleSystem.emission;
                ParticleSystem.MinMaxCurve j = k.rateOverTime;

                if (ambienceProfile != m_weatherManager.currentAmbienceProfile)
                    j.constant = 0;

                k.rateOverTime = j;


            }



        }

        // Update is called once per frame
        void Update()
        {

            if (m_weatherManager.currentAmbienceProfile == ambienceProfile)
            {
                foreach (ParticleType i in m_ParticleTypes)
                {
                    LerpParticles(i, i.emissionAmount);
                }
            }
            else foreach (ParticleType i in m_ParticleTypes)
                {
                    LerpParticles(i, 0);
                }

        }

        public void LerpParticles(ParticleType particle, float amount)
        {

            ParticleSystem.EmissionModule i = particle.particleSystem.emission;
            ParticleSystem.MinMaxCurve j = i.rateOverTime;


            if (amount == 0)
                j.constant = Mathf.Lerp(j.constant, amount, Time.deltaTime * m_TransitionSpeed * 0.2f);
            else
                j.constant = Mathf.Lerp(j.constant, amount, Time.deltaTime * m_TransitionSpeed * 0.025f);

            if (Mathf.Abs(amount - j.constant) < 1)
                j = amount;


            i.rateOverTime = j;


        }
    }
}