using UnityEngine;

namespace CardFramework.Presentation {
    [CreateAssetMenu(fileName = "CardGamesRoomIntroData", menuName = "CardFramework/Card Games Intro", order = 21)]
    public class CardGamesRoomIntroData : GameRoomIntroData {

        [ContextMenu("Reset to Default Options")]
        public void Reset() {
            roomTitle = "Cards Game Room";
            roomDescription = "Welcome to the cards game room. Pick a table to begin.";
            options = new[] {
                new GameRoomOptionData { optionId = "Blackjack", label = "Blackjack", description = "Play classic 21", accentColor = new Color(0.95f, 0.76f, 0.22f, 1f) },
                new GameRoomOptionData { optionId = "Solitaire", label = "Solitaire", description = "Relax with a solo card challenge", accentColor = new Color(0.21f, 0.56f, 0.86f, 1f) },
                new GameRoomOptionData { optionId = "TexasHoldem", label = "Texas Hold'em", description = "Face off at the poker table", accentColor = new Color(0.77f, 0.24f, 0.26f, 1f) }
            };
        }
    }
}
