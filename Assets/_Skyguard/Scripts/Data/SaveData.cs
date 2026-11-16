using System;
using System.Collections.Generic;

namespace Skyguard.Data
{
    public enum Difficulty { Easy, Normal }
    public enum SubtitleLanguage { English, French, Kinyarwanda }
    public enum GameMode { Campaign, Endless }

    [Serializable]
    public class PlayerSettings
    {
        public bool leftHanded;
        public bool seated;
        public bool snapTurn;
        public Difficulty difficulty = Difficulty.Normal;
        public SubtitleLanguage language = SubtitleLanguage.English;
        public float volume = 0.8f;
        public bool showFps;
        public string lastInitials = "SKY";
    }

    [Serializable]
    public class ScoreEntry
    {
        public string initials;
        public int score;
        public int waves;
        public float accuracy;
        public GameMode mode;
        public Difficulty difficulty;
        public bool won;
        public string date;
    }

    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public bool introSeen;
        public bool comfortSetupDone;
        public bool tutorialDone;
        public bool campaignCleared;
        public PlayerSettings settings = new();
        public List<ScoreEntry> scores = new();
    }

    // what the results screen needs once a run is over
    public struct RunResult
    {
        public bool Won;
        public GameMode Mode;
        public int Score;
        public int Waves;
        public float Accuracy;
        public int Rank;
        public float AverageFps;
        public ScoreEntry Entry;
    }
}
