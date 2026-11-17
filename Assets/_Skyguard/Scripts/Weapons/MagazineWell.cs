using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Skyguard.Weapons
{
    // the socket in the blaster's grip. only takes magazines, and takes them straight out of the hand
    public class MagazineWell : XRSocketInteractor
    {
        [SerializeField] float snapRadius = 0.07f;
        [SerializeField] float pauseAfterEject = 0.5f;

        float closedUntil;

        public Magazine Loaded { get; private set; }

        public float SnapRadius
        {
            get => snapRadius;
            set => snapRadius = value;
        }

        public override bool CanHover(IXRHoverInteractable interactable) =>
            IsMagazine(interactable) && base.CanHover(interactable);

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            if (!IsMagazine(interactable) || Time.time < closedUntil)
                return false;
            if (IsSelecting(interactable))
                return true;

            // a stock socket waits for the hand to let go. this one grabs the magazine as soon as it's pushed
            // far enough into the grip, which is what makes the reload feel like a reload
            float distance = Vector3.Distance(interactable.transform.position, attachTransform.position);
            return !hasSelection && distance <= snapRadius;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            // set before base, which fires selectEntered and the blaster reads Loaded in there
            Loaded = args.interactableObject.transform.GetComponent<Magazine>();
            base.OnSelectEntered(args);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            Loaded = null;
        }

        public Magazine Eject()
        {
            var magazine = Loaded;
            if (magazine == null)
                return null;

            interactionManager.SelectExit((IXRSelectInteractor)this, (IXRSelectInteractable)magazine.Grab);
            closedUntil = Time.time + pauseAfterEject;
            return magazine;
        }

        public void Insert(Magazine magazine) =>
            interactionManager.SelectEnter((IXRSelectInteractor)this, (IXRSelectInteractable)magazine.Grab);

        static bool IsMagazine(IXRInteractable interactable) => interactable.transform.TryGetComponent(out Magazine _);
    }
}
