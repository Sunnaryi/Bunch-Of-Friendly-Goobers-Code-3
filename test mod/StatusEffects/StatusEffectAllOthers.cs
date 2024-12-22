using System.Collections;
using System.Linq;

public class StatusEffectAllOthers : StatusEffectWhileActiveX
    {

    public override bool TargetSilenced() => false;

    private bool prime = false;

        public override void Init()
        {
            OnCardPlayed += Check;
            base.Init();
        }

            private IEnumerator Check(Entity entity, Entity[] targets)
            {
                if (count <= 1)
                {
                    yield return Deactivate();
                }

                yield return RemoveStacks(1, false);
            }
        

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (entity != target)
                return false;

            if (!prime && targets.Length == 0)
                return false;
            prime = true;
            if (ActionQueue.GetActions().Any(a => a is ActionTrigger at && at.entity == target))

                return false;
            return true;
        }


    public override bool CanActivate() => Battle.IsOnBoard(target) || target.InHand();

    public override bool CheckActivateOnMove(
        CardContainer[] fromContainers,
        CardContainer[] toContainers)
    {
        return (Battle.IsOnBoard(toContainers) || target.InHand()) && !Battle.IsOnBoard(fromContainers);
    }

    public override bool CheckDeactivateOnMove(
        CardContainer[] fromContainers,
        CardContainer[] toContainers)
    {
        return !Battle.IsOnBoard(toContainers) && (Battle.IsOnBoard(fromContainers) || target.InHand());
    }
}
    













