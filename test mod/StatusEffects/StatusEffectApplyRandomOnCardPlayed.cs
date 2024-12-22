public partial class Goobers
{
    internal class StatusEffectApplyRandomOnCardPlayed : StatusEffectApplyXOnCardPlayed
    {
        public StatusEffectData[] effectsToapply = new StatusEffectData[0];

        public override void Init()
        {
            base.Init();
            Events.OnActionQueued += DetermineEffect;
        }

        private void DetermineEffect(PlayAction arg)
        {
            int r = UnityEngine.Random.Range(0, effectsToapply.Length);
            effectToApply = effectsToapply[r];
        }
    }
}
    













