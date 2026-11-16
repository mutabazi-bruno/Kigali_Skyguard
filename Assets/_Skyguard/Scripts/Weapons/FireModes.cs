using UnityEngine;

namespace Skyguard.Weapons
{
    // upgrades swap the blaster's fire mode instead of editing the blaster
    public interface IFireMode
    {
        void Fire(Transform muzzle, ProjectileLauncher bolts);
    }

    public class SingleShot : IFireMode
    {
        public void Fire(Transform muzzle, ProjectileLauncher bolts) => bolts.Launch(muzzle.position, muzzle.forward);
    }

    public class PiercingShot : IFireMode
    {
        readonly int everyNth;
        int shots;

        public PiercingShot(int everyNth = 5)
        {
            this.everyNth = everyNth;
        }

        public void Fire(Transform muzzle, ProjectileLauncher bolts)
        {
            var bolt = bolts.Launch(muzzle.position, muzzle.forward);
            shots++;

            // every 5th shot goes through the first drone and on into a second one
            if (shots % everyNth == 0 && bolt is PlayerBolt playerBolt)
                playerBolt.PierceCount = 1;
        }
    }
}
