using System;
using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Presentation
{
    // dusk on tower 1, midnight on the hill, the cold hour before dawn on the main relay, then sunrise if you win
    public class NightCycle : MonoBehaviour
    {
        [Serializable]
        public struct Phase
        {
            public Color zenith;
            public Color horizon;
            public Color fog;
            public Color ambient;
            public float stars;
            public float moonIntensity;
        }

        [SerializeField] Material sky;
        [SerializeField] Light moon;
        [SerializeField] Phase dusk;
        [SerializeField] Phase midnight;
        [SerializeField] Phase beforeDawn;
        [SerializeField] Phase dawn;
        [SerializeField] float blendSpeed = 0.25f;

        static readonly int ZenithId = Shader.PropertyToID("_ZenithColor");
        static readonly int HorizonId = Shader.PropertyToID("_HorizonColor");
        static readonly int StarsId = Shader.PropertyToID("_StarStrength");

        Phase from;
        Phase to;
        float blend = 1f;

        void Awake()
        {
            // work on a copy so play mode never rewrites the sky material on disk
            sky = new Material(sky);
            RenderSettings.skybox = sky;
        }

        void Start()
        {
            to = dusk;
            Apply(dusk);
        }

        void OnEnable()
        {
            EventBus.Subscribe<RooftopChanged>(OnRooftopChanged);
            EventBus.Subscribe<RunEnded>(OnRunEnded);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<RooftopChanged>(OnRooftopChanged);
            EventBus.Unsubscribe<RunEnded>(OnRunEnded);
        }

        void OnRooftopChanged(RooftopChanged e)
        {
            // rooftop moves happen behind a black fade, so snap straight to the new sky
            to = e.Index switch { 1 => midnight, 2 => beforeDawn, _ => dusk };
            blend = 1f;
            Apply(to);
        }

        void OnRunEnded(RunEnded e)
        {
            if (!e.Result.Won)
                return;
            from = Current();
            to = dawn;
            blend = 0f;
        }

        void Update()
        {
            if (blend >= 1f)
                return;
            blend = Mathf.Min(1f, blend + Time.deltaTime * blendSpeed);
            Apply(Lerp(from, to, Mathf.SmoothStep(0f, 1f, blend)));
        }

        Phase Current() => new()
        {
            zenith = sky.GetColor(ZenithId),
            horizon = sky.GetColor(HorizonId),
            fog = RenderSettings.fogColor,
            ambient = RenderSettings.ambientLight,
            stars = sky.GetFloat(StarsId),
            moonIntensity = moon.intensity
        };

        void Apply(Phase p)
        {
            sky.SetColor(ZenithId, p.zenith);
            sky.SetColor(HorizonId, p.horizon);
            sky.SetFloat(StarsId, p.stars);
            RenderSettings.fogColor = p.fog;
            RenderSettings.ambientLight = p.ambient;
            moon.intensity = p.moonIntensity;
        }

        static Phase Lerp(Phase a, Phase b, float t) => new()
        {
            zenith = Color.Lerp(a.zenith, b.zenith, t),
            horizon = Color.Lerp(a.horizon, b.horizon, t),
            fog = Color.Lerp(a.fog, b.fog, t),
            ambient = Color.Lerp(a.ambient, b.ambient, t),
            stars = Mathf.Lerp(a.stars, b.stars, t),
            moonIntensity = Mathf.Lerp(a.moonIntensity, b.moonIntensity, t)
        };
    }
}
