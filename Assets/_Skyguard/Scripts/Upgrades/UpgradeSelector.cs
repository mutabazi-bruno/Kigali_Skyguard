using System;
using System.Collections.Generic;
using System.Linq;
using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Upgrades
{
    public class UpgradeSelector : MonoBehaviour
    {
        [SerializeField] UpgradeCard[] cards;
        [SerializeField] float spacing = 0.9f;

        readonly List<IUpgrade> all = new()
        {
            new ExtendedMagazine(),
            new QuickLoad(),
            new PiercingUpgrade(),
            new TowerRepair(),
            new SlowLasers(),
            new Overdrive()
        };

        UpgradeContext context;
        Action chosen;

        void Awake() => Cancel();

        public void Offer(UpgradeContext upgradeContext, Transform anchor, Action onChosen)
        {
            context = upgradeContext;
            chosen = onChosen;

            var options = all.Where(u => u.CanOffer(context)).OrderBy(_ => UnityEngine.Random.value).Take(cards.Length).ToList();
            for (int i = 0; i < cards.Length; i++)
            {
                if (i >= options.Count)
                {
                    cards[i].Hide();
                    continue;
                }
                float x = (i - (options.Count - 1) * 0.5f) * spacing;
                cards[i].Show(options[i], anchor.position + anchor.right * x, anchor.rotation, Pick);
            }
        }

        public void Cancel()
        {
            foreach (var card in cards)
                card.Hide();
            chosen = null;
        }

        void Pick(IUpgrade upgrade)
        {
            var callback = chosen;
            Cancel();
            upgrade.Apply(context);
            EventBus.Publish(new UpgradeChosen { Title = upgrade.Title });
            callback?.Invoke();
        }
    }
}
