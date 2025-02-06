using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

internal class StatusEffectInstantCombineCard : StatusEffectInstant
    {

        [Serializable]
        public struct Combo
        {
            public string[] cardNames;

            public string resultingCardName;

            public bool AllCardsInDeck(List<Entity> deck)
            {
                bool result = true;
                string[] array = cardNames;
                foreach (string cardName in array)
                {
                    if (!HasCard(cardName, deck))
                    {
                        result = false;
                        break;
                    }
                }

                return result;
            }

            public List<Entity> FindCards(List<Entity> deck)
            {
                List<Entity> tooFuse = new List<Entity>();
                string[] array = cardNames;
                foreach (string cardName in array)
                {
                    foreach (Entity item in deck)
                    {
                        if (item.data.name == cardName)
                        {
                            tooFuse.Add(item);
                            break;
                        }
                    }
                }

                return tooFuse;
            }

            public bool HasCard(string cardName, List<Entity> deck)
            {
                foreach (Entity item in deck)
                {
                    if (item.data.name == cardName)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [SerializeField]
        public string combineSceneName = "CardCombine";

        public string[] cardNames;

        public string resultingCardName;

        public bool checkHand = true;
        public bool checkDeck = true;
        public bool checkBoard = true;

        public bool keepUpgrades = true;
        public List<CardUpgradeData> extraUpgrades;

        public bool spawnOnBoard = false;

        public bool changeDeck = false;

        public override IEnumerator Process()
        {
            Combo combo = new Combo()
            {
                cardNames = cardNames,
                resultingCardName = resultingCardName
            };

            List<Entity> fulldeck = new List<Entity>();
            if (checkHand)
            {
                fulldeck.AddRange(References.Player.handContainer.ToList());
            }
            if (checkDeck)
            {
                fulldeck.AddRange(References.Player.drawContainer.ToList());
                fulldeck.AddRange(References.Player.discardContainer.ToList());
            }
            if (checkBoard)
            {
                fulldeck.AddRange(Battle.GetCardsOnBoard(References.Player).ToList());
            }


            if (combo.AllCardsInDeck(fulldeck))
            {
                CombineAction action = new CombineAction(keepUpgrades, extraUpgrades, spawnOnBoard, target.containers[0]);
                action.combineSceneName = combineSceneName;
                action.tooFuse = combo.FindCards(fulldeck);
                action.combo = combo;

                if (changeDeck)
                {
                    EditDeck(combo.cardNames, combo.resultingCardName);
                }

                bool queueAction = true;
                foreach (PlayAction playAction in ActionQueue.instance.queue)
                {
                    if (playAction.GetType() == action.GetType())
                    {
                        queueAction = false;
                        break;
                    }
                }

                if (queueAction)
                {
                    ActionQueue.Add(action);
                }
            }

            yield return base.Process();
        }

        public void EditDeck(string[] cardsToRemove, string cardToAdd)
        {
            List<CardData> oldCards = new List<CardData>();

            foreach (string name in cardsToRemove)
            {
                foreach (CardData card in References.Player.data.inventory.deck)
                {
                    if (card.name == name && !oldCards.Contains(card))
                    {
                        oldCards.Add(card);
                        break;
                    }


                }
            }

            if (oldCards.Count == cardsToRemove.Length)
            {
                List<CardUpgradeData> upgrades = new List<CardUpgradeData> { };

                foreach (CardData card in oldCards)
                {
                    if (keepUpgrades)
                    {
                        upgrades.AddRange(card.upgrades.Select(u => u.Clone()));
                    }

                    References.Player.data.inventory.deck.Remove(card);
                }

                CardData cardDataClone = AddressableLoader.GetCardDataClone(cardToAdd);

                upgrades.AddRange(extraUpgrades.Select(u => u.Clone()));

                foreach (CardUpgradeData upgrade in upgrades)
                {
                    upgrade.Assign(cardDataClone);
                }

            if (cardDataClone.cardType.miniboss)
            {
                References.Player.data.inventory.deck.Insert(0, cardDataClone);
            }

            else
            {
                References.Player.data.inventory.deck.Add(cardDataClone);
            }


        }


        }

        public class CombineAction : PlayAction
        {

            [SerializeField]
            public string combineSceneName;

            public Combo combo;

            public List<Entity> tooFuse;

            public bool keepUpgrades;

            public List<CardUpgradeData> extraUpgrades;

            public bool spawnOnBoard;

            public CardContainer row;

            public CombineAction(bool keepUpgrades, List<CardUpgradeData> extraUpgrades, bool spawnOnBoard, CardContainer row)
            {
                this.keepUpgrades = keepUpgrades;
                this.extraUpgrades = extraUpgrades;
                this.spawnOnBoard = spawnOnBoard;
                this.row = row;


            }

            public override IEnumerator Run()
            {
                return CombineSequence(combo, tooFuse);
            }

            public IEnumerator CombineSequence(Combo combo, List<Entity> tooFuse)
            {
                CombineCardSequence combineSequence = null;
                yield return SceneManager.Load(combineSceneName, SceneType.Temporary, delegate (Scene scene)
                {
                    combineSequence = scene.FindObjectOfType<CombineCardSequence>();
                });
                if ((bool)combineSequence)
                {
                    yield return combineSequence.Run2(tooFuse, combo.resultingCardName, keepUpgrades, extraUpgrades, spawnOnBoard, row);
                }

                yield return SceneManager.Unload(combineSceneName);
            }

        }

    }
    













