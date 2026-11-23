using System;
using System.Collections.Generic;
using Skyguard.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Skyguard.Platform
{
    // the opening cutscene, shown on a big screen in the dark. plays every time the game starts (hold trigger to skip)
    // and can be replayed from the menu
    public class IntroPlayer : MonoBehaviour
    {
        [Serializable]
        public class Caption
        {
            public float time;
            public float duration = 4f;
            public string english;
            public string french;
            public string kinyarwanda;
        }

        [SerializeField] VideoClip[] shots;
        [SerializeField] VideoPlayer playerA;
        [SerializeField] VideoPlayer playerB;
        [SerializeField] RawImage screen;
        [SerializeField] TMP_Text captionText;
        [SerializeField] CanvasGroup captionGroup;
        [SerializeField] Image skipRing;
        [SerializeField] AudioSource music;
        [SerializeField] List<Caption> captions = new();
        [SerializeField] float holdToSkip = 1f;

        [Header("Placement")]
        [SerializeField] Transform head;
        [SerializeField] Transform cinema;
        [SerializeField] float screenDistance = 7f;

        LeaderboardRepository save;
        SubtitleLanguage language;
        InputAction skip;
        VideoPlayer playing;
        VideoPlayer waiting;
        int shotIndex;
        float finishedShotsTime;
        int captionIndex = -1;
        float held;
        float placeAt;
        bool done;

        void Awake()
        {
            save = new LeaderboardRepository(new JsonSaveStorage());
            save.Load();
            language = save.Settings.language;

            skip = new InputAction("Skip", binding: "<XRController>/{TriggerButton}");
            skip.AddBinding("<XRController>/{PrimaryButton}");
            skip.AddBinding("<Keyboard>/space");
            skip.Enable();
        }

        void Start()
        {
            captionGroup.alpha = 0f;
            // the headset pose isn't tracked yet on the first frames, so turn the screen towards the player a moment later
            placeAt = Time.unscaledTime + 0.25f;
            playing = playerA;
            waiting = playerB;
            foreach (var p in new[] { playerA, playerB })
                p.loopPointReached += OnShotFinished;

            Load(playing, 0);
            playing.Play();
            PrepareNext();
            // the edited cut carries its own music and voice, the loop is only for clips without sound
            if (shots[0].audioTrackCount == 0)
                music.Play();
        }

        void OnDestroy() => skip?.Dispose();

        void Update()
        {
            if (done)
                return;

            if (placeAt > 0f && Time.unscaledTime >= placeAt)
            {
                placeAt = 0f;
                FaceThePlayer();
            }

            screen.texture = playing.texture;
            UpdateCaption(finishedShotsTime + (float)playing.time);

            held = skip.IsPressed() ? held + Time.deltaTime : 0f;
            skipRing.fillAmount = held / holdToSkip;
            if (held >= holdToSkip)
                Finish();
        }

        // two players take turns: one plays while the other has the next shot loaded, so there's no black gap
        void PrepareNext()
        {
            if (shotIndex + 1 < shots.Length)
                Load(waiting, shotIndex + 1);
        }

        void Load(VideoPlayer player, int index)
        {
            player.clip = shots[index];
            player.Prepare();
        }

        void OnShotFinished(VideoPlayer finished)
        {
            if (finished != playing)
                return;

            finishedShotsTime += (float)shots[shotIndex].length;
            shotIndex++;
            if (shotIndex >= shots.Length)
            {
                Finish();
                return;
            }

            (playing, waiting) = (waiting, playing);
            playing.Play();
            waiting.Stop();
            PrepareNext();
        }

        void UpdateCaption(float time)
        {
            int next = captionIndex + 1;
            if (next < captions.Count && time >= captions[next].time)
            {
                captionIndex = next;
                var c = captions[next];
                captionText.text = language switch
                {
                    SubtitleLanguage.French => c.french,
                    SubtitleLanguage.Kinyarwanda => c.kinyarwanda,
                    _ => c.english
                };
            }

            // each caption fades in with ishema's line and out when she stops, or when the next line starts
            if (captionIndex < 0)
                return;
            var shown = captions[captionIndex];
            float since = time - shown.time;
            float until = shown.time + shown.duration - time;
            if (captionIndex + 1 < captions.Count)
                until = Mathf.Min(until, captions[captionIndex + 1].time - time);
            captionGroup.alpha = Mathf.Clamp01(Mathf.Min(since * 2f, until * 2f));
        }

        // whichever way the player is facing in the room when the scene loads, the screen goes there.
        // coming from the menu they are usually not facing the scene's forward
        void FaceThePlayer()
        {
            var forward = head.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f)
                return;
            forward.Normalize();

            var spot = head.position + forward * screenDistance;
            spot.y = cinema.position.y;
            cinema.SetPositionAndRotation(spot, Quaternion.LookRotation(forward));
        }

        void Finish()
        {
            done = true;
            save.Data.introSeen = true;
            save.Save();
            SceneFlow.StartGame();
        }
    }
}
