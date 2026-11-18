using System;
using System.Collections.Generic;
using Skyguard.Data;
using UnityEngine;

namespace Skyguard.Presentation
{
    [CreateAssetMenu(menuName = "Skyguard/Subtitle Table")]
    public class SubtitleTable : ScriptableObject
    {
        [Serializable]
        public class Line
        {
            public string key;
            [TextArea] public string english;
            [TextArea] public string french;
            [TextArea] public string kinyarwanda;
            public AudioClip voice;

            public string Text(SubtitleLanguage language) => language switch
            {
                SubtitleLanguage.French => french,
                SubtitleLanguage.Kinyarwanda => kinyarwanda,
                _ => english
            };
        }

        public List<Line> lines = new();

        Dictionary<string, Line> byKey;

        public bool TryGet(string key, out Line line)
        {
            if (byKey == null)
            {
                byKey = new Dictionary<string, Line>();
                foreach (var l in lines)
                    byKey[l.key] = l;
            }
            return byKey.TryGetValue(key, out line);
        }
    }
}
