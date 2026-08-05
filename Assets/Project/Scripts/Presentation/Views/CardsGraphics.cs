using UnityEngine;
using System.Collections.Generic;
using CardFramework.Core.Models;

namespace CardFramework.Presentation.Views {

    [CreateAssetMenu(fileName = "CardsGraphics", menuName = "CardFramework/CardsGraphics", order = 1)]
    public class CardsGraphics : ScriptableObject {
        [System.Serializable]
        public class CardsGraphicsSuitData {
            public CardData.Suit Suit;
            public Sprite Icon;
            public Sprite Jack;
            public Sprite Queen;
            public Sprite King;
        }

        [Header("Card Graphics")]
        public List<CardsGraphicsSuitData> SuitsData;

        [Header("Card Custom Graphics")]
        public CustomCardImages CustomCardImages;

        public CustomCardImages FullBackgroundCardImages;

        public Sprite GetSuitIcon(CardData.Suit suit) {
            var suitData = SuitsData.Find(s => s.Suit == suit);
            return suitData != null ? suitData.Icon : null;
        }

        public Sprite GetFaceCardSprite(CardData card) {
            var suitData = SuitsData.Find(s => s.Suit == card.CardSuit);
            if (suitData == null) return null;

            switch (card.CardRank) {
                case CardData.Rank.Jack:
                    return suitData.Jack;
                case CardData.Rank.Queen:
                    return suitData.Queen;
                case CardData.Rank.King:
                    return suitData.King;
                default:
                    return null;
            }
        }
    }
}