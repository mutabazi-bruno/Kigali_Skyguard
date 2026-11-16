using System;
using System.Collections.Generic;
using UnityEngine;

namespace Skyguard.Data
{
    public class LeaderboardRepository
    {
        public const int MaxEntries = 10;

        readonly ISaveStorage storage;

        public SaveData Data { get; private set; } = new();
        public PlayerSettings Settings => Data.settings;
        public IReadOnlyList<ScoreEntry> TopScores => Data.scores;

        public LeaderboardRepository(ISaveStorage storage)
        {
            this.storage = storage;
        }

        public void Load()
        {
            if (!storage.Exists())
                return;

            try
            {
                Data = JsonUtility.FromJson<SaveData>(storage.Read()) ?? new SaveData();
            }
            catch (ArgumentException)
            {
                // a corrupted save shouldn't stop the game from starting
                Debug.LogWarning("save file was unreadable, starting fresh");
                Data = new SaveData();
            }
        }

        public void Save() => storage.Write(JsonUtility.ToJson(Data, true));

        // returns the 1-based rank, or 0 if the score didn't make the top ten
        public int Submit(ScoreEntry entry)
        {
            var scores = Data.scores;
            int index = scores.FindIndex(s => entry.score > s.score);
            if (index < 0)
                index = scores.Count;

            if (index >= MaxEntries)
                return 0;

            scores.Insert(index, entry);
            if (scores.Count > MaxEntries)
                scores.RemoveAt(scores.Count - 1);

            Save();
            return index + 1;
        }

        public void Rename(ScoreEntry entry, string initials)
        {
            entry.initials = initials;
            Settings.lastInitials = initials;
            Save();
        }
    }
}
