using UnityEngine;



public partial class Goobers
{
    public class TargetConstraintHealthLessThan : TargetConstraint
    {
        [SerializeField]
        public int value;

        public override bool Check(Entity target)
        {
            if (target.hp.current >= value)
            {
                return not;
            }

            return !not;
        }

        public override bool Check(CardData targetData)
        {
            if (targetData.hp >= value)
            {
                return not;
            }

            return !not;
        }
    }
  
 }

















