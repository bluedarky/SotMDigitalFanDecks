﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// TODO: TEST!

namespace RuduenWorkshop.Inquirer
{
    public class InquirerLiesOnLiesCharacterCardController : HeroCharacterCardController
    {
        public string str;

        public InquirerLiesOnLiesCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            List<SelectCardDecision> storedResultsSelect = new List<SelectCardDecision>();
            List<DiscardCardAction> storedResultsDiscard = new List<DiscardCardAction>();

            // Select a distortion to move to the top of its deck.
            coroutine = this.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.MoveCardOnDeck, new LinqCardCriteria((Card c) => c.IsInPlay && !c.IsOneShot && c.IsDistortion && this.GameController.IsCardVisibleToCardSource(c, this.GetCardSource(null)), "distortion cards in play", false, false, null, null, false), storedResultsSelect, false, false, null, true, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Move based on decision.
            SelectCardDecision selectCardDecision = storedResultsSelect.FirstOrDefault();
            if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
            {
                Card card = selectCardDecision.SelectedCard;
                if (selectCardDecision.Choices.Count<Card>() == 1)
                {
                    coroutine = this.GameController.SendMessageAction(card.AlternateTitleOrTitle + " is the only distortion card in play.", Priority.Low, this.GetCardSource(null), selectCardDecision.Choices, true);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
                coroutine = this.GameController.MoveCard(this.TurnTakerController, card, card.NativeDeck, false, false, true, null, false, null, null, null, false, false, null, false, false, false, false, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                card = null;
            }

            // You may discard a card. 
            coroutine = this.GameController.SelectAndDiscardCard(this.DecisionMaker, true, null, storedResultsDiscard, SelectionType.DiscardCard, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // If you do,
            if (this.DidDiscardCards(storedResultsDiscard, null, false))
            {            
                // Play the top card of your deck.
                if (this.TurnTaker.IsHero)
                {
                    coroutine = this.GameController.PlayTopCard(this.DecisionMaker, this.TurnTakerController, false, 1, cardSource: this.GetCardSource());
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
                else
                {
                    coroutine = this.GameController.SendMessageAction(this.Card.AlternateTitleOrTitle + " has no deck to play cards from.", Priority.Medium, this.GetCardSource(null), null, true);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }

        }

        // TODO: Replace with something more unique!
        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    {
                        coroutine = this.SelectHeroToPlayCard(this.DecisionMaker);
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 1:
                    {
                        coroutine = this.GameController.SelectHeroToUsePower(this.DecisionMaker, cardSource: this.GetCardSource());
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 2:
                    {
                        coroutine = this.GameController.SelectHeroToDrawCard(this.DecisionMaker, cardSource: this.GetCardSource());
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
            }
            yield break;
        }
    }
}