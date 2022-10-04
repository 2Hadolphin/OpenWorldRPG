// Distant Lands 2021.



using DistantLands.Cozy.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace DistantLands.Cozy
{
    public class CozyAudio : CozyModule
    {

        [HideInInspector]
        public GameObject audioSource;

        [HideInInspector]
        public AudioMixerGroup weatherFXMixer;
        [HideInInspector]
        public AudioMixerGroup ambienceFXMixer;
        public float transitionTime = 5;

        
        [System.Serializable]
        public class ProfileRelation
        {

            public AudioSource audioSource;
            public WeatherProfile profile;
            public AmbienceProfile ambienceProfile;

        }

        public List<ProfileRelation> m_WeatherAudioSources;
        public List<ProfileRelation> m_AmbienceAudioSources;

        public WeatherProfile currentWeather;
        public AmbienceProfile currentAmbience;



        // Start is called before the first frame update
        void Start()
        {

            if (!enabled)
                return;

            base.SetupModule();

            Transform parent = new GameObject().transform;
            parent.parent = transform;
            parent.localPosition = Vector3.zero;
            parent.localEulerAngles = Vector3.zero;
            parent.localScale = Vector3.one;
            parent.name = "Audio";


            if (forecastModule)
            foreach (WeatherProfile i in forecastModule.forecastProfile.profilesToForecast)
            {

                if (i.soundFX && i.useVFX)
                {
                    AudioSource j = Instantiate(audioSource, parent).GetComponent<AudioSource>();
                    j.outputAudioMixerGroup = weatherFXMixer;
                    j.clip = i.soundFX;
                    j.gameObject.name = i.name;
                    j.volume = 0;
                    j.Play();
                    if (weatherSphere.weatherProfile == i)
                    j.volume = i.FXVolume;

                    ProfileRelation k = new ProfileRelation
                    {
                        audioSource = j,
                        profile = i
                    };

                    m_WeatherAudioSources.Add(k);

                }
            }

            if (ambienceManagerModule)
            foreach (AmbienceProfile i in ambienceManagerModule.ambienceProfiles)
            {
                if (i.soundFX && i.useVFX)
                {
                    AudioSource j = Instantiate(audioSource, parent).GetComponent<AudioSource>();
                    j.outputAudioMixerGroup = ambienceFXMixer;
                    j.clip = i.soundFX;
                    j.gameObject.name = i.name;
                    j.volume = 0;
                    j.Play();
                    if (ambienceManagerModule.currentAmbienceProfile == i)
                        j.volume = i.FXVolume;

                    ProfileRelation k = new ProfileRelation();
                    k.audioSource = j;
                    k.ambienceProfile = i;

                    m_AmbienceAudioSources.Add(k);

                }
            }


        }

        public void ChangeSound(WeatherProfile profile)
        {

            if (profile == currentWeather)
                return;

            foreach (ProfileRelation i in m_WeatherAudioSources)
            {

                if (i.profile == profile)
                    StartCoroutine(LerpFXVolume(i, profile.FXVolume, transitionTime));

                if (i.profile == currentWeather)
                    StartCoroutine(LerpFXVolume(i, 0, transitionTime));

            }

            currentWeather = profile;

        }
        public void ChangeSound(AmbienceProfile profile)
        {

            if (profile == currentAmbience)
                return;

            foreach (ProfileRelation i in m_AmbienceAudioSources)
            {

                if (i.ambienceProfile == profile)
                    StartCoroutine(LerpFXVolume(i, profile.FXVolume, transitionTime));

                if (i.ambienceProfile == currentAmbience)
                    StartCoroutine(LerpFXVolume(i, 0, transitionTime));

            }

            currentAmbience = profile;

        }

        public IEnumerator LerpFXVolume(ProfileRelation relation, float targetVolume, float transitionTime)
        {

            float i = transitionTime;
            float originalVolume = relation.audioSource.volume;

            while (i > 0)
            {

                relation.audioSource.volume = Mathf.Lerp(originalVolume, targetVolume, 1 - (i / transitionTime));

                yield return null;
                i -= Time.deltaTime;

            }


        }
    }
}