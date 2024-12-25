using Deadpan.Enums.Engine.Components.Modding;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Tables;
using System.Collections;
using UnityEngine.Localization;


namespace TestMod
{
    public static class InBattleShopSequence
    {
        /* Unity Object Hierarchy
         * Battle Canvas
         * > Parent Object (with this component)
         * >> Background (translucent black that fades in and fades out)
         * >> Background (Carpet/background, image dimensions of ratio 28:10)
         * >> Shopkeeper Holder (Standard CardLane, image dimensions of ratio 2:3)
         * >>> Shopkeeper (Assumed from board, but should work from the anywhere)
         * >> Shop Items Holder (CardLane, for 5 cards, the dimensions should be of ratio ~ 4:1 [I didn't measure anything])
         * >>> Shop Items (Assumed to be spawned)
         */

        // Fields to keep track of the UnityObjects. No need to change them.
        internal static GameObject parentObject;
        internal static CardControllerSelectCard cc;
        internal static CardLane shopkeeperContainer;
        internal static CardLane shopItemContainer;

        // CreateParentObject makes all of the Unity Objects before they are used. No ned to call this method; call _Run instead.
        internal static void CreateParentObject()
        {
            parentObject = new GameObject("InBattleShopSequence");
            parentObject.SetActive(false);
            parentObject.transform.SetParent(GameObject.Find("Battle/Canvas/CardController/Board/Canvas").transform); // This object is known to exist in the Battle scene
            cc = parentObject.AddComponent<CardControllerSelectCard>(); // This component is in charge of hovering and selecting our cards in the shop
            cc.pressEvent = new UnityEventEntity();   //
            cc.hoverEvent = new UnityEventEntity();   // Always inslude this lines if you create your own CardController
            cc.unHoverEvent = new UnityEventEntity(); //
            cc.pressEvent.AddListener(Select);        // This sets up what clicking on a card does.
            cc.owner = References.Player;             // I believe this is necessary, but unsure. Doesn't hurt.

            GameObject background = new GameObject("Background");   // The background darkens the board. This helps give a clear indication regarding when the shop animations concludes
            background.transform.SetParent(parentObject.transform); // Everything should be under the parentObject hierarchy
            background.transform.position = new Vector3(0, 2, 0);   // (0,2,0) is the effective center of the board
            background.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0f); // Invisible even while active (for now)
            background.GetComponent<RectTransform>().sizeDelta = new Vector2(28f, 10f); // Size is *slightly* larger than the player's view
        


            GameObject carpet = new GameObject("Carpet");       // The carpet is the actually background for most of the routine. Can be any picture you want
            carpet.SetActive(false);                            // Initially off because an animation (TweenUI) will start once it's turned on
            carpet.transform.SetParent(parentObject.transform); // Everything should be under the parentObject hierarchy
            carpet.AddComponent<UnityEngine.UI.Image>().sprite = Goobers.Instance.ImagePath("TEST BACKGROUND.png").ToSprite(); //REPLACE: Please replace the image with something of the right dimensions (and Test with your amin mod class)
            carpet.GetComponent<RectTransform>().sizeDelta = new Vector2(28f, 10f); //Size is *slightly* larger than the player's view
            TweenUI tween = carpet.AddComponent<TweenUI>(); // TweenUI (and any Tween you see) is used to start an aimation.
            tween.fireOnEnable = true;                      // The animation will play when the carpet is active.
            tween.target = carpet;                          // The animation will affect the carpet
            tween.property = TweenUI.Property.Move;         // Move animation
            tween.hasFrom = true;
            tween.from = new Vector3(0, 14f, 0);            // Just above the screen
            tween.to = new Vector3(0, 2, 0);                // Center of board
            tween.duration = 1f;
            tween.ease = LeanTweenType.easeOutQuart;        // This is a common TweenType for the game. Slows down before reaching the destination


            GameObject shopkeeperHolder = new GameObject("ShopkeeperHolder");
            shopkeeperHolder.SetActive(false);
            shopkeeperHolder.transform.SetParent(parentObject.transform);
            shopkeeperHolder.transform.position = new Vector3(0, 4, 0);
            shopkeeperHolder.AddComponent<UnityEngine.UI.Image>().sprite = Goobers.Instance.ImagePath(".png").ToSprite();//REPLACE: if you have your own image, replace this with Color.White
            shopkeeperContainer = shopkeeperHolder.AddComponent<CardLane>();             // CardLanes are a row of cards
            shopkeeperContainer.holder = shopkeeperHolder.GetComponent<RectTransform>(); // Necessary line. the CardLane will not assume the holder is itself
            shopkeeperContainer.SetSize(1, 0.7f);                   // This sets the size of cards and the image. The first value is not a hard cap on cards, but it does affect how far an image will extend.
            shopkeeperContainer.onAdd = new UnityEventEntity();     // Necessary.
            shopkeeperContainer.onRemove = new UnityEventEntity();  // Necessary.
            shopkeeperContainer.owner = References.Player;
            shopkeeperContainer.AssignController(cc);
            tween = shopkeeperHolder.AddComponent<TweenUI>(); //More startup animations
            tween.fireOnEnable = true;
            tween.target = shopkeeperHolder;
            tween.property = TweenUI.Property.Move;
            tween.hasFrom = true;
            tween.from = new Vector3(0, 10f, 0);
            tween.to = new Vector3(0, 4, 0);     // Approximately where the top row is
            tween.duration = 1f;
            tween.ease = LeanTweenType.easeOutBounce; // Reaches the destination with two much momentum and bounces a bit before stabilizing


            GameObject shopItemHolder = new GameObject("ShopItemHolder");
            shopItemHolder.SetActive(false);
            shopItemHolder.transform.SetParent(parentObject.transform);
            shopItemHolder.AddComponent<UnityEngine.UI.Image>().sprite = Goobers.Instance.ImagePath(".png").ToSprite(); //REPLACE: if you have your own image, replace this with Color.White
            shopItemContainer = shopItemHolder.AddComponent<CardLane>();
            shopItemContainer.holder = shopItemHolder.GetComponent<RectTransform>();
            shopItemContainer.onAdd = new UnityEventEntity();
            shopItemContainer.onRemove = new UnityEventEntity();
            shopItemContainer.gap = new Vector2(0.5f, 0f);       //
            shopItemContainer.SetSize(5, 0.7f);                  // Tweak these values if you have more or less cards
            shopItemContainer.owner = References.Player;
            shopItemContainer.AssignController(cc);
            tween = shopItemHolder.AddComponent<TweenUI>();  // More tweening
            tween.fireOnEnable = true;
            tween.target = shopItemHolder;
            tween.property = TweenUI.Property.Move;
            tween.hasFrom = true;
            tween.from = new Vector3(0, -6f, 0);
            tween.to = new Vector3(0, 0, 0);
            tween.duration = 1f;
            tween.ease = LeanTweenType.easeOutBounce;
        }

        //Child is used to 
        private static GameObject Child(int index)
        {
            return parentObject.transform.GetChild(index).gameObject;
        }

        //Fade will fade the background to any level you want it at the speed you want. Could be modified to change the color and other parameters.
        private static void Fade(float endAmount, float time)
        {
            Color from = Child(0).GetComponent<UnityEngine.UI.Image>().color;
            Color to = new Color(from.r, from.g, from.b, endAmount);
            LeanTween.value(Child(0), (c) => Child(0).GetComponent<UnityEngine.UI.Image>().color = c, from, to, time);
        }


        // All status effects should call this one
        //Shopkeep will be the card that flies to the top  
        public static void _Run(Entity shopkeep)
        {
            if (parentObject == null)
            {
                CreateParentObject();
            }
            ActionQueue.Stack(new ActionSequence(Run(shopkeep))
            {
                note = "Battle Shop"
            });
        }

        // Where the shop items start. Pretty high up
        internal static Vector3 start = new Vector3(0f, 15f, 0f);
        // progression determines what stage of the event you are in
        static int progression = 0;
        // if you clicked on an item, this will save that
        static Entity chosenItem;
        // changing the fadeLevels of the background
        static float fadeLevel1 = 0.75f;
        static float fadeLevel2 = 0.5f;

        /* 
         * Frame 1: The shopkeep jumps straight up, leaving the player's view. The outer background fades in. 
         * Frame 2: A carpet falls from the top and several face-down cards are dealt onto the carpet.
         * Frame 3: The shopkeep lands on the carpet, the impact flips all face-down cards.
         * Frame 4: <Shopping for a debuff> (maybe custom music? I don't know how to do that :/ )
         * Frame 5A: [Item Selected] The carpet and the other items fall out-of-view. The shopkeep moves the card towards them. The outer background fades out.
         * Frame 6A: [Item Selected] The shopkeep throws the item into their previous spot on board. Then, the shopkeep jumps down out of view. Game resumes.
         * Frame 5B: [Punch Shopkeep] All items fall to the ground in a random order (Maybe a hit animation on the shopkeep? Need to figure that out :/ ).
         * Frame 6B: [Punch Shopkeep] Carpet rushes off screen from the top, shopkeep moves towards the middle of the screen, the outer background gets even darker (miniboss jingle? weather intensity?).
         * Frame 7B: [Punch Shopkeep] The outer background fades out, and the shopkeep moves back to the board. Game resumes. 
         */
        public static IEnumerator Run(Entity shopkeep)
        {
            //Frame 0: Set-up stuff in the background
            Battle.instance.playerCardController.Disable(); // Prevent the player playing cards during the phase
            cc.Enable(); // Activate our own controller

            progression = 0; // Start
            parentObject.SetActive(true);
            parentObject.transform.SetAsLastSibling(); //Ensures the object is in front of the board and everything else
            Card[] cards = CreateCards(shopkeep);
            Routine.Clump clumpy = new Routine.Clump(); //clumpy is the best Coroutine parallelizer that you could ever wish for.

            foreach (var c in cards)
            {
                c.transform.position = start;
                c.entity.flipper.FlipDownInstant();
                clumpy.Add(c.UpdateData());
            }

            //FRAME 1: shopkeep moves up and the battle fades
            Fade(fadeLevel1, 0.5f);
            LeanTween.move(shopkeep.gameObject, shopkeep.transform.position + new Vector3(0, 12f, 0), 1f).setEaseInQuart();
            yield return Sequences.Wait(1f);

            //FRAME 2: carpet falls
            SetBattleMusicVolume(0.4f);
            Child(1).SetActive(true);
            Child(2).SetActive(true);
            Child(3).SetActive(true);
            Fade(fadeLevel2, 1f);
            yield return Sequences.Wait(1f);

            //Makes sure clumpy has finished loading the cards
            yield return clumpy.WaitForEnd();

            //Items dealt face-down
            foreach (var c in cards)
            {
                c.entity.enabled = true;
                shopItemContainer.Add(c.entity);
                shopItemContainer.TweenChildPosition(c.entity); //Tween
                SfxSystem.OneShot(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/card/drag")); //SFX
                yield return Sequences.Wait(0.1f);
            }
            yield return Sequences.Wait(0.5f);

            //FRAME 3: shopkeep lands
            yield return ShopkeepTransition(shopkeep);
            yield return Sequences.Wait(0.1f);

            //cards flip up
            foreach (var c in cards)
            {
                c.entity.flipper.FlipUp();
            }
            SfxSystem.OneShot(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/location/shop/visit"));
            while (true) //There are two ways to leave this loop: progression=1, progression=1000.
            {
                AllowInput();
                //FRAME 4: shopping
                yield return new WaitUntil(() => progression > 0); //Awaiting a response by the player (via Select)
                switch (progression)
                {
                    case 1: //Item chosen
                        //FRAME 5A: Carpet and other cards recede
                        ResetInput();
                        clumpy.Add(DeconstructSet(0.5f));
                        clumpy.Add(CenterCardAndReturn(chosenItem, 0.7f, 0.4f, 0.7f, true));
                        //FRAME 6A: Move card to position
                        yield return Sequences.Wait(1.1f);
                        Fade(0, 0.6f);
                        yield return clumpy.WaitForEnd();
                        End();
                        yield break;
                    case 100: //Shopkeeper click on
                        yield return null;
                        ConfirmAttack();
                        yield return new WaitUntil(() => progression != 100);
                        break;
                    case 1000: //Shopkeeper punched
                        //FRAME 5B: Cards randomly fall (consume I guess)
                        ResetInput();
                        SfxSystem.OneShot(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/attack/hit_level"));
                        foreach (var c in cards.InRandomOrder()) //Remove this line if you don't want the player to get bling randomly
                        {
                            clumpy.Add(c.entity.Kill(DeathType.Consume));
                            yield return Sequences.Wait(0.3f);
                        }

                        yield return clumpy.WaitForEnd();
                        clumpy.Add(CenterCardAndReturn(shopkeep, 0.7f, 0.4f, 0.7f, true));
                        clumpy.Add(DeconstructSet(0.5f));
                        yield return Sequences.Wait(1.1f);
                        Fade(0, 0.6f);
                        yield return clumpy.WaitForEnd();
                        End();
                        yield break;
                }

            }

        }

        //This method creates the shop cards
        //REPLACE: You can replace the itemCardNames (you can even do this from a status effect) or change the implementation of CreateCards entirely.
        internal static string[] itemCardNames = new string[] { "PTimer", "PNull", "PBlock", "PotionFrenzy", "PRestrict", "PHealth", "Ashi Shi's Vault", "Card Launcher"
            , "Get out", "Blunkytime", "Ashishi Totem", "Mini Terrormisu"};
        static Card[] CreateCards(Entity shopkeep)
        {
            return itemCardNames
                .InRandomOrder().Take(5)
                .Select((s) => Goobers.Instance.TryGet<CardData>(s).Clone())
                .Select((c) => CardManager.Get(c, cc, shopkeep.owner, inPlay: false, isPlayerCard: shopkeep.owner == References.Player)).ToArray();
        }

        //Allow the player to click on cards
        static bool preEnabled = true;
        static void AllowInput()
        {
            preEnabled = InputSystem.Enabled;
            InputSystem.Enable();
        }

        static void ResetInput()
        {
            if (!preEnabled)
            {
                InputSystem.Disable();
            }
        }

        //Changes the battle music volume
        static float prevVolume = -1;
        static void SetBattleMusicVolume(float amount, bool storePrev = true)
        {
            BattleMusicSystem system = GameObject.FindObjectOfType<BattleMusicSystem>();
            if (system != null)
            {
                if (storePrev)
                {
                    system.current.getVolume(out prevVolume);
                }
                system.current.setVolume(amount);
            }
        }

        static void ResetBattleMusicVolume()
        {
            BattleMusicSystem system = GameObject.FindObjectOfType<BattleMusicSystem>();
            if (system != null && prevVolume != -1)
            {
                system.current.setVolume(prevVolume);
                prevVolume = -1;
            }
        }

        //ShopkeepTransition is meant to store all of the previous info (location, inPlay, controller) about the shopkeep before overriding.
        //This data is later used for CenterCardAndReturn
        static CardContainer[] oldContainers;
        static CardController pre_cc;
        static bool pre_inPlay;
        static Transform pre_parent;
        static int index;
        static Vector3 pre_spot;
        public static IEnumerator ShopkeepTransition(Entity shopkeep)
        {
            pre_parent = shopkeep.transform.parent;
            oldContainers = shopkeep.containers.Clone() as CardContainer[];
            index = oldContainers[0].IndexOf(shopkeep);
            pre_spot = oldContainers[0].GetChildPosition(shopkeep);
            shopkeep.transform.SetParent(Child(2).transform, true);
            shopkeep.transform.position = new Vector3(0, 10f, 0);

            yield return Sequences.CardMove(shopkeep, new CardContainer[] { shopkeeperContainer });

            pre_cc = shopkeep.display.hover.controller;
            pre_inPlay = shopkeep.inPlay;
            shopkeep.inPlay = false;
            shopkeep.display.hover.controller = cc;
        }

        //Uses animations to move the carpet and cardLanes offscreen
        public static IEnumerator DeconstructSet(float time = 0.5f)
        {
            LeanTween.move(Child(2), new Vector3(0, 10, 0), time).setEaseInQuart();
            LeanTween.move(Child(3), new Vector3(0, -6, 0), time).setEaseInQuart();
            yield return Sequences.Wait(time);
            LeanTween.move(Child(1), new Vector3(0, 14, 0), time).setEaseInQuart();
            yield return Sequences.Wait(time);
            ResetBattleMusicVolume();
        }

        //Places the card to the right spot
        static Vector3 center = new Vector3(0, 2, -3);
        public static IEnumerator CenterCardAndReturn(Entity entity, float timeIn = 1f, float timeHold = 0f, float timeOut = 1f, bool ping = false)
        {
            entity.RemoveFromContainers();
            entity.transform.SetParent(parentObject.transform, true);
            LeanTween.move(entity.gameObject, center, timeIn).setEaseOutSine();
            yield return Sequences.Wait(timeIn);
            if (ping)
            {
                entity.curveAnimator.Ping();
            }
            yield return Sequences.Wait(timeHold);
            Vector3 to = parentObject.transform.InverseTransformPoint(pre_parent.transform.TransformPoint(pre_spot));
            LeanTween.move(entity.gameObject, to, timeOut).setEaseOutSine();
            yield return Sequences.Wait(timeOut);

            entity.inPlay = pre_inPlay;
            entity.display.hover.controller = pre_cc;
            yield return Sequences.CardMove(entity, oldContainers, index);
        }

        //Ends the routine and frees up card resources
        public static void End()
        {
            Battle.instance.playerCardController.Enable();
            cc.Disable();
            shopkeeperContainer.ClearAndDestroyAllImmediately();
            shopItemContainer.ClearAndDestroyAllImmediately();
            //shopkeeperContainer.ClearAndDestroyAllImmediately();
            Child(1).SetActive(false);
            Child(2).SetActive(false);
            Child(3).SetActive(false);
            parentObject.gameObject.SetActive(false);
        }

        //Runs when a card is clicked on.
        public static void Select(Entity entity)
        {
            if (shopkeeperContainer.Contains(entity))
            {
                SelectShopkeeper(entity);
            }
            else
            {
                SelectItem(entity);
            }
        }

        public static void SelectItem(Entity entity)
        {
            chosenItem = entity;
            progression = 1;
        }

        public static void SelectShopkeeper(Entity _)
        {
            HelpPanelSystem.Show(textString); // Opens the sun bear/spirit panel, displaying the info in the LocalizedString
            HelpPanelSystem.SetEmote(Prompt.Emote.Type.Explain);
            // The last parameter in AddButton is an action. It defines what happens when the button is clicked.
            HelpPanelSystem.AddButton(HelpPanelSystem.ButtonType.Positive, yesString, "Select", () => { progression = 100; });
            // Setting the action to null means nothing happens
            HelpPanelSystem.AddButton(HelpPanelSystem.ButtonType.Positive, noString, "Back", null);
        }

        public static void ConfirmAttack()
        {
            HelpPanelSystem.Show(textString2);
            HelpPanelSystem.SetEmote(Prompt.Emote.Type.Scared);
            HelpPanelSystem.AddButton(HelpPanelSystem.ButtonType.Negative, yesString2, "Select", () => { progression = 1000; });
            HelpPanelSystem.AddButton(HelpPanelSystem.ButtonType.Positive, noString2, "Back", () => { progression = 0; });
        }

        public static LocalizedString textString;
        public static LocalizedString yesString;
        public static LocalizedString noString;

        public static LocalizedString textString2;
        public static LocalizedString yesString2;
        public static LocalizedString noString2;

        //REPLACE: Run this method on Load. Some tutorials have a DefineLocalizedStrings method in your main mod class for this reason.
        public static void DefineStrings()
        {
            string textKey = Goobers.Instance.GUID + ".shopattacktext";
            string yesKey = Goobers.Instance.GUID + ".shopattackyes";
            string noKey = Goobers.Instance.GUID + ".shopattackno";
            string textKey2 = Goobers.Instance.GUID + ".shopattacktext2";
            string yesKey2 = Goobers.Instance.GUID + ".shopattackyes2";
            string noKey2 = Goobers.Instance.GUID + ".shopattackno2";

            StringTable ui = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English);

            ui.SetString(textKey, "Shopkeeper|Only the strongest can shopkeep in the midst of combat. They must be incredibly strong.");
            ui.SetString(yesKey, "*punch*");
            ui.SetString(noKey, "*nod*");
            ui.SetString(textKey2, "Attack Shopkeeper|This is a very bad idea, but if you insist...");
            ui.SetString(yesKey2, "I INSIST");
            ui.SetString(noKey2, "Nevermind");

            textString = ui.GetString(textKey);
            yesString = ui.GetString(yesKey);
            noString = ui.GetString(noKey);
            textString2 = ui.GetString(textKey2);
            yesString2 = ui.GetString(yesKey2);
            noString2 = ui.GetString(noKey2);
        }
    }





    public static class InBattleShopSequence2
    {
        /* Unity Object Hierarchy
         * Battle Canvas
         * > Parent Object (with this component)
         * >> Background (translucent black that fades in and fades out)
         * >> Background (Carpet/background, image dimensions of ratio 28:10)
         * >> Shopkeeper Holder (Standard CardLane, image dimensions of ratio 2:3)
         * >>> Shopkeeper (Assumed from board, but should work from the anywhere)
         * >> Shop Items Holder (CardLane, for 5 cards, the dimensions should be of ratio ~ 4:1 [I didn't measure anything])
         * >>> Shop Items (Assumed to be spawned)
         */

        // Fields to keep track of the UnityObjects. No need to change them.
        internal static GameObject parentObject;
        internal static CardControllerSelectCard cc;
        internal static CardLane shopkeeperContainer;
        internal static CardLane shopItemContainer;

        // CreateParentObject makes all of the Unity Objects before they are used. No ned to call this method; call _Run instead.
        internal static void CreateParentObject()
        {
            parentObject = new GameObject("InBattleShopSequence");
            parentObject.SetActive(false);
            parentObject.transform.SetParent(GameObject.Find("Battle/Canvas/CardController/Board/Canvas").transform); // This object is known to exist in the Battle scene
            cc = parentObject.AddComponent<CardControllerSelectCard>(); // This component is in charge of hovering and selecting our cards in the shop
            cc.pressEvent = new UnityEventEntity();   //
            cc.hoverEvent = new UnityEventEntity();   // Always inslude this lines if you create your own CardController
            cc.unHoverEvent = new UnityEventEntity(); //
            cc.pressEvent.AddListener(Select);        // This sets up what clicking on a card does.
            cc.owner = References.Player;             // I believe this is necessary, but unsure. Doesn't hurt.

            GameObject background = new GameObject("Background");   // The background darkens the board. This helps give a clear indication regarding when the shop animations concludes
            background.transform.SetParent(parentObject.transform); // Everything should be under the parentObject hierarchy
            background.transform.position = new Vector3(0, 2, 0);   // (0,2,0) is the effective center of the board
            background.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0f); // Invisible even while active (for now)
            background.GetComponent<RectTransform>().sizeDelta = new Vector2(28f, 10f); // Size is *slightly* larger than the player's view



            GameObject carpet = new GameObject("Carpet");       // The carpet is the actually background for most of the routine. Can be any picture you want
            carpet.SetActive(false);                            // Initially off because an animation (TweenUI) will start once it's turned on
            carpet.transform.SetParent(parentObject.transform); // Everything should be under the parentObject hierarchy
            carpet.AddComponent<UnityEngine.UI.Image>().sprite = Goobers.Instance.ImagePath("Tagshop.png").ToSprite(); //REPLACE: Please replace the image with something of the right dimensions (and Test with your amin mod class)
            carpet.GetComponent<RectTransform>().sizeDelta = new Vector2(28f, 10f); //Size is *slightly* larger than the player's view
            TweenUI tween = carpet.AddComponent<TweenUI>(); // TweenUI (and any Tween you see) is used to start an aimation.
            tween.fireOnEnable = true;                      // The animation will play when the carpet is active.
            tween.target = carpet;                          // The animation will affect the carpet
            tween.property = TweenUI.Property.Move;         // Move animation
            tween.hasFrom = true;
            tween.from = new Vector3(0, 14f, 0);            // Just above the screen
            tween.to = new Vector3(0, 2, 0);                // Center of board
            tween.duration = 1f;
            tween.ease = LeanTweenType.easeOutQuart;        // This is a common TweenType for the game. Slows down before reaching the destination


            GameObject shopkeeperHolder = new GameObject("ShopkeeperHolder");
            shopkeeperHolder.SetActive(false);
            shopkeeperHolder.transform.SetParent(parentObject.transform);
            shopkeeperHolder.transform.position = new Vector3(0, 4, 0);
            shopkeeperHolder.AddComponent<UnityEngine.UI.Image>().sprite = Goobers.Instance.ImagePath(".png").ToSprite();//REPLACE: if you have your own image, replace this with Color.White
            shopkeeperContainer = shopkeeperHolder.AddComponent<CardLane>();             // CardLanes are a row of cards
            shopkeeperContainer.holder = shopkeeperHolder.GetComponent<RectTransform>(); // Necessary line. the CardLane will not assume the holder is itself
            shopkeeperContainer.SetSize(1, 0.7f);                   // This sets the size of cards and the image. The first value is not a hard cap on cards, but it does affect how far an image will extend.
            shopkeeperContainer.onAdd = new UnityEventEntity();     // Necessary.
            shopkeeperContainer.onRemove = new UnityEventEntity();  // Necessary.
            shopkeeperContainer.owner = References.Player;
            shopkeeperContainer.AssignController(cc);
            tween = shopkeeperHolder.AddComponent<TweenUI>(); //More startup animations
            tween.fireOnEnable = true;
            tween.target = shopkeeperHolder;
            tween.property = TweenUI.Property.Move;
            tween.hasFrom = true;
            tween.from = new Vector3(0, 10f, 0);
            tween.to = new Vector3(0, 4, 0);     // Approximately where the top row is
            tween.duration = 1f;
            tween.ease = LeanTweenType.easeOutBounce; // Reaches the destination with two much momentum and bounces a bit before stabilizing


            GameObject shopItemHolder = new GameObject("ShopItemHolder");
            shopItemHolder.SetActive(false);
            shopItemHolder.transform.SetParent(parentObject.transform);
            shopItemHolder.AddComponent<UnityEngine.UI.Image>().sprite = Goobers.Instance.ImagePath(".png").ToSprite(); //REPLACE: if you have your own image, replace this with Color.White
            shopItemContainer = shopItemHolder.AddComponent<CardLane>();
            shopItemContainer.holder = shopItemHolder.GetComponent<RectTransform>();
            shopItemContainer.onAdd = new UnityEventEntity();
            shopItemContainer.onRemove = new UnityEventEntity();
            shopItemContainer.gap = new Vector2(0.5f, 0f);       //
            shopItemContainer.SetSize(5, 0.7f);                  // Tweak these values if you have more or less cards
            shopItemContainer.owner = References.Player;
            shopItemContainer.AssignController(cc);
            tween = shopItemHolder.AddComponent<TweenUI>();  // More tweening
            tween.fireOnEnable = true;
            tween.target = shopItemHolder;
            tween.property = TweenUI.Property.Move;
            tween.hasFrom = true;
            tween.from = new Vector3(0, -6f, 0);
            tween.to = new Vector3(0, 0, 0);
            tween.duration = 1f;
            tween.ease = LeanTweenType.easeOutBounce;
        }

        //Child is used to 
        private static GameObject Child(int index)
        {
            return parentObject.transform.GetChild(index).gameObject;
        }

        //Fade will fade the background to any level you want it at the speed you want. Could be modified to change the color and other parameters.
        private static void Fade(float endAmount, float time)
        {
            Color from = Child(0).GetComponent<UnityEngine.UI.Image>().color;
            Color to = new Color(from.r, from.g, from.b, endAmount);
            LeanTween.value(Child(0), (c) => Child(0).GetComponent<UnityEngine.UI.Image>().color = c, from, to, time);
        }


        // All status effects should call this one
        //Shopkeep will be the card that flies to the top  
        public static void _Run(Entity shopkeep)
        {
            if (parentObject == null)
            {
                CreateParentObject();
            }
            ActionQueue.Stack(new ActionSequence(Run(shopkeep))
            {
                note = "Battle Shop"
            });
        }

        // Where the shop items start. Pretty high up
        internal static Vector3 start = new Vector3(0f, 15f, 0f);
        // progression determines what stage of the event you are in
        static int progression = 0;
        // if you clicked on an item, this will save that
        static Entity chosenItem;
        // changing the fadeLevels of the background
        static float fadeLevel1 = 0.75f;
        static float fadeLevel2 = 0.5f;

        /* 
         * Frame 1: The shopkeep jumps straight up, leaving the player's view. The outer background fades in. 
         * Frame 2: A carpet falls from the top and several face-down cards are dealt onto the carpet.
         * Frame 3: The shopkeep lands on the carpet, the impact flips all face-down cards.
         * Frame 4: <Shopping for a debuff> (maybe custom music? I don't know how to do that :/ )
         * Frame 5A: [Item Selected] The carpet and the other items fall out-of-view. The shopkeep moves the card towards them. The outer background fades out.
         * Frame 6A: [Item Selected] The shopkeep throws the item into their previous spot on board. Then, the shopkeep jumps down out of view. Game resumes.
         * Frame 5B: [Punch Shopkeep] All items fall to the ground in a random order (Maybe a hit animation on the shopkeep? Need to figure that out :/ ).
         * Frame 6B: [Punch Shopkeep] Carpet rushes off screen from the top, shopkeep moves towards the middle of the screen, the outer background gets even darker (miniboss jingle? weather intensity?).
         * Frame 7B: [Punch Shopkeep] The outer background fades out, and the shopkeep moves back to the board. Game resumes. 
         */
        public static IEnumerator Run(Entity shopkeep)
        {
            //Frame 0: Set-up stuff in the background
            Battle.instance.playerCardController.Disable(); // Prevent the player playing cards during the phase
            cc.Enable(); // Activate our own controller

            progression = 0; // Start
            parentObject.SetActive(true);
            parentObject.transform.SetAsLastSibling(); //Ensures the object is in front of the board and everything else
            Card[] cards = CreateCards(shopkeep);
            Routine.Clump clumpy = new Routine.Clump(); //clumpy is the best Coroutine parallelizer that you could ever wish for.

            foreach (var c in cards)
            {
                c.transform.position = start;
                c.entity.flipper.FlipDownInstant();
                clumpy.Add(c.UpdateData());
            }

            //FRAME 1: shopkeep moves up and the battle fades
            Fade(fadeLevel1, 0.5f);
            LeanTween.move(shopkeep.gameObject, shopkeep.transform.position + new Vector3(0, 12f, 0), 1f).setEaseInQuart();
            yield return Sequences.Wait(1f);

            //FRAME 2: carpet falls
            SetBattleMusicVolume(0.4f);
            Child(1).SetActive(true);
            Child(2).SetActive(true);
            Child(3).SetActive(true);
            Fade(fadeLevel2, 1f);
            yield return Sequences.Wait(1f);

            //Makes sure clumpy has finished loading the cards
            yield return clumpy.WaitForEnd();

            //Items dealt face-down
            foreach (var c in cards)
            {
                c.entity.enabled = true;
                shopItemContainer.Add(c.entity);
                shopItemContainer.TweenChildPosition(c.entity); //Tween
                SfxSystem.OneShot(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/card/drag")); //SFX
                yield return Sequences.Wait(0.1f);
            }
            yield return Sequences.Wait(0.5f);

            //FRAME 3: shopkeep lands
            yield return ShopkeepTransition(shopkeep);
            yield return Sequences.Wait(0.1f);

            //cards flip up
            foreach (var c in cards)
            {
                c.entity.flipper.FlipUp();
            }
            SfxSystem.OneShot(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/location/shop/visit"));
            while (true) //There are two ways to leave this loop: progression=1, progression=1000.
            {
                AllowInput();
                //FRAME 4: shopping
                yield return new WaitUntil(() => progression > 0); //Awaiting a response by the player (via Select)
                switch (progression)
                {
                    case 1: //Item chosen
                        //FRAME 5A: Carpet and other cards recede
                        ResetInput();
                        clumpy.Add(DeconstructSet(0.5f));
                        clumpy.Add(CenterCardAndReturn(chosenItem, 0.7f, 0.4f, 0.7f, true));
                        //FRAME 6A: Move card to position
                        yield return Sequences.Wait(1.1f);
                        Fade(0, 0.6f);
                        yield return clumpy.WaitForEnd();
                        End();
                        yield break;
                    case 100: //Shopkeeper click on
                        yield return null;
                        yield return new WaitUntil(() => progression != 100);
                        break;
                    case 1000: //Shopkeeper punched
                        //FRAME 5B: Cards randomly fall (consume I guess)
                        ResetInput();
                        SfxSystem.OneShot(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/attack/hit_level"));
                        foreach (var c in cards.InRandomOrder()) //Remove this line if you don't want the player to get bling randomly
                        {
                            clumpy.Add(c.entity.Kill(DeathType.Consume));
                            yield return Sequences.Wait(0.3f);
                        }

                        yield return clumpy.WaitForEnd();
                        clumpy.Add(CenterCardAndReturn(shopkeep, 0.7f, 0.4f, 0.7f, true));
                        clumpy.Add(DeconstructSet(0.5f));
                        yield return Sequences.Wait(1.1f);
                        Fade(0, 0.6f);
                        yield return clumpy.WaitForEnd();
                        End();
                        yield break;
                }

            }

        }

        //This method creates the shop cards
        //REPLACE: You can replace the itemCardNames (you can even do this from a status effect) or change the implementation of CreateCards entirely.
        internal static string[] itemCardNames = new string[] { "FortuneTagselect", "WinterTagselect", "DemonTagselect", "LuminTagselect", "NovaTagselect", "DetonatorTagselect",
            "TeethTagselect", "SunTagselect", "SplitTagselect","Restagselect"};
        static Card[] CreateCards(Entity shopkeep)
        {
            return itemCardNames
                .InRandomOrder().Take(3)
                .Select((s) => Goobers.Instance.TryGet<CardData>(s).Clone())
                .Select((c) => CardManager.Get(c, cc, shopkeep.owner, inPlay: false, isPlayerCard: shopkeep.owner == References.Player)).ToArray();
        }

        //Allow the player to click on cards
        static bool preEnabled = true;
        static void AllowInput()
        {
            preEnabled = InputSystem.Enabled;
            InputSystem.Enable();
        }

        static void ResetInput()
        {
            if (!preEnabled)
            {
                InputSystem.Disable();
            }
        }

        //Changes the battle music volume
        static float prevVolume = -1;
        static void SetBattleMusicVolume(float amount, bool storePrev = true)
        {
            BattleMusicSystem system = GameObject.FindObjectOfType<BattleMusicSystem>();
            if (system != null)
            {
                if (storePrev)
                {
                    system.current.getVolume(out prevVolume);
                }
                system.current.setVolume(amount);
            }
        }

        static void ResetBattleMusicVolume()
        {
            BattleMusicSystem system = GameObject.FindObjectOfType<BattleMusicSystem>();
            if (system != null && prevVolume != -1)
            {
                system.current.setVolume(prevVolume);
                prevVolume = -1;
            }
        }

        //ShopkeepTransition is meant to store all of the previous info (location, inPlay, controller) about the shopkeep before overriding.
        //This data is later used for CenterCardAndReturn
        static CardContainer[] oldContainers;
        static CardController pre_cc;
        static bool pre_inPlay;
        static Transform pre_parent;
        static int index;
        static Vector3 pre_spot;
        public static IEnumerator ShopkeepTransition(Entity shopkeep)
        {
            pre_parent = shopkeep.transform.parent;
            oldContainers = shopkeep.containers.Clone() as CardContainer[];
            index = oldContainers[0].IndexOf(shopkeep);
            pre_spot = oldContainers[0].GetChildPosition(shopkeep);
            shopkeep.transform.SetParent(Child(2).transform, true);
            shopkeep.transform.position = new Vector3(0, 10f, 0);

            yield return Sequences.CardMove(shopkeep, new CardContainer[] { shopkeeperContainer });

            pre_cc = shopkeep.display.hover.controller;
            pre_inPlay = shopkeep.inPlay;
            shopkeep.inPlay = false;
            shopkeep.display.hover.controller = cc;
        }

        //Uses animations to move the carpet and cardLanes offscreen
        public static IEnumerator DeconstructSet(float time = 0.5f)
        {
            LeanTween.move(Child(2), new Vector3(0, 10, 0), time).setEaseInQuart();
            LeanTween.move(Child(3), new Vector3(0, -6, 0), time).setEaseInQuart();
            yield return Sequences.Wait(time);
            LeanTween.move(Child(1), new Vector3(0, 14, 0), time).setEaseInQuart();
            yield return Sequences.Wait(time);
            ResetBattleMusicVolume();
        }

        //Places the card to the right spot
        static Vector3 center = new Vector3(0, 2, -3);
        public static IEnumerator CenterCardAndReturn(Entity entity, float timeIn = 1f, float timeHold = 0f, float timeOut = 1f, bool ping = false)
        {
            entity.RemoveFromContainers();
            entity.transform.SetParent(parentObject.transform, true);
            LeanTween.move(entity.gameObject, center, timeIn).setEaseOutSine();
            yield return Sequences.Wait(timeIn);
            if (ping)
            {
                entity.curveAnimator.Ping();
            }
            yield return Sequences.Wait(timeHold);
            Vector3 to = parentObject.transform.InverseTransformPoint(pre_parent.transform.TransformPoint(pre_spot));
            LeanTween.move(entity.gameObject, to, timeOut).setEaseOutSine();
            yield return Sequences.Wait(timeOut);

            entity.inPlay = pre_inPlay;
            entity.display.hover.controller = pre_cc;
            yield return Sequences.CardMove(entity, oldContainers, index);
        }

        //Ends the routine and frees up card resources
        public static void End()
        {
            Battle.instance.playerCardController.Enable();
            cc.Disable();
            shopkeeperContainer.ClearAndDestroyAllImmediately();
            shopItemContainer.ClearAndDestroyAllImmediately();
            //shopkeeperContainer.ClearAndDestroyAllImmediately();
            Child(1).SetActive(false);
            Child(2).SetActive(false);
            Child(3).SetActive(false);
            parentObject.gameObject.SetActive(false);
        }

        //Runs when a card is clicked on.
        public static void Select(Entity entity)
        {
            if (shopkeeperContainer.Contains(entity))
            {
                
            }
            else
            {
                SelectItem(entity);
            }
        }

        public static void SelectItem(Entity entity)
        {
            chosenItem = entity;
            progression = 1;
        }

       
        
    }
}















