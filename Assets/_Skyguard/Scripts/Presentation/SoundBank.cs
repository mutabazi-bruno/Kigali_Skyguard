using UnityEngine;

namespace Skyguard.Presentation
{
    [CreateAssetMenu(menuName = "Skyguard/Sound Bank")]
    public class SoundBank : ScriptableObject
    {
        [Header("Blaster")]
        public AudioClip shot;
        public AudioClip dryFire;
        public AudioClip magazineOut;
        public AudioClip magazineIn;
        public AudioClip fastReload;

        [Header("Drones")]
        public AudioClip laserCharge;
        public AudioClip laserFire;
        public AudioClip explosion;
        public AudioClip bigExplosion;
        public AudioClip hitSpark;
        public AudioClip shieldBlock;
        public AudioClip shieldBreak;
        public AudioClip carrierRelease;

        [Header("Tower and player")]
        public AudioClip towerHit;
        public AudioClip towerAlarm;
        public AudioClip playerHit;

        [Header("Flow")]
        public AudioClip waveStart;
        public AudioClip waveCleared;
        public AudioClip upgradePicked;
        public AudioClip gameOver;
        public AudioClip dawn;
        public AudioClip uiClick;
        public AudioClip ishemaChime;

        [Header("Music")]
        public AudioClip musicMenu;
        public AudioClip musicBattle;
        public AudioClip musicFinal;
        public AudioClip musicDawn;
        public AudioClip musicGameOver;
    }
}
