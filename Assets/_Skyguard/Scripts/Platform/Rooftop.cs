using Skyguard.Gameplay;
using Skyguard.Waves;
using UnityEngine;

namespace Skyguard.Platform
{
    // one of the three towers. just holds the anchors the rest of the game needs
    public class Rooftop : MonoBehaviour
    {
        [SerializeField] Transform playerAnchor;
        [SerializeField] RelayTower tower;
        [SerializeField] SpawnArea spawnArea;
        [SerializeField] Transform practiceTargetAnchor;
        [SerializeField] Transform cardAnchor;

        public Transform PlayerAnchor => playerAnchor;
        public RelayTower Tower => tower;
        public SpawnArea SpawnArea => spawnArea;
        public Transform PracticeTargetAnchor => practiceTargetAnchor;
        public Transform CardAnchor => cardAnchor;
    }
}
