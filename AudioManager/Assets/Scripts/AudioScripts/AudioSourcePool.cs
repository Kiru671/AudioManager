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
                var src = Instantiate(new GameObject());
                AudioSource newSource = src.gameObject.AddComponent<AudioSource>();
                pool.Add(newSource);
            }
        }

        public AudioSource GetAvailableSource()
        {
            var availableSource = pool.First(s => !s.isPlaying);
            if (availableSource == null)
            {
                availableSource = pool.First(s => !IsAudible(s));
                if (availableSource != null)
                {
                    Debug.Log("AudioSourcePool: Reusing an inaudible source.");
                    return availableSource;
                }
                Debug.Log("AudioSourcePool: No available sources, creating new source.");
                if(pool.Count < maxSize)
                {
                    var newSource = gameObject.AddComponent<AudioSource>();
                    pool.Add(newSource);
                    availableSource = newSource;
                }
                else
                {
                    Debug.LogWarning("AudioSourcePool: Maximum pool size reached. Reusing first.");
                    availableSource = pool.First();
                }
            }
            return availableSource;
        }

        public void StopAll()
        {
            foreach (var src in pool)
            {
                src.Stop();
            }
        }
        
        private bool IsAudible(AudioSource source)
        {
            float[] samples = new float[256];
            source.GetOutputData(samples, 0);

            float rms = 0f;
            foreach (float sample in samples) {
                rms += sample * sample;
            }
            rms = Mathf.Sqrt(rms / samples.Length);

            // Consider it audible if RMS is above a small threshold
            bool isAudible = rms > 0.01f;
            return isAudible;
        }
    }
}