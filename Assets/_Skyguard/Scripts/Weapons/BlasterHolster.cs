using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Skyguard.Weapons
{
    // let go of the blaster anywhere and it finds its way back to the hip after a moment
    public class BlasterHolster : MonoBehaviour
    {
        [SerializeField] XRSocketInteractor socket;
        [SerializeField] XRGrabInteractable blaster;
        [SerializeField] float returnDelay = 1.5f;

        float returnAt = -1f;

        void Awake()
        {
            blaster.selectExited.AddListener(_ => returnAt = Time.time + returnDelay);
            blaster.selectEntered.AddListener(_ => returnAt = -1f);
        }

        void Update()
        {
            if (returnAt > 0f && Time.time >= returnAt && !blaster.isSelected)
                ReturnNow();
        }

        public void ReturnNow()
        {
            returnAt = -1f;
            if (blaster.isSelected)
                blaster.interactionManager.CancelInteractableSelection((IXRSelectInteractable)blaster);
            socket.interactionManager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)blaster);
        }
    }
}
