using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class StatusEffectInstantSummonInDeck : StatusEffectInstantSummon
{
    public new enum Position
    {
        InFrontOf,
        EnemyRow,
        Hand,
        AppliersPosition,
        InFrontOfOrOtherRow,
        DrawPile,
        DiscardPile,
    }

    [SerializeField]
    public new bool canSummonMultiple;

    public new StatusEffectSummon targetSummon;

    [SerializeField]
    public new bool summonCopy;

    [SerializeField]
    public new Position summonPosition;

    [SerializeField]
    public new StatusEffectData[] withEffects;

    [SerializeField]
    public new bool queue = true;

    public new bool buildingToSummon;

    public new Entity toSummon;

    public override IEnumerator Process()
    {
        if (canSummonMultiple)
        {
            Routine.Clump clump = new Routine.Clump();
            int amount = GetAmount();
            for (int i = 0; i < amount; i++)
            {
                if (summonCopy)
                {
                    clump.Add(CreateCopyAndTrySummon());
                }
                else
                {
                    clump.Add(TrySummon());
                }
            }

            yield return clump.WaitForEnd();
        }
        else if (queue)
        {
            if (summonCopy)
            {
                new Routine(CreateCopy(target, delegate (Entity e)
                {
                    toSummon = e;
                }));
            }

            ActionQueue.Stack(new ActionSequence(TrySummon())
            {
                note = "Instant Summon"
            }, fixedPosition: true);
        }
        else
        {
            yield return TrySummon();
        }

        yield return base.Remove();
    }

    public new IEnumerator CreateCopy(Entity toCopy, UnityAction<Entity> onComplete)
    {
        buildingToSummon = true;
        Card card = null;
        if (CanSummon(out var container, out var _))
        {
            card = targetSummon.CreateCardCopy(target.data, container, applier.display.hover.controller);
            card.entity.startingEffectsApplied = true;
            yield return card.UpdateData();
            yield return targetSummon.CopyStatsAndEffects(card.entity, toCopy);
        }

        buildingToSummon = false;
        onComplete?.Invoke(card ? card.entity : null);
    }

    public new IEnumerator CreateCopyAndTrySummon()
    {
        yield return CreateCopy(target, delegate (Entity e)
        {
            toSummon = e;
        });
        if ((bool)toSummon)
        {
            yield return TrySummon();
        }
    }

    public new IEnumerator TrySummon()
    {
        if (buildingToSummon)
        {
            yield return new WaitUntil(() => toSummon);
        }

        if (CanSummon(out var container, out var shoveData))
        {
            if (shoveData != null)
            {
                yield return ShoveSystem.DoShove(shoveData, updatePositions: true);
            }

            int amount = GetAmount();
            yield return toSummon ? targetSummon.SummonPreMade(toSummon, container, applier.display.hover.controller, applier, withEffects, amount) : (summonCopy ? targetSummon.SummonCopy(target, container, applier.display.hover.controller, applier, withEffects, amount) : targetSummon.Summon(container, applier.display.hover.controller, applier, withEffects, amount));
        }
        else if (NoTargetTextSystem.Exists())
        {
            if ((bool)toSummon)
            {
                toSummon.RemoveFromContainers();
                UnityEngine.Object.Destroy(toSummon);
            }

            yield return NoTargetTextSystem.Run(target, NoTargetType.NoSpaceToSummon);
        }

        yield return null;
    }

    public new static IEnumerator ApplyEffects(Entity applier, Entity entity, IEnumerable<StatusEffectData> effects, int count)
    {
        Hit hit = new Hit(applier, entity, 0)
        {
            countsAsHit = false,
            canRetaliate = false
        };
        foreach (StatusEffectData effect in effects)
        {
            hit.AddStatusEffect(effect, count);
        }

        yield return hit.Process();
    }

    public new bool CanSummon(out CardContainer container, out Dictionary<Entity, List<CardSlot>> shoveData)
    {
        bool result = false;
        container = null;
        shoveData = null;
        switch (summonPosition)
        {
            case Position.InFrontOf:
                result = CanSummonInFrontOf(target, out container, out shoveData);
                break;

            case Position.EnemyRow:
                result = CanSummonInEnemyRow(target, out container, out shoveData);
                break;

            case Position.Hand:
                container = References.Player.handContainer;
                result = true;
                break;

            case Position.AppliersPosition:
                result = CanSummonInFrontOf(applier, out container, out shoveData);
                break;

            case Position.InFrontOfOrOtherRow:
                if (CanSummonInFrontOf(target, out container, out shoveData))
                {
                    result = container.owner.team == applier.owner.team || CanSummonInEnemyRow(target, out container, out shoveData);
                }
                break;

            case Position.DiscardPile:
                container = References.Player.discardContainer;
                result = true;
                break;

            case Position.DrawPile:
                container = References.Player.drawContainer;
                result = true;
                break;
        }

        return result;
    }

    public new static bool CanSummonInFrontOf(Entity inFrontOf, out CardContainer container, out Dictionary<Entity, List<CardSlot>> shoveData)
    {
        bool flag = false;
        container = null;
        shoveData = null;
        bool flag2 = Battle.IsOnBoard(inFrontOf);
        if (!inFrontOf.alive || !flag2)
        {
            if (inFrontOf.actualContainers.Count > 0)
            {
                container = inFrontOf.actualContainers.RandomItem();
                flag = true;
            }
            else if (inFrontOf.preActualContainers.Length != 0)
            {
                container = inFrontOf.preActualContainers.RandomItem();
                flag = true;
            }
        }

        if (!flag && flag2)
        {
            CardContainer slotToShove = GetSlotToShove(inFrontOf);
            if ((object)slotToShove != null)
            {
                Entity top = slotToShove.GetTop();
                if (!top || ShoveSystem.CanShove(top, top.owner.entity, out shoveData))
                {
                    container = slotToShove;
                    flag = true;
                }
            }
        }

        return flag;
    }

    public new static CardContainer GetSlotToShove(Entity entity)
    {
        if (entity.actualContainers.Count <= 0)
        {
            if (entity.preActualContainers.Length == 0)
            {
                return null;
            }

            return entity.preActualContainers.RandomItem();
        }

        return entity.actualContainers.RandomItem();
    }

    public new static bool CanSummonInEnemyRow(Entity target, out CardContainer container, out Dictionary<Entity, List<CardSlot>> shoveData)
    {
        bool result = false;
        container = null;
        shoveData = null;
        CardContainer[] array = target.containers;
        if (array == null || array.Length == 0)
        {
            array = target.preContainers;
        }

        if (array != null && array.Length != 0)
        {
            array.Shuffle();
            CardContainer[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                if (!(array2[i] is CardSlotLane row))
                {
                    continue;
                }

                CardSlotLane oppositeRow = References.Battle.GetOppositeRow(row);
                if ((object)oppositeRow != null)
                {
                    CardSlot cardSlot = oppositeRow.slots[0];
                    Entity top = cardSlot.GetTop();
                    if (top == null || ShoveSystem.CanShove(top, target.owner.entity, out shoveData))
                    {
                        container = cardSlot;
                        result = true;
                        break;
                    }
                }
            }
        }

        return result;
    }
}

      
    













