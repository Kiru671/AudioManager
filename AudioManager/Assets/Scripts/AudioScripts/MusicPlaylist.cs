using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioScripts
{
    public class MusicPlaylist : MonoBehaviour
    {
        [SerializeField] public MusicTrack[] tracks;
        [HideInInspector] public MusicTrack currentTrack;
        [Tooltip("Order is wholeTrack, percussion, bass, melody, other")]
        [SerializeField] private SourceParams[] musicParamaters = new SourceParams[5];
        [HideInInspector] public SourceParams[] MusicParamaters => musicParamaters;
        
        

        private void Start()
        {
            AudioManager.Instance.SetMusicLayers(tracks[0], musicParamaters);
        }

        public void NextTrack()
        {
            AudioManager.Instance.SetMusicLayers(tracks[Array.IndexOf(tracks, currentTrack) + 1], musicParamaters);
        }
    }
    
    [Serializable]
    public struct MusicTrack
    {
        public string name;
        public AudioClip wholeTrack;
        public AudioClip percussion;
        public AudioClip bass;
        public AudioClip melody;
        public AudioClip harmony;
        public AudioClip other;
        [Range(0,100f)] public float volume;
        public float bpm;
    }
}
