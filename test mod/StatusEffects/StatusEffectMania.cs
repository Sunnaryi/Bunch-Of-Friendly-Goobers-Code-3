using System.Linq;
using System.Collections;

internal class StatusEffectMania : StatusEffectWhileActiveX
    {

    public override bool TargetSilenced() => false;
    public override void Init()
        {
            OnCardPlayed += Check;
            base.Init();
        }
        private IEnumerator Check(Entity entity, Entity[] targets)
        {
        yield return Deactivate(); yield return Remove(); }


        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (entity != target)
                return false;

           
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

















