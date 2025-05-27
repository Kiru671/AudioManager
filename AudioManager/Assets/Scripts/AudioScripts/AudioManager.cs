using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AudioScripts
{
    [RequireComponent(typeof(AudioSourcePool), typeof(MusicLayerController))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSourcePool audioSourcePool;
        [SerializeField] private MusicLayerController musicLayerController;
        [SerializeField] private BeatManager beatManager;
        [SerializeField] private SourceParams defaultSFXParams;
        [SerializeField] private SourceParams[] defaultMusicParams;
        [SerializeField] private List<AudioSource> activeSources;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            activeSources  = new List<AudioSource>();
        }

        public AudioSource PlaySFX(AudioClip clip, Vector3 position, bool loop = false)
        {
            var source = audioSourcePool.GetAvailableSource();
            source.transform.position = position;
            source.clip = clip;
            source.loop = loop;
            source.Play();
            if (!activeSources.Contains(source))
                activeSources.Add(source);
            return source;
        }
        
        public AudioSource PlaySFX(AudioClip clip, Vector3 position, SourceParams sourceParams, bool loop = false)
        {
            var source = AttachParams(audioSourcePool.GetAvailableSource(), sourceParams);
            source.transform.position = position;
            source.clip = clip;
            source.loop = loop;
            source.Play();
            if (!activeSources.Contains(source))
                activeSources.Add(source);
            return source;
        }

        public void SetMusicLayers(MusicTrack track, SourceParams[] layerParams)
        {
            beatManager.BPM = track.bpm;
            beatManager.audioSource = musicLayerController.MusicLayers[(int)MusicLayerController.LayerType.Harmony];

            Debug.Log("AudioManager: Set music layers with BPM: " + track.bpm);
            
            var clips = new AudioClip[] { track.wholeTrack, track.percussion, track.bass, track.melody, track.harmony, track.other };
            for (int i = 0; i < clips.Length; i++)
            {
                if (i < musicLayerController.MusicLayers.Count)
                {
                    var source = musicLayerController.MusicLayers[i];
                    source.clip = clips[i];
                    musicLayerController.SetLayerActive(i, source.clip != null);

                    if (layerParams != null && i < layerParams.Length && layerParams[i] != null)
                    {
                        layerParams[i].ApplyTo(source);
                    }
                }
            }
        }

        public void SetNextTrack(MusicTrack track)
        {
            SetMusicLayers(track, defaultMusicParams);
        }

        public void SkipTrack()
        {
            
        }
        
        public void PlayRandom(AudioClip[] clips, Vector3 position, SourceParams sourceParams = null)
        {
            if (clips.Length == 0) return;
            var randomIndex = Random.Range(0, clips.Length);
            PlaySFX(clips[randomIndex], position, sourceParams ?? defaultSFXParams);
        }
        
        public void SetMusicLayerActive(int index, bool active)
        {
            musicLayerController.SetLayerActive(index, active);
        }

        public void StopAllSFX()
        {
            audioSourcePool.StopAll();
        }
        
        public void StopWithFade(AudioSource source, float fadeTime = 1f)
        {
            StartCoroutine(FadeOutAndStop(source, fadeTime));
        }

        private IEnumerator FadeOutAndStop(AudioSource source, float fadeTime)
        {
            float startVolume = source.volume;
            float t = 0;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
                yield return null;
            }

            source.Stop();
            source.loop = false;
            audioSourcePool.ReleaseSource(source);
        }
        
        public IEnumerator SourceFollow(AudioSource source, System.Func<Vector3> getTargetPosition)
        {
            while (source.isPlaying)
            {
                source.transform.position = getTargetPosition();
                yield return null;
            }
        }
        
        IEnumerator MonitorSources()
        {
            var wait = new WaitForSeconds(1f); // Check once per second
            while (true)
            {
                for (int i = activeSources.Count - 1; i >= 0; i--)
                {
                    var src = activeSources[i];

                    // Remove non-looping SFX that have finished playing
                    if (!src.loop && !src.isPlaying)
                    {
                        audioSourcePool.ReleaseSource(src);
                        activeSources.RemoveAt(i);
                    }
                }
                yield return wait;
            }
        }

        private AudioSource AttachParams(AudioSource src, SourceParams sourceParams)
        {
            sourceParams.ApplyTo(src);
            return src;
        }
    }
}