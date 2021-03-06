﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Inquirer
{
    // TODO: TEST!
    public class UntilYouMakeItCardController : CardController
    {
        public UntilYouMakeItCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Search for persona.
            coroutine = this.GameController.SelectCardFromLocationAndMoveIt(this.DecisionMaker, this.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.DoKeywordsContain("persona"), () => "persona", true, false, null, null, false), new MoveCardDestination[]
            {
                new MoveCardDestination(this.TurnTaker.PlayArea)
            }, true, true, true, false, null, false, false, null, false, false, null, null, this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw card.
            coroutine = this.DrawCard(null, true, null, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Play card.
            coroutine = this.SelectAndPlayCardFromHand(this.DecisionMaker, true, null, null, false, false, true, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}