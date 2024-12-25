using Deadpan.Enums.Engine.Components.Modding;
using System.Collections.Generic;
using System.Linq;
using System.Collections;



public partial class Goobers
{
    public class StatusEffectInstantFillXBoardSlots : StatusEffectInstant
    {
        public bool random = false;

        public bool clearBoardFirst = false;

        public CardData[] withCards;

        public readonly List<CardData> pool = new List<CardData>();

        public override IEnumerator Process()
        {
            List<CardContainer> rows = References.Battle.GetRows(target.owner);
            if (clearBoardFirst)
            {
                foreach (CardContainer cardContainer in rows)
                {
                    CardSlotLane cardSlotLane2 = cardContainer as CardSlotLane;
                    if (!(cardSlotLane2 != null))
                    {
                        continue;
                    }

                    foreach (CardSlot slot3 in cardSlotLane2.slots)
                    {
                        if (slot3.Empty || !slot3.entities.All((Entity e) => e.name != target.name))
                        {
                            continue;
                        }

                        List<Entity> toKill = new List<Entity>(slot3.entities);
                        foreach (Entity e2 in toKill)
                        {
                            yield return e2.Kill(DeathType.Sacrifice);
                        }
                    }
                }
            }

            List<CardSlot> list = new List<CardSlot>();
            List<CardSlot> list2 = new List<CardSlot>();
            List<CardSlot> list3 = new List<CardSlot>();
            int amount = GetAmount();
            int i = 0;
            if (!random)
            {
                CardSlotLane cardSlotLane3 = rows[0] as CardSlotLane;
                if (cardSlotLane3 != null)
                {
                    list2 = cardSlotLane3.slots.ToArray().RemoveFromArray(item => item.Empty).ToList();
                }
                CardSlotLane cardSlotLane4 = rows[1] as CardSlotLane;
                if (cardSlotLane4 != null)
                {
                    list3 = cardSlotLane4.slots.ToArray().RemoveFromArray(item => item.Empty).ToList();
                }
                for (int i2 = 0; i2 < amount; i2++)
                {
                    if (list2.Count() > 0 && list3.Count() > 0)
                    {
                        if (Dead.Random.Range(1, 2) == 1)
                        {
                            list.Add(list2[0]);
                            list2.RemoveAt(0);
                        }
                        else
                        {
                            list.Add(list3[0]);
                            list3.RemoveAt(0);
                        }
                    }
                    else
                    {
                        if (list2.Count() > 0)
                        {
                            list.Add(list2[0]);
                            list2.RemoveAt(0);
                        }
                        else
                        {
                            if (list3.Count() > 0)
                            {
                                list.Add(list3[0]);
                                list3.RemoveAt(0);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                while (i < amount && rows.SelectMany((CardContainer row) => (row as CardSlotLane).slots.Where((CardSlot slot) => slot.Empty && !list.Contains(slot))).Any())
                {
                    CardContainer[] containers = rows.ToArray();
                    CardSlotLane cardSlotLane = containers.RandomItem() as CardSlotLane;
                    if (cardSlotLane != null)
                    {
                        CardSlot slot2 = cardSlotLane.slots.RandomItem();
                        if (slot2.Empty && i < amount && !list.Contains(slot2))
                        {
                            list.Add(slot2);
                            i++;
                        }
                    }
                }
            }

            foreach (CardSlot slot5 in list)
            {
                CardData data = Pull().Clone();
                Card card = CardManager.Get(data, References.Battle.playerCardController, target.owner, inPlay: true, target.owner.team == References.Player.team);
                yield return card.UpdateData();
                CardDiscoverSystem.instance.DiscoverCard(data);
                target.owner.reserveContainer.Add(card.entity);
                target.owner.reserveContainer.SetChildPosition(card.entity);
                ActionQueue.Stack(new ActionMove(card.entity, slot5), fixedPosition: true);
                ActionQueue.Stack(new ActionRunEnableEvent(card.entity), fixedPosition: true);
                foreach (var status in target.statusEffects)
                {
                    if (status is StatusEffectApplyToSummon && !target.silenced)
                    {
                        int statusAmount = status.count;
                        if (status.canBeBoosted) { statusAmount = (int)((statusAmount + target.effectBonus) * (target.effectFactor)); }
                        ActionQueue.Stack(new ActionApplyStatus(card.entity, target, (status as StatusEffectApplyToSummon).effectToApply, statusAmount), fixedPosition: true);
                    }
                }
            }

            list = null;
            yield return base.Process();
        }

        public CardData Pull()
        {
            if (pool.Count <= 0)
            {
                pool.AddRange(withCards);
            }

            return pool.TakeRandom();
        }
    }
  
 }

















