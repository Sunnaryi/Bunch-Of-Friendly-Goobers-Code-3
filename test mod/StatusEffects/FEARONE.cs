using UnityEngine;
using System.Collections;



public partial class Goobers
{
    public class FEARONE : StatusEffectApplyX
    {
        [SerializeField]

        public override bool TargetSilenced() => false;

        public bool instead;

        public bool whenAnyApplied;

        public string[] whenAppliedTypes = new string[1] { "snow" };

        [SerializeField]
        public ApplyToFlags whenAppliedToFlags;

        [SerializeField]
        public int requiredAmount = 0;

        [Header("Adjust Amount Applied")]
        [SerializeField]
        public bool adjustAmount;

        [SerializeField]
        public int addAmount;

        [SerializeField]
        public float multiplyAmount = 1f;

        public override void Init()
        {
            base.PostApplyStatus += Run;
        }

        public bool CheckType(StatusEffectData effectData)
        {
            if (effectData.isStatus)
            {
                if (!whenAnyApplied)
                {
                    return whenAppliedTypes.Contains(effectData.type);
                }

                return true;
            }

            return false;
        }

        public override bool RunApplyStatusEvent(StatusEffectApply apply)
        {
            if ((adjustAmount || instead) && target.enabled && !TargetSilenced() && (target.alive || !targetMustBeAlive) && (bool)apply.effectData && apply.count > 0 && CheckType(apply.effectData) && CheckTarget(apply.target))
            {
                if (instead)
                {
                    apply.effectData = effectToApply;
                }

                if (adjustAmount)
                {
                    apply.count += addAmount;
                    apply.count = Mathf.RoundToInt((float)apply.count * multiplyAmount);
                }
            }

            return false;
        }

        public override bool RunPostApplyStatusEvent(StatusEffectApply apply)
        {
            if (target.enabled && !TargetSilenced() && (bool)apply.effectData && apply.count > 0 && CheckType(apply.effectData) && CheckTarget(apply.target))
            {
                return CheckAmount(apply);
            }

            return false;
        }

        public IEnumerator Run(StatusEffectApply apply)
        {
            return Run(GetTargets(), apply.count);
        }

        public bool CheckFlag(ApplyToFlags whenAppliedTo)
        {
            return (whenAppliedToFlags & whenAppliedTo) != 0;
        }

        public bool CheckTarget(Entity entity)
        {
            if (!Battle.IsOnBoard(target))
            {
                return false;
            }

            if (entity == target)
            {
                return CheckFlag(ApplyToFlags.Self);
            }

            if (entity.owner == target.owner)
            {
                return CheckFlag(ApplyToFlags.Allies);
            }

            if (entity.owner != target.owner)
            {
                return CheckFlag(ApplyToFlags.Enemies);
            }

            return false;
        }

        public bool CheckAmount(StatusEffectApply apply)
        {
            if (requiredAmount == 0)
            {
                return true;
            }

            StatusEffectData statusEffectData = apply.target.FindStatus(apply.effectData.type);
            if ((bool)statusEffectData)
            {
                return statusEffectData.count >= requiredAmount;
            }

            return false;
        }
    }
  
 }

















