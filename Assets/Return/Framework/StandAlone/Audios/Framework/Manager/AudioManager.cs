#define UniTask

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.Audio;
using DarkTonic.MasterAudio;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;
//using Zenject;


namespace Return.Audios
{
    public class AudioManager : SingletonSO<AudioManager>,IStart,ITick //,IInitializable,ITickable
    {
        static Transform m_Transform;

        #region AudioMixer
        const string AudioMixer = "AudioMixer";

        [BoxGroup(AudioMixer)]
        [SerializeField]
        protected AudioMixer m_MasterMixer;

        [BoxGroup(AudioMixer)]
        [SerializeField]
        protected AudioMixer m_AudioSoundMixer;

        /// <summary>
        /// The Music Mixer is not affect by any king of effect, for this reason, it is perfect for all Music Sources in game.
        /// </summary>
        [BoxGroup(AudioMixer)]
        [SerializeField]
        protected AudioMixerGroup m_BackgroundMixerGroup;

        [BoxGroup(AudioMixer)]
        [SerializeField]
        protected AudioMixerGroup m_UIMixerGroup;

        [BoxGroup(AudioMixer)]
        [SerializeField]
        protected AudioMixerGroup m_AmbientMixerGroup;

        [BoxGroup(AudioMixer)]
        [SerializeField]
        protected AudioMixerGroup m_EntityMixerGroup;

        /// <summary>
        /// The SFx Mixer is used to apply effects such echoing and deafness to SFx sources.
        /// </summary>
        [BoxGroup(AudioMixer)]
        [SerializeField]
        [Required]
        [Tooltip("The SFx Mixer is used to apply effects such echoing and deafness to SFx sources.")]
        protected AudioMixerGroup m_SFXMixer;

        /// <summary>
        /// Returns the SFx AudioMixerGroup. (Read Only)
        /// </summary>
        public AudioMixerGroup SFxMixer => m_SFXMixer;

        /// <summary>
        /// Returns the Music AudioMixerGroup. (Read Only)
        /// </summary>
        public AudioMixerGroup MusicMixer => m_BackgroundMixerGroup;
        #endregion

        [SerializeField]
        [Required]
        AudioSource m_DefaultSource;



        #region SourcePool

        [ReadOnly]
        public AudioSource EffectAudioSource;
        [ShowInInspector]
        [ReadOnly]
        static Queue<AudioSource> Sources;

        static Dictionary<AudioTemplate, AudioSourcePool> Templates;

        static List<AudioSource> Using;

        public int capacity = 40;

        public static KdTree<AudioSource> Data = new KdTree<AudioSource>();
        #endregion

        #region Routine

        protected override void Awake()
        {
            base.Awake();
            Routine.AddStartable(this);
        }


        public override void Initialize()
        {
            Debug.Log(nameof(Initialize));

            //Routine = GetBehaviourProxy<AudioManagerRoutine>();
            var slot = GetSharedGameobject();
            m_Transform = slot.transform;


            Sources = new Queue<AudioSource>(capacity);
            Using = new(150);
            Templates = new(capacity);

            for (int i = 0; i < capacity; i++)
            {
                var s = AddSource(m_DefaultSource);
                s.enabled = false;

                Sources.Enqueue(s);
                Data.Add(s);
            }
        }

        void ITick.Tick() => Tick();

        protected virtual void Tick()
        {

        }

        #endregion

        #region Template

        /// <summary>
        /// Get audio source with template.
        /// </summary>
        /// <param name="template">m_DefaultSource to create aduio source.</param>
        /// <param name="forceGet">Set as true to overload as temporary audio source.</param>
        public static AudioSource GetTemplateSource(AudioTemplate template,bool forceGet=true)
        {
            AudioSource audioSource;

            // try template cache
            if (!Templates.TryGetValue(template, out var pool))
            {
                audioSource = AddTemplate(template);
                m_TemplateCache.Add(template, audioSource);
            }
            // try pop sources and unregister cache
            else if(pool.TryGet(out audioSource))
            {
                m_TemplateCache.Remove(template, audioSource);
            }
            // overload create
            else if (forceGet)
            {
                audioSource = AddSource(template.AudioSource);
                m_TemplateCache.Add(template, audioSource);
            }

            //TemplateLog.Add((template, audioSource));

            return audioSource;
        }

        static AudioSource AddSource(AudioSource preset,bool dontDestroyOnLoad=false)
        {
            AudioSource audioSource;

            try
            {
                if (preset == null)
                    audioSource = storageSources.Pop();
                else
                    audioSource= Instantiate(preset, m_Transform);

                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(audioSource);

                return audioSource;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                // load instance audio source.
                return new GameObject("cache_AudioSource").AddComponent<AudioSource>();
            }

        }

        //...................................................//

        /// <summary>
        /// Recycle audiosource for next usage.
        /// </summary>
        public static void ReturnTemplateSource(AudioSource source)
        {
            // wait play done
            if(source.isPlaying)
            {
                _=RecycleSource(source);
                return;
            }

            source.clip = null;

            var length=TemplateLog.Count;

            for (int i = 0; i < length; i++)
                if (TemplateLog[i].Item2 == source)
                    if (Templates.TryGetValue(TemplateLog[i].Item1, out var pool))
                    {
                        pool.Return(source);
                        return;
                    }


            Debug.LogError($"Audio source isn't belong to any template : {source.name}");


            storageSources.Push(source);
        }

        protected static async UniTask RecycleSource(AudioSource source)
        {
            Assert.IsTrue(source.isPlaying);
            Assert.IsFalse(source.loop);

            float time;
            if (source.clip != null)
                time= source.clip.length - source.time + 0.05f;
            else
            {
                Debug.LogError($"{source.name} missing audio clip.");
                time = 10;
            }


            await UniTask.Delay(
                TimeSpan.FromSeconds(time),
                false,
                PlayerLoopTiming.FixedUpdate
                );

            if (source.isPlaying)
                Debug.LogError($"{source.clip.name} is still playing, will been break and recycle.");

            source.Stop();
            ReturnTemplateSource(source);
        } 

        static Stack<AudioSource> storageSources = new(100);

        /// <summary>
        /// Cache template and audio source while .
        /// </summary>
        public static List<(AudioTemplate, AudioSource)> TemplateLog=new ();
        protected static DictionaryList<AudioTemplate, AudioSource> m_TemplateCache = new(50);

        /// <summary>
        /// Rent a audio source with template.
        /// </summary>
        /// <param name="template">Template to load audio source.</param>
        public static bool UseTemplate(AudioTemplate template,out AudioSource audioSource)
        {
            bool enable;

            if (Templates.TryGetValue(template, out var pool))
                enable = pool.TryGet(out audioSource);
            else
            {
                audioSource = AddTemplate(template);
                enable = true;
            }
  
            if (enable)
                TemplateLog.Add((template, audioSource));

            return enable;
        }

        /// <summary>
        /// Add template and return qulify audio source. 
        /// </summary>
        static AudioSource AddTemplate(AudioTemplate template)
        {
            var pool = new AudioSourcePool(template.MaxPoolNumber);
            InstanceSource(template, pool).ToUniTask();


            Templates.Add(template, pool);

            var source = AddSource(template.AudioSource);
            pool.Values[0] = source;
            //pool.Cookies[0] = false;

            return source;
        }

        /// <summary>
        /// Create audio sources from template and inject into pool cache.
        /// </summary>
        static IEnumerator InstanceSource(AudioTemplate template, AudioSourcePool pool)
        {
            var length = pool.Capacity;

            for (int i = 0; i < length; i++)
            {
                if (pool.Values[i].NotNull())
                    continue;

                pool.Values[i] = AddSource(template.AudioSource);
                yield return null;
            }
        }

        #endregion

        protected static AudioSource ForceGetSource() 
        {
            if (Sources.TryDequeue(out var source))
                return source;
            else
                return Instantiate(Instance.m_DefaultSource,m_Transform);
            //return new GameObject("ManagedAudioSource").AddComponent<AudioSource>();
        }

        public static AudioSource RentSource()
        {
            return ForceGetSource();
        }


        public static AudioSource PlayOnceAt(AudioClip clip, Vector3 where, bool forcePlay = false)
        {
            AudioSource source;
            if (Sources.Count == 0)
            {
                if (forcePlay)
                {
                    var s = Data.FindClosest(where);
                    s.PlayOneShot(clip);
                    return s;
                }
                return null;
            }
            else
                source = Sources.Dequeue();

            source.clip = clip;

            if (where != default)
                PlayOnceAt(source, where).ToUniTask();
            else
                PlayOnce(source).ToUniTask();

            return source;
        }

        public static IEnumerator PlayOnce(AudioSource audio)
        {
            audio.enabled = true;
            audio.PlayOneShot(audio.clip);
            yield return new WaitUntil(() => audio.isPlaying == false);

            audio.clip = null;
            audio.enabled = false;
            Sources.Enqueue(audio);
            yield break;
        }

        public static IEnumerator PlayOnceAt(AudioSource audio, Vector3 target)
        {
            audio.transform.position = target;
            audio.enabled = true;
            audio.PlayOneShot(audio.clip);

            var wait = ConstCache.WaitForSeconds(audio.clip.length);

            yield return wait;
            audio.enabled = false;
            Sources.Enqueue(audio);
            yield break;
        }

    




        /// <summary>
        /// Defines the Sound Effects volume, e.g explosions, environment and weapons.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the Sound Effects volume, e.g explosions, environment and weapons.")]
        private float m_SfxVolume = 1.0f;

        /// <summary>
        /// Defines the Voice volume, e.g character responses and CutScenes.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the Voice volume, e.g character responses and CutScenes.")]
        private float m_VoiceVolume = 1.0f;

        /// <summary>
        /// Defines the Music volume.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the Music volume.")]
        private float m_MusicVolume = 1.0f;


        /// <summary>
        /// Maximum Pool Size.
        /// </summary>
        private const int k_MaxInstancedSources = 32;

        /// <summary>
        /// Last instance index used.
        /// </summary>
        private int m_LastPlayedIndex;

        #region PROPERTIES

        /// <summary>
        /// Defines the Sound Effects volume, e.g explosions, environment and weapons.
        /// </summary>
        public float SFxVolume
        {
            get => m_SfxVolume;
            set => m_SfxVolume = Mathf.Clamp01(value);
        }

        /// <summary>
        /// Defines the Voice volume, e.g character responses and CutScenes.
        /// </summary>
        public float VoiceVolume
        {
            get => m_VoiceVolume;
            set => m_VoiceVolume = Mathf.Clamp01(value);
        }

        /// <summary>
        /// Defines the Music volume.
        /// </summary>
        public float MusicVolume
        {
            get => m_MusicVolume;
            set => m_MusicVolume = Mathf.Clamp01(value);
        }

   

        #endregion


    }    
}

