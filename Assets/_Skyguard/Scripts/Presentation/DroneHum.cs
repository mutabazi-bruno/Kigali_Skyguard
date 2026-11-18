using Skyguard.Drones;
using UnityEngine;

namespace Skyguard.Presentation
{
    // each drone type has its own spatial hum, so you hear one behind you before you see it
    [RequireComponent(typeof(AudioSource))]
    public class DroneHum : MonoBehaviour
    {
        [SerializeField] Drone drone;

        AudioSource source;
        float baseVolume;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            baseVolume = source.volume;
        }

        void OnEnable()
        {
            source.volume = baseVolume;
            source.time = Random.Range(0f, source.clip.length * 0.9f);
            source.Play();
        }

        void Update()
        {
            // goes quiet as it falls, the explosion is loud enough on its own
            if (!drone.IsAlive)
                source.volume = Mathf.MoveTowards(source.volume, 0f, Time.deltaTime * 2f);
        }
    }
}
