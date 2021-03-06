﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using RuduenWorkshop.BreachMage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RuduenModsTest
{
    [TestFixture]
    public class BreachMageTest : BaseTest
    {
        [OneTimeSetUp]
        public void DoSetup()
        {
            // Tell the engine about our mod assembly so it can load up our code.
            // It doesn't matter which type as long as it comes from the mod's assembly.
            //var a = Assembly.GetAssembly(typeof(InquirerCharacterCardController)); // replace with your own type
            ModHelper.AddAssembly("RuduenWorkshop", Assembly.GetAssembly(typeof(BreachMageCharacterCardController))); // replace with your own namespace
        }

        protected HeroTurnTakerController BreachMage { get { return FindHero("BreachMage"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(BreachMage);
            Assert.IsInstanceOf(typeof(BreachMageTurnTakerController), BreachMage);
            Assert.IsInstanceOf(typeof(BreachMageCharacterCardController), BreachMage.CharacterCardController);

            Assert.AreEqual(27, BreachMage.CharacterCard.HitPoints);

            AssertNumberOfCardsInHand(BreachMage, 4); // Starting hand.
            AssertNumberOfCardsInDeck(BreachMage, 36); // Starting deck.
        }

        [Test()]
        public void TestBreachMageInitialBreachesWork()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card[] breaches = this.GameController.FindCardsWhere((Card c) => c.DoKeywordsContain("breach") && c.Owner == BreachMage.HeroTurnTaker && c.IsInPlay).ToArray();
            int[] initFocus = new int[] { 0, 1, 2, 3 };

            Assert.IsTrue(breaches.Count() == 4);

            for (int i = 0; i < 4; i++)
            {
                AssertTokenPoolCount(breaches[i].FindTokenPool("FocusPool"), initFocus[i]);
            }

            DestroyCards(breaches);
            AssertIsInPlay(breaches); // Confirm they're indestructible, too. 
        }

        [Test()]
        public void TestBreachMageBreachOpenPowers()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card[] breaches = this.GameController.FindCardsWhere((Card c) => c.DoKeywordsContain("breach") && c.Owner == BreachMage.HeroTurnTaker && c.IsInPlay).ToArray();
            int[] initFocus = new int[] { 0, 1, 2, 3 };
            Card[] spells = FindCardsWhere((Card c) => c.IsSpell && c.Owner == BreachMage.HeroTurnTaker).ToArray();
            PutInHand(spells);

            Assert.IsTrue(breaches.Count() == 4);

            for (int i = 0; i < 4; i++)
            {
                RemoveTokensFromPool(breaches[i].FindTokenPool("FocusPool"), initFocus[i]);
            }

            DecisionSelectCards = new Card[] { spells[0], breaches[0], spells[1], breaches[1], spells[2], breaches[2], spells[3] };

            QuickHandStorage(BreachMage);

            UsePower(breaches[0]); // Stable Breach: Play up to 3 spells. 
            UsePower(breaches[3]); // Open breach: Play + Draw. 
            // Four spells should be in play. 
            AssertIsInPlay(spells[0], spells[1], spells[2], spells[3]);
            QuickHandCheck(-3); // 4 spells played, 1 drawn.

        }

        [Test()]
        public void TestBreachMagePlaySpell()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card card = PutIntoPlay("FlareCascade");
            Card breach = FindCardInPlay("BreachI");

            AssertNextToCard(card, breach);
        }

        [Test()]
        public void TestBreachMageCastSpell()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card card = PutIntoPlay("FlareCascade");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp);
            GoToStartOfTurn(BreachMage);
            QuickHPCheck(-3); // MDP took 3 damage.
            AssertInTrash(card);
        }

        [Test()]
        public void TestBreachMageCastSpellPotent()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card breach = FindCardInPlay("BreachIII");
            Card mdp = FindCardInPlay("MobileDefensePlatform");
            Card card = PutInHand("FlareCascade");

            // Use 2 times to open. 
            UsePower(breach);
            UsePower(breach);

            DecisionSelectCards = new Card[] { breach, card, mdp };


            PlayCard(card);
            QuickHPStorage(mdp);
            GoToStartOfTurn(BreachMage);
            QuickHPCheck(-4); // MDP took 4 damage.
            AssertInTrash(card);
        }


        [Test()]
        public void TestBreachMageInnatePower()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            List<Card> charges = new List<Card>
            {
                PlayCard("HammerCharm", 0)
            };

            QuickHandStorage(BreachMage);
            UsePower(BreachMage.CharacterCard); // Charge innate.
            QuickHandCheck(2); // 2 Cards Drawn.
            AssertInTrash(charges); // All used charges in trash.
        }

        [Test()]
        public void TestBreachMageTwincasterInnatePower()
        {
            List<string> identifiers = new List<string>()
            {
                "BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "BreachMageCharacter", "BreachMageTwincasterCharacter" }
            };

            SetupGameController(identifiers, false, promos);

            StartGame();

            List<Card> usedCards = new List<Card>()
            {
                PlayCard("HammerCharm", 0),
                PlayCard("HammerCharm", 1),
                PlayCard("VisionShock")
            };

            Card mdp = FindCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard);
            QuickHPCheck(-8); // Damage Dealt twice.
            AssertInTrash(usedCards); // All used charges in trash.
        }

        [Test()]
        public void TestBreachMageTwincasterInnatePowerInterrupted()
        {
            List<string> identifiers = new List<string>()
            {
                "BaronBlade", "RuduenWorkshop.BreachMage", "Unity", "Megalopolis"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "BreachMageCharacter", "BreachMageTwincasterCharacter" }
            };

            SetupGameController(identifiers, false, promos);

            StartGame();

            List<Card> usedCards = new List<Card>()
            {
                PlayCard("HammerCharm", 0),
                PlayCard("HammerCharm", 1),
                PlayCard("VisionShock")
            };

            Card mdp = FindCardInPlay("MobileDefensePlatform");
            Card bee = PlayCard("BeeBot");
            DecisionSelectCards = new List<Card>() { usedCards[0], bee, mdp, usedCards[2] };

            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard);
            QuickHPCheck(-2); // Damage Dealt by bee, but not by spell.
            AssertInTrash(usedCards); // All used charges in trash.
        }

        //[Test()]
        //public void TestBreachMageCastSpellStartOfTurn()
        //{
        //    // NOTE: This is expected to fail right now due to a quirk in the Activatable Ability framework. Specifically, no current activatable abilities are optional. 

        //    SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

        //    StartGame();

        //    Card mdp = FindCardInPlay("MobileDefensePlatform");

        //    PlayCard("FocusCharm");
        //    PlayCard("FocusCharm");
        //    PlayCard("FocusCharm");

        //    Card[] spells = new Card[]{
        //        PlayCard("Zap"),
        //        PlayCard("MoltenWave")
        //    };
        //    QuickHPStorage(mdp);

        //    DecisionActivateAbilities = new Card[] { spells[0], null };
        //    DecisionSelectTarget = mdp;

        //    GoToStartOfTurn(BreachMage);

        //    QuickHPCheck(-1); // Zapped.

        //}

        [Test()]
        public void TestBreachMageCastSpellStartOfTurn()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card mdp = FindCardInPlay("MobileDefensePlatform");

            PlayCard("FocusCharm");
            PlayCard("FocusCharm");
            PlayCard("FocusCharm");

            Card[] spells = new Card[]{
                PlayCard("Zap"),
                PlayCard("MoltenWave")
            };
            QuickHPStorage(mdp);

            DecisionSelectCards = new Card[] { spells[0], mdp, null };

            GoToStartOfTurn(BreachMage);

            QuickHPCheck(-1); // Zapped.

        }

        //// TODO: Fix if Handlabra fix!
        //[Test(Description = "Failing Handlabra Case", ExpectedResult = false)]
        //public bool TestBreachMageCastSpellStartOfTurnFailingHandelabra()
        //{
        //    // NOTE: This is expected to fail right now due to a quirk in the Activatable Ability framework. Specifically, no current activatable abilities are optional. 

        //    SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

        //    StartGame();

        //    Card mdp = FindCardInPlay("MobileDefensePlatform");

        //    PlayCard("FocusCharm");
        //    PlayCard("FocusCharm");
        //    PlayCard("FocusCharm");

        //    Card[] spells = new Card[]{
        //        PlayCard("Zap"),
        //        PlayCard("MoltenWave")
        //    };
        //    QuickHPStorage(mdp);

        //    DecisionActivateAbilities = new Card[] { spells[0], null };
        //    DecisionSelectTarget = mdp;

        //    try
        //    {
        //        GoToStartOfTurn(BreachMage);
        //    }
        //    catch (Exception e)
        //    {
        //        if (e is System.NullReferenceException)
        //        {
        //            return false; // The card should be in the play area! Expect a fail right now.
        //        }
        //    }
        //    return true;
        //}

        [Test()]
        public void TestCardCycleOfMagic()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("VisionShock");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card cycle = PutInHand("CycleOfMagic");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            QuickHandStorage(BreachMage);
            PlayCard(cycle);
            QuickHPCheck(-4); // Damage Dealt.
            QuickHandCheck(1); // 2 Cards Drawn, -1 for card played.
            AssertInDeck(spell);
        }

        [Test()]
        public void TestCardFocusBreach()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card play = PutInHand("FlareCascade");

            DecisionSelectCardToPlay = play;

            PutIntoPlay("FocusBreach");

            AssertTokenPoolCount(FindCardInPlay("BreachII").TokenPools.FirstOrDefault(), 0);
            AssertIsInPlay(play);
        }

        [Test()]
        public void TestChargeAuraCharm()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Legacy", "Megalopolis");

            StartGame();

            PlayCard("AuraCharm");

            // Only usable power was used.
            AssertNumberOfUsablePowers(legacy, 0);
        }

        [Test()]
        public void TestChargeHammerCharm()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();
            Card ongoing = PlayCard("LivingForceField");
            DecisionDestroyCard = ongoing;

            PlayCard("HammerCharm");
            AssertInTrash(ongoing); // Ongoing destroyed.
        }

        [Test()]
        public void TestChargeSpiralCharm()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("VisionShock");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("SpiralCharm");
            QuickHPCheck(-4); // Damage Dealt. Base 4.
            AssertIsInPlay(spell); // Spell not destroyed.
        }

        [Test()]
        public void TestChargeVigorCharm()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card card = PutInHand("VigorCharm");

            QuickHPStorage(BreachMage);
            QuickHandStorage(BreachMage);
            PlayCard(card);
            QuickHPCheck(-2); // Damage Dealt. Base 2.
            QuickHandCheck(2); // Draw 2.
            AssertInPlayArea(BreachMage, card); // Charm still in play.
        }

        [Test()]
        public void TestSpellFlareCascade()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("FlareCascade");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            GoToStartOfTurn(BreachMage);
            QuickHPCheck(-3); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed.
        }

        [Test()]
        public void TestSpellFlareCascadeChargeAndBuffed()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();


            Card breach = FindCardInPlay("BreachIII");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card spell = PutInHand("FlareCascade");

            // Use 2 times to open. 
            UsePower(breach);
            UsePower(breach);

            Card[] charges = FindCardsWhere((Card c) => c.Identifier == "HammerCharm").ToArray();

            PutInHand(charges);
            PlayCards(charges);


            DecisionSelectCards = new List<Card>() { breach, spell, mdp, charges[0], mdp, null };

            PlayCard(spell);

            QuickHPStorage(mdp);
            GoToStartOfTurn(BreachMage);
            QuickHPCheck(-8); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed.
        }


        [Test()]
        public void TestSpellHauntingEcho()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("HauntingEcho");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card ongoing = PlayCard("LivingForceField");
            DecisionSelectTarget = mdp;
            DecisionDestroyCard = ongoing;
            DecisionSelectFunction = 1;

            QuickHPStorage(mdp);
            GoToStartOfTurn(BreachMage);
            QuickHPCheck(-2); // Damage Dealt.
            AssertInTrash(ongoing, spell); // Ongoing & Spell destroyed.
        }

        [Test()]
        public void TestSpellMoltenWave()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("MoltenWave");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card bb = GetCardInPlay("BaronBladeCharacter");
            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 1;

            DealDamage(mdp, mdp, 8, DamageType.Fire); // Set up MDP to be destroyed so AoE also hits BB.

            QuickHPStorage(bb);
            GoToStartOfTurn(BreachMage);
            QuickHPCheck(-2); // Damage Dealt.
            AssertInTrash(spell, mdp); // Spell destroyed, MDP destroyed via damage.
        }

        [Test()]
        public void TestSpellShine()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("Shine");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 1;

            QuickHPStorage(mdp);
            QuickHandStorage(BreachMage);
            GoToStartOfTurn(BreachMage);
            QuickHPCheck(-3); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed.
            QuickHandCheck(1); // Card drawn.
        }

        [Test()]
        public void TestSpellVisionShock()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PutIntoPlay("VisionShock");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 1;

            QuickHPStorage(mdp);
            GoToStartOfTurn(BreachMage);
            QuickHPCheck(-4); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed.

            // TODO: Add scrying test at some point! (Right now, more complex than it's worth.)
        }

        [Test()]
        public void TestSpellZap()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PutIntoPlay("Zap");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            GoToStartOfTurn(BreachMage);
            QuickHPCheck(-1); // Damage Dealt.
            AssertInHand(spell); // Spell returned to hand due to interruption.
        }
    }
}