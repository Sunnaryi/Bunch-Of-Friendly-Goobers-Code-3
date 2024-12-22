using System.Collections;



public partial class Goobers
{
    public class StatusEffectMeldXandY : StatusEffectData
    {
        public string statusType1; //Probably snow.

        public string statusType2; //Probably frost.

        public StatusEffectData effectToApply; //Probably Snow.

        public StatusEffectData effectToApply2; //Probably Frost.



        private IEnumerator Run(StatusEffectApply apply)
        {
            if (apply.effectData.type == statusType1)
            {
                return StatusEffectSystem.Apply(apply.target, target, effectToApply2, apply.count); //Apply equal frost
            }
            if (apply.effectData.type == statusType2)
            {
                return StatusEffectSystem.Apply(apply.target, target, effectToApply, apply.count); //Apply equal snow
            }
           
            return null;
        }

        public override bool RunApplyStatusEvent(StatusEffectApply apply) //StatusEffectSystem calls this every time a status is about to be applied.
        {
            if ((target?.enabled != null && apply.applier == target)                                //(1)
                && (apply.effectData?.type == statusType1 || apply.effectData?.type == statusType2)  //(2)
                && !(apply.effectData == effectToApply || apply.effectData == effectToApply2))      //(3)
            {
                return (apply.count > 0);                                 //Call the Run method (if relevant).
            }
            return false;                                                 //Don't call the Run method.
        }
    }
}
    













