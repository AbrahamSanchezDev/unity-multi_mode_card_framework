using System;

namespace CardFramework.Core.Models {
    /// <summary>
    /// Immutable card data structure (POCO - Plain Old C# Object)
    /// No MonoBehaviour, no Unity dependencies. Optimized as a struct to prevent GC allocation.
    /// </summary>
    public struct CardData : IEquatable<CardData> {
        public enum Suit : byte { Clubs = 0, Diamonds = 1, Hearts = 2, Spades = 3 }
        public enum Rank : byte {
            Ace = 1, Two = 2, Three = 3, Four = 4, Five = 5,
            Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10,
            Jack = 11, Queen = 12, King = 13
        }

        public readonly Suit CardSuit;
        public readonly Rank CardRank;

        public CardData(Suit suit, Rank rank) {
            CardSuit = suit;
            CardRank = rank;
        }

        public override bool Equals(object obj) => obj is CardData card && Equals(card);
        public bool Equals(CardData other) => CardSuit == other.CardSuit && CardRank == other.CardRank;
        public override int GetHashCode() => ((int)CardSuit << 4) | (int)CardRank;
        public override string ToString() => $"{CardRank}{CardSuit}";

        public static bool operator ==(CardData left, CardData right) => left.Equals(right);
        public static bool operator !=(CardData left, CardData right) => !left.Equals(right);
    }
}