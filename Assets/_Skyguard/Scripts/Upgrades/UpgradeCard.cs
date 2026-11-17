using System;
using Skyguard.Core;
using TMPro;
using UnityEngine;

namespace Skyguard.Upgrades
{
    // picking an upgrade uses the same skill as the game: you shoot the card you want
    public class UpgradeCard : MonoBehaviour, IDamageable
    {
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text description;
        [SerializeField] float bobHeight = 0.05f;

        IUpgrade upgrade;
        Action<IUpgrade> picked;
        Vector3 restPosition;
        float phase;

        public void Show(IUpgrade offer, Vector3 position, Quaternion rotation, Action<IUpgrade> onPicked)
        {
            upgrade = offer;
            picked = onPicked;
            title.text = offer.Title;
            description.text = offer.Description;
            restPosition = position;
            phase = UnityEngine.Random.value * 6f;
            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        public void TakeDamage(DamageInfo hit)
        {
            if (hit.Source == DamageSource.Player && upgrade != null)
                picked?.Invoke(upgrade);
        }

        void Update()
        {
            transform.position = restPosition + Vector3.up * (Mathf.Sin(Time.time * 1.6f + phase) * bobHeight);
        }
    }
}
