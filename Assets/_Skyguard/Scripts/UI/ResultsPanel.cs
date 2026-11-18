using Skyguard.Data;
using Skyguard.Flow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skyguard.UI
{
    public class ResultsPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text stats;
        [SerializeField] GameObject initialsRow;
        [SerializeField] TMP_Text[] letters;
        [SerializeField] Button[] upButtons;
        [SerializeField] Button[] downButtons;
        [SerializeField] Button saveInitialsButton;
        [SerializeField] Button playAgainButton;
        [SerializeField] Button quitButton;

        readonly char[] initials = new char[3];
        ScoreEntry entry;

        static GameManager Game => GameManager.Instance;

        void Awake()
        {
            for (int i = 0; i < 3; i++)
            {
                int slot = i;
                UiButtons.Hook(upButtons[i], () => Step(slot, 1));
                UiButtons.Hook(downButtons[i], () => Step(slot, -1));
            }
            UiButtons.Hook(saveInitialsButton, SaveInitials);
            UiButtons.Hook(playAgainButton, () => Game.QuitToMenu());
            UiButtons.Hook(quitButton, () => Game.QuitGame());
        }

        public void Show(RunResult result)
        {
            title.text = result.Won ? "Dawn. The city is safe." : "The tower fell.";
            string rank = result.Rank > 0 ? $"#{result.Rank} on the leaderboard" : "Not in the top 10 this time";
            string waves = result.Mode == GameMode.Endless ? $"{result.Waves} rounds survived" : $"{result.Waves} of 10 waves";
            stats.text = $"Score  {result.Score:N0}\n{waves}\nAccuracy  {result.Accuracy:P0}\n{rank}\n<size=70%>average {result.AverageFps:F0} fps</size>";

            entry = result.Entry;
            initialsRow.SetActive(entry != null);
            if (entry == null)
                return;

            for (int i = 0; i < 3; i++)
                initials[i] = i < entry.initials.Length ? entry.initials[i] : 'A';
            RefreshLetters();
        }

        // arcade style: three letters, up and down arrows, no keyboard needed in a headset
        void Step(int slot, int direction)
        {
            int letter = (initials[slot] - 'A' + direction + 26) % 26;
            initials[slot] = (char)('A' + letter);
            RefreshLetters();
        }

        void RefreshLetters()
        {
            for (int i = 0; i < 3; i++)
                letters[i].text = initials[i].ToString();
        }

        void SaveInitials()
        {
            if (entry == null)
                return;
            Game.RenameEntry(entry, new string(initials));
            initialsRow.SetActive(false);
        }
    }
}
