using NUnit.Framework;
using Skyguard.Data;

namespace Skyguard.Tests
{
    public class LeaderboardRepositoryTests
    {
        // keeps the json in memory, so the tests never touch the real save file
        class MemoryStorage : ISaveStorage
        {
            public string Json;
            public int Writes;

            public bool Exists() => Json != null;
            public string Read() => Json;

            public void Write(string json)
            {
                Json = json;
                Writes++;
            }
        }

        static ScoreEntry Entry(int score) => new() { initials = "TST", score = score };

        [Test]
        public void HigherScoresRankFirst()
        {
            var repo = new LeaderboardRepository(new MemoryStorage());
            repo.Submit(Entry(500));
            int rank = repo.Submit(Entry(900));

            Assert.AreEqual(1, rank);
            Assert.AreEqual(900, repo.TopScores[0].score);
            Assert.AreEqual(500, repo.TopScores[1].score);
        }

        [Test]
        public void OnlyKeepsTheTopTen()
        {
            var repo = new LeaderboardRepository(new MemoryStorage());
            for (int i = 1; i <= 12; i++)
                repo.Submit(Entry(i * 100));

            Assert.AreEqual(LeaderboardRepository.MaxEntries, repo.TopScores.Count);
            Assert.AreEqual(1200, repo.TopScores[0].score);
            Assert.AreEqual(0, repo.Submit(Entry(50)), "a score below the whole board shouldn't get a rank");
        }

        [Test]
        public void SurvivesASaveAndReload()
        {
            var storage = new MemoryStorage();
            var first = new LeaderboardRepository(storage);
            first.Settings.language = SubtitleLanguage.Kinyarwanda;
            first.Submit(Entry(1234));

            var second = new LeaderboardRepository(storage);
            second.Load();

            Assert.AreEqual(1234, second.TopScores[0].score);
            Assert.AreEqual(SubtitleLanguage.Kinyarwanda, second.Settings.language);
        }

        [Test]
        public void BrokenSaveFileStartsFresh()
        {
            var repo = new LeaderboardRepository(new MemoryStorage { Json = "{ not json" });
            repo.Load();
            Assert.AreEqual(0, repo.TopScores.Count);
        }
    }
}
