using System;
using UnityEngine;
using UnityEngine.Events;

namespace AudioScripts
{
    public class SoundEmitter : MonoBehaviour
    {
        [SerializeField] private AudioClip clip;
        [SerializeField] private SourceParams sourceParams;
        private Vector3 pos;
            
        public void PlaySound()
        {
            pos = transform.position;
            AudioManager.Instance.PlaySFX(clip, pos, sourceParams);
        }
    }
}
