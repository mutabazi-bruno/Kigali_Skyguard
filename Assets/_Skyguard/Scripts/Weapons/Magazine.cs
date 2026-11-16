using System;
using Skyguard.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Skyguard.Weapons
{
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public class Magazine : MonoBehaviour, IPoolable
    {
        [SerializeField] Transform stripPivot;
        [SerializeField] Renderer stripRenderer;
        [SerializeField] Color fullColour = new(0.18f, 0.9f, 0.84f);
        [SerializeField] Color lowColour = new(1f, 0.35f, 0.21f);
        [SerializeField] int lowAmmo = 3;
        [SerializeField] float cleanupDelay = 3f;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        Rigidbody body;
        MaterialPropertyBlock block;
        float cleanupAt = -1f;

        public XRGrabInteractable Grab { get; private set; }
        public int Rounds { get; private set; }
        public int Capacity { get; private set; }
        public bool IsEmpty => Rounds == 0;
        public bool IsLow => Rounds <= lowAmmo;
        public Action ReturnToPool { private get; set; }

        void Awake()
        {
            Grab = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();
            block = new MaterialPropertyBlock();
            Grab.selectEntered.AddListener(_ => cleanupAt = -1f);
            Grab.selectExited.AddListener(OnLetGo);
        }

        public void Refill(int capacity)
        {
            Capacity = capacity;
            Rounds = capacity;
            RefreshStrip();
        }

        public bool TakeRound()
        {
            if (Rounds == 0)
                return false;
            Rounds--;
            RefreshStrip();
            return true;
        }

        public void Drop(Vector3 push)
        {
            body.AddForce(push, ForceMode.VelocityChange);
            body.AddTorque(UnityEngine.Random.insideUnitSphere * 2f, ForceMode.VelocityChange);
        }

        // if nothing picks it up again it's litter on the roof, so it goes back to the pool
        void OnLetGo(SelectExitEventArgs args) => cleanupAt = Time.time + cleanupDelay;

        void Update()
        {
            if (cleanupAt > 0f && Time.time >= cleanupAt && !Grab.isSelected)
                ReturnToPool();
        }

        void RefreshStrip()
        {
            float fill = Capacity > 0 ? (float)Rounds / Capacity : 0f;
            stripPivot.localScale = new Vector3(1f, Mathf.Max(0.04f, fill), 1f);

            var colour = IsLow ? lowColour : fullColour;
            stripRenderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, colour);
            block.SetColor(EmissionColorId, colour * (Rounds == 0 ? 0.6f : 3f));
            stripRenderer.SetPropertyBlock(block);
        }

        public void OnSpawn()
        {
            cleanupAt = -1f;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        public void OnDespawn()
        {
            if (Grab.isSelected)
                Grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)Grab);
        }
    }
}
