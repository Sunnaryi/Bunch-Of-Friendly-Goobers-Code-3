using System.Collections;



public partial class Goobers
{
    public class StatusEffectInstantSummonWithCharms : StatusEffectInstantSummon
    {
        public CardData trueData;


        public override IEnumerator Process()
        {
            targetSummon.summonCard = trueData.Clone();

            for (int i = 0; i < target.data.upgrades.Count; i++) //Iterate through charms
            {
                CardUpgradeData upgrade = target.data.upgrades[i].Clone();
                if (upgrade.CanAssign(targetSummon.summonCard)) //Check if each upgrade is valid
                {
                    upgrade.Assign(targetSummon.summonCard); //Assign the charm. Suprisingly simple, huh?
                }

            }
            for (int i = ActionQueue.instance.queue.Count - 1; i >= 1; i--)
            {
                if (ActionQueue.instance.queue[i] is ActionTrigger trigger && trigger.entity == target)
                {
                    ActionQueue.Remove(ActionQueue.instance.queue[i]);
                }
            }
            yield return base.Process(); //Do everything InstantSummon does.
        }
    }
}
    













