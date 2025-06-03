using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

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
            activeSources = new List<AudioSource>();
        }

        public AudioSource PlaySFX(SoundData sdata, Vector3 position, bool loop = false)
        {
            var source = audioSourcePool.GetAvailableSource();
            sdata.sourceParams?.ApplyTo(source);
            source.transform.position = position;
            source.clip = sdata.clip;
            source.loop = loop;
            source.Play();
            if (!activeSources.Contains(source))
                activeSources.Add(source);
            StartCoroutine(ReleaseSourceAfterPlay(source));
            return source;
        }

        public void SetMusicLayers(MusicTrack track, SourceParams[] layerParams)
        {
            beatManager.BPM = track.bpm;
            beatManager.audioSource = musicLayerController.MusicLayers[(int)MusicLayerController.LayerType.Harmony];

            Debug.Log("AudioManager: Set music layers with BPM: " + track.bpm);

            var clips = new AudioClip[]
                { track.wholeTrack, track.percussion, track.bass, track.melody, track.harmony, track.other };
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
            MusicPlaylist playlist = GameObject.FindObjectOfType<MusicPlaylist>();
            if (playlist == null)
                return;
            SetMusicLayers(playlist.tracks[Array.IndexOf(playlist.tracks, playlist.currentTrack) + 1],
                playlist.MusicParamaters);
        }

        public void PlayRandom(SoundData[] sdata, Vector3 position)
        {
            if (sdata.Length == 0) return;
            var randomIndex = Random.Range(0, sdata.Length);
            PlaySFX(sdata[randomIndex], position);
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

        public IEnumerator SourceFollow(AudioSource source, Func<Vector3> getTargetPosition)
        {
            while (source.isPlaying)
            {
                source.transform.position = getTargetPosition();
                yield return null;
            }
        }

        private IEnumerator MonitorSources()
        {
            var wait = new WaitForSeconds(1f); // Check once per second
            while (true)
            {
                for (int i = activeSources.Count - 1; i >= 0; i--)
                {
                    var src = activeSources[i];
                    
                    if ((!src.loop && !src.isPlaying) ||
                        audioSourcePool.IsAudible(src, Camera.main?.transform, 0.01f) == false)
                    {
                        {
                            activeSources.RemoveAt(i);
                            audioSourcePool.ReleaseSource(src);
                        }
                    }
                }
                yield return wait;
            }
        }
        private IEnumerator ReleaseSourceAfterPlay(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            source.clip = null;
            source.gameObject.SetActive(false);
        }
        
    }
    [Serializable]
    public struct SoundData
    {
        public AudioClip clip;
        public SourceParams sourceParams;
    }
}