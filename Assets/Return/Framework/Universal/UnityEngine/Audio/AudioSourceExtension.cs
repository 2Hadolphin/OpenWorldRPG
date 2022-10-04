using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Return
{
    public static class AudioSourceExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="restTime"></param>
        /// <returns></returns>
        public static IEnumerator Delegate(this AudioSource source, float restTime)
        {
            var start = source.volume;

            for (float i = 0; i < restTime; i += Time.deltaTime)
            {
                source.volume = Mathf.Lerp(start, 0, i / restTime);
                yield return null;
            }

            source.enabled = false;
            source.volume = start;
            yield break;
        }


        public static void PlayOneShot(this AudioSource source)
        {
            source.PlayOneShot(source.clip, source.volume);
        }

        public static void PlayOneShot(this AudioSource source,AudioClip clip)
        {
            source.loop = false;
            source.clip = clip;
            source.Play();
        }

        public static AudioSource At(this AudioSource source, Vector3 position, Quaternion rotation = default)
        {
            if (rotation == default)
                rotation = Quaternion.identity;

            source.transform.SetPositionAndRotation(position, rotation);

            return source;
        }


    }
}