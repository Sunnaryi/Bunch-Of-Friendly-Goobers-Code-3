using NexPlugin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Goobers
{
    public class StatusEffectApplyXWhenItemPlayed : StatusEffectApplyXOnCardPlayed
    {
        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (entity.data.cardType.name == "Item")
            {
                return true;
            }

            return false;
        }
    }


}







