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

            var text = new StringBuilder();
            for (int i = 0; i < scores.Count; i++)
            {
                var s = scores[i];
                string mode = s.mode == GameMode.Endless ? "Endless" : s.won ? "Dawn" : "Campaign";
                text.AppendLine($"{i + 1,2}.  {s.initials,-3}   {s.score,8:N0}   wave {s.waves,2}   {s.accuracy:P0}   {mode}   {s.difficulty}");
            }
            table.text = text.ToString();
        }
    }
}
