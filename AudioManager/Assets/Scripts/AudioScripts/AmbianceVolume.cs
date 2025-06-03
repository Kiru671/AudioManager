using System;
using UnityEngine;

namespace AudioScripts
{
    public class AmbianceVolume : MonoBehaviour
    {
        private GameObject player;
        private Collider colliderVolume;
        private AudioManager instance;
        private AudioSource source;
        
        [SerializeField] SoundData sdata;
        
        private void Start()
        {
            WaitForPlayerAndPlay();
            colliderVolume = GetComponent<Collider>();
            instance = AudioManager.Instance;
        }

        private void Update()
        {
            //ReEnableOnPlayerClose();
        }

        private async void WaitForPlayerAndPlay()
        {
            // Wait until a GameObject with the tag "Player" exists
            while (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                await System.Threading.Tasks.Task.Yield(); // Wait for the next frame
            }

            source = instance.PlaySFX(sdata, transform.position, true);
            StartCoroutine(instance.SourceFollow(source,
                () =>
                {
                    var closestPoint = colliderVolume.ClosestPoint(player.transform.position);
                    return new Vector3(closestPoint.x, closestPoint.y, closestPoint.z);
                }));
        }

        private void ReEnableOnPlayerClose()
        {
            if (source.maxDistance < Vector3.Distance(player.transform.position, source.transform.position))
            {
                sdata.sourceParams.ApplyTo(source);
                source.enabled = true;
                WaitForPlayerAndPlay();
            }
        }
    }
}
