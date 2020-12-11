﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.BreachMage
{
    public abstract class BreachMageSharedBreachController : RuduenFlippableCardController
    {
        public BreachMageSharedBreachController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.SpecialStringMaker.ShowTokenPool(this.Card.FindTokenPool("FocusPool"), null, null).Condition = (() => this.Card.IsInPlay && this.Card.IsFlipped);
        }

        public virtual IEnumerator UseOpenPower()
        {
            // Play card.
            IEnumerator coroutine = this.SelectAndPlayCardFromHand(this.DecisionMaker, cardCriteria: new LinqCardCriteria((Card c)=>c.DoKeywordsContain("spell")));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public virtual IEnumerator UseFocusPower()
        {
            IEnumerator coroutine = this.GameController.RemoveTokensFromPool(this.Card.FindTokenPool("FocusPool"), 1);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Break down into two powers.
            IEnumerator coroutine;
            if (!this.CardWithoutReplacements.IsFlipped)
            {
                // Power to optionally play a spell. 
                coroutine = this.UseOpenPower();
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
            else
            {
                coroutine = this.UseFocusPower();
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        public override void AddSideTriggers()
        {
            // Triggers while on a certain side. This is re-checked after flipping! 
            if (this.CardWithoutReplacements.IsInPlay)
            {
                if (this.CardWithoutReplacements.IsFlipped)
                {
                    ITrigger flipTrigger = this.AddTrigger
                        (
                            (RemoveTokensFromPoolAction rtfp) => rtfp.TokenPool == this.Card.FindTokenPool("FocusPool") && rtfp.TokenPool.CurrentValue == 0, (RemoveTokensFromPoolAction rtfp) => this.GameController.FlipCard(this, cardSource: this.GetCardSource()), TriggerType.FlipCard, TriggerTiming.After
                        );
                    // When not flipped, triggers (during initial call):
                    this.AddSideTrigger
                    (
                        // Only trigger on unflipped side: Flip at 0 tokens.
                        flipTrigger
                    );
                }
            }
        }

        // Make this card indestructible if any other card asks. This is true on both sides!
        public override bool AskIfCardIsIndestructible(Card card)
        {
            return card == this.Card;
        }
        
        // One spell per breach!
        public override bool CanOtherCardGoNextToThisCard(Card card)
        {
            return !card.IsSpell || this.GetNumberOfSpellCardsNextToThisCard() < 1;
        }

        private int GetNumberOfSpellCardsNextToThisCard()
        {
            return (from c in this.Card.NextToLocation.Cards
                    where c.IsSpell
                    select c).Count<Card>();
        }
    }
}