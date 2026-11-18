using UnityEngine.SceneManagement;

namespace Skyguard.Platform
{
    public static class SceneFlow
    {
        public const string IntroScene = "Intro";
        public const string GameScene = "Skyguard";

        // set from the main menu's "watch intro" button so the intro plays even after the first launch
        public static bool ReplayIntro { get; private set; }

        public static void PlayIntro()
        {
            ReplayIntro = true;
            SceneManager.LoadScene(IntroScene);
        }

        public static void StartGame()
        {
            ReplayIntro = false;
            SceneManager.LoadScene(GameScene);
        }
    }
}
