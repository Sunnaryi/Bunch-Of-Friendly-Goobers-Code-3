using HarmonyLib;
using UnityEngine;
using System.Collections;
using Dead;



public partial class Goobers
{
    [HarmonyPatch(typeof(CombineCardSequence), "Run", typeof(CardData[]), typeof(CardData))]
    public class BetterComineSystem
    {
        static IEnumerator Yeet(CombineCardSequence __instance, CardData[] cards, CardData finalCard)
        {
            CinemaBarSystem.State cinemaBarState = new CinemaBarSystem.State();
            PauseMenu.Block();
            CinemaBarSystem.SetSortingLayer("UI2", 100);
            CinemaBarSystem.In();
            Entity[] entities = __instance.CreateEntities(cards);
            Entity finalEntity = __instance.CreateFinalEntity(finalCard);
            Routine.Clump clump = new Routine.Clump();
            foreach (Entity entity in entities)
            {
                clump.Add(entity.display.UpdateData(false));
            }
            clump.Add(finalEntity.display.UpdateData(false));
            clump.Add(Sequences.Wait(0.5f));
            yield return clump.WaitForEnd();
            Entity[] array = entities;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].transform.localScale = Vector3.one * 0.8f;
            }
            foreach (Entity entity2 in entities)
            {
                foreach (var upgrade in entity2._data.upgrades)
                {
                    References.PlayerData.inventory.upgrades.Add(upgrade);
                }
                References.PlayerData.inventory.deck.Remove(entity2.data);
            }
            References.PlayerData.inventory.deck.Add(finalEntity.data);
            __instance.fader.In();
            Vector3 vector = Vector3.zero;
            foreach (Entity entity3 in entities)
            {
                vector += entity3.transform.position;
            }
            vector /= (float)entities.Length;
            __instance.group.position = vector;
            foreach (Entity entity4 in entities)
            {
                Transform transform = global::UnityEngine.Object.Instantiate<Transform>(__instance.pointPrefab, entity4.transform.position, Quaternion.identity, __instance.group);
                transform.gameObject.SetActive(true);
                entity4.transform.SetParent(transform);
                __instance.points.Add(transform);
                LeanTween.alphaCanvas(((Card)entity4.display).canvasGroup, 1f, 0.4f).setEaseInQuad();
            }
            foreach (Transform transform2 in __instance.points)
            {
                Vector3 normalized = transform2.localPosition.normalized;
                LeanTween.moveLocal(transform2.gameObject, normalized, 0.4f).setEaseInQuart();
            }
            yield return new WaitForSeconds(0.4f);
            __instance.Flash(0.5f, 0.15f);
            Events.InvokeScreenShake(1f, new float?(0f));
            array = entities;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].wobbler.WobbleRandom(1f);
            }
            __instance.hitPs.Play();
            foreach (Transform transform3 in __instance.points)
            {
                Vector3 vector2 = transform3.localPosition.normalized * 3f;
                LeanTween.moveLocal(transform3.gameObject, vector2, 1f).setEase(__instance.bounceCurve);
            }
            LeanTween.moveLocal(__instance.group.gameObject, new Vector3(0f, 0f, -2f), 1f).setEaseInOutQuad();
            LeanTween.rotateZ(__instance.group.gameObject, PettyRandom.Range(160f, 180f), 1f).setOnUpdateVector3(delegate (Vector3 a)
            {
                foreach (Transform transform4 in __instance.points)
                {
                    transform4.transform.eulerAngles = Vector3.zero;
                }
            }).setEaseInOutQuad();
            yield return new WaitForSeconds(1f);
            __instance.Flash(1f, 0.15f);
            Events.InvokeScreenShake(1f, new float?(0f));
            if (__instance.ps)
            {
                __instance.ps.Play();
            }
            __instance.combinedFx.SetActive(true);
            finalEntity.transform.position = Vector3.zero;
            array = entities;
            for (int i = 0; i < array.Length; i++)
            {
                CardManager.ReturnToPool(array[i]);
            }
            __instance.group.transform.localRotation = Quaternion.identity;
            finalEntity.curveAnimator.Ping();
            finalEntity.wobbler.WobbleRandom(1f);
            CinemaBarSystem.Top.SetScript(__instance.titleKey.GetLocalizedString());
            CinemaBarSystem.Bottom.SetPrompt(__instance.continueKey.GetLocalizedString(), "Select");
            while (!InputSystem.IsButtonPressed("Select", true))
            {
                yield return null;
            }
            cinemaBarState.Restore();
            CinemaBarSystem.SetSortingLayer("CinemaBars", 0);
            __instance.fader.gameObject.Destroy();
            __instance.cardSelector.character = References.Player;
            __instance.cardSelector.MoveCardToDeck(finalEntity);
            PauseMenu.Unblock();
            yield break;
        }
        public static bool Prefix(ref IEnumerator __result, ref CombineCardSequence __instance, ref CardData[] cards, ref CardData finalCard)
        {
            __result = Yeet(__instance, cards, finalCard);
            return false;
        }
    }
}















