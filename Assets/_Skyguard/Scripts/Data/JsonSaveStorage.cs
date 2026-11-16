using System.IO;
using UnityEngine;

namespace Skyguard.Data
{
    public class JsonSaveStorage : ISaveStorage
    {
        readonly string path;
        readonly string tempPath;

        public JsonSaveStorage(string fileName = "skyguard_save.json")
        {
            path = Path.Combine(Application.persistentDataPath, fileName);
            tempPath = path + ".tmp";
        }

        public bool Exists() => File.Exists(path);

        public string Read() => File.ReadAllText(path);

        public void Write(string json)
        {
            // write next to it first so a crash mid-save can't leave a half written file behind
            File.WriteAllText(tempPath, json);
            if (File.Exists(path))
                File.Delete(path);
            File.Move(tempPath, path);
        }
    }
}
