using System.Collections.Generic;
using CardFramework.Core.Models;

namespace CardFramework.Core.Engines
{
    /// <summary>
    /// Pure Texas Hold'em game rules and flow engine.
    /// Manages betting rounds and community cards without MonoBehaviour.
    /// </summary>
    public class TexasHoldemEngine
    {
        public enum RoundState { PreFlop, Flop, Turn, River, Showdown }

        private readonly Deck deck = new();
        public List<CardData> PlayerHand { get; } = new();
        public List<CardData> CommunityCards { get; } = new();
        public RoundState CurrentRound { get; private set; } = RoundState.PreFlop;

        public void StartNewHand()
        {
            PlayerHand.Clear();
            CommunityCards.Clear();
            deck.Initialize();
            deck.Shuffle();

            // Deal hole cards (2 to player)
            PlayerHand.Add(deck.Draw());
            PlayerHand.Add(deck.Draw());

            CurrentRound = RoundState.PreFlop;
        }

        public void AdvanceRound()
        {
            switch (CurrentRound)
            {
                case RoundState.PreFlop:
                    // Deal Flop (3 cards)
                    CommunityCards.Add(deck.Draw());
                    CommunityCards.Add(deck.Draw());
                    CommunityCards.Add(deck.Draw());
                    CurrentRound = RoundState.Flop;
                    break;

                case RoundState.Flop:
                    // Deal Turn (1 card)
                    CommunityCards.Add(deck.Draw());
                    CurrentRound = RoundState.Turn;
                    break;

                case RoundState.Turn:
                    // Deal River (1 card)
                    CommunityCards.Add(deck.Draw());
                    CurrentRound = RoundState.River;
                    break;

                case RoundState.River:
                    CurrentRound = RoundState.Showdown;
                    break;
            }
        }
    }
}