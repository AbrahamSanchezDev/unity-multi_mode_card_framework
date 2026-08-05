using UnityEngine;
using CardFramework.Core.Models;

namespace CardFramework.Presentation.Views {

    [System.Serializable]
    public class CardIdentifier {
        public CardData.Rank Rank;
        public Sprite Icon;
    }

    [System.Serializable]
    public class CardSuitIdentifier {
        public CardData.Suit Suit;

        public Sprite DefaultIcon;

        public CardIdentifier[] Icon = new CardIdentifier[] {
            new CardIdentifier { Rank = CardData.Rank.Ace, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Two, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Three, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Four, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Five, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Six, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Seven, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Eight, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Nine, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Ten, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Jack, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.Queen, Icon = null },
            new CardIdentifier { Rank = CardData.Rank.King, Icon = null },
        };

    }

    [CreateAssetMenu(fileName = "CustomCardImages", menuName = "CardFramework/CustomCardImages", order = 1)]
    public class CustomCardImages : ScriptableObject {
        public CardSuitIdentifier[] Suits = new CardSuitIdentifier[]{
            new CardSuitIdentifier { Suit = CardData.Suit.Clubs },
            new CardSuitIdentifier { Suit = CardData.Suit.Diamonds },
            new CardSuitIdentifier { Suit = CardData.Suit.Hearts },
            new CardSuitIdentifier { Suit = CardData.Suit.Spades }
        };

        public Sprite GetSprite(CardData.Suit suit, CardData.Rank rank) {
            var suitIdentifier = System.Array.Find(Suits, s => s.Suit == suit);
            if (suitIdentifier == null) {
                Debug.LogWarning($"No suit identifier found for {suit}. Using default icon.");
                return null;
            }

            var rankIdentifier = System.Array.Find(suitIdentifier.Icon, r => r.Rank == rank);
            if (rankIdentifier == null || rankIdentifier.Icon == null) {
                Debug.LogWarning($"No icon found for {rank} of {suit}. Using default icon.");
                return suitIdentifier.DefaultIcon;
            }

            return rankIdentifier.Icon;
        }
    }
}
