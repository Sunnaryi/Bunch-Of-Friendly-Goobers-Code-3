using System.Collections;

internal class StatusEffectInstantAddDeck : StatusEffectInstant
    {

        public CardData card;
        public override IEnumerator Process()
        {
            References.Player.data.inventory.deck.Add(card.Clone());
            Campaign.PromptSave();

        yield return base.Process();
        }
    }
    













