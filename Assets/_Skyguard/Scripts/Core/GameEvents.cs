using Skyguard.Data;
using Skyguard.Drones;
using UnityEngine;

namespace Skyguard.Core
{
    // weapon
    public struct ShotFired { public Vector3 Position; public Vector3 Direction; public bool LeftHand; }
    public struct DryFired { public Vector3 Position; public bool LeftHand; }
    public struct ShotResolved { public bool HitEnemy; }
    public struct AmmoChanged { public int Rounds; public int Capacity; }
    public struct MagazineEjected { public Vector3 Position; public bool LeftHand; }
    public struct MagazineLoaded { public Vector3 Position; public bool LeftHand; public bool FastReload; }

    // drones
    public struct DroneHit { public Vector3 Position; public DroneType Type; public bool Blocked; }
    public struct DroneDestroyed { public Vector3 Position; public DroneType Type; public int Points; public bool ByPlayer; }
    public struct ShieldBroken { public Vector3 Position; }
    public struct LaserCharging { public Vector3 Position; public float Duration; }
    public struct LaserFired { public Vector3 Position; }
    public struct CarrierReleased { public Vector3 Position; }

    // tower and player
    public struct TowerDamaged { public Vector3 Position; public int Health; public int MaxHealth; }
    public struct TowerRepaired { public int Health; public int MaxHealth; }
    public struct PlayerHit { public Vector3 Position; public int ShieldLeft; }
    public struct PlayerShieldRefilled { public int Points; }
    public struct TargetHit { public Vector3 Position; }

    // run flow
    public struct ScoreChanged { public int Score; public int Multiplier; }
    public struct WaveStarted { public int Wave; public int TotalWaves; public bool Endless; }
    public struct WaveCleared { public int Wave; public bool TowerBonus; }
    public struct UpgradeChosen { public string Title; }
    public struct RooftopChanged { public int Index; }
    public struct GameStateChanged { public GameStateId State; }
    public struct RunEnded { public RunResult Result; }
    public struct SettingsChanged { public PlayerSettings Settings; }
    public struct IshemaSpoke { public string Key; }
}
