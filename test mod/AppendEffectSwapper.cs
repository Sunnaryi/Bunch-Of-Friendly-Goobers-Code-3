using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[HarmonyPatch(typeof(FinalBossGenerationSettings), "ProcessEffects", new Type[]
{
          typeof(IList<CardData>)
})]
internal static class AppendEffectSwapper
{
    internal static void Prefix(FinalBossGenerationSettings __instance)
    {
        foreach (FinalBossEffectSwapper swapper in __instance.effectSwappers)
        {
            if (swapper.effect.name.Contains("Buff Card In Deck On Kill")) //If it's already there no need to check further.
            {
                return;
            }
        }

        List<FinalBossEffectSwapper> swappers = new List<FinalBossEffectSwapper>();
        swappers.Add(CreateSwapper("Add Rift to Hand", "ARIFT NOW", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Add Bone Needle to Hand", "On Hit Equal Teeth To Self", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("While Active Spark When Drawn To Allies In Hand", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("When Sacrificed Summon M2", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Random YraBot", "Random AYraBot", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Add CTB to Hand", "Random Bom On Turn", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Add CTB to Hands", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Gain Sweet Point Self terror", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Random Tala", "Shroom", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("On Card Played Damage To Self", "Frost", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Add Bone Needle to Hand", "On Hit Equal Teeth To Self", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Set Attack", "Set Attack", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Reduce Max Counter", "Reduce Max Counter", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Set Max Health", "Set Max Health", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Set Health", "Set Health", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Not Fast Enough", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Perma into CB23Q", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Perma into CB23R", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Perma into CB23B", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Perma into CB23KG", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Perma into CB23K", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("Shell", minBoost: 0, maxBoost: 0));
        swappers.Add(CreateSwapper("When Ally Is Healed Apply Equal EXP", minBoost: 0, maxBoost: 0));
        __instance.effectSwappers = __instance.effectSwappers.AddRangeToArray(swappers.ToArray()).ToArray();
    }

    internal static FinalBossEffectSwapper CreateSwapper(string effect, string replaceOption = null, string attackOption = null, int minBoost = 0, int maxBoost = 0)
    {
        FinalBossEffectSwapper swapper = ScriptableObject.CreateInstance<FinalBossEffectSwapper>();
        swapper.effect = Goobers.Instance.TryGet<StatusEffectData>(effect);
        swapper.replaceWithOptions = new StatusEffectData[0];
        String s = "";
        if (!replaceOption.IsNullOrEmpty())
        {
            swapper.replaceWithOptions = swapper.replaceWithOptions.Append(Goobers.Instance.TryGet<StatusEffectData>(replaceOption)).ToArray();
            s += swapper.replaceWithOptions[0].name;
        }
        if (!attackOption.IsNullOrEmpty())
        {
            swapper.replaceWithAttackEffect = Goobers.Instance.TryGet<StatusEffectData>(attackOption);
            s += swapper.replaceWithAttackEffect.name;
        }
        if (s.IsNullOrEmpty())
        {
            s = "Nothing";
        }
        swapper.boostRange = new Vector2Int(minBoost, maxBoost);
        Debug.Log($"[Pokefrost] {swapper.effect.name} => {s} + {swapper.boostRange}");
        return swapper;
    }
}
    













