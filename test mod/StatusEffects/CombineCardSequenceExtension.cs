using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public static class CombineCardSequenceExtension

    {

    private static IEnumerator Postfix(IEnumerator __result, CombineCardSequence __instance, CardData[] cards, CardData finalCard)
    {
        yield return __result;

        if (!finalCard.cardType.miniboss)
            yield break;

        var deck = References.PlayerData.inventory.deck;
        if (deck.Remove(finalCard))
            deck.Insert(0, finalCard);
    }
    public static IEnumerator Run2(this CombineCardSequence seq, List<Entity> cardsToCombine, string resultingCard, bool keepUpgrades, List<CardUpgradeData> extraUpgrades, bool spawnOnBoard, CardContainer row)
        {
            CardData cardDataClone = AddressableLoader.GetCardDataClone(resultingCard);

            List<CardUpgradeData> upgrades = new List<CardUpgradeData> { };
            if (keepUpgrades)
            {
                foreach (Entity ent in cardsToCombine)
                {
                    upgrades.AddRange(ent.data.upgrades.Select(u => u.Clone()));
                }
            }
            upgrades.AddRange(extraUpgrades.Select(u => u.Clone()));

            foreach (CardUpgradeData upgrade in upgrades)
            {
                upgrade.Assign(cardDataClone);
            }


            yield return Run2(seq, cardsToCombine.ToArray(), cardDataClone, spawnOnBoard, row);
        }

        public static IEnumerator Run2(this CombineCardSequence seq, Entity[] entities, CardData finalCard, bool spawnOnBoard, CardContainer row)

    {

        PauseMenu.Block();
            Card card = CardManager.Get(finalCard, Battle.instance.playerCardController, References.Player, inPlay: false, isPlayerCard: true);
            card.transform.localScale = Vector3.one * 1f;
            card.transform.SetParent(seq.finalEntityParent);
            Entity finalEntity = card.entity;
            Routine.Clump clump = new Routine.Clump();
            Entity[] array = entities;
            foreach (Entity entity in array)
            {
                clump.Add(entity.display.UpdateData());
            }

            clump.Add(finalEntity.display.UpdateData());
            clump.Add(Sequences.Wait(0.5f));
            yield return clump.WaitForEnd();

            array = entities;
            foreach (Entity entity2 in array)
            {
                entity2.RemoveFromContainers();
            }

            array = entities;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].transform.localScale = Vector3.one * 0.8f;
            }

            seq.fader.In();
            Vector3 zero = Vector3.zero;
            array = entities;
            foreach (Entity entity3 in array)
            {
                zero += entity3.transform.position;
            }

            zero /= (float)entities.Length;

            seq.group.position = zero;
            array = entities;
            foreach (Entity entity4 in array)
            {
                Transform transform = UnityEngine.Object.Instantiate(seq.pointPrefab, entity4.transform.position, Quaternion.identity, seq.group);
                transform.gameObject.SetActive(value: true);
                entity4.transform.SetParent(transform);
                entity4.flipper.FlipUp();
                seq.points.Add(transform);
                LeanTween.alphaCanvas(((Card)entity4.display).canvasGroup, 1f, 0.4f).setEaseInQuad();
            }

            foreach (Transform point in seq.points)
            {
                LeanTween.moveLocal(to: point.localPosition.normalized, gameObject: point.gameObject, time: 0.4f).setEaseInQuart();
            }

            yield return new WaitForSeconds(0.4f);

            Events.InvokeScreenShake(1f, 0f);
            array = entities;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].wobbler.WobbleRandom();
            }

            foreach (Transform point2 in seq.points)
            {
                LeanTween.moveLocal(to: point2.localPosition.normalized * 3f, gameObject: point2.gameObject, time: 1f).setEase(seq.bounceCurve);
            }

            LeanTween.moveLocal(seq.group.gameObject, new Vector3(0f, 0f, -2f), 1f).setEaseInOutQuad();
            LeanTween.rotateZ(seq.group.gameObject, Dead.PettyRandom.Range(160f, 180f), 1f).setOnUpdateVector3(delegate
            {
                foreach (Transform point3 in seq.points)
                {
                    point3.transform.eulerAngles = Vector3.zero;
                }
            }).setEaseInOutQuad();
            yield return new WaitForSeconds(1f);

            Events.InvokeScreenShake(1f, 0f);
            if ((bool)seq.ps)
            {
                seq.ps.Play();
            }

            seq.combinedFx.SetActive(value: true);

            finalEntity.transform.position = Vector3.zero;
            array = entities;
            for (int i = 0; i < array.Length; i++)
            {
                CardManager.ReturnToPool(array[i]);
            }

            seq.group.transform.localRotation = Quaternion.identity;
            finalEntity.curveAnimator.Ping();
            finalEntity.wobbler.WobbleRandom();

            yield return new WaitForSeconds(1f);

            seq.fader.gameObject.Destroy();
            PauseMenu.Unblock();

            //
            bool flag = true;


            if (spawnOnBoard)
            {

                if (row.owner == References.Player && row.Count != 3)
                {
                    yield return Sequences.CardMove(finalEntity, new CardContainer[1] { row });
                    finalEntity.inPlay = true;
                    flag = false;
                }

                if (flag)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        row = Battle.instance.GetRow(References.Player, i);
                        if (row.Count != 3)
                        {

                            yield return Sequences.CardMove(finalEntity, new CardContainer[1] { row });
                            finalEntity.inPlay = true;
                            flag = false;

                            break;
                        }
                    }
                }



            }



        if (spawnOnBoard)

            if (finalEntity.inPlay)
            {
                foreach (var statusEffect in finalEntity.statusEffects.Where(s => s is StatusEffectWhileActiveX).ToArray())
                {
                    if ((statusEffect as StatusEffectWhileActiveX).CanActivate())
                        yield return (statusEffect as StatusEffectWhileActiveX).Activate();

                }


            }

        if (flag)
        {
            yield return Sequences.CardMove(finalEntity, new CardContainer[1] { References.Player.handContainer });
            finalEntity.inPlay = true;
        }
        References.Player.handContainer.TweenChildPositions();
        ActionQueue.Stack(new ActionReveal(finalEntity));
        Events.InvokeEntityShowUnlocked(finalEntity);

        //
        if (flag)
            {
                yield return Sequences.CardMove(finalEntity, new CardContainer[1] { References.Player.handContainer });
                finalEntity.inPlay = true;
            }

            References.Player.handContainer.TweenChildPositions();
            ActionQueue.Stack(new ActionReveal(finalEntity));

        }

   


}
    













