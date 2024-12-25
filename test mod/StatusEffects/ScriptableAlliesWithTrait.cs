using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using System.Linq;
using System.Threading;
using UnityEngine;



public partial class Goobers
{
    public class ScriptableAlliesWithTrait : ScriptableAmount
    {
        public TraitData trait;
        public bool allies;
        public bool enemies;
        public float multiplier = 1;



        public override int Get(Entity entity)
        {

            var result = 0;


            if (allies)
                entity.GetAllAllies().DoIf(a => a?.traits == null,
                a => Debug.LogWarning((a, a?.traits?.Where(t => t != null && t.data != null).Join())));

            if (allies)
            result += entity.GetAllAllies().Count(a => a.traits.Any(t => t.data.name == trait.name));



            if (enemies)
                result += entity.GetAllEnemies().Count(a => a.traits.Any(t => t.data.name == trait.name));


            return Mathf.FloorToInt(result * multiplier);

        }
    }


    public class ScriptableAlliesWithStatus : ScriptableAmount
    {
        public StatusEffectData status;
        public bool allies;
        public bool enemies;
        public float multiplier = 1;



        public override int Get(Entity entity)
        {

            var result = 0;


            if (allies)
                entity.GetAllAllies().DoIf(a => a?.traits == null,
                a => Debug.LogWarning((a, a?.traits?.Where(t => t != null && t.data != null).Join())));

            if (allies)
                result += entity.GetAllAllies().Count(a => a.statusEffects.Any(s => s.name == status.name));



            if (enemies)
                result += entity.GetAllEnemies().Count(a => a.statusEffects.Any(s => s.name == status.name));


            return Mathf.FloorToInt(result * multiplier);

        }
    }
}
    













