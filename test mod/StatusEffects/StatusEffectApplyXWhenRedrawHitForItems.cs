public class StatusEffectApplyXWhenRedrawHitForItems : StatusEffectApplyX
    {
        public override void Init()
        {
            Events.OnRedrawBellHit += RedrawBellHit;
        }

        public void OnDestroy()
        {
            Events.OnRedrawBellHit -= RedrawBellHit;
        }

        public void RedrawBellHit(RedrawBellSystem redrawBellSystem)
        {
            ActionQueue.Stack(new ActionSequence(Run(GetTargets())), fixedPosition: true);
        }
    }
    













