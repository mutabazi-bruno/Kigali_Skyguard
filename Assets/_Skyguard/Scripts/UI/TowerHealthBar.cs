using Skyguard.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skyguard.UI
{
    // floats above its tower and always turns to face the player
    public class TowerHealthBar : MonoBehaviour
    {
        [SerializeField] RelayTower tower;
        [SerializeField] Image fill;
        [SerializeField] TMP_Text label;
        [SerializeField] Gradient colours;

        Transform view;
        int shown = -1;

        void Start() => view = Camera.main.transform;

        void LateUpdate()
        {
            var away = transform.position - view.position;
            away.y = 0f;
            if (away.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(away);

            if (tower.Health == shown)
                return;
            shown = tower.Health;
            float t = (float)tower.Health / tower.MaxHealth;
            fill.fillAmount = t;
            fill.color = colours.Evaluate(t);
            label.text = $"TOWER {tower.Health}%";
        }
    }
}
