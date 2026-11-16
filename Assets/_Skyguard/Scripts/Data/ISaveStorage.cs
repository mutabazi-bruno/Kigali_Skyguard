namespace Skyguard.Data
{
    // the leaderboard only talks to this, so the json file can be swapped for a fake in tests
    public interface ISaveStorage
    {
        bool Exists();
        string Read();
        void Write(string json);
    }
}
