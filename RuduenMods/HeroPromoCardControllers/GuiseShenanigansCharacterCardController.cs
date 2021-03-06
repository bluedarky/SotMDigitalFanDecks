﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Guise
{
    public class GuiseShenanigansCharacterCardController : PromoDefaultCharacterCardController
    {
        public GuiseShenanigansCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = this.GetPowerNumeral(0, 1);

            List<SelectCardsDecision> storedResults = new List<SelectCardsDecision>();

            IEnumerator coroutine;

            // Draw a card.
            coroutine = this.DrawCards(this.DecisionMaker, 1);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (this.FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.IsOngoing && c.Owner == this.HeroTurnTaker).Count() > 0)
            {
                // Select an ongoing.
                coroutine = this.GameController.SelectCardsAndStoreResults(this.DecisionMaker, SelectionType.MakeIndestructible,
                    (Card c) => c.IsInPlayAndHasGameText && c.IsOngoing && c.Owner == this.HeroTurnTaker, powerNumeral,
                    storedResults, false, powerNumeral, cardSource: this.GetCardSource());
                if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                List<Card> selectedCards = this.GetSelectedCards(storedResults).ToList();
                if (selectedCards != null && selectedCards.Count() > 0)
                {
                    MakeIndestructibleStatusEffect makeIndestructibleStatusEffect = new MakeIndestructibleStatusEffect();
                    makeIndestructibleStatusEffect.CardsToMakeIndestructible.IsOneOfTheseCards = selectedCards;
                    makeIndestructibleStatusEffect.CardsToMakeIndestructible.OwnedBy = this.HeroTurnTaker;
                    makeIndestructibleStatusEffect.UntilEndOfNextTurn(this.HeroTurnTaker);
                    coroutine = this.AddStatusEffect(makeIndestructibleStatusEffect, true);
                    if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
            else
            {
                string turnTakerName;

                // Set up response.
                if (base.TurnTaker.IsHero)
                {
                    turnTakerName = this.TurnTaker.Name;
                }
                else
                {
                    turnTakerName = this.Card.Title;
                }
                coroutine = this.GameController.SendMessageAction(turnTakerName + " does not have any Ongoings in play, so he cannot make any indestructible. Whoops!", Priority.Medium, this.GetCardSource(), null, true);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}