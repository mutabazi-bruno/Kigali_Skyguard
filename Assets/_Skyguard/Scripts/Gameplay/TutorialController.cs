using System;
using Skyguard.Core;
using Skyguard.Weapons;
using UnityEngine;

namespace Skyguard.Gameplay
{
    // about a minute on tower 1: grab, shoot the target, drop the magazine, load a new one. no way to fail it
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] PracticeTarget target;
        [SerializeField] Blaster blaster;
        [SerializeField] int targetHits = 3;

        enum Step { Off, Grab, Shoot, Eject, Load }

        Step step = Step.Off;
        int hits;
        Action finished;

        public bool IsRunning => step != Step.Off;

        public void Begin(Transform targetAnchor, Action onFinished)
        {
            finished = onFinished;
            hits = 0;
            target.Show(targetAnchor);
            Subscribe();
            Say("tut_welcome");
            GoTo(Step.Grab, "tut_grab");
        }

        public void Stop()
        {
            if (!IsRunning)
                return;
            step = Step.Off;
            target.Hide();
            Unsubscribe();
        }

        void Update()
        {
            if (step == Step.Grab && blaster.IsHeld)
                GoTo(Step.Shoot, "tut_shoot");
        }

        void OnTargetHit(TargetHit _)
        {
            if (step == Step.Shoot && ++hits >= targetHits)
                GoTo(Step.Eject, "tut_eject");
        }

        void OnEjected(MagazineEjected _)
        {
            if (step == Step.Eject)
                GoTo(Step.Load, "tut_load");
        }

        void OnLoaded(MagazineLoaded _)
        {
            if (step != Step.Load)
                return;
            Say("tut_done");
            Stop();
            finished?.Invoke();
        }

        void GoTo(Step next, string line)
        {
            step = next;
            Say(line);
        }

        static void Say(string key) => EventBus.Publish(new IshemaSpoke { Key = key });

        void Subscribe()
        {
            EventBus.Subscribe<TargetHit>(OnTargetHit);
            EventBus.Subscribe<MagazineEjected>(OnEjected);
            EventBus.Subscribe<MagazineLoaded>(OnLoaded);
        }

        void Unsubscribe()
        {
            EventBus.Unsubscribe<TargetHit>(OnTargetHit);
            EventBus.Unsubscribe<MagazineEjected>(OnEjected);
            EventBus.Unsubscribe<MagazineLoaded>(OnLoaded);
        }

        void OnDisable() => Unsubscribe();
    }
}
