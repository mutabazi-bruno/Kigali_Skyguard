using UnityEngine.SceneManagement;

namespace Skyguard.Platform
{
    public static class SceneFlow
    {
        public const string IntroScene = "Intro";
        public const string GameScene = "Skyguard";

        public static void PlayIntro() => SceneManager.LoadScene(IntroScene);

        public static void StartGame() => SceneManager.LoadScene(GameScene);
    }
}
