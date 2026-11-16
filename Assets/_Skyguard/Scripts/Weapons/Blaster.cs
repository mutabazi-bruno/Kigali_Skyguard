using Skyguard.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Skyguard.Weapons
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Blaster : MonoBehaviour
    {
        [SerializeField] Transform muzzle;
        [SerializeField] MagazineWell well;
        [SerializeField] MagazinePouch pouch;
        [SerializeField] ProjectileLauncher bolts;
        [SerializeField] LineRenderer laserSight;
        [SerializeField] float sightLength = 25f;
        [SerializeField] float fireCooldown = 0.12f;
        [SerializeField] float fastReloadWindow = 1.5f;
        [SerializeField] float ejectPush = 1.2f;

        XRGrabInteractable grab;
        IFireMode fireMode = new SingleShot();
        InputAction ejectLeft;
        InputAction ejectRight;
        float nextShotTime;
        float ejectedAt = -10f;

        public MagazineWell Well => well;
        public IFireMode FireMode => fireMode;
        public Magazine Loaded => well.Loaded;

        public int MagazineCapacity
        {
            get => pouch.Capacity;
            set => pouch.Capacity = value;
        }

        // held by a hand, not resting in the holster socket
        public bool IsHeld => grab.isSelected && grab.firstInteractorSelecting is not XRSocketInteractor;
        public bool HeldInLeftHand => IsHeld && grab.firstInteractorSelecting.handedness == InteractorHandedness.Left;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            grab.activated.AddListener(_ => TryFire());
            well.selectEntered.AddListener(OnMagazineIn);

            // primary button is X on the left touch controller and A on the right
            ejectLeft = new InputAction("Eject Left", binding: "<XRController>{LeftHand}/{PrimaryButton}");
            ejectRight = new InputAction("Eject Right", binding: "<XRController>{RightHand}/{PrimaryButton}");
            ejectRight.AddBinding("<Keyboard>/r");
        }

        void OnEnable()
        {
            ejectLeft.performed += OnEjectLeft;
            ejectRight.performed += OnEjectRight;
            ejectLeft.Enable();
            ejectRight.Enable();
        }

        void OnDisable()
        {
            ejectLeft.performed -= OnEjectLeft;
            ejectRight.performed -= OnEjectRight;
            ejectLeft.Disable();
            ejectRight.Disable();
        }

        void LateUpdate()
        {
            laserSight.enabled = IsHeld;
            if (!IsHeld)
                return;

            laserSight.SetPosition(0, muzzle.position);
            laserSight.SetPosition(1, muzzle.position + muzzle.forward * sightLength);
        }

        public void SetFireMode(IFireMode mode) => fireMode = mode;

        public void ResetForRun()
        {
            fireMode = new SingleShot();
            pouch.Capacity = MagazinePouch.StandardCapacity;
            LoadFresh();
        }

        public void LoadFresh()
        {
            if (well.Loaded != null)
                well.Loaded.Refill(pouch.Capacity);
            else
                well.Insert(pouch.TakeFresh(well.attachTransform));
        }

        void TryFire()
        {
            if (Time.timeScale == 0f || Time.time < nextShotTime)
                return;
            nextShotTime = Time.time + fireCooldown;

            var magazine = well.Loaded;
            if (magazine == null || !magazine.TakeRound())
            {
                EventBus.Publish(new DryFired { Position = muzzle.position, LeftHand = HeldInLeftHand });
                return;
            }

            fireMode.Fire(muzzle, bolts);
            EventBus.Publish(new ShotFired { Position = muzzle.position, Direction = muzzle.forward, LeftHand = HeldInLeftHand });
            EventBus.Publish(new AmmoChanged { Rounds = magazine.Rounds, Capacity = magazine.Capacity });
        }

        void OnEjectLeft(InputAction.CallbackContext _) => EjectMagazine(true);
        void OnEjectRight(InputAction.CallbackContext _) => EjectMagazine(false);

        void EjectMagazine(bool leftHand)
        {
            // only the hand holding the gun can drop its magazine
            if (!IsHeld || HeldInLeftHand != leftHand || Time.timeScale == 0f)
                return;

            var magazine = well.Eject();
            if (magazine == null)
                return;

            magazine.Drop(-well.attachTransform.up * ejectPush);
            ejectedAt = Time.time;
            EventBus.Publish(new MagazineEjected { Position = magazine.transform.position, LeftHand = leftHand });
            EventBus.Publish(new AmmoChanged { Rounds = 0, Capacity = pouch.Capacity });
        }

        void OnMagazineIn(SelectEnterEventArgs args)
        {
            var magazine = well.Loaded;
            if (magazine == null)
                return;

            EventBus.Publish(new MagazineLoaded
            {
                Position = well.attachTransform.position,
                LeftHand = HeldInLeftHand,
                FastReload = Time.time - ejectedAt < fastReloadWindow
            });
            EventBus.Publish(new AmmoChanged { Rounds = magazine.Rounds, Capacity = magazine.Capacity });
        }
    }
}
