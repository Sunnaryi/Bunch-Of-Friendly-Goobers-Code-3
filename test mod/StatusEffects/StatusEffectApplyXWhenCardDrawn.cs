using System.Collections;

namespace Tutorial6_StatusIcons
{
    public class StatusEffectApplyXWhenCardDrawn : StatusEffectApplyX
    {
        public override void Init()
        {
            base.OnEnable += CheckEnable;
        }

        public override bool RunEnableEvent(Entity entity)
        {
            if (entity.InHand() && Battle.IsOnBoard(target))
            {
                return true;
            }

            return false;
        }

        public IEnumerator CheckEnable(Entity entity)
        {
            yield return Run(GetTargets());
        }
    }



    }
    













