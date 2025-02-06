using WildfrostHopeMod.VFX;
using WildfrostHopeMod.SFX;
using BattleEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Deadpan.Enums.Engine.Components.Modding;



public partial class Goobers
{


    public class VFXHelper
    {
        public static GIFLoader VFX;
        public static SFXLoader SFX;


        public static Dictionary<string, string> minibossJingles = new Dictionary<string, string>
{
  {"goobers.Koogooloo", "sp"}, //CHANGE

};

        public static void MinibossIntro(Entity target)
        {

            if (minibossJingles.ContainsKey(target.data.name))
            {
                VFXHelper.SFX.TryPlaySound(minibossJingles[target.data.name]);
            }
        }



    }

    public class BattleEdit
    {

           BattleDataEditor bdEditor;
    }


  
   private CardData[] GetCards(params string[] cards)
        {
            return cards.Select(GetCardData).ToArray();
        }

    
    private CardData GetCardData(string name)
    {
        return TryGet<CardData>(name);
    }


    private void RandomSprite(CardData cardData)
    {
        if (cardData.name != "goobers.FAKEChest") // Card name
            return;
        if (randomSprites.Count == 0)
            return;
        cardData.mainSprite = randomSprites.RandomItem();
    }

    private void RandomSprite2(CardData cardData)
    {
        if (cardData.name != "goobers.Chest") // Card name
            return;
        if (randomSprites2.Count == 0)
            return;
        cardData.mainSprite = randomSprites2.RandomItem();
    }

    private void MikuSprites(CardData cardData)
    {
        if (cardData.name != "goobers.Miku") // Card name
            return;
        if (mikuRandom.Count == 0)
            return;
        cardData.mainSprite = mikuRandom.RandomItem();
    }


}
    













