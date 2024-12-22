using HarmonyLib;

[HarmonyPatch(typeof(StatusEffectBombard), nameof(StatusEffectBombard.SetTargets))]
    class RemoveBombardTarget
    {
        static bool Prefix(StatusEffectBombard __instance)
        {
            if (!__instance.target.alive) return false;

            return true;
        }
    }
    













