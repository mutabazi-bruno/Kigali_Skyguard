using System;
using System.Text;
using Skyguard.Data;
using Skyguard.Flow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skyguard.UI
{
    public class LeaderboardPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text table;
        [SerializeField] Button backButton;

        public event Action Back;

        void Awake() => UiButtons.Hook(backButton, () => Back?.Invoke());

        void OnEnable()
        {
            var scores = GameManager.Instance.Leaderboard.TopScores;
            if (scores.Count == 0)
            {
                table.text = "No scores yet. Hold the towers and get on the board.";
                return;
            }

            // the font isn't monospaced, so the columns are lined up with tmp position tags instead of padding
            var text = new StringBuilder("<color=#7FD8D2><size=80%><pos=0%>#<pos=9%>NAME<pos=27%>SCORE<pos=52%>WAVE<pos=67%>HITS<pos=82%>RESULT</size></color>\n");
            for (int i = 0; i < scores.Count; i++)
            {
                var s = scores[i];
                string mode = s.mode == GameMode.Endless ? "Endless" : s.won ? "Dawn" : "Fell";
                string row = $"<pos=0%>{i + 1}<pos=9%>{s.initials}<pos=27%>{s.score:N0}<pos=52%>{s.waves}<pos=67%>{s.accuracy:P0}<pos=82%>{mode}";
                text.AppendLine(i == 0 ? $"<color=#F5B83D>{row}</color>" : row);
            }
            table.text = text.ToString();
        }
    }
}
