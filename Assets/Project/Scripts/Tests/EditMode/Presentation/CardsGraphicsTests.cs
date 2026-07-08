using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using CardFramework.Core.Models;
using CardFramework.Presentation.Views;

namespace CardFramework.Tests.EditMode.Presentation {
    [TestFixture]
    public class CardsGraphicsTests {
        private CardsGraphics _cardsGraphics;

        // Mock Sprites to verify return paths
        private Sprite _mockIcon;
        private Sprite _mockJack;
        private Sprite _mockQueen;
        private Sprite _mockKing;

        [SetUp]
        public void Setup() {
            // 1. Create instance in-memory safely for EditMode
            _cardsGraphics = ScriptableObject.CreateInstance<CardsGraphics>();
            _cardsGraphics.SuitsData = new List<CardsGraphics.CardsGraphicsSuitData>();

            // 2. Generate dummy sprites to validate pointer equity
            _mockIcon = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector2.zero);
            _mockJack = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector2.zero);
            _mockQueen = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector2.zero);
            _mockKing = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector2.zero);

            // 3. Seed test data for a single suit profile (Hearts)
            var heartsData = new CardsGraphics.CardsGraphicsSuitData {
                Suit = CardData.Suit.Hearts,
                Icon = _mockIcon,
                Jack = _mockJack,
                Queen = _mockQueen,
                King = _mockKing
            };

            _cardsGraphics.SuitsData.Add(heartsData);
        }

        [TearDown]
        public void TearDown() {
            // Clean up instances to prevent memory footprint leakage in the editor runtime
            Object.DestroyImmediate(_cardsGraphics);
            Object.DestroyImmediate(_mockIcon);
            Object.DestroyImmediate(_mockJack);
            Object.DestroyImmediate(_mockQueen);
            Object.DestroyImmediate(_mockKing);
        }

        [Test]
        public void GetSuitIcon_ReturnsCorrectSprite_WhenSuitExists() {
            Sprite result = _cardsGraphics.GetSuitIcon(CardData.Suit.Hearts);
            Assert.AreSame(_mockIcon, result, "Should return the exact mock icon reference configured for Hearts.");
        }

        [Test]
        public void GetSuitIcon_ReturnsNull_WhenSuitDoesNotExist() {
            Sprite result = _cardsGraphics.GetSuitIcon(CardData.Suit.Spades);
            Assert.IsNull(result, "Should return null gracefully if the requested suit database configuration is missing.");
        }

        [Test]
        public void GetFaceCardSprite_ReturnsJack_WhenRankIsJack() {
            var card = new CardData(CardData.Suit.Hearts, CardData.Rank.Jack);
            Sprite result = _cardsGraphics.GetFaceCardSprite(card);
            Assert.AreSame(_mockJack, result);
        }

        [Test]
        public void GetFaceCardSprite_ReturnsQueen_WhenRankIsQueen() {
            var card = new CardData(CardData.Suit.Hearts, CardData.Rank.Queen);
            Sprite result = _cardsGraphics.GetFaceCardSprite(card);
            Assert.AreSame(_mockQueen, result);
        }

        [Test]
        public void GetFaceCardSprite_ReturnsKing_WhenRankIsKing() {
            var card = new CardData(CardData.Suit.Hearts, CardData.Rank.King);
            Sprite result = _cardsGraphics.GetFaceCardSprite(card);
            Assert.AreSame(_mockKing, result);
        }

        [Test]
        public void GetFaceCardSprite_ReturnsNull_WhenRankIsNumericOrAce() {
            var card = new CardData(CardData.Suit.Hearts, CardData.Rank.Ace);
            Sprite result = _cardsGraphics.GetFaceCardSprite(card);
            Assert.IsNull(result, "Aces and low numeric cards do not have unique face art textures and should return null.");
        }

        [Test]
        public void GetFaceCardSprite_ReturnsNull_WhenSuitDoesNotExist() {
            // Test the early short-circuit optimization condition (suitData == null)
            var card = new CardData(CardData.Suit.Spades, CardData.Rank.King);
            Sprite result = _cardsGraphics.GetFaceCardSprite(card);
            Assert.IsNull(result, "Should yield null immediately if the card's suit definition cannot be found in the asset structure.");
        }
    }
}