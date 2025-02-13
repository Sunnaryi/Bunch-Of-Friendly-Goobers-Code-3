﻿using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using System.Linq;
using UnityEngine;

[HarmonyPatch(typeof(PetHutFlagSetter), "SetupFlag")]
internal static class PatchInPetFlag
{
    static void Prefix(PetHutFlagSetter __instance)
    {

        Texture2D Etex = Goobers.Instance.ImagePath("montasign.png").ToTex();
        Sprite Espr = Sprite.Create(Etex, new Rect(0, 0, Etex.width, Etex.height), new Vector2(0.5f, 1f), 160);
        Texture2D Rtex = Goobers.Instance.ImagePath("VOLTAFLAG.png").ToTex();
        Sprite Rspr = Sprite.Create(Rtex, new Rect(0, 0, Rtex.width, Rtex.height), new Vector2(0.5f, 1f), 160);
        Texture2D RtexD = Goobers.Instance.ImagePath("OMMI.png").ToTex();
        Sprite RsprD = Sprite.Create(RtexD, new Rect(0, 0, RtexD.width, RtexD.height), new Vector2(0.5f, 1f), 160);
        Texture2D RtexA = Goobers.Instance.ImagePath("FROGFLAG.png").ToTex();
        Sprite RsprA = Sprite.Create(RtexA, new Rect(0, 0, RtexA.width, RtexA.height), new Vector2(0.5f, 1f), 160);
        Texture2D RtexB = Goobers.Instance.ImagePath("CBSWING.png").ToTex();
        Sprite RsprB = Sprite.Create(RtexB, new Rect(0, 0, RtexB.width, RtexB.height), new Vector2(0.5f, 1f), 160);
        Texture2D Ltex = Goobers.Instance.ImagePath("Poochieflag.png").ToTex();
        Sprite Lspr = Sprite.Create(Ltex, new Rect(0, 0, Ltex.width, Ltex.height), new Vector2(0.5f, 1f), 160);
        Texture2D Vtex = Goobers.Instance.ImagePath("ANGYFLAG.png").ToTex();
        Sprite Vspr = Sprite.Create(Vtex, new Rect(0, 0, Vtex.width, Vtex.height), new Vector2(0.5f, 1f), 160);

        __instance.flagSprites = __instance.flagSprites.Append(Vspr).ToArray();
        __instance.flagSprites = __instance.flagSprites.Append(Lspr).ToArray();
        __instance.flagSprites = __instance.flagSprites.Append(RsprB).ToArray();
        __instance.flagSprites = __instance.flagSprites.Append(RsprA).ToArray();
        __instance.flagSprites = __instance.flagSprites.Append(RsprD).ToArray();
        __instance.flagSprites = __instance.flagSprites.Append(Rspr).ToArray();
        __instance.flagSprites = __instance.flagSprites.Append(Espr).ToArray();
        
     
       
    }

    static Sprite CreateSprite(int density)
    {
        Texture2D Etex = Goobers.Instance.ImagePath("montasign.png").ToTex();
        Sprite Espr = Sprite.Create(Etex, new Rect(0, 0, Etex.width, Etex.height), new Vector2(0.5f, 1f), 160);
        return Espr;

    }

    static Sprite CreateSpriteADA(int density)
    {
        Texture2D Rtex = Goobers.Instance.ImagePath("VOLTAFLAG.png").ToTex();
        Sprite Rspr = Sprite.Create(Rtex, new Rect(0, 0, Rtex.width, Rtex.height), new Vector2(0.5f, 1f), 160);
        return Rspr;

    }
    static Sprite CreateSpriteAD(int density)
    {
        Texture2D RtexD = Goobers.Instance.ImagePath("OMMI.png").ToTex();
        Sprite RsprD = Sprite.Create(RtexD, new Rect(0, 0, RtexD.width, RtexD.height), new Vector2(0.5f, 1f), 160);
        return RsprD;

    }
    static Sprite CreateSpriteA(int density)
    {
        Texture2D RtexA = Goobers.Instance.ImagePath("FROGFLAG.png").ToTex();
        Sprite RsprA = Sprite.Create(RtexA, new Rect(0, 0, RtexA.width, RtexA.height), new Vector2(0.5f, 1f), 160);
        return RsprA;

    }

    static Sprite CreateSprite2(int density)
    {
        Texture2D RtexB = Goobers.Instance.ImagePath("CBSWING.png").ToTex();
        Sprite RsprB = Sprite.Create(RtexB, new Rect(0, 0, RtexB.width, RtexB.height), new Vector2(0.5f, 1f), 160);
        return RsprB;

    }
    static Sprite CreateSprite3(int density)
    {
        Texture2D Ltex = Goobers.Instance.ImagePath("Poochieflag.png").ToTex();
        Sprite Lspr = Sprite.Create(Ltex, new Rect(0, 0, Ltex.width, Ltex.height), new Vector2(0.5f, 1f), 160);
        return Lspr;

    }
    static Sprite CreateSprite4(int density)
    {
        Texture2D Vtex = Goobers.Instance.ImagePath("ANGYFLAG.png").ToTex();
        Sprite Vspr = Sprite.Create(Vtex, new Rect(0, 0, Vtex.width, Vtex.height), new Vector2(0.5f, 1f), 160);
        return Vspr;

    }

}

















