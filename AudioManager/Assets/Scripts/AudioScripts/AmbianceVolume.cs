using System;
using UnityEngine;

namespace AudioScripts
{
    public class AmbianceVolume : MonoBehaviour
    {
        private GameObject player;
        private Collider colliderVolume;
        private AudioManager instance;
        [SerializeField] private AudioClip ambianceClip;
        [SerializeField] private float volume = 1f;
        [SerializeField] private float maxDistance = 50f;
        [SerializeField] private SourceParams sourceParams;
        private AudioSource source;

        private void Start()
        {
            WaitForPlayerAndPlay();
            colliderVolume = GetComponent<Collider>();
            instance = AudioManager.Instance;
        }

        private void Update()
        {
            if (source == null)
                return;
            if (source.maxDistance < Vector3.Distance(player.transform.position, source.transform.position))
            {
                sourceParams.ApplyTo(source);
                source.enabled = true;
            }
        }

        private async void WaitForPlayerAndPlay()
        {
            // Wait until a GameObject with the tag "Player" exists
            while (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                await System.Threading.Tasks.Task.Yield(); // Wait for the next frame
            }

            source = instance.PlaySFX(ambianceClip, transform.position, sourceParams, true);
            StartCoroutine(instance.SourceFollow(source,
                () =>
                {
                    var closestPoint = colliderVolume.ClosestPoint(player.transform.position);
                    return new Vector3(closestPoint.x, closestPoint.y, closestPoint.z);
                }));
        }
    }
}
