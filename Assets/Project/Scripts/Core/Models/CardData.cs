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
        public readonly bool IsFaceUp;
        public readonly int InstanceId;
        public readonly bool HasBeenRevealed;

        private static int _nextInstanceId = 1;

        public CardData(Suit suit, Rank rank, bool isFaceUp = false, int? instanceId = null, bool hasBeenRevealed = false) {
            CardSuit = suit;
            CardRank = rank;
            IsFaceUp = isFaceUp;
            InstanceId = instanceId ?? NextInstanceId();
            HasBeenRevealed = hasBeenRevealed;
        }

        private static int NextInstanceId() {
            int nextId = _nextInstanceId;
            _nextInstanceId++;
            return nextId;
        }

        public override bool Equals(object obj) => obj is CardData card && Equals(card);
        public bool Equals(CardData other) => CardSuit == other.CardSuit && CardRank == other.CardRank;
        public bool HasSameIdentity(CardData other) => InstanceId != 0 && other.InstanceId != 0
            ? InstanceId == other.InstanceId
            : CardSuit == other.CardSuit && CardRank == other.CardRank;
        public override int GetHashCode() => ((int)CardSuit << 4) | (int)CardRank;
        public override string ToString() => $"{CardRank}{CardSuit}";

        public static bool operator ==(CardData left, CardData right) => left.Equals(right);
        public static bool operator !=(CardData left, CardData right) => !left.Equals(right);
    }
}