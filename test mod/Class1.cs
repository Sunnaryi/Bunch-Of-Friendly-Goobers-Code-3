using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Tutorial6_StatusIcons;
using Unity.Burst.CompilerServices;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.Localization.Tables;
using UnityEngine.Networking.Types;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Extensions = Deadpan.Enums.Engine.Components.Modding.Extensions;
using WildfrostHopeMod.Utils;
using static StatusEffectApplyX;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static StatusEffectBonusDamageEqualToX;
using Newtonsoft.Json.Utilities;
using Rewired;
using static DynamicTutorialSystem;
using static Names;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using System.Threading;
using UnityEngine.Localization.Components;
using NaughtyAttributes;
using BattleEditor;
using UnityEngine.Rendering.Universal;
using WildfrostHopeMod.VFX;
using WildfrostHopeMod.SFX;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using TestMod;
using static System.Net.Mime.MediaTypeNames;
using WildfrostHopeMod;
using WildfrostHopeMod.Configs;
using Unity.Mathematics;
using System.Threading.Tasks;
using static Prompt;
using static Goobers;
using Newtonsoft.Json.Linq;
using System.Runtime.Remoting.Lifetime;
using static Goobers.BattleEdit;
using UnityEngine.Localization;
using DeadExtensions;
using System.Security;
using Frostknights;



public partial class Goobers : WildfrostMod
{


 

    public class Scriptable<T> where T : ScriptableObject, new()
    {
        readonly Action<T> modifier;
        public Scriptable() { }
        public Scriptable(Action<T> modifier) { this.modifier = modifier; }
        public static implicit operator T(Scriptable<T> scriptable)
        {
            T result = ScriptableObject.CreateInstance<T>();
            scriptable.modifier?.Invoke(result);
            return result;
        }
    }

    public class EnemyBattles
    {



        public static List<(int tier, BattleDataEditor bdEditor)> list;



    }

    public static List<string> referenced = new List<string>();
    public static int timePassed = 0;
    public static int timeout = 40;
    public Goobers(string modDirectory) : base(modDirectory)
    {
        Instance = this;
        while (Bootstrap.Mods.Count == 0 && timePassed < timeout)
        {
            Thread.Sleep(250);
            timePassed++;
        }

        /*if (timePassed > 0)
        {
            string s = (timePassed == timeout) ? " (Timed Out)" : "";
            AppendFile($"{DateTime.Now} | {timePassed*250} ms {s}");
        }*/

        foreach (WildfrostMod mod in Bootstrap.Mods)
        {
            referenced.Add(mod.GUID);
        }
    }


    public static Goobers Instance;



    public static List<object> assets = new List<object>();


    public override string GUID => "goobers"; //[creator name].[game name].[mod name] is standard convention. LOWERCASE!

    public override string[] Depends => new string[] { "hope.wildfrost.configs", "hope.wildfrost.vfx", "mhcdc9.wildfrost.battle" }; //The GUID of other mods that your mod requires. This tutorial has none of that.

    public override string Title => "A Bunch of Friendly Goobers!"; //See the 1st in-game image

    public override string Description => "Adds many new companions some balanced, some not balanced at all!"; //See the 1st in-game image

    public static string TribeTitleKey => "goobers" + ".TribeTitle";
    public static string TribeDescKey => "goobers" + ".TribeDesc";


    private List<CardDataBuilder> cards;
    private List<CardUpgradeDataBuilder> cardUpgrades;   //The list of custom CardData(Builder)
    private List<StatusEffectDataBuilder> statusEffects; //The list of custom StatusEffectData(Builder)
    private List<ChallengeDataBuilder> challenges;
    private List<TraitDataBuilder> traitEffects;
    private List<ClassDataBuilder> tribes;
    private List<KeywordDataBuilder> keywords;
    private List<UnlockDataBuilder> unlocklist;
    private List<ChallengeListenerBuilder> listenerlist;
    private List<GameModifierDataBuilder> modifier;

    private List<Sprite> randomSprites = new List<Sprite>();
    private List<Sprite> randomSprites2 = new List<Sprite>();


    private Sprite GetSprite(string name)
    {
        return ImagePath(name).ToSprite();
    }

    public TMP_SpriteAsset assetSprites;
    public override TMP_SpriteAsset SpriteAsset => assetSprites;

    private bool preLoaded = false;
    //Used to prevent redundantly reconstructing our data. Not truly necessary.
    private void CreateModAssets() 
    {
        randomSprites.Add(GetSprite("FAKECHEST.png"));
        randomSprites.Add(GetSprite("FAKECHEST2.png"));
        randomSprites.Add(GetSprite("FAKECHEST3.png")); 
        randomSprites.Add(GetSprite("FAKECHEST4.png"));
        randomSprites.Add(GetSprite("FAKECHEST5.png"));
        randomSprites.Add(GetSprite("FAKECHEST6.png"));

        randomSprites2.Add(GetSprite("REALCHEST2.png"));
        randomSprites2.Add(GetSprite("REALCHEST3.png"));
        randomSprites2.Add(GetSprite("REALCHEST4.png"));
        randomSprites2.Add(GetSprite("REALCHEST5.png"));
        randomSprites2.Add(GetSprite("REALCHEST6.png"));






        assetSprites = HopeUtils.CreateSpriteAsset("Spriteseys", directoryWithPNGs: this.ImagePath("Sprites"), textures: new Texture2D[] { }, sprites: new Sprite[] { });

        VFXHelper.VFX = new GIFLoader(ImagePath("Anim"));
        VFXHelper.VFX.RegisterAllAsApplyEffect();

        VFXHelper.SFX = new SFXLoader(ImagePath("Sounds"));
        VFXHelper.SFX.RegisterAllSoundsToGlobal();




        unlocklist = new List<UnlockDataBuilder>();


        UnlockData u1_data = Get<UnlockData>("Charm 2").InstantiateKeepName();
        u1_data.name = "Ashi Shi Tribe";
        unlocklist.Add(
            u1_data.Edit<UnlockData, UnlockDataBuilder>()
            .WithUnlockTitle("Gratitude!")
            .WithUnlockDescription("Defeat Ashi Shi, The Travelling Witch Merchant!")
            .WithType(UnlockData.Type.Tribe)
      

            );

        listenerlist = new List<ChallengeListenerBuilder>();


        listenerlist.Add(
    new ChallengeListenerBuilder(this)
    .Create("Ashi Kills") //Original Name
    .WithCheckType(ChallengeListener.CheckType.MidRun) //Listens whenever any stat is changed immediately
    .WithStat("Defeat the Ashi Boss") //Looks at the stat with this title (if you want to look at a substat, use WithKey as well)
    );

        challenges = new List<ChallengeDataBuilder>();

        ChallengeData c1_data = Get<ChallengeData>("Challenge Charm 2").InstantiateKeepName(); //Verify that the name is correct
        c1_data.name = "Ashi Challenge"; //Don't want to override Challenge Charm 1
        challenges.Add(
            c1_data.Edit<ChallengeData, ChallengeDataBuilder>()
            .WithGoal(1) //Needs to be performed 10 times overall
            .WithTitle("Witchy Business") //Challenge stone title?
            .WithText("Defeat Ashi Shi, The Travelling Witch Merchant!") //Description
            .SubscribeToAfterAllBuildEvent(
                (data) =>
                {
                    data.listener = Get<ChallengeListener>("Ashi Kills");
                    data.reward = Get<UnlockData>("Ashi Shi Tribe");
                }
            )
        );

        statusEffects = new List<StatusEffectDataBuilder>();



        //Code for status effects When Hit With Junk Add Frenzy To Self
        this.CreateIconKeyword("sp", "Sweet Points", "Special points who benefit from them!| Besides that no other additional function.", "sp"
               , new Color(1f, 1f, 1f), new Color(0.8f, 0.153f, 0.7f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("sp", ImagePath("sp.png").ToSprite(), "sp", "frost", Color.white,shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("sp") })

            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;


        this.CreateIconKeyword("sps", "Terrormisu Sweet Points", "I'm Counting.", "Terroricon"
                 , new Color(1f, 1f, 1f), new Color(1f, 0.0f, 0.0f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("Terroricon", ImagePath("Terroricon.png").ToSprite(), "sps", "frost", Color.white, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("sps") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        this.CreateIconKeyword("fp", "Fuse", "Compatible Fushion", "Fuse"
                 , new Color(1f, 1f, 1f), new Color(1f, 0.0f, 0.0f), new Color(0f, 0f, 0f));

        this.CreateIcon("FP", ImagePath("Terroricon.png").ToSprite(), "fp", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("fp") })
         .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        this.CreateIconKeyword("mp", "Solar", "Solar Trigger", "Solar"
                 , new Color(1f, 1f, 1f), new Color(1f, 0.0f, 0.0f), new Color(0f, 0f, 0f));

        this.CreateIconKeyword("ex", "Expresso", "Temporarily gain <keyword=frenzy>.|Clears after triggering.", "expresso"
            , new Color(1f, 1f, 1f), new Color(1f, 1f, 0f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("expresso", ImagePath("expresso.png").ToSprite(), "ex", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("ex") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        this.CreateIconKeyword("ch", "Waffle", "Gain Barrage temporarly.|Counts down when an enemy is hit.", "waffle"
       , new Color(1f, 1f, 1f), new Color(1f, 1f, 0f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("waffle", ImagePath("waffle.png").ToSprite(), "ch", "frost", Color.white, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("ch") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        this.CreateIconKeyword("kitsu", "Gratitude", "Points specific to Ashi Shi cards.|No special properties.", "kitsup"
, new Color(1f, 1f, 1f), new Color(0.9f, 1f, 0.6f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("kitsup", ImagePath("kitsup.png").ToSprite(), "kitsu", "frost", Color.green, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("kitsu") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;


        this.CreateIconKeyword("fre", "Hyper Freeze", "Stops <keyword=counter>, bypasses snow immunities|Cannot be Cleansed.", "hyperfreeze"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("hyperfreeze", ImagePath("hyperfreeze.png").ToSprite(), "fre", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("fre") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

       

        this.CreateIconKeyword("snowfull", "Snow Full immunity", "Cannot be <keyword=snow>'d.", "imsnow"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("imsnow", ImagePath("imsnow.png").ToSprite(), "snowfull", "frost", Color.black, new KeywordData[] { Get<KeywordData>("snowfull") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("inkfull", "Ink Full immunity", "Cannot be <keyword=null>'d.", "imink"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("imink", ImagePath("imink.png").ToSprite(), "inkfull", "frost", Color.black, new KeywordData[] { Get<KeywordData>("inkfull") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("elu", "Elusive", "Cannot be hit.", "eluicon"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("eluicon", ImagePath("eluicon.png").ToSprite(), "elu", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("elu") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("fortunetag", "Tag of Fortune", "When hit, gain <keyword=blings> equal to damage taken by x13.", "fortunetag"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("fortunetag", ImagePath("fortunetag.png").ToSprite(), "fortunetag", "frost", Color.black, new KeywordData[] { Get<KeywordData>("fortunetag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;


        this.CreateIconKeyword("cake", "Cake", "Hit all enemies temporarly.|Counts down when an enemy is hit..", "cakestat"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("cakestat", ImagePath("cakestat.png").ToSprite(), "cake", "frost", Color.white, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("cake") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        this.CreateIconKeyword("teethtag", "Tag of Bones", "When hit, attacker gains <keyword=teeth> equal to damage taken.|Can only be played on units that are on the board. ", "Teethicontag"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("Teethicontag", ImagePath("Teethicontag.png").ToSprite(), "teethtag", "frost", Color.black, new KeywordData[] { Get<KeywordData>("teethtag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;


        this.CreateIconKeyword("co", "Tag of Connections", "When hit, all allies with <keyword=co> take the same amount of damage.| This targets health so it pierces, but can't harm units who do not have health.", "Connections"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("Connections", ImagePath("Connections.png").ToSprite(), "co", "frost", Color.black, new KeywordData[] { Get<KeywordData>("co") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("snowtag", "Tag of Snow", "After attacking, apply 2 <keyword=snow> to self. |Can only be played on units that are on the board.", "snowtagicon"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("snowtagicon", ImagePath("snowtagicon.png").ToSprite(), "snowtag", "frost", Color.black, new KeywordData[] { Get<KeywordData>("snowtag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;


        this.CreateIconKeyword("healtag", "Tag of Restoration", "Every turn, restore own <keyword=health> by 1 |Can only be played on units that are on the board.", "Healtag"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("Healtag", ImagePath("Healtag.png").ToSprite(), "healtag", "frost", Color.black, new KeywordData[] { Get<KeywordData>("healtag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("suntag", "Tag of Sunshine", "Every turn, count down own <keyword=counter> by 2 |Can only be played on units that are on the board.", "suntagicon"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("suntagicon", ImagePath("suntagicon.png").ToSprite(), "suntag", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("suntag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("wintertag", "Tag of Winters", "Retain <keyword=snow>", "wintericon"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("wintericon", ImagePath("wintericon.png").ToSprite(), "wintertag", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("wintertag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("demontag", "Tag of Demons", "Every turn, apply <keyword=demonize> by 8 to self |Can only be played on units that are on the board.", "demonicon"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("demonicon", ImagePath("demonicon.png").ToSprite(), "demontag", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("demontag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("goblintag", "Tag of Goblings", "Gain Gobling's effect.(When hit drop 4 <keyword=blings>)|Can only be played on units that are on the board.", "Gobling"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("Gobling", ImagePath("Gobling.png").ToSprite(), "goblintag", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("goblintag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("novatag", "Tag of Supernovas", "Can Hit All Enemies| Can only be played on units that are on the board.", "novatag"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("novatag", ImagePath("novatag.png").ToSprite(), "novatag", "frost", Color.black, new KeywordData[] { Get<KeywordData>("novatag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;


        this.CreateIconKeyword("lumintag", "Tag of Lumins", "Increase own effects by 1 every turn. |Can only be played on units that are on the board.", "luminicon"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("luminicon", ImagePath("luminicon.png").ToSprite(), "lumintag", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("lumintag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;



        this.CreateIconKeyword("detotag", "Tag of Detonations", "Gain <keyword=goobers.friendlyexplode> by 2 every turn.|Can only be played on units that are on the board.", "detotag"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("detotag", ImagePath("detotag.png").ToSprite(), "detotag", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("detotag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;


        this.CreateIconKeyword("splittag", "Tag of Mitosis", "Split when 2 <keyword=health> lost.|Can only be played on units that are on the board.", "Splittag"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("Splittag", ImagePath("Splittag.png").ToSprite(), "splittag", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("splittag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;



        this.CreateIconKeyword("bloodsac", "Bloody Ritual", "When destroyed, add <card=goobers.Blood> to your deck, but permanently lose this unit if possible. In addition, if the card has a charm, " +
            "obtain the charm the card had.| This includes Crowns", "bsac"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("bsac", ImagePath("bsac.png").ToSprite(), "bloodsac", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("bloodsac") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("restag", "Tag of Resurrection", "When destroyed, summon a copy to the enemy side with <keyword=health> equal to amount lost indluding over-damage.|Cannot be played on Clunker or Summoned Units.", "resertag"
, new Color(1f, 1f, 1f), new Color(5f, 5f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("resertag", ImagePath("resertag.png").ToSprite(), "restag", "frost", Color.black, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("restag") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIconKeyword("bleed", "Bleeding", "Take damage every turn.| Does not count down.", "Bleedingicon"
, new Color(1f, 1f, 1f), new Color(1f, 1f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("Bleedingicon", ImagePath("Bleedingicon.png").ToSprite(), "bleed", "frost", Color.white, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("bleed") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        this.CreateIconKeyword("fear", "Fear", "When <keyword=fear> reaches 4, flee to the discard pile. If a Leader's <keyword=fear> reaches 12 destroy self.| Clunkers cannot be inflicted with this status.", "fearicon"
, new Color(1f, 1f, 1f), new Color(0.8f, 0f, 0.9f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("fearicon", ImagePath("fearicon.png").ToSprite(), "fear", "frost", Color.white, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("fear") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        this.CreateIconKeyword("fear2", "Fear(for Leaders)", "If <keyword=fear2> reaches 12 destroy self.", "fearicon"
, new Color(1f, 1f, 1f), new Color(0.8f, 0f, 0.9f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("fearicon", ImagePath("fearicon.png").ToSprite(), "fear2", "frost", Color.white, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("fear2") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;


        this.CreateIconKeyword("feartrans", "Fear", "", "fearicon"
, new Color(1f, 1f, 1f), new Color(1f, 1f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("fearicon", ImagePath("fearicon.png").ToSprite(), "fear2", "frost", Color.white, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("fear2") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;


        this.CreateIconKeyword("use", "Uses", "When <keyword=use> run out, destroy this card.", "useicon"
, new Color(1f, 1f, 1f), new Color(1f, 1f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("useicon", ImagePath("useicon.png").ToSprite(), "use", "frost", Color.white, shadowColor: new Color(0, 0, 0), new KeywordData[] { Get<KeywordData>("use") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;



        this.CreateIconKeyword("decaying", "Decaying", "Lose <keyword=decaying> every turn, when this card loses all <keyword=decaying>, destroy this card.", "WITHER"
, new Color(1f, 1f, 1f), new Color(1f, 1f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("WITHER", ImagePath("WITHER.png").ToSprite(), "decaying", "frost", Color.white, new Color(0f, 0f, 0f), new KeywordData[] { Get<KeywordData>("decaying") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;



        this.CreateIconKeyword("sap", "Sapphire", "A blue stone generating power", "useicon"
, new Color(1f, 1f, 1f), new Color(1f, 1f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("useicon", ImagePath("useicon.png").ToSprite(), "sap", "frost", Color.white, shadowColor: new Color(0f, 0f, 0f), new KeywordData[] { Get<KeywordData>("sap") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;


        this.CreateIconKeyword("hina", "Hina's Blessing", "Temporary double <keyword=attack>", "useicon"
, new Color(1f, 1f, 1f), new Color(1f, 1f, 1f), new Color(0f, 0f, 0f));

        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("useicon", ImagePath("useicon.png").ToSprite(), "hina", "frost", Color.white, shadowColor: new Color(0f, 0f, 0f), new KeywordData[] { Get<KeywordData>("hina") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;









        //ENEMY EFFECTS

        statusEffects.Add(
 StatusCopy("Summon Fallow", "Summon Curser")
//Makes a copy of the Summon Fallow effect
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
{
    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("Baby Horns"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                            //This is because TryGet will try to prefix the name with your GUID. 
})                                                                          //If that fails, then it uses no GUID-prefixing.
 );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon Curser") //Copying Instant Summon Fallow and changing the name.
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon Curser");
               ((StatusEffectInstantSummon)data).canSummonMultiple = true;
           
           })
         );

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXOnTurn>("Summon Cursers on kill")
        .WithText("Summon <card=goobers.Baby Horns>.")
        .WithStackable(true)
        .WithCanBeBoosted(false)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXOnTurn;

            realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Curser");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;


        }
        ));



        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenHit>("When hit with item destroy card")
          .WithText("If hit with an item, destroy random card in the enemy's hand.")
          .WithCanBeBoosted(false)
          .WithTextInsert("")
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenHit;

               realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Card In Hand");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomCardInHand;
               realData.targetMustBeAlive = false;
               realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsItem()

                    };
           }
               ));


        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenCardDestroyed>("Trigger Self when card destroyed")
          .WithText("Trigger self when any unit is destroyed.")
          .WithStackable(false)
          .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenCardDestroyed;

               realData.effectToApply = TryGet<StatusEffectData>("Trigger");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.targetMustBeAlive = false;

           }
               ));

        statusEffects.Add(
   new StatusEffectDataBuilder(this)
       .Create<StatusEffectApplyXWhenAllyIsKilled>("Trigger Self when ally killed")
       .WithStackable(false)
       .WithCanBeBoosted(false)
       .WithIsReaction(true)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXWhenAllyIsKilled;

            realData.effectToApply = TryGet<StatusEffectData>("Trigger");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            realData.targetMustBeAlive = false;


        }
            ));


        statusEffects.Add(
StatusCopy("When Card Destroyed, Gain Attack", "Increase all ally's when card destroyed")
.WithText("When any card is destroyed, apply <{a}> <keyword=spice> to all allies")                                       //Since this effect is on Shade Serpent, we modify the description shown.
.WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
{
    ((StatusEffectApplyXWhenCardDestroyed)data).effectToApply = TryGet<StatusEffectData>("Spice");
    ((StatusEffectApplyXWhenCardDestroyed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;

})                                                                              //If that fails, then it uses no GUID-prefixing.
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXPreTurn>("Before Attack, Demonize Targets")
    .WithText("Before attacking, apply <{a}> <keyword=demonize> to enemies in the row")
    .WithCanBeBoosted(false)
    .WithTextInsert("")
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXPreTurn;

         realData.effectToApply = TryGet<StatusEffectData>("Demonize");
         realData.applyToFlags = ApplyToFlags.EnemiesInRow;

     }));










        //OTHERS
        statusEffects.Add(StatusCopy("When Spice Or Shell Applied To Self Shroom To RandomEnemy", "Owntempo")

.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
   var se = data as StatusEffectApplyXWhenYAppliedTo;
   se.whenAppliedTypes = new string[] { "frenzy" };
   se.whenAppliedToFlags = StatusEffectApplyX.ApplyToFlags.Self;
   se.effectToApply = null;
   se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
   se.instead = true;
   se.targetMustBeAlive = false;
    se.textKey = null;
}));


        statusEffects.Add(StatusCopy("When Spice Or Shell Applied To Self Shroom To RandomEnemy", "FullImmuneToSnow")
.WithIconGroupName("counter")
   .WithVisible(true)
    .WithIsStatus(true)
     .WithOffensive(false)
    .WithType("snowfull")
    .WithKeyword("snowfull")
   .WithStackable(false)
   .WithCanBeBoosted(false)
   .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
   {
       var se = data as StatusEffectApplyXWhenYAppliedTo;

       se.whenAppliedTypes = new string[] { "snow" };
       se.whenAppliedToFlags = StatusEffectApplyX.ApplyToFlags.Self;
       se.effectToApply = null;
       se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
       se.instead = true;
       se.targetMustBeAlive = false;
   }));


        statusEffects.Add(StatusCopy("When Spice Or Shell Applied To Self Shroom To RandomEnemy", "FullImmuneToInk")
            .WithText("Apply <keyword=inkfull>")
.WithIconGroupName("counter")
    .WithVisible(true)
    .WithIsStatus(true)
    .WithOffensive(false)
    .WithType("inkfull")
    .WithKeyword("inkfull")
    .WithStackable(false)
    .WithCanBeBoosted(false)
    .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
    {
        var se = data as StatusEffectApplyXWhenYAppliedTo;
        se.whenAppliedTypes = new string[] { "ink" };
        se.whenAppliedToFlags = StatusEffectApplyX.ApplyToFlags.Self;
        se.effectToApply = null;
        se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
        se.instead = true;
        se.targetMustBeAlive = false;
    }));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCallaFriend>("Tag time")
.WithCanBeBoosted(false)

);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Tagbox choose")
.WithText("Choose your special tag.")
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.effectToApply = TryGet<StatusEffectData>("Tag time");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.eventPriority = 1000;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantShop>("Shoppers")
.WithText("<keyword=goobers.drawa> <{a}>")
.WithCanBeBoosted(true)

);
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Shop time")
.WithText("Shop Time")
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXOnTurn;

    realData.effectToApply = TryGet<StatusEffectData>("Shoppers");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

})
);

        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantTutor>("Deck Finder")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectInstantTutor;


  realData.eventPriority = 5;
  realData.source = StatusEffectInstantTutor.CardSource.Draw;
  realData.title = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English).GetString("Goobers.TutorACard");

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantTutor>("Discard Finder")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectInstantTutor;


    realData.eventPriority = 5;
    realData.source = StatusEffectInstantTutor.CardSource.Discard;
    realData.title = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English).GetString("Goobers.TutorACard");

})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantTutor>("Custom Finder")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectInstantTutor;


realData.eventPriority = 5;
realData.source = StatusEffectInstantTutor.CardSource.Custom;
realData.title = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English).GetString("Goobers.TutorACard");
realData.amount = 4;
realData.summonCopy = TryGet<StatusEffectInstantSummon>("Instant Summon Junk In Hand");
realData.customCardList = ["BerryS", "SugaryS", "OddS", "BloodS"];


})
);



        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //CARD FINDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------




        statusEffects.Add(new StatusEffectDataBuilder(this)

    .Create<StatusEffectEXP>("SAP")
    .WithIconGroupName("damage")
    .WithText("Apply <{a}> <keyword=sap>")
    .WithVisible(true)
    .WithIsStatus(true)
    .WithStackable(true)
    .WithOffensive(false)
    .WithTextInsert("{a}")
    .WithKeyword("sap")
    .WithType("sap")


    );
        statusEffects.Add(new StatusEffectDataBuilder(this)

       .Create<StatusEffectEthereal>("Decay")
       .WithIconGroupName("damage")
       .WithText("Apply <{a}> <keyword=decaying>")
       .WithVisible(true)
       .WithIsStatus(true)
       .WithStackable(true)
       .WithOffensive(false)
       .WithTextInsert("{a}")
       .WithKeyword("decaying")
       .WithType("decaying")


       );



        statusEffects.Add(new StatusEffectDataBuilder(this)

            .Create<StatusEffectEXP>("EXP")
            .WithIconGroupName("counter")
            .WithText("Apply <{a}> <keyword=sp>")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithStackable(true)
            .WithOffensive(false)
            .WithTextInsert("{a}")
            .WithKeyword("sp")
            .WithType("sp")


            );

        statusEffects.Add(new StatusEffectDataBuilder(this)

           .Create<StatusEffectStealth>("ELU")
           .WithIconGroupName("health")
           .WithVisible(true)
           .WithIsStatus(true)
           .WithStackable(true)
           .WithOffensive(false)
           .WithTextInsert("{a}")
           .WithType("elu")
           .WithKeyword("elu")


           );
    


        statusEffects.Add(new StatusEffectDataBuilder(this)

            .Create<StatusEffectKitsu>("Shi")
            .WithIconGroupName("counter")
            .WithText("Apply <{a}> <keyword=kitsu>")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithStackable(true)
            .WithOffensive(false)
            .WithTextInsert("{a}")
            .WithKeyword("kitsu")
            .WithType("kitsu")


            );
        

        statusEffects.Add(new StatusEffectDataBuilder(this)

         .Create<StatusEffectAllOthers>("Choco")
         .WithIconGroupName("damage")
         .WithText("Apply <{a}> <keyword=ch>")
         .WithVisible(true)
         .WithIsStatus(true)
         .WithStackable(true)
         .WithOffensive(false)
         .WithTextInsert("{a}")
         .WithKeyword("ch")
         .WithType("ch")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                var realData = data as StatusEffectWhileActiveX;
                realData.effectToApply = Get<StatusEffectData>("Row no silence");
                realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                realData.targetMustBeAlive = false;
                     var script = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
                     script.statusType = "ch";
                     ((StatusEffectWhileActiveX)data).scriptableAmount = script;
                 }

        ));


        statusEffects.Add(new StatusEffectDataBuilder(this)

        .Create<StatusEffectAllOthers>("Cake")
        .WithIconGroupName("damage")
        .WithText("Apply <{a}> <keyword=cake>")
        .WithVisible(true)
        .WithIsStatus(true)
        .WithStackable(true)
        .WithOffensive(false)
        .WithTextInsert("{a}")
        .WithKeyword("cake")
        .WithType("cake")

           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    var realData = data as StatusEffectWhileActiveX;
                    realData.effectToApply = Get<StatusEffectData>("All ene");
                    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    realData.targetMustBeAlive = false;
                    var script = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
                    script.statusType = "cake";
                    ((StatusEffectWhileActiveX)data).scriptableAmount = script;
                }

       ));

   statusEffects.Add(StatusCopy("Hit All Enemies", "No text Hit all")
   .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
   {
       var se = data as StatusEffectChangeTargetMode;
       se.textKey = null;
   }));
        statusEffects.Add(new StatusEffectDataBuilder(this)

           .Create<StatusEffectApplyXOnTurn>("Commander")
           .WithText("Trigger all <keyword=goobers.await>")
           .WithStackable(false)
           .WithCanBeBoosted(false)
             .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
             {
                 var realData = data as StatusEffectApplyXOnTurn;

         
                 realData.effectToApply = Get<StatusEffectData>("Trigger (High Prio)");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                 realData.targetMustBeAlive = false;
                 realData.applyConstraints = new[]
    {

                        new TargetConstraintHasTrait()
                        {
                            trait = Get<TraitData>("Await")

                        }
            };

             
             }


          ));

        statusEffects.Add(new StatusEffectDataBuilder(this)

           .Create<StatusEffectMania>("Hina Blessing")
           .WithIconGroupName("damage")
           .WithText("Apply <{a}> <keyword=hina>")
           .WithVisible(true)
           .WithIsStatus(true)
           .WithStackable(true)
           .WithOffensive(false)
           .WithTextInsert("{a}")
           .WithKeyword("hina")
           .WithType("hina")
              .FreeModify<StatusEffectWhileActiveX>(
                   delegate (StatusEffectWhileActiveX data)
                   {
                       data.applyEqualAmount = true;
                       data.effectToApply = Get<StatusEffectData>("Ongoing Increase Attack");
                       data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                       data.targetMustBeAlive = false;
                       var script = ScriptableObject.CreateInstance<ScriptableCurrentAttack>();
                       ((StatusEffectWhileActiveX)data).scriptableAmount = script;
                   }

          ));
        

        statusEffects.Add(new StatusEffectDataBuilder(this)

            .Create<StatusEffectMania>("Expresso")
            .WithIconGroupName("damage")
            .WithText("Apply <{a}> <keyword=ex>")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithStackable(true)
            .WithOffensive(false)
            .WithTextInsert("{a}")
            .WithKeyword("ex")
            .WithType("ex")
               .FreeModify<StatusEffectWhileActiveX>(
                    delegate (StatusEffectWhileActiveX data)
                    {
                        data.TargetSilenced();
                        data.applyEqualAmount = true;
                        data.effectToApply = Get<StatusEffectData>("MultiHit");
                        data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                        data.targetMustBeAlive = false;
                        var script = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
                        script.statusType = "ex";
                        ((StatusEffectWhileActiveX)data).scriptableAmount = script;
                    }

           ));


        statusEffects.Add(new StatusEffectDataBuilder(this)
              .Create<StatusEffectInstantLoseX>("LEXP")
        .WithIconGroupName("Reduce <keyword=sp> <{a}>")
        .WithCanBeBoosted(false)
        .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
        {
            var realData = data as StatusEffectInstantLoseX;

            realData.statusToLose = TryGet<StatusEffectData>("EXP");
        }
        )
        );


        statusEffects.Add(new StatusEffectDataBuilder(this)
           .Create<StatusEffectInstantLoseX>("LEXPT")
     .WithIconGroupName("Reduce <keyword=sps> <{a}>")
     .WithText("Reduce <keyword=sps> <{a}>")
     .WithCanBeBoosted(false)
     .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
     {
         var realData = data as StatusEffectInstantLoseX;

         realData.statusToLose = TryGet<StatusEffectData>("EXPT");
     }
     )
     );
        statusEffects.Add(new StatusEffectDataBuilder(this)

   .Create<StatusEffectInstantLoseX>("Lose Multi")
   .WithIconGroupName("Reduce <keyword=sp> <{a}>")
   .WithCanBeBoosted(false)
   .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
   {
       var realData = data as StatusEffectInstantLoseX;

       realData.statusToLose = TryGet<StatusEffectData>("MultiHit");
   }
   )
   );

        statusEffects.Add(new StatusEffectDataBuilder(this)

                   .Create<StatusEffectInstantLoseHealth>("Lose Health")


                   );





        //TAGS!-------------------------------------------------------------------------
        //TAGS!-------------------------------------------------------------------------
        //TAGS!-------------------------------------------------------------------------
        //TAGS!-------------------------------------------------------------------------
        //TAGS!-------------------------------------------------------------------------
        //TAGS!-------------------------------------------------------------------------

        statusEffects.Add(new StatusEffectDataBuilder(this)
         .Create<StatusEffectConnect>("CON")
         .WithText("Apply <keyword=co>")
                .WithIconGroupName("crown")
                .WithVisible(true)
                .WithIsStatus(true)
                .WithStackable(false)
                .WithKeyword("co")
                .WithType("co")
               .SubscribeToAfterAllBuildEvent(data =>
                 {
                     var realData = data as StatusEffectApplyXWhenHit;
                     {
                         realData.applyEqualAmount = true;
                         realData.dealDamage = true;
                         realData.doesDamage = true;
                         realData.countsAsHit = false;
                         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                         realData.targetMustBeAlive = false;
                         realData.applyConstraints = new TargetConstraint[]
                         {
        new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("CON")
        }


     };
                     }
                 }));



        statusEffects.Add(new StatusEffectDataBuilder(this)
       .Create<StatusEffectConnect>("FortuneTag")
       .WithText("Apply <keyword=fortunetag>")
              .WithIconGroupName("crown")
              .WithVisible(true)
              .WithIsStatus(true)
              .WithStackable(true)
              .WithOffensive(false)
              .WithKeyword("fortunetag")
              .WithType("fortunetag")
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenHit;
                 {
                     realData.applyEqualAmount = true;
                     realData.effectToApply = Get<StatusEffectData>("Gain Gold");
                     realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                     realData.targetMustBeAlive = false;
                     realData.equalAmountBonusMult = 12;

                 }
             }));


        statusEffects.Add(new StatusEffectDataBuilder(this)
       .Create<StatusEffectConnect>("TeethTag")
       .WithText("Apply <keyword=teethtag>")
              .WithIconGroupName("crown")
              .WithVisible(true)
              .WithIsStatus(true)
              .WithStackable(true)
              .WithOffensive(false)
              .WithKeyword("teethtag")
              .WithType("teethtag")
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenHit;
                 {
                     realData.applyEqualAmount = true;
                     realData.effectToApply = Get<StatusEffectData>("Teeth");
                     realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Attacker;
                     realData.targetMustBeAlive = false;

                 }
             }));
        
        statusEffects.Add(new StatusEffectDataBuilder(this)
   .Create<StatusEffectApplyXEveryTurn>("SunTag")
   .WithText("Apply <keyword=suntag>")
          .WithIconGroupName("crown")
          .WithVisible(true)
          .WithIsStatus(true)
          .WithStackable(true)
          .WithOffensive(false)
          .WithKeyword("suntag")
          .WithType("suntag")
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXEveryTurn;
             {
                 realData.effectToApply = Get<StatusEffectData>("Reduce Counter");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.targetMustBeAlive = false;
                 realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                 var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                 script.amount = 2;
                 ((StatusEffectApplyXEveryTurn)data).scriptableAmount = script;
             }
         })); 

                    statusEffects.Add(new StatusEffectDataBuilder(this)
   .Create<StatusEffectApplyXEveryTurn>("HealTag")
   .WithText("Apply <keyword=healtag>")
          .WithIconGroupName("crown")
          .WithVisible(true)
          .WithIsStatus(true)
          .WithStackable(true)
          .WithOffensive(false)
          .WithKeyword("healtag")
          .WithType("healtag")
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXEveryTurn;
             {
                 realData.effectToApply = Get<StatusEffectData>("Heal");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.targetMustBeAlive = false;
             }
         })); 

                                statusEffects.Add(new StatusEffectDataBuilder(this)
   .Create<StatusEffectApplyXEveryTurn>("DetoTag")
   .WithText("Apply <keyword=detotag>")
          .WithIconGroupName("crown")
          .WithVisible(true)
          .WithIsStatus(true)
          .WithStackable(true)
          .WithOffensive(false)
          .WithKeyword("detotag")
          .WithType("detotag")
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXEveryTurn;
             {
                 realData.effectToApply = Get<StatusEffectData>("Temporary Friendplode");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.targetMustBeAlive = false;

                 realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                 var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                 script.amount = 2;
                 ((StatusEffectApplyXEveryTurn)data).scriptableAmount = script;
             }
         })); 

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectHaltX>("HaltSnow")

.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectHaltX;
 {
     realData.effectToHalt = Get<StatusEffectData>("Snow");

 }
}));

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("WinterTag")
.WithText("Apply <keyword=wintertag>")
      .WithIconGroupName("crown")
      .WithVisible(true)
      .WithIsStatus(true)
      .WithStackable(true)
      .WithOffensive(false)
      .WithKeyword("wintertag")
      .WithType("wintertag")
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectWhileActiveX;
         {
             realData.effectToApply = Get<StatusEffectData>("HaltSnow");
             realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
             realData.targetMustBeAlive = false;
   
         }
     }));

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXEveryTurn>("DemonTag")
.WithText("Apply <keyword=demontag>")
     .WithIconGroupName("crown")
     .WithVisible(true)
     .WithIsStatus(true)
     .WithStackable(true)
     .WithOffensive(false)
     .WithKeyword("demontag")
     .WithType("demontag")
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var realData = data as StatusEffectApplyXEveryTurn;
        {
            realData.effectToApply = Get<StatusEffectData>("Demonize");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            realData.targetMustBeAlive = false;
            realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
            var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
            script.amount = 8;
            ((StatusEffectApplyXEveryTurn)data).scriptableAmount = script;
        }
    }));

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHit>("GoblingTag")
.WithText("Apply <keyword=goblintag>")
 .WithIconGroupName("crown")
 .WithVisible(true)
 .WithIsStatus(true)
 .WithStackable(true)
 .WithOffensive(false)
 .WithKeyword("goblintag")
 .WithType("goblintag")
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenHit;
    {
        realData.effectToApply = Get<StatusEffectData>("Gain Gold");
        realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
        realData.targetMustBeAlive = false;
        realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
        var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
        script.amount = 4;
        ((StatusEffectApplyXWhenHit)data).scriptableAmount = script;
    }
}));



        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXPostAttack>("SnowTag")
.WithText("Apply <keyword=snowtag>")
   .WithIconGroupName("crown")
   .WithVisible(true)
   .WithIsStatus(true)
   .WithStackable(true)
   .WithOffensive(false)
   .WithKeyword("snowtag")
   .WithType("snowtag")
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectApplyXPostAttack;
      {
          realData.effectToApply = Get<StatusEffectData>("Snow");
          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
          realData.targetMustBeAlive = false;
          realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
          var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
          script.amount = 2;
          ((StatusEffectApplyXPostAttack)data).scriptableAmount = script;
      }
  }));

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("NovaTag")
.WithText("Apply <keyword=novatag>")
 .WithIconGroupName("crown")
 .WithVisible(true)
 .WithIsStatus(true)
 .WithStackable(true)
 .WithOffensive(false)
 .WithKeyword("novatag")
 .WithType("novatag")
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectWhileActiveX;
    {
        realData.effectToApply = Get<StatusEffectData>("Hit All Enemies");
        realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
        realData.targetMustBeAlive = false;

    }
}));
        statusEffects.Add(new StatusEffectDataBuilder(this)
   .Create<StatusEffectApplyXEveryTurn>("LuminTag")
   .WithText("Apply <keyword=lumintag>")
          .WithIconGroupName("crown")
          .WithVisible(true)
          .WithIsStatus(true)
          .WithStackable(true)
          .WithOffensive(false)
          .WithKeyword("lumintag")
          .WithType("lumintag")
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXEveryTurn;
             {
                 realData.effectToApply = Get<StatusEffectData>("Increase Effects");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.targetMustBeAlive = false;
                 realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                 var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                 script.amount = 1;
                 ((StatusEffectApplyXEveryTurn)data).scriptableAmount = script;
             }
         }));



        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("Splittag")
.WithText("Apply <keyword=splittag>")
.WithIconGroupName("crown")
.WithVisible(true)
.WithIsStatus(true)
.WithStackable(true)
.WithOffensive(false)
.WithKeyword("splittag")
.WithType("splittag")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectWhileActiveX;
{
realData.effectToApply = Get<StatusEffectData>("When X Health Lost Split text none");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.targetMustBeAlive = false;
        realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
        var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
        script.amount = 2;
        ((StatusEffectWhileActiveX)data).scriptableAmount = script;

    }
}));

        statusEffects.Add(
 StatusCopy("When X Health Lost Split", "When X Health Lost Split text none")
 .WithCanBeBoosted(false)
 .WithStackable(false)                                      //Makes a copy of the Summon Fallow effect
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
{
    ((StatusEffectApplyXWhenHealthLost)data).textKey=null; //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too


})                                                                              //If that fails, then it uses no GUID-prefixing.
 );


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDestroyed>("Restag")
.WithText("Apply <keyword=restag>")
.WithIconGroupName("crown")
.WithVisible(true)
.WithIsStatus(true)
.WithStackable(true)
.WithOffensive(false)
.WithKeyword("restag")
.WithType("restag")

.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenDestroyed;
 {
     realData.effectToApply = Get<StatusEffectData>("Instant Summon Copy On Other Side With X Health");
     realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
     realData.targetMustBeAlive = false;
     realData.affectedBySnow = false;
     realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableHealthLost>();
     var script = ScriptableObject.CreateInstance<ScriptableHealthLost>();

    ((StatusEffectApplyXWhenDestroyed)data).scriptableAmount = script;
        realData.applyConstraints = new TargetConstraint[]
      {
        new TargetConstraintIsCardType()
        {
          allowedTypes= new[] { TryGet<CardType>("Friendly"), TryGet<CardType>("Enemy"), TryGet<CardType>("Miniboss"), TryGet<CardType>("BossSmall")}
        }
      };
        realData.targetConstraints = new TargetConstraint[]
{
        new TargetConstraintIsCardType()
        {
          allowedTypes= new[] { TryGet<CardType>("Friendly"), TryGet<CardType>("Enemy"), TryGet<CardType>("Miniboss"), TryGet<CardType>("BossSmall")}
        }
       
};

    }
}));


        //END OF TAGS-------------------------------------
        //END OF TAGS-------------------------------------
        //END OF TAGS-------------------------------------
        //END OF TAGS-------------------------------------
        //END OF TAGS-------------------------------------

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("BloodSac applier")
.WithText("While active, all companions and bosses gain <keyword=bloodsac>.")
.WithIconGroupName("crown")
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectWhileActiveX;
    {
        realData.effectToApply = Get<StatusEffectData>("BloodSac");
        realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies|ApplyToFlags.Allies;
        realData.targetMustBeAlive = false;
        realData.applyConstraints = new TargetConstraint[]
      {
        new TargetConstraintIsCardType()
        {
          allowedTypes= new[] { TryGet<CardType>("Friendly"), TryGet<CardType>("Miniboss"), TryGet<CardType>("BossSmall"), TryGet<CardType>("Boss") }
        }
      };
    }
}
          ));

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDestroyed>("BloodSac")
.WithText("Apply <keyword=bloodsac>")
.WithIconGroupName("crown")
.WithVisible(true)
.WithIsStatus(true)
.WithStackable(true)
.WithOffensive(false)
.WithKeyword("bloodsac")
.WithType("bloodsac")
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenDestroyed;
 {
     realData.effectToApply = Get<StatusEffectData>("Gain Blood");
     realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
     realData.targetMustBeAlive = false;
     realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
     var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
     script.amount = 1;
     ((StatusEffectApplyX)data).scriptableAmount = script;

 }
}));



        statusEffects.Add(new StatusEffectDataBuilder(this)

           .Create<StatusEffectEXP>("EXPT")
           .WithIconGroupName("health")
           .WithVisible(true)
           .WithIsStatus(true)
           .WithStackable(true)
           .WithOffensive(false)
           .WithTextInsert("{a}")
           .WithKeyword("sps")
           .WithType("sps")


           );

        statusEffects.Add(new StatusEffectDataBuilder(this)

          .Create<StatusEffectEXP>("FP")
          .WithIconGroupName("health")
          .WithVisible(true)
          .WithIsStatus(true)
          .WithStackable(true)
          .WithOffensive(false)
          .WithTextInsert("FUSION")
          .WithKeyword("fp")
          .WithType("fp")

          );

        statusEffects.Add(new StatusEffectDataBuilder(this)

        .Create<StatusEffectEXP>("MP")
        .WithIconGroupName("health")
        .WithVisible(false)
        .WithIsStatus(true)
        .WithStackable(true)
        .WithOffensive(false)
        .WithTextInsert("Solar")
        .WithKeyword("mp")
        .WithType("mp")

        );

        statusEffects.Add(
StatusCopy("Snow", "Freezed")
      .WithIconGroupName("counter")
         .WithText("Apply <{a}> <keyword=fre>")
         .WithVisible(true)
         .WithIsStatus(true)
         .WithStackable(true)
         .WithOffensive(false)
         .WithTextInsert("{a}")
         .WithKeyword("fre")
         .WithType("fre")
);
        statusEffects.Add(
   StatusCopy("When Spice X Applied To Self Trigger To Self", "When Spice X Applied To Self Trigger To Self2")
   .WithText($"<keyword={Extensions.PrefixGUID("special", this)}>")
   .WithCanBeBoosted(false)
   .WithStackable(true)                                      //Makes a copy of the Summon Fallow effect
  .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
  {
      ((StatusEffectApplyXWhenYAppliedTo)data).effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
      ((StatusEffectApplyXWhenYAppliedTo)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      ((StatusEffectApplyXWhenYAppliedTo)data).scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
      var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
      script.amount = 1;
      ((StatusEffectApplyX)data).scriptableAmount = script;

  })                                                                              //If that fails, then it uses no GUID-prefixing.
   );

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedTo>("When Spice X Applied To Apply bom to all enemies")
.WithCanBeBoosted(false)
.WithStackable(true)                                      //Makes a copy of the Summon Fallow effect
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
{
    var realData = data as StatusEffectApplyXWhenYAppliedTo;

    realData.effectToApply = TryGet<StatusEffectData>("Weakness"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
    realData.whenAppliedTypes = new string[1] { "spice" };
    realData.whenAppliedToFlags = ApplyToFlags.Self;
    realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
    var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
    script.amount = 7;
    ((StatusEffectApplyX)data).scriptableAmount = script;

})                                                                              //If that fails, then it uses no GUID-prefixing.
);
        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXOnCardPlayed>("Fushion Activate")
          .WithText("")
          .WithStackable(true)
          .WithCanBeBoosted(true)
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectApplyXOnCardPlayed;

              realData.effectToApply = TryGet<StatusEffectData>("FP");
              realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
              realData.applyConstraints = new TargetConstraint[]
     {
        new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("FP")
        }
     };
          }
          ));
        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantRemoveFromDeck>("Remove")
          .WithText("")
          .WithStackable(true)
          .WithCanBeBoosted(true)

          );
        statusEffects.Add(
       new StatusEffectDataBuilder(this)
       .Create<StatusEffectApplyXOnCardPlayed>("Remove Self on played")
       .WithText("<keyword=goobers.oneshot>")
       .WithStackable(true)
       .WithCanBeBoosted(false)
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectApplyXOnCardPlayed;

           realData.effectToApply = TryGet<StatusEffectData>("Remove");
           realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
       }


           ));

        statusEffects.Add(
  new StatusEffectDataBuilder(this)
  .Create<StatusEffectApplyXOnTurn>("Remove Self")
  .WithText("<keyword=goobers.oneshot>")
  .WithStackable(true)
  .WithCanBeBoosted(false)
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectApplyXOnTurn;

      realData.effectToApply = TryGet<StatusEffectData>("Remove");
      realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  }


      ));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnCardPlayed>("Remove Self after use")
.WithText("<keyword=goobers.oneshot>")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXOnCardPlayed;

    realData.effectToApply = TryGet<StatusEffectData>("Remove");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.targetMustBeAlive = false;
}


   ));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
   .Create<StatusEffectInstantMultiple>("Destroy and Delete")
   .WithText("")
   .WithCanBeBoosted(false)
   .WithTextInsert("<card=goobers.Sharoco2>")
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var realData = data as StatusEffectInstantMultiple;

        realData.effects = new StatusEffectInstant[]
    {
     TryGet<StatusEffectInstant>("Sacrifice Ally"),
     TryGet<StatusEffectInstant>("Remove")
    };
    })
    );
        statusEffects.Add(
      new StatusEffectDataBuilder(this)
      .Create<StatusEffectInstantCombineCard>("Solar Combo")
      .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
      {
          ((StatusEffectInstantCombineCard)data).cardNames = new string[3] { "goobers.Sunscreen", "goobers.Sunray", "goobers.Sunburn" };
          ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Solar";
          ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
          ((StatusEffectInstantCombineCard)data).changeDeck = true;
          ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
          ((StatusEffectInstantCombineCard)data).checkBoard = true;
          ((StatusEffectInstantCombineCard)data).checkDeck = true;
          ((StatusEffectInstantCombineCard)data).checkHand = true;
      })
      );
        statusEffects.Add(
         new StatusEffectDataBuilder(this)
         .Create<StatusEffectApplyXWhenDeployed>("Lets go")

         .WithStackable(true)
         .WithCanBeBoosted(false)
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXWhenDeployed;

             realData.effectToApply = TryGet<StatusEffectData>("Solar Combo");
             realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;


         }

         ));
        statusEffects.Add(
       new StatusEffectDataBuilder(this)
       .Create<StatusEffectApplyXWhenDeployed>("Destroy and Delete Sunrise")

       .WithStackable(true)
       .WithCanBeBoosted(false)
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectApplyXWhenDeployed;

           realData.effectToApply = TryGet<StatusEffectData>("Destroy and Delete");
           realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
           realData.applyConstraints = new TargetConstraint[]
  {
        new TargetConstraintIsSpecificCard()
        {
            allowedCards = new CardData[]
            {
                TryGet<CardData>("Sunburn")
            }
        }
  };
       }

       ));

        statusEffects.Add(
           new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXOnCardPlayed>("Fuse Activator")
           .WithText("When used on an Anchor, with fuse materials avalible on the board. Fuse those cards.")
           .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXOnCardPlayed;

               realData.effectToApply = TryGet<StatusEffectData>("FP");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
           }

           ));
        statusEffects.Add(
   StatusCopy("While Active Teeth To Allies", "While Active MP to Sunray")
   .WithText("")
   .WithStackable(true)                                      //Makes a copy of the Summon Fallow effect
  .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
  {
      ((StatusEffectWhileActiveX)data).effectToApply = TryGet<StatusEffectData>("MP"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
      ((StatusEffectWhileActiveX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
      ((StatusEffectWhileActiveX)data).applyConstraints = new TargetConstraint[]
      {
        new TargetConstraintIsSpecificCard()
        {
            allowedCards = new CardData[]
            {
                TryGet<CardData>("Sunray")
            }
        }
      };
  })                                                                              //If that fails, then it uses no GUID-prefixing.
   );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Solar")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Solar");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Solar")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Solar") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Solar");
                      })
                      );

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenYAppliedTo>("Anchor Sunray")
            .WithText($"<keyword={Extensions.PrefixGUID("special", this)}>")
            .WithCanBeBoosted(false)
            .WithTextInsert("")
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenYAppliedTo;

                 realData.effectToApply = TryGet<StatusEffectData>("Make Solar");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.mustReachAmount = true;
                 realData.count = 1;
                 realData.targetMustBeAlive = false;
                 realData.whenAppliedTypes = new string[1] { "mp" };
                 realData.whenAppliedToFlags = ApplyToFlags.Self;

                 var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                 script.amount = 1;
                 ((StatusEffectApplyX)data).scriptableAmount = script;
             }));

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantReplaceInDeck>("Replace with Solar")
          .WithText("")
          .WithCanBeBoosted(false)
          .WithTextInsert("")
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectInstantReplaceInDeck;

               realData.replaceWith = TryGet<CardData>("Solar");
           }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectInstantMultiple>("Make Solar")
    .WithText("")
    .WithCanBeBoosted(false)
    .WithTextInsert("")
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectInstantMultiple;

         realData.effects = new StatusEffectInstant[]
     {
     TryGet<StatusEffectInstant>("Sacrifice Ally"),
     TryGet<StatusEffectInstant>("Instant Summon Solar"),
      TryGet<StatusEffectInstant>("Replace with Solar")
     };
     })
     );

        statusEffects.Add(
         StatusCopy("Summon Fallow", "Summon GoopFlies")
        .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
        .WithTextInsert("<card=goobers.GoopFlies>")                                                         //Makes a copy of the Summon Fallow effect
        .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
        {
            ((StatusEffectSummon)data).summonCard = TryGet<CardData>("GoopFlies"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                   //This is because TryGet will try to prefix the name with your GUID. 
        })                                                                          //If that fails, then it uses no GUID-prefixing.
         );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon GoopFlies") //Copying Instant Summon Fallow and changing the name.
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon GoopFlies");
           })
         );




        statusEffects.Add(
           new StatusEffectDataBuilder(this)
           .Create<StatusEffectImmuneToX>("ImmuneNull")
           .WithText("Resists <keyword=null>")
           .WithStackable(true)
           .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectImmuneToX;

               realData.immunityType = "ink";
           }
           ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXOnCardPlayed>("Trigger Straw Cherry")
          .WithText("When used, Trigger <card=goobers.Cherry>")
          .WithStackable(true)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectApplyXOnCardPlayed;

              realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
              realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
              realData.applyConstraints = new TargetConstraint[]
 {
        new TargetConstraintIsSpecificCard()
        {
            allowedCards = new CardData[]
            {
                TryGet<CardData>("Cherry")
            }
        }
          };
          }
          ));

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXWhenDrawn>("Random Bom")
        .WithText("When drawn, apply <{a}><keyword=weakness> to a random enemy")
        .WithStackable(true)
        .WithCanBeBoosted(false)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXWhenDrawn;

            realData.effectToApply = TryGet<StatusEffectData>("Weakness");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
        }
        ));

        statusEffects.Add(
    new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXOnTurn>("Random Bom On Turn")
    .WithText("Apply <{a}><keyword=weakness> to a random enemy.")
    .WithStackable(true)
    .WithCanBeBoosted(false)
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var realData = data as StatusEffectApplyXOnTurn;

        realData.effectToApply = TryGet<StatusEffectData>("Weakness");
        realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
    }
    ));

        statusEffects.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXOnTurn>("On Turn Gain Max Counter")
            .WithText("Gain <keyword=counter> by  <{a}>")
            .WithStackable(true)
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectApplyX;

                realData.effectToApply = TryGet<StatusEffectData>("Increase Max Counter");
                realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            }
            ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXOnTurn>("Reduce ally behind Counter")
          .WithText("Counts down <keyword=counter> by <{a}> to ally behind")
          .WithStackable(true)
          .WithCanBeBoosted(true)
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectApplyX;

              realData.effectToApply = TryGet<StatusEffectData>("Reduce Counter");
              realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
          }
          ));


        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXPostAttack>("Set Attack to Neutral After Hit")
        .WithText("Set <keyword=attack> to  <{a}> after attacking")
        .WithStackable(true)
        .WithCanBeBoosted(true)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXPostAttack;

            realData.effectToApply = TryGet<StatusEffectData>("Set Attack");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
        }
        ));

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXWhenAllyIsHit>("Retaliate")
        .WithText("Gain <+{a}><keyword=attack> when ally is hit")
        .WithStackable(true)
        .WithCanBeBoosted(true)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXWhenAllyIsHit;

            realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
        }
        ));

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectInstantSplashDamage>("Splash")
        .WithText("{0} <{a}>")
        .WithTextInsert($"<keyword={Extensions.PrefixGUID("splash", this)}>")
        .WithStackable(true)
        .WithCanBeBoosted(true)

        );

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXWhenHit>("Random Soda on Hit")
        .WithText("Gain a Fresh Greasypop to your hand")
        .WithStackable(true)
        .WithCanBeBoosted(true)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXWhenHit;

            realData.effectToApply = TryGet<StatusEffectData>("Random Soda");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            realData.targetMustBeAlive = false;
        }
        ));
        statusEffects.Add(
              new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXOnTurn>("Heal Self")
              .WithText("Restore <keyword=health> by  <{a}>")
              .WithStackable(true)
              .WithCanBeBoosted(true)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyX;

                  realData.effectToApply = TryGet<StatusEffectData>("Heal");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
              }
              ));


        statusEffects.Add(
           new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXOnKill>("Gain Frenzy")
           .WithText("Gain <keyword=frenzy> by  <{a}> on Kill.")
           .WithStackable(true)
           .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyX;

               realData.effectToApply = TryGet<StatusEffectData>("MultiHit");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
           }
           ));
        statusEffects.Add(
           new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXPreTrigger>("Gain Attack Before Hit")
           .WithText("Gain +<{a}> <keyword=attack> before attacking.")
           .WithStackable(true)
           .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXPreTrigger;

               realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
           }
           ));

        statusEffects.Add(
       new StatusEffectDataBuilder(this)
       .Create<StatusEffectApplyXOnTurn>("Increase Max counter in row")
       .WithText("Increase the enemy's max <keyword=counter> by 2 in the row.")
       .WithStackable(true)
       .WithCanBeBoosted(false)
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectApplyXOnTurn;

           realData.effectToApply = TryGet<StatusEffectData>("Increase Max Counter");
           realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
       }
       ));
        //RANDOM STATUS EFFECTS
        statusEffects.Add(
           new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyRandomOnCardPlayed>("Random Test")
           .WithText("Random")
           .WithStackable(true)
           .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
           {
               ((StatusEffectApplyRandomOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
               ((StatusEffectApplyRandomOnCardPlayed)data).effectsToapply = new StatusEffectData[]
               {
                   Get<StatusEffectData>("Spice"),
                   Get<StatusEffectData>("Snow")

               }; 
           }
           ));
        statusEffects.Add(
           new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyRandomWhenHit>("Random Soda")
           .WithText("When hit, Add <{a}> Random Fresh Greasypop to your hand")
           .WithStackable(true)
           .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
           {
               ((StatusEffectApplyRandomWhenHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               ((StatusEffectApplyRandomWhenHit)data).eventPriority = 1;
               ((StatusEffectApplyRandomWhenHit)data).targetMustBeAlive = false;
               ((StatusEffectApplyRandomWhenHit)data).effectsToapply = new StatusEffectData[]
               {
                   Get<StatusEffectData>("Instant Summon FC In Hand"),Get<StatusEffectData>("Instant Summon FC In Hand"),Get<StatusEffectData>("Instant Summon FC In Hand"),Get<StatusEffectData>("Instant Summon FC In Hand"),
                   Get<StatusEffectData>("Instant Summon FC In Hand"),Get<StatusEffectData>("Instant Summon FC In Hand"),Get<StatusEffectData>("Instant Summon FC In Hand"),
                   Get<StatusEffectData>("Instant Summon FM In Hand"),Get<StatusEffectData>("Instant Summon FM In Hand"),Get<StatusEffectData>("Instant Summon FM In Hand"),Get<StatusEffectData>("Instant Summon FM In Hand"),
                   Get<StatusEffectData>("Instant Summon FM In Hand"),Get<StatusEffectData>("Instant Summon FM In Hand"),Get<StatusEffectData>("Instant Summon FM In Hand"),
                   Get<StatusEffectData>("Instant Summon FB In Hand"),Get<StatusEffectData>("Instant Summon FB In Hand"),Get<StatusEffectData>("Instant Summon FB In Hand"),Get<StatusEffectData>("Instant Summon FB In Hand"),
                   Get<StatusEffectData>("Instant Summon FB In Hand"),Get<StatusEffectData>("Instant Summon FB In Hand"),Get<StatusEffectData>("Instant Summon FB In Hand"),
                   Get<StatusEffectData>("Instant FCC Hand"),Get<StatusEffectData>("Instant FCC Hand"),Get<StatusEffectData>("Instant FCC Hand"),
                   Get<StatusEffectData>("Instant FCC Hand"),Get<StatusEffectData>("Instant FCC Hand"),Get<StatusEffectData>("Instant FCC Hand"),
                   Get<StatusEffectData>("Instant Summon FBB In Hand"),Get<StatusEffectData>("Instant Summon FBB In Hand"),Get<StatusEffectData>("Instant Summon FBB In Hand"),
                   Get<StatusEffectData>("Instant Summon FBB In Hand"),Get<StatusEffectData>("Instant Summon FBB In Hand"),Get<StatusEffectData>("Instant Summon FBB In Hand"),
                   Get<StatusEffectData>("Instant Summon FT In Hand"),Get<StatusEffectData>("Instant Summon FT In Hand"),Get<StatusEffectData>("Instant Summon FT In Hand"),
                   Get<StatusEffectData>("Instant Summon FT In Hand"),Get<StatusEffectData>("Instant Summon FT In Hand"),Get<StatusEffectData>("Instant Summon FT In Hand"),
                   Get<StatusEffectData>("Instant Summon FSh In Hand"),Get<StatusEffectData>("Instant Summon FSh In Hand"),Get<StatusEffectData>("Instant Summon FSh In Hand"),
                   Get<StatusEffectData>("Instant Summon FSh In Hand"),Get<StatusEffectData>("Instant Summon FSh In Hand"),Get<StatusEffectData>("Instant Summon FSh In Hand"),
                   Get<StatusEffectData>("Instant Summon FN In Hand"),Get<StatusEffectData>("Instant Summon FN In Hand"),Get<StatusEffectData>("Instant Summon FN In Hand"),
                   Get<StatusEffectData>("Instant Summon FN In Hand"),Get<StatusEffectData>("Instant Summon FN In Hand"),Get<StatusEffectData>("Instant Summon FN In Hand"),
                   Get<StatusEffectData>("Instant Summon FClu In Hand"),Get<StatusEffectData>("Instant Summon FClu In Hand"),Get<StatusEffectData>("Instant Summon FClu In Hand"),
                   Get<StatusEffectData>("Instant Summon FDBERRY In Hand"),Get<StatusEffectData>("Instant Summon FDBERRY In Hand"),
                   Get<StatusEffectData>("Instant Summon FGOLD In Hand"),

               };
           }
           ));


        statusEffects.Add(
    new StatusEffectDataBuilder(this)
    .Create<StatusEffectInstantSummonRandomFromPool>("Test Venda")
    .WithText("Test Soda.")
    .WithStackable(true)
    .WithCanBeBoosted(false)
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var realData = data as StatusEffectInstantSummonRandomFromPool;

        realData.canSummonMultiple = true;
        realData.targetSummon = TryGet<StatusEffectSummon>("Summon Junk");
        realData.summonPosition = StatusEffectInstantSummon.Position.Hand;
        realData.pool = GetCards(


           "FPopSpice", "FPopSpice", "FPopSpice", "FPopSpice", "FPopSpice", "FPopSpice",
           "FPopBerry", "FPopBerry", "FPopBerry", "FPopBerry", "FPopBerry", "FPopBerry",
           "FSBerry", "FSBerry", "FSBerry", "FSBerry", "FSBerry", "FPopmint", "FPopmint", "FPopmint", "FPopmint", "FPopmint", "FPopmint",
"FPopNut", "FPopNut", "FPopNut", "FPopNut", "FPopNut", "FPopNut",
"FPopBurn", "FPopBurn", "FPopBurn", "FPopBurn",
"FPopTeeth", "FPopTeeth", "FPopTeeth", "FPopTeeth",
"FPopClunk", "FPopClunk", "FPopClunk",
"FPopShroom", "FPopShroom", "FPopShroom", "FPopShroom",
"FPopCola", "FPopCola", "FPopCola","FPopCola",
"PopGold", "PopGold"

  );
    }
    ));


        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXWhenHit>("Random Soda on Hit2")
        .WithText("When hit, gain <{a}> Fresh Greasypop to your hand")
        .WithStackable(true)
        .WithCanBeBoosted(true)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXWhenHit;

            realData.effectToApply = TryGet<StatusEffectData>("Test Venda");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            realData.targetMustBeAlive = false;
            realData.attackerConstraints = new[]
                      {

                        new TargetConstraintIsSpecificCard()
                        {
                           not = true, allowedCards = new CardData[]
                            {
                              Get<CardData>("Dollars")
                            }
                        }
                         };
        }
        ));


        statusEffects.Add(
        StatusCopy("When Deployed Copy Effects Of RandomEnemy", "When Deployed Copy Effects of Enemy Front")
       .WithText("When deployed, copy Effect of Enemy in front.")                                       //Since this effect is on Shade Serpent, we modify the description shown.
       .WithTextInsert("")                                                        //Makes a copy of the Summon Fallow effect
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectApplyX;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

           realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.FrontEnemy;   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                 //This is because TryGet will try to prefix the name with your GUID. 
       })
        //If that fails, then it uses no GUID-prefixing.
        );
        statusEffects.Add(
       StatusCopy("On Card Played Trigger RandomAlly", "Trigger All")
      .WithText("Trigger all allies.")                                       //Since this effect is on Shade Serpent, we modify the description shown.
      .WithTextInsert("")                                                        //Makes a copy of the Summon Fallow effect
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyX;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                            //This is because TryGet will try to prefix the name with your GUID. 
      })
       //If that fails, then it uses no GUID-prefixing.
       );




        statusEffects.Add(
    new StatusEffectDataBuilder(this)
    .Create<StatusEffectTriggerWhenCertainAllyAttacks>("Trigger When Bucket Attacks")
    .WithCanBeBoosted(false)                                         //Not a Lumin Vase target
    .WithText("Trigger when {0} in row attacks.")                     //Changing the text description.
    .WithTextInsert("<card=goobers.Bucket>") //You must put the GUID in some way here. $"<card={Extensions.PrefixGUID("shadeSerpent",this)}>" works as well here.
    .WithType("")                                                    //Type is typically used for SFX/VFX when applying the status effect. Not necessary as we are not applying this effect during battle, unless you use the "add effect" command.
    .FreeModify(                                                     //FreeModify allows you to edit variables that the builder does not know about (from classes that extend StatusEffectData).
        delegate (StatusEffectData data)
        {
            data.isReaction = true;                                  //Both of these variables have their own method in StatusEffectData, I just did not see them until now :/
            data.stackable = false;                                  //isReaction gives the reaction symbol at the bottom of the card. stackable allows the effect to stack with others of similar kind.
        })
        .SubscribeToAfterAllBuildEvent(                                  //Finally, declare the ally to be Shade Serpent.
        delegate (StatusEffectData data)
        {
            ((StatusEffectTriggerWhenCertainAllyAttacks)data).ally = TryGet<CardData>("Bucket");
        })

        );

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectTriggerWhenCertainAllyAttacks>("Getting Yelled at")
 .WithCanBeBoosted(false)                                         //Not a Lumin Vase target
 .WithText("<keyword=goobers.yellat>")                     //Changing the text description.
 .WithTextInsert(null) //You must put the GUID in some way here. $"<card={Extensions.PrefixGUID("shadeSerpent",this)}>" works as well here.
 .WithType("")                                                    //Type is typically used for SFX/VFX when applying the status effect. Not necessary as we are not applying this effect during battle, unless you use the "add effect" command.
 .FreeModify(                                                     //FreeModify allows you to edit variables that the builder does not know about (from classes that extend StatusEffectData).
     delegate (StatusEffectData data)
     {
         data.isReaction = true;                                  //Both of these variables have their own method in StatusEffectData, I just did not see them until now :/
         data.stackable = false;                                  //isReaction gives the reaction symbol at the bottom of the card. stackable allows the effect to stack with others of similar kind.
     })
     .SubscribeToAfterAllBuildEvent(                                  //Finally, declare the ally to be Shade Serpent.
     delegate (StatusEffectData data)
     {
         ((StatusEffectTriggerWhenCertainAllyAttacks)data).ally = TryGet<CardData>("Hateu");
     })
     );


        statusEffects.Add(
    new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenCertainAllyAttacks>("OKAY GOT IT BOSS!")
    .WithCanBeBoosted(false)                                         //Not a Lumin Vase target
    .WithText($"<keyword={Extensions.PrefixGUID("special", this)}>")                     //Changing the text description.
    .WithTextInsert(null) //You must put the GUID in some way here. $"<card={Extensions.PrefixGUID("shadeSerpent",this)}>" works as well here.
    .WithType("")
    .FreeModify(                                                     //FreeModify allows you to edit variables that the builder does not know about (from classes that extend StatusEffectData).
        delegate (StatusEffectData data)
        {
            data.isReaction = true;                                  //Both of these variables have their own method in StatusEffectData, I just did not see them until now :/
            data.stackable = false;                                  //isReaction gives the reaction symbol at the bottom of the card. stackable allows the effect to stack with others of similar kind.
        })
        .SubscribeToAfterAllBuildEvent(                                  //Finally, declare the ally to be Shade Serpent.
        delegate (StatusEffectData data)
        {
            ((StatusEffectApplyXWhenCertainAllyAttacks)data).ally = TryGet<CardData>("Hateu");
            ((StatusEffectApplyXWhenCertainAllyAttacks)data).effectToApply = TryGet<StatusEffectData>("Reduce Max Counter");
            ((StatusEffectApplyXWhenCertainAllyAttacks)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Attacker;
            ((StatusEffectApplyXWhenCertainAllyAttacks)data).applyConstraints = new TargetConstraint[]
            {
        new TargetConstraintIsSpecificCard()
        {
            allowedCards = new CardData[]
            {
                TryGet<CardData>("Hateu")
            }
        }
            };
        })
        );



        //FOR GAIN CARD INTO HAND
        statusEffects.Add(
  StatusCopy("Summon Junk", "Summon CTB")
      .SubscribeToAfterAllBuildEvent(data =>
      {
          (data as StatusEffectSummon).summonCard = TryGet<CardData>("CTB");
      })
      );

        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon CTB In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon CTB") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
                  new StatusEffectDataBuilder(this)
                      .Create<StatusEffectInstantSummonInDeck>("Instant Summon CTB")
                      .WithText("...")
                      .WithCanBeBoosted(true)
                      .WithTextInsert("")
                       .SubscribeToAfterAllBuildEvent(data =>
                       {
                           var realData = data as StatusEffectInstantSummonInDeck;

                           realData.targetSummon = TryGet<StatusEffectData>("Summon CTB") as StatusEffectSummon;
                           realData.summonPosition = StatusEffectInstantSummonInDeck.Position.DiscardPile;
                           realData.canSummonMultiple = true;
                       })
                       );
        statusEffects.Add(
           new StatusEffectDataBuilder(this)
               .Create<StatusEffectApplyXWhenDeployed>("Add CTB to Hand")
               .WithText("When deployed, add <{a}> {0} to your discard pile,")
               .WithTextInsert("<card=goobers.CTB>")
               .WithStackable(true)
               .WithCanBeBoosted(true)
               .SubscribeToAfterAllBuildEvent(data =>
               {
                   var realData = data as StatusEffectApplyXWhenDeployed;

                   realData.effectToApply = TryGet<StatusEffectData>("Instant Summon CTB");
                   realData.applyToFlags = ApplyToFlags.Self;
               })
       );
        statusEffects.Add(
            new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXWhenDeployed>("Add CTB to Hands")
                .WithText("and <{a}> to your hand.")
                .WithTextInsert("<card=goobers.CTB>")
                .WithStackable(true)
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyXWhenDeployed;

                    realData.effectToApply = TryGet<StatusEffectData>("Instant Summon CTB In Hand");
                    realData.applyToFlags = ApplyToFlags.Self;
                })
        );

        statusEffects.Add(
    StatusCopy("Summon Junk", "Summon Rift")
        .SubscribeToAfterAllBuildEvent(data =>
        {
            (data as StatusEffectSummon).summonCard = TryGet<CardData>("Rift");
        })
        );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Rift In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Rift") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
            new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXOnTurn>("Add Rift to Hand")
                .WithText("Add <{a}> {0} to your hand.")
                .WithTextInsert("<card=goobers.Rift>")
                .WithStackable(true)
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyX;

                    realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Rift In Hand");
                    realData.applyToFlags = ApplyToFlags.Self;
                })
        );

        statusEffects.Add(
    StatusCopy("Summon Junk", "Summon Bone Needle")
        .SubscribeToAfterAllBuildEvent(data =>
        {
            (data as StatusEffectSummon).summonCard = TryGet<CardData>("Bone Needle");
        })
        );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon One Needle In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Bone Needle") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
            new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXOnTurn>("Add Bone Needle to Hand")
                .WithText("Add <{a}> {0} to your hand.")
                .WithTextInsert("<card=goobers.Bone Needle>")
                .WithStackable(true)
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyX;

                    realData.effectToApply = TryGet<StatusEffectData>("Instant Summon One Needle In Hand");
                    realData.applyToFlags = ApplyToFlags.Self;
                })


        );

        statusEffects.Add(
            new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXOnTurn>("ReduceMAXTURN")
                .WithText("Reduce max <keyword=counter> by 1.")
                .WithTextInsert("")
                .WithStackable(true)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyX;

                    realData.effectToApply = TryGet<StatusEffectData>("Reduce Max Counter");
                    realData.applyToFlags = ApplyToFlags.Self;
                })


        );


        //NEW STATUS EFFECT 2 AREA--------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXOnTurn>("Give all Allies in the row ATK")
              .WithText("+<{a}> <keyword=attack> to Allies in a row")
              .WithTextInsert("")
              .WithStackable(true)
              .WithCanBeBoosted(true)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyX;


                  realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                  realData.applyToFlags = ApplyToFlags.AlliesInRow;
              })

              );
        statusEffects.Add(
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXOnTurn>("Give all Allies in the row HP")
            .WithText("Restore <{a}><keyword=health> to all Allies in a Row ")
            .WithTextInsert("")
            .WithStackable(true)
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectApplyX;

                realData.effectToApply = TryGet<StatusEffectData>("Heal");

                realData.applyToFlags = ApplyToFlags.AlliesInRow;
            })

           );

        statusEffects.Add(
          StatusCopy("While Active Zoomlin When Drawn To Allies In Hand", "While Active Spark When Drawn To Allies In Hand")
         .WithText("While active, all allies gain {0} while in your hand.")                                       //Since this effect is on Shade Serpent, we modify the description shown.
         .WithTextInsert("<keyword=spark>")                                                        //Makes a copy of the Summon Fallow effect
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyX;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

             (data as StatusEffectApplyX).effectToApply = TryGet<StatusEffectData>("Temporary Spark");   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too

         })

         );
        statusEffects.Add(
        StatusCopy("Temporary Zoomlin", "Temporary Spark")                                                 //Makes a copy of the Summon Fallow effect
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

           (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("Spark");
           (data as StatusEffectTemporaryTrait).targetConstraints = new TargetConstraint[]
    {
        new TargetConstraintHasTrait()
        {
          not = true, trait = Get<TraitData>("Spark")
            },
          new TargetConstraintIsUnit()
        {
       
            }




    };

       })
       );



        statusEffects.Add(
            new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXWhenDeployed>("Random when deployed")
                .WithText("When Deployed, set <keyword=health> <keyword=attack> <keyword=counter> randomly")
                .WithTextInsert("")
                 .SubscribeToAfterAllBuildEvent(data =>
                 {
                     var realData = data as StatusEffectApplyXWhenDeployed;

                     realData.effectToApply = TryGet<StatusEffectData>("Randomize Stats (2-5)");

                     realData.applyToFlags = ApplyToFlags.Self;

                 }));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
              .Create<StatusEffectHaltX>("Halt Snow")
              .WithTextInsert("")
               .SubscribeToAfterAllBuildEvent(data =>
               {
                   var realData = data as StatusEffectHaltX;

                   realData.effectToHalt = TryGet<StatusEffectData>("Snow");


               }));

        statusEffects.Add(
   new StatusEffectDataBuilder(this)
       .Create<StatusEffectApplyXOnTurn>("Gain Sweet Point Self")
       .WithText("Gain <{a}> <keyword=sp>")
       .WithStackable(true)
       .WithCanBeBoosted(true)
       .WithTextInsert("")
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXOnTurn;

            realData.effectToApply = TryGet<StatusEffectData>("EXP");

            realData.applyToFlags = ApplyToFlags.Self;


        }));

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyXOnTurn>("Lose Sweet Point Self")
     .WithText("Lose {a} <keyword=sp>")
     .WithStackable(true)
     .WithCanBeBoosted(false)
     .WithTextInsert("")
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyXOnTurn;

          realData.effectToApply = TryGet<StatusEffectData>("LEXP");

          realData.applyToFlags = ApplyToFlags.Self;

      }));

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectApplyXOnCardPlayed>("Gain Sweet Point front ally")
                     .WithText("Apply <{a}> <keyword=sp> to ally in front.")
                     .WithTextInsert("")
                     .WithStackable(true)
                     .WithCanBeBoosted(true)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXOnCardPlayed;

                          realData.effectToApply = TryGet<StatusEffectData>("EXP");

                          realData.applyToFlags = ApplyToFlags.FrontAlly;



                      }));


        statusEffects.Add(
       new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXOnTurn>("Gain Sweet Point Self terror")
           .WithText("Gain 1 <keyword=sps>")
           .WithTextInsert("")
           .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectApplyXOnTurn;

                realData.effectToApply = TryGet<StatusEffectData>("EXPT");

                realData.applyToFlags = ApplyToFlags.Self;

            })

);
        statusEffects.Add(
            new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXOnKill>("On Kill, Gain Sweet Point Self")
                .WithText("On kill, gain <{a}> <keyword=sp>")
                .WithTextInsert("")
                .WithStackable(true)
                .WithCanBeBoosted(true)

                 .SubscribeToAfterAllBuildEvent(data =>
                 {
                     var realData = data as StatusEffectApplyXOnKill;

                     realData.effectToApply = TryGet<StatusEffectData>("EXP");

                     realData.applyToFlags = ApplyToFlags.Self;

                 })


  );
        statusEffects.Add(
           new StatusEffectDataBuilder(this)
               .Create<StatusEffectApplyXWhenDrawn>("When Drawn Heal")
               .WithText("When Drawn, Restore <{a}> <keyword=health> to all allies")
               .WithStackable(true)
               .WithCanBeBoosted(true)
               .WithTextInsert("")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyXWhenDrawn;

                    realData.effectToApply = TryGet<StatusEffectData>("Heal");

                    realData.applyToFlags = ApplyToFlags.Allies;
                })

   );

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXOnTurn>("Double Spice All")
              .WithText("Double <keyword=spice> to all allies,")
              .WithCanBeBoosted(false)
              .WithTextInsert("")
               .SubscribeToAfterAllBuildEvent(data =>
               {
                   var realData = data as StatusEffectApplyXOnTurn;

                   realData.effectToApply = TryGet<StatusEffectData>("Double Spice");

                   realData.applyToFlags = ApplyToFlags.Allies;

                   realData.eventPriority = 2;
               })

  );

        statusEffects.Add(
         new StatusEffectDataBuilder(this)
             .Create<StatusEffectApplyXOnTurn>("Spice row")
             .WithText("then apply <{a}><keyword=spice> to allies in the row")
             .WithStackable(true)
             .WithCanBeBoosted(true)
             .WithTextInsert("")
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXOnTurn;

                  realData.effectToApply = TryGet<StatusEffectData>("Spice");

                  realData.applyToFlags = ApplyToFlags.AlliesInRow;

                  realData.eventPriority = 1;
              })

 );
        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXOnTurn>("Spice now")
          .WithText("Apply <{a}><keyword=spice> to all allies and self")
          .WithStackable(true)
          .WithCanBeBoosted(true)
          .WithTextInsert("")
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXOnTurn;

               realData.effectToApply = TryGet<StatusEffectData>("Spice");

               realData.applyToFlags = ApplyToFlags.Allies;

               realData.eventPriority = 1;
           })

);

        statusEffects.Add(
    new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXOnTurn>("Spice now2")
        .WithStackable(true)
        .WithCanBeBoosted(true)
        .WithTextInsert("")
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXOnTurn;

             realData.effectToApply = TryGet<StatusEffectData>("Spice");

             realData.applyToFlags = ApplyToFlags.Self;

             realData.eventPriority = 1;
         })

);

        statusEffects.Add(
       new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXOnTurn>("Frost row")
           .WithText("then apply <{a}> <keyword=frost> to all enemies.")
           .WithStackable(true)
           .WithCanBeBoosted(true)
           .WithTextInsert("")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectApplyXOnTurn;

                realData.effectToApply = TryGet<StatusEffectData>("Frost");

                realData.applyToFlags = ApplyToFlags.Enemies;

                realData.eventPriority = 1;
            })

);

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXOnTurn>("Frost rowN")
          .WithText("then apply <{a}> <keyword=frost> to enemies in the row.")
          .WithStackable(true)
          .WithCanBeBoosted(true)
          .WithTextInsert("")
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXOnTurn;

               realData.effectToApply = TryGet<StatusEffectData>("Frost");

               realData.applyToFlags = ApplyToFlags.EnemiesInRow;

               realData.eventPriority = 1;
           })

);
        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXOnTurn>("Double Frost enemies row")
          .WithText("Apply double <keyword=frost> to enemies in the row,")
          .WithCanBeBoosted(false)
          .WithTextInsert("")
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXOnTurn;

               realData.effectToApply = TryGet<StatusEffectData>("Double Frost");

               realData.applyToFlags = ApplyToFlags.EnemiesInRow;

               realData.eventPriority = 2;
           })

);

        statusEffects.Add(
     new StatusEffectDataBuilder(this)
         .Create<StatusEffectInstantDoubleX>("Double Frost")
         .WithTextInsert("")
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectInstantDoubleX;

              realData.statusToDouble = TryGet<StatusEffectData>("Frost");

          })

);
        statusEffects.Add(
          new StatusEffectDataBuilder(this)
              .Create<StatusEffectWhileInHandX>("Weaken All")
              .WithText("While in hand, reduce <keyword=attack> by <{a}> to all Enemies")
              .WithStackable(true)
              .WithCanBeBoosted(true)
              .WithTextInsert("")
               .SubscribeToAfterAllBuildEvent(data =>
               {
                   var realData = data as StatusEffectWhileInHandX;

                   realData.effectToApply = TryGet<StatusEffectData>("Ongoing Reduce Attack");
                   realData.applyToFlags = ApplyToFlags.Enemies;
               })
               );

        statusEffects.Add(
     new StatusEffectDataBuilder(this)
         .Create<StatusEffectWhileInHandX>("Shroom All")
         .WithText("While in hand, Apply <{a}> <keyword=shroom> to all enemies")
         .WithStackable(true)
         .WithCanBeBoosted(true)
         .WithTextInsert("")
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectWhileInHandX;

              realData.effectToApply = TryGet<StatusEffectData>("Shroom");
              realData.applyToFlags = ApplyToFlags.Enemies;
          })
          );

        statusEffects.Add(
     new StatusEffectDataBuilder(this)
         .Create<StatusEffectWhileInHandX>("Shroom All Allies")
         .WithText(", Apply <{a}> <keyword=shroom> to all allies")
         .WithStackable(true)
         .WithCanBeBoosted(true)
         .WithTextInsert("")
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectWhileInHandX;

              realData.effectToApply = TryGet<StatusEffectData>("Shroom");
              realData.applyToFlags = ApplyToFlags.Allies;
          })
          );
        statusEffects.Add(
          new StatusEffectDataBuilder(this)
              .Create<StatusEffectWhileInHandX>("Shroom ")
              .WithText("While in hand, reduce <keyword=attack> by <{a}> to all Enemies")
              .WithStackable(true)
              .WithCanBeBoosted(true)
              .WithTextInsert("")
               .SubscribeToAfterAllBuildEvent(data =>
               {
                   var realData = data as StatusEffectWhileInHandX;

                   realData.effectToApply = TryGet<StatusEffectData>("Ongoing Reduce Attack");
                   realData.applyToFlags = ApplyToFlags.Enemies;
               })
  );
        statusEffects.Add(
          new StatusEffectDataBuilder(this)
              .Create<StatusEffectWhileInHandX>("Weaken All Allies")
              .WithText(",reduce <keyword=attack> by <{a}> to all Allies")
              .WithStackable(true)
              .WithCanBeBoosted(true)
              .WithTextInsert("")
               .SubscribeToAfterAllBuildEvent(data =>
               {
                   var realData = data as StatusEffectWhileInHandX;

                   realData.effectToApply = TryGet<StatusEffectData>("Ongoing Reduce Attack");
                   realData.applyToFlags = ApplyToFlags.Allies;
               })

  );

        statusEffects.Add(
         new StatusEffectDataBuilder(this)
             .Create<StatusEffectApplyXWhenDeployed>("DIE")
             .WithText("")
             .WithCanBeBoosted(false)
             .WithTextInsert("")
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenDeployed;

                  realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                  realData.applyToFlags = ApplyToFlags.Self;
              })

 );
 

        statusEffects.Add(
   new StatusEffectDataBuilder(this)
       .Create<StatusEffectApplyXWhenYAppliedTo>("Not Fast Enough")
       .WithText("When my <keyword=sps> reaches 11, I'm leaving.")
       .WithCanBeBoosted(false)
       .WithTextInsert(null)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXWhenYAppliedTo;

            realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Ally");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            realData.mustReachAmount = true;
            realData.whenAppliedTypes = new string[1] { "sps" };
            realData.whenAppliedToFlags = ApplyToFlags.Self;
            realData.descColorHex = "ff7bf3";
            realData.eventPriority = 1;
        }));



        // FOR YRA BOTS-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        statusEffects.Add(
        StatusCopy("Summon Junk", "Summon Mint Soda")
    .SubscribeToAfterAllBuildEvent(data =>
    {
        (data as StatusEffectSummon).summonCard = TryGet<CardData>("FPopmint");
    })
    );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon FM In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Mint Soda") as StatusEffectSummon;
                })
        );
        statusEffects.Add(
        StatusCopy("Summon Junk", "Summon spice Soda")
    .SubscribeToAfterAllBuildEvent(data =>
    {
        (data as StatusEffectSummon).summonCard = TryGet<CardData>("FPopSpice");
    })
    );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon FC In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon spice Soda") as StatusEffectSummon;
                })
        );
        statusEffects.Add(
        StatusCopy("Summon Junk", "Summon BERRY Soda")
    .SubscribeToAfterAllBuildEvent(data =>
    {
        (data as StatusEffectSummon).summonCard = TryGet<CardData>("FPopBerry");
    })
    );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon FB In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon BERRY Soda") as StatusEffectSummon;
                })
        );
        //KEK EFFECT!


        statusEffects.Add(
         new StatusEffectDataBuilder(this)
         .Create<StatusEffectApplyRandomOnCardPlayed>("Random Kek")
         .WithText("Random Effect")
         .WithStackable(true)
         .WithCanBeBoosted(true)
         .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
         {
             ((StatusEffectApplyRandomOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
             ((StatusEffectApplyRandomOnCardPlayed)data).eventPriority = 2;
             ((StatusEffectApplyRandomOnCardPlayed)data).effectsToapply = new StatusEffectData[]
             {
                   Get<StatusEffectData>("Apply Haze"),Get<StatusEffectData>("Instant Summon FC In Hand")

             };
         }
         ));










        // FOR YRA BOTS
        //TALA PLANTS---------------------------------------------------------------------------------------------
        statusEffects.Add(
       StatusCopy("Summon Junk", "Summon Nightshade")
   .SubscribeToAfterAllBuildEvent(data =>
   {
       (data as StatusEffectSummon).summonCard = TryGet<CardData>("Nightshade");
   })
   );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Nightshade In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Nightshade") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
       StatusCopy("Summon Junk", "Summon Tomatoes")
   .SubscribeToAfterAllBuildEvent(data =>
   {
       (data as StatusEffectSummon).summonCard = TryGet<CardData>("Tomatoes");
   })
   );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Tomatoes In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Tomatoes") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyRandomOnCardPlayed>("Random Tala")
          .WithText("Gain <{a}> either {0} to your hand")
          .WithTextInsert("<card=goobers.Nightshade> or <card=goobers.Tomatoes>")
          .WithStackable(true)
          .WithCanBeBoosted(true)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectApplyRandomOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
              ((StatusEffectApplyRandomOnCardPlayed)data).effectsToapply = new StatusEffectData[]
              {
                   Get<StatusEffectData>("Instant Summon Tomatoes In Hand"),
                   Get<StatusEffectData>("Instant Summon Nightshade In Hand")

              };
          }
          ));
        //FOR AYRA------------------------------------------------------------------------------------------------

        statusEffects.Add(
          StatusCopy("Summon Fallow", "Summon AYraB1")
         .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
         .WithTextInsert("<card=goobers.AYraB1>")                                                         //Makes a copy of the Summon Fallow effect
         .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
         {
             ((StatusEffectSummon)data).summonCard = TryGet<CardData>("AYraB1"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                 //This is because TryGet will try to prefix the name with your GUID. 
         })                                                                          //If that fails, then it uses no GUID-prefixing.
          );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon AYraB1") //Copying Instant Summon Fallow and changing the name.
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon AYraB1");
           })
         );
        statusEffects.Add(
          StatusCopy("Summon Fallow", "Summon AYraB2")
         .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
         .WithTextInsert("<card=goobers.GoopFlies>")                                                         //Makes a copy of the Summon Fallow effect
         .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
         {
             ((StatusEffectSummon)data).summonCard = TryGet<CardData>("AYraB2"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                 //This is because TryGet will try to prefix the name with your GUID. 
         })                                                                          //If that fails, then it uses no GUID-prefixing.
          );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon AYraB2") //Copying Instant Summon Fallow and changing the name.
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon AYraB2");
           })
         );
        statusEffects.Add(
          StatusCopy("Summon Fallow", "Summon AYraB3")
         .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
         .WithTextInsert("<card=goobers.GoopFlies>")                                                         //Makes a copy of the Summon Fallow effect
         .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
         {
             ((StatusEffectSummon)data).summonCard = TryGet<CardData>("AYraB3"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                 //This is because TryGet will try to prefix the name with your GUID. 
         })                                                                          //If that fails, then it uses no GUID-prefixing.
          );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon AYraB3") //Copying Instant Summon Fallow and changing the name.
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon AYraB3");
           })
         );
        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyRandomOnCardPlayed>("Random AYraBot")
        .WithText("Summon <{a}> random Yra Bot.{0}")
         .WithTextInsert("<card=goobers.AYraB1>,<card=goobers.AYraB2>,<card=goobers.AYraB3>")
        .WithStackable(false)
        .WithCanBeBoosted(true)
        .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
        {
            ((StatusEffectApplyRandomOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            ((StatusEffectApplyRandomOnCardPlayed)data).effectsToapply = new StatusEffectData[]
            {
                   Get<StatusEffectData>("Instant Summon AYraB1"),
                   Get<StatusEffectData>("Instant Summon AYraB2"),
                   Get<StatusEffectData>("Instant Summon AYraB3"),
            };
        })
        );
        //TALA PLANTS---------------------------------------------------------------------------------------------
        //FOR VENDING MACHINE
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon YraB1 Soda")
    .SubscribeToAfterAllBuildEvent(data =>
    {
        (data as StatusEffectSummon).summonCard = TryGet<CardData>("YraB1");
    })
    );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon B1 In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon YraB1 Soda") as StatusEffectSummon;
                })
        );


        statusEffects.Add(
StatusCopy("Summon Junk", "Summon YraB2 Soda")
   .SubscribeToAfterAllBuildEvent(data =>
   {
       (data as StatusEffectSummon).summonCard = TryGet<CardData>("YraB2");
   })
   );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon B2 In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon YraB2 Soda") as StatusEffectSummon;
                })
        );
      

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Yra3 Soda")
   .SubscribeToAfterAllBuildEvent(data =>
   {
       (data as StatusEffectSummon).summonCard = TryGet<CardData>("YraB3");
   })
   );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon B3 In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Yra3 Soda") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
       new StatusEffectDataBuilder(this)
       .Create<StatusEffectSummon>("Summon terri")
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectSummon;
            
            realData.summonCard = TryGet<CardData>("Terri");
        }));

        statusEffects.Add(
        StatusCopy("When Deployed Summon Wowee", "When Deployed Summon Terrimisu")
       .WithText("When deployed Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
       .WithTextInsert("<card=goobers.Terri>")
        .WithCanBeBoosted(false)//Makes a copy of the Summon Fallow effect
       .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
       {
           ((StatusEffectApplyXWhenDeployed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Terrimisu");
           ((StatusEffectApplyXWhenDeployed)data).applyToFlags = ApplyToFlags.Self;
           ((StatusEffectApplyXWhenDeployed)data).applyConstraints = new TargetConstraint[]
      {
        new TargetConstraintOnBoard()
      
      };

           //This is because TryGet will try to prefix the name with your GUID. 
       })                                                                          //If that fails, then it uses no GUID-prefixing.

 );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Ascended Terrimisu")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Terri");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Terrimisu")
                     .WithText("...")
                     .WithCanBeBoosted(false)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Ascended Terrimisu") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Terri");

                      })
                      );


        statusEffects.Add(
     new StatusEffectDataBuilder(this)
         .Create<StatusEffectApplyXOnTurn>("Trigger Front")
         .WithText("Trigger ally ahead")
         .WithCanBeBoosted(false)
         .WithTextInsert("")
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectApplyXOnTurn;

              realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
              realData.applyToFlags = ApplyToFlags.AllyInFrontOf;
          })
          );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXOnTurn>("Trigger Front2")
    .WithText("Trigger ally ahead")
    .WithCanBeBoosted(false)
    .WithTextInsert("")
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXOnTurn;

         realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
         realData.applyToFlags = ApplyToFlags.AllyInFrontOf;
         realData.eventPriority = 100;
     })
     );
        statusEffects.Add(
          StatusCopy("Summon Fallow", "Summon Arift")
         .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
         .WithTextInsert("<card=goobers.GoopFlies>")                                                         //Makes a copy of the Summon Fallow effect
         .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
         {
             ((StatusEffectSummon)data).summonCard = TryGet<CardData>("ARift"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                //This is because TryGet will try to prefix the name with your GUID. 
         })                                                                          //If that fails, then it uses no GUID-prefixing.
          );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon Arift") //Copying Instant Summon Fallow and changing the name.

        .WithText("Summon <{a}> {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
         .WithTextInsert("<card=goobers.ARift>")
         .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon Arift");
           })

         );
        statusEffects.Add(
    new StatusEffectDataBuilder(this)
    .Create<StatusEffectInstantCombineCard>("UNewtral combo")
    .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
    {
        ((StatusEffectInstantCombineCard)data).cardNames = new string[1] { "goobers.Newtral" };
        ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.UNewtral";
        ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
        ((StatusEffectInstantCombineCard)data).changeDeck = false;
        ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
    })
    );

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenYAppliedTo>("Burrow go!")
            .WithText($"<keyword={Extensions.PrefixGUID("temp", this)}> into <card=goobers.UNewtral> when <keyword=sp> reaches 6.")
            .WithCanBeBoosted(false)
            .WithTextInsert("<card=goobers.Sharoco2>")
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenYAppliedTo;

                 realData.effectToApply = TryGet<StatusEffectData>("UNewtral combo");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.mustReachAmount = true;
                 realData.count = 1;
                 realData.whenAppliedTypes = new string[1] { "sp" };
                 realData.whenAppliedToFlags = ApplyToFlags.Self;

                 var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                 script.amount = 1;
                 ((StatusEffectApplyX)data).scriptableAmount = script;
             }));

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Newtral")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Newtral");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Newtral")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Newtral") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Newtral");
                          realData.withEffects = new StatusEffectData[] { TryGet<StatusEffectData>("Temporary Spark") };
                      })
                      );


        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedTo>("UP go!")
          .WithText($"<keyword={Extensions.PrefixGUID("temp", this)}> into <card=goobers.Newtral> with <keyword=spark> when <keyword=sp> reaches 4.")
          .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedTo;

               realData.effectToApply = TryGet<StatusEffectData>("UP trig");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.mustReachAmount = true;
               realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
               realData.count = 1;
               realData.whenAppliedTypes = new string[1] { "sp" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;

               var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
               script.amount = 1;
               ((StatusEffectApplyX)data).scriptableAmount = script;
           }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectInstantMultiple>("UP trig")
    .WithText("")
    .WithCanBeBoosted(false)
    .WithTextInsert("=")
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectInstantMultiple;

         realData.effects = new StatusEffectInstant[]
     {
     TryGet<StatusEffectInstant>("Sacrifice Ally"),
     TryGet<StatusEffectInstant>("Instant Summon Newtral")
     };
     }

         ));



        statusEffects.Add(
         new StatusEffectDataBuilder(this)
             .Create<StatusEffectApplyXWhenYAppliedTo>("Choco LV2")
             .WithText($"<keyword={Extensions.PrefixGUID("perma", this)}> into <card=goobers.Sharoco2> when <keyword=sp> reaches 40.")
             .WithCanBeBoosted(false)
             .WithTextInsert("<card=goobers.Sharoco2>")
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenYAppliedTo;

                  realData.effectToApply = TryGet<StatusEffectData>("SharocoLV2 combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.mustReachAmount = true;
                  realData.eventPriority = 2;
                  realData.whenAppliedTypes = new string[1] { "sp" };
                  realData.whenAppliedToFlags = ApplyToFlags.Self;
                  realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                  var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                  script.amount = 1;
                  ((StatusEffectApplyX)data).scriptableAmount = script;
              })
              );

        statusEffects.Add(
             new StatusEffectDataBuilder(this)
                 .Create<StatusEffectApplyXWhenYAppliedTo>("Choco LV3")
                 .WithText($"<keyword={Extensions.PrefixGUID("perma", this)}> into <card=goobers.Sharoco3> when <keyword=sp> reaches 100.")
                 .WithCanBeBoosted(false)
                 .WithTextInsert("<card=goobers.Sharoco2>")
                  .SubscribeToAfterAllBuildEvent(data =>
                  {
                      var realData = data as StatusEffectApplyXWhenYAppliedTo;

                      realData.effectToApply = TryGet<StatusEffectData>("SharocoLV3 combo");
                      realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                      realData.mustReachAmount = true;
                      realData.eventPriority = 2;
                      realData.whenAppliedTypes = new string[1] { "sp" };
                      realData.whenAppliedToFlags = ApplyToFlags.Self;
                      realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                      var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                      script.amount = 1;
                      ((StatusEffectApplyX)data).scriptableAmount = script;
                  })
                  );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("SharocoLV2 combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[1] { "goobers.Sharoco" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Sharoco2";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
})
);

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
      .Create<StatusEffectInstantCombineCard>("SharocoLV3 combo")
      .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
      {
          ((StatusEffectInstantCombineCard)data).cardNames = new string[1] { "goobers.Sharoco2" };
          ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Sharoco3";
          ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
          ((StatusEffectInstantCombineCard)data).changeDeck = true;
          ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
      })
      );
        statusEffects.Add(
       new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXPreTurn>("Before Attack, Demonize Targets")
           .WithText("Before attacking, apply <{a}> <keyword=demonize> to enemies in the row")
           .WithCanBeBoosted(false)
           .WithTextInsert("")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectApplyXPreTurn;

                realData.effectToApply = TryGet<StatusEffectData>("Demonize");
                realData.applyToFlags = ApplyToFlags.EnemiesInRow;

            }));



        statusEffects.Add(
    new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXOnTurn>("ARIFT NOW")
        .WithText("Summon {0}")
        .WithCanBeBoosted(false)
        .WithTextInsert("<card=goobers.ARift>")
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXOnTurn;

             realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Arift");
             realData.applyToFlags = ApplyToFlags.Self;
         })
         );
        statusEffects.Add(
   new StatusEffectDataBuilder(this)
       .Create<StatusEffectApplyXOnTurn>("AM2 NOW")
       .WithText("")
       .WithCanBeBoosted(false)
       .WithTextInsert("<card=goobers.ARift>")
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXOnTurn;

            realData.effectToApply = TryGet<StatusEffectData>("Instant Summon AM2");
            realData.applyToFlags = ApplyToFlags.Self;
        })
        );

        statusEffects.Add(
        StatusCopy("Summon Fallow", "Summon AM2")
       .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
       .WithTextInsert("<card=goobers.M2")                                                         //Makes a copy of the Summon Fallow effect
       .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
       {
           ((StatusEffectSummon)data).summonCard = TryGet<CardData>("AM2"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                            //This is because TryGet will try to prefix the name with your GUID. 
       })                                                                          //If that fails, then it uses no GUID-prefixing.
        );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon AM2") //Copying Instant Summon Fallow and changing the name.
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon AM2");
           })
         );


        statusEffects.Add(
        StatusCopy("Summon Fallow", "Summon AM3")
       .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
       .WithTextInsert("<card=goobers.M2")                                                         //Makes a copy of the Summon Fallow effect
       .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
       {
           ((StatusEffectSummon)data).summonCard = TryGet<CardData>("AM3"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                            //This is because TryGet will try to prefix the name with your GUID. 
       })                                                                          //If that fails, then it uses no GUID-prefixing.
        );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon AM3") //Copying Instant Summon Fallow and changing the name.
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon AM3");
           })
         );

        statusEffects.Add(
        StatusCopy("Summon Fallow", "Summon AM4")
       .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
       .WithTextInsert("<card=goobers.M2")                                                         //Makes a copy of the Summon Fallow effect
       .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
       {
           ((StatusEffectSummon)data).summonCard = TryGet<CardData>("AM4"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                            //This is because TryGet will try to prefix the name with your GUID. 
       })                                                                          //If that fails, then it uses no GUID-prefixing.
        );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon AM4") //Copying Instant Summon Fallow and changing the name.
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon AM4");
           })
         );

        statusEffects.Add(
                StatusCopy("When Destroyed Summon Dregg", "When Destroyed Summon AM3")
               .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
               .WithTextInsert("<card=goobers.M3>")                                                         //Makes a copy of the Summon Fallow effect
               .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
               {
                   ((StatusEffectApplyXWhenDestroyed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon AM3"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                                                           //This is because TryGet will try to prefix the name with your GUID. 
               })                                                                          //If that fails, then it uses no GUID-prefixing.

         );
        statusEffects.Add(
        StatusCopy("When Destroyed Summon Dregg", "When Destroyed Summon AM4")
       .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
       .WithTextInsert("<card=goobers.M4>")                                                         //Makes a copy of the Summon Fallow effect
       .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
       {
           ((StatusEffectApplyXWhenDestroyed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon AM4"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                                                   //This is because TryGet will try to prefix the name with your GUID. 
       })                                                                          //If that fails, then it uses no GUID-prefixing.

 );
        //To trigger when hit with a specific Item-----------------------------------------------------------------------------------------------------------------------------------------------

        statusEffects.Add(
     new StatusEffectDataBuilder(this)
     .Create<StatusEffectInstantCombineCard>("InkabomA combo")
     .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
     {
         ((StatusEffectInstantCombineCard)data).cardNames = new string[2] { "goobers.Inkabom", "goobers.Inky Ritual Stone" };
         ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.InkabomA";
         ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
         ((StatusEffectInstantCombineCard)data).changeDeck = true;
         ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
     })
     );
        statusEffects.Add(
    new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXWhenHit>("Ascend Inkabom")
                .WithText("<keyword=goobers.inky>")

                .WithStackable(false)
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyXWhenHit;

                    realData.eventPriority = 2;
                    realData.effectToApply = TryGet<StatusEffectData>("InkabomA combo");
                    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    realData.attackerConstraints = new[]
                    {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Inky Ritual Stone")
                            }
                        }
                    };
                })
);

        statusEffects.Add(
    new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXWhenHit>("Oh Candle")
                .WithText("<keyword=goobers.candle>")
                .WithTextInsert("")
                .WithStackable(false)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyXWhenHit;

                    realData.eventPriority = 2;
                    realData.effectToApply = TryGet<StatusEffectData>("Gain Gold");
                    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    realData.attackerConstraints = new[]
                    {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Candle")
                            }
                        }
                    };
                })
);
        statusEffects.Add(
    new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXWhenHit>("Run away together")
                .WithStackable(false)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyXWhenHit;

                    realData.eventPriority = 1;
                    realData.effectToApply = TryGet<StatusEffectData>("Escape");
                    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    realData.attackerConstraints = new[]
                    {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Candle")
                            }
                        }
                    };
                })
        );

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyRandomOnCardPlayed>("Random YraBot")
          .WithText("Gain <{a}> random Yra Bot to your hand.{0}")
           .WithTextInsert("<card=goobers.YraB1>,<card=goobers.YraB2>,<card=goobers.YraB3>")
          .WithStackable(true)
          .WithCanBeBoosted(true)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectApplyRandomOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
              ((StatusEffectApplyRandomOnCardPlayed)data).effectsToapply = new StatusEffectData[]
              {
                   Get<StatusEffectData>("Instant Summon B1 In Hand"),
                   Get<StatusEffectData>("Instant Summon B2 In Hand"),
                   Get<StatusEffectData>("Instant Summon B3 In Hand"),
              };
          })
          );
        //END OF YRA
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Cola Soda")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     (data as StatusEffectSummon).summonCard = TryGet<CardData>("FPopCola");
 })
 );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant FCC Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Cola Soda") as StatusEffectSummon;
                })
        );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Burn Soda")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     (data as StatusEffectSummon).summonCard = TryGet<CardData>("FPopBurn");
 })
 );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon FBB In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Burn Soda") as StatusEffectSummon;
                })
        );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Clunk Soda")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     (data as StatusEffectSummon).summonCard = TryGet<CardData>("FPopClunk");
 })
 );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon FClu In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Clunk Soda") as StatusEffectSummon;
                })
        );


        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Teeth Soda")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     (data as StatusEffectSummon).summonCard = TryGet<CardData>("FPopTeeth");
 })
 );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon FT In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Teeth Soda") as StatusEffectSummon;
                })
        );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Nut Soda")
.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("FPopNut");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon FN In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Nut Soda") as StatusEffectSummon;
                })
        );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Shroom Soda")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     (data as StatusEffectSummon).summonCard = TryGet<CardData>("FPopShroom");
 })
 );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon FSh In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Shroom Soda") as StatusEffectSummon;
                })
        );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Gold Soda")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     (data as StatusEffectSummon).summonCard = TryGet<CardData>("PopGold");
 })
 );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon FGOLD In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Gold Soda") as StatusEffectSummon;
                })
        );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon FDBERRY")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     (data as StatusEffectSummon).summonCard = TryGet<CardData>("FSBerry");
 })
 );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon FDBERRY In Hand")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon FDBERRY") as StatusEffectSummon;
                })
        );
        //END VENDING MACHINE
        //END OF GAIN CARD


        statusEffects.Add(
              StatusCopy("When Destroyed Apply Damage To Attacker", "When Destroyed Apply Damage To Attacker with text")
             .WithText("When destroyed, deal <{a}> to the attacker.")                                                              //Makes a copy of the Summon Fallow effect

             );

        statusEffects.Add(
        StatusCopy("Summon Fallow", "Summon M2")
       .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
       .WithTextInsert("<card=goobers.M2")                                                         //Makes a copy of the Summon Fallow effect
       .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
       {
           ((StatusEffectSummon)data).summonCard = TryGet<CardData>("M2"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                           //This is because TryGet will try to prefix the name with your GUID. 
       })                                                                          //If that fails, then it uses no GUID-prefixing.
        );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon M2") //Copying Instant Summon Fallow and changing the name.
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon M2");
           })
         );
        statusEffects.Add(
       StatusCopy("When Sacrificed Summon TailsOne", "When Sacrificed Summon M2") //Copying Instant Summon Fallow and changing the name.
       .WithText("<keyword=goobers.dormant>")
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
          {
              ((StatusEffectApplyXWhenDestroyed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon M2");
          })
        );
        statusEffects.Add(
      StatusCopy("When Sacrificed Summon TailsOne", "When Sacrificed Summon M2 2") //Copying Instant Summon Fallow and changing the name.
      .WithText("When <keyword=sacrificed> summon {0}.")
      .WithTextInsert("<card=goobers.M2>")
         .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
         {
             ((StatusEffectApplyXWhenDestroyed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon M2");
         })
       );
        statusEffects.Add(
       StatusCopy("Summon Fallow", "Summon M3")
      .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
      .WithTextInsert("<card=goobers.M3>")                                                         //Makes a copy of the Summon Fallow effect
      .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
      {
          ((StatusEffectSummon)data).summonCard = TryGet<CardData>("M3"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                          //This is because TryGet will try to prefix the name with your GUID. 
      })                                                                          //If that fails, then it uses no GUID-prefixing.
       );

        statusEffects.Add(
      StatusCopy("Summon Fallow", "Summon M4")
     .WithText("Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
     .WithTextInsert("<card=goobers.M4>")                                                         //Makes a copy of the Summon Fallow effect
     .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
     {
         ((StatusEffectSummon)data).summonCard = TryGet<CardData>("M4"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                         //This is because TryGet will try to prefix the name with your GUID. 
     })                                                                          //If that fails, then it uses no GUID-prefixing.
      );


        statusEffects.Add(
   StatusCopy("On Hit Equal Heal To FrontAlly", "On Hit Equal Teeth To ???")
  .WithText("Apply <keyword=teeth> to {0} equal to damage dealt")                                       //Since this effect is on Shade Serpent, we modify the description shown.
  .WithTextInsert("<card=goobers.?>")                                                         //Makes a copy of the Summon Fallow effect
  .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
  {
      ((StatusEffectApplyXOnHit)data).effectToApply = TryGet<StatusEffectData>("Teeth"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
      ((StatusEffectApplyXOnHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
      ((StatusEffectApplyXOnHit)data).applyConstraints = new TargetConstraint[]
      {
        new TargetConstraintIsSpecificCard()
        {
            allowedCards = new CardData[]
            {
                TryGet<CardData>("?")
            }
        },
      };
  })                                                                              //If that fails, then it uses no GUID-prefixing.
   );

        statusEffects.Add(
   StatusCopy("On Hit Equal Heal To FrontAlly", "On Hit Equal Teeth To Self")
   .WithText("On hit, gain <keyword=teeth> equal to damage dealt")                                       //Since this effect is on Shade Serpent, we modify the description shown.
   .WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
   .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
   {
       ((StatusEffectApplyXOnHit)data).effectToApply = TryGet<StatusEffectData>("Teeth"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
       ((StatusEffectApplyXOnHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

   })                                                                              //If that fails, then it uses no GUID-prefixing.
   );
        statusEffects.Add(
   StatusCopy("On Hit Equal Heal To FrontAlly", "On Hit Equal Teeth To All Allies")
   .WithText("On hit, Apply <keyword=teeth>  equal to damage dealt to all allies")                                       //Since this effect is on Shade Serpent, we modify the description shown.
   .WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
   .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
   {
       ((StatusEffectApplyXOnHit)data).effectToApply = TryGet<StatusEffectData>("Teeth"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
       ((StatusEffectApplyXOnHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;

   })                                                                              //If that fails, then it uses no GUID-prefixing.
   );
        statusEffects.Add(
StatusCopy("When Health Lost Apply Equal Attack To Self And Allies", "When Health Lost Apply Equal Attack to Self and Allies")
.WithText("Gain <keyword=attack> to Self and Allies equal to amount of <keyword=health> lost")                                       //Since this effect is on Shade Serpent, we modify the description shown.
.WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
{
    ((StatusEffectApplyXWhenHealthLost)data).effectToApply = TryGet<StatusEffectData>("Increase Attack"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too

})                                                                              //If that fails, then it uses no GUID-prefixing.
);
        statusEffects.Add(
  StatusCopy("On Hit Equal Heal To FrontAlly", "On Hit Equal Shell To All Allies in the Row")
  .WithText("Apply <keyword=shell> equal to damage dealt to allies in the row")                                       //Since this effect is on Shade Serpent, we modify the description shown.
  .WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
  .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
  {
      ((StatusEffectApplyXOnHit)data).effectToApply = TryGet<StatusEffectData>("Shell"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
      ((StatusEffectApplyXOnHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;

  })                                                                              //If that fails, then it uses no GUID-prefixing.
  );



        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXOnHit>("On Hit Equal Gold To damage dealt")
              .WithText("Gain <keyword=blings> equal to damage dealt x3")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXOnHit;

                  realData.applyEqualAmount = true;
                  realData.effectToApply = TryGet<StatusEffectData>("Gain Gold");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.equalAmountBonusMult = 2;


              })
      );
        statusEffects.Add(
 StatusCopy("On Hit Equal Heal To FrontAlly", "On Hit Equal Shell To Self")
 .WithText("")                                       //Since this effect is on Shade Serpent, we modify the description shown.
 .WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
 {
     ((StatusEffectApplyXOnHit)data).effectToApply = TryGet<StatusEffectData>("Shell"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
     ((StatusEffectApplyXOnHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

 })                                                                              //If that fails, then it uses no GUID-prefixing.
 );
        statusEffects.Add(
StatusCopy("Reduce Attack", "Reduce AttackN")
.WithText("Reduce <keyword=attack> by <{a}>")                                       //Since this effect is on Shade Serpent, we modify the description shown.
.WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
                                                                            //If that fails, then it uses no GUID-prefixing.
);
        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXOnKill>("Bloodlust")
 .WithStackable(false)
 .WithCanBeBoosted(false)                                         //Not a Lumin Vase target
 .WithText("On kill, trigger again")                     //Changing the text description.
 .WithTextInsert(null) //You must put the GUID in some way here. $"<card={Extensions.PrefixGUID("shadeSerpent",this)}>" works as well here.
 .WithType("")                                                    //Type is typically used for SFX/VFX when applying the status effect. Not necessary as we are not applying this effect during battle, unless you use the "add effect" command.
     .SubscribeToAfterAllBuildEvent(                                  //Finally, declare the ally to be Shade Serpent.
     delegate (StatusEffectData data)
     {
         ((StatusEffectApplyXOnKill)data).effectToApply = TryGet<StatusEffectData>("Trigger");
     })
     );



        //CB23! ALL EFFECTS


        //FOR INSTANT SUMMON 

        statusEffects.Add(
   new StatusEffectDataBuilder(this)
   .Create<StatusEffectInstantCombineCard>("CB23K combo")
   .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
   {
       ((StatusEffectInstantCombineCard)data).cardNames = new string[1] { "goobers.CB23O" };
       ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.CB23K";
       ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
       ((StatusEffectInstantCombineCard)data).changeDeck = true;
       ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
   })
   );
        //THE CONDITIONS
        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedTo>("Perma into CB23K")
          .WithText($"<keyword={Extensions.PrefixGUID("perma", this)}> into <card=goobers.CB23K> when <keyword=sp> reaches 10.")
          .WithCanBeBoosted(false)
          .WithTextInsert("<card=goobers.Sharoco2>")
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedTo;

               realData.effectToApply = TryGet<StatusEffectData>("CB23K combo");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.mustReachAmount = true;
               realData.eventPriority = 2;
               realData.whenAppliedTypes = new string[1] { "sp" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;


               var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
               script.amount = 1;
               ((StatusEffectApplyX)data).scriptableAmount = script;
           }));
        //THE END OF THE PROCESS

        //FOR INSTANT SUMMON 
        statusEffects.Add(
    new StatusEffectDataBuilder(this)
    .Create<StatusEffectInstantCombineCard>("CB23B combo")
    .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
    {
        ((StatusEffectInstantCombineCard)data).cardNames = new string[1] { "goobers.CB23K" };
        ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.CB23B";
        ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
        ((StatusEffectInstantCombineCard)data).changeDeck = true;
        ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
    })
    );

        //THE CONDITIONS
        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedTo>("Perma into CB23B")
          .WithText($"<keyword={Extensions.PrefixGUID("perma", this)}> into <card=goobers.CB23B> when <keyword=sp> reaches 16.")
          .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedTo;

               realData.effectToApply = TryGet<StatusEffectData>("CB23B combo");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.mustReachAmount = true;
               realData.eventPriority = 2;
               realData.whenAppliedTypes = new string[1] { "sp" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;

               var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
               script.amount = 1;
               ((StatusEffectApplyX)data).scriptableAmount = script;
           }));
        //THE END OF THE PROCESS

        //FOR INSTANT SUMMON 
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("CB23KG combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[1] { "goobers.CB23B" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.CB23KG";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
})
);
        //THE CONDITIONS
        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedTo>("Perma into CB23KG")
          .WithText($"<keyword={Extensions.PrefixGUID("perma", this)}> into <card=goobers.CB23KG> when <keyword=sp> reaches 80.")
          .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedTo;

               realData.effectToApply = TryGet<StatusEffectData>("CB23KG combo");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.mustReachAmount = true;
               realData.eventPriority = 2;
               realData.whenAppliedTypes = new string[1] { "sp" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;

               var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
               script.amount = 1;
               ((StatusEffectApplyX)data).scriptableAmount = script;
           }));


        //THE END OF THE PROCESS
        //for king
        statusEffects.Add(
     new StatusEffectDataBuilder(this)
         .Create<StatusEffectApplyXWhenYAppliedTo>("Support more")
         .WithText("Whenever 80 <keyword=sp> is applied to self, apply 2 <keyword=block> to all allies.")
         .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectApplyXWhenYAppliedTo;

              realData.effectToApply = TryGet<StatusEffectData>("Block");
              realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
              realData.mustReachAmount = true;
              realData.eventPriority = 2;
              realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
              realData.count = 1;
              realData.whenAppliedTypes = new string[1] { "sp" };
              realData.whenAppliedToFlags = ApplyToFlags.Self;

              var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
              script.amount = 2;
              ((StatusEffectApplyX)data).scriptableAmount = script;
          }

));
        statusEffects.Add(
    new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXWhenYAppliedTo>("reset")
        .WithCanBeBoosted(false)
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXWhenYAppliedTo;

             realData.effectToApply = TryGet<StatusEffectData>("LEXP");
             realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
             realData.mustReachAmount = true;
             realData.eventPriority = 1;
             realData.whenAppliedTypes = new string[1] { "sp" };
             realData.whenAppliedToFlags = ApplyToFlags.Self;

         }

));
        statusEffects.Add(
 StatusCopy("When Ally Is Healed Apply Equal Spice", "When Ally Is Healed Apply Equal EXP")
 .WithText("When ally is healed, gain equal <keyword=sp> to self.")                                       //Since this effect is on Shade Serpent, we modify the description shown.
 .WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
 {
     ((StatusEffectApplyXWhenAllyHealed)data).effectToApply = TryGet<StatusEffectData>("EXP"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
     ((StatusEffectApplyXWhenAllyHealed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

 })                                                                              //If that fails, then it uses no GUID-prefixing.
 );
        statusEffects.Add(
        StatusCopy("On Hit Equal Heal To FrontAlly", "On Hit Apply Equal EXP ")
        .WithText("Gain <keyword=sp> to self equal to damage dealt.")                                       //Since this effect is on Shade Serpent, we modify the description shown.
        .WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
        .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
        {
            ((StatusEffectApplyXOnHit)data).effectToApply = TryGet<StatusEffectData>("EXP"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
            ((StatusEffectApplyXOnHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

        })                                                                              //If that fails, then it uses no GUID-prefixing.
        );
        //end of king


        //FOR INSTANT SUMMON 
        statusEffects.Add(
  new StatusEffectDataBuilder(this)
  .Create<StatusEffectInstantCombineCard>("CB23R combo")
  .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
  {
      ((StatusEffectInstantCombineCard)data).cardNames = new string[1] { "goobers.CB23K" };
      ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.CB23R";
      ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
      ((StatusEffectInstantCombineCard)data).changeDeck = true;
      ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
  })
  );

        //THE CONDITIONS
        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedTo>("Perma into CB23R")
          .WithText($"<keyword={Extensions.PrefixGUID("perma", this)}> into <card=goobers.CB23R> when <keyword=spice> reaches 20.")
          .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedTo;

               realData.effectToApply = TryGet<StatusEffectData>("CB23R combo");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.mustReachAmount = true;
               realData.eventPriority = 2;
               realData.whenAppliedTypes = new string[1] { "spice" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;

               var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
               script.amount = 1;
               ((StatusEffectApplyX)data).scriptableAmount = script;
           }));
        //THE END OF THE PROCESS
        //FOR INSTANT SUMMON 

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectInstantCombineCard>("CB23Q combo")
        .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
        {
            ((StatusEffectInstantCombineCard)data).cardNames = new string[1] { "goobers.CB23R" };
            ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.CB23Q";
            ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
            ((StatusEffectInstantCombineCard)data).changeDeck = true;
            ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
        })
        );

        //THE CONDITIONS
        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedTo>("Perma into CB23Q")
          .WithText($"<keyword={Extensions.PrefixGUID("perma", this)}> into <card=goobers.CB23Q> when <keyword=sp> reaches 180.")
          .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedTo;

               realData.effectToApply = TryGet<StatusEffectData>("CB23Q combo");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.mustReachAmount = true;
               realData.eventPriority = 2;
               realData.whenAppliedTypes = new string[1] { "sp" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;

               var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
               script.amount = 1;
               ((StatusEffectApplyX)data).scriptableAmount = script;
           }));
        //THE END OF THE PROCESS
        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedTo>("Double up when sweet points")
          .WithText("Double this card's attack when <keyword=sp> reaches {a}.")
          .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedTo;

               realData.effectToApply = TryGet<StatusEffectData>("Double Attack");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.mustReachAmount = true;
               realData.targetMustBeAlive = false;
               realData.eventPriority = 2;
               realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
               realData.whenAppliedTypes = new string[1] { "sp" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;
           }));


        statusEffects.Add(
    new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXEveryTurn>("Woo woo")
        .WithText("When turn ends, count down a random ally's <keyword=counter> by <{a}>.")
        .WithStackable(true)
        .WithCanBeBoosted(true)
        .WithTextInsert("")
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXEveryTurn;


             realData.effectToApply = TryGet<StatusEffectData>("Reduce Counter");
             realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;
        
         
         }
             ));

        statusEffects.Add(
   new StatusEffectDataBuilder(this)
       .Create<StatusEffectApplyXEveryTurn>("Woo Hoo")
       .WithText("When turn ends, apply <{a}> <keyword=spice> to a random ally.")
       .WithStackable(true)
       .WithCanBeBoosted(true)
       .WithTextInsert("")
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXEveryTurn;


            realData.effectToApply = TryGet<StatusEffectData>("Spice");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;


        }
            ));

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyXEveryTurn>("Boo Hoo")
     .WithText("When turn ends, apply <{a}> <keyword=frost> to a random enemy.")
     .WithStackable(true)
     .WithCanBeBoosted(true)
     .WithTextInsert("")
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyXEveryTurn;


          realData.effectToApply = TryGet<StatusEffectData>("Frost");
          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;


      }
          ));

        //CANDY EFFECTS


        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXPostAttack>("Apply snow equal to SP")
          .WithText("Apply <keyword=snow> equal to <keyword=sp>.")
          .WithCanBeBoosted(false)
          .WithTextInsert("")
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXPostAttack;


               realData.effectToApply = TryGet<StatusEffectData>("Snow");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
               var script = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
               script.statusType = "sp";
               realData.eventPriority = 2;
               ((StatusEffectApplyXPostAttack)data).scriptableAmount = script;
           }
               ));

        statusEffects.Add(StatusCopy("When Spice Or Shell Applied To Self Shroom To RandomEnemy", "Convert snow to Sweetpoints")
.WithText("When <keyword=snow> is applied to this unit, gain <keyword=sp> instead.")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var se = data as StatusEffectApplyXWhenYAppliedTo;
    se.whenAppliedTypes = new string[] { "snow" };
    se.whenAppliedToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    se.effectToApply = TryGet<StatusEffectData>("EXP");
    se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    se.instead = true;
    se.targetMustBeAlive = false;
}));

        statusEffects.Add(
           new StatusEffectDataBuilder(this)
               .Create<StatusEffectApplyXWhenYAppliedTo>("Trigger Ally when SP")
               .WithText("When <keyword=sp> reaches {a} trigger ally ahead.")
               .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyXWhenYAppliedTo;

                    realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
                    realData.mustReachAmount = true;
                    realData.eventPriority = 2;
                    realData.whenAppliedTypes = new string[1] { "sp" };
                    realData.whenAppliedToFlags = ApplyToFlags.Self;
                    realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                    var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                    script.amount = 1;
                    ((StatusEffectApplyX)data).scriptableAmount = script;
                })
                );


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenYAppliedTo>("Apply await when SP")
              .WithText("When <keyword=sp> reaches {a} apply <keyword=goobers.await> to ally in front.")
              .WithCanBeBoosted(false)
               .SubscribeToAfterAllBuildEvent(data =>
               {
                   var realData = data as StatusEffectApplyXWhenYAppliedTo;

                   realData.effectToApply = TryGet<StatusEffectData>("Temporary Await");
                   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
                   realData.mustReachAmount = true;
                   realData.eventPriority = 2;
                   realData.whenAppliedTypes = new string[1] { "sp" };
                   realData.whenAppliedToFlags = ApplyToFlags.Self;
                   realData.scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                   var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
                   script.amount = 1;
                   ((StatusEffectApplyX)data).scriptableAmount = script;
               })
               );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Berry")
  .SubscribeToAfterAllBuildEvent(data =>
  {
      (data as StatusEffectSummon).summonCard = TryGet<CardData>("Berry");
  })
  );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Berry")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Berry") as StatusEffectSummon;
                })
        );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Odd")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     (data as StatusEffectSummon).summonCard = TryGet<CardData>("Odd");
 })
 );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Odd")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Odd") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Sugary")
.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Sugary");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Sugary")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Sugary") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Blood")
.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Blood");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Blood")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Blood") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
      .Create<StatusEffectInstantAddDeck>("Add Blood")
      .WithStackable(false)
      .WithCanBeBoosted(false)
      .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
      {
          ((StatusEffectInstantAddDeck)data).card = Get<CardData>("Blood");

      })
      );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantMultiple>("Gain Blood")
.WithText("")
.WithCanBeBoosted(false)
.WithTextInsert("=")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     var realData = data as StatusEffectInstantMultiple;

     realData.effects = new StatusEffectInstant[]
 {
     TryGet<StatusEffectInstant>("Instant Summon Blood"),
     TryGet<StatusEffectInstant>("Add Blood"),
     TryGet<StatusEffectInstant>("Remove")
 };
 }

     ));
        statusEffects.Add(
          new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXOnCardPlayed>("Gain Blood Now")
              .WithCanBeBoosted(false)
               .SubscribeToAfterAllBuildEvent(data =>
               {
                   var realData = data as StatusEffectApplyXOnCardPlayed;

                   realData.effectToApply = TryGet<StatusEffectData>("Gain Blood");
                   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

               })
               );
        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyRandomOnCardPlayed>("Random ingrediant")
          .WithText("Gain <{a}> random <keyword=goobers.minorin> in your hand.")
           .WithTextInsert("<card=goobers.YraB1>,<card=goobers.YraB2>,<card=goobers.YraB3>")
          .WithStackable(true)
          .WithCanBeBoosted(true)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectApplyRandomOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
              ((StatusEffectApplyRandomOnCardPlayed)data).effectsToapply = new StatusEffectData[]
              {
                   Get<StatusEffectData>("Gain Berry"),
                   Get<StatusEffectData>("Gain Sugary"),
                   Get<StatusEffectData>("Gain Odd"),
              };
          })
          );
        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add Berry")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("Berry");

          })
          );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantMultiple>("Gain Berry")
 .WithText("")
 .WithCanBeBoosted(false)
 .WithTextInsert("=")
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectInstantMultiple;

      realData.effects = new StatusEffectInstant[]
  {
     TryGet<StatusEffectInstant>("Instant Summon Berry"),
     TryGet<StatusEffectInstant>("Add Berry")
  };
  }

      ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add Sugary")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("Sugary");

          })
          );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantMultiple>("Gain Sugary")
 .WithText("")
 .WithCanBeBoosted(false)
 .WithTextInsert("=")
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectInstantMultiple;

      realData.effects = new StatusEffectInstant[]
  {
     TryGet<StatusEffectInstant>("Instant Summon Sugary"),
     TryGet<StatusEffectInstant>("Add Sugary")
  };
  }

      ));

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
      .Create<StatusEffectInstantAddDeck>("Add Odd")
      .WithStackable(false)
      .WithCanBeBoosted(false)
      .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
      {
          ((StatusEffectInstantAddDeck)data).card = Get<CardData>("Odd");

      })
      );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantMultiple>("Gain Odd")
 .WithText("")
 .WithCanBeBoosted(false)
 .WithTextInsert("=")
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectInstantMultiple;

      realData.effects = new StatusEffectInstant[]
  {
     TryGet<StatusEffectInstant>("Instant Summon Odd"),
     TryGet<StatusEffectInstant>("Add Odd")
  };
  }

      ));

 
        statusEffects.Add(
    new StatusEffectDataBuilder(this)
    .Create<StatusEffectInstantCombineCard>("JamKnife combo")
    .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
    {
        ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Bknife", "goobers.Berry" };
        ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.JamKnife";
        ((StatusEffectInstantCombineCard)data).spawnOnBoard = false;
        ((StatusEffectInstantCombineCard)data).checkDeck = false;
        ((StatusEffectInstantCombineCard)data).checkBoard = false;
        ((StatusEffectInstantCombineCard)data).changeDeck = true;
        ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
    })
    );

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantCombineCard>("RushKnife combo")
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
 {
     ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Bknife", "goobers.Sugary" };
     ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.RushKnife";
     ((StatusEffectInstantCombineCard)data).spawnOnBoard = false;
     ((StatusEffectInstantCombineCard)data).checkDeck = false;
     ((StatusEffectInstantCombineCard)data).checkBoard = false;
     ((StatusEffectInstantCombineCard)data).changeDeck = true;
     ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
 })
 );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("OddKnife combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Bknife", "goobers.Odd" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.OddKnife";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = false;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = false;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("BloodKnife combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Bknife", "goobers.Blood" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.BloodKnife";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = false;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = false;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;

})
);
        statusEffects.Add(
   new StatusEffectDataBuilder(this)
               .Create<StatusEffectApplyXWhenHit>("BloodKnife Act")
               .WithStackable(false)
               .WithCanBeBoosted(false)
               .SubscribeToAfterAllBuildEvent(data =>
               {
                   var realData = data as StatusEffectApplyXWhenHit;

                   realData.eventPriority = 999999999;
                   realData.targetMustBeAlive = false;
                   realData.effectToApply = TryGet<StatusEffectData>("BloodKnife combo");
                   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                   realData.attackerConstraints = new[]
                   {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Blood")
                            }
                        }
                   };
               })
);
        statusEffects.Add(
    new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXWhenHit>("JamKnife Act")
                .WithStackable(false)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyXWhenHit;

                    realData.eventPriority = 999999999;
                    realData.targetMustBeAlive = false;
                    realData.effectToApply = TryGet<StatusEffectData>("JamKnife combo");
                    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    realData.attackerConstraints = new[]
                    {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Berry")
                            }
                        }
                    };
                })
);
        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("RushKnife Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("RushKnife combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Sugary")
                            }
                        }
                  };
              })
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXWhenHit>("OddKnife Act")
           .WithStackable(false)
           .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenHit;

               realData.eventPriority = 999999999;
               realData.targetMustBeAlive = false;
               realData.effectToApply = TryGet<StatusEffectData>("OddKnife combo");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.attackerConstraints = new[]
               {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Odd")
                            }
                        }
               };
           })
);
        statusEffects.Add(
        StatusCopy("On Hit Equal Heal To FrontAlly", "On Hit Equal Max Health To FrontAlly")
        .WithText("Increase max <keyword=health> equal to damage dealt to ally in front.")                                       //Since this effect is on Shade Serpent, we modify the description shown.
        .WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
        .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
        {
            ((StatusEffectApplyXOnHit)data).effectToApply = TryGet<StatusEffectData>("Increase Max Health");

        })                                                                              //If that fails, then it uses no GUID-prefixing.
        );

        //CAY KE EFFECTS
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Sweetcake")
.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("SweetCake");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Sweetcake")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Sweetcake") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenYAppliedToCustom>("Add Sweetcake")
            .WithText("When <keyword=sp> reaches 25, add <{a}> <card=goobers.SweetCake> to your hand.")
            .WithStackable(true)
            .WithCanBeBoosted(true)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

                 realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Sweetcake");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.requiredAmount = 25;
                 realData.eventPriority = 5;
                 realData.whenAppliedTypes = new string[1] { "sp" };
                 realData.whenAppliedToFlags = ApplyToFlags.Self;

             })
             );





        statusEffects.Add(
      StatusCopy("Summon Junk", "Summon Berry Cake")
      .SubscribeToAfterAllBuildEvent(data =>
      {
          (data as StatusEffectSummon).summonCard = TryGet<CardData>("Berry Cake");
      })
      );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Berry Cake")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Berry Cake") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenYAppliedToCustom>("Add Berry Cake")
            .WithText("<keyword=sp> required 25 - add <{a}> <card=goobers.Berry Cake> to your hand.")
            .WithStackable(true)
            .WithCanBeBoosted(true)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

                 realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Berry Cake");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.requiredAmount = 25;
                 realData.eventPriority = 5;
                 realData.whenAppliedTypes = new string[1] { "sp" };
                 realData.whenAppliedToFlags = ApplyToFlags.Self;


             })
             );


        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Sugar Cake")
.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Sugar Cake");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Sugar Cake")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Sugar Cake") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenYAppliedToCustom>("Add Sugar Cake")
            .WithText("<keyword=sp> required 30 - add 1 <card=goobers.Sugar Cake> to your hand.")
               .WithStackable(true)
            .WithCanBeBoosted(true)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

                 realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Sugar Cake");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.requiredAmount = 30;
                 realData.eventPriority = 5;
                 realData.whenAppliedTypes = new string[1] { "sp" };
                 realData.whenAppliedToFlags = ApplyToFlags.Self;


             })
             );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Bizzare Cake")
.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Bizzare Cake");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Bizzare Cake")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Bizzare Cake") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenYAppliedToCustom>("Add Bizzare Cake")
            .WithText("<keyword=sp> required 20 - add <{a}> <card=goobers.Bizzare Cake> to your hand.")
            .WithStackable(true)
            .WithCanBeBoosted(true)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

                 realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Bizzare Cake");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.requiredAmount = 20;
                 realData.eventPriority = 5;
                 realData.whenAppliedTypes = new string[1] { "sp" };
                 realData.whenAppliedToFlags = ApplyToFlags.Self;


             })
             );

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyRandomOnCardPlayed>("Random Buff")
          .WithText("Apply <{a}> either, <keyword=block>/<keyword=attack>/Increase Effect/<keyword=health>/<keyword=shell>/<keyword=spice>")
          .WithStackable(true)
          .WithCanBeBoosted(true)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectApplyRandomOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
              ((StatusEffectApplyRandomOnCardPlayed)data).effectsToapply = new StatusEffectData[]
              {
                   Get<StatusEffectData>("Block"),Get<StatusEffectData>("Increase Attack"),Get<StatusEffectData>("Increase Effects"),
                   Get<StatusEffectData>("Shell"),Get<StatusEffectData>("Increase Max Health"),
                   Get<StatusEffectData>("Spice")

              };
          }
          ));


        statusEffects.Add(
  new StatusEffectDataBuilder(this)
  .Create<StatusEffectInstantCombineCard>("Straw Kay combo")
  .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
  {
      ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Cake", "goobers.Berry" };
      ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Straw Kay";
      ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
      ((StatusEffectInstantCombineCard)data).checkDeck = false;
      ((StatusEffectInstantCombineCard)data).checkBoard = true;
      ((StatusEffectInstantCombineCard)data).changeDeck = true;
      ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
  })
  );

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantCombineCard>("Sugar Kay combo")
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
 {
     ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Cake", "goobers.Sugary" };
     ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Hyper Kay";
     ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
     ((StatusEffectInstantCombineCard)data).checkDeck = false;
     ((StatusEffectInstantCombineCard)data).checkBoard = true;
     ((StatusEffectInstantCombineCard)data).changeDeck = true;
     ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
 })
 );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Odd Kay combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Cake", "goobers.Odd" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Odd Kay";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Blood Kay combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Cake", "goobers.Blood" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Blood Kay";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenHit>("Straw Kay Act")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenHit;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("Straw Kay combo");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                          realData.attackerConstraints = new[]
                          {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Berry")
                            }
                        }
                          };
                      }));



        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("Hyper Kay Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Sugar Kay combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Sugary")
                            }
                        }
                  };
              }));

        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("Odd Kay Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Odd Kay combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Odd")
                            }
                        }
                  };
              }));

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
             .Create<StatusEffectApplyXWhenHit>("Blood Kay Act")
             .WithStackable(false)
             .WithCanBeBoosted(false)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenHit;

                 realData.eventPriority = 999999999;
                 realData.targetMustBeAlive = false;
                 realData.effectToApply = TryGet<StatusEffectData>("Blood Kay combo");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.attackerConstraints = new[]
                 {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Blood")
                            }
                        }
                 };
             }));
        statusEffects.Add(
       new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXOnCardPlayed>("Remove it")
           .WithText("Reduce Max <keyword=health> by <{a}>.")
           .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectApplyXOnCardPlayed;

                realData.effectToApply = TryGet<StatusEffectData>("Reduce Max Health");
                realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;

            })
            );


        statusEffects.Add(
      StatusCopy("Temporary Pigheaded", "Temporary Grand Maid")
     //Makes a copy of the Summon Fallow effect
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

         (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("GrandMaid");   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too

     })
     );
        statusEffects.Add(
     StatusCopy("Temporary Pigheaded", "Temporary Maid")
    //Makes a copy of the Summon Fallow effect
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

        (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("Maid");   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too

    })
    );

        statusEffects.Add(
StatusCopy("Temporary Pigheaded", "Temporary Draw")
//Makes a copy of the Summon Fallow effect
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

    (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("Draw");   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too

})
);

      

        statusEffects.Add(
 StatusCopy("Temporary Pigheaded", "Temporary Caketrait")
//Makes a copy of the Summon Fallow effect
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

    (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("Caketrait");   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too

})
);
       
        statusEffects.Add(
 StatusCopy("Temporary Pigheaded", "Temporary Heartburn")
//Makes a copy of the Summon Fallow effect
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

    (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("Heartburn");   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too

})
);


        statusEffects.Add(
 StatusCopy("Temporary Pigheaded", "Temporary Explode")
//Makes a copy of the Summon Fallow effect
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

    (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("Explode");   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too

})
);




        statusEffects.Add(
new StatusEffectDataBuilder(this)
   .Create<StatusEffectWhileActiveX>("Heartburn all row")
   .WithText("While active, all allies in the row gain <keyword=heartburn>.")
   .WithStackable(false)
   .WithCanBeBoosted(true)
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectWhileActiveX;

       realData.eventPriority = 1;
       realData.targetMustBeAlive = true;
       realData.effectToApply = TryGet<StatusEffectData>("Temporary Heartburn");
       realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
   }));


        statusEffects.Add(
    StatusCopy("Temporary Pigheaded", "Temporary Hexplosion")
   //Makes a copy of the Summon Fallow effect
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

       (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("HeavyExplosion");   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too

   })
   ); 

                   statusEffects.Add(
    StatusCopy("Temporary Pigheaded", "Temporary Friendplode")
   //Makes a copy of the Summon Fallow effect
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

       (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("Friendplode");   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too

   })
   ); 

        statusEffects.Add(
    StatusCopy("Temporary Noomlin", "Temporary Await")
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

       (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("Await");
    
   }
   ));

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXPreTrigger>("Apply Await")
          .WithText("Apply <keyword=goobers.await> to ally in front.")
          .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXPreTrigger;

               realData.effectToApply = TryGet<StatusEffectData>("Temporary Await");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;

           })
           );
        statusEffects.Add(
       new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXOnCardPlayed>("Apply Maid")
           .WithText("Apply <{a}> <keyword=goobers.maid> to ally in front.")
           .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectApplyXOnCardPlayed;

                realData.effectToApply = TryGet<StatusEffectData>("Temporary Maid");
                realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;

            })
            );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Blood Cake")
.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Blood Cake");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Blood Cake")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Blood Cake") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyXOnTurn>("Get ritual cake")
     .WithText("Gain <{a}> <card=goobers.Blood Cake> to your hand.")
     .WithStackable(true)
     .WithCanBeBoosted(true)
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyXOnTurn;

          realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Blood Cake");
          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

      })
      );
        //END OF CA KAY



        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Muf")
.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Muf");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Muf")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Muf") as StatusEffectSummon;
                })
        );

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyXOnTurn>("Get Muf")
     .WithText("Gain <{a}> <card=goobers.Muf> to your hand.")
     .WithStackable(true)
     .WithCanBeBoosted(true)
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyXOnTurn;

          realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Muf");
          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

      })
      );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXOnCardPlayed>("Double up")
    .WithText("Double <keyword=attack>")

    .WithCanBeBoosted(false)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXOnCardPlayed;

         realData.effectToApply = TryGet<StatusEffectData>("Double Attack");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

     })
     );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDrawn>("Hit me")
.WithText("<keyword=goobers.drawa> <{a}>")
.WithStackable(true)
.WithCanBeBoosted(true)
 .SubscribeToAfterAllBuildEvent(data =>
 {
     var realData = data as StatusEffectApplyXWhenDrawn;

     realData.effectToApply = TryGet<StatusEffectData>("Instant Draw");
     realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

 })
 );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
            .Create<StatusEffectWhileActiveX>("Maid")
            .WithStackable(true)
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectWhileActiveX;

                realData.eventPriority = 1;
                realData.targetMustBeAlive = true;
                realData.effectToApply = TryGet<StatusEffectData>("Ongoing Increase Attack");
                realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies| ApplyToFlags.Enemies;

                realData.applyConstraints = new[]
                {

                        new TargetConstraintHasTrait()
                        {
                            trait = Get<TraitData>("Maid")

                        }
                };
            }));

        


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Sweet Start")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed;

    realData.effectToApply = TryGet<StatusEffectData>("EXP");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

})
);

        statusEffects.Add(new StatusEffectDataBuilder(this)
    .Create<StatusEffectWhileActiveXWithBoostableScriptableAmount>("Grand Maid")
    .WithStackable(true)
    .WithCanBeBoosted(true)
    .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
    {
        var se = data as StatusEffectWhileActiveXWithBoostableScriptableAmount;
        se.scriptableAmount = new ScriptableAlliesWithTrait() { allies = true, trait = Get<TraitData>("Maid") };
        se.effectToApply = Get<StatusEffectData>("Ongoing Increase Attack");
        se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
        se.eventPriority = 100;

    }));
       


        statusEffects.Add(
 new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyXWhenYAppliedToCustom>("Gain maid when")
     .WithText("<keyword=sp> required 15 - gain <{a}> <keyword=goobers.gmaid>.")
     .WithStackable(true)
     .WithCanBeBoosted(true)
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

          realData.effectToApply = TryGet<StatusEffectData>("Temporary Grand Maid");
          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
          realData.requiredAmount = 15;
          realData.eventPriority = 5;
          realData.whenAppliedTypes = new string[1] { "sp" };
          realData.whenAppliedToFlags = ApplyToFlags.Self;


      })
      );

        statusEffects.Add(new StatusEffectDataBuilder(this)
  .Create<StatusEffectApplyXWhenHealed>("Maid equal HP")
  .WithText("Gain <keyword=goobers.maid> equal to the amount of <keyword=health> healed.")
  .WithCanBeBoosted(false)
  .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
  {
      var se = data as StatusEffectApplyXWhenHealed;
      se.applyEqualAmount = true;
      se.effectToApply = Get<StatusEffectData>("Temporary Maid");
      se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  }));


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Heal self")
.WithText("Heal <{a}> <keyword=health>")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var se = data as StatusEffectApplyXOnTurn;
    se.effectToApply = Get<StatusEffectData>("Heal");
    se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
}));



        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnHit>("Heal Equal2")
.WithText("Heal allies in the row equal to damage dealt.")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var se = data as StatusEffectApplyXOnHit;
    se.applyEqualAmount = new ScriptableCurrentAttack();
    se.effectToApply = Get<StatusEffectData>("Heal");
    se.applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
}));



        statusEffects.Add(
  new StatusEffectDataBuilder(this)
      .Create<StatusEffectApplyXWhenYAppliedToCustom>("When SP ally behind frenzy")
      .WithText("<keyword=sp> required 25 - apply <{a}> <keyword=frenzy> to the ally behind.")
      .WithStackable(true)
      .WithCanBeBoosted(true)
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

           realData.effectToApply = TryGet<StatusEffectData>("MultiHit");
           realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
           realData.requiredAmount = 25;
           realData.eventPriority = 5;
           realData.whenAppliedTypes = new string[1] { "sp" };
           realData.whenAppliedToFlags = ApplyToFlags.Self;

       })
       );



        statusEffects.Add(
  new StatusEffectDataBuilder(this)
      .Create<StatusEffectApplyXWhenItemPlayed>("Cunning")
      .WithCanBeBoosted(false)
      .WithIsReaction(true)
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectApplyXWhenItemPlayed;

           realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
           realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
           realData.eventPriority = 5;


       })
       );

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedToCustom>("Apply Maid when")
          .WithText("<keyword=sp> required 20 - add <{a}> <keyword=goobers.maid> to ally ahead.")
          .WithStackable(true)
          .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

               realData.effectToApply = TryGet<StatusEffectData>("Temporary Maid");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
               realData.requiredAmount = 20;
               realData.eventPriority = 5;
               realData.whenAppliedTypes = new string[1] { "sp" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;

           })
           );

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenAllyIsKilled>("Block All")
          .WithText("When an ally is killed, apply <{a}> <keyword=block> to all allies.")
          .WithStackable(true)
          .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenAllyIsKilled;

               realData.effectToApply = TryGet<StatusEffectData>("Block");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;

               realData.eventPriority = 5;


           })
           );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
   .Create<StatusEffectApplyXOnCardPlayed>("Block Alli")
   .WithText("Apply <{a}> <keyword=block> to all allies.")
   .WithStackable(true)
   .WithCanBeBoosted(true)
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var realData = data as StatusEffectApplyXOnCardPlayed;

        realData.effectToApply = TryGet<StatusEffectData>("Block");
        realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;

        realData.eventPriority = 5;


    })
    );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Madeberry combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Made", "goobers.Berry" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Madeberry";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
})
);

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantCombineCard>("Madesugar combo")
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
 {
     ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Made", "goobers.Sugary" };
     ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Madesugar";
     ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
     ((StatusEffectInstantCombineCard)data).checkDeck = false;
     ((StatusEffectInstantCombineCard)data).checkBoard = true;
     ((StatusEffectInstantCombineCard)data).changeDeck = true;
     ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
 })
 );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Madeodd combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Made", "goobers.Odd" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Madeodd";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Madeblood combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Made", "goobers.Blood" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Madeblood";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenHit>("Berrymade Act")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenHit;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("Madeberry combo");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                          realData.attackerConstraints = new[]
                          {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Berry")
                            }
                        }
                          };
                      }));



        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("Sugarmade Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Madesugar combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Sugary")
                            }
                        }
                  };
              }));

        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("Oddmade Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Madeodd combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Odd")
                            }
                        }
                  };
              }));

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
             .Create<StatusEffectApplyXWhenHit>("Bloodmade Act")
             .WithStackable(false)
             .WithCanBeBoosted(false)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenHit;

                 realData.eventPriority = 999999999;
                 realData.targetMustBeAlive = false;
                 realData.effectToApply = TryGet<StatusEffectData>("Madeblood combo");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.attackerConstraints = new[]
                 {

                 new TargetConstraintIsSpecificCard()
                 {
                     allowedCards = new CardData[]
                        {
                            Get<CardData>("Blood")
                        }
                 }
             };
             }));



        statusEffects.Add(
new StatusEffectDataBuilder(this)
       .Create<StatusEffectWhileActiveX>("While active decrease all enemies")
       .WithText("While active, reduce  <keyword=attack> by <{a}> to enemies in the row")
       .WithStackable(true)
       .WithCanBeBoosted(true)
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectWhileActiveX;

           realData.eventPriority = 1;
           realData.targetMustBeAlive = true;
           realData.effectToApply = TryGet<StatusEffectData>("Ongoing Reduce Attack");
           realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
       }));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
      .Create<StatusEffectApplyXOnTurn>("Increase Effects To FrontAlly")
      .WithText("Increase the effects of the ally ahead by <{a}>")
      .WithStackable(true)
      .WithCanBeBoosted(true)
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyXOnTurn;

          realData.eventPriority = 1;
          realData.targetMustBeAlive = true;
          realData.effectToApply = TryGet<StatusEffectData>("Increase Effects");
          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
      }));



        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Blobery")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Blobery");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Blobery")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Blobery") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Blobery");
                      })
                      );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Strobery")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Strobery");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Strobery")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Strobery") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Strobery");
                      })
                      );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Chocobery")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Chocobery");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Chocobery")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Chocobery") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Chocobery");
                      })
                      );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Blabery")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Blabery");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Blabery")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Blabery") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Blabery");
                      })
                      );

        statusEffects.Add(
     new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyRandomOnCardPlayed>("Random maid")
     .WithText("Gain a random Shorcake <keyword=goobers.maid> card in your hand. <keyword=goobers.loadout>")
     .WithStackable(false)
     .WithCanBeBoosted(false)
     .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
     {
         ((StatusEffectApplyRandomOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         ((StatusEffectApplyRandomOnCardPlayed)data).effectsToapply = new StatusEffectData[]
         {
                   Get<StatusEffectData>("Instant Summon Blabery"),Get<StatusEffectData>("Instant Summon Chocobery"),Get<StatusEffectData>("Instant Summon Strobery"),
                   Get<StatusEffectData>("Instant Summon Blobery")


         };
     }
     ));


        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenCardDestroyed>("Trigger Self when card destroyed")
          .WithCanBeBoosted(false)
           .WithIsReaction(true)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenCardDestroyed;

               realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.targetMustBeAlive = false;

           }
               ));


        statusEffects.Add(
StatusCopy("When Enemy (Shroomed) Is Killed Apply Their Shroom To RandomEnemy", "When Enemyt Killed but nom")
.WithText("When a <keyword=weakness>'d enemy dies, apply their <keyword=weakness> to a random enemy,")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Weakness");
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).contextEqualAmount = new ScriptableCurrentStatus() { statusType = Get<StatusEffectData>("Weakness").type };
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

                 new TargetConstraintHasStatus()
                 {
            status = TryGet<StatusEffectData>("Weakness")
        }
             };
})
);
        statusEffects.Add(
StatusCopy("When Enemy (Shroomed) Is Killed Apply Their Shroom To RandomEnemy", "Gain SP when bom dies")
.WithText("then gain <{a}> <keyword=sp>.")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("EXP");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).applyEqualAmount = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

                 new TargetConstraintHasStatus()
                 {
            status = TryGet<StatusEffectData>("Weakness")
        }
            };
})
);

           statusEffects.Add(
StatusCopy("When Enemy (Shroomed) Is Killed Apply Their Shroom To RandomEnemy", "Gain SP when bom dies2")
.WithText("Gain <{a}> <keyword=sp>, when an enemy with <keyword=weakness> is killed.")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("EXP");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).applyEqualAmount = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

                 new TargetConstraintHasStatus()
                 {
            status = TryGet<StatusEffectData>("Weakness")
        }
            };
})
);
        statusEffects.Add(
   new StatusEffectDataBuilder(this)
       .Create<StatusEffectApplyXWhenYAppliedToCustom>("MORE BOM")
       .WithText("<keyword=sp> required 14 - add <{a}> <keyword=weakness> to enemies in the row.")
       .WithStackable(true)
       .WithCanBeBoosted(true)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

            realData.effectToApply = TryGet<StatusEffectData>("Weakness");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
            realData.requiredAmount = 14;
            realData.eventPriority = 5;
            realData.whenAppliedTypes = new string[1] { "sp" };
            realData.whenAppliedToFlags = ApplyToFlags.Self;

        })
        );

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyXWhenYAppliedToCustom>("MORE BOM2")
     .WithText("<keyword=sp> required 50 - add <{a}> <keyword=weakness> to enemies in the row.")
     .WithStackable(true)
     .WithCanBeBoosted(true)
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

          realData.effectToApply = TryGet<StatusEffectData>("Weakness");
          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
          realData.requiredAmount = 50;
          realData.eventPriority = 5;
          realData.whenAppliedTypes = new string[1] { "sp" };
          realData.whenAppliedToFlags = ApplyToFlags.Self;

      })
      );
        //CANDY EFFECTS 2





        statusEffects.Add(
new StatusEffectDataBuilder(this) 
       .Create<StatusEffectApplyXOnHit>("Target RAAH")
       .WithStackable(true)
       .WithCanBeBoosted(true)
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectApplyXOnHit;

           realData.eventPriority = 999;
           realData.targetMustBeAlive = false;
           realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Ally");
           realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
           realData.applyConstraints = new[] { new TargetConstraintHealthLessThan(){ value = 20 } };
       }));


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("Gain Expresso for every maid")
.WithText("Gain <keyword=frenzy> for every ally <keyword=goobers.maid> on the board.")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectWhileActiveX;
    realData.scriptableAmount = new ScriptableAlliesWithTrait()  { allies = true, trait = TryGet<TraitData>("Maid") };
    realData.effectToApply = Get<StatusEffectData>("MultiHit");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;



})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDestroyed>("Heavy Explsion")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDestroyed;

    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
    realData.dealDamage = true;
    realData.doesDamage = true;
    realData.countsAsHit = true;
    realData.targetMustBeAlive = false;

})
);
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDestroyed>("Friend Heavy Explsion")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDestroyed;

realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
realData.dealDamage = true;
realData.doesDamage = true;
realData.countsAsHit = true;
realData.targetMustBeAlive = false;

})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Heavy Explsion Plus")
.WithText("Gain <{a}> <keyword=goobers.hexplosion>.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXOnTurn;

    realData.effectToApply = TryGet<StatusEffectData>("Temporary Hexplosion");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
 

})
);



        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Bahanna Split combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
   ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Banana", "goobers.Vanillog", "goobers.Chocolog", "goobers.Strawberilog" };
   ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Bahanna Split";
   ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
   ((StatusEffectInstantCombineCard)data).checkDeck = true;
   ((StatusEffectInstantCombineCard)data).checkBoard = true;
   ((StatusEffectInstantCombineCard)data).changeDeck = true;
   ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenHit>("Banana Split Act")
                      .WithText("<keyword=goobers.bananasplit>")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenHit;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("Bahanna Split combo");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                          realData.attackerConstraints = new[]
                          {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Fuse")
                            }
                        }
                          };
                      }));





        statusEffects.Add(
        StatusCopy("When Deployed Summon Wowee", "When Deployed Summon Fixed Coffee")
       .WithText("When deployed Summon {0}")                                       //Since this effect is on Shade Serpent, we modify the description shown.
       .WithTextInsert("<card=goobers.Fixed Coffee>")
        .WithCanBeBoosted(false)//Makes a copy of the Summon Fallow effect
       .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
       {
           ((StatusEffectApplyXWhenDeployed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Coffee");
           ((StatusEffectApplyXWhenDeployed)data).applyToFlags = ApplyToFlags.Self;
           ((StatusEffectApplyXWhenDeployed)data).applyConstraints = new TargetConstraint[]
      {
        new TargetConstraintOnBoard()

      };

           //This is because TryGet will try to prefix the name with your GUID. 
       })                                                                          //If that fails, then it uses no GUID-prefixing.

 );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Ascended Coffee")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Fixed Coffee");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Coffee")
                     .WithText("...")
                     .WithCanBeBoosted(false)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Ascended Coffee") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Fixed Coffee");

                      })
                      );



        statusEffects.Add(StatusCopy("When Spice Or Shell Applied To Self Shroom To RandomEnemy", "Sweet Points into Attack")
  .WithStackable(false)
  .WithCanBeBoosted(false)
  .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
  {
      var se = data as StatusEffectApplyXWhenYAppliedTo;
      se.textKey = null; 
      se.whenAppliedTypes = new string[] { "sp" };
      se.whenAppliedToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      se.effectToApply = Get<StatusEffectData>("Spice");
      se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      se.instead = true;
      se.targetMustBeAlive = false;
      se.eventPriority = 50;
  }));

        statusEffects.Add(StatusCopy("When Spice Or Shell Applied To Self Shroom To RandomEnemy", "Sweet Points into Health")
.WithText("")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var se = data as StatusEffectApplyXWhenYAppliedTo;
    se.textKey = null;
    se.whenAppliedTypes = new string[] { "sp" };
se.whenAppliedToFlags = StatusEffectApplyX.ApplyToFlags.Self;
se.effectToApply = Get<StatusEffectData>("Shell");
se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
se.instead = true;
se.targetMustBeAlive = false;
    se.eventPriority = 100;
}));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHit>("Heal allies when hit")
.WithText("When hit, restore allies <keyword=health> equal to damage taken.")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenHit;

    realData.applyEqualAmount = true;
realData.effectToApply = TryGet<StatusEffectData>("Heal");
realData.eventPriority = 5;
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDestroyed>("Momo not spooked")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenDestroyed;

 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
    realData.effectToApply = TryGet<StatusEffectData>("Momo combo");
    realData.targetMustBeAlive = false;

})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Momo combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Momo"};
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Mo MoA";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));







        statusEffects.Add(
           new StatusEffectDataBuilder(this)
               .Create<StatusEffectApplyXWhenYAppliedTo>("When ally is sp gain spice")
               .WithText("When allies gains <keyword=sp>, gain equal amount of <keyword=spice>.")
               .WithStackable(true)
               .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyXWhenYAppliedTo;

                    realData.effectToApply = TryGet<StatusEffectData>("Spice");
                    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    realData.eventPriority = 2;
                    realData.applyEqualAmount = true;
                    realData.contextEqualAmount= new ScriptableCurrentStatus() { statusType = Get<StatusEffectData>("EXP").type };
                    realData.whenAppliedTypes = new string[1] { "sp" };
                    realData.whenAppliedToFlags = ApplyToFlags.Allies;
                    realData.eventPriority = 9999;
                })
                );


        //THE CONDITIONS
        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedTo>("Perma soice cake")
          .WithText($"<keyword={Extensions.PrefixGUID("perma", this)}> into <card=goobers.Peppiake> when <keyword=spice> reaches 60.")
          .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedTo;

               realData.effectToApply = TryGet<StatusEffectData>("combo spice");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.mustReachAmount = true;
               realData.eventPriority = 2;
               realData.whenAppliedTypes = new string[1] { "spice" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;

               var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
               script.amount = 1;
               ((StatusEffectApplyX)data).scriptableAmount = script;
           }));
        //THE END OF THE PROCESS
        //FOR INSTANT SUMMON 

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectInstantCombineCard>("combo spice")
        .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
        {
            ((StatusEffectInstantCombineCard)data).cardNames = new string[1] { "goobers.Peppifin" };
            ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Peppiake";
            ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
            ((StatusEffectInstantCombineCard)data).checkHand = true;
            ((StatusEffectInstantCombineCard)data).changeDeck = true;
            ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
        })
        );





        statusEffects.Add(
   StatusCopy("When Ally Is Healed Apply Equal Spice", "When Ally Is Healed trigger self")
   .WithText("When ally is healed, trigger self")
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectApplyXWhenAllyHealed;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

      (data as StatusEffectApplyXWhenAllyHealed).effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
      (data as StatusEffectApplyXWhenAllyHealed).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      (data as StatusEffectApplyXWhenAllyHealed).isReaction = true;

  }
  ));

        statusEffects.Add(
     new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyXWhenYAppliedTo>("Sp triggering")
     .WithCanBeBoosted(false)                                         //Not a Lumin Vase target
     .WithText("Trigger when ally gains <keyword=sp>.")     //You must put the GUID in some way here. $"<card={Extensions.PrefixGUID("shadeSerpent",this)}>" works as well here.
                                              //Type is typically used for SFX/VFX when applying the status effect. Not necessary as we are not applying this effect during battle, unless you use the "add effect" command.
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectApplyXWhenYAppliedTo;

      realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
      realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      realData.eventPriority = 2;
      realData.whenAppliedTypes = new string[1] { "sp" };
      realData.whenAppliedToFlags = ApplyToFlags.Allies;
      realData.eventPriority = 9999;
  })

         );


        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenYAppliedTo>("Sp triggering Knight")
 .WithCanBeBoosted(false)                                         //Not a Lumin Vase target
 .WithText("Trigger when <keyword=sp> is gained.")     //You must put the GUID in some way here. $"<card={Extensions.PrefixGUID("shadeSerpent",this)}>" works as well here.
                                                        //Type is typically used for SFX/VFX when applying the status effect. Not necessary as we are not applying this effect during battle, unless you use the "add effect" command.
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenYAppliedTo;

    realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.eventPriority = 2;
    realData.whenAppliedTypes = new string[1] { "sp" };
    realData.whenAppliedToFlags = ApplyToFlags.Self;
    realData.eventPriority = 9999;
})

     );
        statusEffects.Add(StatusCopy("Trigger When Self Or Ally Loses Block", "Trigger When Self Or Ally Loses Scrap")
.WithText("Trigger when self or ally loses <keyword=scrap>")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectApplyXWhenUnitLosesY;
    realData.statusType = "scrap";
    realData.targetMustBeAlive = false;
}));


        statusEffects.Add(new StatusEffectDataBuilder(this)
       .Create<StatusEffectApplyXWhenAllyIsHit>("Vein")
       .WithText("When an ally is hit, gain <{a}> <keyword=ex>")
       .WithStackable(true)
       .WithCanBeBoosted(true)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenAllyIsHit;
                 {
                 
                     realData.effectToApply = TryGet<StatusEffectData>("Expresso");
                     realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                     realData.targetMustBeAlive = false;
                     realData.eventPriority = 10000;

                 }
             }));



        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Oroberry combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Oro", "goobers.Berry" };
((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Oroberry";
((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
((StatusEffectInstantCombineCard)data).checkDeck = false;
((StatusEffectInstantCombineCard)data).checkBoard = true;
((StatusEffectInstantCombineCard)data).changeDeck = true;
((StatusEffectInstantCombineCard)data).keepUpgrades = true;
})
);

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantCombineCard>("Orosugar combo")
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
 {
     ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Oro", "goobers.Sugary" };
     ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Orosugary";
     ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
     ((StatusEffectInstantCombineCard)data).checkDeck = false;
     ((StatusEffectInstantCombineCard)data).checkBoard = true;
     ((StatusEffectInstantCombineCard)data).changeDeck = true;
     ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
 })
 );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Oroodd combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Oro", "goobers.Odd" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Oroodd";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Oroblood combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Oro", "goobers.Blood" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Oroblood";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenHit>("BerryOro Act")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenHit;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("Oroberry combo");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                          realData.attackerConstraints = new[]
                          {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Berry")
                            }
                        }
                          };
                      }));



        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("SugarOro Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Orosugar combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Sugary")
                            }
                        }
                  };
              }));

        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("OddOro Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Oroodd combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Odd")
                            }
                        }
                  };
              }));

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
             .Create<StatusEffectApplyXWhenHit>("BloodOro Act")
             .WithStackable(false)
             .WithCanBeBoosted(false)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenHit;

                 realData.eventPriority = 999999999;
                 realData.targetMustBeAlive = false;
                 realData.effectToApply = TryGet<StatusEffectData>("Oroblood combo");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.attackerConstraints = new[]
                 {

                 new TargetConstraintIsSpecificCard()
                 {
                     allowedCards = new CardData[]
                        {
                            Get<CardData>("Blood")
                        }
                 }
             };
             }));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Red velvet aura")
  .WithText("When deployed, all enemies in the row lose half of their <keyword=health>")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var se = data as StatusEffectApplyXWhenDeployed;
se.effectToApply = TryGet<StatusEffectData>("Lose Half Health");
se.applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
se.eventPriority = 1;
se.targetMustBeAlive = false;
}));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("velvet combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Velvet", "goobers.BloodKnife" };
((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Red Velvet";
((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
((StatusEffectInstantCombineCard)data).checkDeck = true;
((StatusEffectInstantCombineCard)data).checkBoard = true;
((StatusEffectInstantCombineCard)data).changeDeck = true;
((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenHit>("Velvet Split Act")
                      .WithText("<keyword=goobers.velvetfuse>")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenHit;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("velvet combo");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                          realData.attackerConstraints = new[]
                          {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Fuse")
                            }
                        }
                          };
                      }));




        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenItemPlayed>("Fuel")
    .WithText("When an item is played, gain <{a}> <keyword=attack>")
    .WithStackable(true)
    .WithCanBeBoosted(true)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenItemPlayed;

         realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         realData.eventPriority = 5;


     })
     );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenItemPlayed>("Superfuel")
.WithText("When an item is played, gain <{a}> <keyword=frenzy>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenItemPlayed;

 realData.effectToApply = TryGet<StatusEffectData>("MultiHit");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
 realData.eventPriority = 5;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenRedrawHit>("Redraw Bell then SP")
.WithText("Gain <{a}> <keyword=sp> when redraw bell is hit")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenRedrawHit;

realData.effectToApply = TryGet<StatusEffectData>("EXP");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 5;


})
); 


                    statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("Drawn cards gain temp attack")
.WithText("Gain <keyword=attack> equal amount of cards in your hand.")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectWhileActiveX;

    realData.applyEqualAmount = true;
    realData.effectToApply = TryGet<StatusEffectData>("Ongoing Increase Attack");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.eventPriority = 5;
    realData.applyFormatKey = new UnityEngine.Localization.LocalizedString();
    var script = ScriptableObject.CreateInstance<ScriptableCardsInHand>();
    ((StatusEffectWhileActiveX)data).scriptableAmount = script;


})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("Drawn cards gain Frenzy")
.WithText("reduce <keyword=attack> equal amount of cards in your hand.")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectWhileActiveX;

realData.applyEqualAmount = true;
realData.effectToApply = TryGet<StatusEffectData>("Ongoing Reduce Attack");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 5;
realData.applyFormatKey = new UnityEngine.Localization.LocalizedString();
var script = ScriptableObject.CreateInstance<ScriptableCardsInHand>();
((StatusEffectWhileActiveX)data).scriptableAmount = script;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Crepey combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Crepey" };
((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.CrepeyLV2";
((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
((StatusEffectInstantCombineCard)data).checkDeck = true;
((StatusEffectInstantCombineCard)data).checkBoard = true;
((StatusEffectInstantCombineCard)data).changeDeck = true;
((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedToCustom>("Crepe now")
          .WithText("<keyword=sp> required 60 - <keyword=goobers.perma> into <card=goobers.CrepeyLV2>.")
          .WithCanBeBoosted(false)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

               realData.effectToApply = TryGet<StatusEffectData>("Crepey combo");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.requiredAmount = 60;
               realData.eventPriority = 5;
               realData.whenAppliedTypes = new string[1] { "sp" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;

           })
           );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXPreTrigger>("Drawn cards gain attack")
.WithText("Before triggering, gain <keyword=attack> equal to the amount of cards in your hand.")
.WithStackable(false)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXPreTrigger;

realData.applyEqualAmount = true;
realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 5;
realData.applyFormatKey = new UnityEngine.Localization.LocalizedString();
var script = ScriptableObject.CreateInstance<ScriptableCardsInHand>();
((StatusEffectApplyXPreTrigger)data).scriptableAmount = script;


})
);

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedToCustom>("draw more when sp")
          .WithText("<keyword=sp> required 20 - all allies in the row gain <{a}> <keyword=draw>.")
          .WithStackable(true)
          .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

               realData.effectToApply = TryGet<StatusEffectData>("Temporary Draw");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
               realData.requiredAmount = 20;
               realData.eventPriority = 5;
               realData.whenAppliedTypes = new string[1] { "sp" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;

           })
           );


        statusEffects.Add(
      new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyXWhenYAppliedToCustom>("sp reach gain health")
          .WithText("<keyword=sp> required 10 - increase max <keyword=health> by <{a}>.")
          .WithStackable(true)
          .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

               realData.effectToApply = TryGet<StatusEffectData>("Increase Max Health");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
               realData.requiredAmount = 10;
               realData.eventPriority = 5;
               realData.whenAppliedTypes = new string[1] { "sp" };
               realData.whenAppliedToFlags = ApplyToFlags.Self;

           })
           );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXPreTrigger>("draw more health")
    .WithText("Draw cards equal amount of <keyword=health>.")
    .WithCanBeBoosted(false)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXPreTrigger;

         realData.effectToApply = TryGet<StatusEffectData>("Draw Cards");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         realData.eventPriority = 5;
         realData.applyEqualAmount = true;
         realData.contextEqualAmount = new ScriptableCurrentHealth();
         realData.applyFormatKey = new UnityEngine.Localization.LocalizedString();
         var script = ScriptableObject.CreateInstance<ScriptableCurrentHealth>();
         ((StatusEffectApplyXPreTrigger)data).scriptableAmount = script;


     })
     );


        //ASHI SHI TAG RESTORE!!!!!!! --------------------------------------------------------------
        //ASHI SHI TAG RESTORE!!!!!!! --------------------------------------------------------------
        //ASHI SHI TAG RESTORE!!!!!!! --------------------------------------------------------------
        //ASHI SHI TAG RESTORE!!!!!!! --------------------------------------------------------------
        //ASHI SHI TAG RESTORE!!!!!!! --------------------------------------------------------------
        //ASHI SHI TAG RESTORE!!!!!!! --------------------------------------------------------------

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon FortuneTag")
.SubscribeToAfterAllBuildEvent(data =>
{
 (data as StatusEffectSummon).summonCard = TryGet<CardData>("FortuneTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant FortuneTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon FortuneTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When fortune dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant FortuneTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("FortuneTag")
        }
};
}

));


        statusEffects.Add(
StatusCopy("Summon Junk", "Summon SunTag")
.SubscribeToAfterAllBuildEvent(data =>
{
(data as StatusEffectSummon).summonCard = TryGet<CardData>("SunTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant SunTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon SunTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When SunTag dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant SunTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("SunTag")
        }
};
}

));


        statusEffects.Add(
StatusCopy("Summon Junk", "Summon TeethTag")
.SubscribeToAfterAllBuildEvent(data =>
{
(data as StatusEffectSummon).summonCard = TryGet<CardData>("TeethTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant TeethTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon TeethTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When TeethTag dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant TeethTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("TeethTag")
        }
};
}

));


        statusEffects.Add(
StatusCopy("Summon Junk", "Summon DetonatorTag")
.SubscribeToAfterAllBuildEvent(data =>
{
(data as StatusEffectSummon).summonCard = TryGet<CardData>("DetonatorTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant DetonatorTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon DetonatorTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When DetonatorTag dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant DetonatorTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("DetoTag")
        }
};
}

));

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon NovaTag")
.SubscribeToAfterAllBuildEvent(data =>
{
 (data as StatusEffectSummon).summonCard = TryGet<CardData>("NovaTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant NovaTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon NovaTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When NovaTag dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant NovaTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("NovaTag")
        }
};
}

));

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon LuminTag")
.SubscribeToAfterAllBuildEvent(data =>
{
(data as StatusEffectSummon).summonCard = TryGet<CardData>("LuminTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant LuminTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon LuminTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When LuminTag dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant LuminTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("LuminTag")
        }
};
}

));
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon DemonTag")
.SubscribeToAfterAllBuildEvent(data =>
{
(data as StatusEffectSummon).summonCard = TryGet<CardData>("DemonTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant DemonTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon DemonTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When DemonTag dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant DemonTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("DemonTag")
        }
};
}

));

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon WinterTag")
.SubscribeToAfterAllBuildEvent(data =>
{
(data as StatusEffectSummon).summonCard = TryGet<CardData>("WinterTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant WinterTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon WinterTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When WinterTag dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant WinterTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("WinterTag")
        }
};
}

));

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon HealingTag")
.SubscribeToAfterAllBuildEvent(data =>
{
 (data as StatusEffectSummon).summonCard = TryGet<CardData>("HealingTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant HealingTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon HealingTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When HealingTag dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant HealingTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("HealTag")
        }
};
}

));

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon SnowTag")
.SubscribeToAfterAllBuildEvent(data =>
{
(data as StatusEffectSummon).summonCard = TryGet<CardData>("SnowTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant SnowTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon SnowTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When SnowTag dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant SnowTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("SnowTag")
        }
};
}

));

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon ConnectionTag")
.SubscribeToAfterAllBuildEvent(data =>
{
(data as StatusEffectSummon).summonCard = TryGet<CardData>("ConnectionTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant ConnectionTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon ConnectionTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When ConnectionTag dies gain again").WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant ConnectionTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("CON")
        }
};
}

));

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon GoblingTag")
 .WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
(data as StatusEffectSummon).summonCard = TryGet<CardData>("GoblingTag");
})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant GoblingTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon GoblingTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When GoblingTag dies gain again")
 .WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant GoblingTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("GoblingTag")
        }
};
}

));
        statusEffects.Add(
   StatusCopy("Summon Junk", "Summon SplitTag")
    .WithCanBeBoosted(false)
   .SubscribeToAfterAllBuildEvent(data =>
   {
       (data as StatusEffectSummon).summonCard = TryGet<CardData>("SplitTag");
   })
   );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant SplitTag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon SplitTag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When SplitTag dies gain again")
 .WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant SplitTag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("Splittag")
        }
};
}

));

        statusEffects.Add(
  StatusCopy("Summon Junk", "Summon Restag")
   .WithCanBeBoosted(false)
  .SubscribeToAfterAllBuildEvent(data =>
  {
      (data as StatusEffectSummon).summonCard = TryGet<CardData>("Restag");
  })
  );
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Restag")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Restag") as StatusEffectSummon;
                })
        );

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitIsKilled>("When Restag dies gain again")
 .WithCanBeBoosted(false)

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectApplyXWhenUnitIsKilled).effectToApply = TryGet<StatusEffectData>("Instant Restag");
    (data as StatusEffectApplyXWhenUnitIsKilled).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    (data as StatusEffectApplyXWhenUnitIsKilled).eventPriority = 10;
    (data as StatusEffectApplyXWhenUnitIsKilled).targetMustBeAlive = false;
    (data as StatusEffectApplyXWhenUnitIsKilled).enemy = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).ally = true;
    (data as StatusEffectApplyXWhenUnitIsKilled).unitConstraints = new[]
                 {

      new TargetConstraintHasStatus()
        {
            status = TryGet<StatusEffectData>("Restag")
        }
};
}

));

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
      .Create<StatusEffectApplyXWhenDeployed>("Free fortunetag")
      .WithText("When deployed,add 1 <card=goobers.FortuneTag> to your hand")
      .WithStackable(false)
      .WithCanBeBoosted(false)
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyXWhenDeployed;

          realData.effectToApply = TryGet<StatusEffectData>("Instant FortuneTag");
          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
          realData.eventPriority = 100;


      }

      ));

        //END OF ASHI TAG RESTORE-------------------------------------------------------
        //END OF ASHI TAG RESTORE-------------------------------------------------------
        //END OF ASHI TAG RESTORE-------------------------------------------------------  //END OF ASHI TAG RESTORE-------------------------------------------------------
        //END OF ASHI TAG RESTORE-------------------------------------------------------
        //END OF ASHI TAG RESTORE-------------------------------------------------------
        //END OF ASHI TAG RESTORE-------------------------------------------------------
        //END OF ASHI TAG RESTORE-------------------------------------------------------

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHit>("Hit help all")
.WithText("When hit, reduce allies <keyword=counter> by <{a}>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenHit;

realData.effectToApply = TryGet<StatusEffectData>("Reduce Counter");
realData.eventPriority = 5;
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
    realData.targetMustBeAlive = false;


})
);



        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXInstant>("ATK1")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXInstant;

    realData.effectToApply = TryGet<StatusEffectData>("Set Attack");
    realData.eventPriority = 5;
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
    script.amount = 1;
    ((StatusEffectApplyX)data).scriptableAmount = script;

})
);
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXInstant>("ATK2")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXInstant;

  realData.effectToApply = TryGet<StatusEffectData>("Set Attack");
  realData.eventPriority = 5;
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
  script.amount = 2;
  ((StatusEffectApplyX)data).scriptableAmount = script;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXInstant>("ATK3")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXInstant;

  realData.effectToApply = TryGet<StatusEffectData>("Set Attack");
  realData.eventPriority = 5;
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
  script.amount = 3;
  ((StatusEffectApplyX)data).scriptableAmount = script;

})
);

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
      .Create<StatusEffectApplyXInstant>("ATK4")
      .WithStackable(true)
      .WithCanBeBoosted(false)
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyXInstant;

          realData.effectToApply = TryGet<StatusEffectData>("Set Attack");
          realData.eventPriority = 5;
          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
          var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
          script.amount = 4;
          ((StatusEffectApplyX)data).scriptableAmount = script;

      })
      );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXInstant>("ATK5")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXInstant;

  realData.effectToApply = TryGet<StatusEffectData>("Set Attack");
  realData.eventPriority = 5;
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
  script.amount = 5;
  ((StatusEffectApplyX)data).scriptableAmount = script;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXInstant>("ATK6")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXInstant;

  realData.effectToApply = TryGet<StatusEffectData>("Set Attack");
  realData.eventPriority = 5;
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
  script.amount = 6;
  ((StatusEffectApplyX)data).scriptableAmount = script;

})
);
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXInstant>("ATK7")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXInstant;

realData.effectToApply = TryGet<StatusEffectData>("Set Attack");
realData.eventPriority = 5;
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
script.amount = 7;
((StatusEffectApplyX)data).scriptableAmount = script;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXInstant>("ATK8")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXInstant;

realData.effectToApply = TryGet<StatusEffectData>("Set Attack");
realData.eventPriority = 5;
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
script.amount = 8;
((StatusEffectApplyX)data).scriptableAmount = script;

})
);



        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyRandomOnHit>("Goofy Knife")
.WithText("On hit, change <keyword=attack> randomly from 1-8")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
((StatusEffectApplyRandomOnHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
((StatusEffectApplyRandomOnHit)data).effectsToapply = new StatusEffectData[]
{
                   Get<StatusEffectData>("ATK1"),
                    Get<StatusEffectData>("ATK2"),
                     Get<StatusEffectData>("ATK3"),
                      Get<StatusEffectData>("ATK4"),
                       Get<StatusEffectData>("ATK5"),
                        Get<StatusEffectData>("ATK6"),
                         Get<StatusEffectData>("ATK7"),
                         Get<StatusEffectData>("ATK8"),



};
}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("sp frenzy drawe")
.WithText("<keyword=sp> required 30 - gain <{a}> <keyword=frenzy>.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

 realData.effectToApply = TryGet<StatusEffectData>("MultiHit");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
 realData.requiredAmount = 30;
 realData.eventPriority = 5;
 realData.whenAppliedTypes = new string[1] { "sp" };
 realData.whenAppliedToFlags = ApplyToFlags.Self;

})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("sp waffle drawe")
.WithText("<keyword=sp> required 10 - all allies gain <{a}> <keyword=ch>.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

    realData.effectToApply = TryGet<StatusEffectData>("Choco");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
    realData.requiredAmount = 10;
    realData.eventPriority = 5;
    realData.whenAppliedTypes = new string[1] { "sp" };
    realData.whenAppliedToFlags = ApplyToFlags.Self;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("sp demon drawe")
.WithText("<keyword=sp> required 10 - apply <{a}> <keyword=bleed> to all enemies.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

realData.effectToApply = TryGet<StatusEffectData>("Bleeding");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
realData.requiredAmount = 10;
realData.eventPriority = 5;
realData.whenAppliedTypes = new string[1] { "sp" };
realData.whenAppliedToFlags = ApplyToFlags.Self;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("Drawn cards gain teeth")
.WithText("Allies gain <keyword=teeth> equal amount of cards in your hand.")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectWhileActiveX;

realData.applyEqualAmount = true;
realData.effectToApply = TryGet<StatusEffectData>("Teeth");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
realData.eventPriority = 5;
realData.applyFormatKey = new UnityEngine.Localization.LocalizedString();
var script = ScriptableObject.CreateInstance<ScriptableCardsInHand>();
((StatusEffectWhileActiveX)data).scriptableAmount = script;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("Drawn cards gain Shekkll")
.WithText("While active, set <keyword=shell> equal to cards in hand")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectWhileActiveX;

realData.applyEqualAmount = true;
realData.effectToApply = TryGet<StatusEffectData>("Shell");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 5;
realData.applyFormatKey = new UnityEngine.Localization.LocalizedString();
var script = ScriptableObject.CreateInstance<ScriptableCardsInHand>();
((StatusEffectWhileActiveX)data).scriptableAmount = script;


})
);

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("All allies gain attack equal health")
.WithText("While active, all allies gain <keyword=attack> equal to amount of current <keyword=health>.")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectWhileActiveX;
realData.scriptableAmount = new ScriptableCurrentHealth() {};
realData.effectToApply = Get<StatusEffectData>("Ongoing Increase Attack");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;



}));


        statusEffects.Add(
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenYAppliedToCustom>("When SP more draw2")
            .WithText("Increase own max <keyword=health> equal amount of cards in hand, when <keyword=sp> reaches 30.")
            .WithCanBeBoosted(false)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

                 realData.effectToApply = TryGet<StatusEffectData>("Increase Max Health");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.requiredAmount = 30;
                 realData.eventPriority = 2;
                 realData.whenAppliedTypes = new string[1] { "sp" };
                 realData.whenAppliedToFlags = ApplyToFlags.Self;

                 var script = ScriptableObject.CreateInstance<ScriptableCardsInHand>();
                 ((StatusEffectApplyX)data).scriptableAmount = script;
             }));






        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Cherripan combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Crepey", "goobers.Berry" };
((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Cherripan";
((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
((StatusEffectInstantCombineCard)data).checkDeck = false;
((StatusEffectInstantCombineCard)data).checkBoard = true;
((StatusEffectInstantCombineCard)data).changeDeck = true;
((StatusEffectInstantCombineCard)data).keepUpgrades = true;
})
);

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantCombineCard>("Crepeysugar combo")
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
 {
     ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Crepey", "goobers.Sugary" };
     ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.CrepeySugary";
     ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
     ((StatusEffectInstantCombineCard)data).checkDeck = false;
     ((StatusEffectInstantCombineCard)data).checkBoard = true;
     ((StatusEffectInstantCombineCard)data).changeDeck = true;
     ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
 })
 );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Crepeyodd combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Crepey", "goobers.Odd" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.CrepeyOdd";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("crepeyblood combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Crepey", "goobers.Blood" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.CrepeyBlood";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenHit>("Crepebe Act")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenHit;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("Cherripan combo");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                          realData.attackerConstraints = new[]
                          {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Berry")
                            }
                        }
                          };
                      }));



        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("Crepes Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Crepeysugar combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Sugary")
                            }
                        }
                  };
              }));

        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("Crepeo Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Crepeyodd combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Odd")
                            }
                        }
                  };
              }));

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
             .Create<StatusEffectApplyXWhenHit>("Crepebl Act")
             .WithStackable(false)
             .WithCanBeBoosted(false)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenHit;

                 realData.eventPriority = 999999999;
                 realData.targetMustBeAlive = false;
                 realData.effectToApply = TryGet<StatusEffectData>("crepeyblood combo");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.attackerConstraints = new[]
                 {

                 new TargetConstraintIsSpecificCard()
                 {
                     allowedCards = new CardData[]
                        {
                            Get<CardData>("Blood")
                        }
                 }
             };
             }));




        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectNextPhase>("QueentoAmazon")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectNextPhase;
    realData.goToNextPhase = true;
    realData.nextPhase = TryGet<CardData>("The Amazon");
    realData.preventDeath = true;
    realData.animation = TryGet<StatusEffectNextPhase>("FinalBossPhase2").animation;




}));

        statusEffects.Add(new StatusEffectDataBuilder(this)
 .Create<StatusEffectNextPhase>("AmazontoKing")
 .WithStackable(true)
 .WithCanBeBoosted(false)
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
 {
     var realData = data as StatusEffectNextPhase;
     realData.goToNextPhase = true;
     realData.nextPhase = TryGet<CardData>("The King");
     realData.preventDeath = true;
     realData.animation = TryGet<StatusEffectNextPhase>("FinalBossPhase2").animation;
    



     
 }));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantFillXBoardSlots>("Kingmoment")

.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectInstantFillXBoardSlots;


    realData.eventPriority = 60;
    realData.withCards = new CardData[] { Get<CardData>("Pawn"), Get<CardData>("Pawn"), Get<CardData>("Pawn"), Get<CardData>("Pawn")};


}

));





        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("When Deplpyed pawns")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectApplyXWhenDeployed;
    realData.effectToApply = Get<StatusEffectData>("Kingmoment");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;



}));


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("When Deplpyed pawns")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectApplyXWhenDeployed;
    realData.effectToApply = Get<StatusEffectData>("Kingmoment");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;



}));



        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Enemy Bishop")
.WithText("Restore <keyword=health> and apply <keyword=sp> by <{a}> to all allies.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
   var realData = data as StatusEffectApplyXOnTurn;
   realData.effectToApply = Get<StatusEffectData>("EXP");
   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
    realData.eventPriority = 2;



}));


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Enemy Bishop1")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectApplyXOnTurn;
    realData.effectToApply = Get<StatusEffectData>("Heal");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
    realData.eventPriority = 3;


}));



        statusEffects.Add(
     new StatusEffectDataBuilder(this)
         .Create<StatusEffectApplyXWhenYAppliedTo>("SP TRIGGER")
         .WithText("Trigger when <keyword=sp>'d.")
         .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectApplyXWhenYAppliedTo;

              realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
              realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
              realData.eventPriority = 2;
              realData.whenAppliedTypes = new string[1] { "sp" };
              realData.whenAppliedToFlags = ApplyToFlags.Self;
              realData.isReaction = true;
          }));



        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenAllyIsKilled>("Queen go grr")
.WithText("When ally is killed, count down and reduce <keyword=counter> by <{a}>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectApplyXWhenAllyIsKilled;
realData.effectToApply = Get<StatusEffectData>("Queen effect");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 3;


}));

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenAllyIsKilled>("Queen go grr1")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
   var realData = data as StatusEffectApplyXWhenAllyIsKilled;
   realData.effectToApply = Get<StatusEffectData>("Reduce Counter");
   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
   realData.eventPriority = 3;


}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantMultiple>("Queen effect")
.WithText("")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectInstantMultiple;

    realData.effects = new StatusEffectInstant[]
{
     TryGet<StatusEffectInstant>("Reduce Max Counter"),
     TryGet<StatusEffectInstant>("Reduce Counter")
};
}

    ));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
   .Create<StatusEffectApplyXWhenHit>("SP to ally behind")
   .WithText("When hit, ally behind gains <keyword=sp> equal to the amount of current <keyword=sp>.")
   .WithCanBeBoosted(false)
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var realData = data as StatusEffectApplyXWhenHit;

        realData.effectToApply = TryGet<StatusEffectData>("EXP");
        realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
        realData.eventPriority = 2;
        var script = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
        script.statusType = "sp";
        ((StatusEffectApplyX)data).scriptableAmount = script;
    }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenHit>("SP to Allies when hit")
 .WithText("When hit, allies gains <keyword=sp> equal to the amount of current <keyword=sp>.")
 .WithCanBeBoosted(false)
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectApplyXWhenHit;

      realData.effectToApply = TryGet<StatusEffectData>("EXP");
      realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
      realData.eventPriority = 2;
      var script = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
      script.statusType = "sp";
      ((StatusEffectApplyX)data).scriptableAmount = script;
  }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenAllyIsKilled>("Killed reduce")
.WithText("When ally is killed, reduce own effects by 1.")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenAllyIsKilled;

    realData.effectToApply = TryGet<StatusEffectData>("Reduce Effects");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.eventPriority = 2;
}));
        

               statusEffects.Add(
     new StatusEffectDataBuilder(this)
         .Create<StatusEffectApplyXWhenYAppliedTo>("SUPERNOVA")
         .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectApplyXWhenYAppliedTo;

              realData.effectToApply = TryGet<StatusEffectData>("blue combo");
              realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
              realData.eventPriority = 2;
              realData.whenAppliedTypes = new string[1] { "novatag" };
              realData.whenAppliedToFlags = ApplyToFlags.Self;
          }));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("blue combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
((StatusEffectInstantCombineCard)data).cardNames = new string[] { "Blue" };
((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Hypernova";
((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
((StatusEffectInstantCombineCard)data).checkDeck = false;
((StatusEffectInstantCombineCard)data).checkBoard = true;
((StatusEffectInstantCombineCard)data).changeDeck = true;
((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));

        statusEffects.Add(
    new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXWhenHit>("Supernovatime")
        .WithText("Trigger when ally or self is hit.")
        .WithCanBeBoosted(false)
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXWhenHit;

             realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
             realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
             realData.eventPriority = 2;
             realData.isReaction = true;
         }));
        statusEffects.Add(
  new StatusEffectDataBuilder(this)
      .Create<StatusEffectApplyXWhenAllyIsHit>("Supernovatime1")
      .WithCanBeBoosted(false)
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectApplyXWhenAllyIsHit;

           realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
           realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
           realData.eventPriority = 2;
       }));


        statusEffects.Add(
    new StatusEffectDataBuilder(this)
        .Create<StatusEffectBleed>("Bleeding")
       .WithText("Apply <{a}> <keyword=bleed>")
.WithIconGroupName("health")
.WithVisible(true)
.WithIsStatus(true)
.WithStackable(true)
.WithOffensive(true)
    .WithTextInsert("{a}")
           .WithKeyword("bleed")
           .WithType("bleed")



        );





        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Crepeyberry combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
  ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Cinaroll", "goobers.Berry" };
  ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.BerryCinaroll";
  ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
  ((StatusEffectInstantCombineCard)data).checkDeck = false;
  ((StatusEffectInstantCombineCard)data).checkBoard = true;
  ((StatusEffectInstantCombineCard)data).changeDeck = true;
  ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
})
);

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantCombineCard>("Cinarollsugar combo")
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
 {
     ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Cinaroll", "goobers.Sugary" };
     ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.SugaryCinaroll";
     ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
     ((StatusEffectInstantCombineCard)data).checkDeck = false;
     ((StatusEffectInstantCombineCard)data).checkBoard = true;
     ((StatusEffectInstantCombineCard)data).changeDeck = true;
     ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
 })
 );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Cinarollodd combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Cinaroll", "goobers.Odd" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.OddCinaroll";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Cinarollblood combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Cinaroll", "goobers.Blood" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.BloodCinaroll";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenHit>("Cinarollb Act")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenHit;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("Crepeyberry combo");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                          realData.attackerConstraints = new[]
                          {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Berry")
                            }
                        }
                          };
                      }));



        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("Cinarolls Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Cinarollsugar combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Sugary")
                            }
                        }
                  };
              }));

        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("Cinarollo Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Cinarollodd combo");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Odd")
                            }
                        }
                  };
              }));

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
             .Create<StatusEffectApplyXWhenHit>("Cinarollbl Act")
             .WithStackable(false)
             .WithCanBeBoosted(false)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenHit;

                 realData.eventPriority = 999999999;
                 realData.targetMustBeAlive = false;
                 realData.effectToApply = TryGet<StatusEffectData>("Cinarollblood combo");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.attackerConstraints = new[]
                 {

                 new TargetConstraintIsSpecificCard()
                 {
                     allowedCards = new CardData[]
                        {
                            Get<CardData>("Blood")
                        }
                 }
             };
             }));






    



        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenYAppliedTo>("When SP more draw")
    .WithText("Restore <keyword=health> equal amount of cards in hand, when <keyword=sp> reaches 20.")
    .WithCanBeBoosted(false)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenYAppliedTo;

         realData.effectToApply = TryGet<StatusEffectData>("Heal");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
         realData.eventPriority = 2;
         realData.whenAppliedTypes = new string[1] { "sp" };
         realData.whenAppliedToFlags = ApplyToFlags.Self;

         var script = ScriptableObject.CreateInstance<ScriptableAmountIsXByStatusCount>();
         script.statusName = "sp";
         ((StatusEffectApplyX)data).count = 5;
     }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("Pecan sweet")
.WithText("When allies gain <keyword=sp>, gain <{a}> <keyword=ex> to self.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

  realData.effectToApply = TryGet<StatusEffectData>("Expresso");
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  realData.requiredAmount = 1;
  realData.eventPriority = 50;
  realData.whenAppliedTypes = new string[1] { "sp" };
  realData.whenAppliedToFlags = ApplyToFlags.Allies;

})
);

          statusEffects.Add(StatusCopy("When Spice Or Shell Applied To Self Shroom To RandomEnemy", "Sweet Points into MAX Health")
.WithText("")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var se = data as StatusEffectApplyXWhenYAppliedTo;
    se.textKey = null;
    se.whenAppliedTypes = new string[] { "sp" };
se.whenAppliedToFlags = StatusEffectApplyX.ApplyToFlags.Self;
se.effectToApply = Get<StatusEffectData>("Increase Max Health");
se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
se.instead = true;
se.targetMustBeAlive = false;
    se.eventPriority = 100;
}));




        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("Odd double")
.WithText("<keyword=sp> required 20 - double <keyword=attack>.")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

realData.effectToApply = TryGet<StatusEffectData>("Double Attack");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.requiredAmount = 20;
realData.eventPriority = 50;
realData.whenAppliedTypes = new string[1] { "sp" };
realData.whenAppliedToFlags = ApplyToFlags.Self;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantGif>("Gif test")
.WithCanBeBoosted(false)



);



        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("Gain Snow")
.WithText("<keyword=sp> required 6 - increase own effects by <{a}>.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

realData.effectToApply = TryGet<StatusEffectData>("Increase Effects");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.requiredAmount = 6;
realData.eventPriority = 50;
realData.whenAppliedTypes = new string[1] { "sp" };
realData.whenAppliedToFlags = ApplyToFlags.Self;

})
);

     
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDiscarded>("Discard Destroy")
.WithText("When discarded, destroy this card")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDiscarded;

realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Ally");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 50;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDiscarded>("Discard Destroy")

.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDiscarded;

realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Ally");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 50;


})
);

        statusEffects.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectInstantLoseX>("LFullImmuneToInk")
      .WithIconGroupName("Reduce <keyword=sp> <{a}>")
      .WithCanBeBoosted(false)
      .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
      {
          var realData = data as StatusEffectInstantLoseX;

          realData.statusToLose = TryGet<StatusEffectData>("FullImmuneToInk");
      }
      )
      );

        statusEffects.Add(new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXWhenDeployed>("Lose Full Ink when Deployed")
     .WithCanBeBoosted(false)
     .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
     {
         var realData = data as StatusEffectApplyXWhenDeployed;

         realData.effectToApply = TryGet<StatusEffectData>("LFullImmuneToInk");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

     }
     )
     );


        statusEffects.Add(new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXOnTurn>("Expresso time for all")
           .WithText("Apply 1 <keyword=ex> to all allies")
     .WithCanBeBoosted(false)
     .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
     {
         var realData = data as StatusEffectApplyXOnTurn;

         realData.effectToApply = TryGet<StatusEffectData>("Expresso");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;

     }
     )
     );


        statusEffects.Add(new StatusEffectDataBuilder(this)
         .Create<StatusEffectApplyXOnTurn>("Expresso time for all")
         .WithText("Apply 1 <keyword=ex> to all allies")
   .WithCanBeBoosted(false)
   .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
   {
       var realData = data as StatusEffectApplyXOnTurn;

       realData.effectToApply = TryGet<StatusEffectData>("Expresso");
       realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;

   }
   )
   );

        statusEffects.Add(
     new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyRandomOnCardPlayed>("Random Buff2")
     .WithText("Apply <{a}> either, <keyword=block>/<keyword=attack>/Increase Effect/<keyword=health>/<keyword=shell>/<keyword=spice> to all allies")
     .WithStackable(true)
     .WithCanBeBoosted(true)
     .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
     {
         ((StatusEffectApplyRandomOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
         ((StatusEffectApplyRandomOnCardPlayed)data).effectsToapply = new StatusEffectData[]
         {
                   Get<StatusEffectData>("Block"),Get<StatusEffectData>("Increase Attack"),Get<StatusEffectData>("Increase Effects"),
                   Get<StatusEffectData>("Shell"),Get<StatusEffectData>("Increase Max Health"),
                   Get<StatusEffectData>("Spice")

         };
     }
     ));

        statusEffects.Add(new StatusEffectDataBuilder(this)
      .Create<StatusEffectWhileActiveX>("Hog to allies")
      .WithText("While active, allies gain <keyword=pigheaded>.")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
{
    var realData = data as StatusEffectWhileActiveX;

    realData.effectToApply = TryGet<StatusEffectData>("Temporary Pigheaded");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;

}
)
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Red velvet aura2")
 .WithText("When the enemy is deployed, half their <keyword=health>")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
   var se = data as StatusEffectApplyXWhenDeployed;
   se.whenEnemyDeployed = true;
    se.effectToApply=TryGet<StatusEffectData>("Lose Half Health");
    se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;

}));

        statusEffects.Add(
     new StatusEffectDataBuilder(this)
         .Create<StatusEffectApplyXWhenYAppliedToCustom>("Pecan")
         .WithStackable(true)
         .WithCanBeBoosted(true)
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

              realData.effectToApply = TryGet<StatusEffectData>("Shell");
              realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
              realData.requiredAmount = 1;
              realData.eventPriority = 5;
              realData.whenAppliedTypes = new string[1] { "sp" };
              realData.whenAppliedToFlags = ApplyToFlags.Self;

          })
          );

        statusEffects.Add(
    new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXWhenYAppliedToCustom>("Hangry")
        .WithStackable(true)
        .WithCanBeBoosted(true)
         .SubscribeToAfterAllBuildEvent(data =>
         {
             var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

             realData.effectToApply = TryGet<StatusEffectData>("Spice");
             realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
             realData.requiredAmount = 1;
             realData.eventPriority = 5;
             realData.whenAppliedTypes = new string[1] { "sp" };
             realData.whenAppliedToFlags = ApplyToFlags.Self;

         })
         );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenDestroyed>("Preventer")
    .WithStackable(true)
    .WithCanBeBoosted(true)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenDestroyed;

         realData.effectToApply = TryGet<StatusEffectData>("Snow");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
         realData.targetMustBeAlive = false;
         realData.eventPriority = 100;
   

     })
     );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenUnitLosesY>("Scrap hitter")
    .WithText("Trigger when ally or self loses <keyword=scrap>")
    .WithStackable(true)
    .WithIsReaction(true)
    .WithCanBeBoosted(false)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenUnitLosesY;

         realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         realData.targetMustBeAlive = false;
         realData.eventPriority = 100;
         realData.whenAllLost = false;
         realData.statusType = "scrap";
         realData.allies = true; 
                    })
     );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("Winnerashitime")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectWhileActiveX;

realData.eventPriority = 1;
realData.targetMustBeAlive = true;
realData.effectToApply = TryGet<StatusEffectData>("Increase Max Counter");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
}));



        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("blender1")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
   ((StatusEffectInstantCombineCard)data).cardNames = new string[] {"goobers.Berry" };
   ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Galaxy";
   ((StatusEffectInstantCombineCard)data).spawnOnBoard = false;
   ((StatusEffectInstantCombineCard)data).checkDeck = false;
   ((StatusEffectInstantCombineCard)data).checkBoard = true;
   ((StatusEffectInstantCombineCard)data).changeDeck = true;
   ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
})
);

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantCombineCard>("blender2")
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
 {
     ((StatusEffectInstantCombineCard)data).cardNames = new string[] {"goobers.Sugary" };
     ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Diabetes";
     ((StatusEffectInstantCombineCard)data).spawnOnBoard = false;
     ((StatusEffectInstantCombineCard)data).checkDeck = false;
     ((StatusEffectInstantCombineCard)data).checkBoard = true;
     ((StatusEffectInstantCombineCard)data).changeDeck = true;
     ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
 })
 );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("blender3")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Odd" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.how";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = false;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("blender4")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] {  "goobers.Blood" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Chalice";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = false;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenHit>("blender1 Act")
                      .WithText("When  an <keyword=goobers.minorin> is used on this card, turn ingredient into a smoothie.")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenHit;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("blender1");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                          realData.attackerConstraints = new[]
                          {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Berry")
                            }
                        }
                          };
                      }));



        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("blender2 Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("blender2");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Sugary")
                            }
                        }
                  };
              }));

        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("blender3 Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("blender3");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Odd")
                            }
                        }
                  };
              }));

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
             .Create<StatusEffectApplyXWhenHit>("blender4 Act")
             .WithStackable(false)
             .WithCanBeBoosted(false)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenHit;

                 realData.eventPriority = 999999999;
                 realData.targetMustBeAlive = false;
                 realData.effectToApply = TryGet<StatusEffectData>("blender4");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.attackerConstraints = new[]
                 {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Blood")
                            }
                        }
                 };
             }));


        statusEffects.Add(
    StatusCopy("Increase Effects", "Increase Effects with text")
   .WithText("Increase effects by <{a}>")                                       //Since this effect is on Shade Serpent, we modify the description shown.
                                                      //Makes a copy of the Summon Fallow effect
                                                                   //If that fails, then it uses no GUID-prefixing.
    );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<FEARONE>("FEAR")
 .WithIconGroupName("counter")
            .WithText("Apply <{a}> <keyword=fear>")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithStackable(true)
            .WithOffensive(true)
            .WithTextInsert("{a}")
            .WithKeyword("fear")
            .WithType("fear")
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as FEARONE;

   
    realData.effectToApply = TryGet<StatusEffectData>("Flee");
    realData.removeOnDiscard = true;
    realData.affectedBySnow = false;
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.requiredAmount = 4;
    realData.eventPriority = 1000;
    realData.whenAppliedTypes = new string[1] { "fear" };
    realData.whenAppliedToFlags = ApplyToFlags.Self;
    var script = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
    script.statusType = "fear";
    ((FEARONE)data).scriptableAmount = script;

    realData.applyConstraints = new TargetConstraint[]
{
        new TargetConstraintIsCardType()
        {
         not=true, allowedTypes= new[] { TryGet<CardType>("Leader")}
        }
};
    realData.targetConstraints = new TargetConstraint[]
    {
        new TargetConstraintIsCardType()
        {
          allowedTypes= new[] { TryGet<CardType>("Leader"),TryGet<CardType>("Friendly"), TryGet<CardType>("Enemy"), TryGet<CardType>("Summoned") }
            } };

})
);

 

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<FEARONE>("FEARLEADREAL")
.WithIconGroupName("counter")
  .WithVisible(true)
  .WithIsStatus(true)
  .WithStackable(true)
  .WithOffensive(true)
  .WithTextInsert("{a}")
  .WithKeyword("fear2")
  .WithType("fear2")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as FEARONE;

   
    realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Ally");
realData.removeOnDiscard = true;
realData.affectedBySnow = false;
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.requiredAmount = 12;
realData.eventPriority = 1000;
realData.whenAppliedTypes = new string[1] { "fear2" };
realData.whenAppliedToFlags = ApplyToFlags.Self;
    var script = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
    script.statusType = "fear2";
    ((FEARONE)data).scriptableAmount = script;

    realData.applyConstraints = new TargetConstraint[]
        {
        new TargetConstraintIsCardType()
        {
          allowedTypes= new[] { TryGet<CardType>("Leader") }
        }
};
realData.targetConstraints = new TargetConstraint[]
    {
        new TargetConstraintIsCardType()
        {
          allowedTypes= new[] { TryGet<CardType>("Leader")}
            } };

})
);





        statusEffects.Add(
new StatusEffectDataBuilder(this)
      .Create<StatusEffectApplyXWhenHit>("Fear on hit")
      .WithText("When hit, apply <{a}> <keyword=fear> to the attacker.")
      .WithStackable(true)
      .WithCanBeBoosted(true)
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectApplyXWhenHit;

          realData.eventPriority = 999999999;
          realData.targetMustBeAlive = false;
          realData.effectToApply = TryGet<StatusEffectData>("FEAR");
          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Attacker;
          realData.applyConstraints = new TargetConstraint[]
{
        new TargetConstraintIsCardType()
        {
         not=true, allowedTypes= new[] { TryGet<CardType>("Leader")}
        }
};

      }));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyXWhenHit>("Fear on hit2")
     .WithStackable(true)
     .WithCanBeBoosted(true)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenHit;

         realData.eventPriority = 999999999;
         realData.targetMustBeAlive = false;
         realData.effectToApply = TryGet<StatusEffectData>("FEARLEADREAL");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Attacker;

     }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Inflict when deployed")
.WithText("Increase <keyword=attack> by <{a}> when an enemy is deployed.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed;

    realData.eventPriority = 999999999;
    realData.targetMustBeAlive = false;
    realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
    realData.whenEnemyDeployed = true;
    realData.whenSelfDeployed = false;
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Inflict dam when deployed")
.WithText("When enemy is deployed, reduce their max <keyword=health> by <{a}>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenDeployed;

 realData.eventPriority = 999999999;
 realData.targetMustBeAlive = false;
    realData.effectToApply = TryGet<StatusEffectData>("Reduce Max Health");
    realData.whenEnemyDeployed = true;
 realData.whenSelfDeployed = false;
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;

}));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("Fear While active")
.WithText("While active, apply <{a}> <keyword=fear> to all enemies.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectWhileActiveX;

 realData.eventPriority = 1;
 realData.targetMustBeAlive = false;
 realData.effectToApply = TryGet<StatusEffectData>("FEAR");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
    realData.applyConstraints = new TargetConstraint[]
{
        new TargetConstraintIsCardType()
        {
         not=true, allowedTypes= new[] { TryGet<CardType>("Leader")}
        }
};
}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("Fear While active2")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectWhileActiveX;

  realData.eventPriority = 1;
  realData.targetMustBeAlive = false;
  realData.effectToApply = TryGet<StatusEffectData>("FEARLEADREAL");
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;

}));


        statusEffects.Add(
StatusCopy("Trigger (High Prio)", "Trigger (High Prio) for attack")
.WithText("Trigger target.")

);
        


     statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantDoubleX>("Double sp")
.WithText("Double <keyword=sp>")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectInstantDoubleX;

    realData.eventPriority = 999999999;
    realData.statusToDouble = TryGet<StatusEffectData>("EXP");
}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnCardPlayed>("Attack to all SP")
.WithText("Apply <{a}> <keyword=attack> to all allies with <keyword=sp>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXOnCardPlayed;

 realData.eventPriority = 999999999;
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
    realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
    realData.applyConstraints = new TargetConstraint[]
    {
        new TargetConstraintHasStatus()
        {
          status= TryGet<StatusEffectData>("EXP")
        }
    };

})
);
        

             statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectMultiConsume>("Uses")
.WithText("Apply <keyword=use>")
.WithStackable(true)
.WithCanBeBoosted(false)
     .WithIconGroupName("crown")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithOffensive(false)
            .WithTextInsert("{a}")
            .WithKeyword("use")
            .WithType("use")

);



        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("MMSN combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.MMSN" };
((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.MMSN2";
((StatusEffectInstantCombineCard)data).spawnOnBoard = false;
((StatusEffectInstantCombineCard)data).checkDeck = true;
((StatusEffectInstantCombineCard)data).checkBoard = true;
((StatusEffectInstantCombineCard)data).changeDeck = true;
((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenDeployed>("MMSN Act")
                      .WithText("<keyword=goobers.unlimita>")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenDeployed;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("MMSN combo");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                        
           
                      }));


        statusEffects.Add(
        new StatusEffectDataBuilder(this)
                    .Create<StatusEffectApplyXWhenDeployed>("MMSN Act")
                    .WithText("<keyword=goobers.unlimita>")
                    .WithStackable(false)
                    .WithCanBeBoosted(false)
                    .SubscribeToAfterAllBuildEvent(data =>
                    {
                        var realData = data as StatusEffectApplyXWhenDeployed;

                        realData.eventPriority = 999999999;
                        realData.targetMustBeAlive = false;
                        realData.effectToApply = TryGet<StatusEffectData>("MMSN combo");
                        realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;


                    }));



        statusEffects.Add(
       new StatusEffectDataBuilder(this)
                   .Create<StatusEffectApplyXWhenAnyoneTakesDamage>("Gain spice bleed")
                   .WithText("When anyone takes <keyword=bleed> damage, gain <{a}> <keyword=sp>.")
                   .WithStackable(true)
                   .WithCanBeBoosted(true)
                   .SubscribeToAfterAllBuildEvent(data =>
                   {
                       var realData = data as StatusEffectApplyXWhenAnyoneTakesDamage;

                       realData.eventPriority = 999999999;
                       realData.targetMustBeAlive = false;
                       realData.effectToApply = TryGet<StatusEffectData>("EXP");
                       realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                       realData.targetDamageType = "bleed";



                   }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("Blood attacker")
.WithText("<keyword=sp> required 15 - gain <{a}> <keyword=attack>.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.requiredAmount = 15;
realData.eventPriority = 50;
realData.whenAppliedTypes = new string[1] { "sp" };
realData.whenAppliedToFlags = ApplyToFlags.Self;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHealed>("Trigger self when healed")
.WithText("When healed, trigger self.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXWhenHealed;

  realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  realData.eventPriority = 50;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHealed>("Increase attack to allies when healed")
.WithText("When healed, all allies gain <{a}> <keyword=attack>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
   var realData = data as StatusEffectApplyXWhenHealed;

   realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
   realData.eventPriority = 50;


})
);



        statusEffects.Add(
     new StatusEffectDataBuilder(this)
     .Create<StatusEffectNextPhase>("DortoExe")
 .WithStackable(true)
 .WithCanBeBoosted(false)
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
 {
     var realData = data as StatusEffectNextPhase;
     realData.goToNextPhase = true;
     realData.nextPhase = TryGet<CardData>("Exe");
     realData.preventDeath = true;
     realData.animation = TryGet<StatusEffectNextPhase>("FinalBossPhase2").animation;

 }));


        statusEffects.Add(
    new StatusEffectDataBuilder(this)
    .Create<StatusEffectNextPhase>("Exetocre")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectNextPhase;
    realData.goToNextPhase = true;
    realData.nextPhase = TryGet<CardData>("Cre");
    realData.preventDeath = true;
    realData.animation = TryGet<StatusEffectNextPhase>("FinalBossPhase2").animation;

}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectNextPhase>("Cretopea")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectNextPhase;
realData.goToNextPhase = true;
realData.nextPhase = TryGet<CardData>("Pea");
realData.preventDeath = true;
realData.animation = TryGet<StatusEffectNextPhase>("FinalBossPhase2").animation;

}));




        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantFillXBoardSlots>("Executionersummons")

.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectInstantFillXBoardSlots;


 realData.eventPriority = 60;
 realData.withCards = new CardData[] { Get<CardData>("Judge"), Get<CardData>("Baby Horns"), Get<CardData>("Baby Horns"), Get<CardData>("Mourn") };
    realData.clearBoardFirst = true;


}

));





        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Judgetime")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectApplyXWhenDeployed;
    realData.effectToApply = Get<StatusEffectData>("Executionersummons");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;



}));




        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantFillXBoardSlots>("Cremationsummons")

.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectInstantFillXBoardSlots;


realData.eventPriority = 60;
realData.withCards = new CardData[] { Get<CardData>("Dumples"), Get<CardData>("Torture"), Get<CardData>("Torture"), Get<CardData>("Mourn") };
    realData.clearBoardFirst = true;


}

));





        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Cretime")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectApplyXWhenDeployed;
    realData.effectToApply = Get<StatusEffectData>("Cremationsummons");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;



}));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantFillXBoardSlots>("Peacesummons")

.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectInstantFillXBoardSlots;


realData.eventPriority = 60;
realData.withCards = new CardData[] { Get<CardData>("Halo"), Get<CardData>("Halo"), Get<CardData>("Inny"), Get<CardData>("Susu") };
realData.clearBoardFirst = true;


}

));





        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Peacetime")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectApplyXWhenDeployed;
    realData.effectToApply = Get<StatusEffectData>("Peacesummons");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;



}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenYAppliedToCustom>("Bocin Effect")
    .WithText("<keyword=sp> required 15 - increase max <keyword=health> by <{a}>")
    .WithStackable(true)
    .WithCanBeBoosted(true)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

         realData.effectToApply = TryGet<StatusEffectData>("Increase Max Health");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         realData.requiredAmount = 15;
         realData.eventPriority = 5;
         realData.whenAppliedTypes = new string[1] { "sp" };
         realData.whenAppliedToFlags = ApplyToFlags.Self;

     })
     );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
  .Create<StatusEffectApplyXOnTurn>("Heal allies in row")
  .WithText("Restore <{a}> <keyword=health> to allies in row.")
  .WithStackable(true)
  .WithCanBeBoosted(true)
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectApplyXOnTurn;

       realData.effectToApply = TryGet<StatusEffectData>("Heal");
       realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
       realData.eventPriority = 10;


   })
   );

        statusEffects.Add(
StatusCopy("When Ally Is Healed Apply Equal Spice", "When Ally Is Healed Apply Equal EXP")
.WithText("When ally is healed, gain equal <keyword=sp> to self.")                                       //Since this effect is on Shade Serpent, we modify the description shown.
.WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
{
    ((StatusEffectApplyXWhenAllyHealed)data).effectToApply = TryGet<StatusEffectData>("EXP"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
    ((StatusEffectApplyXWhenAllyHealed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;


})                                                                              //If that fails, then it uses no GUID-prefixing.
);



        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenYAppliedToCustom>("Pecan Effect")
    .WithText("<keyword=sp> required 9 - Gain <{a}> <keyword=shell>")
    .WithStackable(true)
    .WithCanBeBoosted(true)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

         realData.effectToApply = TryGet<StatusEffectData>("Shell");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         realData.requiredAmount = 9;
         realData.eventPriority = 5;
         realData.whenAppliedTypes = new string[1] { "sp" };
         realData.whenAppliedToFlags = ApplyToFlags.Self;

     })
     );

   


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenAllyIsHit>("Sugaroro new")
.WithText("When ally is hit, increase max <keyword=health> by <{a}>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXWhenAllyIsHit;

  realData.effectToApply = TryGet<StatusEffectData>("Increase Max Health");
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.eventPriority = 5;
  realData.targetMustBeAlive = false;

   

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Gain max health :3")
.WithText("On turn, increase own max <keyword=health> by <{a}>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXOnTurn;

  realData.effectToApply = TryGet<StatusEffectData>("Increase Max Health");
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  realData.eventPriority = 5;
  realData.targetMustBeAlive = false;



})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
  .Create<StatusEffectApplyXWhenYAppliedToCustom>("SP to increase max health")
  .WithText("<keyword=sp> required 10 - Gain <{a}> <keyword=health>")
  .WithStackable(true)
  .WithCanBeBoosted(true)
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

       realData.effectToApply = TryGet<StatusEffectData>("Increase Max Health");
       realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
       realData.requiredAmount = 10;
       realData.eventPriority = 5;
       realData.whenAppliedTypes = new string[1] { "sp" };
       realData.whenAppliedToFlags = ApplyToFlags.Self;

   })
   );

         statusEffects.Add(
new StatusEffectDataBuilder(this)
  .Create<StatusEffectApplyXWhenYAppliedToCustom>("Trigger when sporo")
  .WithText("<keyword=sp> required 15 - Trigger self")
  .WithStackable(false)
  .WithCanBeBoosted(false)
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

       realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
       realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
       realData.requiredAmount = 15;
       realData.eventPriority = 5;
       realData.whenAppliedTypes = new string[1] { "sp" };
       realData.whenAppliedToFlags = ApplyToFlags.Self;

   })
   );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHit>("Multi whenhit")
.WithText("When hit, gain x<{a}> <keyword=frenzy>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenHit;

realData.effectToApply = TryGet<StatusEffectData>("MultiHit");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 5;
    realData.targetMustBeAlive = false;


})
);

        statusEffects.Add(
     new StatusEffectDataBuilder(this)
         .Create<StatusEffectApplyXOnTurn>("Scrap All")
         .WithText("Apply <{a}> <keyword=scrap> to all allies.")
         .WithStackable(true)
         .WithCanBeBoosted(true)
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectApplyXOnTurn;

              realData.effectToApply = TryGet<StatusEffectData>("Scrap");
              realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;

              realData.eventPriority = 5;


          })
          );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHit>("Whenhit frostrow")
.WithText("When hit, apply <{a}> <keyword=frost> to the attacker")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenHit;

 realData.effectToApply = TryGet<StatusEffectData>("Frost");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
    realData.targetMustBeAlive = false;
 realData.eventPriority = 5;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitLosesY>("Scrap gain when ally")
 .WithText("Gain <{a}> <keyword=scrap> when ally loses <keyword=scrap>")
 .WithStackable(true)
 .WithCanBeBoosted(true)
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectApplyXWhenUnitLosesY;

      realData.effectToApply = TryGet<StatusEffectData>("Scrap");
      realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      realData.targetMustBeAlive = true;
      realData.eventPriority = 100;
      realData.whenAllLost = false;
      realData.statusType = "scrap";
      realData.allies = true;
      realData.self = false;
  })
  );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Fear transferer")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.eventPriority = 999999999;
realData.targetMustBeAlive = false;
realData.effectToApply = TryGet<StatusEffectData>("FEAR TRANSFER");
realData.whenEnemyDeployed = true;
realData.whenSelfDeployed = false;
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
    realData.targetConstraints = new TargetConstraint[]
    {
        new TargetConstraintIsSpecificCard()
        {
          allowedCards= new[] { TryGet<CardData>("Fear Sprout") }
        } };
}));


        statusEffects.Add(StatusCopy("When Spice Or Shell Applied To Self Shroom To RandomEnemy", "FEAR TRANSFER")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
 var se = data as StatusEffectApplyXWhenYAppliedTo;
 se.textKey = null;
 se.whenAppliedTypes = new string[] { "fear" };
 se.whenAppliedToFlags = StatusEffectApplyX.ApplyToFlags.Self;
 se.effectToApply = Get<StatusEffectData>("FEARLEADREAL");
 se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
 se.instead = true;
 se.targetMustBeAlive = false;
 se.eventPriority = 50;
    se.applyConstraints = new TargetConstraint[]
    {
        new TargetConstraintIsCardType()
        {
          allowedTypes= new[] { TryGet<CardType>("Leader") }
        }
};
    se.targetConstraints = new TargetConstraint[]
    {
        new TargetConstraintIsCardType()
        {
          allowedTypes= new[] { TryGet<CardType>("Leader") }
            } };
}));



       

   


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnCardPlayed>("Finder")
.WithText("Pick a <keyword=goobers.minorin> to add to your deck")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectApplyXOnCardPlayed;
realData.effectToApply = Get<StatusEffectData>("Custom Finder");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 3;


}));


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDrawn>("Destroy self when drawn")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectApplyXWhenDrawn;
realData.effectToApply = Get<StatusEffectData>("Sacrifice Ally");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 1;


}));



        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenUnitLosesY>("Scrap Frost")
    .WithText("When <keyword=scrap> is lost, apply <{a}> <keyword=frost> to enemies in the row")
    .WithStackable(true)
    .WithCanBeBoosted(true)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenUnitLosesY;

         realData.effectToApply = TryGet<StatusEffectData>("Frost");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
         realData.targetMustBeAlive = false;
         realData.eventPriority = 100;
         realData.whenAllLost = false;
         realData.statusType = "scrap";
         realData.self= true;
     })
     );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenUnitLosesY>("Scrap frenzy")
 .WithText("When <keyword=scrap> is lost, apply x<{a}> <keyword=frenzy> to self")
 .WithStackable(true)
 .WithCanBeBoosted(true)
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectApplyXWhenUnitLosesY;

      realData.effectToApply = TryGet<StatusEffectData>("MultiHit");
      realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      realData.targetMustBeAlive = false;
      realData.eventPriority = 100;
      realData.whenAllLost = false;
      realData.statusType = "scrap";
      realData.self = true;
  })
  );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantTutor>("Money Spent")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectInstantTutor;


realData.eventPriority = 5;
realData.source = StatusEffectInstantTutor.CardSource.Custom;
realData.title = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English).GetString("Vending Machine Noises");
realData.amount = 11;
realData.summonCopy = TryGet<StatusEffectInstantSummon>("Instant Summon Gearhammer In Hand");
realData.customCardList = ["FPopSpice", "FPopBerry","FSBerry","FPopmint","FPopNut","FPopBurn","FPopTeeth","FPopClunk","FPopShroom","FPopCola","PopGold"];


})
);

        statusEffects.Add(
         new StatusEffectDataBuilder(this)
                     .Create<StatusEffectApplyXWhenHit>("Vending Machine go Brrr")
                     .WithStackable(false)
                     .WithCanBeBoosted(false)
                     .SubscribeToAfterAllBuildEvent(data =>
                     {
                         var realData = data as StatusEffectApplyXWhenHit;

                         realData.eventPriority = 999999999;
                         realData.targetMustBeAlive = false;
                         realData.effectToApply = TryGet<StatusEffectData>("Money Spent");
                         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                         realData.attackerConstraints = new[]
                         {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Dollars")
                            }
                        }
                         };
                     }));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Gain Explode")
.WithText("Gain <{a}> <keyword=explode>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXOnTurn;

realData.effectToApply = TryGet<StatusEffectData>("Temporary Explode");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 5;
realData.targetMustBeAlive = false;


})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnHit>("Apply friendly Explode")
.WithText("Apply <{a}> <keyword=goobers.friendlyexplode>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXOnHit;

realData.effectToApply = TryGet<StatusEffectData>("Temporary Friendplode");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
realData.eventPriority = 5;
realData.targetMustBeAlive = false;


})
);
        statusEffects.Add(
       StatusCopy("Summon Fallow", "Summon Bomba Bug")                                                      //Makes a copy of the Summon Fallow effect
      .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
      {
          ((StatusEffectSummon)data).summonCard = TryGet<CardData>("Bomba Bug"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                 //This is because TryGet will try to prefix the name with your GUID. 
      })                                                                          //If that fails, then it uses no GUID-prefixing.
       );


        statusEffects.Add(
   new StatusEffectDataBuilder(this)
   .Create<StatusEffectInstantSummon>("Bomba summon")
   .WithStackable(true)
   .WithCanBeBoosted(false)
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectInstantSummon;

       realData.canSummonMultiple = true;
       realData.targetSummon = TryGet<StatusEffectSummon>("Summon Bomba Bug");
       realData.summonPosition = StatusEffectInstantSummon.Position.InFrontOf;

   }
   ));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHit>("Summon Bomba when hit")
.WithText("When hit, summon <card=goobers.Bomba Bug>")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenHit;

realData.effectToApply = TryGet<StatusEffectData>("Bomba summon");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 5;
realData.targetMustBeAlive = false;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenAllyIsKilled>("When ally halve self")
.WithText("When ally is killed, lose half <keyword=health>")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenAllyIsKilled;

 realData.effectToApply = TryGet<StatusEffectData>("Lose Half Health");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
 realData.eventPriority = 5;
 realData.targetMustBeAlive = false;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantFillXBoardSlots>("Random Goopfly")

.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectInstantFillXBoardSlots;


  realData.eventPriority = 60;
  realData.withCards = new CardData[] { Get<CardData>("Angry GoopFlies"), Get<CardData>("Rager Fly"), Get<CardData>("Heavy Goopfly"), Get<CardData>("Weakening Goopfly")};
    realData.random = true;

}

));
 


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHit>("Hitgoop")
.WithText("When hit, deploy <{a}> Goopfly ally")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenHit;

realData.effectToApply = TryGet<StatusEffectData>("Random Goopfly");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 5;
realData.targetMustBeAlive = false;

          


})
);







        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("While Active Fexplode")
.WithText("While active, enemies gain <{a}> <keyword=goobers.friendlyexplode>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectWhileActiveX;

 realData.effectToApply = TryGet<StatusEffectData>("Temporary Friendplode");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
 realData.eventPriority = 5;
 realData.targetMustBeAlive = false;


})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHit>("Worried")
.WithText("When hit, trigger <card=goobers.Terri>")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
   var realData = data as StatusEffectApplyXWhenHit;

   realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
   realData.eventPriority = 5;
   realData.targetMustBeAlive = false;
    realData.applyConstraints = new[]
                         {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Terri")
                            }
                        }
                         };


})
);


        statusEffects.Add(
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectWhileInHandX>("Snow all")
            .WithText("While in hand, apply <{a}> <keyword=snow>, and retain <keyword=snow> to all units")
            .WithStackable(false)
            .WithCanBeBoosted(false)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectWhileInHandX;

                 realData.effectToApply = TryGet<StatusEffectData>("Snow");
                 realData.applyToFlags = ApplyToFlags.Allies| ApplyToFlags.Enemies;
                 realData.eventPriority = 5;
             })

);


        statusEffects.Add(
       new StatusEffectDataBuilder(this)
           .Create<StatusEffectWhileInHandX>("Halt Snow all")
    .WithStackable(false)
            .WithCanBeBoosted(false)
 
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectWhileInHandX;

                realData.effectToApply = TryGet<StatusEffectData>("HaltSnow");
                realData.applyToFlags = ApplyToFlags.Allies | ApplyToFlags.Enemies;
                realData.eventPriority = 2;
            })

);




        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("While Active Fexplode")
.WithText("While active, enemies gain <{a}> <keyword=goobers.friendlyexplode>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectWhileActiveX;

realData.effectToApply = TryGet<StatusEffectData>("Temporary Friendplode");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
realData.eventPriority = 5;
realData.targetMustBeAlive = false;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("attack gem")
.WithText("While active, <keyword=sap> to allies in the row")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectWhileActiveX;

    realData.eventPriority = 1;
    realData.targetMustBeAlive = true;
    realData.effectToApply = TryGet<StatusEffectData>("SAP");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
}));




        statusEffects.Add(new StatusEffectDataBuilder(this)
    .Create<StatusEffectWhileActiveXBoostableScriptable>("Gain magic")
.WithText("<keyword=sap> - attack to all <{a}>")
    .WithStackable(true)
    .WithCanBeBoosted(false)
    .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
    {
        var se = data as StatusEffectWhileActiveXBoostableScriptable;
        se.scriptableAmount = new ScriptableCurrentStatus() { statusType = Get<StatusEffectData>("SAP").type };
        se.effectToApply = Get<StatusEffectData>("On Turn Add Attack To Allies");
        se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
        se.eventPriority = 100;

    }));


        statusEffects.Add(
        StatusCopy("On Hit Equal Heal To FrontAlly", "Nom nom yummy")
        .WithText("Restore <keyword=health> equal to damage dealt to self")                                       //Since this effect is on Shade Serpent, we modify the description shown.
        .WithTextInsert("")                                                         //Makes a copy of the Summon Fallow effect
        .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
        {
            ((StatusEffectApplyXOnHit)data).effectToApply = TryGet<StatusEffectData>("Heal");
            ((StatusEffectApplyXOnHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

        })                                                                              //If that fails, then it uses no GUID-prefixing.
        );



        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectApplyRandomWhenHit>("Venda When Hit")
          .WithStackable(true)
          .WithCanBeBoosted(true)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectApplyRandomWhenHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
              ((StatusEffectApplyRandomWhenHit)data).eventPriority = 1;
              ((StatusEffectApplyRandomWhenHit)data).targetMustBeAlive = false;
              ((StatusEffectApplyRandomWhenHit)data).effectsToapply = new StatusEffectData[]
              {
                   Get<StatusEffectData>("Spice"),Get<StatusEffectData>("Spice"),Get<StatusEffectData>("Spice"),Get<StatusEffectData>("Spice"),
                  Get<StatusEffectData>("Shell"), Get<StatusEffectData>("Shell"), Get<StatusEffectData>("Shell"), Get<StatusEffectData>("Shell"),
                  Get<StatusEffectData>("Heal"),Get<StatusEffectData>("Heal"),Get<StatusEffectData>("Heal"),Get<StatusEffectData>("Heal"),
                  Get<StatusEffectData>("Reduce Counter"),Get<StatusEffectData>("Reduce Counter"),Get<StatusEffectData>("Reduce Counter"),Get<StatusEffectData>("Reduce Counter"),
                  Get<StatusEffectData>("Scrap"),
                   Get<StatusEffectData>("MultiHit"),

              };
          }
          ));

        statusEffects.Add(new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenHit>("Borrowed Ashi")
   .WithText("When hit, gain x8 <keyword=blings> equal damage taken.")
          .SubscribeToAfterAllBuildEvent(data =>
          {
              var realData = data as StatusEffectApplyXWhenHit;
              {
                  realData.applyEqualAmount = true;
                  realData.effectToApply = Get<StatusEffectData>("Gain Gold");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.targetMustBeAlive = false;
                  realData.equalAmountBonusMult = 7;

              }
          }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("Blood Nightshade")
.WithText("<keyword=sp> required 13 - deal damage to enemies in the row equal amount of current health.")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

    realData.doesDamage = true;
    realData.countsAsHit = true;
    realData.dealDamage = true;
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
realData.requiredAmount = 13;
realData.eventPriority = 50;
    var script = ScriptableObject.CreateInstance<ScriptableCurrentHealth>();
    ((StatusEffectApplyXWhenYAppliedToCustom)data).scriptableAmount = script;
    realData.whenAppliedTypes = new string[1] { "sp" };
realData.whenAppliedToFlags = ApplyToFlags.Self;

})

);
    


        statusEffects.Add(
    new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXWhenAnyoneTakesDamage>("Bleed to expresso")
                .WithText("When anyone takes <keyword=bleed> damage, random ally gains <{a}> <keyword=ex>.")
                .WithStackable(true)
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    var realData = data as StatusEffectApplyXWhenAnyoneTakesDamage;

                    realData.eventPriority = 999999999;
                    realData.targetMustBeAlive = false;
                    realData.effectToApply = TryGet<StatusEffectData>("Expresso");
                    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;
                    realData.targetDamageType = "bleed";



                }));

        statusEffects.Add(
      new StatusEffectDataBuilder(this)
                  .Create<StatusEffectApplyXWhenAnyoneTakesDamage>("Bleed to spice to items")
                  .WithText("When anyone takes <keyword=bleed> damage, apply <{a}> <keyword=spice> to all cards in your hand.")
                  .WithStackable(true)
                  .WithCanBeBoosted(true)
                  .SubscribeToAfterAllBuildEvent(data =>
                  {
                      var realData = data as StatusEffectApplyXWhenAnyoneTakesDamage;

                      realData.eventPriority = 999999999;
                      realData.targetMustBeAlive = false;
                      realData.effectToApply = TryGet<StatusEffectData>("Spice");
                      realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Hand;
                      realData.targetDamageType = "bleed";
                      realData.applyConstraints = new[]
                     {

                        new TargetConstraintDoesAttack()
                      
                         };


                  }));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("Blood attacker all")
.WithText("<keyword=sp> required 20 - gain <{a}> <keyword=attack> and trigger self.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

 realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self| ApplyToFlags.Allies;
 realData.requiredAmount = 20;
 realData.eventPriority = 50;
 realData.whenAppliedTypes = new string[1] { "sp" };
 realData.whenAppliedToFlags = ApplyToFlags.Self;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("Blood Triger")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.requiredAmount = 20;
realData.eventPriority = 50;
realData.whenAppliedTypes = new string[1] { "sp" };
realData.whenAppliedToFlags = ApplyToFlags.Self;

})
);


        statusEffects.Add(
   new StatusEffectDataBuilder(this)
               .Create<StatusEffectApplyXWhenAnyoneTakesDamage>("Bleed trigger self")
               .WithText("When anyone takes <keyword=bleed> damage, trigger self,")
               .WithStackable(false)
               .WithCanBeBoosted(false)
               .SubscribeToAfterAllBuildEvent(data =>
               {
                   var realData = data as StatusEffectApplyXWhenAnyoneTakesDamage;

                   realData.eventPriority = 999;
                   realData.targetMustBeAlive = false;
                   realData.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                   realData.targetDamageType = "bleed";



               }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenAnyoneTakesDamage>("Bleed trigger self2")
            .WithText("and all allies and self gain <{a}> <keyword=attack>.")
            .WithStackable(true)
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectApplyXWhenAnyoneTakesDamage;

                realData.eventPriority = 999999999;
                realData.targetMustBeAlive = false;
                realData.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self| ApplyToFlags.Allies;
                realData.targetDamageType = "bleed";



            }));




        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Darkia comboa")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
  ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Darkia", "goobers.Berry" };
  ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Darkiaberry";
  ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
  ((StatusEffectInstantCombineCard)data).checkDeck = false;
  ((StatusEffectInstantCombineCard)data).checkBoard = true;
  ((StatusEffectInstantCombineCard)data).changeDeck = true;
  ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
})
);

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
 .Create<StatusEffectInstantCombineCard>("Darkia combob")
 .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
 {
     ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Darkia", "goobers.Sugary" };
     ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Darkiasugary";
     ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
     ((StatusEffectInstantCombineCard)data).checkDeck = false;
     ((StatusEffectInstantCombineCard)data).checkBoard = true;
     ((StatusEffectInstantCombineCard)data).changeDeck = true;
     ((StatusEffectInstantCombineCard)data).keepUpgrades = true;
 })
 );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Darkia comboc")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Darkia", "goobers.Odd" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Darkiaodd";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Darkia combod")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    ((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Darkia", "goobers.Blood" };
    ((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Darkiablood";
    ((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
    ((StatusEffectInstantCombineCard)data).checkDeck = false;
    ((StatusEffectInstantCombineCard)data).checkBoard = true;
    ((StatusEffectInstantCombineCard)data).changeDeck = true;
    ((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenHit>("Darkia comboa Act")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenHit;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("Darkia comboa");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                          realData.attackerConstraints = new[]
                          {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Berry")
                            }
                        }
                          };
                      }));



        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("Darkia combob Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Darkia combob");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Sugary")
                            }
                        }
                  };
              }));

        statusEffects.Add(
  new StatusEffectDataBuilder(this)
              .Create<StatusEffectApplyXWhenHit>("Darkia comboc Act")
              .WithStackable(false)
              .WithCanBeBoosted(false)
              .SubscribeToAfterAllBuildEvent(data =>
              {
                  var realData = data as StatusEffectApplyXWhenHit;

                  realData.eventPriority = 999999999;
                  realData.targetMustBeAlive = false;
                  realData.effectToApply = TryGet<StatusEffectData>("Darkia comboc");
                  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                  realData.attackerConstraints = new[]
                  {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Odd")
                            }
                        }
                  };
              }));

        statusEffects.Add(
 new StatusEffectDataBuilder(this)
             .Create<StatusEffectApplyXWhenHit>("Darkia combod Act")
             .WithStackable(false)
             .WithCanBeBoosted(false)
             .SubscribeToAfterAllBuildEvent(data =>
             {
                 var realData = data as StatusEffectApplyXWhenHit;

                 realData.eventPriority = 999999999;
                 realData.targetMustBeAlive = false;
                 realData.effectToApply = TryGet<StatusEffectData>("Darkia combod");
                 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                 realData.attackerConstraints = new[]
                 {

                 new TargetConstraintIsSpecificCard()
                 {
                     allowedCards = new CardData[]
                        {
                            Get<CardData>("Blood")
                        }
                 }
             };
             }));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantCombineCard>("Darkia LV3 combo")
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
((StatusEffectInstantCombineCard)data).cardNames = new string[] { "goobers.Darkiablood", "goobers.Chalice", "goobers.Blood" };
((StatusEffectInstantCombineCard)data).resultingCardName = "goobers.Darkiachalice";
((StatusEffectInstantCombineCard)data).spawnOnBoard = true;
((StatusEffectInstantCombineCard)data).checkDeck = true;
((StatusEffectInstantCombineCard)data).checkBoard = true;
((StatusEffectInstantCombineCard)data).changeDeck = true;
((StatusEffectInstantCombineCard)data).keepUpgrades = true;


}
));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
                      .Create<StatusEffectApplyXWhenHit>("Darkia LV3 combo Act")
                      .WithText("<keyword=goobers.acceptance>")
                      .WithStackable(false)
                      .WithCanBeBoosted(false)
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectApplyXWhenHit;

                          realData.eventPriority = 999999999;
                          realData.targetMustBeAlive = false;
                          realData.effectToApply = TryGet<StatusEffectData>("Darkia LV3 combo");
                          realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                          realData.attackerConstraints = new[]
                          {

                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                              Get<CardData>("Fuse")
                            }
                        }
                          };
                      }));



        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("Blood Expresso")
.WithText("<keyword=sp> required 18 - random ally gains <keyword=ex>.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

    realData.effectToApply = TryGet<StatusEffectData>("Expresso");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;
 realData.requiredAmount = 18;
 realData.eventPriority = 50;
 realData.whenAppliedTypes = new string[1] { "sp" };
 realData.whenAppliedToFlags = ApplyToFlags.Self;

})

);



        statusEffects.Add(
new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXPreTrigger>("No Damage")
            .WithText("No damage.")
            .WithStackable(true)
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var realData = data as StatusEffectApplyXPreTrigger;

                realData.eventPriority = 999999999;
                realData.targetMustBeAlive = false;
                realData.effectToApply = TryGet<StatusEffectData>("Reduce Attack");
                realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;


            }));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
           .Create<StatusEffectApplyXOnHit>("Bling Steal")
           .WithText("Take <{a}> <keyword=blings>")
           .WithStackable(true)
           .WithCanBeBoosted(true)
           .SubscribeToAfterAllBuildEvent(data =>
           {
               var realData = data as StatusEffectApplyXOnHit;

        
               realData.effectToApply = TryGet<StatusEffectData>("Take Gold");
               realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
               realData.equalAmountBonusMult = 2;


           })
   );

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDestroyed>("Destroy to Blings")
.WithText("When destroyed, gain <{a}> <keyword=blings>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectApplyXWhenDestroyed;
realData.effectToApply = Get<StatusEffectData>("Gain Gold");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 1;
realData.targetMustBeAlive = false;


}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
   .Create<StatusEffectApplyXOnTurn>("Frenzy Snowball")
   .WithText("Gain <{a}> <keyword=frenzy>")
   .WithStackable(true)
   .WithCanBeBoosted(true)
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectApplyXOnTurn;


       realData.effectToApply = TryGet<StatusEffectData>("MultiHit");
       realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;


   })
);
        statusEffects.Add(
    new StatusEffectDataBuilder(this)
    .Create<StatusEffectNextPhase>("Mimicgo")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectNextPhase;
    realData.goToNextPhase = true;
    realData.nextPhase = TryGet<CardData>("Mimic");
    realData.preventDeath = true;
    realData.animation = TryGet<StatusEffectNextPhase>("FinalBossPhase2").animation;

}));



        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectChangeTargetModeNoSilence>("Row no silence")
.SubscribeToAfterAllBuildEvent(data =>
{
var se = data as StatusEffectChangeTargetModeNoSilence;
se.targetMode = new TargetModeRow();
}));


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectChangeTargetModeNoSilence>("All ene")
.SubscribeToAfterAllBuildEvent(data =>
{
var se = data as StatusEffectChangeTargetModeNoSilence;
se.targetMode = new TargetModeAll();
}));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("Blood Spice")
.WithText("<keyword=sp> required 18 - random ally gains <keyword=spice>.")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

realData.effectToApply = TryGet<StatusEffectData>("Spice");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Hand;
realData.requiredAmount = 18;
realData.eventPriority = 50;
realData.whenAppliedTypes = new string[1] { "sp" };
realData.whenAppliedToFlags = ApplyToFlags.Self;

})

);


        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Expresso")
.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Expresso");

})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Expresso")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Expresso") as StatusEffectSummon;
                    (data as StatusEffectInstantSummon).withEffects = new StatusEffectData[] { TryGet<StatusEffectData>("Temporary Consume") };
                })
        );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Expresso Service")
.WithText("Gain <{a}> <card=goobers.Expresso> with <keyword=consume> to your hand")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXOnTurn;


realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Expresso");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;


})
);


        statusEffects.Add(
         StatusCopy("While Active Zoomlin When Drawn To Allies In Hand", "While Active Zoomspresso")
        .WithText("While active, <card=goobers.Expresso> gain <keyword=zoomlin>")                                       //Since this effect is on Shade Serpent, we modify the description shown.
        .WithTextInsert("<keyword=spark>")                                                        //Makes a copy of the Summon Fallow effect
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyX;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

            (data as StatusEffectApplyX).effectToApply = TryGet<StatusEffectData>("Temporary Free Espresso");   //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
            (data as StatusEffectWhileActiveX).applyConstraints= new TargetConstraint[]
      {
        new TargetConstraintIsItem()
             


   };
        })

        );



        statusEffects.Add(
       StatusCopy("Temporary Zoomlin", "Temporary Free Espresso")                                                 //Makes a copy of the Summon Fallow effect
      .SubscribeToAfterAllBuildEvent(data =>
      {
          var realData = data as StatusEffectTemporaryTrait;        //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.

          (data as StatusEffectTemporaryTrait).trait = TryGet<TraitData>("Zoomlin");
          (data as StatusEffectTemporaryTrait).targetConstraints = new TargetConstraint[]
   {
        new TargetConstraintIsSpecificCard()
                        {
                           allowedCards = new CardData[] { TryGet<CardData>("Expresso") }

                        }


   };

      })
      );


        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Waffle")
.SubscribeToAfterAllBuildEvent(data =>
{
 (data as StatusEffectSummon).summonCard = TryGet<CardData>("Waffle");

})
);
        statusEffects.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Waffle")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Waffle") as StatusEffectSummon;
                    (data as StatusEffectInstantSummon).withEffects = new StatusEffectData[] { TryGet<StatusEffectData>("Temporary Zoomlin") };
                })
        );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Waffle Service")
.WithText("Gain <{a}> <card=goobers.Waffle> with <keyword=zoomlin> to your hand")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXOnTurn;


    realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Waffle");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;


})
);
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnCardPlayed>("Snow all enemiesw")
.WithText("Apply <{a}> <keyword=snow> to all enemies")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXOnCardPlayed;


realData.effectToApply = TryGet<StatusEffectData>("Snow");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;


})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectWhileActiveX>("Maid Now cabinate")
.WithText("While active, allies in the row gain <keyword=goobers.maid> <{a}>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectWhileActiveX;


realData.effectToApply = TryGet<StatusEffectData>("Temporary Maid");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;


})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantDoubleX>("Double EXP")
.WithText("Double the target's <keyword=sp>")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectInstantDoubleX;


    realData.statusToDouble = TryGet<StatusEffectData>("EXP");


})
);


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDestroyed>("Sac SP")
.WithText("When sacrificed, all allies gain <{a}> <keyword=sp>")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDestroyed;


    realData.effectToApply = TryGet<StatusEffectData>("EXP");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
    realData.sacrificed = true;
    realData.targetMustBeAlive = false;


})
);


        statusEffects.Add(
StatusCopy("Summon Snuffer", "Summon Cuppy")
.WithText("Summon <card=goobers.Cuppy>")
.SubscribeToAfterAllBuildEvent(data =>
{
(data as StatusEffectSummon).summonCard = TryGet<CardData>("Cuppy");

})
);
        statusEffects.Add(
            StatusCopy("Instant Summon BlackGoat", "Instant Summon Cuppy")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    (data as StatusEffectInstantSummon).targetSummon = TryGet<StatusEffectData>("Summon Cuppy") as StatusEffectSummon;
                })
        );



        statusEffects.Add(
      StatusCopy("When Sacrificed Increase Attack To Allies And Summon BlackGoat", "Summon self")
     .WithText("When sacrificed, resummon")                                                     //Makes a copy of the Summon Fallow effect
     .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
     {
         ((StatusEffectApplyXWhenDestroyed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Cuppy"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                                                   //This is because TryGet will try to prefix the name with your GUID. 
     })                                                                          //If that fails, then it uses no GUID-prefixing.
      );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnCardPlayed>("Health increase random")
.WithText("Increase max <keyword=health> by <{a}> to random ally")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXOnCardPlayed;


realData.effectToApply = TryGet<StatusEffectData>("Increase Max Health");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;


})
);


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectChangeTargetModeNoSilence>("All ene")
.SubscribeToAfterAllBuildEvent(data =>
{
   var se = data as StatusEffectChangeTargetModeNoSilence;
   se.targetMode = new TargetModeAll();
}));





        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectChangeTargetMode>("All Weakness")
.WithText("Hits all enemies with <keyword=weakness>")
.SubscribeToAfterAllBuildEvent(data =>
{
    var se = (StatusEffectChangeTargetMode)data;

    var onlyBom = (new TargetConstraintHasStatus());
    onlyBom.status = TryGet<StatusEffectData>("Weakness");
    var targetMode = new TargetModeAll();
    se.targetMode = targetMode;
    targetMode.constraints = [onlyBom];

}));

        //FRIENDLY ASHI STUFF----------------------------------------------------------------------------------------------------------------------------------------------------
        //FRIENDLY ASHI STUFF----------------------------------------------------------------------------------------------------------------------------------------------------
        //FRIENDLY ASHI STUFF----------------------------------------------------------------------------------------------------------------------------------------------------
        //FRIENDLY ASHI STUFF----------------------------------------------------------------------------------------------------------------------------------------------------
        //FRIENDLY ASHI STUFF----------------------------------------------------------------------------------------------------------------------------------------------------
        //FRIENDLY ASHI STUFF----------------------------------------------------------------------------------------------------------------------------------------------------
        //FRIENDLY ASHI STUFF----------------------------------------------------------------------------------------------------------------------------------------------------


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDrawn>("Berrydestroyed")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectApplyXWhenDrawn;
realData.effectToApply = Get<StatusEffectData>("Gain Berry");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 3;
    realData.targetMustBeAlive = false;


}));
       
        
        
        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDrawn>("Sugarydestroyed")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
 var realData = data as StatusEffectApplyXWhenDrawn;
 realData.effectToApply = Get<StatusEffectData>("Gain Sugary");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
 realData.eventPriority = 3;
    realData.targetMustBeAlive = false;


}));


        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDrawn>("OddDestroyed2")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
 var realData = data as StatusEffectApplyXWhenDrawn;
 realData.effectToApply = Get<StatusEffectData>("Gain Odd");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.targetMustBeAlive = false;
 realData.eventPriority = 3;


}));

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDrawn>("BloodDestroyed2")
.WithStackable(true)
.WithCanBeBoosted(true)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectApplyXWhenDrawn;
realData.effectToApply = Get<StatusEffectData>("Gain Blood");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.targetMustBeAlive = false;
realData.eventPriority = 3;


}));
        //FRIENDLY ASHI STUFF ACTUAL----------------------------------------------------------------------------------------------------------------------------------------------

        statusEffects.Add(
    new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenDeployed>("Remove Self on deploy")
    .WithText("<keyword=goobers.oneshot>")
    .WithStackable(true)
    .WithCanBeBoosted(false)
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var realData = data as StatusEffectApplyXWhenDeployed;

        realData.effectToApply = TryGet<StatusEffectData>("Remove");
        realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
        realData.eventPriority = 10000;
    }


        ));

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXWhenDeployed>("kms")

        .WithStackable(true)
        .WithCanBeBoosted(false)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXWhenDeployed;

            realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Ally");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            realData.eventPriority = 1;


        }

        )); 

               statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("To Hand SplitTag")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed;

    realData.effectToApply = TryGet<StatusEffectData>("Instant SplitTag");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.eventPriority = 100;

}
 ));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("To Hand FortuneTag")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed;

    realData.effectToApply = TryGet<StatusEffectData>("Instant FortuneTag");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.eventPriority = 100;

}
 ));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("To Hand WinterTag")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.effectToApply = TryGet<StatusEffectData>("Instant WinterTag");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 100;

}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("To Hand DemonTag")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.effectToApply = TryGet<StatusEffectData>("Instant DemonTag");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 100;

}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("To Hand LuminTag")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.effectToApply = TryGet<StatusEffectData>("Instant LuminTag");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 100;

}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("To Hand NovaTag")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.effectToApply = TryGet<StatusEffectData>("Instant NovaTag");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 100;

}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("To Hand DetonatorTag")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.effectToApply = TryGet<StatusEffectData>("Instant DetonatorTag");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 100;

}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("To Hand TeethTag")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.effectToApply = TryGet<StatusEffectData>("Instant TeethTag");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self; 
realData.eventPriority = 100;

}
));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("To Hand SunTag")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenDeployed;

 realData.effectToApply = TryGet<StatusEffectData>("Instant SunTag");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self; 
realData.eventPriority = 100;

}
)); 
   statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Gain SplitTag")
.WithText("Add <card=goobers.SplitTag> to your deck.")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed;

    realData.effectToApply = TryGet<StatusEffectData>("Add SplitTag");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.eventPriority = 100;

}
 ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add SplitTag")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("SplitTag");

          })
          );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Gain FortuneTag")
.WithText("Add <card=goobers.FortuneTag> to your deck.")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
 var realData = data as StatusEffectApplyXWhenDeployed;

 realData.effectToApply = TryGet<StatusEffectData>("Add FortuneTag");
 realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
 realData.eventPriority = 100;

}
 ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add FortuneTag")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("FortuneTag");

          })
          );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Gain WinterTag")
.WithText("Add <card=goobers.WinterTag> to your deck.")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXWhenDeployed;

  realData.effectToApply = TryGet<StatusEffectData>("Add WinterTag");
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  realData.eventPriority = 100;

}
));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add WinterTag")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("WinterTag");

          })
          );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Gain DemonTag")
.WithText("Add <card=goobers.DemonTag> to your deck.")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXWhenDeployed;

  realData.effectToApply = TryGet<StatusEffectData>("Add DemonTag");
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  realData.eventPriority = 100;

}
));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add DemonTag")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("DemonTag");

          })
          );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Gain LuminTag")
.WithText("Add <card=goobers.LuminTag> to your deck.")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXWhenDeployed;

  realData.effectToApply = TryGet<StatusEffectData>("Add LuminTag");
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  realData.eventPriority = 100;

}
));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add LuminTag")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("LuminTag");

          })
          );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Gain NovaTag")
.WithText("Add <card=goobers.NovaTag> to your deck.")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
   var realData = data as StatusEffectApplyXWhenDeployed;

   realData.effectToApply = TryGet<StatusEffectData>("Add NovaTag");
   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
   realData.eventPriority = 100;

}
));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add NovaTag")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("NovaTag");

          })
          );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Gain DetonatorTag")
.WithText("Add <card=goobers.DetonatorTag> to your deck.")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
   var realData = data as StatusEffectApplyXWhenDeployed;

   realData.effectToApply = TryGet<StatusEffectData>("Add DetonatorTag");
   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
   realData.eventPriority = 100;

}
));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add DetonatorTag")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("DetonatorTag");

          })
          );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Gain TeethTag")
.WithText("Add <card=goobers.TeethTag> to your deck.")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.effectToApply = TryGet<StatusEffectData>("Add TeethTag");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 100;

}
));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add TeethTag")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("TeethTag");

          })
          );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Gain SunTag")
.WithText("Add <card=goobers.SunTag> to your deck.")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.effectToApply = TryGet<StatusEffectData>("Add SunTag");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 100;

}
));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add SunTag")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("SunTag");

          })
          );




        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("To Hand Restag")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.effectToApply = TryGet<StatusEffectData>("Instant Restag");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 100;

}
));
        statusEffects.Add(
     new StatusEffectDataBuilder(this)
     .Create<StatusEffectApplyXWhenDeployed>("Gain Restag")
     .WithText("Add <card=goobers.Restag> to your deck.")
     .WithCanBeBoosted(false)
     .WithTextInsert("")
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenDeployed;

         realData.effectToApply = TryGet<StatusEffectData>("Add Restag");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         realData.eventPriority = 100;

     }
      ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add Restag")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("Restag");

          })
          );

        //-------------------------------------------------Ashi stuff-----------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------Ashi stuff-----------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------Ashi stuff-----------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------Ashi stuff-----------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------Ashi stuff-----------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------Ashi stuff-----------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------Ashi stuff-----------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------Ashi stuff-----------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------Ashi stuff-----------------------------------------------------------------------------------------------------------------

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantShop>("Shoppers")
.WithText("<keyword=goobers.drawa> <{a}>")
.WithCanBeBoosted(true)

);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnCardPlayed>("Shoppers test")
    .WithText("Heyo! Please pick a card, and I shall leave! :3 (Tap to Challenge)")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
 var se = data as StatusEffectApplyXOnCardPlayed;
 se.effectToApply = TryGet<StatusEffectData>("Shoppers");
 se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
 se.eventPriority = 1;
 se.targetMustBeAlive = false;
}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenDeployed>("Oh a customer")
       .WithText("Heyo! Please pick a card, and I shall leave! :3 (Tap to Challenge)")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var se = data as StatusEffectApplyXWhenDeployed;
    se.effectToApply = TryGet<StatusEffectData>("Shoppers");
    se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    se.eventPriority = 1;
    se.targetMustBeAlive = true;
}));



        statusEffects.Add(
new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenYAppliedToCustom>("Oh okay")
 .WithCanBeBoosted(false)
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

      realData.effectToApply = TryGet<StatusEffectData>("AshiBossACT");
      realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      realData.requiredAmount = 100;
      realData.eventPriority = 50;
      realData.whenAppliedTypes = new string[1] { "kitsu" };
      realData.whenAppliedToFlags = ApplyToFlags.Self;

  })
  );
        

                   statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenAllyIsKilled>("Gain Shi when ally killed")
.WithText("Gain 1 <keyword=kitsu> when an ally dies.")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenAllyIsKilled
;

    realData.effectToApply = TryGet<StatusEffectData>("Shi");
    realData.eventPriority = 5;
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenYAppliedTo>("Respect XD")
    .WithText("When my <keyword=kitsu> reaches 10, I destroy myself and reward you! :3")
    .WithCanBeBoosted(false)
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenYAppliedTo;

         realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Ally");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         realData.mustReachAmount = true;
         realData.eventPriority = 2;
         realData.whenAppliedTypes = new string[1] { "kitsu" };
         realData.whenAppliedToFlags = ApplyToFlags.Self;
        

         var script = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
         script.amount = 1;
         ((StatusEffectApplyX)data).scriptableAmount = script;
     }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
 .Create<StatusEffectApplyXWhenDestroyed>("Respect XDX")
 .WithCanBeBoosted(false)
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectApplyXWhenDestroyed;

      realData.effectToApply = TryGet<StatusEffectData>("winner");
      realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      realData.targetMustBeAlive = false;
      realData.eventPriority = 100;
  }));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
   .Create<StatusEffectApplyXWhenDeployed>("Kill allies while active")
   .WithText("Congrats! I'll stick around you, you seem fun! First lets get outta here deploy all the enemies I'll kill em as they come.")
   .WithCanBeBoosted(false)
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var realData = data as StatusEffectApplyXWhenDeployed;

        realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Ally");
        realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
        realData.eventPriority = 9999;
        realData.whenAllyDeployed = true;
    }));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantMultiple>("winner")
.WithText("")
.WithCanBeBoosted(false)
.WithTextInsert("=")
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectInstantMultiple;

realData.effects = new StatusEffectInstant[]
{


      TryGet<StatusEffectInstant>("Instant Summon Ashiloser")

};
}

));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenWin>("New Respect")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenWin;

realData.effectToApply = TryGet<StatusEffectData>("Add Ashi Friendly");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.targetMustBeAlive = false;
realData.eventPriority = 100;
}));
        

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Ashiloser")

.SubscribeToAfterAllBuildEvent(data =>
{
  (data as StatusEffectSummon).summonCard = TryGet<CardData>("Ashiloser");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Ashiloser")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Ashiloser") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Ashiloser");
                      })
                      );








        statusEffects.Add(
     new StatusEffectDataBuilder(this)
     .Create<StatusEffectInstantAddDeck>("Add Ashi Friendly")
     .WithStackable(false)
     .WithCanBeBoosted(false)
     .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
     {
         ((StatusEffectInstantAddDeck)data).card = Get<CardData>("Ashifriendly");

     })
     );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenYAppliedToCustom>("Oh okay")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
   var realData = data as StatusEffectApplyXWhenYAppliedToCustom;

   realData.effectToApply = TryGet<StatusEffectData>("AshiBossACT");
   realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
   realData.requiredAmount = 100;
   realData.eventPriority = 50;
   realData.whenAppliedTypes = new string[1] { "kitsu" };
   realData.whenAppliedToFlags = ApplyToFlags.Self;

})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenHit>("Increase Effect self when hit")
.WithText("When hit, increase own effect by {a}")
.WithStackable(true)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenHit;

    realData.effectToApply = TryGet<StatusEffectData>("Increase Effects");
    realData.eventPriority = 5;
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.targetMustBeAlive = false;


})
);

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDestroyed>("Overloaded")
.WithText("When Destroyed, apply <{a}> <keyword=overload> to enemy in front.")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDestroyed;

    realData.effectToApply = TryGet<StatusEffectData>("Overload");

    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.FrontEnemy;
    realData.eventPriority = 5;


})
);


        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXWhenDeployed>("Die nowAshi")

        .WithStackable(false)
        .WithCanBeBoosted(false)
        .SubscribeToAfterAllBuildEvent(data =>
        {
            var realData = data as StatusEffectApplyXWhenDeployed;

            realData.effectToApply = TryGet<StatusEffectData>("Flee");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
            realData.eventPriority = 100;


        }

        ));
        statusEffects.Add(
  new StatusEffectDataBuilder(this)
  .Create<StatusEffectApplyXWhenDeployed>("Ashi Boss Start")

  .WithStackable(false)
  .WithCanBeBoosted(false)
  .SubscribeToAfterAllBuildEvent(data =>
  {
      var realData = data as StatusEffectApplyXWhenDeployed;

      realData.effectToApply = TryGet<StatusEffectData>("Starto fighto");
      realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      realData.eventPriority = 50;


  }

  ));



        statusEffects.Add(
   new StatusEffectDataBuilder(this)
   .Create<StatusEffectInstantFillXBoardSlots>("Starto fighto")

   .WithStackable(false)
   .WithCanBeBoosted(false)
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectInstantFillXBoardSlots;

   
       realData.eventPriority = 60;
       realData.withCards = new CardData[] { Get<CardData>("Magmi"), Get<CardData>("Fledgli"), Get<CardData>("Shellgi"), Get<CardData>("Obbi"), Get<CardData>("Koi") };


   }

   ));


        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Magmi")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Magmi");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Magmi")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Magmi") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Magmi");
                      })
                      );


        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Fledgli")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Fledgli");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Fledgli")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Fledgli") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Fledgli");
                      })
                      );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Shellgi")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Shellgi");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Shellgi")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Shellgi") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Shellgi");
                      })
                      );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Obbi")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Obbi");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Obbi")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Obbi") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Obbi");
                      })
                      );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Friendzi")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Friendzi");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Friendzi")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Friendzi") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Friendzi");
                      })
                      );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Koi")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Koi");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Koi")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Koi") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Koi");
                      })
                      );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Nom Nom")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Nom Nom");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Nom Nom")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Nom Nom") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Nom Nom");
                      })
                      );
        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Bloodi")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Bloodi");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Bloodi")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Bloodi") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Bloodi");
                      })
                      );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Retali")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Retali");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Retali")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Retali") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Retali");
                      })
                      );

        statusEffects.Add(
StatusCopy("Summon Junk", "Summon Luni")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("Luni");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Luni")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Luni") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Luni");
                      })
                      );

        statusEffects.Add(
  new StatusEffectDataBuilder(this)
  .Create<StatusEffectApplyRandomOnAllyKilled>("Another Friend")
  .WithText("")
  .WithStackable(false)
  .WithCanBeBoosted(false)
  .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
  {
      ((StatusEffectApplyRandomOnAllyKilled)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
      ((StatusEffectApplyRandomOnAllyKilled)data).effectsToapply = new StatusEffectData[]
      {
                   Get<StatusEffectData>("Instant Summon Magmi"),
                   Get<StatusEffectData>("Instant Summon Fledgli"),
                   Get<StatusEffectData>("Instant Summon Shellgi"),
                   Get<StatusEffectData>("Instant Summon Obbi"),
                   Get<StatusEffectData>("Instant Summon Friendzi"),
                   Get<StatusEffectData>("Instant Summon Koi"),
                   Get<StatusEffectData>("Instant Summon Nom Nom"),
                   Get<StatusEffectData>("Instant Summon Bloodi"),
                   Get<StatusEffectData>("Instant Summon Retali"),
                   Get<StatusEffectData>("Instant Summon Luni")


      };
  }
  ));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
   .Create<StatusEffectInstantMultiple>("GOO!")
   .WithText("")
   .WithCanBeBoosted(false)
   .WithTextInsert("=")
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var realData = data as StatusEffectInstantMultiple;

        realData.effects = new StatusEffectInstant[]
    {

     TryGet<StatusEffectInstant>("Instant Summon Fledgli"),
     TryGet<StatusEffectInstant>("Instant Summon Shellgi"),
     TryGet<StatusEffectInstant>("Instant Summon Friendzi"),
     TryGet<StatusEffectInstant>("Instant Summon Nom Nom"),
    };
    }

        ));



        //ASHI SHI POTION EFFECTS-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------



        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Blunky aura")
.WithText("Apply {a} <keyword=block> to enemy deployed,")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.eventPriority = 999999999;
    realData.targetMustBeAlive = true;
    realData.effectToApply = TryGet<StatusEffectData>("Block");
realData.whenEnemyDeployed = true;
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;

}));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Blunky aura2")
.WithText("and {a} <keyword=block> to ally deployed.")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed;

    realData.eventPriority = 999999999;
    realData.targetMustBeAlive = true;
    realData.effectToApply = TryGet<StatusEffectData>("Block");
    realData.whenAllyDeployed = true;
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;

}));


        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Blunky aura3")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectApplyXWhenDeployed;

realData.eventPriority = 999999999;
realData.targetMustBeAlive = true;
realData.effectToApply = TryGet<StatusEffectData>("Block");
realData.whenSelfDeployed = true;
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;

}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectInstantAddRandomCharmToInventory>("Blunky time")

.SubscribeToAfterAllBuildEvent(data =>
{
var realData = data as StatusEffectInstantAddRandomCharmToInventory;

realData.eventPriority = 1;
realData.customList = [TryGet<CardUpgradeData>("BlunkyCharm")];

}));



        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenWin>("Add BlunkyCharm")
.WithText("Add <card=goobers.Blunkycharm> to your inventory at the end of the battle.")
.WithCanBeBoosted(false)
.WithTextInsert("")
.SubscribeToAfterAllBuildEvent(data =>
{
  var realData = data as StatusEffectApplyXWhenWin;

  realData.effectToApply = TryGet<StatusEffectData>("Blunky time");
  realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
  realData.eventPriority = 1;

}
  ));







        statusEffects.Add(
new StatusEffectDataBuilder(this)
  .Create<StatusEffectApplyXWhenWin>("Add Card Launcher")
  .WithText("Add <card=goobers.Card Reciever> to your deck at the end of the battle.")
  .WithCanBeBoosted(false)
  .WithTextInsert("")
   .SubscribeToAfterAllBuildEvent(data =>
   {
       var realData = data as StatusEffectApplyXWhenWin;

       realData.effectToApply = TryGet<StatusEffectData>("Add Card Reciever");
       realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
       realData.eventPriority = 1;

   }
       ));


        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add Card Reciever")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("Card Reciever");

          })
          );




        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Launch Card")
.WithStackable(false)
.WithCanBeBoosted(false)
.WithText("Discard left most card in your hand")
.SubscribeToAfterAllBuildEvent(data =>
{
   var realData = data as StatusEffectApplyXOnTurn;

   realData.effectToApply = TryGet<StatusEffectData>("Flee");
   realData.eventPriority = 1;
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Hand;
    realData.applyConstraints = new[]
                  {

                        new TargetConstraintLeftmostItemInHand()
                  
                       
                   };

}));



        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDestroyed>("VAULT BREACHED")
.WithText("When destroyed, gain 500 <keyword=blings>")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectApplyXWhenDestroyed;
realData.effectToApply = Get<StatusEffectData>("Gain Gold");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 1;
realData.targetMustBeAlive = false;


}));

        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenWin>("A Dollar for you")
.WithText("If battle ends, and this card is not destroyed gain 2 Dollar cards")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
    var realData = data as StatusEffectApplyXWhenWin;
    realData.effectToApply = Get<StatusEffectData>("Add Dollar");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.eventPriority = 1;
    realData.targetMustBeAlive = false;


}));
        statusEffects.Add(new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenWin>("A Dollar for you2")
.WithStackable(false)
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
{
var realData = data as StatusEffectApplyXWhenWin;
realData.effectToApply = Get<StatusEffectData>("Add Dollar");
realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
realData.eventPriority = 2;
realData.targetMustBeAlive = false;


}));


        statusEffects.Add(
         new StatusEffectDataBuilder(this)
         .Create<StatusEffectInstantAddDeck>("Add Dollar")
         .WithStackable(false)
         .WithCanBeBoosted(false)
         .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
         {
             ((StatusEffectInstantAddDeck)data).card = Get<CardData>("Dollars");

         })
         );









        statusEffects.Add(
StatusCopy("Summon Junk", "Summon AshiBoss")

.SubscribeToAfterAllBuildEvent(data =>
{
    (data as StatusEffectSummon).summonCard = TryGet<CardData>("AshiBoss");
})
);

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon AshiBoss")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon AshiBoss") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("AshiBoss");
                      })
                      );


        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectInstantMultiple>("AshiBossACT")
    .WithText("")
    .WithCanBeBoosted(false)
    .WithTextInsert("=")
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectInstantMultiple;

         realData.effects = new StatusEffectInstant[]
     {
     TryGet<StatusEffectInstant>("Sacrifice Ally"),
     TryGet<StatusEffectInstant>("Instant Summon AshiBoss")
     };
     }

         ));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Shroom Random")
.WithStackable(true)
.WithCanBeBoosted(true)
.WithText("Apply <{a}> <keyword=shroom> to a random enemy.")
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXOnTurn;

    realData.effectToApply = TryGet<StatusEffectData>("Shroom");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
    realData.eventPriority = 1;

}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXOnTurn>("Healthy potion")
.WithStackable(true)
.WithCanBeBoosted(true)
.WithText("Increase max <keyword=health> to all allies by <{a}>")
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXOnTurn;

    realData.effectToApply = TryGet<StatusEffectData>("Increase Max Health");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
    realData.eventPriority = 1;

}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("Hehehe")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed;

    realData.effectToApply = TryGet<StatusEffectData>("Shi");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.eventPriority = 100;

})
);


  


        statusEffects.Add(
        StatusCopy("Summon Enemy Leech", "Summon Enemy Restrict")

        .SubscribeToAfterAllBuildEvent(data =>
        {
            (data as StatusEffectSummon).summonCard = TryGet<CardData>("Restrict");
        })
        );

        statusEffects.Add(
                 new StatusEffectDataBuilder(this)
                     .Create<StatusEffectInstantSummonWithCharms>("Instant Summon Restrict")
                     .WithText("...")
                     .WithCanBeBoosted(true)
                     .WithTextInsert("")
                      .SubscribeToAfterAllBuildEvent(data =>
                      {
                          var realData = data as StatusEffectInstantSummonWithCharms;

                          realData.targetSummon = TryGet<StatusEffectData>("Summon Enemy Restrict") as StatusEffectSummon;
                          realData.trueData = TryGet<CardData>("Restrict");
                          realData.summonPosition = StatusEffectInstantSummon.Position.EnemyRow;
                      })
                      );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("When hit deployRes")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed;

    realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Restrict");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.targetMustBeAlive = (false);
    realData.eventPriority = 9999;
    realData.whenSelfDeployed = true;

}));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("When hit deployRes2")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed;

    realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Restrict");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.targetMustBeAlive = (false);
    realData.eventPriority = 9999;
    realData.whenSelfDeployed = true;


}));
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("When hit deployRes3")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed;

    realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Restrict");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.targetMustBeAlive = (false);
    realData.eventPriority = 9999;
    realData.whenSelfDeployed = true;


}));

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenDeployed>("When hit deployRes5")
.WithCanBeBoosted(false)
.SubscribeToAfterAllBuildEvent(data =>
{
    var realData = data as StatusEffectApplyXWhenDeployed; 

    realData.effectToApply = TryGet<StatusEffectData>("Instant Summon Restrict");
    realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
    realData.targetMustBeAlive = (false);
    realData.eventPriority = 9999;
    realData.whenSelfDeployed = true;


}));

  

        statusEffects.Add(
        StatusCopy("Summon Fallow", "Summon RestrictI")
       .WithText("Summon <card=goobers.RestrictI>")                                       //Since this effect is on Shade Serpent, we modify the description shown.
       .WithTextInsert("<card=goobers.GoopFlies>")                                                         //Makes a copy of the Summon Fallow effect
       .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)          //Changes the summoned card to Shade Snake, but not immediately. Once Shade Snake is properly loaded, the delegate is called.
       {
           ((StatusEffectSummon)data).summonCard = TryGet<CardData>("RestrictI"); //Alternatively, I could've put TryGet<CardData>("mhcdc9.wildfrost.tutorial.shadeSnake") or TryGet<CardData>(Extensions.PrefixGUID("shadeSnake",this)) or the Get variants too
                                                                                  //This is because TryGet will try to prefix the name with your GUID. 
       })                                                                          //If that fails, then it uses no GUID-prefixing.
        );
        statusEffects.Add(
        StatusCopy("Instant Summon Fallow", "Instant Summon RestrictI") //Copying Instant Summon Fallow and changing the name.
           .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)   //Replacing the targetSummon with our StatusEffectSummon, once the time is right. 
           {
               ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon RestrictI");
           })
         );

        statusEffects.Add(new StatusEffectDataBuilder(this)
    .Create<StatusEffectChangeTargetMode>("Hit All Allies")
    .SubscribeToAfterAllBuildEvent(data =>
    {
        var se = data as StatusEffectChangeTargetMode;
        se.targetMode = new TargetModeAllAllies();
    }));

        statusEffects.Add(
                StatusCopy("Hit Random Target", "Hit Truly Random Target")
                .WithText("Hits a random target")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectChangeTargetMode)data).targetMode = ScriptableObject.CreateInstance<TargetModeTrulyRandom>();
                    ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
                })
                );

        statusEffects.Add(
               StatusCopy("Hit All Enemies", "Hit All Taunt")
               .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
               {
                   ((StatusEffectChangeTargetMode)data).targetMode = ScriptableObject.CreateInstance<TargetModeTaunt>();
                   ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
               })
               );

        statusEffects.Add(
              StatusCopy("Hit All Enemies", "Hit All Taunt")
              .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
              {
                  ((StatusEffectChangeTargetMode)data).targetMode = ScriptableObject.CreateInstance<TargetModeTaunt>();
                  ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
              })
              );

        //Status 27: While Active Taunted To Enemies
        statusEffects.Add(
            StatusCopy("While Active Aimless To Enemies", "While Active Taunted To Enemies")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Taunted");
                ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
            })
            );

        //Status 28: Temporary Taunted
        statusEffects.Add(
            StatusCopy("Temporary Aimless", "Temporary Taunted")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Taunted");
            })
            );

        statusEffects.Add(
  new StatusEffectDataBuilder(this)
      .Create<StatusEffectApplyXWhenWin>("Add PRestrict")
      .WithText("Add <card=goobers.PRestrictI> to your deck at the end of the battle.")
      .WithCanBeBoosted(false)
      .WithTextInsert("")
       .SubscribeToAfterAllBuildEvent(data =>
       {
           var realData = data as StatusEffectApplyXWhenWin;

           realData.effectToApply = TryGet<StatusEffectData>("Add Restrict");
           realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
           realData.eventPriority = 3;
       }
           ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add Restrict")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("PRestrictI");

          })
          );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenWin>("Add PotionFrenzyI")
    .WithText("Add <card=goobers.PotionFrenzyI> to your deck at the end of the battle.")
    .WithCanBeBoosted(false)
    .WithTextInsert("")
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenWin;

         realData.effectToApply = TryGet<StatusEffectData>("Add PotionFrenzyII");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         realData.eventPriority = 1;
     
     }
         ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add PotionFrenzyII")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("PotionFrenzyI");

          })
          ); 

                    statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenWin>("Add PHealthI")
    .WithText("Add <card=goobers.PHealthI> to your deck at the end of the battle.")
    .WithCanBeBoosted(false)
    .WithTextInsert("")
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenWin;

         realData.effectToApply = TryGet<StatusEffectData>("Add PHealthII");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         realData.eventPriority = 2;
     }
         ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add PHealthII")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("PHealthI");

          })
          ); 

                         statusEffects.Add(
new StatusEffectDataBuilder(this)
    .Create<StatusEffectApplyXWhenWin>("Add PBlockII")
    .WithText("Add <card=goobers.PBlockI> to your deck at the end of the battle.")
    .WithCanBeBoosted(false)
    .WithTextInsert("")
     .SubscribeToAfterAllBuildEvent(data =>
     {
         var realData = data as StatusEffectApplyXWhenWin;

         realData.effectToApply = TryGet<StatusEffectData>("Add PBlockI");
         realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
         realData.eventPriority = 2;
     }
         ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add PBlockI")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("PBlockI");

          })
          );

        statusEffects.Add(
         new StatusEffectDataBuilder(this)
         .Create<StatusEffectApplyXOnTurn>("Kill all")
         .WithText("Kill all enemies.")
         .WithStackable(false)
         .WithCanBeBoosted(false)
         .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
         {
             var realData = data as StatusEffectApplyXOnTurn;

             realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Enemy");
             realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
             realData.eventPriority = 2;
         })
         );

        statusEffects.Add(
        new StatusEffectDataBuilder(this)
        .Create<StatusEffectApplyXOnTurn>("Kill all random")
        .WithText("Kill a random enemy.")
        .WithStackable(false)
        .WithCanBeBoosted(false)
        .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
        {
            var realData = data as StatusEffectApplyXOnTurn;

            realData.effectToApply = TryGet<StatusEffectData>("Sacrifice Enemy");
            realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
            realData.eventPriority = 2;
        })
        );

        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenWin>("Add PNullII")
.WithText("Add <card=goobers.PNullI> to your deck at the end of the battle.")
.WithCanBeBoosted(false)
.WithTextInsert("")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     var realData = data as StatusEffectApplyXWhenWin;

     realData.effectToApply = TryGet<StatusEffectData>("Add PNullI");
     realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
     realData.eventPriority = 2;
     realData.targetMustBeAlive = false;
 }
     ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add PNullI")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("PNullI");

          })
          );
        statusEffects.Add(
new StatusEffectDataBuilder(this)
.Create<StatusEffectApplyXWhenWin>("Add PTimerII")
.WithText("Add <card=goobers.PTimerI> to your deck at the end of the battle.")
.WithCanBeBoosted(false)
.WithTextInsert("")
 .SubscribeToAfterAllBuildEvent(data =>
 {
     var realData = data as StatusEffectApplyXWhenWin;

     realData.effectToApply = TryGet<StatusEffectData>("Add PTimerI");
     realData.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
     realData.eventPriority = 2;
 }
     ));

        statusEffects.Add(
          new StatusEffectDataBuilder(this)
          .Create<StatusEffectInstantAddDeck>("Add PTimerI")
          .WithStackable(false)
          .WithCanBeBoosted(false)
          .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
          {
              ((StatusEffectInstantAddDeck)data).card = Get<CardData>("PTimerI");

          })
          );




        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //ANIMATION EFFECTS SECTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!














        cards = new List<CardDataBuilder>();

        //Code for cards
        //CHARACTER , UNITS CARDS!---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        cards.Add(
            new CardDataBuilder(this).CreateUnit("Inkabom", "Inkabom")
            .SetSprites("Inkabom.png", "Inkabom BG.png")
            .SetStats(8, 1, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Friendly")                                            //All summons are "Summoned". This line is necessary.
            .WithFlavour("mmmm Slweepy")
            .SetAttackEffect(SStack("Null", 2))
            .WithIdleAnimationProfile("GoopAnimationProfile")

             .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
             {
                 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {

        SStack("Hit All Enemies", 1),
        SStack("Ascend Inkabom",1)

                      };

                 data.greetMessages = ["hmm? oh hello, didn't even realize I was frozen hehe, anyways I seem to have lost my stone, maybe the baddies have it. I wanna follow u. -w-"];
             })

            );
        cards.Add(
            new CardDataBuilder(this).CreateUnit("Sunray", "Sunray")
            .SetSprites("Sunray.png", "Sunray BG.png")
            .SetStats(6, null, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Friendly")                                            //All summons are "Summoned". This line is necessary.
            .WithFlavour("Where'd the music go? >:0")
            .SetTraits(
                TStack("Frontline", 1),
                TStack("Fragile", 1)
                )
            .WithText("<keyword=goobers.sunray>")
          
             .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
             {
                 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {
                 SStack("On Card Played Reduce Counter To Allies", 3),
                 SStack("Lets go",1)

                         };
                 data.greetMessages = ["Thanks for saving me! I sure do hope Sunscreen and Sunburn are alright, may I tag along? I gotta make sure they're ok :0"];
             })

            );




        cards.Add(
      new CardDataBuilder(this).CreateUnit("Theinfested", "The Infested One")
       .SetSprites("The Infested One.png", "GOOP BG.png")
       .SetStats(8, null, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
       .WithCardType("Friendly")                                            //All summons are "Summoned". This line is necessary.
       .WithFlavour("AUuuuggHHH???")
       .WithIdleAnimationProfile("GoopAnimationProfile")
       .SetTraits(
              TStack("Smackback", 1)
              )
       .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
       {
           data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
           {
        SStack("Summon GoopFlies", 1),
                };
           data.greetMessages = ["bzzzzzzzzz"];
       })
             );


        cards.Add(
              new CardDataBuilder(this).CreateUnit("Hateu", "Hateu")
              .SetSprites("Hateu.png", "Hateu BG.png")
              .SetStats(3, 9, 2)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
              .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
              .WithFlavour("Damn, Out of Battery!")
              .SetAttackEffect(
                  SStack("Demonize", 1))

              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
                SStack("On Turn Gain Max Counter", 1)

                  };
                  data.greetMessages = ["RAAAAHHH THAT STUPID POLAR BEAR SHOT US OUT OF THE SKY, Let me teach these a@#$%@^ a lesson."];
              })
              );



        cards.Add(
            new CardDataBuilder(this).CreateUnit("M1", "4 Masked Deity - Dormant")
            .SetSprites("Dormant.png", "M1 BG.png")
            .SetStats(12, null, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("")
       
            .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
            {
                data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {
                SStack("When Sacrificed Summon M2",1)

                };
                data.greetMessages = ["..."];
            })
            );

        cards.Add(
           new CardDataBuilder(this).CreateUnit("Momo", "Mo Mo")
           .SetSprites("Mo Mo.png", "Mo Mo BG.png")
           .SetStats(5, 4, 6)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
           .WithCardType("Friendly") 
           .WithText("<keyword=goobers.momoa>")//All summons are "Summoned". This line is necessary.
           .WithFlavour("")
              .SetTraits(
              TStack("Barrage", 1),
               TStack("Fragile", 1))

 
           .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
           {
               data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {
                SStack("When Health Lost Apply Equal Attack To Self And Allies",1)

               };
               data.greetMessages = ["Me spooked..."];
           })
           );

        cards.Add(
           new CardDataBuilder(this).CreateUnit("InkabomA", "Inkabom Ascended")
           .SetSprites("InkabomA.png", "InkabomA BG.png")
           .SetStats(15, 2, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
           .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
           .WithFlavour("")
           .SetAttackEffect(
                  SStack("Null", 3), SStack("Frost", 2))
           .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
           {
               data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {
                SStack("Hit All Enemies",1),
                SStack("ImmuneToSnow",1)


               };
           })
           );
        //CHARACTER , UNITS CARDS 2!-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        cards.Add(
           new CardDataBuilder(this).CreateUnit("Yra", "Yra")
           .SetSprites("YRA.png", "YRA BG.png")
           .SetStats(4, 0, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
           .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.   
            .SetTraits(
              TStack("Spark", 1))
         
           .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
           {
               data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {
                SStack("Random YraBot",1)

               };
               data.greetMessages = ["I can't work with all this snow around, let's take it down."];
           })
           );

        cards.Add(
         new CardDataBuilder(this).CreateUnit("Tala", "Tala")
         .SetSprites("Tala.png", "Tala BG.png")
         .SetStats(7, null, 4)
         .WithCardType("Friendly")
     
         .SetAttackEffect(
                  SStack("Haze", 1))
         .SubscribeToAfterAllBuildEvent(delegate (CardData data)
         {
             data.startWithEffects = new CardData.StatusEffectStacks[]
             {

                SStack("Random Tala",1),
                SStack("On Card Played Damage To Self",2)
             };
             data.greetMessages = ["Thank you, I got fresh fruits ready, they may help in combat."];
         })
         );
        cards.Add(
           new CardDataBuilder(this).CreateUnit("Raven", "Raven")
           .SetSprites("Raven.png", "Raven BG.png")
           .SetStats(4, 3, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
           .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
           .WithFlavour("Hehe Money")
     
             .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
             {
                 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {
                        SStack("On Hit Equal Gold To damage dealt",1),
                        SStack("Oh Candle",150),
                        SStack("Run away together",1)

                 };
                 data.greetMessages = ["Thanks, I think we'll make triple the profit if we team up, so let's team up"];
             })

           );
        cards.Add(
             new CardDataBuilder(this).CreateUnit("Soluna", "Soluna")
             .SetSprites("Soluna.png", "Soluna BG.png")
             .SetStats(2, null, 6)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
             .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
             .WithFlavour("Zzz")
             .SetTraits(
               TStack("Fragile", 1))
           
               .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
               {
                   data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                   {
                        SStack("Increase Max counter in row",2),(SStack("Block", 1))
                   };
                   data.greetMessages = ["zZZZ..."];
               })

             );



        cards.Add(
         new CardDataBuilder(this).CreateUnit("Luvu", "Luvu")
         .SetSprites("Luvu.png", "Luvu BG.png")
         .SetStats(12, 10, 15)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
         .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
         .WithFlavour("I better continue on working or else... Hateu will yell at me again")
     
           .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
           {
               data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {
                        SStack("ReduceMAXTURN",1),
                        SStack("Getting Yelled at",1)
               };
               data.greetMessages = ["Phew man thanks for helping me, can I tag along? I'd hate to run into my boss..."];
           })

         );
        cards.Add(
    new CardDataBuilder(this).CreateUnit("Sharoco", "Sharoco LV1")
    .SetSprites("SharocoLV1.png", "SharocoLV1 BG.png")
    .SetStats(3, 5, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
    .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
    .WithFlavour("Hmm where shark?")
    .WithText($"<keyword={Extensions.PrefixGUID("carry", this)}>")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
      {
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
              SStack("On Kill, Gain Sweet Point Self",10),
              SStack("Choco LV2",40),
              SStack("reset",40)

          };

          data.traits = new List<CardData.TraitStacks>

          { CreateTraitStack("Maid", 2) };
          data.greetMessages = ["Hmm... me join you? *shark noises*"];
      })

    );

        cards.Add(
         new CardDataBuilder(this).CreateUnit("Sharoco2", "Sharoco LV2")
         .SetSprites("SharocoLV2.png", "SharocoLV2 BG.png")
         .SetStats(6, 6, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
         .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
         .WithFlavour("Gooooo")
         .WithText($"<keyword={Extensions.PrefixGUID("carry", this)}>")
           .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
           {
               data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {
                   SStack("On Kill, Gain Sweet Point Self",20),
                   SStack("Choco LV3",100),
                   SStack("MultiHit",1),
                    SStack("reset",100)
               };

               data.traits = new List<CardData.TraitStacks>

          { CreateTraitStack("Maid", 4) };

           })

         );

        cards.Add(
    new CardDataBuilder(this).CreateUnit("Sharoco3", "Sharoco LV3")
    .SetSprites("SharocoLV3.png", "SharocoLV3 BG.png")
    .SetStats(8, 8, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
    .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
    .WithFlavour("Go Sharky")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
      {
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {

                   SStack("MultiHit",1),
                   SStack("Before Attack, Demonize Targets",1)

          };
          data.traits = new List<CardData.TraitStacks>

          { CreateTraitStack("Maid", 6) };

      })

    );

        cards.Add(
    new CardDataBuilder(this).CreateUnit("Newtral", "Newtral")
    .SetSprites("Newtral.png", "Newtral BG.png")
    .SetStats(5, 6, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
    .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
    .WithFlavour("ACK THE SUN")
     .SetTraits(
     TStack("Barrage", 1))

     .WithText($"<keyword={Extensions.PrefixGUID("carry", this)}>")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
      {
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
                   SStack("Gain Sweet Point Self",2),
                   SStack("Burrow go!",6)

          };

          data.greetMessages = ["BRRRRR.... Thanks for saving me, boss must be frozen somewhere too... May I tag along?"];

      })

    );

        cards.Add(
    new CardDataBuilder(this).CreateUnit("UNewtral", "Newtral Burrowed")
    .SetSprites("Sneaky.png", "Sneaky BG.png")
    .SetStats(100000, 0, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
    .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
    .WithFlavour("Brbrbbrbrbrbrbbrrrr")
     .SetAttackEffect(
                  SStack("Weakness", 3))
     .WithText($"<keyword={Extensions.PrefixGUID("carry", this)}>")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
      {
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
              SStack("Gain Sweet Point Self",4),
              SStack("UP go!",4),
               SStack("ELU",1000)
          };

      })

    );

        cards.Add(
    new CardDataBuilder(this).CreateUnit("Sunburn", "Sunburn")
    .SetSprites("Sunburn.png", "Sunburn BG.png")
    .SetStats(2, 4, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
    .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
    .WithFlavour("Sunshine ready to roll!!!")

      .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
      {
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {

                 SStack("Double Spice All",1),
                 SStack("Spice row",4)

          };
          data.greetMessages = ["WOOO that fall was wicked! I wonder where the other two landed... Can I join you? it sucks travelling alone XD"];
      })

    );

        cards.Add(
  new CardDataBuilder(this).CreateUnit("Sunscreen", "Sunscreen")
  .SetSprites("Sunscreen.png", "Sunscreen BG.png")
  .SetStats(4, 1, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
  .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
  .WithFlavour("Whistles")
  .SetTraits(
     TStack("Backline", 1))

   .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
   {
       data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
       {

              SStack("Double Frost enemies row",1),
              SStack("Frost rowN",2)
       };
       data.greetMessages = ["Hnnngggg... Where did those two run off to..? Oh, Can I come with? I'll be sure to be of assistence"];
   })
  );
        cards.Add(
   new CardDataBuilder(this).CreateUnit("Solar", "Solar Flares")
   .SetSprites("Solar.png", "Solar BG.png")
   .SetStats(18, 12, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
   .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
   .WithFlavour("WOOOO!!!!")
    .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
    {
        data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {

              SStack("Spice now",5),
              SStack("Spice now2",5),
              SStack("On Card Played Reduce Counter To Allies", 4),
              SStack("Frost row",5)


        };
    })
   );


        //PET CARDS!PET CARDS!PET CARDS!PET CARDS!PET CARDS!PET CARDS!PET CARDS!PET CARDS!PET CARDS!---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        cards.Add(
              new CardDataBuilder(this).CreateUnit("Angy", "Angy")
              .SetSprites("Angy.png", "Angy BG.png")
              .SetStats(5, 3, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
              .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
              .WithFlavour("Oink Oink!")
                .SetTraits(
               TStack("Smackback", 1))
              .IsPet("", true)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                    {
                        SStack("Set Attack to Neutral After Hit",3),
                        SStack("Retaliate", 2)

                    };
                })

              );

        cards.Add(
              new CardDataBuilder(this).CreateUnit("Poochie", "Poochie")
              .SetSprites("Poochi.png", "Poochi BG.png")
              .SetStats(2, 2, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
              .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
              .WithFlavour("bork bork")
              .IsPet("", true)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                    {
                        SStack("While Active Spark When Drawn To Allies In Hand",1)


                    };
                })

              );
        cards.Add(
            new CardDataBuilder(this).CreateUnit("CB23O", "CB-23 Pawn")
            .SetSprites("CB23P.png", "CB23P BG.png")
            .SetStats(4, 3, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("ONWARD FRIEND! :D")
            .IsPet("", true)
            .WithText($"<keyword={Extensions.PrefixGUID("carry", this)}>")
              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
                        SStack("Gain Sweet Point Self",2),
                        SStack("Perma into CB23K",10),
                          SStack("reset",10)

                  };
              })

            );

        //Entirety of CB23!---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        cards.Add(
            new CardDataBuilder(this).CreateUnit("CB23K", "CB-23 Knight")
            .SetSprites("CB23K.png", "CB23K BG.png")
            .SetStats(6, 3, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("Let me at them.")
            .WithText($"<keyword={Extensions.PrefixGUID("carry", this)}>")
              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
                         SStack("Gain Sweet Point Self",2),
                         SStack("On Card Played Apply Attack To Self", 1),
                         SStack("Perma into CB23B",16),
                         SStack("Perma into CB23R",20),
                           SStack("reset",20)
                  };
              })

            );

        cards.Add(
            new CardDataBuilder(this).CreateUnit("CB23B", "CB-23 Bishop")
            .SetSprites("CB23B.png", "CB23B BG.png")
            .SetStats(9, 2, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("ONWARD FRIEND! :D")
            .WithText($"<keyword={Extensions.PrefixGUID("carry", this)}>")
              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
                      SStack("Perma into CB23KG",80),
                         SStack("When Ally Is Healed Apply Equal EXP",1),
                         SStack("On Turn Add Health & Attack To Allies",1),
                           SStack("reset",80)


                  };
              })

            );


        cards.Add(
            new CardDataBuilder(this).CreateUnit("CB23R", "CB-23 Rook")
            .SetSprites("CB23R.png", "CB23R BG.png")
            .SetStats(10, 7, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("ONWARD FRIEND! :D")
            .SetTraits(
          TStack("Barrage", 1))
            .WithText($"<keyword={Extensions.PrefixGUID("carry", this)}>")
              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {

                         SStack("On Card Played Apply Attack To Self", 2),
                         SStack("On Hit Apply Equal EXP ",1),
                         SStack("Perma into CB23Q",180),
                           SStack("reset",180)
                  };
              })

            );

        cards.Add(
            new CardDataBuilder(this).CreateUnit("CB23KG", "CB-23 King")
            .SetSprites("CB23KING.png", "CB23KING BG.png")
            .SetStats(13, 2, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("ONWARD FRIEND! :D")
            .WithText($"<keyword={Extensions.PrefixGUID("carry", this)}>")
              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
                         SStack("When Ally Is Healed Apply Equal EXP",1),
                         SStack("On Turn Add Health & Attack To Allies",3),
                         SStack("Support more",80),
                         SStack("reset",80)



                  };
              })

            );

        cards.Add(
            new CardDataBuilder(this).CreateUnit("CB23Q", "CB-23 Queen")
            .SetSprites("CB23Q.png", "CB23Q BG.png")
            .SetStats(12, 10, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Friendly")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("ONWARD FRIEND! :D")
              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
                      SStack("Hit All Enemies",1),
                      SStack("Gain Sweet Point Self",5),
                      SStack("On Card Played Apply Attack To Self", 2),
                      SStack("Double up when sweet points",25),
                      SStack("reset",25)

                  };
              })

            );
        //SUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONSSUMMONS!---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        cards.Add(
              new CardDataBuilder(this).CreateUnit("GoopFlies", "Goopflies")
              .SetSprites("GoopFlies.png", "GOOP BG.png")
              .SetStats(2, 1, 2)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
              .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
              .WithFlavour("BZZZZZZZZZZZZZZZZZZZZZ")
              .SetAttackEffect(SStack("Shroom", 2))
              .SetTraits(new CardData.TraitStacks(Get<TraitData>("Explode"), 2))
              .SetStartWithEffect(SStack("MultiHit", 1))
              );

        //FOR THE 4 FACED FIGURE----------------------------------
        cards.Add(
              new CardDataBuilder(this).CreateUnit("M2", "4 Masked Deity - Executioner")
              .SetSprites("M2.png", "M2 BG.png")
              .SetStats(3, 15, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
              .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
              .WithFlavour("SOOO SADDD")
              .SetTraits(
               TStack("Barrage", 1))
              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
              
                   SStack("Summon M3",1),
                   SStack("Destroy Self After Turn",1)

                  };
              })
              );
        cards.Add(
              new CardDataBuilder(this).CreateUnit("AM2", "Ascended 4 Masked Deity - Executioner")
              .SetSprites("M2.png", "M2 BG.png")
              .SetStats(6, 7, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
              .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
              .WithFlavour("SOOO SADDD")
              .SetTraits(
               TStack("Barrage", 1))
              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
                       SStack("When Destroyed Summon AM3",1)

                  };
              })
              );

        cards.Add(
              new CardDataBuilder(this).CreateUnit("M3", "4 Masked Deity - Cremator")
              .SetSprites("M3.png", "M3 BG.png")
              .SetStats(4, 1, 3)                                                //Shade Snake has 4 health, 3 attack, and no timer.
              .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
              .WithFlavour("AHAHAHAHHAHAHAHAH")
              .SetTraits(TStack("Barrage", 1))
              .SetAttackEffect(SStack("Overload", 8))
              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
                  
                   SStack("Summon M4",1),
                   SStack("Destroy Self After Turn",1)

                  };
              })
              );

        cards.Add(
              new CardDataBuilder(this).CreateUnit("AM3", "Ascended 4 Masked Deity - Cremator")
              .SetSprites("M3.png", "M3 BG.png")
              .SetStats(6, 2, 4)                                                //Shade Snake has 4 health, 3 attack, and no timer.
              .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
              .WithFlavour("AHAHAHAHHAHAHAHAH")
              .SetTraits(TStack("Barrage", 1))
              .SetAttackEffect(SStack("Overload", 3))
              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {

                      SStack("When Destroyed Summon AM4",1)
                  };
              })
              );
        cards.Add(
            new CardDataBuilder(this).CreateUnit("M4", "4 Masked Deity - Peace")
            .SetSprites("M4.png", "M4 BG.png")
            .SetStats(5, null, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("Oh stop...")
            .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
            {
                data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {
                   SStack("On Turn Heal & Cleanse Allies",5),
                   SStack("Heal Self",5),
                   SStack("When Sacrificed Summon M2 2",1)

                };
            })
              );

        cards.Add(
          new CardDataBuilder(this).CreateUnit("AM4", "Ascended 4 Masked Deity - Peace")
          .SetSprites("M4.png", "M4 BG.png")
          .SetStats(10, null, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
          .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
          .WithFlavour("Oh stop...")
          .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
          {
              data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
              {
                   SStack("On Turn Heal & Cleanse Allies",3),
                   SStack("Heal Self",5)

              };
          })
            );
        //FOR LEADERS-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------



        cards.Add(
           new CardDataBuilder(this).CreateUnit("Bucket", "Bucket")
           .SetSprites("BUCKET.png", "BUCKET BG.png")
           .SetStats(12, 5, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
           .WithCardType("Leader")                                        //All summons are "Summoned". This line is necessary.
           .WithFlavour("NO I CANT RIFT AWAY WAAAAAAAAAAAAAAAAAH NOT FAIR!")



           .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
           {
               data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {
                SStack("Add Rift to Hand", 1)

               };

               data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
               {
            GiveUpgrade()                   //By our definition, no argument will give a crown
               };
           })

           );


        cards.Add(
              new CardDataBuilder(this).CreateUnit("Cea", "Cea")
              .SetSprites("CEA.png", "CEA BG.png")
              .SetStats(8, 4, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
              .WithCardType("Leader")                                        //All summons are "Summoned". This line is necessary.
              .WithFlavour("GASP IM NOT GLASS!")

              .SetAttackEffect(
                  SStack("Frost", 3))

              .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

              {
                  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
                      SStack("Reduce ally behind Counter", 3),
                      SStack("Scrap", 2)

                  };


                  data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
               {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
               };
              })

              );
        cards.Add(
             new CardDataBuilder(this).CreateUnit("Strike", "Strike")
             .SetSprites("Astrit.png", "Astrit BG.png")
             .SetStats(6, 2, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
             .WithCardType("Leader")                                        //All summons are "Summoned". This line is necessary.
             .WithFlavour("ehhehehehehhehehe")
              .SetAttackEffect(
                  SStack("Overload", 3))

             .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

             {
                 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {
                        SStack("When Deployed Copy Effects of Enemy Front",1),

                      SStack("Scrap",2),



                 };


                 data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
              {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
              };
             })

             );










        cards.Add(
             new CardDataBuilder(this).CreateUnit("?", "???")
             .SetSprites("Redone.png", "Bone BG.png")
             .SetStats(12, 1, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
             .WithCardType("Leader")                                        //All summons are "Summoned". This line is necessary.
             .WithFlavour("...")


             .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

             {
                 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {
                      SStack("Teeth",3),
                      SStack("Add Bone Needle to Hand",2),
                      SStack("Block",1)

                 };


                 data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
              {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
              };
             })

             );

        cards.Add(
             new CardDataBuilder(this).CreateUnit("Sasha", "Sasha")
             .SetSprites("Sasha.png", "Sasha BG.png")
             .SetStats(7, 3, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
             .WithCardType("Leader")                                        //All summons are "Summoned". This line is necessary.
             .WithFlavour("Oh dear where did my nuts go?? :0")


             .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

             {
                 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {
                      SStack("Shell",7),
                      SStack("On Turn Apply Shell To Self",2),
                      SStack("On Hit Equal Shell To All Allies in the Row",1)
                 };


                 data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
              {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
              };
             })

             );



        cards.Add(
            new CardDataBuilder(this).CreateUnit("Terri", "Tiramisu")
            .SetSprites("Terrimisu.png", "Terrimisu BG.png")
            .SetStats(6, 0, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Leader")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("AAAAAAA COFFEE HELP!")
            .SetTraits(
               TStack("Backline", 1))//All summons are "Summoned". This line is necessary.

            .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

            {
                data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {
                     SStack("On Card Played Trigger RandomAlly",1)
                };
            }
            )
            );
        cards.Add(
          new CardDataBuilder(this).CreateUnit("Coffee", "Coffee")
          .SetSprites("Coffee.png", "Terrimisu BG.png")
          .SetStats(14, 10, 8)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
          .WithCardType("Leader")

          .WithFlavour("*protective noises*")


          .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

          {
              data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
              {
                     SStack("When Deployed Summon Terrimisu",1),
                     SStack("Worried",1)
              };


              data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
           {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
           };
          })

          );

        cards.Add(
            new CardDataBuilder(this).CreateUnit("Terror", "Terrormisu")
            .SetSprites("Terrormisu.png", "Terrormisu BG.png")
            .SetStats(100, null, 1)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Leader")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("*Coughs* I will kill them all")
            .WithText($"<keyword={Extensions.PrefixGUID("nuh", this)}>")

            .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

            {
                data.charmSlots = -10;
                data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {
                     SStack("Gain Sweet Point Self terror",1),
                     SStack("Not Fast Enough",11),
                     SStack("FullImmuneToSnow",1),
                     SStack("FullImmuneToInk",1),
                     SStack("Trigger Front",1),
                      SStack("Shell",200)
                };
                data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
           {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
           };
            })
            );

        cards.Add(
            new CardDataBuilder(this).CreateUnit("Cherry", "Straw Cherry")
            .SetSprites("StrawC.png", "StrawC BG.png")
            .SetStats(12, 4, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
            .WithCardType("Leader")                                        //All summons are "Summoned". This line is necessary.
            .WithFlavour("Woo Hoo!!!")
            .SetTraits(
                TStack("Aimless", 1))
            .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

            {
                data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {

                     SStack("Splash",3)

               };
                data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {
                   SStack("Add CTB to Hand",3),
                   SStack("Add CTB to Hands",2)
                };
                data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
           {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
           };
            })
            );

        //FOR ITEMS AND CLUNKERS-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        cards.Add(
            new CardDataBuilder(this).CreateItem("Bone Needle", "Bone Needle")
            .SetSprites("Bone.png", "Bone BG.png")
            .SetStats(null, 0)
            .WithCardType("Item")
             .SetTraits(
                TStack("Zoomlin", 1),
                TStack("Combo", 1),
                TStack("Consume", 1))
             .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
             {
                 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {
                SStack("On Hit Equal Teeth To ???",1),
                SStack("Gain Attack Before Hit",3),
                 };
             })
        );
        cards.Add(
            new CardDataBuilder(this).CreateItem("UBone Needle", "Universal Bone Needle")
            .SetSprites("UBone.png", "UBone BG.png")
            .SetStats(null, 4)
            .WithCardType("Item")
             .SetTraits(
                TStack("Consume", 1))
             .WithValue(65)
             .AddPool("GeneralItemPool")
             .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
             {
                 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {

                SStack("On Hit Equal Teeth To All Allies",1)
                 };
             })
        );
        cards.Add(
            new CardDataBuilder(this).CreateItem("Inkabomb", "Inkabom's Strike")
            .SetSprites("Inkabomb.png", "Inkabomb BG.png")
            .SetStats(null, 5)
            .WithCardType("Item")
            .SetAttackEffect(SStack("Null", 1))
            .AddPool("GeneralItemPool")
            .WithValue(53)
             .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
             {
                 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {
                SStack("Gain Frenzy",1),
                SStack("Hit All Inkd Enemies", 1),
                SStack("MultiHit",1)

                 };
             })
        );
        cards.Add(
            new CardDataBuilder(this).CreateItem("ALLINONE", "All in One!")
            .SetSprites("Allinone.png", "Allinone BG.png")
            .SetStats(null, 8)
            .WithCardType("Item")
            .SetTraits(
                TStack("Consume", 1))
            .AddPool("GeneralItemPool")
            .WithValue(102)
             .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
             {

                 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {
                SStack("Remove Self after use",1),

                };
                 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {
                SStack("Snow",4),
                SStack("Null",3),
                SStack("Shroom",4),
                SStack("Overload",4),
                SStack("Demonize",4),
                SStack("Weakness",4),
                SStack("Frost",3),
                SStack("Haze",1)


                 };
             })
           );
        cards.Add(
           new CardDataBuilder(this).CreateUnit("Rift", "Crystal Rift")
           .SetSprites("RIFT.png", "RIFT BG.png")
           .SetStats(null, 3, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
           .WithCardType("Clunker")                                        //All summons are "Summoned". This line is necessary.
           .WithFlavour("*Woom woom woom*")
           .SetTraits(
               TStack("Pigheaded", 1))
          .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
          {
              data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
              {
                SStack("When Destroyed Apply Damage To Attacker with text",5),
                SStack("Trigger When Bucket Attacks",1),
                SStack("Scrap",1)
              };
          })

           );

        cards.Add(
           new CardDataBuilder(this).CreateUnit("ARift", "Ascended Crystal Rift")
           .SetSprites("RIFT.png", "RIFT BG.png")
           .SetStats(2, 5, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
           .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
           .WithFlavour("*Woom woom woom*")
           .SetTraits(
               TStack("Frontline", 1))
          .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
          {
              data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
              {
                SStack("When Hit Equal Damage To Attacker",1),
                SStack("Trigger When Bucket Attacks",1)
              };
          })

           );
        cards.Add(
      new CardDataBuilder(this).CreateItem("BatteryAcid", "Battery Acid")
      .SetSprites("Battery Acid.png", "Battery Acid BG.png")
      .SetStats(null, null)
      .WithCardType("Item")
      .WithFlavour("HIT EM!")
      .SetAttackEffect(SStack("Overload", 2),
       SStack("Increase Attack", 3))
      .AddPool("GeneralItemPool")
      .WithValue(43)

);

        cards.Add(
        new CardDataBuilder(this).CreateItem("RSS", "Radioactive Shiz Storm")
        .SetSprites("RSS.png", "RSS BG.png")
        .SetStats(null, 1)
        .WithCardType("Item")
        .WithFlavour("HIT EM!")
        .SetAttackEffect(SStack("Overload", 1))
        .AddPool("GeneralItemPool")
        .WithValue(53)
        .SubscribeToAfterAllBuildEvent(delegate (CardData data)
        {
            data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
           {

       SStack("Hit All Enemies",1)
               };
        })
           );
        //TALA ITEMS---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        cards.Add(
       new CardDataBuilder(this).CreateItem("Nightshade", "Weaponized Nightshade")
       .SetSprites("Nightshade.png", "Tala BG.png")
       .SetStats(null, null)
       .WithCardType("Item")
       .WithFlavour("")
       .SetTraits(
               TStack("Zoomlin", 1), TStack("Consume", 1))

        .SubscribeToAfterAllBuildEvent(delegate (CardData data)
        {
            data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
           {

       SStack("Reduce AttackN", 2),
       SStack("Shroom",3)
               };
        })
           );
        cards.Add(
       new CardDataBuilder(this).CreateItem("Tomatoes", "Special Tomatoes")
       .SetSprites("Tomato.png", "Tala BG.png")
       .SetStats(null, null)
       .WithCardType("Item")
       .WithFlavour("")
       .SetTraits(
               TStack("Zoomlin", 1), TStack("Consume", 1))
       .SetAttackEffect(SStack("Increase Attack", 3))
       .CanPlayOnHand(true)
       );

        //ALL GREASYPOPS-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        cards.Add(
   new CardDataBuilder(this).CreateItem("PopCola", "Greasypop: Cola Splash")
   .SetSprites("Cola.png", "Cola BG.png")
   .SetStats(null, 0)
   .WithCardType("Item")
   .WithFlavour("HIT EM!")
   .AddPool("GeneralItemPool")
   .SetTraits(TStack("Barrage", 1))
   .SetAttackEffect(SStack("Null", 3),
                 SStack("Heal", 1))
   .WithValue(42)

      );
        cards.Add(
  new CardDataBuilder(this).CreateItem("FPopCola", "Fresh Greasypop: Cola Splash")
  .SetSprites("Cola.png", "Fresh.png")
  .SetStats(null, 0)
  .WithCardType("Item")
  .WithFlavour("HIT EM!")
  .SetTraits(TStack("Barrage", 1), TStack("Consume", 1))
  .SetAttackEffect(SStack("Null", 3),
                SStack("Reduce Effects", 1))

     );

        cards.Add(
 new CardDataBuilder(this).CreateItem("PopNut", "Greasypop: Nutty Peanut")
 .SetSprites("Nut.png", "Nut BG.png")
 .SetStats(null, null)
 .WithCardType("Item")
 .WithFlavour("HIT EM!")
 .SetStartWithEffect(SStack("MultiHit", 1))
 .SetTraits(TStack("Aimless", 1))
 .SetAttackEffect(SStack("Shell", 2))
 .AddPool("GeneralItemPool")
 .WithValue(56)
    );
        cards.Add(
        new CardDataBuilder(this).CreateItem("FPopNut", "Fresh Greasypop: Nutty Peanut")
.SetSprites("Nut.png", "Fresh.png")
.SetStats(null, null)
.WithCardType("Item")
.WithFlavour("HIT EM!")
.SetStartWithEffect(SStack("MultiHit", 1))
.SetTraits(TStack("Aimless", 1), TStack("Consume", 1))
.SetAttackEffect(SStack("Shell", 8))
    );
        cards.Add(
 new CardDataBuilder(this).CreateItem("PopBurn", "Greasypop: Hyper Burn")
 .SetSprites("Burn.png", "Burn BG.png")
 .SetStats(null, 0)
 .WithCardType("Item")
 .WithFlavour("HIT EM!")
 .SetAttackEffect(SStack("Overload", 3),
                  SStack("Increase Max Health", 3))
 .AddPool("GeneralItemPool")
 .WithValue(55)

    );
        cards.Add(
        new CardDataBuilder(this).CreateItem("FPopBurn", "Fresh Greasypop: Hyper Burn")
.SetSprites("Burn.png", "Fresh.png")
.SetStats(null, 0)
.WithCardType("Item")
.WithFlavour("HIT EM!")
.SetTraits(TStack("Consume", 1))
.SetAttackEffect(SStack("Overload", 4),
                 SStack("Increase Max Health", 1))
 );
        cards.Add(
 new CardDataBuilder(this).CreateItem("PopTeeth", "Greasypop: Funny Bone")
 .SetSprites("Teeth.png", "Teeth BG.png")
 .SetStats(null, null)
 .WithCardType("Item")
 .WithFlavour("HIT EM!")
 .SetAttackEffect(SStack("Teeth", 3))
 .AddPool("GeneralItemPool")
 .WithValue(56)
 );
        cards.Add(
       new CardDataBuilder(this).CreateItem("FPopTeeth", "Fresh Greasypop: Funny Bone")
       .SetSprites("Teeth.png", "Fresh.png")
       .SetStats(null, null)
       .WithCardType("Item")
       .WithFlavour("HIT EM!")
       .SetAttackEffect(SStack("Teeth", 7))
       .SetTraits(TStack("Consume", 1))

          );
        cards.Add(
 new CardDataBuilder(this).CreateItem("PopClunk", "Greasypop: Clunky Bunky")
 .SetSprites("Wood.png", "Wood BG.png")
 .SetStats(null, null)
 .WithCardType("Item")
 .WithFlavour("HIT EM!")
 .SetAttackEffect(SStack("Instant Add Scrap", 1))
 .AddPool("GeneralItemPool")
 .WithValue(62)
    );
        cards.Add(
 new CardDataBuilder(this).CreateItem("FPopClunk", "Fresh Greasypop: Clunky Bunky")
 .SetSprites("Wood.png", "Fresh.png")
 .SetStats(null, null)
 .WithCardType("Item")
 .WithFlavour("HIT EM!")
 .SetAttackEffect(SStack("Instant Add Scrap", 2))
 .SetTraits(TStack("Consume", 1))

    );

        cards.Add(
 new CardDataBuilder(this).CreateItem("PopShroom", "Greasypop: Toxic Tonic")
 .SetSprites("Shroom.png", "Shroom BG.png")
 .SetStats(null, 0)
 .WithCardType("Item")
 .WithFlavour("HIT EM!")
 .SetAttackEffect(SStack("Shroom", 2))
 .AddPool("GeneralItemPool")
  .WithValue(42)
    );
        cards.Add(
 new CardDataBuilder(this).CreateItem("FPopShroom", "Fresh Greasypop: Toxic Tonic")
 .SetSprites("Shroom.png", "Fresh.png")
 .SetStats(null, 0)
 .WithCardType("Item")
 .WithFlavour("HIT EM!")
 .SetTraits(TStack("Barrage", 1), TStack("Consume", 1))
 .SetAttackEffect(SStack("Shroom", 6))

    );
        cards.Add(
new CardDataBuilder(this).CreateItem("DPopGold", "A Drop Of Greasypop: Golden Paradise")
.SetSprites("A DROP.png", "Fresh.png")
.SetStats(null, null)
.WithCardType("Item")
.WithFlavour("HIT EM!")
.SetTraits(TStack("Aimless", 1))
.SetTraits(TStack("Consume", 1))
.SetAttackEffect(SStack("Block", 1),
                  SStack("Shell", 1),
                  SStack("Spice", 1),
                  SStack("Increase Max Health", 1),
                  SStack("Increase Attack", 1))
.WithValue(59)
   .AddPool("GeneralItemPool")
);

        cards.Add(
  new CardDataBuilder(this).CreateItem("PopGold", "Fresh Greasypop: Golden Paradise")
  .SetSprites("GSoda.png", "GOLD BG.png")
  .SetStats(null, null)
  .WithCardType("Item")
  .WithFlavour("HIT EM!")
  .SetTraits(TStack("Consume", 1))
  .SetAttackEffect(SStack("Block", 6),
                   SStack("Shell", 8),
                   SStack("Spice", 8),
                   SStack("Increase Max Health", 8),
                   SStack("Increase Attack", 8),
                   SStack("MultiHit", 5))
     );

        cards.Add(
  new CardDataBuilder(this).CreateItem("Inky Ritual Stone", "Inky Ritual Stone")
  .SetSprites("Ink Stone.png", "Ink Stone BG.png")
  .SetStats(null, 0)
  .WithCardType("Item")
  .SetTraits(
              TStack("Zoomlin", 1))
  .WithFlavour("A stone with Ink trapped inside.")
  .WithValue(12)
     );

        cards.Add(
  new CardDataBuilder(this).CreateItem("Candle", "Candle's Blessing")
  .SetSprites("Candle.png", "Candle BG.png")
  .SetStats(null, 0)
  .WithCardType("Item")
  .SetTraits(
              TStack("Consume", 1))
   .AddPool("GeneralItemPool")
  .WithFlavour("A stone with Ink trapped inside.")
  .WithValue(61)
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)

  {
      data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {

           SStack("Increase Attack",3),
           SStack("Reduce Counter",3)


      };
  })

     );
        cards.Add(
  new CardDataBuilder(this).CreateItem("BlossomH", "Blossom of Healing")
  .SetSprites("HEAL BLOS.png", "HEAL BLOS BG.png")
  .SetStats(null, null)
  .WithCardType("Item")
   .AddPool("GeneralItemPool")
  .WithFlavour("A stone with Ink trapped inside.")
  .CanPlayOnBoard(true)
  .CanPlayOnFriendly(false)
  .CanPlayOnHand(false)
  .CanPlayOnEnemy(false)
  .WithValue(58)
  .WithText($"<keyword={Extensions.PrefixGUID("unplayable", this)}>")
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)

  {
      data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {

           SStack("When Drawn Heal",2)



      };
  })

     );
        cards.Add(
        new CardDataBuilder(this).CreateItem("ChimW", "Weakening Wind Chime")
  .SetSprites("Weak Chim.png", "Weak Chim BG.png")
  .SetStats(null, null)
  .WithCardType("Item")
   .AddPool("GeneralItemPool")
  .WithFlavour("A stone with Ink trapped inside.")
  .CanPlayOnBoard(true)
  .CanPlayOnFriendly(false)
  .CanPlayOnHand(false)
  .CanPlayOnEnemy(false)
  .WithValue(53)
  .WithText($"<keyword={Extensions.PrefixGUID("unplayable", this)}>")
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)

  {
      data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
           SStack("Weaken All",2),
           SStack("Weaken All Allies",1),
      };
  })

     );

        cards.Add(
        new CardDataBuilder(this).CreateItem("ChimS", "Fungus Infested Chime")
  .SetSprites("ChimShroom.png", "ChimShroom BG.png")
  .SetStats(null, null)
  .WithCardType("Item")
   .AddPool("GeneralItemPool")
  .WithFlavour("A stone with Ink trapped inside.")
  .CanPlayOnBoard(true)
  .CanPlayOnFriendly(false)
  .CanPlayOnHand(false)
  .CanPlayOnEnemy(false)
  .WithValue(50)
  .WithText($"<keyword={Extensions.PrefixGUID("unplayable", this)}>")
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)

  {
      data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {

           SStack("Shroom All",3),
           SStack("Shroom All Allies",1)
      };
  })

     );

        cards.Add(
        new CardDataBuilder(this).CreateItem("CTB", "Cherry Trigger Bomb")
  .SetSprites("Stringbomb.png", "Stringbomb BG.png")
  .SetStats(null, 3)
  .WithCardType("Item")
  .WithFlavour("FSSSSSSSSSS.")
  .SetTraits(TStack("Draw", 1),TStack("Zoomlin",1))
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)


  {
      data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
           SStack("Trigger Straw Cherry",1),
           SStack("Random Bom",3),
           SStack("Uses",2)
      };
  })

     );


        //YRA CLUNKS----------------------------------------------------------------------------------------------------------------------------------------

        cards.Add(
     new CardDataBuilder(this).CreateUnit("YraB1", "Model Goldfish")
     .SetSprites("B1.png", "B1 BG.png")
     .SetStats(null, null, 3)
     .WithCardType("Clunker")
     .WithFlavour("")
     .SubscribeToAfterAllBuildEvent(delegate (CardData data)

     {
         data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
         {

           SStack("Scrap",1),
           SStack("Give all Allies in the row HP",3),
           SStack("Give all Allies in the row ATK",1)

         };
     })
          );
        cards.Add(
     new CardDataBuilder(this).CreateUnit("YraB2", "Model Shark")
     .SetSprites("B2.png", "B2 BG.png")
     .SetStats(null, 2, 3)
     .WithCardType("Clunker")
     .WithFlavour("")
     .SetStartWithEffect(SStack("On Card Played Apply Attack To Self", 2), SStack("Scrap", 1))



     );
        cards.Add(
new CardDataBuilder(this).CreateUnit("YraB3", "Model Turtle")
.SetSprites("B3.png", "B3 BG.png")
.SetStats(null, 4, 7)
.WithCardType("Clunker")
.WithFlavour("")
.SetTraits(TStack("Frontline", 1))
.SetStartWithEffect(SStack("When Hit Apply Shell To AllyBehind", 3), SStack("Scrap", 2), SStack("Shell", 3))

);


        cards.Add(
     new CardDataBuilder(this).CreateUnit("AYraB1", "Model Goldfish")
     .SetSprites("B1.png", "B1 BG.png")
     .SetStats(6, null, 3)
     .WithCardType("Summoned")
     .WithFlavour("")
     .SubscribeToAfterAllBuildEvent(delegate (CardData data)

     {
         data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
         {


           SStack("Give all Allies in the row HP",3),
           SStack("Give all Allies in the row ATK",1)

         };
     })
          );
        cards.Add(
     new CardDataBuilder(this).CreateUnit("AYraB2", "Model Shark")
     .SetSprites("B2.png", "B2 BG.png")
     .SetStats(4, 3, 3)
     .WithCardType("Summoned")
     .WithFlavour("")
     .SetStartWithEffect(SStack("On Card Played Apply Attack To Self", 3)



     )

     );
        cards.Add(
new CardDataBuilder(this).CreateUnit("AYraB3", "Model Turtle")
.SetSprites("B3.png", "B3 BG.png")
.SetStats(8, 4, 7)
.WithCardType("Summoned")
.WithFlavour("")
.SetTraits(TStack("Frontline", 1))
.SetStartWithEffect(SStack("When Hit Apply Shell To AllyBehind", 5))

);



        //STARTING INVENTORY----------------------------------------------------------------------------------------------------------------------------------------------
        //STARTING INVENTORY----------------------------------------------------------------------------------------------------------------------------------------------
        //STARTING INVENTORY----------------------------------------------------------------------------------------------------------------------------------------------
        //STARTING INVENTORY----------------------------------------------------------------------------------------------------------------------------------------------
        cards.Add(
          new CardDataBuilder(this).CreateItem("Bat", "Baseball Bat")
          .SetSprites("Bat.png", "Bat BG.png")
          .SetStats(null, 3)
          .WithCardType("Item")
          .WithFlavour("HIT EM!")
          );
        cards.Add(
         new CardDataBuilder(this).CreateItem("SHOTGUN", "SHOTGUN (to the face)")
         .SetSprites("Shotgun.png", "Shotgun Bg.png")
         .SetStats(null, 6)
         .WithCardType("Item")
         .WithFlavour("SHOOT EM HARDER!")
         .SetTraits(TStack("Consume", 1))
          );
        cards.Add(
        new CardDataBuilder(this).CreateItem("KFez", "Kek Fez")
        .SetSprites("Fez.png", "Fez BG.png")
        .SetStats(null, null)
        .WithCardType("Item")
        .WithFlavour("HIT EM!")
        .SetTraits(TStack("Consume", 1))
        .SetAttackEffect(SStack("Block", 2), SStack("Reduce Counter", 2))
         );

        cards.Add(
       new CardDataBuilder(this).CreateUnit("Punchy", "Damaged Sentient Punching Bag")
       .SetSprites("lol.png", "lol BG.png")
       .SetStats(null, 1)
       .WithCardType("Clunker")
       .WithFlavour("HIT EM!")
       .WithText("<keyword=goobers.damaged>")
       .SetTraits(TStack("Smackback", 1))
       .SubscribeToAfterAllBuildEvent(delegate (CardData data)

       {
           data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
           {

           SStack("Scrap",2),
           };
       })
          );
        cards.Add(
  new CardDataBuilder(this).CreateUnit("Punchyfixed", "Sentient Punching Bag")
  .SetSprites("Fixedpuncy.png", "Fixedpuncy BG.png")
  .SetStats(null, 3)
  .WithCardType("Clunker")
  .WithFlavour("HIT EM!")
  .SetTraits(TStack("Smackback", 1))
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)

  {
      data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {

           SStack("Scrap",5),
           SStack("MultiHit",1),
      };
  })
     );
        cards.Add(
      new CardDataBuilder(this).CreateItem("Fixer", "Sewing Needle")
      .SetSprites("fixer.png", "fixer BG.png")
      .SetStats(null, null)
      .WithCardType("Item")
      .CanPlayOnBoard(false)
      .CanPlayOnHand(false)
      .CanPlayOnEnemy(false)
      .WithValue(23)
      .CanPlayOnFriendly(false)
      .WithFlavour("Used to fixed damaged stuff.")

       );

        cards.Add(
     new CardDataBuilder(this).CreateItem("Popmint", "Greasypop: Frosty Mint")
     .SetSprites("Popmint.png", "Popmint BG.png")
     .SetStats(null, 0)
     .WithCardType("Item")
     .WithFlavour("HIT EM!")
        .WithValue(54)
     .SetAttackEffect(SStack("Snow", 2),
                   SStack("Shell", 1))
        );
        cards.Add(
     new CardDataBuilder(this).CreateItem("FPopmint", "Fresh Greasypop: Frosty Mint")
     .SetSprites("Popmint.png", "Fresh.png")
     .SetStats(null, 0)
     .WithCardType("Item")
     .WithFlavour("HIT EM!")
     .SetTraits(TStack("Consume", 1))
     .SetAttackEffect(SStack("Snow", 5),
                   SStack("Heal", 1))
        );
        cards.Add(
new CardDataBuilder(this).CreateItem("PopSpice", "Greasypop: Chilly Willy")
.SetSprites("Popspice.png", "Popspice BG.png")
.SetStats(null, null)
.WithCardType("Item")
.WithFlavour("HIT EM!")
.CanPlayOnHand(true)
.WithValue(54)
.SetAttackEffect(SStack("Spice", 3))

   );
        cards.Add(
new CardDataBuilder(this).CreateItem("FPopSpice", "Fresh Greasypop: Chilly Willy")
.SetSprites("Popspice.png", "Fresh.png")
.SetStats(null, null)
.WithCardType("Item")
.WithFlavour("HIT EM!")
.CanPlayOnHand(true)
.SetTraits(TStack("Consume", 1))
.SetAttackEffect(SStack("Spice", 6),
                 SStack("Increase Attack", 2))
  );
        cards.Add(
        new CardDataBuilder(this).CreateItem("PopBerry", "Greasypop: Strawberry Deluxe")
.SetSprites("Popberry.png", "Popberry BG.png")
.SetStats(null, null)
.WithCardType("Item")
.WithFlavour("HIT EM!")
.WithValue(60)
.AddPool("GeneralItemPool")
.SetAttackEffect(SStack("Heal", 3), SStack("Increase Max Health", 3))
   );

        cards.Add(
       new CardDataBuilder(this).CreateItem("SBerry", "Greasypop: Sugar Shine")
.SetSprites("PopSHINE.png", "PopSHINE BG.png")
.SetStats(null, null)
.WithCardType("Item")
.CanPlayOnBoard(true)
.WithFlavour("HIT EM!")
.WithValue(54)
.SetAttackEffect(SStack("Reduce Counter", 2),
SStack("Shell", 1))
  );

        cards.Add(
       new CardDataBuilder(this).CreateItem("FSBerry", "Fresh Greasypop: Sugar Shine")
.SetSprites("PopSHINE.png", "Fresh.png")
.SetStats(null, null)
.WithCardType("Item")
.CanPlayOnBoard(true)
.WithFlavour("HIT EM!")
.SetAttackEffect(SStack("Reduce Max Counter", 3),
SStack("Spice", 5))
  );
        cards.Add(
       new CardDataBuilder(this).CreateItem("FPopBerry", "Fresh Greasypop: Strawberry Deluxe")
       .SetSprites("Popberry.png", "Fresh.png")
       .SetStats(null, null)
       .WithCardType("Item")
       .WithFlavour("HIT EM!")
       .SetTraits(TStack("Consume", 1))
       .SetAttackEffect(SStack("Heal", 6),
                 SStack("Increase Max Health", 8))
  );

        cards.Add(
       new CardDataBuilder(this).CreateUnit("VendingMachine", "Vending Machine")
       .SetSprites("Vend.png", "Vend BG.png")
       .SetStats(null, null)
       .WithCardType("Clunker")
       .WithFlavour("HIT EM!")
       .AddPool("GeneralItemPool")
       .WithValue(70)
       .SubscribeToAfterAllBuildEvent(delegate (CardData data)

       {
           data.startWithEffects = new CardData.StatusEffectStacks[]
           {

       SStack("Scrap",2),
       SStack("Random Soda on Hit2",1),
       SStack("Vending Machine go Brrr",1)
           };
       })
          );

        //SPECIAL CARDS!

        cards.Add(
       new CardDataBuilder(this).CreateUnit("SunrayIpod", "Sunray's Music Player")
       .SetSprites("Sipod.png", "Sipod BG.png")
       .SetStats(null, null)
       .WithCardType("Clunker")
       .WithFlavour("HIT EM!")
       .AddPool("GeneralItemPool")
       .WithValue(70)
       .SubscribeToAfterAllBuildEvent(delegate (CardData data)

       {
           data.startWithEffects = new CardData.StatusEffectStacks[]
           {

       SStack("Woo woo",1),
       SStack("Scrap",1)
     
           };
       })
          );

        cards.Add(
    new CardDataBuilder(this).CreateUnit("SunburnIpod", "Sunburn's Music Player")
    .SetSprites("Burnpod.png", "Burnpod BG.png")
    .SetStats(null, null)
    .WithCardType("Clunker")
    .WithFlavour("HIT EM!")
    .AddPool("GeneralItemPool")
    .WithValue(70)
    .SubscribeToAfterAllBuildEvent(delegate (CardData data)

    {
        data.startWithEffects = new CardData.StatusEffectStacks[]
        {

       SStack("Woo Hoo",2),
       SStack("Scrap",1)

        };
    })
       );

        cards.Add(
   new CardDataBuilder(this).CreateUnit("SunscreenIpod", "Sunscreen's Music Player")
   .SetSprites("Screenpod.png", "Screenpod BG.png")
   .SetStats(null, null)
   .WithCardType("Clunker")
   .WithFlavour("HIT EM!")
   .AddPool("GeneralItemPool")
   .WithValue(70)
   .SubscribeToAfterAllBuildEvent(delegate (CardData data)

   {
       data.startWithEffects = new CardData.StatusEffectStacks[]
       {

       SStack("Boo Hoo",2),
       SStack("Scrap",1)

       };
   })
      );

        cards.Add(
new CardDataBuilder(this).CreateItem("GrilledBeef", "A Job Well Done!")
.SetSprites("BEEF.png", "BEEF BG.png")
.SetStats(null, null)
.WithCardType("Item")
.WithFlavour("HIT EM!")
.AddPool("GeneralItemPool")
.WithValue(100)
.SetTraits(TStack("Barrage",1), TStack("Consume",1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
            data.startWithEffects = new CardData.StatusEffectStacks[]
                  {

     SStack("Remove Self after use", 1)

                  };
            data.attackEffects = new CardData.StatusEffectStacks[]
{

       SStack("Reduce Max Counter",2)

};

})
   );

        cards.Add(
   new CardDataBuilder(this).CreateItem("Hateu Drill", "Hateu's Drill Hammer")
   .SetSprites("DRILL.png", "DRILL BG.png")
   .SetStats(null, 1)
   .WithCardType("Item")
   .WithFlavour("*Obliteration noises*")
   .AddPool("GeneralItemPool")
   .WithValue(70)
   .SetTraits(TStack("Consume", 1))
   .SubscribeToAfterAllBuildEvent(delegate (CardData data)

   {
       data.startWithEffects = new CardData.StatusEffectStacks[]
       {

       SStack("MultiHit",8)

       };
   })
      );

        cards.Add(
new CardDataBuilder(this).CreateItem("HateuYell", "Harsh Attack Orders")
.SetSprites("YELL.png", "YELL BG.png")
.SetStats(null, 3)
.WithCardType("Item")
.WithFlavour("HIT EM!")
.AddPool("GeneralItemPool")
.WithValue(70)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
 data.attackEffects = new CardData.StatusEffectStacks[]
 {

       SStack("Frost",100),  SStack("Reduce Counter",100), SStack("Null",1)

 };

 
})
);


       
        //-------------------------------------------------------------ASHI TRIBE--------------------------------------------------------------------------
        //-------------------------------------------------------------ASHI TRIBE--------------------------------------------------------------------------
        //-------------------------------------------------------------ASHI TRIBE--------------------------------------------------------------------------
        //-------------------------------------------------------------ASHI TRIBE--------------------------------------------------------------------------
        //-------------------------------------------------------------ASHI TRIBE--------------------------------------------------------------------------
        //-------------------------------------------------------------ASHI TRIBE--------------------------------------------------------------------------
        //-------------------------------------------------------------ASHI TRIBE--------------------------------------------------------------------------
        //-------------------------------------------------------------ASHI TRIBE--------------------------------------------------------------------------



        cards.Add(
new CardDataBuilder(this).CreateUnit("Ashi Leader", "Ashi Shi")
.SetSprites("Ashi.png", "Ashi BG.png")
.SetStats(8, 4, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Leader")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("You dont like my tags? Alright lemme get the punching gloves!")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                     {
SStack("FullImmuneToInk",1)
           };
  data.traits = new List<CardData.TraitStacks>

 {CreateTraitStack("TagR", 1)};

    data.createScripts = new CardScript[]
  {
                   GiveUpgrade(),
  };

})
);
        //INVENTORY START----------------------------------
        //INVENTORY START----------------------------------
        //INVENTORY START----------------------------------
        //INVENTORY START----------------------------------

        cards.Add(
new CardDataBuilder(this).CreateItem("ConnectionTag", "Tag of Connections")
.SetSprites("CONNECTIONTAG.png", "CONNECTIONTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)       //New lines (replaces flavor text)
{
            data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                            {
SStack("CON",1) ,

  };
   data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Noomlin", 1)};


})
);
        cards.Add(
new CardDataBuilder(this).CreateItem("SnowTag", "Tag of Snow")
.SetSprites("SNOWTAG.png", "SNOWTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                             {
SStack("SnowTag",1),
SStack("Snow",1)

};
 data.traits = new List<CardData.TraitStacks>




{ CreateTraitStack("Consume", 1),CreateTraitStack("Draw", 1)};


})
);

        cards.Add(
new CardDataBuilder(this).CreateItem("HealingTag", "Tag of Restoration")
.SetSprites("RESTAG.png", "RESTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item")
    .CanPlayOnHand(true)
      .CanPlayOnBoard(true).WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                {
SStack("HealTag",1),SStack("Heal",4)

};
data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1), CreateTraitStack("Draw", 1)};


})
);


        cards.Add(
new CardDataBuilder(this).CreateItem("GoblingTag", "Tag of Goblings")
.SetSprites("GOBTAG.png", "GOBTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                    {
SStack("GoblingTag",1)

};
data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Noomlin", 1),CreateTraitStack("Draw", 1)};


})
);
        cards.Add(
new CardDataBuilder(this).CreateItem("Sunshine Bottle", "Sunshine in a Bottle")
.SetSprites("SUNJAR.png", "SUNJAR BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                    {
SStack("Reduce Counter",2)

};
data.traits = new List<CardData.TraitStacks>

{};

})
);

        cards.Add(
new CardDataBuilder(this).CreateItem("Snowy Spell", "Snowy Spell")
.SetSprites("SNOWSPELL.png", "SNOWSPELL BG.png")
.SetStats(null, 2)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                        {
SStack("Snow",2)

};
 data.traits = new List<CardData.TraitStacks>

 {};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Alarm", "Alarm Bell")
.SetSprites("ALARM BELL.png", "ALARM BELL BG.png")
.SetStats(null,null, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Clunker")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
data.startWithEffects= new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
  {
       SStack("Hit help all",1), SStack("Scrap",3)

};
data.traits = new List<CardData.TraitStacks>

{};

})
);

        cards.Add(
new CardDataBuilder(this).CreateItem("Mimic Blade", "Goofy Blade(*Clown horn noises)")
.SetSprites("GOOFY.png", "GOOFY BG.png")
.SetStats(null, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                               {
SStack("Goofy Knife",1)

};
data.traits = new List<CardData.TraitStacks>

{ };

})
);

        cards.Add(
new CardDataBuilder(this).CreateItem("Whackohammer", "Giant azz Hammer")
.SetSprites("BIGHAM.png", "BIGHAM BG.png")
.SetStats(null, 10)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
   data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                   {


};
   data.traits = new List<CardData.TraitStacks>

   { CreateTraitStack("Consume", 1)};

})
);

   

       
        //SPECIAL TAGS---------------------------------------
        //SPECIAL TAGS---------------------------------------
        //SPECIAL TAGS---------------------------------------
        //SPECIAL TAGS---------------------------------------
        //SPECIAL TAGS---------------------------------------
        //SPECIAL TAGS---------------------------------------
        //SPECIAL TAGS---------------------------------------
        //SPECIAL TAGS---------------------------------------

        cards.Add(
new CardDataBuilder(this).CreateUnit("Tagbox", "Tagbox")
.SetSprites("Tagbox.png", "Tagbox BG.png")
.SetStats(100,null, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Leader")
.WithText("<keyword=goobers.tag>")

.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{


    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                               {
SStack("Remove Self on deploy",1),SStack("Tagbox choose",1)

};
    data.createScripts = new CardScript[]
 {
                   GiveUpgrade(),
 };


})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("FortuneTagselect", "Tag of Fortune")
.SetSprites("FORTUNETAGA.png", "FORTUNETAGA BG.png")
.SetStats(1, null, 0)                                                    //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned")
.WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                       {
 SStack("kms",1) ,SStack("Gain FortuneTag",1), SStack("To Hand FortuneTag",1)

    };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("WinterTagselect", "Tag of Winters")
.SetSprites("WINTERTAG.png", "WINTERTAG BG.png")
.SetStats(1, null, 0)                                                    //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                    {
 SStack("kms",1),SStack("Gain WinterTag",1), SStack("To Hand WinterTag",1)

 };


})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("DemonTagselect", "Tag of Demons")
.SetSprites("DTAG.png", "DTAG BG.png")
.SetStats(1, null, 0)                                                      //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                    {
 SStack("kms",1),SStack("Gain DemonTag",1), SStack("To Hand DemonTag",1)

 };
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("LuminTagselect", "Tag of Lumins")
.SetSprites("LTAG.png", "LTAG BG.png")
.SetStats(1, null, 0)                                                     //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned")
.CanPlayOnHand(true)
.CanPlayOnBoard(true).WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                    {
 SStack("kms",1),SStack("Gain LuminTag",1), SStack("To Hand LuminTag",1)

 };
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("NovaTagselect", "Tag of Supernovas")
.SetSprites("NTAG.png", "NTAG BG.png")
.SetStats(1, null, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned")
.CanPlayOnHand(true)
.CanPlayOnBoard(true).WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                    {
 SStack("kms",1),SStack("Gain NovaTag",1), SStack("To Hand NovaTag",1)

 };


})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("DetonatorTagselect", "Tag of Detonation")
.SetSprites("DETTAG.png", "DETTAG BG.png")
.SetStats(1, null, 0)                                                    //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                    {
 SStack("kms",1),SStack("Gain DetonatorTag",1), SStack("To Hand DetonatorTag",1)

 };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("TeethTagselect", "Tag of Teeth")
.SetSprites("TEETHTAG.png", "TEETHTAG BG.png")
.SetStats(1, null, 0)                                                     //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                    {
 SStack("kms",1),SStack("Gain TeethTag",1), SStack("To Hand TeethTag",1)

 };


})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("SunTagselect", "Tag of Shine")
.SetSprites("STAG.png", "STAG BG.png")
.SetStats(1, null, 0)                                                    //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned")
.CanPlayOnHand(true).WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                    {
 SStack("kms",1),SStack("Gain SunTag",1), SStack("To Hand SunTag",1)

 };
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("SplitTagselect", "Tag of Mitosis")
.SetSprites("MITAG.png", "MITAG BG.png")
.SetStats(1, null, 0)                                                    //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned")
.CanPlayOnHand(true).WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                               {
 SStack("kms",1),SStack("Gain SplitTag",1), SStack("To Hand SplitTag",1)

};
})
);



        cards.Add(
new CardDataBuilder(this).CreateUnit("Restagselect", "Tag of Resurrection")
.SetSprites("REVTAG.png", "REVTAG BG.png")
.SetStats(1, null, 0)                                                    //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned")
.WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                               {
 SStack("kms",1),SStack("Gain Restag",1),SStack("To Hand Restag",1)

};


})
);
        //ACTUAL TAGS--------------------------------------------------------------------------------
        //ACTUAL TAGS--------------------------------------------------------------------------------

        //ACTUAL TAGS--------------------------------------------------------------------------------
        //ACTUAL TAGS-------------------------------------------------------------------------------- //ACTUAL TAGS--------------------------------------------------------------------------------
        //ACTUAL TAGS--------------------------------------------------------------------------------
        //ACTUAL TAGS--------------------------------------------------------------------------------
        //ACTUAL TAGS--------------------------------------------------------------------------------
        //ACTUAL TAGS--------------------------------------------------------------------------------
        //ACTUAL TAGS--------------------------------------------------------------------------------

        cards.Add(
new CardDataBuilder(this).CreateItem("Restag", "Tag of Resurrection")
.SetSprites("REVTAG.png", "REVTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
   data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                              {
SStack("Restag",1)

};
   data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Draw", 1)};


})
);



        cards.Add(
new CardDataBuilder(this).CreateItem("SplitTag", "Tag of Mitosis")
.SetSprites("MITAG.png", "MITAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{

    data.targetConstraints = new[]
 {

                        new TargetConstraintHasTrait()
                        { not = true , trait = Get<TraitData>("Backline")},

                          new TargetConstraintHasTrait()
                        { not = true , trait = Get<TraitData>("Frontline")}
    };

    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                            {
SStack("Splittag",1)

 };
  data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Draw", 1)};


})
);


        cards.Add(
new CardDataBuilder(this).CreateItem("FortuneTag", "Tag of Fortune")
.SetSprites("FORTUNETAGA.png", "FORTUNETAGA BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                         {
SStack("FortuneTag",1)

   };
 data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Draw", 1)};


})
);


        cards.Add(
new CardDataBuilder(this).CreateItem("WinterTag", "Tag of Winters")
.SetSprites("WINTERTAG.png", "WINTERTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                            {
SStack("WinterTag",1), SStack("Snow",1)

};
 data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Draw", 1)};


})
);

        cards.Add(
new CardDataBuilder(this).CreateItem("DemonTag", "Tag of Demons")
.SetSprites("DTAG.png", "DTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                               {
SStack("DemonTag",1), SStack("Demonize",1)

};
data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Draw", 1),CreateTraitStack("Noomlin", 1)};


})
);

        cards.Add(
new CardDataBuilder(this).CreateItem("LuminTag", "Tag of Lumins")
.SetSprites("LTAG.png", "LTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                   {
SStack("LuminTag",1)

};
data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Noomlin", 1),CreateTraitStack("Draw", 1)};


})
);

        cards.Add(
new CardDataBuilder(this).CreateItem("NovaTag", "Tag of Supernovas")
.SetSprites("NTAG.png", "NTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
  data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                       {
SStack("NovaTag",1)

};
  data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Draw", 1)};


})
);

        cards.Add(
new CardDataBuilder(this).CreateItem("DetonatorTag", "Tag of Detonation")
.SetSprites("DETTAG.png", "DETTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item")
.WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                         {
SStack("DetoTag",1)

};
data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Draw", 1)};


})
);


        cards.Add(
new CardDataBuilder(this).CreateItem("TeethTag", "Tag of Teeth")
.SetSprites("TEETHTAG.png", "TEETHTAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
  data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                         {
SStack("TeethTag",1)

};
  data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1)};


})
);

        cards.Add(
new CardDataBuilder(this).CreateItem("SunTag", "Tag of Shine")
.SetSprites("STAG.png", "STAG BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item").WithText("<keyword=goobers.tag>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                           {
SStack("SunTag",1)

};
 data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Draw", 1)};


})
);








        //ENEMIES

        //Slimey slime slimes TIER 1


        cards.Add(
new CardDataBuilder(this).CreateUnit("Slumo", "Slumoo")
.SetSprites("Slumo.png", "Slumo BG.png")
.SetStats(4, 1, 3)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
   data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
     {
          SStack("When X Health Lost Split",1)

 };
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Caramoo", "Caramoo")
.SetSprites("Caramoo.png", "Caramoo BG.png")
.SetStats(6, 1, 6)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
          SStack("When Ally Is Killed Gain Their Attack",1)

};
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Mugoo", "Mugooloo")
.SetSprites("Mugoo.png", "Mugoo BG.png")
.SetStats(8, 3, 5)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
          SStack("When X Health Lost Split",4)

};
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Koogooloo", "Koogooloo")
.SetSprites("Kuugooloo.png", "Kuugooloo BG.png")
.SetStats(13, 5, 4)
.WithCardType("Miniboss")
.WithFlavour("")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
         {
             SStack("Killed reduce",1),
          SStack("When X Health Lost Split",8)
          

};
})
);





        //THE HITABEARS TIER 2


        cards.Add(
new CardDataBuilder(this).CreateUnit("Bo Bo", "Bo Bo")
.SetSprites("BOBO.png", "BOBO BG.png")
.SetStats(8, 2, 3)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
          SStack("When Hit Gain Attack To Self (No Ping)",3)

  };
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Do Bo", "Do Bo")
.SetSprites("ROBO.png", "ROBO BG.png")
.SetStats(10, 4, 5)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
   {
          SStack("Heal allies when hit",1)

};
})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Ro Ro", "Ro Ro")
.SetSprites("RORO.png", "RORO BG.png")
.SetStats(10, 1, 5)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
          SStack("Retaliate",4)

};
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Mo Mo Senior", "Mo Mo Senior")
.SetSprites("MOMOSEN.png", "MOMOSEN BG.png")
.SetStats(22, 2, 4)
.WithCardType("Miniboss")
.WithFlavour("")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
         SStack("When Health Lost Apply Equal Attack to Self and Allies",1),
         SStack("Momo not spooked",1),
         SStack("FullImmuneToInk",1)

};
})
);

        //MOMO BUT BETTER

        cards.Add(
new CardDataBuilder(this).CreateUnit("Mo MoA", "Alpha Mo Mo")
.SetSprites("MOMOA.png", "MOMOA BG.png")
.SetStats(8, 5, 5)
.WithCardType("Friendly")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
            {
         SStack("When Health Lost Apply Equal Attack to Self and Allies",1),
         SStack("Heal allies when hit",1)
};

    data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Heartburn", 1),CreateTraitStack("Barrage", 1)};
})
);

        //Rework! Corrupted 4 Masked Deity
        //Rework! Corrupted 4 Masked Deity

        //Rework! Corrupted 4 Masked Deity
        //Rework! Corrupted 4 Masked Deity
        //Rework! Corrupted 4 Masked Deity
        //Rework! Corrupted 4 Masked Deity
        //Rework! Corrupted 4 Masked Deity

        //Rework! Corrupted 4 Masked Deity


        cards.Add(
new CardDataBuilder(this).CreateUnit("Cursed", "Cursed Lamps")
.SetSprites("CLAMP.png", "CLAMP BG.png")
.SetStats(null, null, 0)
.WithCardType("Enemy")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
  {
     SStack("When Hit Reduce Attack To Attacker",2),
     SStack("Scrap",1)
  };
})
);
        cards.Add(
new CardDataBuilder(this).CreateUnit("Baby Horns", "Baby Horns")
.SetSprites("BBhor.png", "BBhorns.png")
.SetStats(3, 2, 3)
.WithCardType("Enemy")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
     SStack("When hit with item destroy card",1),
     SStack("When Hit Apply Demonize To Attacker",1)

    };
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Judge", "Judge")
.SetSprites("Judge.png", "Judge BG.png")
.SetStats(10, 3, 4)
.WithCardType("Enemy")
.WithValue(350)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
   {
 
     SStack("Snow",1)

       };
    data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Revenge", 1)};
})
  );

        cards.Add(
new CardDataBuilder(this).CreateUnit("Mourn", "Mourner")
.SetSprites("Mourn.png", "Mourn BG.png")
.SetStats(8, null, 0)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
     SStack("Increase all ally's when card destroyed",2)

    };
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Torture", "Toury")
.SetSprites("TORCH.png", "TORCH BG.png")
.SetStats(12, 1, 4)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(300)
.SetTraits(TStack("Backline", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
  {
     SStack("Overload",2)

   };

})
);
        cards.Add(
     new CardDataBuilder(this).CreateUnit("Dumples", "Dumples")
     .SetSprites("Dumples.png", "Dumples BG.png")
     .SetStats(8, 2, 3)
     .WithCardType("Enemy")
     .WithFlavour("")
     .WithValue(300)
     .SubscribeToAfterAllBuildEvent(delegate (CardData data)

     {
         data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
     SStack("Before Attack, Demonize Targets",1)

        };


     })
       );

        cards.Add(
  new CardDataBuilder(this).CreateUnit("Halo", "Halos")
  .SetSprites("HALOS.png", "HALOS BG.png")
  .SetStats(11, 1, 3)
  .WithCardType("Enemy")
  .WithFlavour("")
  .WithValue(300)
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)

  {
      data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
 {

     SStack("When Healed Apply Attack To Self",2)
     

     };


  })
    );

        cards.Add(
 new CardDataBuilder(this).CreateUnit("Inny", "Inny Inny")
 .SetSprites("INNY.png", "INNY BG.png")
 .SetStats(12, 3, 4)
 .WithCardType("Enemy")
 .WithFlavour("")
 .WithValue(300)
 .SubscribeToAfterAllBuildEvent(delegate (CardData data)

 {
     data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{

     SStack("Trigger self when healed",1)


    };


 })
   );
        cards.Add(
new CardDataBuilder(this).CreateUnit("Susu", "Nurser")
.SetSprites("NURSER.png", "NURSER BG.png")
.SetStats(10, 3, 5)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{

     SStack("Increase attack to allies when healed",2)


};


})
); 

        cards.Add(
new CardDataBuilder(this).CreateUnit("Dorm", "4 Masked Deity: Dormant")
.SetSprites("DORM.png", "DORM BG.png")
.SetStats(10, null, 7)
.WithCardType("Boss")
.WithValue(600)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
   data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
 {

     SStack("FullImmuneToSnow",1),
     SStack("FullImmuneToInk",1),
     SStack("Hit All Enemies",1),
     SStack("DortoExe",1)

     };
   data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
     SStack("Increase Max Counter",1)

  };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Backline", 1)};

})
 );

        cards.Add(
  new CardDataBuilder(this).CreateUnit("Exe", "Corrupted Deity: Executioner")
  .SetSprites("EXE.png", "EXE BG.png")
  .SetStats(19, 3, 3)
  .WithCardType("Boss")
  .WithFlavour("")
  .WithValue(600)
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)

  {
      data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {

     SStack("ImmuneToSnow",1),
     SStack("Exetocre",1),
     SStack("Judgetime",4)

        };
      data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
 {
     SStack("Demonize",1)

     };
      data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Backline", 1)};
  })
    );

        cards.Add(
new CardDataBuilder(this).CreateUnit("Cre", "Corrupted Deity: Cremator")
.SetSprites("CRE.png", "CRE BG.png")
.SetStats(17, 0, 5)
.WithCardType("Boss")
.WithFlavour("")
.WithValue(600)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{


     
     SStack("Cretopea",1),
     SStack("Cretime",4)

    };
  data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
     SStack("Overload",3)

 };

    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Backline", 1)};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Pea", "Corrupted Deity: Peace")
.SetSprites("PEA.png", "PEA BG.png")
.SetStats(18, 0, 2)
.WithCardType("Boss")
.WithFlavour("")
.WithValue(600)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
  {

         SStack("ImmuneToSnow",1),
     SStack("On Card Played Heal & Cleanse To Allies",3)
     ,SStack("Peacetime",4)

};
data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
  {


};

    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Backline", 1)};

})
);


        //CHESS BOSS BATTLE


        cards.Add(
    new CardDataBuilder(this).CreateUnit("Pawn", "Pawn")
    .SetSprites("Epawn.png", "Epawn BG.png")
    .SetStats(10, 0, 0)
    .WithCardType("Enemy")
    .WithFlavour("")
    .WithValue(400)
    .SubscribeToAfterAllBuildEvent(delegate (CardData data)

    {
        data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
   {
    

       };
        data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Cunning", 1),CreateTraitStack("Maid", 1)};

    })
      );

        cards.Add(
new CardDataBuilder(this).CreateUnit("The Knight", "The Knight")
.SetSprites("EKnight.png", "EKnight BG.png")
.SetStats(17, 4, 3)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(500)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
    SStack("ImmuneToSnow",1), SStack("SP TRIGGER",1)

};
data.traits = new List<CardData.TraitStacks>

{
    CreateTraitStack("Longshot", 1)
};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("The Bishop", "The Bishop")
.SetSprites("ERook.png", "ERook BG.png")
.SetStats(15, null, 3)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(20)
.WithValue(500)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
SStack("Block",2),SStack("Enemy Bishop",2),SStack("Enemy Bishop1",2)

};
data.traits = new List<CardData.TraitStacks>

{

};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("The Rook", "The Rook")
.SetSprites("Ebishop.png", "Ebishop BG.png")
.SetStats(20, 4, 4)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(600)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
SStack("ImmuneToSnow",1)

};
data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Barrage", 1), CreateTraitStack("Cravings",1)};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("The Queen", "The Queen")
.SetSprites("Queene.png", "Queene BG.png")
.SetStats(90, 8, 10)
.WithCardType("Boss")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
            {
  SStack("ImmuneToSnow",1),SStack("QueentoAmazon",1),SStack("Queen go grr",1)

};
 data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Maid", 1),CreateTraitStack("Backline", 1)};

    
})
);




        cards.Add(
new CardDataBuilder(this).CreateUnit("The Amazon", "The Amazon")
.SetSprites("AmazonE.png", "AmazonE BG.png")
.SetStats(35, 4, 3)
.WithCardType("Boss")
.WithFlavour("")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] 
               {

SStack("Block",6),SStack("ImmuneToSnow",1),SStack("Hit All Enemies",1),SStack("AmazontoKing",1)

};
data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Backline", 1),CreateTraitStack("Cravings",1),CreateTraitStack("Maid", 1)};


})
);

        cards.Add(
   new CardDataBuilder(this).CreateUnit("The King", "The King")
   .SetSprites("Kinge.png", "Kinge BG.png")
   .SetStats(1, null, 0)
   .WithCardType("Boss")
   .WithValue(800)
   .WithFlavour("Does absolutely nothing")
   .SubscribeToAfterAllBuildEvent(delegate (CardData data)

   {
       data.startWithEffects = new CardData.StatusEffectStacks[] 
                   {
SStack("When Deplpyed pawns",4)

   };
       data.traits = new List<CardData.TraitStacks>

   {};

       
   })
   );

        //FEAR DEITY FIGHT-------------------------------------------------------------
        //FEAR DEITY FIGHT-------------------------------------------------------------
        //FEAR DEITY FIGHT-------------------------------------------------------------
        //FEAR DEITY FIGHT-------------------------------------------------------------
        //FEAR DEITY FIGHT-------------------------------------------------------------
        //FEAR DEITY FIGHT-------------------------------------------------------------

        cards.Add(
    new CardDataBuilder(this).CreateUnit("Fear Dog", "Feahound")
    .SetSprites("DOG.png", "Spider BG.png")
    .SetStats(18, 5, 6)
    .WithCardType("Enemy")
    .WithFlavour("Grrrr raf raf")
    .WithValue(400)
    .SubscribeToAfterAllBuildEvent(delegate (CardData data)

    {

        data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                          {


};
        data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
   {

       SStack("When Hit Reduce Counter To Self",2)
       };



        data.traits = new List<CardData.TraitStacks>

   {
        
        };

    })
      );


        cards.Add(
  new CardDataBuilder(this).CreateUnit("Fear Fish", "Feash")
  .SetSprites("Fish.png", "Fish BG.png")
  .SetStats(19, 4, 3)
  .WithCardType("Enemy")
  .WithFlavour("")
  .WithValue(400)
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)

  {

      data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                        {
SStack("FEAR",1)

};
      data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
 {


     };
      data.traits = new List<CardData.TraitStacks>

      {
          CreateTraitStack("Knockback",1)
      };

  })
    );
        cards.Add(
new CardDataBuilder(this).CreateUnit("Fear Snake", "Madosha")
.SetSprites("Snake.png", "Spider BG.png")
.SetStats(15, 1, 3)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                      {
SStack("FEAR",1)

};
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
    SStack("MultiHit",2)
  };
   data.traits = new List<CardData.TraitStacks>

   {
       CreateTraitStack("Aimless", 1)
   };

})
 );

        cards.Add(
new CardDataBuilder(this).CreateUnit("Fear Spider", "Araco")
.SetSprites("Spider.png", "Spider BG.png")
.SetStats(22, 8, 5)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
 {

     SStack("Fear on hit",1),SStack("Fear on hit2",1),SStack("ImmuneToSnow",1),SStack("Block",1)
};
data.traits = new List<CardData.TraitStacks>

{
CreateTraitStack("Frontline", 1)
};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Fear Sprout", "Drain Flower")
.SetSprites("Plant.png", "Spider BG.png")
.SetStats(3, null, 0)
.WithCardType("Enemy")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
     {

         SStack("Block",15),SStack("Fear While active",1),SStack("Fear While active2",1),SStack("Fear transferer",1),
         SStack("FullImmuneToInk",1)
};
data.traits = new List<CardData.TraitStacks>

{
    CreateTraitStack("Backline", 1)
};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Fear Deity", "Deity of Fear")
.SetSprites("Deity.png", "Deity BG.png")
.SetStats(65, 6, 3)
.WithCardType("Miniboss")
.WithFlavour("")
.WithValue(600)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                         {
SStack("FEAR",2)

};
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
         {
             SStack("Inflict when deployed",4),SStack("ImmuneToSnow",1),SStack("Inflict dam when deployed",2)

};
data.traits = new List<CardData.TraitStacks>

{
    CreateTraitStack("Barrage", 1)
};

})
);
        //FIGHT 4 - HATEU MINERS----------------------------------------------------------------------------------------
        //FIGHT 4 - HATEU MINERS----------------------------------------------------------------------------------------
        //FIGHT 4 - HATEU MINERS----------------------------------------------------------------------------------------
        //FIGHT 4 - HATEU MINERS----------------------------------------------------------------------------------------
        //FIGHT 4 - HATEU MINERS----------------------------------------------------------------------------------------
        //FIGHT 4 - HATEU MINERS----------------------------------------------------------------------------------------




        cards.Add(
new CardDataBuilder(this).CreateUnit("Mini Miner", "Minor Miney")
.SetSprites("MINIMINE.png", "MINIMINE BG.png")
.SetStats(12, 2, 4)
.WithCardType("Enemy")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
         {

         SStack("Scrap",2),SStack("Scrap frenzy",1)
};
data.traits = new List<CardData.TraitStacks>

{
      
};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Helmet Guy", "Helmey")
.SetSprites("HELMETGUY.png", "HELMETGUY BG.png")
.SetStats(8, 5, 6)
.WithCardType("Enemy")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
             {

         SStack("Scrap",3),SStack("Scrap All",1)
};
  data.traits = new List<CardData.TraitStacks>

  {
       CreateTraitStack("Backline", 1)
  };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Davey", "Angry Davey")
.SetSprites("DAVE.png", "DAVE BG.png")
.SetStats(12, 5, 0)
.WithCardType("Enemy")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
   data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {

        SStack("Scrap hitter",1),SStack("Scrap",1)
};
   data.traits = new List<CardData.TraitStacks>

   {

   };

})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Fros", "Fross")
.SetSprites("FROS.png", "FROS BG.png")
.SetStats(5, 10, 7)
.WithCardType("Enemy")
.WithValue(400)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
   data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {

        SStack("Scrap Frost",2), SStack("Scrap",2)
};
   data.traits = new List<CardData.TraitStacks>

   {

   };

})
);



        cards.Add(
new CardDataBuilder(this).CreateUnit("Greg", "Greg")
.SetSprites("GREG.png", "GREG BG.png")
.SetStats(44, 8, 9)
.WithCardType("Miniboss")
.WithFlavour("")
.WithValue(600)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                    {


};
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
            SStack("ImmuneToSnow",1),SStack("Multi whenhit",2), SStack("Scrap",3)

};
data.traits = new List<CardData.TraitStacks>

{
   
};

})
);


        //Scrapyard Fight 1
        //Scrapyard Fight 1
        //Scrapyard Fight 1
        //Scrapyard Fight 1

        cards.Add(
new CardDataBuilder(this).CreateUnit("Jacky", "Jacky")
.SetSprites("JACKY.png", "JACKY BG.png")
.SetStats(null, 1, 2)
.WithCardType("Enemy")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                      {

     SStack("Scrap",1)
};
data.traits = new List<CardData.TraitStacks>

{
};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Pricko", "Splinter")
.SetSprites("SPLINTER.png", "SPLINTER BG.png")
.SetStats(4, 2, 3)
.WithCardType("Enemy")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                   {

     SStack("Scrap",1),SStack("When Hit Gain Teeth To Self",1)
};
 data.traits = new List<CardData.TraitStacks>

{
};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Broken Spike", "Damaged Spike Wall")
.SetSprites("DAMWALL.png", "DAMWALL BG.png")
.SetStats(null, 3, 0)
.WithCardType("Enemy")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                 {

     SStack("Scrap",2)
};
  data.traits = new List<CardData.TraitStacks>

  {
      CreateTraitStack("Smackback", 1),  CreateTraitStack("Frontline", 1)
  };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Hippo Scra", "Scrap Heap-po")
.SetSprites("HIPPO.png", "HIPPO BG.png")
.SetStats(8, 3, 4)
.WithCardType("Miniboss")
.WithValue(600)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                        {


};
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
           SStack("Bonus Damage Equal To Scrap",1), SStack("Scrap gain when ally",1), SStack("Scrap",3)

};
 data.traits = new List<CardData.TraitStacks>

 {

 };

})
);

        // Goop Bugs! FIGHT 7---------------------------------------------------------------------------------------------------------------------------------------------------

        cards.Add(
new CardDataBuilder(this).CreateUnit("Applier", "Infester")
.SetSprites("INFESTER.png", "INFESTER BG.png")
.SetStats(19, 3, 4)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

  data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                      {


  };
  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
    SStack("Apply friendly Explode",3)
  };
  data.traits = new List<CardData.TraitStacks>

  {
         CreateTraitStack("Barrage", 1)
  };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Grr Bug", "Grr grr bug")
.SetSprites("GRR BUG.png", "GRR BUG BG.png")
.SetStats(20, 1, 2)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                        {


};
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
    SStack("MultiHit",2),
    SStack("Nom nom yummy",1)
};
 data.traits = new List<CardData.TraitStacks>

{
        
};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Nest", "GoopNest")
.SetSprites("NEST.png", "NEST BG.png")
.SetStats(30, null, 0)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                        {


};
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
    SStack("While Active Fexplode",1)
};
 data.traits = new List<CardData.TraitStacks>

{
         CreateTraitStack("Backline", 1)
};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Bomba Mother", "Bomba Mother")
.SetSprites("MOMBOM.png", "MOMBOM BG.png")
.SetStats(30, 20, 9)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(300)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                        {


};
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
    SStack("Summon Bomba when hit",1)
};
data.traits = new List<CardData.TraitStacks>

{
         
};

})
);



        cards.Add(
new CardDataBuilder(this).CreateUnit("Bomba Bug", "Baby Bomba Goopfly")
.SetSprites("EXPLOBUG.png", "EXPLOBUG BG.png")
.SetStats(12, 1, 3)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                        {


    };
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
   
            SStack("Gain Explode",4)
    };
    data.traits = new List<CardData.TraitStacks>

    {
    
    };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Grand Bug", "Grand Infested")
.SetSprites("GRANDBUG.png", "GRANDBUG BG.png")
.SetStats(500000, 2, 4)
.WithCardType("Miniboss")
.WithValue(600)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                    {


};
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {

            SStack("ImmuneToSnow",1), SStack("When ally halve self",1),SStack("Hitgoop",3),SStack("Hit All Enemies",1),SStack("FullImmuneToInk",1)
};
data.traits = new List<CardData.TraitStacks>

{

};

})
);

        cards.Add(
       new CardDataBuilder(this).CreateUnit("Angry GoopFlies", "Angry Goopflies")
       .SetSprites("GoopFlies.png", "GOOP BG.png")
       .SetStats(19, 1, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
       .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
       .WithFlavour("BZZZZZZZZZZZZZZZZZZZZZ")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)

      {

          data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                              {
SStack("Shroom", 2)

          };
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
              {
         SStack("MultiHit", 2)

          };
          data.traits = new List<CardData.TraitStacks>

 {
      CreateTraitStack("Friendplode", 2)
 };

      })
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Rager Fly", "Burner Goopfly")
.SetSprites("BURNER.png", "BURNER BG.png")
.SetStats(28, 1, 3)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                     {


 };
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
     {
         SStack("When Hit Apply Spice To Self",3), SStack("MultiHit",2)
           
 };
 data.traits = new List<CardData.TraitStacks>

 {
      CreateTraitStack("Friendplode", 2)
 };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Heavy Goopfly", "Heavy Goopfly")
.SetSprites("HEAVY.png", "HEAVY BG.png")
.SetStats(22, 7, 5)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                     {


 };
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
     {

             SStack("Heavy Explsion Plus",2)
 };
 data.traits = new List<CardData.TraitStacks>

 {
      CreateTraitStack("Friendplode", 2)
 };

})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Weakening Goopfly", "Weakening Goopfly")
.SetSprites("WEAKY.png", "WEAKY BG.png")
.SetStats(26,1, 2)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                        {
SStack("Frost",3)

    };
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {

           
    };
    data.traits = new List<CardData.TraitStacks>

 {
      CreateTraitStack("Friendplode", 2)
 };

})
);

        // FIGHT 2 ENCHANTED BLINGS



        cards.Add(
new CardDataBuilder(this).CreateUnit("Berryglury", "Robberry")
.SetSprites("BERRYGLURY.png", "BLINGBG.png")
.SetStats(7, 1, 1)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(30)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                 {


};
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
 {
     SStack("Bling Steal",15),SStack("When Hit Apply Gold To Attacker (No Ping)",5)

};
data.traits = new List<CardData.TraitStacks>

{

};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Blingclops", "Beringclops")
.SetSprites("BLINGCLOPS.png", "BLINGBG.png")
.SetStats(14, 6, 4)
.WithCardType("Enemy")
.WithFlavour("")
.WithValue(30)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                     {


};
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
     {
         SStack("When Hit Apply Gold To Attacker (No Ping)",20),SStack("Bling Steal",40)

};
 data.traits = new List<CardData.TraitStacks>

 {
     CreateTraitStack("Greed", 1)
 };

})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Chest", "Chest")
.SetSprites("REALCHEST.png", "BLINGBG2.png")
.SetStats(null, null, 0)
.WithCardType("Clunker")
.WithFlavour("")
.WithValue(30)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                        {


   };
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
            SStack("Destroy to Blings",100),
            SStack("Scrap",1)
   };
    data.traits = new List<CardData.TraitStacks>

    {

    };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("FAKEChest", "Chest")
.SetSprites("FAKECHEST.png", "BLINGBG2.png")
.SetStats(null, null, 0)
.WithCardType("Clunker")
.WithFlavour("")
.WithValue(30)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                {


};
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
   SStack("Destroy to Blings",100),
            SStack("Scrap",1),SStack("Mimicgo",1)
};
data.traits = new List<CardData.TraitStacks>

{

};

})
);



        cards.Add(
new CardDataBuilder(this).CreateUnit("Mimic", "Mimic")
.SetSprites("MIMIC.png", "BLINGBG.png")
.SetStats(11, 3, 3)
.WithCardType("Enemy")
.WithValue(30)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                   {


};
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
   {

      SStack("Bling Steal",50), SStack("Scrap",1),SStack("ImmuneToSnow",1)
};
data.traits = new List<CardData.TraitStacks>

{
     CreateTraitStack("Greed", 1)
};

})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Bling bird", "Bling Birb")
.SetSprites("BLINGBIRB.png", "BLINGBG2.png")
.SetStats(25, 3, 3)
.WithCardType("Miniboss")
.WithValue(30)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

  data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                       {


};
  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
       {
           SStack("Bling Steal",100),SStack("Frenzy Snowball",1)
};
  data.traits = new List<CardData.TraitStacks>

  {
      CreateTraitStack("Greed", 1),CreateTraitStack("Aimless", 1)
  };

})
);

        //Bombers 4

        cards.Add(
new CardDataBuilder(this).CreateUnit("Rabom", "Rabom")
.SetSprites(".png", " BG.png")
.SetStats(11, 2, 4)
.WithCardType("Enemy")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

 data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                       {
 SStack("Weakness",3)

};
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
       {

     
};
 data.traits = new List<CardData.TraitStacks>

{
     
};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Raquiby", "Raquiby")
.SetSprites(".png", " BG.png")
.SetStats(15, 1, 1)
.WithCardType("Enemy")
.WithValue(200)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                          {


};
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
              SStack("All Weakness",1)

};
data.traits = new List<CardData.TraitStacks>

{

};

})
);



        //CHARMS--------------------------------------------------------------------------------------------------------
        //CHARMS--------------------------------------------------------------------------------------------------------
        //CHARMS--------------------------------------------------------------------------------------------------------
        //CHARMS--------------------------------------------------------------------------------------------------------
        //CHARMS--------------------------------------------------------------------------------------------------------
        //CHARMS--------------------------------------------------------------------------------------------------------
        //CHARMS--------------------------------------------------------------------------------------------------------
        //CHARMS--------------------------------------------------------------------------------------------------------


        cardUpgrades = new List<CardUpgradeDataBuilder>();

 

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("HeavyExplosionCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("NukeCharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("Nukeskull Charm")                           //Sets in-game name as Glacial Charm
.WithText("Gain <keyword=goobers.hexplosion> <7>") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.WithTier(2)
 .WithPools("GeneralCharmPool")
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
 
  cardUpgrade.giveTraits = new CardData.TraitStacks[] { CreateTraitStack("HeavyExplosion", 7) };
  cardUpgrade.targetConstraints = new TargetConstraint[] {  new TargetConstraintCanBeHit() };
})
);


        cardUpgrades.Add(
 new CardUpgradeDataBuilder(this)
 .Create("CharmPotofGreed")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
 .WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
 .WithImage("Jarcharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
 .WithTitle("A piece of a Greedy Pot")                           //Sets in-game name as Glacial Charm
 .WithText("Gain <keyword=goobers.drawa> <2>") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
 .WithTier(2)
 .WithPools("GeneralCharmPool")
 .SubscribeToAfterAllBuildEvent(
 (cardUpgrade) =>
 {
     cardUpgrade.effects = new CardData.StatusEffectStacks[] { SStack("Hit me", 2) };
 })
 );

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("WaffleCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("Wafflecharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("Yummy Waffle")                           //Sets in-game name as Glacial Charm
.WithText("Gain <1> <keyword=ch>, and +1 <keyword=attack>") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.WithTier(2)
.ChangeDamage(2)
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
   cardUpgrade.effects = new CardData.StatusEffectStacks[] { SStack("Choco", 1)};
    cardUpgrade.targetConstraints = new TargetConstraint[] { new TargetConstraintDoesAttack(), new TargetConstraintAttackMoreThan() { value = 0 } };
})
);

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("CakeCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("Cakecharm2.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("A Cake on Display")                           //Sets in-game name as Glacial Charm
.WithText("Gain <2> <keyword=cake>, and -2 <keyword=attack>") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.WithTier(2)
.ChangeDamage(-2)
 .WithPools("GeneralCharmPool")
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
cardUpgrade.effects = new CardData.StatusEffectStacks[] { SStack("Cake", 2) };
    cardUpgrade.targetConstraints = new TargetConstraint[] { new TargetConstraintDoesAttack(), new TargetConstraintAttackMoreThan() { value = 2 } };
})
);

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("LockCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("Lockcharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("LOCK")                           //Sets in-game name as Glacial Charm
.WithText("Gain <keyword=goobers.retain>, this charm does not take a charm slot.") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.WithTier(2)
 .WithPools("GeneralCharmPool")
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
    cardUpgrade.takeSlot = false;
    cardUpgrade.giveTraits = new CardData.TraitStacks[] { CreateTraitStack("Retain", 1) };
})
);

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("BleedCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("BloodCharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("Ritual Knife Charm")                           //Sets in-game name as Glacial Charm
.WithText("Apply <1> <keyword=bleed>") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.WithTier(2)
 .WithPools("GeneralCharmPool")
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
    cardUpgrade.attackEffects = new CardData.StatusEffectStacks[] { SStack("Bleeding", 1) };

})
);

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("ExpressoCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("Expresscharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("Spilling Expresso")                           //Sets in-game name as Glacial Charm
.WithText("Gain <2> <keyword=ex> , -1 <keyword=attack>") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.WithTier(2)
.ChangeDamage(-1)
 .WithPools("GeneralCharmPool")
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
    cardUpgrade.effects = new CardData.StatusEffectStacks[] { SStack("Expresso", 2)};
    cardUpgrade.targetConstraints = new TargetConstraint[] { new TargetConstraintDoesAttack() , new TargetConstraintAttackMoreThan() { value = 1 } };
})
);



        cardUpgrades.Add(
 new CardUpgradeDataBuilder(this)
 .Create("VendyShieldCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
 .WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
 .WithImage("Vendingcharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
 .WithTitle("Mini Vending Machine")                           //Sets in-game name as Glacial Charm
 .WithText("Gain <keyword=goobers.vendy> <3>") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
 .WithTier(1)
  .WithPools("GeneralCharmPool")
 .SubscribeToAfterAllBuildEvent(
 (cardUpgrade) =>
 {
     cardUpgrade.giveTraits = new CardData.TraitStacks[] { CreateTraitStack("GreasyShield", 3) };
     cardUpgrade.targetConstraints = new TargetConstraint[] { new TargetConstraintDoesAttack(), new TargetConstraintCanBeHit() };
 })
 );




        cardUpgrades.Add(
     new CardUpgradeDataBuilder(this)
     .Create("AshiCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
     .WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
     .WithImage("Ashishicharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
     .WithTitle("Mini Ashi Shi Charm- Bling Bling Friendo!")                           //Sets in-game name as Glacial Charm
     .WithText("When hit, gain x8 <keyword=blings> equal damage taken.") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 

     .SubscribeToAfterAllBuildEvent(
     (cardUpgrade) =>
     {
         cardUpgrade.effects = new CardData.StatusEffectStacks[] { SStack("Borrowed Ashi", 1) };
         cardUpgrade.targetConstraints = new TargetConstraint[] { new TargetConstraintCanBeHit() };
     })
     );

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("TerrorCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("Terrorcharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("Mini Terrormisu Charm - >:C")                           //Sets in-game name as Glacial Charm
.WithText("Trigger ally ahead, Deal <2> damage to self. This charm does not take a charm slot.") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 

.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
    cardUpgrade.takeSlot = false;
    cardUpgrade.effects = new CardData.StatusEffectStacks[] { SStack("Trigger Front2", 1), SStack("On Card Played Damage To Self", 1) };
    cardUpgrade.targetConstraints = new TargetConstraint[] { new TargetConstraintHasHealth() };
})
);

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("FancySwordCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("Heavyblow.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("Polished Sword")                           //Sets in-game name as Glacial Charm
.WithText("<+8> <keyword=attack>, this charm does not take a charm slot.")
.ChangeDamage(8)//Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
    cardUpgrade.takeSlot = false;

    cardUpgrade.targetConstraints = new TargetConstraint[] { new TargetConstraintDoesAttack(), new TargetConstraintAttackMoreThan() { value = 0 } };
})
);


        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("BlunkyCharm")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("Blunkycharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("Blunky's Upgraded Ice Cube Maker")                           //Sets in-game name as Glacial Charm
.WithText("When deployed, gain <2> <keyword=block>") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
   cardUpgrade.effects = new CardData.StatusEffectStacks[] { SStack("When Deployed Apply Block To Self", 2) };
    cardUpgrade.targetConstraints = new[] { new TargetConstraintIsUnit() };
})
);

     

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("CharmSweetStart")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("Sweetpointcharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("Sweet Starter Charm")                           //Sets in-game name as Glacial Charm
.WithText("Gain <keyword=goobers.spstart> 16") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.WithTier(2)
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
    cardUpgrade.giveTraits = new CardData.TraitStacks[] { CreateTraitStack("Sweet Start", 16) };
    cardUpgrade.targetConstraints = new[] { new TargetConstraintIsUnit() };
})
);

        cardUpgrades.Add(
  new CardUpgradeDataBuilder(this)
  .Create("CharmMaid")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
  .WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
  .WithImage("MaidCharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
  .WithTitle("Maid Outfit")                           //Sets in-game name as Glacial Charm
  .WithText("Gain <keyword=goobers.maid> <3>") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
  .WithTier(2)
  .SubscribeToAfterAllBuildEvent(
  (cardUpgrade) =>
  {
      cardUpgrade.giveTraits = new CardData.TraitStacks[] { CreateTraitStack("Maid", 3) };
      cardUpgrade.targetConstraints = new[] { new TargetConstraintIsUnit() };

  })
  );

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("CharmSweettooth")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("Sweettooth.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("Pecan Charm")                           //Sets in-game name as Glacial Charm
.WithText("Gain <keyword=goobers.pecan> 1") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.WithTier(2)
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
    cardUpgrade.giveTraits = new CardData.TraitStacks[] { CreateTraitStack("Pecan", 1) };
    cardUpgrade.targetConstraints = new[] { new TargetConstraintCanBeHit()};
})
);

        cardUpgrades.Add(
new CardUpgradeDataBuilder(this)
.Create("CharmCravings")                    //Internally named as CardUpgradeGlacial, sets its type to charm, and adds it to the general pool
.WithType(CardUpgradeData.Type.Charm)                 //Not needed since we used CreateCharm (why did I put this here :/). If we do not want the charm in the general pool, you would have to use this method to make the upgrade a charm.
.WithImage("cravings.png")                        //Sets the image file path to "GlacialCharm.png". See below.
.WithTitle("Hangry Charm")                           //Sets in-game name as Glacial Charm
.WithText("Gain <keyword=goobers.hangry> <2>") //Get allows me to skip the GUID. The Text class does not.                                                                   //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
.WithTier(2)
.SubscribeToAfterAllBuildEvent(
(cardUpgrade) =>
{
    cardUpgrade.giveTraits = new CardData.TraitStacks[] { CreateTraitStack("Hangry", 2) };
    cardUpgrade.targetConstraints = new TargetConstraint[] { new TargetConstraintDoesAttack(), new TargetConstraintCanBeHit() };
})
);


       




        traitEffects = new List<TraitDataBuilder>();

        traitEffects.Add(new TraitDataBuilder(this)
      .Create("TagR")
  .SubscribeToAfterAllBuildEvent(
      (trait) => 
      {
            trait.keyword = Get<KeywordData>("tagrecycle");
            trait.effects = new StatusEffectData[] { Get<StatusEffectData>("When fortune dies gain again"), Get<StatusEffectData>("When SunTag dies gain again"),
            Get<StatusEffectData>("When TeethTag dies gain again"),Get<StatusEffectData>("When DetonatorTag dies gain again"),Get<StatusEffectData>("When NovaTag dies gain again"),
            Get<StatusEffectData>("When LuminTag dies gain again"),Get<StatusEffectData>("When DemonTag dies gain again"),Get<StatusEffectData>("When WinterTag dies gain again"),
            Get<StatusEffectData>("When SnowTag dies gain again"),Get<StatusEffectData>("When HealingTag dies gain again"),Get<StatusEffectData>("When ConnectionTag dies gain again"),
            Get<StatusEffectData>("When GoblingTag dies gain again"),Get<StatusEffectData>("When SplitTag dies gain again"),Get<StatusEffectData>("When Restag dies gain again")};
      })); 

        traitEffects.Add(new TraitDataBuilder(this)
       .Create("Caketrait")
   .SubscribeToAfterAllBuildEvent(
       (trait) =>
       {
           trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Hit All Enemies") };
       }));


        traitEffects.Add(new TraitDataBuilder(this)
        .Create("Maid")
    .SubscribeToAfterAllBuildEvent(
        (trait) =>
        {
            trait.keyword = Get<KeywordData>("maid");
            trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Maid")};
        }));

        traitEffects.Add(new TraitDataBuilder(this)
    .Create("GrandMaid")
.SubscribeToAfterAllBuildEvent(
    (trait) =>
    {
        trait.keyword = Get<KeywordData>("gmaid");
        trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Grand Maid") };
    }));

        traitEffects.Add(new TraitDataBuilder(this)
  .Create("Sweet Start")
.SubscribeToAfterAllBuildEvent(
  (trait) =>
  {
      trait.keyword = Get<KeywordData>("spstart");
      trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Sweet Start") };
  }));
        traitEffects.Add(new TraitDataBuilder(this)
          .Create("Cunning")
.SubscribeToAfterAllBuildEvent(
  (trait) =>
  {
      trait.keyword = Get<KeywordData>("cunning");
      trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Cunning") };
  }));


        traitEffects.Add(new TraitDataBuilder(this)
.Create("Retain")
.SubscribeToAfterAllBuildEvent(
 (trait) =>
 {
     trait.keyword = Get<KeywordData>("retain");

 }));


        traitEffects.Add(new TraitDataBuilder(this)
    .Create("Taunt")
    .SubscribeToAfterAllBuildEvent(
     (trait) =>
     {
         trait.keyword = Get<KeywordData>("taunt");
         trait.effects = new StatusEffectData[] { Get<StatusEffectData>("While Active Taunted To Enemies") };
     }));

          traitEffects.Add(new TraitDataBuilder(this)
    .Create("Taunted")
    .SubscribeToAfterAllBuildEvent(
     (trait) =>
     {
         trait.keyword = Get<KeywordData>("taunted");
         trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Hit All Taunt") };
     }));

        traitEffects.Add(new TraitDataBuilder(this)
  .Create("Await")
  .SubscribeToAfterAllBuildEvent(
   (trait) =>
   {
       trait.keyword = Get<KeywordData>("await");
   }));

        traitEffects.Add(new TraitDataBuilder(this)
.Create("HeavyExplosion")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
  trait.keyword = Get<KeywordData>("hexplosion");
  trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Heavy Explsion") };
}));

        traitEffects.Add(new TraitDataBuilder(this)
.Create("BlindShot")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
trait.keyword = Get<KeywordData>("bshot");
trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Hit Truly Random Target") };
}));

        traitEffects.Add(new TraitDataBuilder(this)
.Create("Sweettooth")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
  trait.keyword = Get<KeywordData>("sweettooth");
  trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Sweet Points into Health") };
}));
        traitEffects.Add(new TraitDataBuilder(this)
.Create("Cravings")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
  trait.keyword = Get<KeywordData>("cravings");
  trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Sweet Points into Attack") };
}));

        traitEffects.Add(new TraitDataBuilder(this)
     .Create("Bloodlust")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
    trait.keyword = Get<KeywordData>("bloodlust");
    trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Trigger Self when card destroyed") };
}));





        traitEffects.Add(new TraitDataBuilder(this)
.Create("Friendplode")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
    trait.keyword = Get<KeywordData>("friendlyexplode");
    trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Friend Heavy Explsion") };
}));


        traitEffects.Add(new TraitDataBuilder(this)
.Create("Hypernova")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
trait.keyword = Get<KeywordData>("hypernova");
trait.effects = new StatusEffectData[] { Get<StatusEffectData>("SUPERNOVA") };
}));


        traitEffects.Add(new TraitDataBuilder(this)
.Create("Sweetsy")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
trait.keyword = Get<KeywordData>("sweetsy");
trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Sweet Points into MAX Health") };
}));

        traitEffects.Add(new TraitDataBuilder(this)
.Create("HardDiscard")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
trait.keyword = Get<KeywordData>("nobell");
trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Discard Destroy") };
})); 

        traitEffects.Add(new TraitDataBuilder(this)
.Create("Pecan")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
    trait.keyword = Get<KeywordData>("pecan");
    trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Pecan") };
}));

        traitEffects.Add(new TraitDataBuilder(this)
   .Create("Revenge")
   .SubscribeToAfterAllBuildEvent(
   (trait) =>
   {
       trait.keyword = Get<KeywordData>("revenge");
       trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Trigger Self when ally killed") };
   }));

        traitEffects.Add(new TraitDataBuilder(this)
.Create("AnchorMinorTarget")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
            trait.keyword = Get<KeywordData>("anchorminor");
        }));

        traitEffects.Add(new TraitDataBuilder(this)
.Create("GreasyShield")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
 trait.keyword = Get<KeywordData>("vendy");
 trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Venda When Hit") };
}));


        traitEffects.Add(new TraitDataBuilder(this)
.Create("Hangry")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
trait.keyword = Get<KeywordData>("hangry");
trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Hangry") };
}));


        traitEffects.Add(new TraitDataBuilder(this)
.Create("Notbothered")
.SubscribeToAfterAllBuildEvent(
(trait) =>
{
 trait.keyword = Get<KeywordData>("unbothered");
 trait.effects = new StatusEffectData[] { Get<StatusEffectData>("Owntempo") };
}));
        

        keywords = new List<KeywordDataBuilder>();

        keywords.Add(
new KeywordDataBuilder(this)
.Create("unbothered")                               //The internal name for the upgrade.
.WithTitle("Own Tempo")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.7f, 7f, 0.1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Cannot gain <keyword=frenzy> in battle") //Format is body|note.
.WithNoteColour(new Color(0.5f, 0.4f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.5f, 0.4f, 0f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("hangry")                               //The internal name for the upgrade.
.WithTitle("Hangry")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.7f, 0f, 0.9f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("When <keyword=sp>'d gain <keyword=spice> as well") //Format is body|note.
.WithNoteColour(new Color(0.4f, 0.4f, 0.4f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(true)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("charm")                               //The internal name for the upgrade.
.WithTitle("Charm (not a card)")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.7f, 0f, 0.9f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("A charm, card displays the effects of the charm.| UNLESS YOU CHEAT IT TO YOUR DECK >:C") //Format is body|note.
.WithNoteColour(new Color(0.4f, 0.4f, 0.4f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("acceptance")                               //The internal name for the upgrade.
.WithTitle("Anchor Major: Acceptance")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(1f, 0f, 0f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Needs <card=goobers.Chalice>, <card=goobers.Blood> to fuse.| Just needs to be in your deck, hand or board.") //Format is body|note.
.WithNoteColour(new Color(0.4f, 0.4f, 0.4f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);


        keywords.Add(
new KeywordDataBuilder(this)
.Create("vendy")                               //The internal name for the upgrade.
.WithTitle("Greasypop Shield")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0f, 0f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("When hit, gain/apply either 22%<keyword=spice>, 22%<keyword=shell>, 22%restore <keyword=health>, 22%countdown <keyword=counter>, 5%<keyword=scrap> or 5%<keyword=frenzy> to self.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(true)
);


        keywords.Add(
new KeywordDataBuilder(this)
.Create("unlimita")                               //The internal name for the upgrade.
.WithTitle("Time to Work")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0f, 0f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Special Trigger: If you have my service number, I can call in the girls to help whenever you like :)") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("revenge")                               //The internal name for the upgrade.
.WithTitle("Revenge")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Trigger when ally is killed.") //Format is body|note.
.WithNoteColour(new Color(0.4f, 0.4f, 0.4f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("pecan")                               //The internal name for the upgrade.
.WithTitle("Pecan")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("When applied with <keyword=sp> gain <keyword=shell>.") //Format is body|note.
.WithNoteColour(new Color(0.4f, 0.4f, 0.4f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(true)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("finding")                               //The internal name for the upgrade.
.WithTitle("Searching")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0f, 0.5f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Special Trigger: I have a second reason for taking on this quest, I'm looking for someone important.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.3f, 1f))  //Somewhat teal
.WithBodyColour(new Color(0.8f, 0.5f, 0.8f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
 new KeywordDataBuilder(this)
 .Create("nobell")                               //The internal name for the upgrade.
 .WithTitle("Hard Discard")                            //The in-game name for the upgrade.
 .WithTitleColour(new Color(1f, 0.5f, 5f)) //Light purple on the title of the keyword pop-up
 .WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
 .WithDescription("When discarded, destroy this card.") //Format is body|note.
 .WithNoteColour(new Color(1f, 1f, 1f))  //Somewhat teal
 .WithBodyColour(new Color(0.8f, 0.8f, 0.8f))       //Cyan-ish
 .WithCanStack(false)
 );


        keywords.Add(
    new KeywordDataBuilder(this)
    .Create("sweetsy")                               //The internal name for the upgrade.
    .WithTitle("Sweetsy")                            //The in-game name for the upgrade.
    .WithTitleColour(new Color(1f, 0.5f, 5f)) //Light purple on the title of the keyword pop-up
    .WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
    .WithDescription("When applied with <keyword=sp> increase max <keyword=health> instead.") //Format is body|note.
    .WithNoteColour(new Color(1f, 1f, 1f))  //Somewhat teal
    .WithBodyColour(new Color(0.8f, 0.8f, 0.8f))       //Cyan-ish
    .WithCanStack(false)
    );

        keywords.Add(
    new KeywordDataBuilder(this)
    .Create("tag")                               //The internal name for the upgrade.
    .WithTitle("Tag")                            //The in-game name for the upgrade.
    .WithTitleColour(new Color(0f, 0.9f, 0f)) //Light purple on the title of the keyword pop-up
    .WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
    .WithDescription("A special status that gives a unit an additional effect, but can be cancelled out with <keyword=null>.") //Format is body|note.
    .WithNoteColour(new Color(0.85f, 0.9f, 1f))  //Somewhat teal
    .WithBodyColour(new Color(0.8f, 0.8f, 0.8f))       //Cyan-ish
    .WithCanStack(false)
    );



        keywords.Add(
new KeywordDataBuilder(this)
.Create("hypernova")                               //The internal name for the upgrade.
.WithTitle("Achieve Supernova")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0f, 0.5f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Special Trigger: She can ascend past Nova, she just needs an attachment.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.3f, 1f))  //Somewhat teal
.WithBodyColour(new Color(0.8f, 0.5f, 0.8f))       //Cyan-ish
.WithCanStack(false)
);


        keywords.Add(
new KeywordDataBuilder(this)
.Create("friendlyexplode")                               //The internal name for the upgrade.
.WithTitle("Friendly Explosion")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0.7f, 0f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("When destroyed, deal damage to all allies.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(true)
);


        keywords.Add(
new KeywordDataBuilder(this)
.Create("tagrecycle")                               //The internal name for the upgrade.
.WithTitle("Tag Retriever")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0f, 0f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("When any unit with a Tag Status is killed, regain that specific tag to your hand.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("caketrait")                               //The internal name for the upgrade.
.WithTitle("Hit all Enemies")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0f, 0f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Hit all Enemies") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("velvetfuse")                               //The internal name for the upgrade.
.WithTitle("Anchor Major: Bloody Enhancement")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0f, 0f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Needs <card=goobers.BloodKnife> to fuse.| Just needs to be in your deck, hand or board.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("warning")                               //The internal name for the upgrade.
.WithTitle("Warning")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0f, 0f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Do not pick this if you have 2 units crowned, as you will get softlocked and you'll need to reset the fight.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("momoa")                               //The internal name for the upgrade.
.WithTitle("Papa Is Scary Now")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Special Trigger: Papa is scary, I hope to save my family from him...") //Format is body|note.
.WithNoteColour(new Color(0.4f, 0.4f, 0.4f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
  new KeywordDataBuilder(this)
  .Create("bloodlust")                               //The internal name for the upgrade.
  .WithTitle("Bloodlust")                            //The in-game name for the upgrade.
  .WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
  .WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
  .WithDescription("Trigger self when any unit is destroyed.") //Format is body|note.
  .WithNoteColour(new Color(0.4f, 0.4f, 0.4f))  //Somewhat teal
  .WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
  .WithCanStack(false)                             //The keyword does not show its stack number.
  );
        keywords.Add(
   new KeywordDataBuilder(this)
   .Create("cravings")                               //The internal name for the upgrade.
   .WithTitle("Cravings")                            //The in-game name for the upgrade.
   .WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
   .WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
   .WithDescription("When applied with <keyword=sp> gain <keyword=spice> instead.") //Format is body|note.
   .WithNoteColour(new Color(0.4f, 0.4f, 0.4f))  //Somewhat teal
   .WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
   .WithCanStack(false)                             //The keyword does not show its stack number.
   );
        keywords.Add(
new KeywordDataBuilder(this)
.Create("sweettooth")                               //The internal name for the upgrade.
.WithTitle("Sweet Tooth")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("When applied with <keyword=sp> gain <keyword=shell> instead.") //Format is body|note.
.WithNoteColour(new Color(0.4f, 0.4f, 0.4f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("bananasplit")                               //The internal name for the upgrade.
.WithTitle("Anchor Major: Banana Split")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Needs <card=goobers.Vanillog>, <card=goobers.Chocolog>, <card=goobers.Strawberilog> to fuse.| Just needs to be in your deck, hand or board.") //Format is body|note.
.WithNoteColour(new Color(0.4f, 0.4f, 0.4f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("anchor")                               //The internal name for the upgrade.
.WithTitle("Anchor Major")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Cards that need certain cards to fuse in battle. | Charms that the cards had will be carried over") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("bshot")
.WithTitleColour(new Color(0.50f, 0.50f, 0.85f))
.WithTitle("Blindshot")
.WithShowName(true)
.WithDescription("Hits a random enemy in any row.")
.WithCanStack(false)
);

        keywords.Add(
 new KeywordDataBuilder(this)
 .Create("hexplosion")
 .WithTitleColour(new Color(0.50f, 0.50f, 0.85f))
 .WithTitle("Heavy Explosion")
 .WithShowName(true)
 .WithDescription("When destroyed, deal damage to all enemies.")
 .WithCanStack(true)
 );

        keywords.Add(
    new KeywordDataBuilder(this)
    .Create("unfrenzy")
    .WithTitle("Unfrenzy")
    .WithShowName(true)
    .WithDescription("Only triggers once reguardless of if this card has frenzy.| This is a weird bug :( Thankfully the units deployed are not affected just this card")
    .WithCanStack(false)
    );
        keywords.Add(
       new KeywordDataBuilder(this)
       .Create("await")
       .WithTitle("Awaiting Command")
       .WithShowName(true)
       .WithDescription("Trigger when when Tiramisu triggers.")
       .WithCanStack(false)
       );

        keywords.Add(
           new KeywordDataBuilder(this)
           .Create("taunt")
           .WithTitle("Taunt")
           .WithShowName(true)
           .WithDescription("All enemies are <keyword=goobers.taunted>")
           .WithCanStack(false)
           );

        //Taunted Keyword
        keywords.Add(
           new KeywordDataBuilder(this)
           .Create("taunted")
           .WithTitle("Taunted")
           .WithShowName(true)
           .WithDescription("Target only enemies with <keyword=goobers.taunt>|Hits them all!")
           .WithCanStack(false)
           );


        keywords.Add(
new KeywordDataBuilder(this)
.Create("retain")                               //The internal name for the upgrade.
.WithTitle("Retain")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Cannot be discarded.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);
        keywords.Add(
new KeywordDataBuilder(this)
.Create("loadout")                               //The internal name for the upgrade.
.WithTitle("Loadout")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Charms equipped onto this card will be copied to the unit deployed.(The <keyword=health>,<keyword=attack> and <keyword=counter> are there as a point of reference)") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);
        keywords.Add(
new KeywordDataBuilder(this)
.Create("yellat")                               //The internal name for the upgrade.
.WithTitle("Getting Yelled At")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0f, 0.5f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Special Trigger: I hope my boss isn't close by :c.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.85f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)
);
        keywords.Add(
new KeywordDataBuilder(this)
.Create("inky")                               //The internal name for the upgrade.
.WithTitle("Where could it be?")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0f, 0.5f, 1f))//Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Special Trigger: Hmmm... Where is my stone :3???.") //Format is body|note.
.WithNoteColour(new Color(0f, 0f, 0f))  //Somewhat teal
.WithBodyColour(new Color(1f, 1f, 1f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("sunray")                               //The internal name for the upgrade.
.WithTitle("Band Back Together")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0f, 0.5f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Special Trigger: I need to find my bandmates! They're freezing out there.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.85f, 0.1f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("dormant")                               //The internal name for the upgrade.
.WithTitle("Dormant")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0f, 0.5f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Special Trigger: A sacrifice.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("damaged")                               //The internal name for the upgrade.
.WithTitle("Damaged")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0.90f, 0.10f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Special Trigger: There has to be a way to restore them to their former glory!") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("candle")                               //The internal name for the upgrade.
.WithTitle("Caution")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0f, 0.5f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Special Trigger: I hope Candle is safe back at home, if I saw her I'm ditching the team and bringing her back home.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.2f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("cunning")                               //The internal name for the upgrade.
.WithTitle("Cunning")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0.90f, 0.10f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Trigger when Item is used.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);
        keywords.Add(
new KeywordDataBuilder(this)
.Create("spstart")                               //The internal name for the upgrade.
.WithTitle("Sweet Start")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0.90f, 0.10f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("When deployed, gain <keyword=sp>.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(true)                             //The keyword does not show its stack number.
);
        keywords.Add(
new KeywordDataBuilder(this)
.Create("anchorminor")                               //The internal name for the upgrade.
.WithTitle("Anchor Minor")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0.90f, 0.10f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("When a <keyword=goobers.minorin> is used on this card, upgrade to a different card permanently.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("maid")                               //The internal name for the upgrade.
.WithTitle("Maid")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0.90f, 0.10f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("While active, increase <keyword=attack> of all <keyword=goobers.maid> units on the field.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(true)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("gmaid")                               //The internal name for the upgrade.
.WithTitle("Grand Maid")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.90f, 0.90f, 0.10f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Gain <keyword=attack> for every ally <keyword=goobers.maid> on the field.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(true)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("minorin")                               //The internal name for the upgrade.
.WithTitle("Ingredient Minor")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(0.50f, 0.50f, 0.85f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Used to upgrade <keyword=goobers.anchorminor>.") //Format is body|note.
.WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
.WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);
        keywords.Add(
    new KeywordDataBuilder(this)
    .Create("special")                               //The internal name for the upgrade.
    .WithTitle("Special Trigger")                            //The in-game name for the upgrade.
    .WithTitleColour(new Color(0.85f, 0.44f, 0.85f)) //Light purple on the title of the keyword pop-up
    .WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
    .WithDescription("This card has a hidden effect with a hidden trigger to activate") //Format is body|note.
    .WithNoteColour(new Color(0.85f, 0.44f, 0.85f))  //Somewhat teal
    .WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
    .WithCanStack(false)                             //The keyword does not show its stack number.
    );

        keywords.Add(
 new KeywordDataBuilder(this)
 .Create("unplayable")                               //The internal name for the upgrade.
 .WithTitle("Unplayable")                            //The in-game name for the upgrade.
 .WithTitleColour(new Color(1f, 1f, 1f)) //Light purple on the title of the keyword pop-up
 .WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
 .WithDescription("Cannot be played") //Format is body|note.
 .WithNoteColour(new Color(1f, 1f, 1f))  //Somewhat teal
 .WithBodyColour(new Color(0.2f, 0.5f, 0.5f))       //Cyan-ish
 .WithCanStack(false)                             //The keyword does not show its stack number.
 );

        keywords.Add(
 new KeywordDataBuilder(this)
 .Create("carry")                               //The internal name for the upgrade.
 .WithTitle("Inherit")                            //The in-game name for the upgrade.
 .WithTitleColour(new Color(1f, 1f, 1f)) //Light purple on the title of the keyword pop-up
 .WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
 .WithDescription("Charms carry over to the next form.") //Format is body|note.
 .WithNoteColour(new Color(1f, 1f, 1f))  //Somewhat teal
 .WithBodyColour(new Color(0.8f, 0.5f, 0.6f))       //Cyan-ish
 .WithCanStack(false)                             //The keyword does not show its stack number.
 );
        keywords.Add(
new KeywordDataBuilder(this)
.Create("temp")                               //The internal name for the upgrade.
.WithTitle("Temp-Shift")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(1f, 1f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Temporarly turns into another card, reverts back after the end of the battle.") //Format is body|note.
.WithNoteColour(new Color(1f, 1f, 1f))  //Somewhat teal
.WithBodyColour(new Color(0.8f, 0.5f, 0.6f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("perma")                               //The internal name for the upgrade.
.WithTitle("Perma-Shift")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(1f, 1f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Permanently turns into another card for the entire run.") //Format is body|note.
.WithNoteColour(new Color(1f, 1f, 1f))  //Somewhat teal
.WithBodyColour(new Color(0.8f, 0.5f, 0.6f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("nope")                               //The internal name for the upgrade.
.WithTitle("Cannot be Hit")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(1f, 1f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Cannot be hit by enemies. But still vulnerable to allies inflicted with <keyword=haze>") //Format is body|note.
.WithNoteColour(new Color(1f, 1f, 1f))  //Somewhat teal
.WithBodyColour(new Color(0.8f, 0.5f, 0.6f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("nuh")                               //The internal name for the upgrade.
.WithTitle("No")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(1f, 1f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("Will not equip charms.") //Format is body|note.
.WithNoteColour(new Color(1f, 1f, 1f))  //Somewhat teal
.WithBodyColour(new Color(0.8f, 0.5f, 0.6f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
       new KeywordDataBuilder(this)
       .Create("splash")                               //The internal name for the upgrade.
       .WithTitle("Splash Damage")                            //The in-game name for the upgrade.
       .WithTitleColour(new Color(1f, 1f, 1f)) //Light purple on the title of the keyword pop-up
       .WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
       .WithDescription("Deal damage to nearby enemies.") //Format is body|note.
       .WithNoteColour(new Color(1f, 1f, 0f))  //Somewhat teal
       .WithBodyColour(new Color(0.9f, 0.7f, 0.2f))       //Cyan-ish
       .WithCanStack(false)                             //The keyword does not show its stack number.
       );

        keywords.Add(
    new KeywordDataBuilder(this)
    .Create("anchor")                               //The internal name for the upgrade.
    .WithTitle("Anchor")                            //The in-game name for the upgrade.
    .WithTitleColour(new Color(1f, 1f, 1f)) //Light purple on the title of the keyword pop-up
    .WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
    .WithDescription("The main material for fusing, retains charms when fusing.") //Format is body|note.
    .WithNoteColour(new Color(1f, 1f, 0f))  //Somewhat teal
    .WithBodyColour(new Color(0.9f, 0.7f, 0.2f))       //Cyan-ish
    .WithCanStack(false)                             //The keyword does not show its stack number.
    );

        keywords.Add(
new KeywordDataBuilder(this)
.Create("oneshot")                               //The internal name for the upgrade.
.WithTitle("One Shot")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(1f, 1f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("After use, remove this card from your deck.") //Format is body|note.
.WithNoteColour(new Color(1f, 1f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.9f, 0.7f, 0.2f))       //Cyan-ish
.WithCanStack(false)                             //The keyword does not show its stack number.
);

        keywords.Add(
new KeywordDataBuilder(this)
.Create("drawa")                               //The internal name for the upgrade.
.WithTitle("Draw Again")                            //The in-game name for the upgrade.
.WithTitleColour(new Color(1f, 1f, 1f)) //Light purple on the title of the keyword pop-up
.WithShowName(true)                              //Shows name in Keyword box (as opposed to a nonexistant icon).
.WithDescription("When drawn, draw another card") //Format is body|note.
.WithNoteColour(new Color(1f, 1f, 0f))  //Somewhat teal
.WithBodyColour(new Color(0.9f, 0.7f, 0.2f))       //Cyan-ish
.WithCanStack(true)
//The keyword does not show its stack number.
);
        tribes = new List<ClassDataBuilder>();

        tribes.Add(TribeCopy("Clunk", "Draw")
     .WithFlag("Images/Goober Banner.png")
     .WithSelectSfxEvent(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/location/charmmerchant/visit"))
     .SubscribeToAfterAllBuildEvent(   //New lines start here
     (data) =>                         //Other tutorials typically write out delegate here, this is the condensed notation (data is assumed to be ClassData)
     {
         GameObject gameObject = data.characterPrefab.gameObject.InstantiateKeepName();   //Copy the previous prefab
         UnityEngine.Object.DontDestroyOnLoad(gameObject);                                //GameObject may be destroyed if their scene is unloaded. This ensures that will never happen to our gameObject
         gameObject.name = "Player (Tutorial.Draw)";                                      //For comparison, the clunkmaster character is named "Player (Clunk)"
         data.characterPrefab = gameObject.GetComponent<Character>();
         data.leaders = DataList<CardData>("Bucket", "Cea", "Strike", "?", "Sasha", "Coffee", "Terror", "Cherry");
         Inventory inventory = new Inventory();
         inventory.deck.list = DataList<CardData>("Bat", "Bat", "Bat", "SHOTGUN", "SHOTGUN", "Popmint", "Popmint", "PopSpice", "SBerry", "KFez", "Punchy").ToList(); //Some odds and ends

         data.startingInventory = inventory;

         RewardPool unitPool = CreateRewardPool("GoobersUnitPool", "Units", DataList<CardData>( //The 
          "Inkabom", "Sunray", "M1", "Hateu", "Theinfested", "Momo", "Yra", "Tala", "Raven", "Luvu", "Soluna", "Sharoco", "Newtral", "Sunburn", "Sunscreen"
          , "Bombom", "Tusk", "Zula", "Kokonut", "Pyra", "Dimona", "Chompom"));

         RewardPool itemPool = CreateRewardPool("GoobersItemPool", "Items", DataList<CardData>(

             "Inky Ritual Stone","Peppereaper", "DragonflamePepper", "Peppering", "SpiceStones", "ZoomlinWafers", "IceShard", "FoggyBrew",
             "Blingo", "Krono", "PepperFlag", "MobileCampfire", "Madness", "Joob", "Peppermaton",
             "Bumblebee", "BlazeTea", "Recycler", "Fixer"));


         data.rewardPools = new RewardPool[]
         {

    unitPool,
    itemPool,
    Extensions.GetRewardPool("GeneralUnitPool"),
    Extensions.GetRewardPool("GeneralItemPool"),
    Extensions.GetRewardPool("GeneralCharmPool"),
    Extensions.GetRewardPool("GeneralModifierPool"),        //The snow pools are not Snowdwellers, there are general snow units/cards/charms.
    Extensions.GetRewardPool("SnowCharmPool"),        //
         };



         //Set the characterPrefab to our new prefab
     })                                                                                //The above line may need one of the FMOD references
  );

        tribes.Add(TribeCopy("Magic", "Candy")
        .WithFlag("Images/Candy Banner.png")
.WithSelectSfxEvent(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/status/shell"))
.SubscribeToAfterAllBuildEvent(   //New lines start here
(data) =>                         //Other tutorials typically write out delegate here, this is the condensed notation (data is assumed to be ClassData)
{
    GameObject gameObject = data.characterPrefab.gameObject.InstantiateKeepName();   //Copy the previous prefab
    UnityEngine.Object.DontDestroyOnLoad(gameObject);                                //GameObject may be destroyed if their scene is unloaded. This ensures that will never happen to our gameObject
    gameObject.name = "Player (goobers.Candy)";                                      //For comparison, the clunkmaster character is named "Player (Clunk)"
    data.characterPrefab = gameObject.GetComponent<Character>();
    data.leaders = DataList<CardData>("Ice Cream", "Tiramisu", "Mont", "Banana","Terror", "Darkia");
    Inventory inventory = new Inventory();
    inventory.deck.list = DataList<CardData>("Bknife", "Bknife", "Bknife", "Expresso", "Cof", "Ice", "Ice", "Bakey", "IPMinor", "Fuse").ToList(); //Some odds and ends

    data.startingInventory = inventory;

    RewardPool unitPool = CreateRewardPool("CandyUnitPool", "Units", DataList<CardData>( //The 
     "Soluna", "Sharoco", "Newtral", "Made", "Cake", "BloodW", "Vanillog", "Chocolog", "Strawberilog", "Peppifin", "Mousso", "Oro", "Velvet", "Cinaroll"
     ,"Crepey", "Chompom", "Espressa"));

    RewardPool itemPool = CreateRewardPool("CandyItemPool", "Items", DataList<CardData>(
       "Blender", "Cotton Candy", "Expressoplus", "Ingredient Service", "VIPSonly","Damaged Coffee", "Wafflemaker", "Waffle", "Double Muffin", "Cabinet", "Strawberry Rush", "Clown Mask",
       "Wake up","Peppereaper", "DragonflamePepper", "Peppering", "SpiceStones", "IceShard", "FoggyBrew","Blingo"));

    RewardPool ChaemPool = CreateRewardPool("CandyItemPool", "Charms", DataList<CardUpgradeData>(
        "CharmCravings", "CharmSweettooth", "CharmSweetStart", "CharmMaid" ,"CardUpgradeInk", "CardUpgradeSpice", "CardUpgradeShroom", "CardUpgradeBom", "CardUpgradeOverload", "CardUpgradeTeethWhenHit"
    ));


    data.rewardPools = new RewardPool[]
    {

    unitPool,
    itemPool,
    ChaemPool,
    Extensions.GetRewardPool("GeneralUnitPool"),
    Extensions.GetRewardPool("GeneralItemPool"),
    Extensions.GetRewardPool("GeneralCharmPool"),
    Extensions.GetRewardPool("GeneralModifierPool"),
    Extensions.GetRewardPool("SnowCharmPool"),

    };


})                                                                                //The above line may need one of the FMOD references
);

        tribes.Add(TribeCopy("Magic", "Witch")
    .WithFlag("Images/Kitsu Banner.png")
.WithSelectSfxEvent(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/status/heal"))
.SubscribeToAfterAllBuildEvent(   //New lines start here
(data) =>                         //Other tutorials typically write out delegate here, this is the condensed notation (data is assumed to be ClassData)
{
    GameObject gameObject = data.characterPrefab.gameObject.InstantiateKeepName();   //Copy the previous prefab
    UnityEngine.Object.DontDestroyOnLoad(gameObject);                                //GameObject may be destroyed if their scene is unloaded. This ensures that will never happen to our gameObject
    gameObject.name = "Player (goobers.Witch)";                                      //For comparison, the clunkmaster character is named "Player (Clunk)"
    data.characterPrefab = gameObject.GetComponent<Character>();
    data.leaders = DataList<CardData>("Ashi Leader");
    Inventory inventory = new Inventory();
    inventory.deck.list = DataList<CardData>("Tagbox","Mimic Blade", "Mimic Blade", "Mimic Blade", "Snowy Spell", "Snowy Spell", "Sunshine Bottle", "Alarm", "Whackohammer",
        "ConnectionTag", "ConnectionTag", "SnowTag", "HealingTag", "GoblingTag", "IPMinor").ToList(); //Some odds and ends

    data.startingInventory = inventory;

    RewardPool unitPool = CreateRewardPool("WitchUnitPool", "Units", DataList<CardData>( //The 
     "Made", "Cake", "BloodW", "Peppifin", "Mousso", "Oro", "Cinaroll" , "Crepey"));

    RewardPool itemPool = CreateRewardPool("WitchItemPool", "Items", DataList<CardData>(
        "VendingMachine"));

    RewardPool ChaemPool = CreateRewardPool("WitchItemPool", "Charms", DataList<CardUpgradeData>(
        "CharmCravings", "CharmSweettooth", "CharmSweetStart", "CharmMaid"
    ));


    data.rewardPools = new RewardPool[]
    {

    unitPool,
    ChaemPool,
     Extensions.GetRewardPool("GeneralUnitPool"),
    Extensions.GetRewardPool("GeneralItemPool"),
    Extensions.GetRewardPool("GeneralCharmPool"),
    Extensions.GetRewardPool("GeneralModifierPool"),
    Extensions.GetRewardPool("SnowCharmPool"),

    };

    data.requiresUnlock = TryGet<UnlockData>("Ashi Shi Tribe");

})                                                                                //The above line may need one of the FMOD references
);




        modifier = new List<GameModifierDataBuilder>();




        


















        preLoaded = true;

    }
    private void AshiAssets()
    {


        //SHI SHI CORNER---------------------------------------------------------------------------------------------------------------------------------------------------------

        cards.Add(
new CardDataBuilder(this).CreateUnit("Ashi", "Ashi Shi")
.SetSprites("Ashi.png", "Ashi BG.png")
.SetStats(1000, null, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.SetTraits(TStack("Backline", 1))
  .CanBeHit(false)
  .CanShoveToOtherRow(true)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
              SStack("Oh a customer",1),
              SStack("Oh okay",100),
              SStack("Hehehe",50),
                SStack("FullImmuneToSnow",1),
       SStack("FullImmuneToInk",1),
        SStack("ELU",1000),
                  };

})
);
        cards.Add(
new CardDataBuilder(this).CreateUnit("AshiBoss", "Ashi Shi")
.SetSprites("Ashiboss.png", "Ashiboss BG.png")
.SetStats(10000, null, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("BossSmall")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
  .CanBeHit(false)

.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
      SStack("Die nowAshi",1),
       SStack("FullImmuneToSnow",1),
       SStack("FullImmuneToInk",1),
        SStack("ELU",1000),
       SStack("Another Friend",1),
      SStack("Ashi Boss Start",5),
      SStack("Gain Shi when ally killed",1),
      SStack("Respect XD",10),
      SStack("Respect XDX",1),
      SStack("Scrap",1000)

                  };


})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Ashiloser", "Ashi Shi")
.SetSprites("Ashi.png", "Ashi BG.png")
.SetStats(1000, null, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.SetTraits(TStack("Backline", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
              SStack("Kill allies while active",1),
                SStack("FullImmuneToSnow",1),
       SStack("FullImmuneToInk",1),
        SStack("ELU",1000),
        SStack("Winnerashitime",50),
        SStack("New Respect",1)


                  };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Ashifriendly", "Ashi Shi")
.SetSprites("Ashi.png", "Ashi BG.png")
.SetStats(30, 5, 2)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("Not a fan of tags huh? alright time to throw hands UwU")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                     {
SStack("FullImmuneToInk",1),
SStack("Free fortunetag",1)
                };
    data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Greed", 1),CreateTraitStack("TagR", 1)};

})
);



        cards.Add(
        new CardDataBuilder(this).CreateUnit("Magmi", "Magmi")
        .SetSprites("Magmi.png", "Magmi BG.png")
        .SetStats(46, 2, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
        .WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
        .WithFlavour("")
        .WithText("When killed, deploy the next ally.")
        .SetTraits(TStack("Barrage", 1))
        .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
        {
            data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
                      SStack("FullImmuneToInk",1),
                      SStack("MultiHit",1)


                        };

            data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
     SStack("Overload",3),

};

        })
        );

      
        cards.Add(
    new CardDataBuilder(this).CreateUnit("Fledgli", "Fledgli")
    .SetSprites("Fledgli.png", "Fledgli BG.png")
    .SetStats(38, 9, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
    .WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
    .WithFlavour("")
    .WithText("When killed, deploy the next ally.")
    .SetTraits(TStack("Aimless", 1))
    .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
    {
        data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
              {
                  SStack("FullImmuneToInk",1),
                  SStack("ImmuneToSnow",1)

                    };

        data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
     SStack("Demonize",1),

};
    })
    );

        cards.Add(
new CardDataBuilder(this).CreateUnit("Shellgi", "Shellgi")
.SetSprites("Shellgi.png", "Shellgi BG.png")
.SetStats(10, 1, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.WithText("When killed, deploy the next ally.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {

                  SStack("Increase Effect self when hit",2),
                  SStack("On Card Played Apply Shell To Allies",2),
                  SStack("Shell",25),

                };


})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Obbi", "Obbi")
.SetSprites("Obbi.png", "Obbi BG.png")
.SetStats(36, 15, 10)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.WithText("When killed, deploy the next ally.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
                  SStack("FullImmuneToInk",1),
                  SStack("When Hit Apply Snow To Attacker",4),


                };


})
);
        cards.Add(
new CardDataBuilder(this).CreateUnit("Friendzi", "Friendzi")
.SetSprites("FRIENDZI.png", "FRIENDZI BG.png")
.SetStats(45, 4, 2)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.WithText("When killed, deploy the next ally.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
                  SStack("While Active Frenzy To Allies",1),
                  SStack("Block",1)
                };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Koi", "Koi")
.SetSprites("KOI.png", "KOI BG.png")
.SetStats(21, null, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.WithText("When killed, deploy the next ally.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
            {
                  SStack("FullImmuneToInk",1),
                  SStack("Heavy Explsion Plus",2)


            };


})
);
        cards.Add(
new CardDataBuilder(this).CreateUnit("Nom Nom", "Nomi Nomi")
.SetSprites("NOM NOM.png", "NOM NOM BG.png")
.SetStats(28, 6, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.WithText("When killed, deploy the next ally.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {

                  SStack("MultiHit",1),
                  SStack("On Card Played Destroy Rightmost Card In Hand",1)


          };
})
);
        cards.Add(
new CardDataBuilder(this).CreateUnit("Bloodi", "Bloodi")
.SetSprites("BLOODI.png", "BLOODI BG.png")
.SetStats(18, 4, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.WithText("When killed, deploy the next ally.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {



       };

    data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Cunning", 1)};
})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Retali", "Retali")
.SetSprites("TALI.png", "TALI BG.png")
.SetStats(25, 4, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.WithText("When killed, deploy the next ally.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Bloodlust", 1)};
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Luni", "Luni")
.SetSprites("LUNI.png", "LUNI BG.png")
.SetStats(16, 0, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.WithText("When killed, deploy the next ally.")
.SetTraits(TStack("Barrage", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                     {
                  SStack("FullImmuneToInk",1)



     };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
     SStack("Increase Max Counter",2),

};
})
);
        //ASHI SHI POTIONS!---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //ASHI SHI POTIONS!---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //ASHI SHI POTIONS!---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        cards.Add(
new CardDataBuilder(this).CreateUnit("PotionFrenzy", "Potion of Frenzy")
.SetSprites("Pfrenzy.png", "Pfrenzy BG.png")
.SetStats(1000, null, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Enemy")
.SetTraits(TStack("Backline", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
        SStack("Scrap",50),
    SStack("While Active Frenzy To Allies",1),
     SStack("FullImmuneToSnow",1),
     SStack("FullImmuneToInk",1),
     SStack("Add PotionFrenzyI",1),
     SStack("ELU",1000)
   };
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("PotionFrenzyI", "Potion of Frenzy")
.SetSprites("Pfrenzy.png", "Pfrenzy BG.png")
.SetStats(null, null)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Clunker")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{

    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
   {
    SStack("Scrap",2),SStack("While Active Frenzy To AlliesInRow",1)
  };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
 {


 };
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("PRestrict", "Potion of Restriction")
.SetSprites("Restrict.png", "Restrict BG.png")
.SetStats(1000, null, 0)
.WithCardType("Enemy")
.SetTraits(TStack("Backline", 1))
.WithText("Restrict 4 spaces on your side of the field. <keyword=goobers.warning>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {

           SStack("When hit deployRes",1),
           SStack("When hit deployRes2",1),
           SStack("When hit deployRes3",1),
           SStack("When hit deployRes5",1),
            SStack("FullImmuneToSnow",1),
     SStack("FullImmuneToInk",1),
           SStack("Add PRestrict",1),
           SStack("Scrap",10000),
           SStack("ELU",1000)
    };
})
  );
        cards.Add(
 new CardDataBuilder(this).CreateUnit("Restrict", "Restricty")
 .SetSprites("Restrict Act.png", "Restrict BG.png")
 .SetStats(1000, null, 0)
 .WithCardType("Enemy")
 .SetTraits(TStack("Pigheaded", 1))
 .SubscribeToAfterAllBuildEvent(delegate (CardData data)

 {
     data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
   {
       SStack("Scrap",50),
        SStack("FullImmuneToInk",1),
        SStack("ELU",1000)
    };
 })
   );


        cards.Add(
    new CardDataBuilder(this).CreateItem("PRestrictI", "Potion of Restriction")
    .SetSprites("Restrict.png", "Restrict BG.png")
    .SetStats(null, null)
    .WithCardType("Item")
    .SetTraits(TStack("Consume", 1))
    .CanPlayOnBoard(true)
    .SubscribeToAfterAllBuildEvent(delegate (CardData data)

    {
        data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
            SStack("Summon RestrictI",1)
        };
        data.playOnSlot = true;
    })
      );
        cards.Add(
new CardDataBuilder(this).CreateUnit("RestrictI", "Restricty")
.SetSprites("Restrict Act.png", "Restrict BG.png")
.SetStats(23, null, 0)
.WithCardType("Summoned")
.SetTraits(TStack("Pigheaded", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
 {
        SStack("FullImmuneToInk",1),
  };
    data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Taunt", 1)};
})
 );

        cards.Add(
new CardDataBuilder(this).CreateUnit("PBlock", "Potion of Protection")
.SetSprites("Pblock.png", "Pblock BG.png")
.SetStats(1000, null, 4)
.WithCardType("Enemy")
.SetTraits(TStack("Backline", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
  {
     SStack("Scrap",1000),
     SStack("Block All",1),
     SStack("FullImmuneToSnow",1),
     SStack("FullImmuneToInk",1),
     SStack("Add PBlockII",1),
     SStack("ELU",1000)

   };
})
 );

        cards.Add(
new CardDataBuilder(this).CreateItem("PBlockI", "Potion of Protection")
.SetSprites("Pblock.png", "Pblock BG.png")
.SetStats(null, null)
.WithCardType("Item")
.CanPlayOnBoard(true)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
  {
   SStack("Uses",3)

   };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
  {
     SStack("Block",1)
    

  };
    data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Barrage", 1)};
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("PHealth", "Potion of Restoration")
.SetSprites("Phealth.png", "Phealth BG.png")
.SetStats(1000, null, 1)
.WithCardType("Enemy")
.SetTraits(TStack("Backline", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
 SStack("Scrap",1000),
     SStack("Healthy potion",1),
     SStack("Add PHealthI",1),
      SStack("FullImmuneToSnow",1),
     SStack("FullImmuneToInk",1),
     SStack("ELU",1000)

    };
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("PHealthI", "Potion of Restoration")
.SetSprites("Phealth.png", "Phealth BG.png")
.SetStats(null, null, 3)
.WithCardType("Clunker")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
     SStack("Scrap",2),
     SStack("Healthy potion",2),

    };
})
);
        cards.Add(
new CardDataBuilder(this).CreateUnit("PNull", "Potion of Toxins")
.SetSprites("Ppotion.png", "Ppotion BG.png")
.SetStats(1000, null, 4)
.WithCardType("Enemy")
.SetTraits(TStack("Backline", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
 SStack("Scrap",1000),
     SStack("Shroom Random",1),
     SStack("MultiHit",2),
     SStack("FullImmuneToSnow",1),
     SStack("FullImmuneToInk",1),
     SStack("Add PNullII",1),
     SStack("ELU",1000)
 };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("PNullI", "Potion of Toxins")
.SetSprites("Ppotion.png", "Ppotion BG.png")
.SetStats(null, null, 2)
.WithCardType("Clunker")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
    SStack("Scrap",2),
     SStack("Shroom Random",1),
     SStack("MultiHit",3)
    };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("PTimer", "Potion of Death")
.SetSprites("Ptimer.png", "Ptimer BG.png")
.SetStats(1000, null, 8)
.WithCardType("Enemy")
.SetTraits(TStack("Backline", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{   SStack("Scrap",1000),
     SStack("Kill all",1),
      SStack("FullImmuneToSnow",1),
   SStack("FullImmuneToInk",1),
   SStack("Add PTimerII",1),
     SStack("ELU",1000)
 };


})
);
        cards.Add(
new CardDataBuilder(this).CreateUnit("PTimerI", "Potion of Death")
.SetSprites("Ptimer.png", "Ptimer BG.png")
.SetStats(null, null, 8)
.WithCardType("Clunker")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {    SStack("Scrap",1),
     SStack("Kill all random",1),


    };


})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Ashi Shi's Vault", "Ashi Shi's Vault")
.SetSprites("VAULT.png", "VAULT BG.png")
.SetStats(null, null, 15)
.WithCardType("Enemy")
.SetTraits(TStack("Backline", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{   
    SStack("Scrap",25),  SStack("VAULT BREACHED",500), SStack("A Dollar for you",1), SStack("A Dollar for you2",1),SStack("On Turn Apply Scrap To Self",1000), SStack("ImmuneToSnow",1),
   SStack("FullImmuneToInk",1),

};


})
);
        cards.Add(
new CardDataBuilder(this).CreateItem("Dollars", "Ashi Shi Dollars")
.SetSprites("DOLLAR.png", "DOLLAR BG.png")
.SetStats(null, 0)
.WithCardType("Item")
.WithText("Useless cash in the Wildfrost world, but maybe it could be used to buy something from something that was not in this world.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.targetConstraints = new[]
   {

                        new TargetConstraintIsSpecificCard()
                        {
                           allowedCards = new CardData[] { TryGet<CardData>("VendingMachine") }

                        }
            };
    data.charmSlots = -10;
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
     SStack("Remove Self after use", 1)




};
})

);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Card Launcher", "Card Launcher")
.SetSprites("CARDLAUN.png", "CARDLAUN BG.png")
.SetStats(null, null, 2)
.WithCardType("Enemy")
.SetTraits(TStack("Backline", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    

 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
        SStack("Launch Card",1),
       SStack("Scrap",1000),
      SStack("FullImmuneToSnow",1),
   SStack("FullImmuneToInk",1),
     SStack("ELU",1000),
     SStack("Add Card Launcher",1),
     SStack("MultiHit",1)

};


})
);
        cards.Add(
new CardDataBuilder(this).CreateUnit("Card Reciever", "Card Launcher")
.SetSprites("CARDLAUN.png", "CARDLAUN BG.png")
.SetStats(null, null, 4)
.WithCardType("Clunker")
.SetTraits(TStack("Draw",3))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

    data.charmSlots = -10;
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
    
    SStack("Scrap",2)



};
})

);
        cards.Add(
new CardDataBuilder(this).CreateUnit("Get out", "Lucky You!")
.SetSprites("GETOUT.png", "GETOUT BG.png")
.SetStats(null, null, 0)
.WithFlavour("I will be sad if you pick this *wink wink* :D")
.WithCardType("Clunker")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{

 data.charmSlots = -10;
 data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{

    SStack("Scrap",1)



};
})

);



        cards.Add(
new CardDataBuilder(this).CreateUnit("Blunkytime", "Blunky's Totem")
.SetSprites("BLUNKYTOTEM.png", "BLUNKYTOTEM BG.png")
.SetStats(null, null, 0)
.WithCardType("Enemy")
.SetTraits(TStack("Backline", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{


  

    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
       {
           SStack("Add BlunkyCharm",1),
  SStack("Blunky aura",1),SStack("Blunky aura2",1),
       SStack("Scrap",1000),
      SStack("FullImmuneToSnow",1),
   SStack("FullImmuneToInk",1),
     SStack("ELU",1000),
     SStack("Blunky aura3",1)

   };


})
);


        

cards.Add(
new CardDataBuilder(this).CreateUnit("Blunkycharm", "Blunkycharm")
.SetSprites("BLUNKFRIDGE.png", "BLUNKFRIDGE BG.png")
.SetStats(null, null, 0)
.WithText("<keyword=goobers.charm>")
.WithCardType("Summoned")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.mainSprite.name = "Nothing";
    data.charmSlots = -10;
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
   {

    SStack("Scrap",1),
    SStack("When Deployed Apply Block To Self",2)



   };
})

);


        preLoaded = true;


    }
    private void CreateModAssetsCafeUnits()
    {

        //Candy Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy Units---------------------------------------------------------------------------------------------------------------------------------------------------------


        cards.Add(
    new CardDataBuilder(this).CreateUnit("BloodW", "Blood Witch")
     .SetSprites("BLOODW.png", "BLOODW BG.png")
     .SetStats(10, 5, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
     .WithCardType("Friendly")                                            //All summons are "Summoned". This line is necessary.
     .WithFlavour("Heh, coward.")
     .WithText("Card info: <card=goobers.Blood>")
     .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
     {
         data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
         {

             SStack("BloodSac applier",1),SStack("Hog to allies",1)
          };
         data.greetMessages = ["You seek for the forbidden ingredient? Be sure you're ready, it may cost you your companions."];

         data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Pigheaded", 1)};

         data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
        {
                   GiveUpgrade("CrownCursed")            //By our definition, no argument will give a crown
        };
     }



     )
           );


        cards.Add(
new CardDataBuilder(this).CreateItem("Blood", "Blood Vial")
.SetSprites("Inblood.png", "Inblood BG.png")
.SetStats(null, 0)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.CanPlayOnHand(true)
.CanPlayOnBoard(true)
.WithFlavour("Knife")
.WithText("<keyword=goobers.minorin>, <keyword=goobers.nuh>")
    .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
    {

        data.targetConstraints = new[]
  {

                        new TargetConstraintHasTrait()
                        {
                            trait = Get<TraitData>("AnchorMinorTarget")

                        }
            };
        data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
              {

                     SStack("Hit me",1),SStack("No Damage",100000)

              };

        data.charmSlots = -10;
        data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Retain", 1),CreateTraitStack("Zoomlin", 1),CreateTraitStack("Consume", 1)};
    })


            );



        //CAY KE RELATED
        cards.Add(
     new CardDataBuilder(this).CreateUnit("Cake", "Cay Ke")
      .SetSprites("Cay.png", "Cay BG.png")
      .SetStats(8, 3, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
      .WithCardType("Friendly")                                            //All summons are "Summoned". This line is necessary.
      .WithFlavour("But... I want to bake...")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
      {
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
              SStack("Gain Sweet Point Self",12),
              SStack("Add Sweetcake",1),
              SStack("reset",25),
              SStack("Straw Kay Act",1),
              SStack("Hyper Kay Act",1),
              SStack("Odd Kay Act",1),
              SStack("Blood Kay Act",1)


           };
          data.greetMessages = ["Oh! Here to try out some cakes? :D They'll take a while to make though... Let me tag along I'll make some soon!"];


          data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("AnchorMinorTarget",1)};

      })
            );

        cards.Add(
new CardDataBuilder(this).CreateItem("SweetCake", "Cake!")
.SetSprites("Sweetcake.png", "Sweetcake BG.png")
.SetStats(null, null)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.CanPlayOnHand(true)
.CanPlayOnBoard(true)
.WithFlavour("Knife")
.SetTraits(TStack("Noomlin", 1), TStack("Consume", 1))
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
  {
      data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
          SStack("EXP",15)

       };

  })
            );
        cards.Add(
  new CardDataBuilder(this).CreateUnit("Straw Kay", "Straw Kay")
   .SetSprites("Straw Cay.png", "Straw Cay BG.png")
   .SetStats(9, 3, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
   .WithCardType("Friendly")                                            //All summons are "Summoned". This line is necessary.
   .WithFlavour("Not a strawberry cake fan?")
   .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
   {
       data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
       {
              SStack("Gain Sweet Point Self",7),
              SStack("Add Berry Cake",1),
              SStack("reset",25)

        };
       data.greetMessages = ["Oh! Here to try out some cakes? :D They'll take a while to make though... Let me tag along I'll make some soon!"];
   })
         );
        cards.Add(
new CardDataBuilder(this).CreateItem("Berry Cake", "Strawberry Cake!")
.SetSprites("STRAWCAKE.png", "STRAWCAKE BG.png")
.SetStats(null, null)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.CanPlayOnHand(true)
.CanPlayOnBoard(true)
.WithFlavour("Knife")
.SetTraits(TStack("Noomlin", 1), TStack("Consume", 1))
 .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
 {
     data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
     {
          SStack("Cake",1),
          SStack("Increase Attack",3),
          SStack("Increase Max Health",6)

      };

 })
 );

        cards.Add(
new CardDataBuilder(this).CreateUnit("Hyper Kay", "Glazed Kay")
 .SetSprites("Sugar Cay.png", "Sugar Cay BG.png")
 .SetStats(3, 1, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
 .WithCardType("Friendly")                                            //All summons are "Summoned". This line is necessary.
 .WithFlavour("I wish I never came with you...")
 .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
 {
     data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
     {
         SStack("MultiHit",2),

              SStack("Gain Sweet Point Self",2),
              SStack("Add Sugar Cake",1),
              SStack("reset",30)

      };

     data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Notbothered", 1),CreateTraitStack("Aimless", 1)};

     data.greetMessages = ["Oh! Here to try out some cakes? :D They'll take a while to make though... Let me tag along I'll make some soon!"];
 })
       );

        cards.Add(
new CardDataBuilder(this).CreateItem("Sugar Cake", "Sugar Glazed Cake!")
.SetSprites("SUGARCAKE.png", "SUGARCAKE BG.png")
.SetStats(null, null)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.CanPlayOnHand(true)
.CanPlayOnBoard(true)
.WithFlavour("Knife")
.SetTraits(TStack("Noomlin", 1), TStack("Consume", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
          SStack("EXP",2),
          SStack("MultiHit",1),
          SStack("Choco",2)

     };



})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Odd Kay", "Bikay")
.SetSprites("bicay.png", "bicay BG.png")
.SetStats(5, 4, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("You dont like my cakes? :c")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
      SStack("Shell",4),
              SStack("Gain Sweet Point Self",5),
              SStack("Add Bizzare Cake",1),
              SStack("reset",20)

     };
    data.greetMessages = ["Oh! Here to try out some cakes? :D They'll take a while to make though... Let me tag along I'll make some soon!"];
})
    );

        cards.Add(
new CardDataBuilder(this).CreateItem("Bizzare Cake", "Bizzare Cake!")
.SetSprites("WEIRDCAKE.png", "WEIRDCAKE BG.png")
.SetStats(null, 0)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.CanPlayOnHand(true)
.CanPlayOnBoard(true)
.WithFlavour("Knife")
.SetTraits(TStack("Noomlin", 1), TStack("Consume", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{

    data.startWithEffects = new CardData.StatusEffectStacks[]
 {
         SStack("Random Buff",5)

  };

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Blood Kay", "Kaydy")
.SetSprites("Bloodcay.png", "Bloodcay BG.png")
.SetStats(8, 0, 8)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("c-cake..?")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
     {
              SStack("Get ritual cake",1)

   };
    data.greetMessages = ["Oh! Here to try out some cakes? :D They'll take a while to make though... Let me tag along I'll make some soon!"];
})
   );

        cards.Add(
new CardDataBuilder(this).CreateItem("Blood Cake", "Ritual Cake")
.SetSprites("RITUALCAKE.png", "RITUALCAKE BG.png")
.SetStats(null, 6)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.CanPlayOnHand(true)
.CanPlayOnBoard(true)
.WithFlavour("Knife")
.SetTraits(TStack("Noomlin", 1), TStack("Consume", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[]
  {
         SStack("Remove it",25)

  };

    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
          SStack("Null",1000)


 };
})
);
        cards.Add(
new CardDataBuilder(this).CreateUnit("Made", "Madeleine")
.SetSprites("Made.png", "Made BG.png")
.SetStats(6, 1, 2)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("If you wish, *picks up a gun*")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
              SStack("Gain Sweet Point Self",5),
              SStack("Gain maid when",1),
              SStack("reset",15),
              SStack("Berrymade Act",1),
              SStack("Sugarmade Act",1),
              SStack("Oddmade Act",1),
              SStack("Bloodmade Act",1),
  };

    data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("GrandMaid", 1),CreateTraitStack("AnchorMinorTarget",1)};



    data.greetMessages = ["ACHOO! Dang it was cold in there... Oh, yeah thanks for saving me. I'll tag along if you want."];
})
  );
        cards.Add(
new CardDataBuilder(this).CreateUnit("Madeberry", "Madeleinana")
.SetSprites("MadeBerry.png", "MadeBerry BG.png")
.SetStats(3, 1, 2)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("I uh... Okay master :/")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
              SStack("Maid equal HP",1),
              SStack("Heal self",1),
              SStack("Heal Equal2",1),
    };

    data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("GrandMaid", 2),CreateTraitStack("Heartburn", 1)};
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Madesugar", "Madelecara")
.SetSprites("MadeSugar.png", "MadeSugar BG.png")
.SetStats(5, 2, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour(">:C")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {
              SStack("MultiHit",2),
              SStack("Gain Sweet Point Self",3),
              SStack("When SP ally behind frenzy",1),
              SStack("reset",25)

    };

    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Maid", 2)};
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Madeodd", "Maddy Cannon")
.SetSprites("MadeOdd.png", "MadeOdd BG.png")
.SetStats(5, 6, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")
.WithFlavour("RAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH! GIVE ME BACK MY CANNON!")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                      {
              SStack("Scrap",2)


    };



    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("GrandMaid", 5),CreateTraitStack("Barrage", 1),};
})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Madeblood", "Malain")
.SetSprites("Bloodmade.png", "Bloodmade BG.png")
.SetStats(5, 5, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("I NEED BLOODSHED")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{

    data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Cunning", 1),CreateTraitStack("GrandMaid", 3)};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Vanillog", "Vanillog")
.SetSprites("Vanlog.png", "Vanlog BG.png")
.SetStats(10, 2, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("010010010010000001100100011100100110010101100001011011010110010101100100001000000110111101100110001000000111010001101000011010010111001100100000011001000110000101111001")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                     {
              SStack("MultiHit",2)


   };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
   {
          SStack("Snow",2)

    };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("BlindShot", 1)};

    data.greetMessages = ["01101100 01100101 01110100 00100000 01101101 01100101 00100000 01101000 01100101 01101100 01110000"];

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Chocolog", "Chocolog")
.SetSprites("Chocolog.png", "Chocolog BG.png")
.SetStats(7, 1, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("010010010010000001100011011000010110111000100000011000010110100101101101001000010010000001011001011010010111000001110000011010010110010100100001")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                     {
              SStack("MultiHit",3)


   };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
   {
          SStack("Demonize",1)

    };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("BlindShot", 1)};


    data.greetMessages = ["01001101 01110101 01110011 01110100 00100000 01100100 01100101 01110011 01110100 01110010 01101111 " +
        "01111001 00100000 01110100 01101000 01100101 00100000 01100110 01110010 01101111 01110011 01110100 00101100 00100000 01110100 01101111 " +
        "01101111 00100000 01100011 01101111 01101100 01100100 00101110"];

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Strawberilog", "Strawberilog")
.SetSprites("Strawlog.png", "Strawlog BG.png")
.SetStats(8, 2, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("Beep! beep! I'm a Strawberry!")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                         {
              SStack("MultiHit",1),
              SStack("On Hit Equal Heal To FrontAlly",1)


    };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
       {


    };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("BlindShot", 1)};

    data.greetMessages = ["*Beep boop noises*"];

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Peppifin", "Peppifin")
.SetSprites("Peppifin.png", "Peppifin BG.png")
.SetStats(6, 1, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")
.WithFlavour("Dude wtf, how am I gonna get to full strength now???")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
 {
          SStack("Perma soice cake",60),SStack("reset",60)


 };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {


 };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Cravings", 1),CreateTraitStack("Notbothered", 1)};


    data.greetMessages = ["Thanks for helping me out, lemme tag along. Unfortunetly I lost my flare, so idk if I'll be any good..."];

})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Peppiake", "Peppiake")
.SetSprites("Peppicake.png", "Peppicake BG.png")
.SetStats(6, 2, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("Please take this off me...")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
     {
              SStack("When ally is sp gain spice",1),
          


    };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
            {


    };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Cravings", 1),CreateTraitStack("Barrage", 1),CreateTraitStack("Notbothered", 1)};

})
);

     

        cards.Add(
new CardDataBuilder(this).CreateUnit("Mousso", "Mousso")
.SetSprites("Mousse.png", "Mousse BG.png")
.SetStats(10, 2, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("Glorbal??????????????")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
              SStack("When X Health Lost Split",3),
              SStack("sp reach gain health",5),
              SStack("reset",10)


 };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Pigheaded", 1)};

    data.greetMessages = ["Glerbal glorbal thounkou foroi sovong me! Glarbal glorbal com wit yoouz?? *glorbal noises*"];

})
);



        cards.Add(
new CardDataBuilder(this).CreateUnit("Oro", "Oro")
.SetSprites("Oro.png", "Oro BG.png")
.SetStats(13, 5, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("All that training for nothing...")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
          SStack("SP to ally behind",1),
          SStack("BerryOro Act",1),
          SStack("SugarOro Act",1),
          SStack("OddOro Act",1),
          SStack("BloodOro Act",1),
          SStack("EXP",3)



    };

    data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Smackback", 1),CreateTraitStack("AnchorMinorTarget",1)};


    data.greetMessages = ["Hey! why did you let me out I was training in there! ugh, whatever want me to come along?"];
})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Oroberry", "Raspboro")
.SetSprites("Rasporo.png", "Rasporo BG.png")
.SetStats(11, 5, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("This is... heavily unwanted...")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {

              SStack("When Ally Is Healed trigger self",1)
    };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Smackback", 1)};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Orosugary", "Choro")
.SetSprites("Choro.png", "Choro BG.png")
.SetStats(8, 4, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("I hate durians, get this off me!")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {

               SStack("SP to Allies when hit",1),
               SStack("EXP",10),
               SStack("Lose Sweet Point Self",100)
    };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Smackback", 1)};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Oroodd", "Oddoro")
.SetSprites("Oddoro.png", "Oddoro BG.png")
.SetStats(null, 4, 0)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("I-I'm stuck... You'll need to push me to the enemy.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
              {

              SStack("Scrap hitter",1),
               SStack("Scrap",3)
    };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Barrage", 1)};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Oroblood", "Veinval")
.SetSprites("ONIORO.png", "ONIORO BG.png")
.SetStats(10, 5, 6)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("Foolish!")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {
              SStack("Vein",1),
               SStack("Trigger When Ally Is Hit",1)
  };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Barrage", 1),CreateTraitStack("Smackback", 1)};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Velvet", "Velvet")
.SetSprites("Velvet.png", "Velvet BG.png")
.SetStats(7, 5, 8)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("Why are you taking my knifes? >:c")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                    {
   SStack("Trigger When Enemy Is Killed",1),
   SStack("Velvet Split Act",1)
    };

    data.greetMessages = ["I need new blades... Hopefully I can find one suitable if I follow you."];
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Red Velvet", "Red Velvet")
.SetSprites("Red Velvet.png", "Red Velvet BG.png")
.SetStats(7, 9, 8)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("What the f**k")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                    {
   SStack("Trigger When Enemy Is Killed",1),
   SStack("Red velvet aura",1)
    };


})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Cinaroll", "Cin Cin")
.SetSprites("Cincin.png", "Cincin BG.png")
.SetStats(7, 1, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("You took me along just to take my pillow?")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                    {
   SStack("On Hit Damage Damaged Target",7),
   SStack("Cinarollb Act",1),
   SStack("Cinarolls Act",1),
   SStack("Cinarollo Act",1),
   SStack("Cinarollbl Act",1)
    };

    data.greetMessages = ["I WAS SLEEPING WT- Oh I was frozen? Thanks for helping me out, can I come with? You'll need to carry my pillow."];

    data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("AnchorMinorTarget",1)};

})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("BerryCinaroll", "Bocin")
.SetSprites("CincinB.png", "CincinB BG.png")
.SetStats(4, 1, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("Wasteful >:|")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                          {
     SStack("Damage Equal To Health",1),
     SStack("On Turn Heal Allies",2),
     SStack("Sugaroro new",2),
     SStack("Gain max health :3",1)
    };

    data.traits = new List<CardData.TraitStacks>

          {
    };
})
);
       
        cards.Add(
new CardDataBuilder(this).CreateUnit("SugaryCinaroll", "Caracin")
.SetSprites("CaraCin.png", "CaraCin BG.png")
.SetStats(6, 1, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("Not a fan of shells?")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                             {
     SStack("Shell",1),
     SStack("Gain Sweet Point Self",3),
     SStack("Bonus Damage Equal To Shell",1),
     SStack("Pecan sweet",1),
     SStack("Pecan Effect",2),
     SStack("reset",9)
   };

    data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Frontline", 1)};

})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("OddCinaroll", "Cinerate")
.SetSprites("ODDCIN.png", "ODDCIN BG.png")
.SetStats(2, 0, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("But I need fuel...")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                 {
     SStack("While Active Consume To Items In Hand",1),
     SStack("When Card Destroyed, Gain Attack",3),
     SStack("Odd double",1),
     SStack("reset",20),
     SStack("Scrap",3)
    };



})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("BloodCinaroll", "Carnage")
.SetSprites("CARNAGE.png", "CARNAGE BG.png")
.SetStats(2, 2, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")
.WithFlavour("POJWPWJO-KWP-JW-K[PAKP[OQKPIONSJOMSO *NOISES*")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                    {
     SStack("Superfuel",1)
   };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                 {
      SStack("Bleeding", 1)
};


})
);



        cards.Add(
new CardDataBuilder(this).CreateUnit("Crepey", "Pandroppi")
.SetSprites("Crepey.png", "Crepey BG.png")
.SetStats(2, 2, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("Oh... not a fan of cards? :'c")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                     {
     SStack("Crepebe Act",1),
     SStack("Redraw Bell then SP",10)
     ,SStack("Drawn cards gain temp attack",1),
     SStack("Crepe now",1),
     SStack("reset",60),
     SStack("Crepes Act",1),
     SStack("Crepeo Act",1),
     SStack("Crepebl Act",1)
 };

    data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Maid", 1),CreateTraitStack("AnchorMinorTarget",1)};



    data.greetMessages = ["Hello hello! Pick a card any card! Oh wait I'm an option? Yippie! Pick me, pick me!!! :D."];

})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("CrepeyLV2", "Drapscone")
.SetSprites("Droppan.png", "Droppan BG.png")
.SetStats(5, 3, 4)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("But all the bells :c")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                         {
     SStack("Redraw Bell then SP",10)
     ,SStack("Drawn cards gain attack",1)
     ,SStack("draw more when sp",1)
     , SStack("reset",20)
    };
    data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Maid", 2)};

})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("Cherripan", "Cherripan")
.SetSprites("Panberry.png", "Panberry BG.png")
.SetStats(3, 7, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("uHH... fine okay then.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                         {
     SStack("Redraw Bell then SP",14)
     ,SStack("draw more health",1),
     SStack("When SP more draw2",1)
     , SStack("reset",30)
    };
    data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Maid", 1)};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("CrepeySugary", "Honibee")
.SetSprites("Honipan.png", "Honipan BG.png")
.SetStats(6, 10, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("You sicken me >:c THOSE CARDS WERE MINE")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                      {
     SStack("Redraw Bell then SP",15)
     ,SStack("Drawn cards gain Frenzy",1)
     ,SStack("sp frenzy drawe",2)
     , SStack("reset",30)

    };

    data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Maid", 1)};

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("CrepeyOdd", "Wafflecake")
.SetSprites("WafflePan.png", "WafflePan BG.png")
.SetStats(1, 3, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("WAFFLE HATER???")
.SetTraits(TStack("Barrage", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                          {
     SStack("Redraw Bell then SP",5)
     ,SStack("sp waffle drawe",1)
     ,SStack("Drawn cards gain Shekkll",1)
     , SStack("reset",10)
     ,SStack("Scrap",2)
    };
    data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Maid", 1)};

})
);


        cards.Add(
new CardDataBuilder(this).CreateUnit("CrepeyBlood", "Ambipantious")
.SetSprites("BloodPan.png", "BloodPan BG.png")
.SetStats(6, 3, 3)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.WithFlavour("Oh dear, did my teeth scare you?")
.SetTraits(TStack("Barrage", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                              {
     SStack("Redraw Bell then SP",8)
     ,SStack("sp demon drawe",3)
     ,SStack("Drawn cards gain teeth",1)
     , SStack("reset",10)
    };

    data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Maid", 3)};
})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Hypernova", "Hyper Nova")
.SetSprites("hnova.png", "hnova BG.png")
.SetStats(5, 5, 12)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                  {
     SStack("Block",2), SStack("Supernovatime",1),SStack("Supernovatime1",1),SStack("Hit All Enemies",1)

    };

    data.attackEffects = new CardData.StatusEffectStacks[] { };

    data.greetMessages = ["HOW DID YOU LOSE WITH THIS UNIT WITH YOU??? LMAO - From Sunny, the dev <3"];

})
);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Espressa", "Latteasha")
.SetSprites("TEASHA.png", "TEASHA BG.png")
.SetStats(7, 2, 5)                                                   //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Friendly")                                             //All summons are "Summoned". This line is necessary.
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                                                  {
     SStack("Expresso Service",1),SStack("While Active Zoomspresso",1),SStack("MultiHit",1)

    };

    data.attackEffects = new CardData.StatusEffectStacks[] { };
    

     data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Notbothered", 1),CreateTraitStack("Maid", 2)};

    data.greetMessages = ["Greetings, I can help you espresso yourself :D"];

})
);


        //Candy PETS Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy PETS Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy PETS Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy PETS Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy PETS Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy PETS Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy PETS Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy PETS Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy PETS Units---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy PETS Units---------------------------------------------------------------------------------------------------------------------------------------------------------

        cards.Add(
new CardDataBuilder(this).CreateItem("MMSN", "Monta Maid Service Number")
.SetStats(3, 3, 3)
.SetSprites("PHONE.png", "PHONE BG.png")                                             //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item")
.WithText("<keyword=goobers.unfrenzy>")
.CanBeHit(true)
.IsPet("", true)
.NeedsTarget(false)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
 {
     SStack("Random maid",1),SStack("Uses",4)

    };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Noomlin", 1)};
})
);

        cards.Add(
new CardDataBuilder(this).CreateItem("MMSN2", "*Clap Clap* Assistance Girls <3")
.SetStats(3, 3, 4)
.SetSprites("CLAPCLAP.png", "CLAPCLAP BG.png")                                             //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Item")
.WithText("<keyword=goobers.unfrenzy>")
.CanBeHit(true)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{

    

    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
     SStack("Random maid",1)

    };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Noomlin", 1)};
})
);

        cards.Add(
       new CardDataBuilder(this).CreateUnit("Blobery", "Blobery Shorcake")
       .SetStats(6, 3, 3)
       .SetSprites("BLUE.png", "BLUE BG.png")                                             //Shade Snake has 4 health, 3 attack, and no timer.
     .WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
       .WithFlavour("")
       .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
       {

           data.mainSprite.name = "Nothing";
           data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
            SStack("While Active Increase Attack To Allies",2),

           };
           data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Maid", 1)};
       })
       );

        cards.Add(
   new CardDataBuilder(this).CreateUnit("Strobery", "Strobery Shorcake")
   .SetStats(4, 3, 4)
   .SetSprites("STRAW.png", "STRAW BG.png")                                             //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned")                              //All summons are "Summoned". This line is necessary.
   .WithFlavour("")
   .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
   {
       data.mainSprite.name = "Nothing";
       data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
          SStack("When Ally is Hit Heal To Target",1)

       };
       data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Maid", 1)};
   })
   );
        cards.Add(
 new CardDataBuilder(this).CreateUnit("Chocobery", "Choco Shorcake")
 .SetStats(5, 4, 5)
 .SetSprites("CHOCO.png", "CHOCO BG.png")                                             //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned")                                      //All summons are "Summoned". This line is necessary.
 .WithFlavour("")
 .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
 {
     data.mainSprite.name = "Nothing";
     data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
        SStack("Increase Effects To FrontAlly",1)

     };
     data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Maid", 1)};
 })
 );
        cards.Add(
new CardDataBuilder(this).CreateUnit("Blabery", "Blabery Shorcake")
.SetStats(3, 2, 4)
.SetSprites("BLA.png", "BLA BG.png")                                             //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Summoned")                                        //All summons are "Summoned". This line is necessary.
.WithFlavour("")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
  SStack("While active decrease all enemies",2),


 };
    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Maid", 1)};
})
);


        //Candy Leaders---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy Leaders---------------------------------------------------------------------------------------------------------------------------------------------------------
        //Candy Leaders---------------------------------------------------------------------------------------------------------------------------------------------------------


       

        cards.Add(
        new CardDataBuilder(this).CreateUnit("Darkia", "Darkia")
        .SetSprites("DARKIA.png", "DARKIA BG.png")
        .SetStats(9, 2, 4)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
        .WithCardType("Leader")
        .WithFlavour("Hmm... Unsatisfactory...")
        .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

        {

            data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
            SStack("Bleeding",1)

        };
            data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
            {
                SStack("Gain spice bleed",5),
                SStack("Blood attacker",3),
                SStack("reset",15),
                SStack("Darkia comboa Act",1),
                SStack("Darkia combob Act",1),
                SStack("Darkia comboc Act",1),
                SStack("Darkia combod Act",1)
            };

            data.traits = new List<CardData.TraitStacks>

         {CreateTraitStack("Maid", 2),CreateTraitStack("AnchorMinorTarget",1)};


            data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
         {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
         };
        })

        );

        cards.Add(
        new CardDataBuilder(this).CreateUnit("Darkiaberry", "Nisha")
        .SetSprites("DARKIABERRY.png", "DARKIABERRY BG.png")
        .SetStats(13, 4, 3)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
        .WithCardType("Leader")
        .WithFlavour("Hmm... Unsatisfactory...")
        .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

        {

            data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
            SStack("Bleeding",1)

        };
            data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
            {
                SStack("Gain spice bleed",4),
                SStack("Blood Nightshade",1),
                SStack("reset",13)
            };

            data.traits = new List<CardData.TraitStacks>

         {CreateTraitStack("GrandMaid", 2),CreateTraitStack("Heartburn", 1) };
            

            data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
         {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
         };
        })

        );


        cards.Add(
      new CardDataBuilder(this).CreateUnit("Darkiasugary", "Coakia")
      .SetSprites("DARKIASUGAR.png", "DARKIASUGAR BG.png")
      .SetStats(8, 0, 3)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
      .WithCardType("Leader")
      .WithFlavour("Hmm... Unsatisfactory...")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

      {

          data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
            SStack("Bleeding",1)

      };
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {
              SStack("Gain spice bleed",6),
                SStack("Blood Expresso",1),
                SStack("MultiHit",3),
                SStack("reset",18)
          };

          data.traits = new List<CardData.TraitStacks>

       {CreateTraitStack("Maid", 2),CreateTraitStack("Aimless", 1)};


          data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
       {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
       };
      })

      );



        cards.Add(
  new CardDataBuilder(this).CreateUnit("Darkiaodd", "Sawcha")
  .SetSprites("DARKIAODD.png", "DARKIAODD BG.png")
  .SetStats(4, 4, 4)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
  .WithCardType("Leader")
  .WithFlavour("Hmm... Unsatisfactory...")
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

  {

      data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
  {
            SStack("Bleeding",1)

  };
      data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
                SStack("Gain spice bleed",9),
          SStack("Blood Spice",3),
                SStack("Scrap",3),
                SStack("reset",18)
      };

      data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Maid", 2),CreateTraitStack("Barrage", 1)};


      data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
   {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
   };
  })

  );


        cards.Add(
new CardDataBuilder(this).CreateUnit("Darkiablood", "Darkia LV2")
.SetSprites("DARKIABLO.png", "DARKIABLO BG.png")
.SetStats(10, 4, 3)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Leader")
.WithFlavour("Hmm... Unsatisfactory...")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{

data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
            SStack("Bleeding",2)

};
data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
{
    SStack("Darkia LV3 combo Act",1),
                    SStack("Gain spice bleed",8),
                SStack("Blood attacker all",3),
                SStack("Blood Triger",1),
                SStack("reset",20)

};

data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Maid", 2),CreateTraitStack("GrandMaid", 1)};


data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
{
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
};
})

);

        cards.Add(
new CardDataBuilder(this).CreateUnit("Darkiachalice", "Darkia LV3")
.SetSprites("Darkiacha.png", "Darkiacha BG.png")
.SetStats(17, 6, 3)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
.WithCardType("Leader")
.WithFlavour("Hmm... Unsatisfactory...")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{

  data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
            SStack("Bleeding",3)

};
  data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
               SStack("Gain spice bleed",8),
                SStack("Blood attacker all",4),
                SStack("Blood Triger",1),
                SStack("reset",20)

};

  data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Maid", 2),CreateTraitStack("GrandMaid", 2),CreateTraitStack("Bloodlust", 1)};


  data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
    {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
};
})

);




        cards.Add(
         new CardDataBuilder(this).CreateUnit("Ice Cream", "Stracciatella")
         .SetSprites("Icream.png", "Icream BG.png")
         .SetStats(9, 3, 3)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
         .WithCardType("Leader")
         .WithFlavour("Oh no ice cream? ok")
         .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

         {

             data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
         {

      (SStack("Snow",2))
         };
             data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
             {
                     SStack("Gain Snow",1),
                     SStack("Convert snow to Sweetpoints",1),
                     SStack("reset",6)
             };


             data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
          {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
          };
         })

         );

        cards.Add(
        new CardDataBuilder(this).CreateUnit("Tiramisu", "Tiramisu(Leader)")
        .SetSprites("Tiramisulead.png", "Tiramisulead BG.png")
        .SetStats(6, 4, 3)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
        .WithCardType("Leader")
        .WithFlavour("Coffee, I hope you're out there somewhere...")
        .WithText("<keyword=goobers.finding>")
        .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

        {
            data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
            {
                     SStack("Gain Sweet Point Self",8),
                     SStack("Trigger Ally when SP",24),
                     SStack("reset",24)

            };


            data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
         {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
         };
        })

        );

        cards.Add(
       new CardDataBuilder(this).CreateUnit("TiramisuA", "Tiramisu The Commander")
       .SetSprites("ATIRAMISU.png", "ATIRAMISU BG.png")
       .SetStats(8, 4, 3)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
       .WithCardType("Leader")
       .WithFlavour("Rest coffee let me handle this.")
       .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

       {
           data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
           {
                     SStack("When Deployed Summon Fixed Coffee",1),
                     SStack("Gain Sweet Point Self",6),
                     SStack("Trigger Ally when SP",12),
                     SStack("Commander",1),
                     SStack("reset",12)

           };


           data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
        {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
        };
       })

       );

        cards.Add(
    new CardDataBuilder(this).CreateUnit("Fixed Coffee", "Guardian Coffee")
    .SetSprites("FIXEDXOFFEE.png", "ATIRAMISU BG.png")
    .SetStats(18, 8, 5)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
    .WithCardType("Leader")
    .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

    {
        data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {


        };
        data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Await", 1)};

        data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
     {
                   GiveUpgrade(),                   //By our definition, no argument will give a crown
     };
    })

    );
        cards.Add(
  new CardDataBuilder(this).CreateUnit("Damaged Coffee", "Broken Guardian")
  .SetSprites("DAMcoffee.png", "DAMcoffee BG.png")
  .SetStats(null, null, 0)                                                  //Shade Snake has 4 health, 3 attack, and no timer.
  .WithCardType("Clunker")
  .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

  {
      data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
      {
          SStack("Scrap",2)

      };
      data.traits = new List<CardData.TraitStacks>

      { };


  })

  );



        cards.Add(
       new CardDataBuilder(this).CreateUnit("Mont", "Mont Blanc")
       .SetSprites("MONT.png", "MONT BG.png")
       .SetStats(9, 3, 4)
       .WithCardType("Leader")
       .WithFlavour("How will we be of service?")
       .SubscribeToAfterAllBuildEvent(delegate (CardData data)
       {
           data.startWithEffects = new CardData.StatusEffectStacks[]
           {
            SStack("MMSN Act",1),SStack("Gain Sweet Point Self",5), SStack("Apply Maid when",1), SStack("reset",20),SStack("Gain Expresso for every maid",1)
           };
           data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("GrandMaid", 1)};

           data.createScripts = new CardScript[]
        {
                   GiveUpgrade(),
        };
       })

       );

        cards.Add(
    new CardDataBuilder(this).CreateUnit("Banana", "Babamna")
    .SetSprites("Banana.png", "Banana BG.png")
    .SetStats(7, 1, 4)
    .WithCardType("Leader")
    .WithFlavour("Too doo doo!")
    .SetAttackEffect(SStack("Weakness", 2))
    .SubscribeToAfterAllBuildEvent(delegate (CardData data)
    {
        data.startWithEffects = new CardData.StatusEffectStacks[]
        {
                    SStack("Banana Split Act",1),
                    SStack("Gain SP when bom dies2",7),
                    SStack("MORE BOM",3),
                    SStack("reset",14),
                    SStack("MultiHit",1)

        };
        data.traits = new List<CardData.TraitStacks>


            {CreateTraitStack("Aimless", 1)};


        data.createScripts = new CardScript[]
     {
                   GiveUpgrade(),
     };
    })

    );

        cards.Add(
 new CardDataBuilder(this).CreateUnit("Bahanna Split", "Babamna Split")
 .SetSprites("Stacksplit.png", "Stacksplit BG.png")
 .SetStats(20, 8, 4)
 .WithCardType("Leader")
 .WithFlavour("Too doo doo!")
 .SetAttackEffect(SStack("Weakness", 3), SStack("Frost", 3), SStack("Snow", 2))
 .SubscribeToAfterAllBuildEvent(delegate (CardData data)
 {
     data.startWithEffects = new CardData.StatusEffectStacks[]
     {
                    SStack("When Enemyt Killed but nom",1),
                    SStack("Gain SP when bom dies",25),
                    SStack("MORE BOM2",10),
                    SStack("reset",50),
                    SStack("MultiHit",3)

     };
     data.traits = new List<CardData.TraitStacks>


         {CreateTraitStack("Aimless", 1)};


     data.createScripts = new CardScript[]
  {
                   GiveUpgrade(),
  };
 })

 );

        //STARTING CANDYINVENTORY----------------------------------------------------------------------------------------------------------------------------------------------
        //STARTING CANDYINVENTORY----------------------------------------------------------------------------------------------------------------------------------------------
        //STARTING CANDYINVENTORY----------------------------------------------------------------------------------------------------------------------------------------------
        //STARTING CANDYINVENTORY----------------------------------------------------------------------------------------------------------------------------------------------

        cards.Add(
new CardDataBuilder(this).CreateUnit("Bakey", "Babakery")
.SetSprites("Bake.png", "Terrimisu BG.png")
.SetStats(null, null, 3)
.WithCardType("Clunker")
.CanPlayOnHand(true)
.CanPlayOnFriendly(true)
.WithFlavour("Ba bop")

.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
             {

      (SStack("Scrap",2)),
      (SStack("Get Muf",1))
      };
}

));
        cards.Add(
new CardDataBuilder(this).CreateItem("Muf", "Muffin")
.SetSprites("Muf.png", "Bake BG.png")
.SetStats(null, null)
.WithCardType("Item")
.CanPlayOnHand(true)
.CanPlayOnFriendly(true)
.SetTraits(TStack("Consume", 1), TStack("Noomlin", 1))

.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
          {

      (SStack("EXP",6))
          };
}

));

        cards.Add(
 new CardDataBuilder(this).CreateItem("Bknife", "Butterknife")
 .SetSprites("Bknife.png", "Bknife BG.png")
 .SetStats(null, 2)
 .WithCardType("Item")
 .CanPlayOnFriendly(true)
 .WithFlavour("Knife")
 .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

 {


     data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
             {

                     SStack("JamKnife Act",1),
                     SStack("RushKnife Act",1),
                     SStack("OddKnife Act",1),
                     SStack("BloodKnife Act",1)
             };


     data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("AnchorMinorTarget",1)};

 }

));



        cards.Add(
new CardDataBuilder(this).CreateItem("JamKnife", "Jammy Knife")
.SetSprites("JamKnife.png", "JamKnife BG.png")
.SetStats(null, 4)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.WithFlavour("Cleaned from it's jam")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
       SStack("On Hit Equal Max Health To FrontAlly",1),SStack("Uses",2)

    };
}

));

        cards.Add(
new CardDataBuilder(this).CreateItem("RushKnife", "Sugar Rush Knife")
.SetSprites("Sugarknife.png", "Sugarknife BG.png")
.SetStats(null, 2)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.WithFlavour("KNIFE! KNIFE! KNIFE!")
.SetTraits(TStack("Aimless", 1))
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
       SStack("MultiHit",2),SStack("Gain Sweet Point front ally",2)

    };
}

));

        cards.Add(
new CardDataBuilder(this).CreateUnit("OddKnife", "Goofy Knife")
.SetSprites("SillyKnife.png", "SillyKnife BG.png")
.SetStats(null, 4, 2)
.WithCardType("Clunker")
.WithFlavour("Goofy ENHANCED!")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)
{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
       SStack("Scrap",2)
    };
}

));
        cards.Add(
new CardDataBuilder(this).CreateItem("BloodKnife", "Red Velvet Knife")
.SetSprites("BloodKnife.png", "BloodKnife BG.png")
.SetStats(null, 5)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.WithFlavour("not so bloody Knife")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
       SStack("Bleeding",3),

    };
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
        SStack("On Card Played Apply Attack To Self",5)
        };
}

));

        cards.Add(
new CardDataBuilder(this).CreateItem("Fuse", "Fusion Cookie")
.SetSprites("Fuse.png", "Fuse BG.png")
.SetStats(null, 0)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.CanPlayOnHand(true)
.CanPlayOnBoard(true)
.WithFlavour("fusion???")
.WithText("When used on a card with <keyword=goobers.anchor> and card requirements are met, activate a fusion.")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {

         SStack("Hit me",1)
    };
    data.traits = new List<CardData.TraitStacks>

    {CreateTraitStack("Retain", 1),CreateTraitStack("Zoomlin", 1),CreateTraitStack("Consume", 1)};
}

));




        cards.Add(
               new CardDataBuilder(this).CreateItem("Ice", "Ice Cream")
               .SetSprites("Icey.png", "Icey BG.png")
               .SetStats(null, 1)
               .WithCardType("Item")
               .CanPlayOnFriendly(true)
               .WithFlavour("Yum")
               .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

               {
                   data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                   {
                     SStack("Snow",2)

                   };

                   data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {

                     SStack("Gain Sweet Point front ally",2)

               };
               }

              ));

        cards.Add(
       new CardDataBuilder(this).CreateItem("Expresso", "Expresso")
       .SetSprites("Spresso.png", "Spresso BG.png")
       .SetStats(null, null)
       .WithCardType("Item")
       .CanPlayOnHand(true)
       .CanPlayOnFriendly(true)
       .WithFlavour("No expressing yourself anymore lmao")
       .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

       {
           data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
           {
                     SStack("Expresso",1)

           };
       }

      ));
        cards.Add(
       new CardDataBuilder(this).CreateItem("IPMinor", "Ingredient Pack Minor")
       .SetSprites("Ipack.png", "Ipack BG.png")
       .WithCardType("Item")
       .WithFlavour("Yum")
       .NeedsTarget(false)
       .SetTraits(TStack("Noomlin", 1), TStack("Consume", 1))
       .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

       {
           data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {

                     SStack("Random ingrediant",1)

                };

       }

      ));

        cards.Add(
new CardDataBuilder(this).CreateItem("Berry", "Berry Ingredient")
.SetSprites("Inberry.png", "Inberry BG.png")
.SetStats(null, 0)
.WithCardType("Item")
.CanPlayOnFriendly(true)
.CanPlayOnHand(true)
.CanPlayOnBoard(true)
.WithFlavour("Knife")
.WithText("<keyword=goobers.minorin>, <keyword=goobers.nuh>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)
{
    data.targetConstraints = new[]
  {

                        new TargetConstraintHasTrait()
                        {
                            trait = Get<TraitData>("AnchorMinorTarget")

                        }
            };
    data.charmSlots = -10;
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {

                     SStack("Hit me",1),SStack("No Damage",100000)

                };
    data.traits = new List<CardData.TraitStacks>

    {CreateTraitStack("Retain", 1),CreateTraitStack("Zoomlin", 1),CreateTraitStack("Consume", 1)};
}
));
        cards.Add(
      new CardDataBuilder(this).CreateItem("Sugary", "Sugary Ingredient")
      .SetSprites("Insugar.png", "Insugar BG.png")
      .SetStats(null, 0)
      .WithCardType("Item")
      .CanPlayOnFriendly(true)
      .CanPlayOnHand(true)
      .CanPlayOnBoard(true)
      .WithFlavour("Knife")
      .WithText("<keyword=goobers.minorin>, <keyword=goobers.nuh>")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)
      {
          data.targetConstraints = new[]
    {

                        new TargetConstraintHasTrait()
                        {
                            trait = Get<TraitData>("AnchorMinorTarget")

                        }
            };
          data.charmSlots = -10;
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {

                     SStack("Hit me",1),SStack("No Damage",100000)

                };
          data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Retain", 1),CreateTraitStack("Zoomlin", 1),CreateTraitStack("Consume", 1)};
      })

      );

        cards.Add(
      new CardDataBuilder(this).CreateItem("Odd", "Odd Ingredient")
      .SetSprites("Inodd.png", "Inodd BG.png")
      .SetStats(null, 0)
      .WithCardType("Item")
      .CanPlayOnFriendly(true)
      .CanPlayOnHand(true)
      .CanPlayOnBoard(true)
      .WithFlavour("Knife")
      .WithText("<keyword=goobers.minorin>, <keyword=goobers.nuh>")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)
      {
          data.targetConstraints = new[]
  {

                        new TargetConstraintHasTrait()
                        {
                            trait = Get<TraitData>("AnchorMinorTarget")

                        }
            };
          data.charmSlots = -10;
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {

                     SStack("Hit me",1),SStack("No Damage",100000)

                };
          data.traits = new List<CardData.TraitStacks>

   {CreateTraitStack("Retain", 1),CreateTraitStack("Zoomlin", 1),CreateTraitStack("Consume", 1)};
      })

      );


        cards.Add(
       new CardDataBuilder(this).CreateItem("Cof", "Cup of Coffee(CHUG CHUG)")
       .SetSprites("Cupofjoe.png", "Cupofjoe BG.png")
       .SetStats(null, null)
       .WithCardType("Item")
       .CanPlayOnFriendly(true)
       .WithFlavour("Yum")
       .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

       {
           data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
           {
                     SStack("Reduce Counter",3)
           };
       }

      ));


        cards.Add(
    new CardDataBuilder(this).CreateUnit("Blender", "Blendy")
    .SetSprites("Blender.png", "Blender BG.png")
    .SetStats(null, null, 0)
    .WithCardType("Clunker")
    .WithValue(85)
    .SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

    {
        data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
               {

                     SStack("MultiHit",1), SStack("SP TRIGGER",1),
                     SStack("Scrap",2),SStack("blender1 Act",1),SStack("blender2 Act",1),SStack("blender3 Act",1),SStack("blender4 Act",1)


               };
        data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {
                     SStack("Bleeding",1)
        };

        data.traits = new List<CardData.TraitStacks>

          {CreateTraitStack("Aimless", 1),CreateTraitStack("AnchorMinorTarget",1)};

    }

   ));

        cards.Add(
new CardDataBuilder(this).CreateItem("Galaxy", "Galaxy Smoothie")
.SetSprites("Blender1.png", "Blender1 BG.png")
.SetStats(null, null)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
           {




           };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
    {
                    SStack("Increase Attack",4), SStack("Increase Max Health",4)
    };

    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1)};
}

));
        cards.Add(
new CardDataBuilder(this).CreateItem("Diabetes", "Hyper Sugar Shake")
.SetSprites("Blender2.png", "Blender2 BG.png")
.SetStats(null, null)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
              {



       };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
       {
                    SStack("Choco",1), SStack("EXP",10)
   };

    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1)};
}

));

        cards.Add(
new CardDataBuilder(this).CreateItem("how", "Energy Drink")
.SetSprites("Blender3.png", "Blender3 BG.png")
.SetStats(null, null)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                  {




    };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
           {
                  SStack("Reduce Counter",5),
    };

    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1),CreateTraitStack("Zoomlin", 1)};
}

));

        cards.Add(
new CardDataBuilder(this).CreateItem("Chalice", "Ritual Liquid")
.SetSprites("Blender4.png", "Blender4 BG.png")
.SetStats(null, 2)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                    {


  };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
             {
                     SStack("Bleeding",7)
  };

    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Consume", 1)};
}

));


        cards.Add(
new CardDataBuilder(this).CreateItem("Cotton Candy", "Cotton Candy")
.SetSprites("COTTONCANDY.png", "COTTONCANDY BG.png")
.SetStats(null, 4)

.WithValue(75)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)        //New lines (replaces flavor text)

{
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                    {


  };
    data.attackEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
             {
                     SStack("Trigger (High Prio) for attack",1)
  };

    data.traits = new List<CardData.TraitStacks>

{CreateTraitStack("Aimless", 1)};
}

));

        cards.Add(
new CardDataBuilder(this).CreateItem("VIPSonly", "VIPS only!")
.SetSprites("VIP.png", "VIP BG.png")
.SetStats(null, null)

.NeedsTarget(false)
.WithValue(88)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[]
                {

SStack("Attack to all SP",5)
    };
    data.attackEffects = new CardData.StatusEffectStacks[]
         {

    };

    data.traits = new List<CardData.TraitStacks>

    {

    };
}

));

        cards.Add(
new CardDataBuilder(this).CreateItem("Expressoplus", "Expresso with SUGAR!")
.SetSprites("EXTREMO.png", "EXTREMO BG.png")
.SetStats(null, null)
 .CanPlayOnHand(true)
       .CanPlayOnFriendly(true)
.NeedsTarget(true)
.WithValue(90)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[]
                {


    };
    data.attackEffects = new CardData.StatusEffectStacks[]
         {
             SStack("Expresso",3),
             SStack("Weakness",2)
    };

    data.traits = new List<CardData.TraitStacks>

    {
        CreateTraitStack("Consume", 1)
    };
}

));





        cards.Add(
new CardDataBuilder(this).CreateItem("Ingredient Service", "Ingredient Delivery Phone")
.SetSprites("INGREDIENTCALL.png", "INGREDIENTCALL BG.png")
.SetStats(null, null)
.NeedsTarget(false)
.WithValue(40)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[]
                {
SStack("Finder",1),
SStack("Remove Self on played",1)

    };
    data.attackEffects = new CardData.StatusEffectStacks[]
         {
            
    };

    data.traits = new List<CardData.TraitStacks>

    {
        CreateTraitStack("Consume", 1)
    };
}

));







        cards.Add(
new CardDataBuilder(this).CreateUnit("BerryS", "Berry Ingredient")
.SetSprites("Inberry.png", "Inberry BG.png")
.SetStats(1,null, 0)
.WithCardType("Summoned")
.CanPlayOnFriendly(true)
.CanPlayOnHand(true)
.CanPlayOnBoard(true)
.WithFlavour("Knife")
.WithText("<keyword=goobers.minorin>")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)
{
    data.mainSprite.name = "Nothing";
    data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
        {

                     SStack("Berrydestroyed",1),
                     SStack("Destroy self when drawn",1)

        };
data.traits = new List<CardData.TraitStacks>

{};
}
));
        cards.Add(
      new CardDataBuilder(this).CreateUnit("SugaryS", "Sugary Ingredient")
      .SetSprites("Insugar.png", "Insugar BG.png")
.SetStats(1, null, 0)
      .WithCardType("Summoned")
      .CanPlayOnFriendly(true)
      .CanPlayOnHand(true)
      .CanPlayOnBoard(true)
      .WithFlavour("Knife")
      .WithText("<keyword=goobers.minorin>")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)
      {
          data.mainSprite.name = "Nothing";
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {

                     SStack("Sugarydestroyed",1),
                     SStack("Destroy self when drawn",1)

                };
          data.traits = new List<CardData.TraitStacks>

   {};
      })

      );

        cards.Add(
      new CardDataBuilder(this).CreateUnit("OddS", "Odd Ingredient")
      .SetSprites("Inodd.png", "Inodd BG.png")
.SetStats(1, null, 0)
      .WithCardType("Summoned")
      .CanPlayOnFriendly(true)
      .CanPlayOnHand(true)
      .CanPlayOnBoard(true)
      .WithFlavour("Knife")
      .WithText("<keyword=goobers.minorin>")
      .SubscribeToAfterAllBuildEvent(delegate (CardData data)
      {
          data.mainSprite.name = "Nothing";
          data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
                {

                     SStack("OddDestroyed2",1),
                     SStack("Destroy self when drawn",1)

                };
          data.traits = new List<CardData.TraitStacks>

          
   {};

          
      })

      );

        cards.Add(
   new CardDataBuilder(this).CreateUnit("BloodS", "Blood Vial")
   .SetSprites("Inblood.png", "Inblood BG.png")
.SetStats(1, null, 0)
   .WithCardType("Summoned")
   .CanPlayOnFriendly(true)
   .CanPlayOnHand(true)
   .CanPlayOnBoard(true)
   .WithFlavour("Knife")
   .WithText("<keyword=goobers.minorin>")
   .SubscribeToAfterAllBuildEvent(delegate (CardData data)
   {
       data.mainSprite.name = "Nothing";
       data.startWithEffects = new CardData.StatusEffectStacks[] //Manually set Shade Serpent's effects to the desired effect... when the time is right.
             {

                     SStack("BloodDestroyed2",1),
                     SStack("Destroy self when drawn",1)

             };
       data.traits = new List<CardData.TraitStacks>


       { };


   })

   );


        cards.Add(
new CardDataBuilder(this).CreateItem("Time out", "Time out guys :3")
.SetSprites("TIMEOUT.png", "TIMEOUT BG.png")
.SetStats(null, null)
.WithFlavour("No time out???")
.WithCardType("Item")
.NeedsTarget(false)
.CanPlayOnBoard(false)
.CanPlayOnHand(false)
.CanPlayOnFriendly(false)
.CanShoveToOtherRow(false)
.WithValue(150)
.AddPool("GeneralItemPool")
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
 data.startWithEffects = new CardData.StatusEffectStacks[]
             {
    SStack("Snow all",1), SStack("Halt Snow all",1),SStack("Decay",5)

 };
 data.attackEffects = new CardData.StatusEffectStacks[]
      {
        
 };

 data.traits = new List<CardData.TraitStacks>

 {
     CreateTraitStack("HardDiscard", 1)
 };
}

));

        cards.Add(
new CardDataBuilder(this).CreateItem("Razorknife", "Bloody Blade")
.SetSprites("BLOODYBLADE.png", "BLOODYBLADE BG.png")
.SetStats(null, 3)
.WithCardType("Item")
.WithValue(88)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[]
           {


};
data.attackEffects = new CardData.StatusEffectStacks[]
    {
         SStack("Bleeding",2)
};

data.traits = new List<CardData.TraitStacks>

{

};
}

));
        cards.Add(
new CardDataBuilder(this).CreateUnit("Wafflemaker", "Wawaffle")
.SetSprites("WAFMAKER.png", "WAFMAKER BG.png")
.SetStats(null, null, 6)
.WithCardType("Clunker")
.WithValue(88)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
    data.startWithEffects = new CardData.StatusEffectStacks[]
                   {
SStack("Waffle Service",1),SStack("Scrap",3)

    };
    data.attackEffects = new CardData.StatusEffectStacks[]
            {

    };

    data.traits = new List<CardData.TraitStacks>

    {

    };
}

));
        cards.Add(
new CardDataBuilder(this).CreateItem("Waffle", "Yummy Waffle")
.SetSprites("WAF.png", "WAF BG.png")
.SetStats(null, null)
.WithCardType("Item")
.WithValue(60)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[]
                   {


};
data.attackEffects = new CardData.StatusEffectStacks[]
            {
         SStack("Choco",2)
};

data.traits = new List<CardData.TraitStacks>

{
    CreateTraitStack("Consume", 1)
};
}

));

        cards.Add(
new CardDataBuilder(this).CreateItem("Wake up", "Wake up! NOW!")
.SetSprites("WAKE.png", "WAKE BG.png")
.SetStats(null, null)
.WithCardType("Item")
.WithValue(90)
.NeedsTarget(false)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[]
                       {
SStack("On Card Played Reduce Counter To Allies",2),SStack("Snow all enemiesw",1)

};
data.attackEffects = new CardData.StatusEffectStacks[]
                {
       
};

data.traits = new List<CardData.TraitStacks>

{
    CreateTraitStack("Consume", 1)
};
}

));

        cards.Add(
new CardDataBuilder(this).CreateItem("Double Muffin", "Heavily Sugared Brownie")
.SetSprites("Doubrownie.png", "Doubrownie BG.png")
.SetStats(null, null)
.WithCardType("Item")
.WithValue(50)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
   data.startWithEffects = new CardData.StatusEffectStacks[]
                           {

};
   data.attackEffects = new CardData.StatusEffectStacks[]
                    {
     SStack("EXP",2),SStack("Double EXP",1)
};

   data.traits = new List<CardData.TraitStacks>

{
    CreateTraitStack("Consume", 1)
};
}

));



        cards.Add(
new CardDataBuilder(this).CreateUnit("Cabinet", "Service Cabinet")
.SetSprites("CABINET.png", "CABINET BG.png")
.SetStats(null, null, 0)
.WithCardType("Clunker")
.WithValue(90)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[]
                            {
   SStack("Scrap",2),SStack("Maid Now cabinate",1)
};
data.attackEffects = new CardData.StatusEffectStacks[]
                     {

};

data.traits = new List<CardData.TraitStacks>

{
};
}

));



        cards.Add(
new CardDataBuilder(this).CreateItem("Strawberry Rush", "Blaster Straw")
.SetSprites("STRAWBLIND.png", "STRAWBLIND BG.png")
.SetStats(null, null)
.WithCardType("Item")
.NeedsTarget(false)
.WithValue(50)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
  data.startWithEffects = new CardData.StatusEffectStacks[]
                            {
SStack("MultiHit",2),SStack("Health increase random",2),SStack("Uses",2)
};
  data.attackEffects = new CardData.StatusEffectStacks[]
                     {
    
};

  data.traits = new List<CardData.TraitStacks>

{
  
};
}

));


        cards.Add(
new CardDataBuilder(this).CreateItem("Clown Mask", "Clown Mask")
.SetSprites("CLOWNMASK.png", "CLOWNMASK BG.png")
.SetStats(null, null)
.WithCardType("Item")
.CanPlayOnBoard(true)
.WithValue(50)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
data.startWithEffects = new CardData.StatusEffectStacks[]
                              {
SStack("Summon Cuppy",1)
};
data.attackEffects = new CardData.StatusEffectStacks[]
                       {
     
};

data.traits = new List<CardData.TraitStacks>

{
     CreateTraitStack("Consume", 1)
};


    data.playOnSlot = true;
}

));



        cards.Add(
new CardDataBuilder(this).CreateUnit("Cuppy", "Cuppy")
.SetSprites("CUPPY.png", "CUPPY BG.png")
.SetStats(5, null, 0)
.WithCardType("Summoned")
.WithValue(70)
.SubscribeToAfterAllBuildEvent(delegate (CardData data)

{
  data.startWithEffects = new CardData.StatusEffectStacks[]
                                {
   SStack("Summon self",1),SStack("Sac SP",16)
};
  data.attackEffects = new CardData.StatusEffectStacks[]
                         {

};

  data.traits = new List<CardData.TraitStacks>

  {
      
  };
}

));




        preLoaded = true;
    }





    private TargetConstraint[] TryGet<T>()
    {
        throw new NotImplementedException();
    }
    private void FinalFightEffectSwap()
    {
        BattleData finalBoss = TryGet<BattleData>("Final Boss");
        BattleGenerationScriptFinalBoss scriptFinalBoss = finalBoss.generationScript as BattleGenerationScriptFinalBoss;
        FinalBossGenerationSettings finalBossGenerationSettings = scriptFinalBoss.settings;
        finalBossGenerationSettings.cardModifiers = finalBossGenerationSettings.cardModifiers.AddRangeToArray(new FinalBossCardModifier[]
        {
            new FinalBossCardModifier()
            {
                card = TryGet<CardData>("Bucket"),//the card to modify (Stampley)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {TryGet<StatusEffectData>("Add Rift to Hand") }
                    },
                    new  CardScriptAddPassiveEffect()
                    {
                        effect = TryGet<StatusEffectData>("ARIFT NOW"),
                        countRange = new Vector2Int(1, 1)
                    }//CardScript that adds "Destroy the rightmost card" effect
                    
                
                }
            },
            new FinalBossCardModifier()
            {
                card = Get<CardData>("?"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Add Bone Needle to Hand") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
          
                    new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("MultiHit"),
                        countRange = new Vector2Int(1, 2)
                    },//CardScript that adds x1 frenzy
                    new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("On Hit Equal Teeth To Self"),
                        countRange = new Vector2Int(1, 1)
                    },//CardScript that adds "When hit, gain x1 frenzy" effect
                     new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Set Attack"),
                        countRange = new Vector2Int(2, 3)
                    },//CardScript that adds "When hit, gain x1 frenzy" effect
                    

                }
            },
             new FinalBossCardModifier()
            {
                card = Get<CardData>("Poochie"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("While Active Spark When Drawn To Allies In Hand") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                    new CardScriptAddPassiveEffect()
                    {

                        effect = Get<StatusEffectData>("Trigger All"),
                        countRange = new Vector2Int(1, 1)
                    },//CardScript that adds "When hit, gain x1 frenzy" effect
                    

                }
            },

             new FinalBossCardModifier()
            {
                card = Get<CardData>("M1"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("When Sacrificed Summon M2") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                    new CardScriptAddPassiveEffect()
                    {

                        effect = Get<StatusEffectData>("DIE"),
                        countRange = new Vector2Int(1, 1)
                    },//CardScript that adds "When hit, gain x1 frenzy" effect
                      new CardScriptAddPassiveEffect()
                    {

                        effect = Get<StatusEffectData>("AM2 NOW"),
                        countRange = new Vector2Int(1, 1)
                    },//CardScript that adds "When hit, gain x1 frenzy" effect
                      new CardScriptAddPassiveEffect()
                    {

                        effect = Get<StatusEffectData>("Destroy Self After Turn"),
                        countRange = new Vector2Int(1, 1)
                    },//CardScript that adds "When hit, gain x1 frenzy" effect
                    
                    

                }
            },


              new FinalBossCardModifier()
            {
                card = Get<CardData>("Yra"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Random YraBot") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                    new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Random AYraBot"),
                        countRange = new Vector2Int(1, 1)
                    },//CardScript that adds "When hit, gain x1 frenzy" effect
                     new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Set Max Health"),
                        countRange = new Vector2Int(21, 21)
                    },//CardScript that adds "When hit, gain x1 frenzy" effec
                      new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Set Health"),
                        countRange = new Vector2Int(21, 21)
                    },//CardScript that adds "When hit, gain x1 frenzy" effec
                     
                }
            },
                 new FinalBossCardModifier()
            {
                card = Get<CardData>("Tala"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Random Tala") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                      new CardScriptRemoveAttackEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Haze") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                     new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("On Card Played Damage To Self") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                    new CardScriptAddAttackEffect()
                    {
                        effect = Get<StatusEffectData>("Shroom"),
                        countRange = new Vector2Int(3, 3)
                    },//CardScript that adds "When hit, gain x1 frenzy" effect
                     new CardScriptAddAttackEffect()
                 {
                        effect = Get<StatusEffectData>("Frost"),
                        countRange = new Vector2Int(2, 2)
                    },//CardScript that adds "When hit, gain x1 frenzy" effect
                     new CardScriptAddAttackEffect()
                 {
                        effect = Get<StatusEffectData>("Haze"),
                        countRange = new Vector2Int(1, 1)
                    },//CardScript that adds "When hit, gain x1 frenzy" effect
                       new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Set Max Health"),
                        countRange = new Vector2Int(12, 13)
                    },//CardScript that adds "When hit, gain x1 frenzy" effec
                          new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Set Health"),
                        countRange = new Vector2Int(12, 13)
                    },//CardScript that adds "When hit, gain x1 frenzy" effec
                }
            },

                        new FinalBossCardModifier()
            {
                card = Get<CardData>("Terror"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Gain Sweet Point Self terror") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                      new CardScriptRemoveAttackEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Not Fast Enough") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
            
                     new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Shell") }
                    },


                       new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Set Max Health"),
                        countRange = new Vector2Int(70, 80)
                    },//CardScript that adds "When hit, gain x1 frenzy" effec
                  
                           new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Set Attack"),
                        countRange = new Vector2Int(6, 10)
                    },

                               new CardScriptAddTrait()
                    {
                        trait = Get<TraitData>("Backline"),
                        countRange = new Vector2Int(1, 1)
                    },//CardScript that adds "When hit, gain x1 frenzy" effec

                }
            },

                             new FinalBossCardModifier()
            {
                card = Get<CardData>("Cherry"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Add CTB to Hand") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
             
                     new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Add CTB to Hands") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                        
                       new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Random Bom On Turn"),
                        countRange = new Vector2Int(2, 4)
                    },//CardScript that adds "When hit, gain x1 frenzy" effec
                     
               new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Set Max Health"),
                        countRange = new Vector2Int(45, 60)
                    },//CardScript that adds "When hit, gain x1 frenzy" effec
                          new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Reduce Max Counter"),
                        countRange = new Vector2Int(4, 4)
                    },//CardScript that adds "When hit, gain x1 frenzy" effec
                }
            },
                                            new FinalBossCardModifier()
            {
                card = Get<CardData>("CB23O"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Perma into CB23K") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
             
                     new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Gain Sweet Point Self") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                        
              
                }
            },

          new FinalBossCardModifier()
            {
                card = Get<CardData>("CB23K"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Perma into CB23B") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
             
                     new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Gain Sweet Point Self") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                         new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Perma into CB23R") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                         
              
                }
            },
           new FinalBossCardModifier()
            {
                card = Get<CardData>("CB23B"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Perma into CB23KG") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
             
                     new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("When Ally Is Healed Apply Equal EXP") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
                      


                }
              },
           new FinalBossCardModifier()
            {
                card = Get<CardData>("CB23R"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Perma into CB23Q") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
             
                     new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("On Hit Apply Equal EXP ") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
              
                }
            },
           new FinalBossCardModifier()
            {
                card = Get<CardData>("CB23Q"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
                    new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Perma into CB23Q") }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies." effect
             
                    new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("Gain Sweet Point Self"),
                        countRange = new Vector2Int(5, 5)
                    },

                }
            },


                        new FinalBossCardModifier()
            {
                card = Get<CardData>("BloodW"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {
            
                  
                           new CardScriptAddAttackEffect()
                    {
                        effect = Get<StatusEffectData>("Bleeding"),
                        countRange = new Vector2Int(2, 3)
                    },
                                new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Hog to allies")
                       }
                    },
                        new CardScriptAddPassiveEffect()
                       {
                        effect = Get<StatusEffectData>("While Active Pigheaded To Enemies"),
                        countRange = new Vector2Int(1,1)
                    },

                               new CardScriptAddTrait()
                    {
                        trait = Get<TraitData>("Backline"),
                        countRange = new Vector2Int(1, 1)
                    },//CardScript that adds "When hit, gain x1 frenzy" effec

                }
            },
                new FinalBossCardModifier()
            {
                card = Get<CardData>("Cake"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {


                           new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Gain Sweet Point Self"), Get<StatusEffectData>("Add Sweetcake"), Get<StatusEffectData>("reset"),
                        Get<StatusEffectData>("Straw Kay Act"),Get<StatusEffectData>("Hyper Kay Act"),Get<StatusEffectData>("Odd Kay Act"),Get<StatusEffectData>("Blood Kay Act")}
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies."

                       new CardScriptAddPassiveEffect()
                    {
                        effect = Get<StatusEffectData>("On Turn Add Attack To Allies"),
                        countRange = new Vector2Int(2, 3)
                    },

                }
            },

                 new FinalBossCardModifier()
            {
                card = Get<CardData>("Straw Kay"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {


                           new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Gain Sweet Point Self"), Get<StatusEffectData>("Add Berry Cake"), Get<StatusEffectData>("reset"),
                       }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies."

                       new CardScriptAddPassiveEffect()
                       {
                        effect = Get<StatusEffectData>("On Turn Heal & Cleanse Allies"),
                        countRange = new Vector2Int(2, 3)
                    },

                }
            },

                   new FinalBossCardModifier()
            {
                card = Get<CardData>("Hyper Kay"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {


                           new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Gain Sweet Point Self"), Get<StatusEffectData>("Add Sugar Cake"), Get<StatusEffectData>("reset"),
                       }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies."

                       new CardScriptAddPassiveEffect()
                       {
                        effect = Get<StatusEffectData>("Expresso time for all"),
                        countRange = new Vector2Int(1,1)
                    },

                }
            },



                     new FinalBossCardModifier()
            {
                card = Get<CardData>("Odd Kay"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {


                           new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Gain Sweet Point Self"), Get<StatusEffectData>("Add Bizzare Cake"), Get<StatusEffectData>("reset"),
                       }
                    },//CardScript that removes "While active, gain frenzy equal to the number of Clunker allies."

                       new CardScriptAddPassiveEffect()
                       {
                        effect = Get<StatusEffectData>("Random Buff2"),
                        countRange = new Vector2Int(4,6)
                    },

                }
            },

                                new FinalBossCardModifier()
            {
                card = Get<CardData>("Blood Kay"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {


                           new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Get ritual cake")
                       }
                    },

                      
                           
                           //CardScript that removes "While active, gain frenzy equal to the number of Clunker allies."

                       new CardScriptAddAttackEffect()
                       {
                        effect = Get<StatusEffectData>("Bleeding"),
                        countRange = new Vector2Int(7,10)
                    },
                   

                }
            },
                  new FinalBossCardModifier()
            {
                card = Get<CardData>("Red Velvet"),//the card to modify (Gearboxer)
                runAll = new CardScript[]
                {


                           new CardScriptRemovePassiveEffect()
                    {
                        toRemove = new StatusEffectData[] {Get<StatusEffectData>("Red velvet aura")
                       }
                    },

                      
                           
                           //CardScript that removes "While active, gain frenzy equal to the number of Clunker allies."

                     new CardScriptAddAttackEffect()
                       {
                        effect = Get<StatusEffectData>("Red velvet aura2"),
                        countRange = new Vector2Int(1,1)
                    },


                }
            }



        });

    }




    //NOT BUILDERS--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
  
  
    RewardPool blank_pool = ScriptableObject.CreateInstance<RewardPool>();


    private void add_pools_extra(WildfrostMod mod)
    {

        add_pools();
    }

 

    private void add_pools()
    {
        foreach (ClassData tribe in AddressableLoader.GetGroup<ClassData>("ClassData"))
        {
            if (tribe.name != "goobers.Candy")
            if (tribe.name != "goobers.Draw")
                {

                bool contained = false;
                foreach (var pool in tribe.rewardPools)
                {
                    if (pool.name == "ThisIsAPool")
                    {
                        contained = true;
                    }
                }
                if (!contained)
                {
                    Debug.LogWarning("ADDEDPOOL");
                    tribe.rewardPools = tribe.rewardPools.AddItem(blank_pool).ToArray();
                }
            }


        }
        foreach (var tribe in References.Classes)
        {
            if (tribe.name != "goobers.Candy")
            if (tribe.name != "goobers.Draw")
                { 

            bool contained = false;
            foreach (var pool in tribe.rewardPools)
            {
                if (pool.name == "ThisIsAPool")
                {
                    contained = true;
                }
            }
            if (!contained)
            {
                tribe.rewardPools = tribe.rewardPools.AddItem(blank_pool).ToArray();
            }
          }

        }

    }
    private void CreateLocalizedStrings()
    {
        StringTable uiText = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English);
        uiText.SetString(TribeTitleKey, "The Goobers");                                       //Create the title
        uiText.SetString(TribeDescKey, "These folks came here for a nice vacation, but they found the land covered in snow and frost, " +
            "and so this clan was born from their intense desire to relax and take a break. " +
            "\n\n" +
            "This tribe ranges from local folk, to gods from whole other dimensions. " +
            "Sasha noted that some folks and luggage fell out of the plane on the way, and have probably froze lol.");


        uiText.SetString("Goobers.TutorACard", "Hello! What would you like to order?!");
        uiText.SetString("Vending Machine Noises", "*Vending Machine Noises* Beep Boop Please select your flavor!");


    }

    internal void ReunlockData() //Call this method after base.Load()
    {
        List<string> list = SaveSystem.LoadProgressData("completedChallenges", new List<string>()); //completedChallenges is never scrubbed
        List<string> list2 = SaveSystem.LoadProgressData("unlocked", new List<string>());

        AddressableLoader.GetGroup<ChallengeData>("ChallengeData")
            .Where(c => c?.reward?.name != null && c.ModAdded == this && list.Contains(c.name))
            .Do(c => list2.Add(c.reward.name));

        SaveSystem.SaveProgressData("unlocked", list2); //Be careful not to delete the player's entire unlock data!!!
    }

    public Task AddEnemyToBattle() 
    {
        foreach (CampaignNode node in References.Campaign.nodes.InRandomOrder())
        {
            if (node.type.isBattle && !node.type.isBoss && TryAddCard("goobers.Ashi", node))
            {
                break;
            }
        }
        return Task.CompletedTask;
    }

    

    [ConfigItem(true, null, "Custom Battles")]

    public bool toggleCustomBattles = true;

    public override void Load()
    {
        Events.OnSceneLoaded += SceneLoaded;
        Events.OnSceneLoaded += SceneLoaded2;
        Events.OnSceneLoaded += SceneLoaded3; 

        if (!preLoaded) { 
            
            CreateModAssets();
            AshiAssets();
            CreateModAssetsCafeUnits();


        }
        base.Load();
        {
            Events.OnMinibossIntro += VFXHelper.MinibossIntro;

            Events.OnCardDataCreated += RandomSprite;
            Events.OnCardDataCreated += RandomSprite2;

            Events.OnCampaignGenerated += AddEnemyToBattle;

            blank_pool = CreateRewardPool("ThisIsAPool", "Units", new DataFile[18] { Get<CardData>("Inkabom"), Get<CardData>("Sunray"), Get<CardData>("M1"),
            Get<CardData>("Hateu"), Get<CardData>("Theinfested"), Get<CardData>("Momo"), Get<CardData>("Yra"), Get<CardData>("Tala"),
            Get<CardData>("Raven"),Get<CardData>( "Luvu"), Get<CardData>("Soluna"), Get<CardData>("Sharoco"), Get<CardData>("Newtral"),
            Get<CardData>("Sunburn"), Get<CardData>("Sunscreen"), Get<CardData>("Vanillog"), Get<CardData>("Chocolog"),Get<CardData>("Strawberilog") }); Events.OnModLoaded += add_pools_extra;


            blank_pool = CreateRewardPool("ThisIsAPool1", "Items", new DataFile[1] { Get<CardData>("Inky Ritual Stone") }); Events.OnModLoaded += add_pools_extra;

            FinalFightEffectSwap();
            ReunlockData();

            CreateLocalizedStrings();

            InBattleShopSequence.DefineStrings(); 
            Events.OnEntityKilled += TootooKill;
            Events.OnEntityKilled += DefeatShopkeeper;


            this.Get<CardData>("Blue").traits = new List<CardData.TraitStacks>
                                                    {
                CreateTraitStack("Hypernova", 1)
                                                    };


            EnemyBattles.list = new List<(int tier, BattleDataEditor bdEditor)>();

            EnemyBattles.list.Add( // test code. delete
       (
       tier: 0,
       new BattleDataEditor(this, "Caramel", goldGivers: 0)
.SetSprite("caramel slime.png")
.SetNameRef("Caramel Slimes")
.EnemyDictionary(('C', "Slumo"), ('W', "Mugoo"),('V', "Caramoo"),('B', "Koogooloo"))
.StartWavePoolData(0, "Wave 1: Momo look")
.ConstructWaves(3, 0, "CCV", "VCC", "CVC")
.StartWavePoolData(1, "Wave 2: Mo Mo look its your son")
.ConstructWaves(2, 2, "W", "WC", "CW")
.StartWavePoolData(2, "Wave 3: Oh frick! MOMO")
.ConstructWaves(3, 9, "CCB", "BCC", "CBCC")
.AddBattleToLoader().LoadBattle(0, exclusivity: BattleEditor.BattleStack.Exclusivity.removeUnmodded)
.GiveMiniBossesCharms(new string[] { "Koogooloo" }, "CardUpgradeHeart", "CardUpgradeFrenzyReduceAttack")

));

            EnemyBattles.list.Add( // test code. delete
     (
     tier: 0,
     new BattleDataEditor(this, "Junkyard", goldGivers: 1)
.SetSprite("SCRAPYARDICON.png")
.SetNameRef("Junkyard(smh gnomes did not recycle)")
.EnemyDictionary(('C', "Jacky"), ('W', "Pricko"), ('V', "Broken Spike"), ('B', "Hippo Scra"))
.StartWavePoolData(0, "Wave 1: Momo look")
.ConstructWaves(3, 0, "CCW", "CWC", "WCC")
.StartWavePoolData(1, "Wave 2: Mo Mo look its your son")
.ConstructWaves(2, 2, "VV", "VW", "CW")
.StartWavePoolData(2, "Wave 3: Oh frick! MOMO")
.ConstructWaves(3, 9, "CCBCC", "CWBCC")
.AddBattleToLoader().LoadBattle(0, exclusivity: BattleEditor.BattleStack.Exclusivity.removeUnmodded)
.GiveMiniBossesCharms(new string[] { "Hippo Scra" }, "CardUpgradeFury", "CardUpgradeScrap")

));


            EnemyBattles.list.Add( // test code. delete
              (
              tier: 1,
              new BattleDataEditor(this, "HITABEAR", goldGivers: 1)
.SetSprite("Hitabear.png")
.SetNameRef("Hitabear Zone")
.EnemyDictionary(('C', "Do Bo"), ('W', "Bo Bo"), ('P', "Ro Ro"), ('B', "Mo Mo Senior"))
.StartWavePoolData(0, "Wave 1: Momo look")
.ConstructWaves(3, 0, "WCW", "WWC", "CWW")
.StartWavePoolData(1, "Wave 2: Mo Mo look its your son")
.ConstructWaves(3, 2, "PWC", "PWW", "WPC")
.StartWavePoolData(2, "Wave 3: Oh frick! MOMO")
.ConstructWaves(3, 9, "BC", "BW", "BCW")
.AddBattleToLoader().LoadBattle(1, exclusivity: BattleEditor.BattleStack.Exclusivity.removeUnmodded)
.GiveMiniBossesCharms(new string[] { "Mo Mo Senior" }, "CardUpgradeHeart", "CardUpgradeSnowball")

));


            EnemyBattles.list.Add( // test code. delete
              (
              tier: 1,
              new BattleDataEditor(this, "Robbery", goldGivers: 0)
.SetSprite("CHESTICON.png")
.SetNameRef("Abandoned Bling Factory")
.EnemyDictionary(('C', "Berryglury"), ('W', "Blingclops"), ('P', "Chest"), ('L', "FAKEChest"), ('B', "Bling bird"))
.StartWavePoolData(0, "Wave 1: Money")
.ConstructWaves(3, 0, "CCPP", "CWPP", "CWCP")
.StartWavePoolData(1, "Wave 2: Money Money")
.ConstructWaves(3, 2, "WLPL", "LPCL", "LPLWC")
.StartWavePoolData(2, "Wave 3: Money Money Money")
.ConstructWaves(3, 9, "BLP", "BLL", "LBP")
.AddBattleToLoader().LoadBattle(1, exclusivity: BattleEditor.BattleStack.Exclusivity.removeUnmodded)
.GiveMiniBossesCharms(new string[] { "Bling bird" }, "CardUpgradeAimless", "CardUpgradeAcorn")

));


            EnemyBattles.list.Add( // test code. delete
                           (
                           tier: 2,
                           new BattleDataEditor(this, "Executioner", goldGivers: 0)
           .SetSprite("Deityicon.png")
           .SetNameRef("4 Masked Deity")
           .EnemyDictionary(('C', "Judge"), ('W', "Baby Horns"), ('P', "Cursed"), ('K', "Mourn"), ('B', "Dorm"))
           .StartWavePoolData(0, "Wave 1: Hell 1")
           .ConstructWaves(3, 0, "WWPPB", "PPWWB")
           .StartWavePoolData(1, "Wave 2: Demons arrive!")
           .ConstructWaves(3, 2, "WW", "CW", "WP")
           .ToggleBattle(true)
           .AddBattleToLoader().LoadBattle(2, exclusivity: BattleEditor.BattleStack.Exclusivity.removeUnmodded)
           .GiveMiniBossesCharms(new string[] { "Dorm" }, "CardUpgradeHeart")

           ));


            EnemyBattles.list.Add( // test code. delete
                  (
                  tier: 3,
                  new BattleDataEditor(this, "Miner", goldGivers: 1)
  .SetSprite("Minericon.png")
  .SetNameRef("Frosted Mines")
  .EnemyDictionary(('C', "Mini Miner"), ('W', "Helmet Guy"), ('P', "Davey"), ('K', "Fros"),('B', "Greg"))
  .StartWavePoolData(0, "Wave 1: Spooky")
  .ConstructWaves(3, 0, "CCK", "CKC", "KKC","CKC")
  .StartWavePoolData(1, "Wave 2: Spooky2")
  .ConstructWaves(4, 8, "CPWK", "KPCW","CPWW")
  .StartWavePoolData(2, "Wave 3: AAAAAAAAAA")
  .ConstructWaves(3, 9, "CCB","CKB","KKB")
  .ToggleBattle(true)
  .AddBattleToLoader().LoadBattle(3, exclusivity: BattleEditor.BattleStack.Exclusivity.removeUnmodded)
  .GiveMiniBossesCharms(new string[] { "Greg" }, "CardUpgradeBoost", "CardUpgradeSun")

  ));

            EnemyBattles.list.Add( // test code. delete
                     (
                     tier: 4,
                     new BattleDataEditor(this, "FEAR", goldGivers: 1)
     .SetSprite("BattleFearicon.png")
     .SetNameRef("Glaring Eyes")
     .EnemyDictionary(('C', "Fear Dog"), ('W', "Fear Snake"), ('P', "Fear Spider"), ('K', "Fear Sprout"), ('F', "Fear Fish"), ('B', "Fear Deity"))
     .StartWavePoolData(0, "Wave 1: Spooky")
     .ConstructWaves(3, 0, "CCWK", "CWKF", "FCWK")
     .StartWavePoolData(1, "Wave 2: Spooky2")
     .ConstructWaves(4, 8, "PWCF", "PCFW")
     .StartWavePoolData(2, "Wave 3: AAAAAAAAAA")
     .ConstructWaves(4, 9, "CB", "BW", "KB", "BC")
     .ToggleBattle(true)
     .AddBattleToLoader().LoadBattle(4, exclusivity: BattleEditor.BattleStack.Exclusivity.removeUnmodded)
     .GiveMiniBossesCharms(new string[] { "Fear Deity" }, "CardUpgradeShellOnKill", "CardUpgradeSun")

     ));

        

            EnemyBattles.list.Add( // test code. delete
                       (
                       tier: 5,
                       new BattleDataEditor(this, "Chess", goldGivers: 0)
       .SetSprite("chessfight.png")
       .SetNameRef("Chess and Biscuits")
       .EnemyDictionary(('C', "Pawn"), ('W', "The Knight"), ('P', "The Rook"), ('K', "The Bishop"), ('B', "The Queen"))
       .StartWavePoolData(0, "Wave 1: Chessing starts")
       .ConstructWaves(3, 0, "CCB", "WCB")
       .StartWavePoolData(1, "Wave 2: Chessing :3")
       .ConstructWaves(4, 8, "WWCK", "PCPK")
       .StartWavePoolData(2, "Wave 3: CHessing so hard")
       .ConstructWaves(4, 9, "PCPK", "CCKK")
       .ToggleBattle(true)
       .AddBattleToLoader().LoadBattle(5, exclusivity: BattleEditor.BattleStack.Exclusivity.removeUnmodded)
       .GiveMiniBossesCharms(new string[] { "The Queen" }, "CharmCravings", "CharmSweettooth")

       ));

            EnemyBattles.list.Add( // test code. delete
                                (
                                tier: 6,
                                new BattleDataEditor(this, "Goopfight", goldGivers: 0)
                .SetSprite("BUGICON.png")
                .SetNameRef("Infested Frost Lands")
                .EnemyDictionary(('C', "Applier"), ('W', "Nest"), ('P', "Bomba Mother"), ('K', "Bomba Bug"), ('L', "Grr Bug"), ('B', "Grand Bug"))
                .StartWavePoolData(0, "Wave 1: Intro")
                .ConstructWaves(3, 0, "KKPCW", "LLPWC", "LKCPW")
                .StartWavePoolData(1, "Wave 2: BAZZZZ!!")
                .ConstructWaves(3, 9, "BLL", "BCL")
                .ToggleBattle(true)
                .AddBattleToLoader().LoadBattle(6, exclusivity: BattleEditor.BattleStack.Exclusivity.removeUnmodded)
                .GiveMiniBossesCharms(new string[] { "Grand Bug" }, "CardUpgradeFrosthand", "CardUpgradeBom", "CardUpgradeSnowball")

                ));

            foreach ((int tier, BattleDataEditor bdEditor) in EnemyBattles.list)
            {
         
                bdEditor.ToggleBattle(this.toggleCustomBattles);
            }

            // We have to unhook this in Unload
            ConfigManager.GetConfigSection(this).OnConfigChanged += ToggleBattlesOnConfigChanged;
        }


        GameMode gameMode = Get<GameMode>("GameModeNormal"); //GameModeNormal is the standard game mode. 
        gameMode.classes = gameMode.classes.Append(Get<ClassData>("Draw")).ToArray();//Actual loading      
        gameMode.classes = gameMode.classes.Append(Get<ClassData>("Candy")).ToArray();
        gameMode.classes = gameMode.classes.Append(Get<ClassData>("Witch")).ToArray();

  



        Events.OnEntityCreated += FixImage;


        FloatingText ftext = GameObject.FindObjectOfType<FloatingText>(true);
        ftext.textAsset.spriteAsset.fallbackSpriteAssets.Add(assetSprites);


    }


 

    void ToggleBattlesOnConfigChanged(ConfigItem item, object value)
    {
        foreach ((int tier, BattleDataEditor bdEditor) in EnemyBattles.list)
        {
            if (bdEditor == null || bdEditor.bd == null)
            {
                Debug.Log($"[{Title}] Custom battle is null. Ignoring...");
                continue;
            }

            if (item.fieldName == nameof(this.toggleCustomBattles) && value is bool on)
            {
                bdEditor.ToggleBattle(this.toggleCustomBattles);
            }
        }
    }

    void ToggleBattle(BattleData bd, int tier, bool on = true)
    {
        GameMode gameMode = Get<GameMode>("GameModeNormal");
        if (bd == null)
            // If BD is null, we should panic
            throw new NullReferenceException("Toggle Battle error: BD is null somehow! Did you put this in the wrong place?");

        // This will delete the battle if config is off
        if (!on)
        {
            gameMode.populator.tiers[tier].battlePool = gameMode.populator.tiers[tier].battlePool.Where(battle => battle != bd).ToArray();
            Debug.Log($"[{Title}] {bd.name} has been removed from tier {tier}");
        }
        else
        {
            gameMode.populator.tiers[tier].battlePool = gameMode.populator.tiers[tier].battlePool.With(bd);
            Debug.Log($"[{Title}] {bd.name} has been added to tier {tier}");
        }



    }




    private bool TryAddCard(string name, CampaignNode node)
    {
        if (node.data.TryGetValue("waves", out object value))
        {
            BattleWaveManager.WaveData[] waves = ((SaveCollection<BattleWaveManager.WaveData>)value).collection;
            for (int j = 0; j < waves.Length; j++)
            {
                if (waves[j].Count < 6)
                {
                    waves[j].AddCard(Goobers.Instance.TryGet<CardData>(name));
                    return true;
                }
            }
            if (waves.Length > 0)
            {
                waves[0].AddCard(Goobers.Instance.TryGet<CardData>(name));
                return true;
            }
        }
        return false;
    }


  


    public class StatusEffectInstantFillXBoardSlots : StatusEffectInstant
    {
        public bool random = false;

        public bool clearBoardFirst = false;

        public CardData[] withCards;

        public readonly List<CardData> pool = new List<CardData>();

        public override IEnumerator Process()
        {
            List<CardContainer> rows = References.Battle.GetRows(target.owner);
            if (clearBoardFirst)
            {
                foreach (CardContainer cardContainer in rows)
                {
                    CardSlotLane cardSlotLane2 = cardContainer as CardSlotLane;
                    if (!(cardSlotLane2 != null))
                    {
                        continue;
                    }

                    foreach (CardSlot slot3 in cardSlotLane2.slots)
                    {
                        if (slot3.Empty || !slot3.entities.All((Entity e) => e.name != target.name))
                        {
                            continue;
                        }

                        List<Entity> toKill = new List<Entity>(slot3.entities);
                        foreach (Entity e2 in toKill)
                        {
                            yield return e2.Kill(DeathType.Sacrifice);
                        }
                    }
                }
            }

            List<CardSlot> list = new List<CardSlot>();
            List<CardSlot> list2 = new List<CardSlot>();
            List<CardSlot> list3 = new List<CardSlot>();
            int amount = GetAmount();
            int i = 0;
            if (!random)
            {
                CardSlotLane cardSlotLane3 = rows[0] as CardSlotLane;
                if (cardSlotLane3 != null)
                {
                    list2 = cardSlotLane3.slots.ToArray().RemoveFromArray(item => item.Empty).ToList();
                }
                CardSlotLane cardSlotLane4 = rows[1] as CardSlotLane;
                if (cardSlotLane4 != null)
                {
                    list3 = cardSlotLane4.slots.ToArray().RemoveFromArray(item => item.Empty).ToList();
                }
                for (int i2 = 0; i2 < amount; i2++)
                {
                    if (list2.Count() > 0 && list3.Count() > 0)
                    {
                        if (Dead.Random.Range(1, 2) == 1)
                        {
                            list.Add(list2[0]);
                            list2.RemoveAt(0);
                        }
                        else
                        {
                            list.Add(list3[0]);
                            list3.RemoveAt(0);
                        }
                    }
                    else
                    {
                        if (list2.Count() > 0)
                        {
                            list.Add(list2[0]);
                            list2.RemoveAt(0);
                        }
                        else
                        {
                            if (list3.Count() > 0)
                            {
                                list.Add(list3[0]);
                                list3.RemoveAt(0);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                while (i < amount && rows.SelectMany((CardContainer row) => (row as CardSlotLane).slots.Where((CardSlot slot) => slot.Empty && !list.Contains(slot))).Any())
                {
                    CardContainer[] containers = rows.ToArray();
                    CardSlotLane cardSlotLane = containers.RandomItem() as CardSlotLane;
                    if (cardSlotLane != null)
                    {
                        CardSlot slot2 = cardSlotLane.slots.RandomItem();
                        if (slot2.Empty && i < amount && !list.Contains(slot2))
                        {
                            list.Add(slot2);
                            i++;
                        }
                    }
                }
            }

            foreach (CardSlot slot5 in list)
            {
                CardData data = Pull().Clone();
                Card card = CardManager.Get(data, References.Battle.playerCardController, target.owner, inPlay: true, target.owner.team == References.Player.team);
                yield return card.UpdateData();
                CardDiscoverSystem.instance.DiscoverCard(data);
                target.owner.reserveContainer.Add(card.entity);
                target.owner.reserveContainer.SetChildPosition(card.entity);
                ActionQueue.Stack(new ActionMove(card.entity, slot5), fixedPosition: true);
                ActionQueue.Stack(new ActionRunEnableEvent(card.entity), fixedPosition: true);
                foreach (var status in target.statusEffects)
                {
                    if (status is StatusEffectApplyToSummon && !target.silenced)
                    {
                        int statusAmount = status.count;
                        if (status.canBeBoosted) { statusAmount = (int)((statusAmount + target.effectBonus) * (target.effectFactor)); }
                        ActionQueue.Stack(new ActionApplyStatus(card.entity, target, (status as StatusEffectApplyToSummon).effectToApply, statusAmount), fixedPosition: true);
                    }
                }
            }

            list = null;
            yield return base.Process();
        }

        public CardData Pull()
        {
            if (pool.Count <= 0)
            {
                pool.AddRange(withCards);
            }

            return pool.TakeRandom();
        }
    }

 




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
    public override void Unload()
    {

        Events.OnModLoaded -= add_pools_extra;

        Events.OnSceneLoaded -= SceneLoaded;



        ConfigManager.GetConfigSection(this).OnConfigChanged -= ToggleBattlesOnConfigChanged;
        base.Unload();

        Events.OnCardDataCreated -= RandomSprite;
        Events.OnCardDataCreated -= RandomSprite2;

        Events.OnEntityKilled -= TootooKill;
        Events.OnEntityKilled -= DefeatShopkeeper;
        Events.OnCampaignGenerated -= AddEnemyToBattle;

        this.Get<CardData>("Blue").traits = new List<CardData.TraitStacks>
                                                    {
                CreateTraitStack("Hypernova", 1)
                                                    };
        Events.OnMinibossIntro -= VFXHelper.MinibossIntro;


        GameMode gameMode = Get<GameMode>("GameModeNormal");
        gameMode.classes = RemoveNulls(gameMode.classes); //Without this, a non-restarted game would crash on tribe selection


    }


    public override List<T> AddAssets<T, Y>()           //This method is called 6-7 times in base.Load() for each Builder. Can you name them all?
    {


        var typeName = typeof(Y).Name;
        switch (typeName)                                //Checks what the current builder is
        {
            case nameof(CardData):
                return cards.Cast<T>().ToList();
            case nameof(CardUpgradeData):
                return cardUpgrades.Cast<T>().ToList();
            case nameof(KeywordData):
                return keywords.Cast<T>().ToList();
            case nameof(StatusEffectData):
                return statusEffects.Cast<T>().ToList();
            case nameof(TraitData):
                return traitEffects.Cast<T>().ToList();
            case nameof(ClassData):                       //To avoid confusion with C# classes, the word "tribe" will be used to talk about an instance of ClassData.
                return tribes.Cast<T>().ToList();
            case nameof(UnlockData):                       //To avoid confusion with C# classes, the word "tribe" will be used to talk about an instance of ClassData.
                return unlocklist.Cast<T>().ToList();//Loads our status effects
            case nameof(ChallengeData):                       //To avoid confusion with C# classes, the word "tribe" will be used to talk about an instance of ClassData.
                return challenges.Cast<T>().ToList();
            case nameof(ChallengeListener):                       //To avoid confusion with C# classes, the word "tribe" will be used to talk about an instance of ClassData.
                return listenerlist.Cast<T>().ToList();
            case nameof(GameModifierData):                       //To avoid confusion with C# classes, the word "tribe" will be used to talk about an instance of ClassData.
                return modifier.Cast<T>().ToList();
         



            default:
                return null;
        }

    }




    public T TryGet<T>(string name) where T : DataFile
    {
        T data;
        if (typeof(StatusEffectData).IsAssignableFrom(typeof(T)))
            data = base.Get<StatusEffectData>(name) as T;
        else
            data = base.Get<T>(name);

        if (data == null)
            throw new Exception($"TryGet Error: Could not find a [{typeof(T).Name}] with the name [{name}] or [{Extensions.PrefixGUID(name, this)}]");

        return data;
    }
    private void SceneLoaded(Scene scene)
    {
        if (scene.name != "Campaign")
            return;
        CombineCardSystem.Combo combo = new CombineCardSystem.Combo
        {
            cardNames = new string[3] { "goobers.Sunray", "goobers.Sunburn", "goobers.Sunscreen" },
            resultingCardName = "goobers.Solar"
        };

        CombineCardSystem.Combo combo2 = new CombineCardSystem.Combo
        {
            cardNames = new string[2] { "goobers.Fixer", "goobers.Punchy" },
            resultingCardName = "goobers.Punchyfixed"
        };

        CombineCardSystem combineCardSystem = GameObject.FindObjectOfType<CombineCardSystem>(true);
        combineCardSystem.enabled = true;
        combineCardSystem.combos = combineCardSystem.combos.AddItem(combo).ToArray();
    }
    private void SceneLoaded2(Scene scene)
    {
        if (scene.name != "Campaign")
            return;

        CombineCardSystem.Combo combo = new CombineCardSystem.Combo
        {
            cardNames = new string[2] { "goobers.Fixer", "goobers.Punchy" },
            resultingCardName = "goobers.Punchyfixed"
        };

        CombineCardSystem combineCardSystem = GameObject.FindObjectOfType<CombineCardSystem>(true);
        combineCardSystem.enabled = true;
        combineCardSystem.combos = combineCardSystem.combos.AddItem(combo).ToArray();
    }

    private void SceneLoaded3(Scene scene)
    {
        if (scene.name != "Campaign")
            return;

        CombineCardSystem.Combo combo = new CombineCardSystem.Combo
        {
            cardNames = new string[2] { "goobers.Damaged Coffee", "goobers.Tiramisu" },
            resultingCardName = "goobers.TiramisuA"
        };

        CombineCardSystem combineCardSystem = GameObject.FindObjectOfType<CombineCardSystem>(true);
        combineCardSystem.enabled = true;
        combineCardSystem.combos = combineCardSystem.combos.AddItem(combo).ToArray();
    }


    public void TootooKill(Entity entity, DeathType deathType)
    {
        if (entity?.lastHit?.attacker?.name == "SunburstDart")
        {
            StatsSystem.instance.Stats.Add("Sunburst Tootoo Killss", 1);
        }
    }
    public void DefeatShopkeeper(Entity entity, DeathType deathType)
    {
        if (entity?.data?.name == "goobers.AshiBoss")
        {
            StatsSystem.instance.Stats.Add("Defeat the Ashi Boss", 1);
        }
    }


    public class StatusEffectSafeTempTrait : StatusEffectTemporaryTrait
    {
        public int queued = 0;
        public int finished = 0;
        public override IEnumerator StackRoutine(int stacks)
        {
            int current = queued;
            queued++;
            yield return new WaitUntil(() => finished == current);
            yield return base.StackRoutine(stacks);
            finished++;
        }
    }





    public class TargetModeAllAllies : TargetMode
    {
        [SerializeField]
        public TargetConstraint[] constraints;

        public override bool NeedsTarget => false;

        public override Entity[] GetPotentialTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            hashSet.AddRange(from e in entity.GetAllAllies()
                             where (bool)e && e.enabled && e.alive && e.canBeHit && CheckConstraints(e)
                             select e);
            if (hashSet.Count <= 0)
            {
                return null;
            }

            return hashSet.ToArray();
        }

        public override Entity[] GetSubsequentTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            hashSet.AddRange(Battle.GetCardsOnBoard(target.owner));
            hashSet.Remove(entity);
            if (hashSet.Count <= 0)
            {
                return null;
            }

            return hashSet.ToArray();
        }

        public bool CheckConstraints(Entity target)
        {
            TargetConstraint[] array = constraints;
            if (array != null && array.Length > 0)
            {
                return constraints.All((TargetConstraint c) => c.Check(target));
            }

            return true;
        }
    }
    public class StatusEffectTemporaryNegativeTrait : StatusEffectTemporaryTrait
    {
        public bool destroyCardOnEmpty = false;

        public override IEnumerator StackRoutine(int stacks)
        {
            added = target.GainTrait(trait, -stacks, temporary: true);
            yield return target.UpdateTraits();
            addedAmount -= stacks;
            target.display.promptUpdateDescription = true;
            target.PromptUpdate();

            yield return CheckEmpty();
        }

        public override IEnumerator EndRoutine()
        {
            yield return base.EndRoutine();

            yield return CheckEmpty();
        }


        public IEnumerator CheckEmpty()
        {
            if (!target.traits.Any(t => t.count > 0 && t.data.name == trait.name))
            {
                if (destroyCardOnEmpty)
                {
                    yield return target.Kill(DeathType.Consume);
                }
            }
        }
    }



    public class StatusEffectMultiConsume : StatusEffectData
    {
        private void OnDestroy()
        {
            Events.OnActionPerform -= CheckAction;
        }

        public override void Init()
        {
            Events.OnActionPerform += CheckAction;
        }

        public override IEnumerator RemoveStacks(int amount, bool removeTemporary)
        {
            count -= amount;
            if (removeTemporary)
                temporary -= amount;
            if (count <= 0)
            {
                yield return Remove();
                yield return new ActionConsume(target).Run();
            }

            target.PromptUpdate();
        }

        private void CheckAction(PlayAction action)
        {
            if (action is not ActionReduceUses actionReduceUses)
                return;

            if (actionReduceUses.entity != target)
                return;

            ActionQueue.Stack(
                new ActionSequence(RemoveStacks(1, false)) { note = "MultiConsume count down" }
            );
        }
    }

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

    public class StatusEffectInstantTutor : StatusEffectInstant
    {
        public enum CardSource
        {
            Draw,
            Discard,
            Custom // Use Summon Copy
        }

        public CardSource source = CardSource.Draw;
        public string[] customCardList;
        public int amount;
        public StatusEffectInstantSummon summonCopy;
        public CardData.StatusEffectStacks[] addEffectStacks;
        public LocalizedString title;

        private CardContainer _cardContainer;
        private GameObject _gameObject;
        private GameObject _objectGroup;

        private Entity _selected;
        private CardPocketSequence _sequence;
        public Predicate<CardData> Predicate;

        public override IEnumerator Process()
        {


            _sequence = FindObjectOfType<CardPocketSequence>(true);
            var cc = (CardControllerSelectCard)_sequence.cardController;
            cc.pressEvent.AddListener(ChooseCard);
            cc.canPress = true;
            var container = GetCardContainer();

            if (source == CardSource.Custom)
                foreach (var entity in container)
                    yield return entity.GetCard().UpdateData();

            CinemaBarSystem.In();
            CinemaBarSystem.SetSortingLayer("UI2");
            if (!title.IsEmpty)
            CinemaBarSystem.Top.SetPrompt(title.GetLocalizedString(), "Select");
            _sequence.AddCards(container);
            yield return _sequence.Run();

            if (_selected != null) //Card Selected
            {
                Events.InvokeCardDraw(1);
                yield return Sequences.CardMove(_selected, [References.Player.handContainer]);
                References.Player.handContainer.TweenChildPositions();
                Events.InvokeCardDrawEnd();
                _selected.flipper.FlipUp();
                yield return Sequences.WaitForAnimationEnd(_selected);
                yield return new ActionRunEnableEvent(_selected).Run();
                _selected.display.hover.enabled = true;

                foreach (var stack in addEffectStacks)
                    ActionQueue.Stack(new ActionApplyStatus(_selected, null, stack.data, stack.count));

                _selected.display.promptUpdateDescription = true;
                _selected.PromptUpdate();

                ActionQueue.Stack(new ActionSequence(_selected.UpdateTraits()) { note = $"[{_selected}] Update Traits" });

                _selected = null;
            }

            _cardContainer?.ClearAndDestroyAllImmediately();

            cc.canPress = false;
            cc.pressEvent.RemoveListener(ChooseCard);

            CinemaBarSystem.Clear();
            CinemaBarSystem.Out();

            yield return Remove();
        }

        private void ChooseCard(Entity entity)
        {
            _selected = entity;
            _sequence.promptEnd = true;

            if (!summonCopy)
                return;

            var cardData = _selected.data;
            summonCopy.targetSummon.summonCard = cardData;
            summonCopy.withEffects = [.. addEffectStacks.Select(s => s.data)];
            ActionQueue.Stack(new ActionApplyStatus(target, target, summonCopy, count));
            _selected = null;
        }

        private CardContainer GetCardContainer()
        {
            switch (source)
            {
                case CardSource.Draw:
                    return References.Player.drawContainer;
                case CardSource.Discard:
                    return References.Player.discardContainer;
                case CardSource.Custom:
                    _objectGroup = new GameObject("SelectCardRoutine");
                    _objectGroup.SetActive(false);
                    _objectGroup.transform.SetParent(GameObject.Find("Canvas/Padding/HUD/DeckpackLayout").transform.parent
                        .GetChild(0));
                    _objectGroup.transform.SetAsFirstSibling();

                    _gameObject = new GameObject("SelectCard");
                    var rect = _gameObject.AddComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(7, 2);

                    _cardContainer = CreateCardGrid(_objectGroup.transform, rect);

                    FillCardContainer();

                    _cardContainer.AssignController(Battle.instance.playerCardController);

                    return _cardContainer;
                default:
                    return null;
            }
        }

        private void FillCardContainer()
        {
            if (customCardList.Length <= 0)
            {
                PredicateContainer();
                return;
            }

            amount = amount == 0 ? customCardList.Length : amount;
            foreach (var cardName in InPettyRandomOrder(customCardList).Take(amount))
            {
                var cardData = Goobers.Instance.TryGet<CardData>(cardName).Clone();
                var card = CardManager.Get(cardData, Battle.instance.playerCardController, References.Player,
                    true,
                    true);
                _cardContainer.Add(card.entity);
            }
        }

        private void PredicateContainer()
        {
            var predicate = Goobers.Instance.TryGet<StatusEffectInstantTutor>(name).Predicate;
            if (predicate is null)
                throw new ArgumentException("No predicate found");

            var cards = AddressableLoader.GetGroup<CardData>("CardData")
                .Where(c => predicate.Invoke(c) && c.mainSprite?.name != "Nothing")
                .OrderBy(_ => PettyRandom.Range(0f, 1f)).ToList();
            if (amount != 0)
                cards = cards.Take(amount).ToList();

            cards.Do(cardData =>
            {
                var card = CardManager.Get(cardData.Clone(), Battle.instance.playerCardController, References.Player,
                    true,
                    true);
                _cardContainer.Add(card.entity);
            });
        }

        // Random Order from Pokefrost StatusEffectChangeData
        private static IOrderedEnumerable<T> InPettyRandomOrder<T>(IEnumerable<T> source)
        {
            return source.OrderBy(_ => Dead.PettyRandom.Range(0f, 1f));
        }

        // Card Grid Code by Phan
        private static CardContainerGrid CreateCardGrid(Transform parent, RectTransform bounds = null)
        {
            return CreateCardGrid(parent, new Vector2(2.25f, 3.375f), 5, bounds);
        }

        private static CardContainerGrid CreateCardGrid(Transform parent, Vector2 cellSize, int columnCount,
            RectTransform bounds = null)
        {
            var gridObj = new GameObject("CardGrid", typeof(RectTransform), typeof(CardContainerGrid));
            gridObj.transform.SetParent(bounds ?? parent);
            gridObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var grid = gridObj.GetComponent<CardContainerGrid>();
            grid.holder = grid.GetComponent<RectTransform>();
            grid.onAdd = new UnityEventEntity(); // Fix null reference
            grid.onAdd.AddListener(entity =>
                entity.flipper.FlipUp()); // Flip up card when it's time (without waiting for others)
            grid.onRemove = new UnityEventEntity(); // Fix null reference

            grid.cellSize = cellSize;
            grid.columnCount = columnCount;

            AddScrollers(gridObj); // No click-and-drag. That needs Scroll View
            var scroller = gridObj.GetOrAdd<Scroller>();
            scroller.bounds = bounds; // Change scroller.bounds here if it only scrolls partially

            return grid;
        }

        /// <summary>
        ///     Generic way to make scrollable. Click-and-drag uses ScrollView
        /// </summary>
        /// <param name="parentObject"></param>
        private static void AddScrollers(GameObject parentObject)
        {
            var scroller = parentObject.GetOrAdd<Scroller>(); // Scroll with mouse
            parentObject.GetOrAdd<ScrollToNavigation>().scroller = scroller; // Scroll with controllers
            parentObject.GetOrAdd<TouchScroller>().scroller = scroller; // Scroll with touchscreen
        }



    }

    public class StatusEffectEthereal : StatusEffectData
    {
        public override void Init()
        {
            OnTurnEnd += Check;
        }

        public override bool RunTurnEndEvent(Entity entity)
        {
            if (!target.enabled)
                return false;

            if (entity != target.owner.entity)
                return false;

            return target.InHand();
        }

        private IEnumerator Check(Entity entity)
        {
            yield return Sequences.Wait(0.2f);
            const int amount = 1;
            yield return CountDown(target, amount);
        }

        public override IEnumerator RemoveStacks(int amount, bool removeTemporary)
        {
            count -= amount;
            if (removeTemporary)
                temporary -= amount;
            if (count <= 0)
            {
                yield return Remove();
                yield return target.Kill();
            }

            target.PromptUpdate();
        }
    }


    internal class StatusEffectWhileActiveXBoostableScriptable : StatusEffectWhileActiveX
    {
        public override int GetAmount(Entity entity, bool equalAmount = false, int equalTo = 0)
        {
            var i = scriptableAmount ? GetAmount() : 1;
            return base.GetAmount(entity, equalAmount, equalTo) * i;
        }
    }

    internal class TargetConstraintLeftmostItemInHand : TargetConstraint
    {
        public override bool Check(Entity target)
        {
            var hand = References.Player.handContainer.ToList();
            if (hand == null || !hand.Any(e => e.data.cardType.item))
                return not;

            var result = hand.Last(e => e.data.cardType.item) == target;
            return not ? !result : result;
        }
    }



    public class StatusEffectChangeTargetModeNoSilence : StatusEffectData
    {
        [SerializeField]
        public TargetMode targetMode;

        public TargetMode pre;

        public override bool RunBeginEvent()
        {
            pre = target.targetMode;
            
            target.targetMode = targetMode;
            

            return false;
        }

        public override bool RunEndEvent()
        {
            target.targetMode = pre;
            return false;
        }

        public override bool RunEffectBonusChangedEvent()
        {
            RunEndEvent();
            RunBeginEvent();
            return false;
        }

    }






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



    //ANIMATION STATUS APPLIERS-------------------------------------------------------------------------------------------------------------------------- //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------

    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------
    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------
    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------
    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------
    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------

    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------





    public class StatusEffectInstantGif : StatusEffectInstant
    {
        public float waitFor;

        public override IEnumerator Process()
        {

            VFXHelper.VFX.TryPlayEffect(key: "sp", target.transform.position, target.transform.lossyScale);
            VFXHelper.SFX.TryPlaySound("sp");
            target.curveAnimator.Ping();
            yield return new WaitForSeconds(0.01f);
            yield return base.Process();

        }

    }

    public class StatusEffectKitsu : StatusEffectData
    {

        public override void Init()
        {
            base.OnStack += StackRoutine1;
        }

        public override bool RunStackEvent(int stacks)
        {
            return base.RunStackEvent(stacks);
        }

        public IEnumerator StackRoutine1(int stack)
        {

            VFXHelper.VFX.TryPlayEffect(key: "", target.transform.position, target.transform.lossyScale);
            VFXHelper.SFX.TryPlaySound("");
            target.curveAnimator.Ping();
            yield return new WaitForSeconds(0.01f);

        }

    }

    public class StatusEffectBleed : StatusEffectData
    {
        public bool subbed;

        public bool primed;

        public override void Init()
        {
            base.OnTurnEnd += DealDamage;
            Events.OnPostProcessUnits += Prime;
            subbed = true;
        }

        public void OnDestroy()
        {
            Unsub();
        }

        public void Unsub()
        {
            if (subbed)
            {
                Events.OnPostProcessUnits -= Prime;
                subbed = false;
            }
        }

        public void Prime(Character character)
        {
            primed = true;
            Unsub();
        }

        public override bool RunTurnEndEvent(Entity entity)
        {
            if (primed && target.enabled)
            {
                return entity == target;
            }

            return false;
        }

        public IEnumerator DealDamage(Entity entity)
        {
            Hit hit = new Hit(GetDamager(), target, count)
            {
                screenShake = 0.25f,
                damageType = "bleed"
            };


            VFXHelper.VFX.TryPlayEffect("bleedhit", target.transform.position, 1f * target.transform.lossyScale);
            VFXHelper.SFX.TryPlaySound("");
            target.curveAnimator.Ping();
            yield return new WaitForSeconds(0f);
            yield return hit.Process();
            yield return Sequences.Wait(0.2f);

        }


    }



    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------
    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------
    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------
    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------
    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------
    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------

    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------
    //ANIMATION STATUS APPLIERS--------------------------------------------------------------------------------------------------------------------------


   







    public class TargetModeTaunt : TargetMode   
    {
        public override Entity[] GetPotentialTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            hashSet.AddRange(from e in entity.GetAllEnemies()
                             where (bool)e && e.enabled && e.alive && e.canBeHit && HasTaunt(e)
                             select e);
            if (hashSet.Count <= 0)
            {
                TargetModeBasic targetModeBasic = new TargetModeBasic();
                return targetModeBasic.GetPotentialTargets(entity, target, targetContainer);
            }

            return hashSet.ToArray();
        }

        public override Entity[] GetSubsequentTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            return GetTargets(entity, target, targetContainer);
        }

        public bool HasTaunt(Entity entity)
        {
            foreach (CardData.TraitStacks t in entity.data.traits)
            {
                if (t.data.name == "goobers.Taunt")
                {
                    return true;
                }
            }
            return false;
        }

    }


    private CardData.StatusEffectStacks SStack(string name, int amount) => new CardData.StatusEffectStacks(TryGet<StatusEffectData>(name), amount);
    private CardData.TraitStacks TStack(string name, int amount) => new CardData.TraitStacks(TryGet<TraitData>(name), amount);
    //See above

    //Note: you need to add the reference DeadExtensions.dll in order to use InstantiateKeepName(). 
    private StatusEffectDataBuilder StatusCopy(string oldName, string newName)
    {
        StatusEffectData data = TryGet<StatusEffectData>(oldName).InstantiateKeepName();
        data.name = GUID + "." + newName;
        StatusEffectDataBuilder builder = data.Edit<StatusEffectData, StatusEffectDataBuilder>();
        builder.Mod = this;
        return builder;
    }

    private CardDataBuilder CardCopy(string oldName, string newName)
    {
        CardData data = TryGet<CardData>(oldName).InstantiateKeepName();
        data.name = GUID + "." + newName;
        CardDataBuilder builder = data.Edit<CardData, CardDataBuilder>();
        builder.Mod = this;
        return builder;
    }

    private ClassDataBuilder TribeCopy(string oldName, string newName)
    {
        ClassData data = TryGet<ClassData>(oldName).InstantiateKeepName();
        data.name = GUID + "." + newName;
        ClassDataBuilder builder = data.Edit<ClassData, ClassDataBuilder>();
        builder.Mod = this;
        return builder;
    }


    internal T[] RemoveNulls<T>(T[] data) where T : DataFile
    {
        List<T> list = data.ToList();
        list.RemoveAll(x => x == null || x.ModAdded == this);
        return list.ToArray();
    }
    private T[] DataList<T>(params string[] names) where T : DataFile => names.Select((s) => TryGet<T>(s)).ToArray();

    private RewardPool CreateRewardPool(string name, string type, DataFile[] list)
    {
        RewardPool pool = new RewardPool();
        pool.name = name;
        pool.type = type;            //The usual types are Units, Items, Charms, and Modifiers.
        pool.list = list.ToList();
        return pool;
    }
    public class StatusEffectInstantLoseHealth : StatusEffectInstant
    {
        [SerializeField]
        public StatusEffectData increaseHealthEffect;

        public override IEnumerator Process()
        {
            int amount = GetAmount();
            int num = Mathf.Min(target.hp.current, amount);
            target.hp.max -= amount;
            target.hp.current -= amount;
            target.PromptUpdate();
            Hit hit = new Hit(target, applier, 0)
            {
                canRetaliate = false,
                countsAsHit = false
            };

            yield return hit.Process();
            yield return base.Process();
        }
    }


    public class StatusEffectInstantIncreaseCounter : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            Hit hit = new Hit(((StatusEffectData)this).applier, ((StatusEffectData)this).target, 0)
            {
                countsAsHit = false,
                counterReduction = ((StatusEffectData)this).GetAmount() - ((StatusEffectData)this).GetAmount() - 1
            };
            yield return (object)hit.Process();
            yield return (object)base.Process();
        }
    }


    public class StatusEffectInstantAddRandomCharmToInventory : StatusEffectInstant
    {
        public bool canCreateMultiple;
        public CardUpgradeData[] customList;

        public override IEnumerator Process()
        {
            var inventory = References.PlayerData.inventory;

            var i = canCreateMultiple ? GetAmount() : 1;

            while (i-- > 0)
            {
                var charm = GetCharm();
                inventory.upgrades.Add(charm);
                Debug.Log($"Added {charm.name} to inventory");
            }

            yield return base.Process();
        }

        private CardUpgradeData GetCharm()
        {
            if (customList is { Length: > 0 })
                return customList.RandomItem();

            var component = References.Player.GetComponent<CharacterRewards>();
            return component.Pull<CardUpgradeData>(target, "Charms", 1)[0];
        }
    }







    public class StatusEffectApplyXWhenHitOnce : StatusEffectApplyXWhenHit
    {
        public override void Init()
        {
            base.PostHit += CheckHit;
        }
        public new IEnumerator CheckHit(Hit hit)
        {
            yield return Run(GetTargets(hit, GetTargetContainers(), GetTargetActualContainers()), hit.damage + hit.damageBlocked);
            yield return this.Remove();
            target.display.promptUpdateDescription = true;
            target.PromptUpdate();
        }
    }

    public class StatusEffectDouble : StatusEffectApplyXInstant
    {
        public override bool RunPostApplyStatusEvent(StatusEffectApply apply)
        {
            if ((bool)apply.effectData && apply.count > 0 && apply.effectData.type == type && apply.target != target)
            {
                ActionQueue.Stack(new ActionSequence(Remove())
                {
                    note = "Remove Double from [" + target.name + "]"
                }, fixedPosition: true);
            }

            return false;
        }

        public override bool RunBeginEvent()
        {
            target.effectFactor += 1f;
            return false;
        }

        public override bool RunEndEvent()
        {
            target.effectFactor -= 1f;
            return false;
        }
    }



    internal class ScriptableAmountIsXByStatusCount : ScriptableAmount
    {
        public string statusName;
        private int count;
        private int xamount;

        public override int Get(Entity entity)
        {
            if (entity.statusEffects.Find((effect) => effect.name == statusName).count > count) return xamount;
            return 0;
        }
        public ScriptableAmountIsXByStatusCount(string statusName, int xamount, int count)
        {
            this.statusName = statusName;
            this.count = count;
            this.xamount = xamount;
        }

    }

    public class StatusEffectApplyXWhenWin : StatusEffectApplyX
    {
        public string[] rewardPoolNames;
        public Func<CardData, bool> constraint;
        public int cap = 10;
        public string text = "{0} Found A Card";
        public string cappedText = "{0} Found A Fortune!";

        public static List<ulong> BlockingQueue = new List<ulong>();


        public override void Init()
        {
            Events.OnBattleEnd += CheckPickup;
            base.Init();
        }

        public void OnDestroy()
        {
            Events.OnBattleEnd -= CheckPickup;
        }

        public virtual void CheckPickup()
        {

           

            if (target.IsAliveAndExists())
            {
            }
            if (References.Battle.winner == References.Player)
            {
                ActionQueue.Stack(new ActionSequence(Run(GetTargets())) { note = "Add card to deck" });
            }
        }

        public virtual IEnumerator Run()
        {
           
            yield break;

        }


    }






    internal class StatusEffectApplyRandomOnAllyKilled : StatusEffectApplyXWhenAllyIsKilled
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

   
        public class StatusEffectConnect : StatusEffectApplyXWhenHit
    {

        public Hit storedHit;

        public override void Init()
        {

            base.Init();
        }

        public override bool RunHitEvent(Hit hit)
        {

            if (target.enabled && hit.target == target && hit.canRetaliate && (!targetMustBeAlive || (target.alive && Battle.IsOnBoard(target))) && hit.Offensive && hit.BasicHit)
            {

                storedHit = hit;

                return false;
            }
     

            return false;
        }

        public override bool RunPostHitEvent(Hit hit)
        {
            return hit == storedHit;
        }

        public override bool RunTurnEndEvent(Entity entity)
        {
            if (target.enabled)
            {
                return entity == target;
            }

            return false;
        }

        public IEnumerator Decrease(Entity entity)
        {
            int amount = 0;
            Events.InvokeStatusEffectCountDown(this, ref amount);
            if (amount != 0)
            {
                yield return CountDown(entity, amount);
            }
        }

   

    }





   













    [HarmonyPatch]
    public class ExtraPopups
    {
        static readonly Dictionary<string, (string keyword, PopGroup group)[]> flavours = new()
    {
        { "Pengoon", [
            ("scrap", PopGroup.Right),
            ("scrap", PopGroup.LeftOverflow),
        ] },
        { "Chungoon", [
            ("scrap", PopGroup.Right),
            ("scrap", PopGroup.LeftOverflow),
        ] },
    };

        public enum PopGroup
        {
            Left,
            LeftOverflow,
            Right,
            RightOverflow,
            Bottom
        }

        static Transform GetGroup(CardInspector inspector, PopGroup popGroup) =>
            popGroup switch
            {
                PopGroup.Left => inspector.leftPopGroup,
                PopGroup.LeftOverflow => inspector.leftOverflowPopGroup,
                PopGroup.Right => inspector.rightPopGroup,
                PopGroup.RightOverflow => inspector.rightOverflowPopGroup,
                PopGroup.Bottom => inspector.bottomPopGroup
            };
        static Transform GetGroup(InspectSystem inspector, PopGroup popGroup) =>
            popGroup switch
            {
                PopGroup.Left => inspector.leftPopGroup,
                PopGroup.LeftOverflow => inspector.leftOverflowPopGroup,
                PopGroup.Right => inspector.rightPopGroup,
                PopGroup.RightOverflow => inspector.rightOverflowPopGroup,
                PopGroup.Bottom => inspector.bottomPopGroup
            };

        [HarmonyPatch(typeof(CardInspector), nameof(CardInspector.CreatePopups))]
        static void Postfix(CardInspector __instance, Entity inspect)
        {
            if (inspect.display is not Card card) return;
            if (!flavours.TryGetValue(inspect.name, out var flavour)) return;

            foreach ((string keyword, PopGroup group) in flavour)
            {
                KeywordData data = Text.ToKeyword(keyword);
                __instance.Popup(data, GetGroup(__instance, group));
            }
        }

        [HarmonyPatch(typeof(InspectSystem), nameof(InspectSystem.CreatePopups))]
        static void Postfix(InspectSystem __instance)
        {
            Entity inspect = __instance.inspect;
            if (inspect.display is not Card card) return;
            if (!flavours.TryGetValue(inspect.name, out var flavour)) return;

            foreach ((string keyword, PopGroup group) in flavour)
            {
                KeywordData data = Text.ToKeyword(keyword);
                __instance.Popup(data, GetGroup(__instance, group));
            }
        }
    }



    [HarmonyPatch(typeof(References), nameof(References.Classes), MethodType.Getter)]
    static class FixClassesGetter
    {
        static void Postfix(ref ClassData[] __result) => __result = AddressableLoader.GetGroup<ClassData>("ClassData").ToArray();
    }

    private CardScript GiveUpgrade(string name = "Crown") //Give a crown
    {
        CardScriptGiveUpgrade script = ScriptableObject.CreateInstance<CardScriptGiveUpgrade>(); //This is the standard way of creating a ScriptableObject
        script.name = $"Give {name}";                               //Name only appears in the Unity Inspector. It has no other relevance beyond that.
        script.upgradeData = Get<CardUpgradeData>(name);
        return script;
    }

    private void FixImage(Entity entity)
    {
        if (entity.display is Card card && !card.hasScriptableImage) //These cards should use the static image

            card.mainImage.gameObject.SetActive(true);               //And this line turns them on
    }
    public void UnloadFromClasses()
    {
        List<ClassData> tribes = AddressableLoader.GetGroup<ClassData>("ClassData");
        foreach (ClassData tribe in tribes)
        {
            if (tribe == null || tribe.rewardPools == null) { continue; } //This isn't even a tribe; skip it.

            foreach (RewardPool pool in tribe.rewardPools)
            {
                if (pool == null) { continue; }; //This isn't even a reward pool; skip it.

                pool.list.RemoveAllWhere((item) => item == null || item.ModAdded == this); //Find and remove everything that needs to be removed.
            }
        }

    }
  
 }




[HarmonyPatch(typeof(ActionRedraw), nameof(ActionRedraw.Process))]
class RedrawHoldsRetain
{
    static bool Prefix(ActionRedraw __instance)
    {
        if ((bool)__instance.character)
        {
            foreach (Entity item in __instance.character.handContainer)
            {
                if (item.traits.FirstOrDefault(item2 => item2.data.name.ToLower().Contains("retain")) == null)
                {
                    item.display.hover.SetHoverable(value: false);
                    ActionQueue.Stack(new ActionMove(item, __instance.character.discardContainer));
                }
            }
            if (__instance.drawCount < 0)
            {
                ActionQueue.Add(new ActionDrawHand(__instance.character));
            }
            else if (__instance.drawCount > 0)
            {
                ActionQueue.Add(new ActionDraw(__instance.character, __instance.drawCount));
            }
        }
        return false;
    }
}


  

internal class StatusEffectApplyRandomOnHit : StatusEffectApplyXOnHit
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

namespace Tutorial6_StatusIcons
{

    public static class Ext
    {
        public static StringTable Collection => LocalizationHelper.GetCollection("Card Text", SystemLanguage.English);
        public static StringTable KeyCollection => LocalizationHelper.GetCollection("Tooltips", SystemLanguage.English);

        public static GameObject CreateIcon(this WildfrostMod mod, string name, Sprite sprite, string type, string copyTextFrom, Color textColor, KeywordData[] keys)
        {
            return CreateIcon(mod, name, sprite, type, copyTextFrom, textColor, shadowColor: new Color(0, 0, 0), keys);
        }

        /// <param name="type">has to equal the image filename</param>
        public static GameObject CreateIcon(this WildfrostMod mod, string name, Sprite sprite, string type, string copyTextFrom, Color textColor, Color shadowColor, KeywordData[] keys)
        {
            GameObject gameObject = new GameObject(name);
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
            StatusIcon icon = gameObject.AddComponent<StatusIconExt>();
            Dictionary<string, GameObject> cardIcons = CardManager.cardIcons;
            if (!copyTextFrom.IsNullOrEmpty())
            {
                GameObject text = cardIcons[copyTextFrom].GetComponentInChildren<TextMeshProUGUI>().gameObject.InstantiateKeepName();
                text.transform.SetParent(gameObject.transform);
                icon.textElement = text.GetComponent<TextMeshProUGUI>();
                icon.textColour = textColor;
                icon.textColourAboveMax = textColor;
                icon.textColourBelowMax = textColor;
                icon.textElement.fontMaterial.SetColor("_UnderlayColor", shadowColor);

                Material material = icon.textElement.materialForRendering;
                material = new Material(material); // credit to Hopeful :3
                material.SetColor("_UnderlayColor", shadowColor);
            }
            icon.onCreate = new UnityEngine.Events.UnityEvent();
            icon.onDestroy = new UnityEngine.Events.UnityEvent();
            icon.onValueDown = new UnityEventStatStat();
            icon.onValueUp = new UnityEventStatStat();
            icon.afterUpdate = new UnityEngine.Events.UnityEvent();

            UnityEngine.UI.Image image = gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;

            CardHover cardHover = gameObject.AddComponent<CardHover>();
            cardHover.enabled = false;
            cardHover.IsMaster = false;

            CardPopUpTarget cardPopUp = gameObject.AddComponent<CardPopUpTarget>();
            cardPopUp.keywords = keys;
            cardHover.pop = cardPopUp;
            cardPopUp.posX = -1; // Display keyword to the left of the card

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.sizeDelta *= 0.01f;

            gameObject.SetActive(true);
            icon.type = type;
            cardIcons[type] = gameObject;

            return gameObject;
        }
        //This creates the keyword
        public static KeywordData CreateIconKeyword(this WildfrostMod mod, string name, string title, string desc, string icon, Color body, Color titleC, Color panel)
        {
            KeywordData data = ScriptableObject.CreateInstance<KeywordData>();
            data.name = name;
            KeyCollection.SetString(data.name + "_text", title);
            data.titleKey = KeyCollection.GetString(data.name + "_text");
            KeyCollection.SetString(data.name + "_desc", desc);
            data.descKey = KeyCollection.GetString(data.name + "_desc");
            data.showIcon = true;
            data.showName = false;
            data.iconName = icon;
            data.ModAdded = mod;
            data.bodyColour = body;
            data.titleColour = titleC;
            data.panelColor = panel;
            AddressableLoader.AddToGroup<KeywordData>("KeywordData", data);
            return data;
        }


        //This custom class extends the StatusIcon class to automatically add listeners so that the number on the icon will update automatically
        public class StatusIconExt : StatusIcon
        {
            public override void Assign(Entity entity)
            {
                base.Assign(entity);
                SetText();
                onValueDown.AddListener(delegate { Ping(); });
                onValueUp.AddListener(delegate { Ping(); });
                afterUpdate.AddListener(SetText);
                onValueDown.AddListener(CheckDestroy);
            }
        }

    }


  

   



}


namespace TestMod
{

    //Warning: if you put this method on an OnDeploy effect, punching on the shopkeeper will have the card deploy again.
    //This will cause a loop that can be broken out of (after the player collects hundreds of bling :3).
    public class StatusEffectInstantShop : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            InBattleShopSequence._Run(target);
            //References.instance.StartCoroutine(WaitForControl());
            return base.Process();
        }
    }

    public class StatusEffectInstantCallaFriend : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            InBattleShopSequence2._Run(target);
            //References.instance.StartCoroutine(WaitForControl());
            return base.Process();
        }
    }
}

[HarmonyPatch(typeof(TribeHutSequence), "SetupFlags")]
    class PatchTribeHut
    {
        static string TribeName = "Draw";
        static void Postfix(TribeHutSequence __instance)                                            //After it unlocks the base mods, it'll move on to ours.
        {
            GameObject gameObject = GameObject.Instantiate(__instance.flags[0].gameObject);         //Clone the Snowdweller flag
            gameObject.transform.SetParent(__instance.flags[0].gameObject.transform.parent, false); //Place it in the same group as the others
            TribeFlagDisplay flagDisplay = gameObject.GetComponent<TribeFlagDisplay>();
            ClassData tribe = Goobers.Instance.TryGet<ClassData>("Draw");
            flagDisplay.flagSprite = tribe.flag;                                                    //Replace the flag with our tribe flag
            __instance.flags = __instance.flags.Append(flagDisplay).ToArray();                      //Add it the flag to the list to check
            flagDisplay.SetAvailable();                                                             //Set it available
            flagDisplay.SetUnlocked();                                                              //And unlocked

            //Tribe Hut Patch Part 2

            TribeDisplaySequence sequence2 = GameObject.FindObjectOfType<TribeDisplaySequence>(true);   //TribeDisplaySequence sequence should be unique, so Find should find the right one.
            GameObject gameObject2 = GameObject.Instantiate(sequence2.displays[1].gameObject);          //Copy one of them (Shademancers)
            gameObject2.transform.SetParent(sequence2.displays[2].gameObject.transform.parent, false);  //Place the copy in the right place in the hieracrhy
            sequence2.tribeNames = sequence2.tribeNames.Append(TribeName).ToArray();                    //Add the name to the list
            sequence2.displays = sequence2.displays.Append(gameObject2).ToArray();                      //Add the display itself to the list

            //In case VS isn't recognizing Button, the class is from UnityEngine.UI.
            Button button = flagDisplay.GetComponentInChildren<Button>();                               //Find the button component on our flagDisplay
            button.onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);   //Deactivate the cloned listener (which opens the Snowdweller display)
            button.onClick.AddListener(() => { sequence2.Run(TribeName); });                            //Add our own listener that opens our display

            //(SfxOneShot)
            gameObject2.GetComponent<SfxOneshot>().eventRef = FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/card/draw_multi"); //Shuffling noises

            //0: Flag (ImageSprite)
            gameObject2.transform.GetChild(0).GetComponent<ImageSprite>().SetSprite(tribe.flag);        //Set the sprite of the ImageSprite component to our tribe flag

            //1: Left (ImageSprite)
            Sprite needle = Goobers.Instance.TryGet<CardData>("Sasha").mainSprite;             //Find needle's sprite
            gameObject2.transform.GetChild(1).GetComponent<ImageSprite>().SetSprite(needle);            //and set it as the left image

            //2: Right (ImageSprite)
            Sprite muncher = Goobers.Instance.TryGet<CardData>("Inkabom").mainSprite;           //Find Frost Muncher's sprite
            gameObject2.transform.GetChild(2).GetComponent<ImageSprite>().SetSprite(muncher);           //and set it as the right image
            gameObject2.transform.GetChild(2).localScale *= 1.2f;                                       //and make it 20% bigger

            //3: Textbox (Image)
            gameObject2.transform.GetChild(3).GetComponent<UnityEngine.UI.Image>().color = new Color(0.8f, 0f, 0.6f); //Change the color of the textbox background

            //3-0: Text (LocalizeStringEvent)
            StringTable collection = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English);   //Find a string table (in the desired language)
            gameObject2.transform.GetChild(3).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString(Goobers.TribeDescKey);
            //Set the string in the LocaliseStringEvent

            //4:Title Ribbon (Image)
            //4-0: Text (LocalizeStringEvent)
            gameObject2.transform.GetChild(4).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString(Goobers.TribeTitleKey);
            //Set the string in the LocaliseStringEvent

        }
    }

















