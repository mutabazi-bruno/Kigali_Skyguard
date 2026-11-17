using Skyguard.Gameplay;
using Skyguard.Waves;
using Skyguard.Weapons;

namespace Skyguard.Upgrades
{
    public class UpgradeContext
    {
        public Blaster Blaster;
        public RelayTower Tower;
        public ScoreKeeper Score;
        public WaveManager Waves;
    }

    public interface IUpgrade
    {
        string Title { get; }
        string Description { get; }
        bool CanOffer(UpgradeContext context);
        void Apply(UpgradeContext context);
    }

    public class ExtendedMagazine : IUpgrade
    {
        public string Title => "Extended Magazine";
        public string Description => "18 shots per magazine";
        public bool CanOffer(UpgradeContext context) => context.Blaster.MagazineCapacity < 18;

        public void Apply(UpgradeContext context)
        {
            context.Blaster.MagazineCapacity = 18;
            context.Blaster.LoadFresh();
        }
    }

    public class QuickLoad : IUpgrade
    {
        public string Title => "Quick Load";
        public string Description => "Magazines snap in from further away";
        public bool CanOffer(UpgradeContext context) => context.Blaster.Well.SnapRadius < 0.12f;
        public void Apply(UpgradeContext context) => context.Blaster.Well.SnapRadius = 0.14f;
    }

    public class PiercingUpgrade : IUpgrade
    {
        public string Title => "Piercing Shot";
        public string Description => "Every 5th shot goes through two drones";
        public bool CanOffer(UpgradeContext context) => context.Blaster.FireMode is not PiercingShot;
        public void Apply(UpgradeContext context) => context.Blaster.SetFireMode(new PiercingShot());
    }

    public class TowerRepair : IUpgrade
    {
        public string Title => "Tower Repair";
        public string Description => "+30 tower health";
        public bool CanOffer(UpgradeContext context) => context.Tower.Health < context.Tower.MaxHealth;
        public void Apply(UpgradeContext context) => context.Tower.Repair(30);
    }

    public class SlowLasers : IUpgrade
    {
        public string Title => "Slow Lasers";
        public string Description => "Enemy lasers 20% slower";
        public bool CanOffer(UpgradeContext context) => context.Waves.LaserSpeedMultiplier > 0.65f;
        public void Apply(UpgradeContext context) => context.Waves.LaserSpeedMultiplier *= 0.8f;
    }

    public class Overdrive : IUpgrade
    {
        public string Title => "Overdrive";
        public string Description => "Double points next wave";
        public bool CanOffer(UpgradeContext context) => !context.Score.OverdriveArmed;
        public void Apply(UpgradeContext context) => context.Score.ArmOverdrive();
    }
}
