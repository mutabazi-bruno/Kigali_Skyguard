using UnityEngine;

namespace Skyguard.Platform
{
    // the holster and magazine pouch ride on this. it follows the body, but only turns once
    // the head has looked away for a bit, so glancing around doesn't swing the belt
    public class BeltFollow : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] Transform holster;
        [SerializeField] Transform pouch;
        [SerializeField] float dropBelowHead = 0.62f;
        [SerializeField] float turnAfter = 40f;
        [SerializeField] float turnSpeed = 4f;

        float yaw;
        bool turning;

        void Start() => yaw = head.eulerAngles.y;

        public void SetLeftHanded(bool leftHanded)
        {
            // holster on the gun hand's hip, pouch on the other
            Place(holster, leftHanded ? -1f : 1f);
            Place(pouch, leftHanded ? 1f : -1f);
        }

        static void Place(Transform item, float side)
        {
            var p = item.localPosition;
            item.localPosition = new Vector3(Mathf.Abs(p.x) * side, p.y, p.z);
        }

        void LateUpdate()
        {
            float headYaw = head.eulerAngles.y;
            float gap = Mathf.Abs(Mathf.DeltaAngle(yaw, headYaw));
            if (gap > turnAfter)
                turning = true;
            if (turning)
            {
                yaw = Mathf.LerpAngle(yaw, headYaw, Time.unscaledDeltaTime * turnSpeed);
                turning = gap > 2f;
            }

            var position = head.position + Vector3.down * dropBelowHead;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
        }
    }
}
