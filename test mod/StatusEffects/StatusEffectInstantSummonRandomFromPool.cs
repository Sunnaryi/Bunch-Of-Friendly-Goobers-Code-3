using System.Collections;

internal class StatusEffectInstantSummonRandomFromPool : StatusEffectInstantSummon
    {
        public CardData[] pool;

        public override IEnumerator Process()
        {
            Routine.Clump clump = new();
            var amount = GetAmount();
            for (var i = 0; i < amount; i++)
            {
                if (pool.Length > 0)
                    targetSummon.summonCard = pool.RandomItem();
                clump.Add(TrySummon());
                yield return clump.WaitForEnd();
            }

            yield return Remove();
        }
    }
    













