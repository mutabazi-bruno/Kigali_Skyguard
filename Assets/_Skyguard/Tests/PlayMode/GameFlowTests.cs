using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Skyguard.Core;
using Skyguard.Drones;
using Skyguard.Flow;
using Skyguard.Gameplay;
using Skyguard.Upgrades;
using Skyguard.Waves;
using Skyguard.Weapons;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using Object = UnityEngine.Object;

namespace Skyguard.Tests
{
    // plays the real scene without a headset: the "player" here just damages drones directly
    public class GameFlowTests
    {
        GameManager game;

        [UnitySetUp]
        public IEnumerator LoadGame()
        {
            // no headset in the test runner, so openxr logs errors on startup. expected here
            LogAssert.ignoreFailingMessages = true;
            yield return SceneManager.LoadSceneAsync("Skyguard");
            yield return null;
            game = GameManager.Instance;
            game.Leaderboard.Data.tutorialDone = true;
            game.Leaderboard.Data.comfortSetupDone = true;
        }

        [UnityTearDown]
        public IEnumerator ResetTime()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator MenuOpensWhenTheGameStarts()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            var menu = GameObject.Find("Menus");
            Assert.IsNotNull(menu, "menu canvas is hidden on the main menu");
            Assert.AreEqual(GameStateId.MainMenu, game.State);
        }

        [UnityTest]
        public IEnumerator HandsCanReachTheBlasterAndMagazines()
        {
            yield return null;
            int needed = (1 << LayerMask.NameToLayer("Blaster")) | (1 << LayerMask.NameToLayer("Magazine"));
            // no controllers in the test runner, so the rig has the hands switched off
            var hands = Object.FindObjectsByType<SphereInteractionCaster>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.IsNotEmpty(hands);
            foreach (var hand in hands)
                Assert.AreEqual(needed, hand.physicsLayerMask & needed, hand.name + " can't touch the blaster or magazines");
        }

        // the tutorial's last step waits for this event, it used to never arrive
        [UnityTest]
        public IEnumerator ReloadingAnnouncesTheNewMagazine()
        {
            yield return null;
            var well = Object.FindAnyObjectByType<MagazineWell>();
            var pouch = Object.FindAnyObjectByType<MagazinePouch>();
            well.Eject(); // the blaster is empty on the menu, so usually nothing comes out

            int rounds = -1;
            Action<MagazineLoaded> onLoaded = _ => rounds = well.Loaded.Rounds;
            EventBus.Subscribe(onLoaded);
            well.Insert(pouch.TakeFresh(well.attachTransform));
            EventBus.Unsubscribe(onLoaded);

            Assert.AreEqual(pouch.Capacity, rounds);
        }

        [UnityTest]
        public IEnumerator ShieldDiscStopsGunnerLasers()
        {
            yield return null;
            var shield = Object.FindObjectsByType<ShieldBlock>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
            Assert.IsNotNull(shield.GetComponent<BoxCollider>());
            Assert.AreEqual(LayerMask.NameToLayer("PlayerHitbox"), shield.gameObject.layer, "lasers only hit the hitbox layer");

            bool blocked = false;
            Action<LaserBlocked> onBlocked = _ => blocked = true;
            EventBus.Subscribe(onBlocked);
            shield.TakeDamage(new DamageInfo { Amount = 1, Source = DamageSource.Drone, Point = shield.transform.position });
            EventBus.Unsubscribe(onBlocked);

            Assert.IsTrue(blocked);
        }

        // drones dive at the tower from every side, so one behind it has to stay hittable
        [UnityTest]
        public IEnumerator BoltsFlyThroughTheTowerLattice()
        {
            yield return null;
            var bolt = Resources.FindObjectsOfTypeAll<PlayerBolt>().First();
            var mask = (LayerMask)typeof(Projectile).GetField("hitMask", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bolt);

            foreach (var tower in Object.FindObjectsByType<RelayTower>(FindObjectsSortMode.None))
            {
                var centre = tower.GetComponent<CapsuleCollider>().bounds.center;
                var across = tower.transform.right;
                Assert.IsFalse(Physics.Raycast(centre - across * 3f, across, 6f, mask, QueryTriggerInteraction.Ignore), tower.name);
            }
        }

        [UnityTest]
        public IEnumerator HandShieldSettingShowsAndHidesTheDisc()
        {
            yield return null;
            var holder = Object.FindObjectsByType<ShieldBlock>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single().transform.parent;

            game.Settings.frontShield = false;
            game.ApplySettings();
            Assert.IsFalse(holder.gameObject.activeSelf);

            game.Settings.frontShield = true;
            game.ApplySettings();
            Assert.IsTrue(holder.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator ClearingAWaveOffersUpgradesThenStartsTheNextWave()
        {
            game.StartCampaign();
            yield return Until(() => game.State == GameStateId.Wave, 5f);

            Time.timeScale = 4f;
            yield return Until(() =>
            {
                ShootEverything();
                return game.State != GameStateId.Wave;
            }, 30f);
            Assert.AreEqual(GameStateId.Upgrade, game.State);

            var card = Object.FindObjectsByType<UpgradeCard>(FindObjectsSortMode.None).First(c => c.gameObject.activeInHierarchy);
            card.TakeDamage(new DamageInfo { Amount = 1, Source = DamageSource.Player });

            yield return Until(() => game.State == GameStateId.Wave, 5f);
            Assert.AreEqual(2, game.Wave);
        }

        [UnityTest]
        public IEnumerator TowerFallingEndsTheRun()
        {
            game.StartCampaign();
            yield return Until(() => game.State == GameStateId.Wave, 5f);

            game.Tower.SetHealth(5);
            Time.timeScale = 4f;
            yield return Until(() => game.State == GameStateId.Results, 40f);
            Assert.AreEqual(GameStateId.Results, game.State);
        }

        [UnityTest]
        public IEnumerator PauseFreezesTheWaveAndResumeCarriesOn()
        {
            game.StartCampaign();
            yield return Until(() => game.State == GameStateId.Wave, 5f);

            game.Pause();
            Assert.AreEqual(GameStateId.Paused, game.State);
            Assert.AreEqual(0f, Time.timeScale);

            game.Resume();
            Assert.AreEqual(GameStateId.Wave, game.State);
            Assert.AreEqual(1f, Time.timeScale);
        }

        static void ShootEverything()
        {
            var waves = Object.FindAnyObjectByType<WaveManager>();
            foreach (var drone in waves.Active.Where(d => d.IsAlive).ToList())
            {
                // hit from behind so shielded drones can't block it
                drone.TakeDamage(new DamageInfo { Amount = 5, Source = DamageSource.Player, Direction = drone.transform.forward, Point = drone.transform.position });
            }
        }

        static IEnumerator Until(Func<bool> condition, float timeout)
        {
            float end = Time.unscaledTime + timeout;
            while (!condition() && Time.unscaledTime < end)
                yield return null;
        }
    }
}
