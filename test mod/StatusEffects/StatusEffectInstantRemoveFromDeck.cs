using HarmonyLib;
using System.Collections;
using UnityEngine;

public class StatusEffectInstantRemoveFromDeck : StatusEffectInstant
    {

        public override IEnumerator Process()
        {
            Debug.Log("Test log 1");
            Inventory inventory = References.PlayerData.inventory;
            CardData current = target.data;

            foreach (CardUpgradeData upgrade in current.upgrades)
                inventory.upgrades.Add(Goobers.Instance.TryGet<CardUpgradeData>(upgrade.name).Clone());

            inventory.deck.RemoveWhere(c => c.id == current.id);
        Campaign.PromptSave();


        yield return base.Process();
        }


    }
    













