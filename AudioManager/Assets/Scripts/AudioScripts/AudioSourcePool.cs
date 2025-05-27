using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AudioScripts
{
    public class AudioSourcePool : MonoBehaviour
    {
        [SerializeField] private int poolSize = 10;
        [SerializeField] private int maxSize = 25;
        private List<AudioSource> pool;

        private void Awake()
        {
            pool = new List<AudioSource>();

            for (int i = 0; i < poolSize; i++)
            {
                var src = new GameObject("AudioSource_" + i);
                //src.transform.SetParent(transform); // Optional: organize hierarchy
                AudioSource newSource = src.AddComponent<AudioSource>();
                newSource.playOnAwake = false;
                pool.Add(newSource);
            }
        }

        public AudioSource GetAvailableSource()
        {
            AudioSource availableSource = pool.FirstOrDefault(s => !s.isPlaying);

            if (availableSource != null)
            {
                //Debug.Log("AudioSourcePool: Reusing unused source.");
                
                return availableSource;
            }
            if (pool.Count < maxSize)
            {
                var src = new GameObject("AudioSource_" + pool.Count);
                src.transform.SetParent(transform);
                availableSource = src.AddComponent<AudioSource>();
                pool.Add(availableSource);
            }
            else
            {
                Debug.LogWarning("AudioSourcePool: Max pool size reached. Reusing first source.");
                availableSource = pool[0];
            }
            
            return availableSource;
        }

        public void ReleaseSource(AudioSource source)
        {
            if (pool.Contains(source))
            {
                source.Stop();
                source.clip = null;
                source.loop = false;
                source.enabled = false;
            }
            else
            {
                Debug.LogWarning("AudioSourcePool: Tried to release source not in pool.");
            }
        }

        public void StopAll()
        {
            foreach (var src in pool)
            {
                src.Stop();
                ReleaseSource(src);
            }
        }

        /*public bool IsAudible(AudioSource source)
        {
            if (!source.enabled || !source.isPlaying)
                return false;

            float[] samples = new float[256];
            source.GetOutputData(samples, 0);

            float rms = 0f;
            foreach (float sample in samples)
            {
                rms += sample * sample;
            }
            rms = Mathf.Sqrt(rms / samples.Length);

            float db = 20f * Mathf.Log10(rms + 1e-6f); // avoid log(0)
            bool isAudible = db > -79f;

            Debug.LogWarning("AudioSourcePool: Source is " + (isAudible ? "audible" : "inaudible") + $" (dB: {db})");
            return isAudible;
        }*/
    }
}
