using UnityEngine;
using UnityEngine.InputSystem;

namespace Skyguard.Presentation
{
    // drives the glove's finger poses from the quest controller.
    // grip curls middle, ring and little. trigger curls the index and pulls the thumb in.
    // both pressed = fist.
    [RequireComponent(typeof(Animator))]
    public class HandAnimator : MonoBehaviour
    {
        [SerializeField] InputActionProperty gripAction;     // XRI Right/Left Interaction > Select Value
        [SerializeField] InputActionProperty triggerAction;  // XRI Right/Left Interaction > Activate Value
        [SerializeField] float smoothing = 18f;              // higher = snappier fingers

        Animator anim;
        float grip, trigger;
        static readonly int GripId = Animator.StringToHash("Grip");
        static readonly int TriggerId = Animator.StringToHash("Trigger");

        void Awake() => anim = GetComponent<Animator>();

        void Update()
        {
            float g = gripAction.action != null ? gripAction.action.ReadValue<float>() : 0f;
            float t = triggerAction.action != null ? triggerAction.action.ReadValue<float>() : 0f;

            // ease toward the button value so fingers don't jump between frames.
            // unscaled so the hands still move on the pause menu
            float k = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
            grip = Mathf.Lerp(grip, g, k);
            trigger = Mathf.Lerp(trigger, t, k);

            anim.SetFloat(GripId, grip);
            anim.SetFloat(TriggerId, trigger);
        }
    }
}
