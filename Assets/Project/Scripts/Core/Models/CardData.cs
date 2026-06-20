namespace CardsFramework.Core.Models
{
    public enum CardSuit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum CardValue
    {
        Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, 
        Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14
    }

    /// <summary>
    /// Pure and unchanging POCO representation of a card.
    /// Completely decoupled from Unity rendering.
    /// </summary>
    public class CardData
    {
        public CardSuit Suit { get; }
        public CardValue Value { get; }

        public CardData(CardSuit suit, CardValue value)
        {
            Suit = suit;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Value} of {Suit}";
        }
    }
}