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


        private void Start()
        {
            WaitForPlayerAndPlay();
            colliderVolume = GetComponent<Collider>();
            instance = AudioManager.Instance;
        }

        private async void WaitForPlayerAndPlay()
        {
            // Wait until a GameObject with the tag "Player" exists
            while (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                await System.Threading.Tasks.Task.Yield(); // Wait for the next frame
            }

            StartCoroutine(instance.SourceFollow(
                instance.PlaySFXLooping(ambianceClip, transform.position, sourceParams),
                () =>
                {
                    var closestPoint = colliderVolume.ClosestPoint(player.transform.position);
                    return new Vector3(closestPoint.x, closestPoint.y, closestPoint.z);
                }));
        }
    }
}
