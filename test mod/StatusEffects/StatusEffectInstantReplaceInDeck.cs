using System.Collections;

public class StatusEffectInstantReplaceInDeck : StatusEffectInstant
    {

        public CardData replaceWith;
        public override IEnumerator Process()
        {

            Transform();
            yield return base.Process();
        }
        public void Transform()
        {

            Inventory inventory = References.PlayerData.inventory;
            CardData current = target.data;
            CardData transformInto = replaceWith.Clone();

            foreach (CardUpgradeData upgrade in current.upgrades)
            {
                if (upgrade.CanAssign(transformInto))
                {
                    upgrade.Assign(transformInto);
                }
                else
                {
                    inventory.upgrades.Add(Goobers.Instance.TryGet<CardUpgradeData>(upgrade.name).Clone());
                }
            }

            if (current.cardType.name == "Leader")
            {
                transformInto.cardType = current.cardType;
                transformInto.SetCustomData("OverrideCardType", "Leader");
            }

            Card card = CardManager.Get(transformInto, null, References.Player, false, true);
            //Checks for renames
            CardData baseCard = Goobers.Instance.TryGet<CardData>(current.name);
            if (baseCard.title != current.title)
            {
                transformInto.forceTitle = current.title;
                if (card != null)
                {
                    card.SetName(current.title);
                    UnityEngine.Debug.Log("[Pokefrost] renamed evolution to " + current.title);
                }
                Events.InvokeRename(card.entity, current.title);
            }

            if (current.cardType.name == "Leader")
            {
                inventory.deck.Insert(0, card.entity.data);
            }
            else
            {
                inventory.deck.Add(card.entity.data);
            }
            inventory.deck.RemoveWhere(c => c.id == current.id);

        }
    }
    













