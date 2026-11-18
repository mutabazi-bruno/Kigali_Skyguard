using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Presentation
{
    // the shield gauntlet on the off-hand wrist. its hex disc glows faintly all the time, flares white
    // when it blocks a laser, and turns orange once a few lasers have got past it
    public class ShieldGauntlet : MonoBehaviour
    {
        [SerializeField] Transform energyDisc;
        [SerializeField] Renderer discRenderer;
        [SerializeField] float flashTime = 0.6f;
        [SerializeField] float idleAlpha = 0.18f;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly Color Healthy = new(0.1f, 0.8f, 1f, 0.6f);
        static readonly Color Cracked = new(1f, 0.35f, 0.2f, 0.7f);
        static readonly Color Blocked = new(0.75f, 1f, 1f, 0.95f);

        MaterialPropertyBlock block;
        Vector3 restScale;
        float flash;
        Color resting = Healthy;
        Color flashColour = Healthy;

        void Awake()
        {
            block = new MaterialPropertyBlock();
            restScale = energyDisc.localScale;
        }

        void OnEnable()
        {
            EventBus.Subscribe<PlayerHit>(OnHit);
            EventBus.Subscribe<PlayerShieldRefilled>(OnRefilled);
            EventBus.Subscribe<LaserBlocked>(OnBlocked);
            EventBus.Subscribe<SettingsChanged>(OnSettingsChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<PlayerHit>(OnHit);
            EventBus.Unsubscribe<PlayerShieldRefilled>(OnRefilled);
            EventBus.Unsubscribe<LaserBlocked>(OnBlocked);
            EventBus.Unsubscribe<SettingsChanged>(OnSettingsChanged);
        }

        void OnHit(PlayerHit e)
        {
            resting = e.ShieldLeft <= 1 ? Cracked : Healthy;
            Flash(resting);
        }

        void OnRefilled(PlayerShieldRefilled e)
        {
            resting = Healthy;
            Flash(Healthy);
        }

        void OnBlocked(LaserBlocked e) => Flash(Blocked);

        // a quick pulse when the shield is switched back on, so you notice it come up
        void OnSettingsChanged(SettingsChanged e)
        {
            if (e.Settings.frontShield)
                Flash(resting);
        }

        void Flash(Color colour)
        {
            flashColour = colour;
            flash = flashTime;
        }

        void Update()
        {
            flash = Mathf.Max(0f, flash - Time.deltaTime);
            float t = flash / flashTime;
            energyDisc.localScale = restScale * (1f + 0.15f * t);

            var c = Color.Lerp(resting, flashColour, t);
            c.a = Mathf.Lerp(idleAlpha, flashColour.a, t);
            discRenderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, c);
            discRenderer.SetPropertyBlock(block);
        }
    }
}
