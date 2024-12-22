﻿public class TargetModeTrulyRandom : TargetModeAll
    {
        public override bool Random => true;
        public override Entity[] GetTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            Entity random = base.GetTargets(entity, target, targetContainer)?.RandomItem();
            return random ? new Entity[] { random } : null;
        }
        public override Entity[] GetSubsequentTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            Entity random = base.GetSubsequentTargets(entity, target, targetContainer)?.RandomItem();
            return random ? new Entity[] { random } : null;
        }
    }
    













