using Skyguard.Data;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace Skyguard.Platform
{
    // applies the comfort choices from the first-launch setup and the settings menu
    public class PlayerRig : MonoBehaviour
    {
        [SerializeField] Transform cameraOffset;
        [SerializeField] SnapTurnProvider snapTurn;
        [SerializeField] BeltFollow belt;
        [SerializeField] Transform wristDisplay;
        [SerializeField] Transform leftWristMount;
        [SerializeField] Transform rightWristMount;
        [SerializeField] Transform frontShield;
        [SerializeField] Transform leftShieldMount;
        [SerializeField] Transform rightShieldMount;
        [SerializeField] float seatedLift = 0.45f;

        public void Apply(PlayerSettings settings)
        {
            // seated players get lifted so they still see over the parapet
            var offset = cameraOffset.localPosition;
            cameraOffset.localPosition = new Vector3(offset.x, settings.seated ? seatedLift : 0f, offset.z);

            snapTurn.enabled = settings.snapTurn;
            belt.SetLeftHanded(settings.leftHanded);

            // the wrist display and the hand shield go on the hand that isn't holding the gun
            wristDisplay.SetParent(settings.leftHanded ? rightWristMount : leftWristMount, false);
            frontShield.SetParent(settings.leftHanded ? rightShieldMount : leftShieldMount, false);
            frontShield.gameObject.SetActive(settings.frontShield);
        }
    }
}
